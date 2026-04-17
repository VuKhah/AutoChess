using UnityEngine;
using System.Collections.Generic;

public class ProjectSanityChecker : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== BẮT ĐẦU KIỂM CHỨNG NỀN TẢNG DỰ ÁN ===");

        // 1. Kiểm tra Database
        if (CardDatabase.Instance == null)
        {
            Debug.LogError("LỖI: CardDatabase chưa được khởi tạo!");
            return;
        }
        Debug.Log("OK: CardDatabase đã nạp " + "..." + " lá bài.");

        // 2. Kiểm tra Logic Kinh tế
        EconomyManager testEco = new EconomyManager();
        testEco.ResetEconomy();
        testEco.Buy(); // Mua 1 lá
        Debug.Log(testEco.CurrentCoin == 7 ? "OK: Kinh tế trừ tiền đúng (Còn 7)" : "LỖI: Kinh tế trừ tiền sai!");

        // 3. Kiểm tra Logic Chiến đấu & Taunt
        RunCombatTest();

        // 4. Kiểm tra AI Library
        if (AIManager.Instance != null && AIManager.Instance.loadedLibrary != null)
        {
            Debug.Log("OK: Đã tìm thấy bộ não HardBot với Gene Aggression: " + AIManager.Instance.loadedLibrary.hardBot.genes[0]);
        }
        else
        {
            Debug.LogWarning("CHÚ Ý: AI_Library.json chưa được tạo hoặc chưa nạp thành công.");
        }

        Debug.Log("=== KẾT THÚC KIỂM CHỨNG ===");
    }

    void RunCombatTest()
    {
        CombatResolver resolver = new CombatResolver();
        List<CardInstance> pBoard = new List<CardInstance>(new CardInstance[6]);
        List<CardInstance> eBoard = new List<CardInstance>(new CardInstance[6]);

        // Tạo 1 quân taunt ở slot 5 cho đối thủ
        CardDefinition tauntDef = new CardDefinition { cardName = "Tường Chắn", ability = AbilityType.Taunt, baseATK = 1, baseHP = 10 };
        eBoard[5] = new CardInstance(tauntDef, 5);

        // Tạo 1 quân đánh của mình ở slot 0
        CardDefinition atkDef = new CardDefinition { cardName = "Chiến Binh", ability = AbilityType.None, baseATK = 2, baseHP = 5 };
        pBoard[0] = new CardInstance(atkDef, 0);

        TurnRecord log = new TurnRecord();
        resolver.ResolveTurn(pBoard, eBoard, log);

        // Kiểm tra log xem quân taunt ở slot 5 có bị mất máu không (dù quân mình ở slot 0)
        if (eBoard[5].currentHP < 10)
        {
            Debug.Log("OK: Logic Taunt hoạt động! Unit slot 0 đã tự tìm đánh Taunt slot 5.");
        }
        else
        {
            Debug.LogError("LỖI: Logic Taunt thất bại!");
        }
    }
}