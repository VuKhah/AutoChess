import os

def build_tree(root_path):
    tree_lines = []

    for root, dirs, files in os.walk(root_path):
        # Tính level (độ sâu)
        level = root.replace(root_path, '').count(os.sep)
        indent = '│   ' * level
        folder_name = os.path.basename(root)

        tree_lines.append(f"{indent}├── {folder_name}/")

        sub_indent = '│   ' * (level + 1)
        for f in files:
            tree_lines.append(f"{sub_indent}├── {f}")

    return tree_lines


def save_tree(root_path, output_file=r"D:\workspaces\specialized-essays\AutoChess\Assets\Resources\Document\04_Tools\assets_tree.txt"):
    tree = build_tree(root_path)
    with open(output_file, "a", encoding="utf-8") as f:
        for line in tree:
            f.write(line + "\n")

    print(f"✅ Saved tree to {output_file}")


if __name__ == "__main__":
    assets_path = r"D:\workspaces\specialized-essays\AutoChess\Assets\Resources\Document"
    save_tree(assets_path)
    assets_path = r"D:\workspaces\specialized-essays\AutoChess\Assets\Scripts"
    save_tree(assets_path)