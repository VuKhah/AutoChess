using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class GameManager
{
    public void SyncBoards()
    {
        if (playerSlots != null)
        {
            int pLen = Mathf.Min(playerSlots.Length, playerBoard.Count);
            for (int i = 0; i < pLen; i++)
            {
                if (playerSlots[i] == null) continue;
                CardUI ui = playerSlots[i].GetComponentInChildren<CardUI>();
                playerBoard[i] = (ui != null) ? ui.currentInstance : null;
                if (playerBoard[i] != null) playerBoard[i].slotIndex = i;
            }
        }
        if (enemySlots != null)
        {
            int eLen = Mathf.Min(enemySlots.Length, enemyBoard.Count);
            for (int i = 0; i < eLen; i++)
            {
                if (enemySlots[i] == null) continue;
                CardUI ui = enemySlots[i].GetComponentInChildren<CardUI>();
                enemyBoard[i] = (ui != null) ? ui.currentInstance : null;
                if (enemyBoard[i] != null) enemyBoard[i].slotIndex = i;
            }
        }
        // Cập nhật nhấp nháy shop theo trạng thái board+hand vừa sync.
        UpdateShopMergeHints();
    }

    // Xóa UI của các unit đã chết trên sân player (dùng sau OnDeploy Consume trong shop phase)
    public void CleanupDeadBoardUnitsUI()
    {
        foreach (var slot in playerSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui?.currentInstance != null && ui.currentInstance.IsDead)
                Destroy(ui.gameObject);
        }
        SyncBoards();
    }

    public void StartCombatPhase()
    {
        if (isCombatActive || isGameEnded) return;

        // Chụp snapshot sân player TRƯỚC MỌI THAY ĐỔI COMBAT
        SnapshotPreCombatBoard();
        SummonEnemyTeam();
        SyncBoards();

        isCombatActive = true;
        UIManager.Instance.UpdateUIState(true);
        AudioManager.Instance?.BattleStart();
        StartCoroutine(CombatSequence());
    }

    // --- ĐẠO DIỄN TRẬN ĐẤU ---
    private IEnumerator CombatSequence()
    {
        // Giai đoạn A: Đảm bảo tọa độ UI chính xác
        // Dùng GetComponentInParent để tránh NullRef khi parent là null (root transform)
        RectTransform playerPanel = playerSlots != null && playerSlots.Length > 0
            ? playerSlots[0].GetComponentInParent<RectTransform>()
            : null;
        RectTransform enemyPanel = enemySlots != null && enemySlots.Length > 0
            ? enemySlots[0].GetComponentInParent<RectTransform>()
            : null;
        if (playerPanel != null) LayoutRebuilder.ForceRebuildLayoutImmediate(playerPanel);
        if (enemyPanel  != null) LayoutRebuilder.ForceRebuildLayoutImmediate(enemyPanel);
        yield return new WaitForEndOfFrame();

        // Giai đoạn B: Tính toán toàn bộ trận đấu trong 1 tích tắc
        TurnRecord combatLog = new TurnRecord();
        resolver.ResolveTurn(playerBoard, enemyBoard, combatLog);

        // Giai đoạn C: Trình diễn từng action
        foreach (var action in combatLog.actions)
        {
            yield return StartCoroutine(VisualizeAction(action));
            // StatChange không cần delay — cập nhật chỉ số tức thì không làm gián đoạn battle
            if (action.actionType != CombatActionType.StatChange)
                yield return new WaitForSeconds(0.1f);
        }

        // Giai đoạn D: Chờ người chơi nhìn kết quả
        yield return new WaitForSeconds(1.0f);

        // Giai đoạn E: Tính kết quả + dọn dẹp
        CheckVictoryConditions();
        EndCombatAndPrepareNextTurn();

        Debug.Log("<color=yellow>--- TRẬN ĐẤU KẾT THÚC, QUAY LẠI SHOP ---</color>");
    }

    private IEnumerator VisualizeAction(CombatAction action)
    {
        if (action.actionType == CombatActionType.StatChange)
        {
            VisualizeStatChange(action);
            yield break;
        }

        if (action.actionType == CombatActionType.Summon)
        {
            yield return StartCoroutine(VisualizeSummon(action));
            yield break;
        }

        CardVisuals attackerVis = (action.isPlayerAttacking ? playerSlots : enemySlots)[action.attackerIdx].GetComponentInChildren<CardVisuals>(true);
        CardVisuals targetVis   = (action.isPlayerAttacking ? enemySlots  : playerSlots)[action.targetIdx].GetComponentInChildren<CardVisuals>(true);

        if (attackerVis == null || targetVis == null) yield break;

        Vector3 impactPos = (attackerVis.transform.position + targetVis.transform.position) / 2;
        yield return StartCoroutine(attackerVis.AttackAnimation(impactPos, 0.12f));

        CardUI atkUI = attackerVis.GetComponent<CardUI>();
        CardUI tarUI = targetVis.GetComponent<CardUI>();

        atkUI.currentInstance.currentHP = action.atkHPAfter;
        tarUI.currentInstance.currentHP = action.defHPAfter;

        atkUI.Setup(atkUI.currentInstance);
        tarUI.Setup(tarUI.currentInstance);

        // Death + Reborn: chạy song song attacker/defender nhưng CHẶN combat đến khi xong
        // để các card khác không tấn công trước khi unit hồi sinh.
        Coroutine atkDeathOrReborn = null;
        Coroutine tarDeathOrReborn = null;

        if (atkUI.currentInstance.IsDead)
        {
            if (action.attackerReborn) atkDeathOrReborn = StartCoroutine(PlayRebornVisual(attackerVis, atkUI, action.attackerRevivedHP));
            else                       atkDeathOrReborn = StartCoroutine(attackerVis.DieAnimation());
        }
        if (tarUI.currentInstance.IsDead)
        {
            if (action.defenderReborn) tarDeathOrReborn = StartCoroutine(PlayRebornVisual(targetVis, tarUI, action.defenderRevivedHP));
            else                       tarDeathOrReborn = StartCoroutine(targetVis.DieAnimation());
        }

        if (atkDeathOrReborn != null) yield return atkDeathOrReborn;
        if (tarDeathOrReborn != null) yield return tarDeathOrReborn;

        yield return new WaitForSeconds(0.2f);
    }

    private void VisualizeStatChange(CombatAction action)
    {
        Transform[] slots = action.statIsPlayerSide ? playerSlots : enemySlots;
        if (action.statSlotIdx < 0 || action.statSlotIdx >= slots.Length) return;
        CardUI ui = slots[action.statSlotIdx].GetComponentInChildren<CardUI>(true);
        if (ui?.currentInstance == null) return;
        ui.currentInstance.currentATK = action.statNewATK;
        ui.currentInstance.currentHP  = action.statNewHP;
        ui.Setup(ui.currentInstance);

        if (action.flashType != FlashType.None)
        {
            CardVisuals vis = ui.GetComponent<CardVisuals>();
            if (vis != null)
                StartCoroutine(vis.FlashEffect(GetFlashColor(action.flashType)));
        }
    }

    private static Color GetFlashColor(FlashType type)
    {
        switch (type)
        {
            case FlashType.Buff:            return new Color(0.2f, 1f,    0.2f, 0.85f); // xanh lá
            case FlashType.Debuff:          return new Color(1f,   0.15f, 0.15f, 0.85f); // đỏ
            case FlashType.Status:          return new Color(0.3f, 0.6f,  1f,   0.85f); // xanh dương
            case FlashType.SynergyBabylon:  return new Color(1f,   0.85f, 0.05f, 0.9f); // vàng
            case FlashType.SynergyOlympus:  return new Color(0.1f, 0.85f, 1f,   0.9f); // cyan
            case FlashType.SynergyNiles:    return new Color(0.1f, 1f,    0.45f, 0.9f); // lục
            default:                        return Color.white;
        }
    }

    private IEnumerator VisualizeSummon(CombatAction action)
    {
        Transform[] slots = action.isPlayerSummon ? playerSlots : enemySlots;
        if (action.summonSlotIdx < 0 || action.summonSlotIdx >= slots.Length) yield break;

        CardDefinition data = CardDatabase.Instance.GetCard(action.summonCardID);
        if (data == null) yield break;

        Transform slot = slots[action.summonSlotIdx];
        foreach (Transform child in slot)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        GameObject go = Instantiate(cardPrefab, slot);
        RectTransform cardRect = go.GetComponent<RectTransform>();
        CardSlotFitter.FitToSlot(cardRect, slot);
        StartCoroutine(FitCardAfterLayout(cardRect, slot));

        CardInstance visualInstance = new CardInstance(data, action.summonSlotIdx)
        {
            isBattleSpawned = true,
            currentHP = action.summonHP,
            currentATK = action.summonATK
        };

        CardUI ui = go.GetComponent<CardUI>();
        ui.Setup(visualInstance);

        CardVisuals vis = go.GetComponent<CardVisuals>();
        if (vis != null)
        {
            vis.SetBoardPose();
            yield return StartCoroutine(vis.RebornAnimation());
        }

        Debug.Log($"<color=green>[UI]</color> Visual summon: {data.cardName} tại slot {action.summonSlotIdx}");
    }

    private IEnumerator PlayRebornVisual(CardVisuals vis, CardUI ui, int revivedHP)
    {
        // 1. Diễn hiệu ứng chết để người chơi thấy bị hạ gục
        yield return StartCoroutine(vis.DieAnimation());

        // 2. Tạm dừng nhịp ngắn — khoảng lặng trước khi hồi sinh
        yield return new WaitForSeconds(0.25f);

        // 3. Khôi phục HP + dữ liệu hiển thị, vẫn giữ card ẩn để RebornAnimation tự fade-in
        ui.currentInstance.currentHP = revivedHP;
        ui.Setup(ui.currentInstance);

        // 4. Diễn hiệu ứng hồi sinh tại đúng slot cũ (card vẫn nằm trong slot parent)
        yield return StartCoroutine(vis.RebornAnimation());
    }

    // Tạo CardUI cho unit được triệu hồi trong battle mà chưa có GameObject
    private void SpawnMissingBoardUI()
    {
        SpawnMissingOnSide(playerBoard, playerSlots);
        SpawnMissingOnSide(enemyBoard,  enemySlots);
    }

    private void SpawnMissingOnSide(List<CardInstance> board, Transform[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            CardInstance unit = (i < board.Count) ? board[i] : null;
            if (unit == null) continue;

            CardUI existing = slots[i].GetComponentInChildren<CardUI>();
            if (existing != null)
            {
                existing.Setup(unit);
            }
            else
            {
                GameObject go = Instantiate(cardPrefab, slots[i]);
                RectTransform cardRect = go.GetComponent<RectTransform>();
                CardSlotFitter.FitToSlot(cardRect, slots[i]);
                StartCoroutine(FitCardAfterLayout(cardRect, slots[i]));
                go.GetComponent<CardUI>().Setup(unit);
                go.GetComponent<CardVisuals>()?.SetBoardPose();
                Debug.Log($"<color=green>[UI]</color> Spawn UI cho unit triệu hồi: {unit.Data.cardName} tại slot {i}");
            }
        }
    }

    private void CheckVictoryConditions()
    {
        bool pAlive = playerBoard.Any(u => u != null && !u.IsDead);
        bool eAlive = enemyBoard.Any(u => u != null && !u.IsDead);

        if (pAlive && !eAlive)
        {
            playerCups++;
            AudioManager.Instance?.Win();
            if (pendingWagerCoins > 0)
            {
                economy.Earn(pendingWagerCoins);
                Debug.Log($"<color=yellow>[WAGER]</color> Thắng! Nhận {pendingWagerCoins} đồng.");
                pendingWagerCoins = 0;
            }
        }
        else if (!pAlive && eAlive)
        {
            playerHP--;
            AudioManager.Instance?.Lose();
            pendingWagerCoins = 0; // thua → hủy wager
        }

        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);

        if (playerHP <= 0) GameOver();
        else if (playerCups >= winConditionCups) WinGame();
    }
}
