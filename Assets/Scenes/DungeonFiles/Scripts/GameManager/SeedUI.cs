using UnityEngine;
using TMPro;

public class SeedUI : MonoBehaviour
{
    public SeedInventory inventory;      // 비워두면 자동으로 Player에서 찾음
    public TextMeshProUGUI seedText;     // 씨앗 UI 텍스트

    void Start()
    {
        if (!inventory)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) inventory = p.GetComponent<SeedInventory>();
        }
    }

    void Update()
    {
        if (!inventory || !seedText) return;

        string seedDisplay = "Seeds:\n";

        foreach (var seed in inventory.GetAllSeedCounts())
        {
            seedDisplay += $"{seed.Key.seedName}: {seed.Value}\n";
        }

        seedText.text = seedDisplay;  // 씨앗 종류별로 갯수 표시
    }
}