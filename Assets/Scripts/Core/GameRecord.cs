using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class GameRecord
{
    public string matchID;
    public List<TurnRecord> turns = new List<TurnRecord>();
}

[System.Serializable]
public class TurnRecord
{
    public int turnIndex;
    public List<CardSnapshot> playerBoard; // Trạng thái board người chơi
    public List<CardSnapshot> enemyBoard;  // Trạng thái board bot
    public List<string> combatLog;         // Ghi lại chuỗi hiệu ứng kích hoạt
}

[System.Serializable]
public class CardSnapshot
{
    public string cardID;
    public int hp;
    public int atk;
}