using UnityEngine;
// ─────────────────────────────────────────────────────────────
// File: RunManager.cs
// Purpose : 4주 러닝 전체 루프(일 진행, 예보→확정, 전투 2회, 상납/엔딩/게임오버).
// Defines : class RunManager : MonoBehaviour
// Fields  : day(1~28), weekTribute(주 단위 상납), Weather/Combat/Farm/Economy 참조
// API     : StartRun()  → 예보(내일/모레 등 horizon) 초기 롤링
//           NextDay()   → 오늘 확정(예보 기반)→농사 업데이트→전투/상점→상납→예보 큐 자동 전진
// Used By : UI/메인 게임 루프(버튼/자동 진행 트리거).
// Notes   : day%7==0 상납 실패 시 GameOver, day>28 시 Ending 처리.
//           UI는 weather.GetForecast(0/1)로 내일/모레 예보를 표시.
// ─────────────────────────────────────────────────────────────
using System.Collections;
using Game.Core;
using Game.Systems;           // WeatherSystem
using Game.Systems.Combat;    // CombatSystem
using Game.Data;

namespace Game.Core
{
    public class RunManager : MonoBehaviour
    {
        [Header("Systems")]
        public WeatherSystem weather;
        public CombatSystem combat;
        public FarmingSystem farm;
        public Economy economy;
        public Inventory inventory;

        [Header("Run Config")]
        public int day = 1;            // 1 ~ 28
        public int weekTribute = 500;  // 주 단위 상납금

        public void StartRun()
        {
            // 예보(내일/모레 … horizon) 초기화
            // WeatherSystem.Awake에서 이미 채웠다면 생략 가능하지만, 명시적으로 호출해 둔다.
            weather.RollForecastHorizon();

            // UI: 내일/모레 예보를 화면에 갱신하고 싶으면 여기서 읽어 반영
            // var fTomorrow = weather.GetForecast(0);
            // var fDayAfter = weather.GetForecast(1);
            // TODO: UI 업데이트
        }

        public IEnumerator NextDay()
        {
            // 1) "내일 예보"를 기반으로 오늘 날씨 확정 + 예보 큐 전진(모레 유지)
            var today = weather.ResolveTodayFromForecastAndAdvance();

            // 2) 농사 업데이트(선호 날씨면 성장, 아니면 즉사)
            farm.OnNewDay(today);

            // 3) 전투는 하루 2번만 허용
            //    UI에서 방 선택 → yield return combat.PlayOneClear(room, inventory, dropMult);
            //    두 번까지 실행 가능(CombatSystem 내부 제한)

            // 4) 장터에서 판매/업그레이드
            // farm.HarvestAndSell(economy);
            // skillSystem.TryUpgrade(...);

            // 5) 주말 상납
            if (day % 7 == 0)
            {
                if (!economy.TrySpend(weekTribute))
                {
                    GameOver();
                    yield break;
                }
            }

            // 6) 날짜 진행/엔딩
            day++;
            if (day > 28)
            {
                Ending();
                yield break;
            }

            // 7) UI: 전진된 예보(새로운 내일/모레)를 다시 갱신
            // var fTomorrow = weather.GetForecast(0);
            // var fDayAfter = weather.GetForecast(1);
            // TODO: UI 업데이트

            yield return null;
        }

        void GameOver() { /* 씬 전환/패널 띄우기 */ }
        void Ending() { /* 엔딩 처리 */ }
    }
}
