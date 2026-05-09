using System.Collections.Generic;
using UnityEngine;

// TTE Engine: Trigger → Target → Effect
public class AbilityEngine
{
    public void TriggerAbility(TriggerType triggerContext, CardInstance source, CardInstance directEnemy, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        if (source == null || source.Data.abilities == null || source.Data.abilities.Count == 0) return;

        int scaleFactor = source.mergeLevel + 1;

        for (int i = 0; i < source.Data.abilities.Count; i++)
        {
            AbilityData ability = source.Data.abilities[i];
            if (ability == null || ability.trigger != triggerContext) continue;

            if (ability.triggerLimit > 0 && source.abilityTriggerCounts[i] >= ability.triggerLimit) continue;

            // StartOfBattle + AddStats = Growth (tăng trưởng vĩnh viễn)
            bool isGrowth = triggerContext == TriggerType.StartOfBattle && ability.effect == EffectType.AddStats;

            List<CardInstance> targets = FindTargets(ability, source, directEnemy, allyBoard, enemyBoard);
            foreach (var target in targets)
            {
                if (isGrowth) ApplyGrowth(ability, target, scaleFactor);
                else ExecuteEffect(ability, target, source, allyBoard, enemyBoard, scaleFactor);
            }

            if (ability.triggerLimit > 0) source.abilityTriggerCounts[i]++;
        }
    }

    private void ApplyGrowth(AbilityData ability, CardInstance target, int scaleFactor)
    {
        if (target == null || target.IsDead) return;
        int atkGain = ability.effectValue1 * scaleFactor;
        int hpGain  = ability.effectValue2 * scaleFactor;
        target.growthATKBonus += atkGain;
        target.growthHPBonus  += hpGain;
        target.currentATK = target.Data.baseATK + target.permanentATKBonus + target.growthATKBonus;
        target.currentHP  = target.Data.baseHP  + target.permanentHPBonus  + target.growthHPBonus;
        Debug.Log($"<color=lime>[GROWTH]</color> {target.Data.cardName} tăng trưởng +{atkGain}ATK/+{hpGain}HP (tổng tăng: +{target.growthATKBonus}/+{target.growthHPBonus})");
    }

    private void ExecuteEffect(AbilityData ability, CardInstance target, CardInstance source, List<CardInstance> allyBoard, List<CardInstance> enemyBoard, int scaleFactor = 1)
    {
        if (target == null) return;

        // Reborn, Summon, SummonConsumed được phép thực thi dù target đã chết (OnDeath effects)
        bool isDeathSafeEffect = ability.effect == EffectType.Reborn
                              || ability.effect == EffectType.Summon
                              || ability.effect == EffectType.SummonConsumed;
        if (target.IsDead && !isDeathSafeEffect) return;

        switch (ability.effect)
        {
            case EffectType.AddStats:
            {
                int atk = ability.effectValue1 * scaleFactor;
                int hp  = ability.effectValue2 * scaleFactor;
                target.currentATK += atk;
                target.currentHP  += hp;
                if (ability.isPermanent)
                {
                    target.permanentATKBonus += atk;
                    target.permanentHPBonus  += hp;
                }
                Debug.Log($"<color=cyan>[ABILITY]</color> {target.Data.cardName} được buff +{atk}ATK / +{hp}HP");
                break;
            }

            case EffectType.GiveBuff:
            {
                if (ability.isTaunt)      { target.isTaunt = true;         Debug.Log($"<color=cyan>[BUFF]</color> {target.Data.cardName} nhận Taunt!"); }
                if (ability.isReborn)     { target.isReborn = true;        Debug.Log($"<color=cyan>[BUFF]</color> {target.Data.cardName} nhận Reborn!"); }
                if (ability.isSafeguard)  { target.safeguardActive = true; Debug.Log($"<color=cyan>[BUFF]</color> {target.Data.cardName} nhận Safeguard!"); }
                break;
            }

            case EffectType.DealDamage:
            {
                int dmg = ability.effectValue1 * scaleFactor;
                target.currentHP -= dmg;
                Debug.Log($"<color=orange>[ABILITY]</color> {target.Data.cardName} chịu {dmg} sát thương kỹ năng");
                break;
            }

            case EffectType.Destroy:
                target.currentHP = 0;
                if (ability.isConsume)
                    source.consumedCardIDs.Add(target.Data.cardID);
                Debug.Log($"<color=red>[ABILITY]</color> {source.Data.cardName} tiêu diệt {target.Data.cardName}{(ability.isConsume ? " (Consumed!)" : "")}");
                break;

            case EffectType.Reborn:
                if (!target.hasRebornUsed)
                {
                    int reviveHP = ability.effectValue1 * scaleFactor;
                    target.Revive(reviveHP);
                    Debug.Log($"<color=magenta>[ABILITY]</color> {target.Data.cardName} HỒI SINH với {reviveHP} HP!");
                    BroadcastAllyEvent(TriggerType.OnAllyReborn, target, allyBoard, enemyBoard);
                }
                break;

            case EffectType.Summon:
                CardInstance summoned = SummonUnit(ability.summonCardID, allyBoard);
                if (summoned != null)
                {
                    Debug.Log($"<color=green>[SUMMON]</color> {source.Data.cardName} triệu hồi {summoned.Data.cardName}!");
                    TriggerAbility(TriggerType.OnDeploy, summoned, null, allyBoard, enemyBoard);
                    BroadcastAllyEvent(TriggerType.OnAllySummon, summoned, allyBoard, enemyBoard);
                }
                break;

            case EffectType.GainCoin:
            {
                int coins = ability.effectValue1 * scaleFactor;
                GameManager.Instance.AddCoin(coins);
                Debug.Log($"<color=yellow>[ABILITY]</color> {source.Data.cardName} cộng {coins} Coin! (Deploy)");
                break;
            }

            case EffectType.TriggerAbility:
                // Guard chống chain vô hạn: không cho phép TriggerAbility kích TriggerAbility
                if (target.Data.abilities != null)
                {
                    foreach (var ab in target.Data.abilities)
                    {
                        if (ab != null && ab.effect != EffectType.TriggerAbility)
                            TriggerAbility(ab.trigger, target, null, allyBoard, enemyBoard);
                    }
                }
                break;

            case EffectType.SummonConsumed:
                if (source.consumedCardIDs != null && source.consumedCardIDs.Count > 0)
                {
                    Debug.Log($"<color=magenta>[CONSUME]</color> {source.Data.cardName} giải phóng {source.consumedCardIDs.Count} unit đã tiêu thụ!");
                    foreach (var cardID in source.consumedCardIDs)
                    {
                        CardInstance released = SummonUnit(cardID, allyBoard);
                        if (released != null)
                        {
                            TriggerAbility(TriggerType.OnDeploy, released, null, allyBoard, enemyBoard);
                            BroadcastAllyEvent(TriggerType.OnAllySummon, released, allyBoard, enemyBoard);
                        }
                    }
                    source.consumedCardIDs.Clear();
                }
                break;
        }
    }

