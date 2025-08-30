using UnityEngine;
using System.Collections.Generic;
using Game.Data;
using Game.Core;

namespace Game.Systems
{
    // ─────────────────────────────────────────────────────────────
    // File: FarmingSystem.cs
    // Purpose : 4개 밭 블럭을 운영. 오늘 심으면 "다음 날" 날씨에 따라
    //           즉시 성숙 / 유지 / 즉사(밭 비움) 판정.
    //           한 블럭 심기에는 씨앗 N개가 필요(인스펙터 설정).
    // ─────────────────────────────────────────────────────────────
    public class FarmingSystem : MonoBehaviour
    {
        public enum PlotState { Empty, Planted, Mature } // Dead은 곧바로 Empty 처리

        [System.Serializable]
        public class Plot
        {
            public PlotState state;
            public SeedDef seed;     // 현재 심은 씨앗 종류
        }

        [Header("Plots (고정 4칸)")]
        [Tooltip("밭 4칸 상태")]
        public List<Plot> plots = new List<Plot>(4) { new Plot(), new Plot(), new Plot(), new Plot() };

        [Header("Planting Rule")]
        [Min(1), Tooltip("한 블럭을 심는 데 필요한 씨앗 개수")]
        public int seedsPerBlock = 3;

        [Header("Worst Weather Mapping")]
        [Tooltip("씨앗의 preferred(최상)와 반대인 '최악' 날씨 매핑(미지정 시 기본 규칙 사용)")]
        public WorstMapEntry[] worstMap = new WorstMapEntry[]
        {
            new WorstMapEntry{ forPreferred = WeatherType.Heat,  worst = WeatherType.Snow },
            new WorstMapEntry{ forPreferred = WeatherType.Rain,  worst = WeatherType.Heat },
            new WorstMapEntry{ forPreferred = WeatherType.Snow,  worst = WeatherType.Heat },
            new WorstMapEntry{ forPreferred = WeatherType.Cloud, worst = WeatherType.Rain },
        };

        [System.Serializable]
        public struct WorstMapEntry { public WeatherType forPreferred; public WeatherType worst; }

        Dictionary<WeatherType, WeatherType> worstLookup;

        void Awake()
        {
            worstLookup = new Dictionary<WeatherType, WeatherType>();
            foreach (var e in worstMap) worstLookup[e.forPreferred] = e.worst;
        }

        /// <summary>인벤토리에서 씨앗을 seedsPerBlock만큼 소모하고 해당 칸을 심습니다.</summary>
        public bool TryPlant(Inventory inv, SeedDef seed, int plotIndex)
        {
            if (seed == null || plotIndex < 0 || plotIndex >= plots.Count) return false;
            var p = plots[plotIndex];
            if (p.state != PlotState.Empty) return false;

            if (inv == null || !inv.TryConsume(seed, seedsPerBlock)) return false;

            p.seed = seed;
            p.state = PlotState.Planted;
            plots[plotIndex] = p;
            return true;
        }

        /// <summary>
        /// "다음 날 아침"에 호출. 오늘 날씨에 따라 각 칸 판정:
        /// - 최상(Preferred): 즉시 Mature
        /// - 보통: Planted 유지(변화 없음)
        /// - 최악: 즉시 비움(Empty)
        /// </summary>
        public void OnNewDay(WeatherType today)
        {
            for (int i = 0; i < plots.Count; i++)
            {
                var p = plots[i];
                if (p.state != PlotState.Planted || p.seed == null) { plots[i] = p; continue; }

                var preferred = p.seed.preferred;
                var worst = GetWorst(preferred);

                if (today == preferred)
                {
                    p.state = PlotState.Mature; // 바로 수확 가능
                }
                else if (today == worst)
                {
                    // 최악: 즉시 실패, 밭 비움
                    p = new Plot(); // Empty로 초기화
                }
                // 보통: 그대로 유지

                plots[i] = p;
            }
        }

        /// <summary>성숙한 칸만 판매하고 비웁니다. 수익 합계를 반환.</summary>
        public int HarvestAndSell(Economy econ)
        {
            int earned = 0;
            for (int i = 0; i < plots.Count; i++)
            {
                var p = plots[i];
                if (p.state == PlotState.Mature && p.seed != null)
                {
                    earned += p.seed.sellPrice;
                    p = new Plot(); // Empty
                    plots[i] = p;
                }
            }
            if (earned > 0 && econ != null) econ.AddMoney(earned);
            return earned;
        }

        WeatherType GetWorst(WeatherType preferred)
        {
            if (worstLookup != null && worstLookup.TryGetValue(preferred, out var w)) return w;

            // 기본 규칙(백업): preferred와 같은 계열 제외, 임의로 하나 선택
            switch (preferred)
            {
                case WeatherType.Heat: return WeatherType.Snow;
                case WeatherType.Rain: return WeatherType.Heat;
                case WeatherType.Snow: return WeatherType.Heat;
                case WeatherType.Cloud: return WeatherType.Rain;
                default: return WeatherType.Cloud;
            }
        }
    }
}
