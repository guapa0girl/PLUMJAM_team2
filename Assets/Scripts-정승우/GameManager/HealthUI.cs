using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    public PlayerStat player;        // ����θ� PlayerStat.instance ���
    public Slider bar;               // Slider (�ϴ� �߾�)
    public TextMeshProUGUI value;    // "����/�ִ�" �ؽ�Ʈ

    void Start()
    {
        if (!player) player = PlayerStat.instance;
    }

    void Update()
    {
        if (!player) return;

        if (bar)
        {
            bar.maxValue = player.maxHealth;
            bar.value = player.health;
        }

        if (value)
        {
            int cur = Mathf.CeilToInt(player.health);
            int max = Mathf.CeilToInt(player.maxHealth);
            value.text = $"{cur} / {max}";
        }
    }
}