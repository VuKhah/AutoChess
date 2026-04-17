using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Cần thêm cái này để dùng ToList()

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance;

    private List<CardDefinition> unitList = new List<CardDefinition>();
    private List<CardDefinition> magicList = new List<CardDefinition>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // Đảm bảo Singleton duy nhất

        LoadDatabase();
    }

    void LoadDatabase()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("CardsData");
        if (jsonFile == null)
        {
            Debug.LogError("LỖI: Không tìm thấy file CardsData trong thư mục Resources!");
            return;
        }

        CardDataWrapper data = JsonUtility.FromJson<CardDataWrapper>(jsonFile.text);
        unitList.Clear();
        magicList.Clear();
        foreach (var card in data.cards)
        {
            if (card.cardType == CardType.Unit)
                unitList.Add(card);
            else if (card.cardType == CardType.Magic)
                magicList.Add(card);
        }
        Debug.Log($"<color=green>[DATABASE]</color> Đã nạp thành công {unitList.Count + magicList.Count} lá bài.");
    }
    public List<CardDefinition> GetAllCards()
    {
        return unitList.Concat(magicList).ToList();
    }

    public List<CardDefinition> GetRandomShop(int count)
    {
        List<CardDefinition> allCards = GetAllCards();
        List<CardDefinition> shop = new List<CardDefinition>();

        if (allCards.Count == 0) return shop;

        for (int i = 0; i < count; i++)
        {
            shop.Add(allCards[Random.Range(0, allCards.Count)]);
        }
        return shop;
    }

    public CardDefinition GetCard(string id)
    {
        var unit = unitList.FirstOrDefault(card => card.cardID == id);
        if (unit != null) return unit;

        var magic = magicList.FirstOrDefault(card => card.cardID == id);
        if (magic != null) return magic;

        return null;
    }
    public List<CardDefinition> GetAllUnits() => unitList;
    public List<CardDefinition> GetAllMagics() => magicList;
}

