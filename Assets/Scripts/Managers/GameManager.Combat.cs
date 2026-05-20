using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class GameManager
{
    public void SyncBoards()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            CardUI ui = playerSlots[i].GetComponentInChildren<CardUI>();
            playerBoard[i] = (ui != null) ? ui.currentInstance : null;
        }
        for (int i = 0; i < enemySlots.Length; i++)
        {
            CardUI ui = enemySlots[i].GetComponentInChildren<CardUI>();
            enemyBoard[i] = (ui != null) ? ui.currentInstance : null;
        }
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
        StartCoroutine(CombatSequence());
    }

    // --- ĐẠO DIỄN TRẬN ĐẤU ---
    private IEnumerator CombatSequence()
    {
        // Giai đoạn A: Đảm bảo tọa độ UI chính xác
        LayoutRebuilder.ForceRebuildLayoutImmediate(playerSlots[0].parent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemySlots[0].parent.GetComponent<RectTransform>());
        yield return new WaitForEndOfFrame();

        // Giai đoạn B: Tính toán toàn bộ trận đấu trong 1 tích tắc
        TurnRecord combatLog = new TurnRecord();
        resolver.ResolveTurn(playerBoard, enemyBoard, combatLog);

        // Giai đoạn B.5: Spawn UI cho unit được triệu hồi trong trận
        SpawnMissingBoardUI();
        yield return new WaitForEndOfFrame();

        // Giai đoạn C: Trình diễn từng action
        foreach (var action in combatLog.actions)
        {
            yield return StartCoroutine(VisualizeAction(action));
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
        CardVisuals attackerVis = (action.isPlayerAttacking ? playerSlots : enemySlots)[action.attackerIdx].GetComponentInChildren<CardVisuals>(true);
        CardVisuals targetVis = (action.isPlayerAttacking ? enemySlots : playerSlots)[action.targetIdx].GetComponentInChildren<CardVisuals>(true);

        if (attackerVis == null || targetVis == null) yield break;

        CardUI atkUI = attackerVis.GetComponent<CardUI>();
        CardUI tarUI = targetVis.GetComponent<CardUI>();

        // Kiểm tra unit ở slot có đúng là unit trong action không.
        // Nếu unit triệu hồi đã thế chỗ unit gốc (SpawnMissingBoardUI gán trước visualization),
        // bỏ qua HP update + DieAnimation cho phía đó để tránh ẩn card unit triệu hồi.
        bool atkMatch = atkUI.currentInstance?.Data?.cardName == action.attackerName;
        bool defMatch = tarUI.currentInstance?.Data?.cardName == action.targetName;

        if (atkMatch)
        {
            Vector3 impactPos = (attackerVis.transform.position + targetVis.transform.position) / 2;
            yield return StartCoroutine(attackerVis.AttackAnimation(impactPos, 0.12f));
        }

        if (atkMatch)
        {
            atkUI.currentInstance.currentHP = action.atkHPAfter;
            atkUI.Setup(atkUI.currentInstance);
            if (atkUI.currentInstance.IsDead) StartCoroutine(attackerVis.DieAnimation());
        }
        if (defMatch)
        {
            tarUI.currentInstance.currentHP = action.defHPAfter;
            tarUI.Setup(tarUI.currentInstance);
            if (tarUI.currentInstance.IsDead) StartCoroutine(targetVis.DieAnimation());
        }

        yield return new WaitForSeconds(0.2f);
    }

    // Tạo CardUI cho unit được triệu hồi trong battle mà chưa có GameObject
    private void SpawnMissingBoardUI()
    {
        SpawnMissingOnSide(playerBoard, playerSlots);
        SpawnMissingOnSide(enemyBoard, enemySlots);
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

        if (pAlive && !eAlive) playerCups++;
        else if (!pAlive && eAlive) playerHP--;

        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);

        if (playerHP <= 0) GameOver();
        else if (playerCups >= winConditionCups) WinGame();
    }
}
