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

        if (enemyBot != null)
        {
            for (int i = 0; i < enemyBot.board.Count; i++) enemyBot.board[i] = null;
            enemyBot.DecidePrepPhase(CardDatabase.Instance.GetRandomUnitShop(5, GetCurrentShopTier()));
            SpawnBotBoard();
        }
        else
        {
            SpawnRandomEnemyTeam();
        }
    }

    private void SpawnBotBoard()
    {
        for (int i = 0; i < enemyBot.board.Count && i < enemySlots.Length; i++)
        {
            CardInstance unit = enemyBot.board[i];
            if (unit == null) continue;
            GameObject cardObj = Instantiate(cardPrefab, enemySlots[i]);
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            CardSlotFitter.FitToSlot(cardRect, enemySlots[i]);
            StartCoroutine(FitCardAfterLayout(cardRect, enemySlots[i]));
            if (cardObj.TryGetComponent<CardDraggable>(out var drg)) drg.enabled = false;
            cardObj.GetComponent<CardUI>().Setup(new CardInstance(unit.Data, i));
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
