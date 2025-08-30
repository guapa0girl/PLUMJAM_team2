using UnityEngine;
// ─────────────────────────────────────────────────────────────
// File: RunManager.cs
// Purpose : 4주 러닝 전체 루프(일 진행, 예보→확정, 전투 2회, 상납/엔딩/게임오버).
// Defines : class RunManager : MonoBehaviour
// Fields  : day(1~28), weekTribute(주 단위 상납), Weather/Combat/Farm/Economy 참조
// API     : StartRun()  → 첫 예보 롤링
//           NextDay()   → 오늘 확정→농사 업데이트→전투/상점→상납→다음날 예보
// Used By : UI/메인 게임 루프(버튼/자동 진행 트리거).
// Notes   : day%7==0 상납 실패 시 GameOver, day>28 시 Ending 처리.
// ─────────────────────────────────────────────────────────────
using System.Collections;
using Game.Core;
using Game.Systems;
using Game.Systems.Combat;
using Game.Data;

namespace Game.Core
{
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
}