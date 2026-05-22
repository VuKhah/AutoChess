using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{
    private void SnapshotPreCombatBoard()
    {
        preCombatSnapshot = new (int, CardInstance)[playerSlots.Length];
        for (int i = 0; i < playerSlots.Length; i++)
        {
            CardUI ui = playerSlots[i].GetComponentInChildren<CardUI>();
            preCombatSnapshot[i] = (i, ui?.currentInstance);
        }
    }

    public void EndCombatAndPrepareNextTurn()
    {
        // Xóa summoned units, restore unit gốc từ snapshot
        RestorePreCombatPlayerBoard();
        // Sync playerBoard từ UI vừa restore — loại bỏ dead refs từ trận vừa xong
        SyncBoards();
        ResetAllCardsInSlots(handSlots);
        CleanupEnemySlots();
        // Sync lại sau CleanupEnemySlots để enemyBoard không còn dead units từ trận vừa xong
        SyncBoards();
        ExecuteNextTurn();
    }

    private void RestorePreCombatPlayerBoard()
    {
        foreach (var slot in playerSlots)
            foreach (Transform child in slot) Destroy(child.gameObject);

        if (preCombatSnapshot == null) return;

        foreach (var (slotIdx, unit) in preCombatSnapshot)
        {
            if (unit == null) continue;
            // Reset HP/ATK về full; growthBonus/permanentBonus giữ nguyên → stat không bao giờ giảm
            unit.ResetStats();

            GameObject go = Instantiate(cardPrefab, playerSlots[slotIdx]);
            RectTransform cardRect = go.GetComponent<RectTransform>();
            CardSlotFitter.FitToSlot(cardRect, playerSlots[slotIdx]);
            StartCoroutine(FitCardAfterLayout(cardRect, playerSlots[slotIdx]));
            go.GetComponent<CardUI>().Setup(unit);
            go.GetComponent<CardVisuals>()?.SetBoardPose();
        }
    }

    private void CleanupEnemySlots()
    {
        foreach (var slot in enemySlots)
            foreach (Transform child in slot) Destroy(child.gameObject);
    }

    private void ResetAllCardsInSlots(Transform[] slots)
    {
        foreach (var slot in slots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>(true);
            if (ui != null)
            {
                ui.currentInstance.ResetStats();
                CardVisuals vis = ui.GetComponent<CardVisuals>();
                if (vis != null) vis.ResetVisuals();
                ui.Setup(ui.currentInstance);
            }
        }
    }

    private void SummonEnemyTeam()
    {
        foreach (var slot in enemySlots)
            foreach (Transform child in slot) Destroy(child.gameObject);

        if (enemyBots.Count > 0)
        {
            int shopTier = GetCurrentShopTier();

            // Tất cả đối thủ đều phát triển đội hình mỗi lượt, kể cả khi không đấu
            foreach (var bot in enemyBots)
            {
                bot.EndCombatPhase();
                bot.DecidePrepPhase(CardDatabase.Instance.GetRandomUnitShop(5, shopTier));
            }

            // Round-robin: turn 1→bot0, turn 2→bot1, turn 3→bot2, turn 4→bot0, ...
            currentOpponentIndex = (currentTurn - 1) % enemyBots.Count;
            SpawnBotBoard(CurrentOpponent);
        }
        else
        {
            SpawnRandomEnemyTeam();
        }
    }

    private void SpawnBotBoard(BotAgent bot)
    {
        for (int i = 0; i < bot.board.Count && i < enemySlots.Length; i++)
        {
            CardInstance unit = bot.board[i];
            if (unit == null) continue;
            GameObject cardObj = Instantiate(cardPrefab, enemySlots[i]);
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            CardSlotFitter.FitToSlot(cardRect, enemySlots[i]);
            StartCoroutine(FitCardAfterLayout(cardRect, enemySlots[i]));
            if (cardObj.TryGetComponent<CardDraggable>(out var drg)) drg.enabled = false;
            // Truyền instance thật — sau combat, bot.board[i].IsDead phản ánh đúng kết quả
            // (Fix Bug 2: không tạo CardInstance mới làm mất mergeLevel + growth bonus)
            cardObj.GetComponent<CardUI>().Setup(unit);
            cardObj.GetComponent<CardVisuals>()?.SetBoardPose();
        }
    }

    private void SpawnRandomEnemyTeam()
    {
        List<CardDefinition> allUnits = CardDatabase.Instance.GetAllCards().FindAll(c => c.cardType == CardType.Unit);
        if (allUnits.Count == 0) return;
        for (int i = 0; i < 3 && i < enemySlots.Length; i++)
        {
            CardDefinition data = allUnits[Random.Range(0, allUnits.Count)];
            GameObject cardObj = Instantiate(cardPrefab, enemySlots[i]);
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            CardSlotFitter.FitToSlot(cardRect, enemySlots[i]);
            StartCoroutine(FitCardAfterLayout(cardRect, enemySlots[i]));
            if (cardObj.TryGetComponent<CardDraggable>(out var drg)) drg.enabled = false;
            cardObj.GetComponent<CardUI>().Setup(new CardInstance(data, i));
            cardObj.GetComponent<CardVisuals>()?.SetBoardPose();
        }
    }
}