    public void BroadcastAllyEvent(TriggerType context, CardInstance subject, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        foreach (var unit in allyBoard)
        {
            if (unit != null && !unit.IsDead && unit != subject)
                TriggerAbility(context, unit, subject, allyBoard, enemyBoard);
        }
    }

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
                if (rightIndex < 6)
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
                if (directEnemy != null) validTargets.Add(directEnemy);
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

    private CardInstance SummonUnit(string cardID, List<CardInstance> board)
    {
        for (int i = 0; i < 6; i++)
        {
            if (board[i] == null || board[i].IsDead)
            {
                CardDefinition data = CardDatabase.Instance.GetCard(cardID);
                if (data == null) return null;
                board[i] = new CardInstance(data, i);
                Debug.Log($"<color=green>[SUMMON]</color> Đã triệu hồi {data.cardName} vào slot {i}");
                return board[i];
            }
        }
        return null;
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

    public void ApplyMagicToUnit(CardInstance magic, CardInstance unit)
    {
        if (magic == null || unit == null) return;

        switch (magic.Data.magicGroup)
        {
            case "StatBoost":
                unit.permanentATKBonus += magic.Data.statBonusATK;
                unit.permanentHPBonus += magic.Data.statBonusHP;
                break;

            case "AddAbility":
                if (magic.Data.abilities != null && magic.Data.abilities.Count > 0)
                {
                    if (unit.Data.abilities == null) unit.Data.abilities = new List<AbilityData>();
                    foreach (var ab in magic.Data.abilities)
                        if (ab != null) unit.Data.abilities.Add(ab);
                }
                break;

            case "AddTaunt":
                unit.isTaunt = true;
                break;

            case "RemoveTaunt":
                unit.isTaunt = false;
                break;

            case "Economy":
                GameManager.Instance.AddBonusCoin(1);
                Debug.Log($"<color=yellow>[ECONOMY]</color> Đã nhận 1 vàng từ phép {magic.Data.cardName}");
                break;

            default:
                Debug.LogWarning($"Chưa có logic xử lý cho magicGroup: {magic.Data.magicGroup}");
                break;
        }

        unit.ResetStats();
        Debug.Log($"<color=cyan>[MAGIC]</color> Áp dụng phép {magic.Data.cardName} lên {unit.Data.cardName}. ATK/HP mới: {unit.currentATK}/{unit.currentHP}");
    }
}
