#!/usr/bin/env python3
"""Extract probably unused Firewatch voice recordings to OGG files."""

import argparse
import csv
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

try:
    from wem2ogg import wem_to_ogg
except ImportError:
    print("Missing dependency. Install it with: python -m pip install wem2ogg", file=sys.stderr)
    raise SystemExit(1)


SPEECH_ID = re.compile(rb'"speechID"\s*:\s*(\d+)')
PLAY_EVENT = re.compile(rb'Play_(\d{5})(?![A-Za-z0-9_])')
MANIFEST_EVENT = re.compile(
    r"^\s*\d+\s+Play_(?P<id>\d{5})\s+.*?\\Voice\\Voice\\(?P<speaker>[^\\]+)\\"
)


def arguments():
    parser = argparse.ArgumentParser(
        description=(
            "Extract English voice recordings which have no speech definition "
            "or direct reference in the shipped game data."
        )
    )
    parser.add_argument(
        "--game-path",
        type=Path,
        default=Path(r"C:\Program Files\GOG Galaxy\Games\Firewatch"),
        help="Firewatch installation directory",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("unused-voice"),
        help="Output directory (default: ./unused-voice)",
    )
    return parser.parse_args()


def scan_game_data(data_path):
    defined_ids = set()
    directly_referenced_ids = set()

    candidates = [
        path
        for path in data_path.iterdir()
        if path.is_file()
        and (
            re.fullmatch(r"sharedassets\d+\.assets", path.name)
            or re.fullmatch(r"level\d+", path.name)
            or path.name in {"resources.assets", "globalgamemanagers", "globalgamemanagers.assets"}
        )
    ]
    candidates.extend((data_path / "Managed").glob("*.dll"))

    for path in candidates:
        data = path.read_bytes()
        defined_ids.update(int(match.group(1)) for match in SPEECH_ID.finditer(data))
        directly_referenced_ids.update(int(match.group(1)) for match in PLAY_EVENT.finditer(data))

    return defined_ids, directly_referenced_ids


def load_media_map(soundbanks_info):
    media = {}
    root = ET.parse(soundbanks_info).getroot()
    streamed_files = root.find("StreamedFiles")
    if streamed_files is None:
        return media

    for item in streamed_files.findall("File"):
        if item.get("Language") != "English(US)":
            continue
        short_name = item.findtext("ShortName", "")
        if not re.fullmatch(r"\d{5}\.wav", short_name):
            continue
        media[int(short_name[:5])] = int(item.get("Id"))
    return media


def load_voice_events(manifest):
    events = {}
    for line in manifest.read_text(encoding="utf-8", errors="replace").splitlines():
        match = MANIFEST_EVENT.match(line)
        if match:
            events[int(match.group("id"))] = match.group("speaker")
    return events


def safe_name(value):
    return re.sub(r'[^A-Za-z0-9_. -]+', "_", value).strip()


def main():
    args = arguments()
    data_path = args.game_path / "Firewatch_Data"
    audio_path = data_path / "StreamingAssets" / "Audio" / "GeneratedSoundBanks" / "Windows"
    info_path = audio_path / "SoundbanksInfo.xml"
    manifest_path = audio_path / "English(US)" / "global_voice.txt"
    wem_path = audio_path / "English(US)"

    required = (data_path, info_path, manifest_path)
    missing = [str(path) for path in required if not path.exists()]
    if missing:
        raise SystemExit("Required Firewatch files were not found:\n" + "\n".join(missing))

    print("Scanning game data...")
    defined_ids, directly_referenced_ids = scan_game_data(data_path)
    media = load_media_map(info_path)
    events = load_voice_events(manifest_path)

    unused_ids = sorted(set(events) - defined_ids - directly_referenced_ids)
    args.output.mkdir(parents=True, exist_ok=True)
    index_rows = []

    print(f"Extracting {len(unused_ids)} probable unused recordings...")
    for number, speech_id in enumerate(unused_ids, 1):
        media_id = media.get(speech_id)
        if media_id is None:
            continue
        source = wem_path / f"{media_id}.wem"
        if not source.exists():
            continue

        speaker = events[speech_id]
        filename = f"{speech_id:05d} - {safe_name(speaker)}.ogg"
        destination = args.output / filename
        destination.write_bytes(wem_to_ogg(source.read_bytes()))
        index_rows.append(
            {
                "SpeechId": speech_id,
                "Speaker": speaker,
                "AudioEvent": f"Play_{speech_id:05d}",
                "WwiseMediaId": media_id,
                "File": filename,
            }
        )
        if number % 100 == 0:
            print(f"  {number}/{len(unused_ids)}")

    with (args.output / "index.csv").open("w", newline="", encoding="utf-8-sig") as stream:
        writer = csv.DictWriter(stream, fieldnames=index_rows[0].keys() if index_rows else [
            "SpeechId", "Speaker", "AudioEvent", "WwiseMediaId", "File"
        ])
        writer.writeheader()
        writer.writerows(index_rows)

    print(f"Done. Extracted {len(index_rows)} recordings to: {args.output.resolve()}")
    print("These are probable unused files, not definitive proof that no runtime path can play them.")


if __name__ == "__main__":
    main()
