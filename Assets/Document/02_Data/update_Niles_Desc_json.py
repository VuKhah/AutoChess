import json

file_path = "D:/workspaces/specialized-essays/AutoChess/Assets/Document/02_Data/CardsNiles.json"
with open(file_path, "r", encoding="utf-8") as f:
    data = json.load(f)

descriptions = {
    "U_01_Niles": "OnAllyDeath: ban Reborn cho LowestHealthAlly (tối đa 2 lần)",
    "U_02_Niles": "OnAllySummon: Consume unit đó (tối đa 2 lần). Slain: triệu hồi lại tất cả unit đã Consume.",
    "U_03_Niles": "OnAllyReborn: Tăng +30/+30 cho đồng minh đó.",
    "U_04_Niles": "OnAllySummon: Tăng vĩnh viễn +1/+1 cho bản thân.",
    "U_05_Niles": "OnAllySummon (mỗi 2 lần): Tăng +1/+1 cho toàn bộ đồng minh Niles trong trận đấu này.",
    "U_06_Niles": "Slain: Tăng vĩnh viễn +2/+0 cho toàn bộ đồng minh Niles.",
    "U_07_Niles": "OnAllyReborn: Tăng vĩnh viễn +3/+3 cho toàn bộ đồng minh.",
    "U_08_Niles": "OnAllySummon: Tăng +4/+4 cho đồng minh đó trong trận đấu này.",
    "U_09_Niles": "Slain: Ban Reborn cho 1 đồng minh ngẫu nhiên.",
    "U_10_Niles": "Slain: Triệu hồi 2 Niles Warrior.",
    "U_11_Niles": "OnDeploy: Consume đồng minh bên trái. Slain: Triệu hồi lại tất cả unit đã bị Consume.",
    "U_12_Niles": "Đầu trận: Kích hoạt kỹ năng của đồng minh bên trái.",
    "U_13_Niles": "OnAllyDeath (mỗi 3 lần): Tăng vĩnh viễn +1/+0 cho toàn bộ đồng minh Niles.",
    "U_14_Niles": "Khi tấn công: Gây 5 sát thương cho kẻ địch trực tiếp.",
    "U_15_Niles": "OnAllyDeath: Tăng vĩnh viễn +1/+1 cho bản thân.",
    "U_16_Niles": "OnDeploy: Nhận 1 Coin.",
    "U_17_Niles": "Aura: Tăng vĩnh viễn +2/+0 cho đồng minh ngẫu nhiên.",
    "U_18_Niles": "OnAllySummon: Tăng +1/+1 cho bản thân trong trận đấu này.",
    "U_19_Niles": "Slain: Triệu hồi 2 Medjed.",
    "U_20_Niles": "Slain: Triệu hồi 1 Mummy.",
    "U_21_Niles": "Taunt. Reborn."
}

for card in data.get("cards", []):
    card_id = card.get("cardID")
    if card_id in descriptions:
        card["description"] = descriptions[card_id]

with open(file_path, "w", encoding="utf-8") as f:
    json.dump(data, f, indent=4, ensure_ascii=False)
