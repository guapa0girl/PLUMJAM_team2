using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Timer")]
    public float startSeconds = 60f;       // 1S분
    public bool autoStart = true;
    public TextMeshProUGUI timerText;       // 상단 중앙 텍스트

    float timeLeft;
    bool running;

    void Awake()
    {
        instance = this;
        ResetTimer();
        if (autoStart) StartTimer();
        UpdateTimerUI();
    }

    void Update()
    {
        if (!running) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running = false;
            UpdateTimerUI();
            return; // 여기서 끝. (원하면 이후 행동은 버튼/매니저로 연결)
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (!timerText) return;
        int total = Mathf.CeilToInt(timeLeft);
        timerText.text = $"{total / 60:00}:{total % 60:00}";
    }

    public void StartTimer() => running = true;
    public void StopTimer() => running = false;
    public void ResetTimer() { timeLeft = startSeconds; }
    public float TimeLeft => timeLeft;
}