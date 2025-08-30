using UnityEngine;
using TMPro;
// ─────────────────────────────────────────────────────────────
// File: RunManager.cs
// Purpose : 4주 러닝 전체 루프(일 진행, 예보→확정, 전투 2회, 상납/엔딩/게임오버).
// Notes   : day%7==0 상납 실패 시 GameOver, day>28 시 Ending 처리.
//           UI는 weather.GetForecast(0/1)로 내일/모레 예보를 표시.
// ─────────────────────────────────────────────────────────────
using System.Collections;
using Game.Systems;           // WeatherSystem, FarmingSystem
using Game.Data;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.Core
{
    public class RunManager : MonoBehaviour
    {
        [Header("Systems")]
        public WeatherSystem weather;
        public FarmingSystem farm;
        public Economy economy;
        public Inventory inventory;

        [Header("Run Config")]
        public int day = 1;            // 1 ~ 28
        public int weekTribute = 500;  // 주 단위 상납금

        [Header("UI")]
        public Button nextDayButton;
        public Text dayText;                 // (선택)
        public TextMeshProUGUI dayTMP;       // (선택)

        // 중복 실행 방지
        bool isRunningNextDay = false;

        void Start()
        {
            // 인스펙터에서 안 걸어도 자동 연결되도록 처리
            if (nextDayButton != null)
            {
                nextDayButton.onClick.RemoveListener(OnClickNextDay);
                nextDayButton.onClick.AddListener(OnClickNextDay);
                Debug.Log("[RunManager] NextDayBtn 리스너 연결 완료");
            }
            else
            {
                Debug.LogWarning("[RunManager] nextDayButton 미할당 (인스펙터에서 버튼을 드래그하거나, 코드로 할당하세요)");
            }

            UpdateDayUI();
        }

        void OnDestroy()
        {
            // 중복 리스너 방지
            if (nextDayButton != null)
                nextDayButton.onClick.RemoveListener(OnClickNextDay);
        }

        public void OnClickNextDay()
        {
            if (isRunningNextDay) return;
            StartCoroutine(NextDay());
        }

        void UpdateDayUI()
        {
            string label = $"Day {day}";
            if (dayTMP != null) dayTMP.text = label;
            if (dayText != null) dayText.text = label;
        }

        // (필요할 때만 호출해서 EventSystem 보정)
        void EnsureUIInputModule()
        {
            var es = EventSystem.current;
            if (es == null)
            {
                var go = new GameObject("EventSystem");
                es = go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
                Debug.Log("[UI-FIX] EventSystem + StandaloneInputModule 생성");
                return;
            }
            if (es.currentInputModule == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
                Debug.Log("[UI-FIX] StandaloneInputModule 추가");
            }
        }

        public void StartRun()
        {
            UpdateDayUI();
        }

        public IEnumerator NextDay()
        {
            isRunningNextDay = true;
            if (nextDayButton) nextDayButton.interactable = false;
            Debug.Log($"[RunManager] NextDay 시작 (현재 day={day})");

            try
            {
                // 널가드
                if (weather == null) { Debug.LogError("[RunManager] WeatherSystem 참조 필요"); yield break; }
                if (farm == null) { Debug.LogError("[RunManager] FarmingSystem 참조 필요"); yield break; }
                if (economy == null) { Debug.LogError("[RunManager] Economy 참조 필요"); yield break; }

                // A) 큐 전진 '전'의 내일 예보로 밭 프리뷰 갱신
                var fTomorrowPre = weather.GetForecast(0); // 전진 전 내일
                var tomorrowType = ForecastMode(fTomorrowPre);
                farm.ApplyForecastToPlots(tomorrowType);
                Debug.Log($"[RunManager] Forecast Preview (Tomorrow={tomorrowType})");

                // B) 오늘 확정 + (truth/forecast) 큐 전진
                var today = weather.ResolveTodayFromForecastAndAdvance();
                Debug.Log($"[RunManager] Today Resolved = {today}");

                // C) 오늘 날씨 농사 판정
                farm.OnNewDay(today);

                // D) 주말 상납
                if (day % 7 == 0)
                {
                    if (!economy.TrySpend(weekTribute))
                    {
                        GameOver();
                        yield break;
                    }
                }

                // E) 날짜 진행/엔딩
                day++;
                if (day > 28)
                {
                    Ending();
                    yield break;
                }

                // F) UI 갱신
                UpdateDayUI();
                Debug.Log($"[RunManager] NextDay 종료 (현재 day={day})");

                yield return null;
            }
            finally
            {
                if (nextDayButton) nextDayButton.interactable = true;
                isRunningNextDay = false;
            }
        }

        // 예보 확률에서 최대값(모드) 날씨 타입 얻기
        WeatherType ForecastMode(WeatherSystem.Forecast f)
        {
            float best = f.heat; int idx = 0;
            if (f.rain > best) { best = f.rain; idx = 1; }
            if (f.snow > best) { best = f.snow; idx = 2; }
            if (f.cloud > best) { best = f.cloud; idx = 3; }

            switch (idx)
            {
                case 0: return WeatherType.Heat;
                case 1: return WeatherType.Rain;
                case 2: return WeatherType.Snow;
                default: return WeatherType.Cloud;
            }
        }

        void GameOver() { /* 씬 전환/패널 띄우기 */ }
        void Ending() { /* 엔딩 처리 */ }
    }
}

