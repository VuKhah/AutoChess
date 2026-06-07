import json

file_path = "D:/workspaces/specialized-essays/AutoChess/Assets/Document/02_Data/CardsBabylons.json"
with open(file_path, "r", encoding="utf-8") as f:
    data = json.load(f)

descriptions = {
    "U_01_Babylon": "OnSell (tối đa 3 lần): Tăng vĩnh viễn +0/+0 cho bản thân.",
    "U_02_Babylon": "OnDeploy (mỗi 3 lần): Nhận 1 Coin.",
    "U_03_Babylon": "OnDeploy: Tăng vĩnh viễn +1/+1 cho toàn bộ đồng minh Babylon.",
    "U_04_Babylon": "OnDeploy: Tăng vĩnh viễn +3/+3 cho 1 đồng minh ngẫu nhiên.",
    "U_05_Babylon": "OnSell: Tăng vĩnh viễn +0/+0 cho 2 đồng minh Babylon.",
    "U_06_Babylon": "OnSell: Tăng vĩnh viễn +5/+5 cho toàn bộ đồng minh.",
    "U_07_Babylon": "Taunt. Khi bị đánh: Gây 0 sát thương cho kẻ địch trực tiếp.",
    "U_08_Babylon": "Taunt. OnDeploy: Tăng vĩnh viễn +20/+20 cho bản thân.",
    "U_09_Babylon": "OnDeploy: Triệu hồi 2 đơn vị.",
    "U_10_Babylon": "OnDeploy: Tăng vĩnh viễn +3/+3 cho đồng minh bên phải.",
    "U_11_Babylon": "Aura: Tăng vĩnh viễn +3/+2 cho bản thân.",
    "U_12_Babylon": "Khi tấn công: Nhận 2 Coin.",
    "U_13_Babylon": "OnDeploy: Tăng vĩnh viễn +0/+0 cho 1 đồng minh ngẫu nhiên.",
    "U_14_Babylon": "Taunt. Khi bị đánh: Tăng +1/+2 cho toàn bộ đồng minh Babylon trong trận đấu này.",
    "U_15_Babylon": "Trigger 12: Tăng vĩnh viễn +1/+0 cho toàn bộ đồng minh Babylon.",
    "U_16_Babylon": "OnDeploy: Nhận 2 Coin.",
    "U_17_Babylon": "Taunt. Reborn.",
    "U_18_Babylon": "Trigger 12: Tăng vĩnh viễn +0/+1 cho toàn bộ đồng minh Babylon.",
    "U_19_Babylon": "OnSell: Triệu hồi 1 đơn vị.",
    "U_20_Babylon": "OnSell: Tăng vĩnh viễn +1/+1 cho bản thân.",
    "U_21_Babylon": "OnDeploy: Tăng vĩnh viễn +0/+1 cho bản thân.",
    "U_22_Babylon": "OnDeploy: Nhận 1 Coin."
}

for card in data.get("cards", []):
    card_id = card.get("cardID")
    if card_id in descriptions:
        card["description"] = descriptions[card_id]

with open(file_path, "w", encoding="utf-8") as f:
    json.dump(data, f, indent=4, ensure_ascii=False)
