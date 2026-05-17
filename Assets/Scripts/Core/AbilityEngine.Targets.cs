using System.Collections.Generic;
using UnityEngine;

public partial class AbilityEngine
{
    private List<CardInstance> FindTargets(AbilityData ability, CardInstance source, CardInstance directEnemy, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        List<CardInstance> validTargets = new List<CardInstance>();

        switch (ability.target)
        {
            case TargetType.Self:
                validTargets.Add(source);
                break;

            case TargetType.DirectEnemy:
                if (directEnemy != null && !directEnemy.IsDead)
                    validTargets.Add(directEnemy);
                break;

            case TargetType.AllAllies:
            {
                var pool = allyBoard.FindAll(u => u != null && !u.IsDead);
                if (ability.targetTribe != 0) pool = pool.FindAll(u => u.Data.tribe == (Tribe)ability.targetTribe);
                validTargets.AddRange(pool);
                break;
            }

            case TargetType.RandomAlly:
            {
                var pool = allyBoard.FindAll(u => u != null && !u.IsDead && u != source);
                if (ability.targetTribe != 0) pool = pool.FindAll(u => u.Data.tribe == (Tribe)ability.targetTribe);
                validTargets.AddRange(GetRandomFromPool(pool, ability.targetCount > 0 ? ability.targetCount : 1, null));
                break;
            }

            case TargetType.RandomEnemy:
            {
                var pool = enemyBoard.FindAll(u => u != null && !u.IsDead);
                if (ability.targetTribe != 0) pool = pool.FindAll(u => u.Data.tribe == (Tribe)ability.targetTribe);
                validTargets.AddRange(GetRandomFromPool(pool, ability.targetCount > 0 ? ability.targetCount : 1, null));
                break;
            }

            case TargetType.LowestHealthAlly:
                CardInstance lowestHPUnit = null;
                int minHP = int.MaxValue;
                foreach (var unit in allyBoard)
                {
                    if (unit != null && !unit.IsDead && unit.currentHP < minHP)
                    {
                        minHP = unit.currentHP;
                        lowestHPUnit = unit;
                    }
                }
                if (lowestHPUnit != null) validTargets.Add(lowestHPUnit);
                break;

            case TargetType.LeftAlly:
                int leftIndex = source.slotIndex - 1;
                if (leftIndex >= 0)
                {
                    CardInstance leftUnit = allyBoard[leftIndex];
                    if (leftUnit != null && !leftUnit.IsDead) validTargets.Add(leftUnit);
                }
                break;

            case TargetType.RightAlly:
                int rightIndex = source.slotIndex + 1;
                if (rightIndex < allyBoard.Count)
                {
                    CardInstance rightUnit = allyBoard[rightIndex];
                    if (rightUnit != null && !rightUnit.IsDead) validTargets.Add(rightUnit);
                }
                break;

            case TargetType.AllNilesAllies:
                validTargets.AddRange(allyBoard.FindAll(u => u != null && !u.IsDead && u.Data.tribe == Tribe.Niles));
                break;

            case TargetType.AllBabylonAllies:
                validTargets.AddRange(allyBoard.FindAll(u => u != null && !u.IsDead && u.Data.tribe == Tribe.Babylon));
                break;

            case TargetType.TriggerSubject:
                // Unit đã gây ra sự kiện — được pass qua directEnemy trong OnAllySummon/OnAllyDeath/OnAllyReborn
                // BUG FIX: Kiểm tra !IsDead — nếu subject đã bị consume/destroy trước khi unit này react
                // (ví dụ: hai Sekhmet cùng lắng nghe OnAllySummon, cái thứ nhì thấy dead target),
                // trả về rỗng để TriggerAbility không tốn triggerLimit count vô ích.
                if (directEnemy != null && !directEnemy.IsDead) validTargets.Add(directEnemy);
                break;

            case TargetType.AllAlliesExceptSelf:
            {
                var pool = allyBoard.FindAll(u => u != null && !u.IsDead && u != source);
                if (ability.targetTribe != 0) pool = pool.FindAll(u => u.Data.tribe == (Tribe)ability.targetTribe);
                validTargets.AddRange(pool);
                break;
            }
        }

        return validTargets;
    }

    private List<CardInstance> GetRandomFromPool(List<CardInstance> pool, int count, CardInstance exclude)
    {
        List<CardInstance> results = new List<CardInstance>();
        List<CardInstance> temp = new List<CardInstance>(pool.FindAll(u => u != null && !u.IsDead && u != exclude));
        for (int i = 0; i < count && temp.Count > 0; i++)
        {
            int r = Random.Range(0, temp.Count);
            results.Add(temp[r]);
            temp.RemoveAt(r); // Chống trùng lặp mục tiêu
        }
        return results;
    }
}
