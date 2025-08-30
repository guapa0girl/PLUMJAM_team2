using UnityEngine;
using TMPro;

public class SeedUI : MonoBehaviour
{
    public SeedInventory inventory;      // ����θ� �ڵ����� Player���� ã��
    public TextMeshProUGUI seedText;     // ���� UI �ؽ�Ʈ

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

        seedText.text = seedDisplay;  // ���� �������� ���� ǥ��
    }
}