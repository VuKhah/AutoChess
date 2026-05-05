import json
import glob
import argparse
from pathlib import Path


def load_cards_from_file(path: Path):
    try:
        with path.open("r", encoding="utf-8") as f:
            data = json.load(f)
    except Exception as e:
        print(f"[WARN] Failed to read {path}: {e}")
        return []

    if isinstance(data, dict) and "cards" in data and isinstance(data["cards"], list):
        return data["cards"]
    if isinstance(data, list):
        return data

    print(f"[WARN] File {path} does not contain a top-level 'cards' list or an array. Skipping.")
    return []


def merge_files(input_files, output_file: Path):
    all_cards = []
    duplicates = []
    seen = set()

    for file in input_files:
        p = Path(file)
        if not p.exists():
            print(f"[WARN] Input file not found: {p}")
            continue
        cards = load_cards_from_file(p)
        for c in cards:
            cid = c.get("cardID")
            if cid is None:
                print(f"[WARN] Card without cardID in {p}: {c}")
                continue
            if cid in seen:
                duplicates.append((cid, p))
                continue
            seen.add(cid)
            all_cards.append(c)

    if duplicates:
        print(f"Found {len(duplicates)} duplicate cardID(s). First occurrences kept, duplicates skipped:")
        for cid, src in duplicates:
            print(f" - {cid} (duplicate in {src})")

    # Ensure output directory exists
    output_file.parent.mkdir(parents=True, exist_ok=True)
    with output_file.open("w", encoding="utf-8") as f:
        json.dump({"cards": all_cards}, f, indent=4, ensure_ascii=False)

    print(f"Merged {len(all_cards)} cards into {output_file}")


def main():
    # merge CardsBabylons.json CardsNiles.json CardsMagic.json -o CardsData.json
    parser = argparse.ArgumentParser(description="Merge JSON card files into a single CardsData.json")
    parser.add_argument("input_files", nargs="+", help="Input JSON files to merge")
    parser.add_argument("-o", "--output", required=True, help="Output file path (e.g., CardsData.json)")
    args = parser.parse_args()
    merge_files(args.input_files, Path(args.output))
    

if __name__ == '__main__':
    main()


#python merge_data.py CardsBabylons.json CardsNiles.json CardsMagic.json -o CardsData.json