using UnityEngine;
using TMPro;
// ��������������������������������������������������������������������������������������������������������������������������
// File: RunManager.cs
// Purpose : 4�� ���� ��ü ����(�� ����, ������Ȯ��, ���� 2ȸ, ��/����/���ӿ���).
// Notes   : day%7==0 �� ���� �� GameOver, day>28 �� Ending ó��.
//           UI�� weather.GetForecast(0/1)�� ����/�� ������ ǥ��.
// ��������������������������������������������������������������������������������������������������������������������������
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
        public int weekTribute = 500;  // �� ���� �󳳱�

        [Header("UI")]
        public Button nextDayButton;
        public Text dayText;                 // (����)
        public TextMeshProUGUI dayTMP;       // (����)

        // �ߺ� ���� ����
        bool isRunningNextDay = false;

        void Start()
        {
            // �ν����Ϳ��� �� �ɾ �ڵ� ����ǵ��� ó��
            if (nextDayButton != null)
            {
                nextDayButton.onClick.RemoveListener(OnClickNextDay);
                nextDayButton.onClick.AddListener(OnClickNextDay);
                Debug.Log("[RunManager] NextDayBtn ������ ���� �Ϸ�");
            }
            else
            {
                Debug.LogWarning("[RunManager] nextDayButton ���Ҵ� (�ν����Ϳ��� ��ư�� �巡���ϰų�, �ڵ�� �Ҵ��ϼ���)");
            }

            UpdateDayUI();
        }

        void OnDestroy()
        {
            // �ߺ� ������ ����
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

        // (�ʿ��� ���� ȣ���ؼ� EventSystem ����)
        void EnsureUIInputModule()
        {
            var es = EventSystem.current;
            if (es == null)
            {
                var go = new GameObject("EventSystem");
                es = go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
                Debug.Log("[UI-FIX] EventSystem + StandaloneInputModule ����");
                return;
            }
            if (es.currentInputModule == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
                Debug.Log("[UI-FIX] StandaloneInputModule �߰�");
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
            Debug.Log($"[RunManager] NextDay ���� (���� day={day})");

            try
            {
                // �ΰ���
                if (weather == null) { Debug.LogError("[RunManager] WeatherSystem ���� �ʿ�"); yield break; }
                if (farm == null) { Debug.LogError("[RunManager] FarmingSystem ���� �ʿ�"); yield break; }
                if (economy == null) { Debug.LogError("[RunManager] Economy ���� �ʿ�"); yield break; }

                // A) ť ���� '��'�� ���� ������ �� ������ ����
                var fTomorrowPre = weather.GetForecast(0); // ���� �� ����
                var tomorrowType = ForecastMode(fTomorrowPre);
                farm.ApplyForecastToPlots(tomorrowType);
                Debug.Log($"[RunManager] Forecast Preview (Tomorrow={tomorrowType})");

                // B) ���� Ȯ�� + (truth/forecast) ť ����
                var today = weather.ResolveTodayFromForecastAndAdvance();
                Debug.Log($"[RunManager] Today Resolved = {today}");

                // C) ���� ���� ��� ����
                farm.OnNewDay(today);

                // D) �ָ� ��
                if (day % 7 == 0)
                {
                    if (!economy.TrySpend(weekTribute))
                    {
                        GameOver();
                        yield break;
                    }
                }

                // E) ��¥ ����/����
                day++;
                if (day > 28)
                {
                    Ending();
                    yield break;
                }

                // F) UI ����
                UpdateDayUI();
                Debug.Log($"[RunManager] NextDay ���� (���� day={day})");

                yield return null;
            }
            finally
            {
                if (nextDayButton) nextDayButton.interactable = true;
                isRunningNextDay = false;
            }
        }

        // ���� Ȯ������ �ִ밪(���) ���� Ÿ�� ���
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

        void GameOver() { /* �� ��ȯ/�г� ���� */ }
        void Ending() { /* ���� ó�� */ }
    }
}

