using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    public PlayerStat player;        // 비워두면 PlayerStat.instance 사용
    public Slider bar;               // Slider (하단 중앙)
    public TextMeshProUGUI value;    // "현재/최대" 텍스트

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