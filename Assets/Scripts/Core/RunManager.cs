using UnityEngine;
// RunManager.cs
public class RunManager : MonoBehaviour
{
    public WeatherSystem weather;
    public CombatSystem combat;
    public FarmingSystem farm;
    public Economy economy;

    public int day = 1;            // 1 ~ 28
    public int weekTribute = 500;  // 예: 주 단위 상납금
    public Inventory inventory;

    public void StartRun()
    {
        weather.RollTomorrowForecast(); // 첫 화면에 예보 제공
    }

    public IEnumerator NextDay()
    {
        // 내일 → 오늘 확정
        var today = weather.ResolveTodayFromForecast(); // “오늘 날씨”
        farm.OnNewDay(today);

        // 전투는 하루 2번만 허용
        // UI에서 방 선택 → combat.PlayOneClear(room,...)

        // 장터에서 판매/업그레이드
        // farm.HarvestAndSell(economy);

        // 주말 상납
        if (day % 7 == 0)
        {
            if (!economy.TrySpend(weekTribute))
            {
                GameOver(); yield break;
            }
        }

        day++;
        if (day > 28) { Ending(); yield break; }

        // 다음날 예보 준비
        weather.RollTomorrowForecast();
        yield return null;
    }

    void GameOver() { /* 씬 전환/패널 띄우기 */ }
    void Ending() { /* 엔딩 처리 */ }
}
