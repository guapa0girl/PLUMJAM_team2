using UnityEngine;
using System.Collections.Generic;
using Game.Data;
using Game.Core;

namespace Game.Systems
{
    public class FarmingSystem : MonoBehaviour
    {
        public enum PlotState { Empty, Planted, Mature }

        [System.Serializable]
        public class Plot
        {
            public PlotState state;
            public SeedDef seed; // 현재 심은 씨앗 종류
        }

        [System.Serializable]
        public class PlotVisual
        {
            [Tooltip("이 플롯이 Planted/Mature 되면 켜질 오브젝트")]
            public GameObject visual;
        }

        [Header("Plots (고정 4칸)")]
        [Tooltip("밭 4칸 상태")]
        public List<Plot> plots = new List<Plot>(4) { new Plot(), new Plot(), new Plot(), new Plot() };

        [Header("Plots Visuals")]
        public PlotVisual[] plotVisuals = new PlotVisual[4];

        [Header("Planting Rule")]
        [Min(1), Tooltip("한 블럭을 심는 데 필요한 씨앗 개수")]
        public int seedsPerBlock = 1;

        [Header("Worst Weather Mapping")]
        [Tooltip("씨앗의 preferred와 반대인 '최악' 날씨 매핑")]
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

            // 시작 시 모든 비주얼 꺼주기
            for (int i = 0; i < plotVisuals.Length; i++)
            {
                if (plotVisuals[i] != null && plotVisuals[i].visual != null)
                    plotVisuals[i].visual.SetActive(false);
            }
        }

        /// <summary>인벤토리에서 씨앗 소모 후 심기</summary>
        public bool TryPlant(Inventory inv, SeedDef seed, int plotIndex)
        {
            if (seed == null || plotIndex < 0 || plotIndex >= plots.Count) return false;
            var p = plots[plotIndex];
            if (p.state != PlotState.Empty) return false;

            if (inv == null || !inv.TryConsume(seed, seedsPerBlock)) return false;

            p.seed = seed;
            p.state = PlotState.Planted;
            plots[plotIndex] = p;

            // 비주얼 켜기
            if (plotIndex < plotVisuals.Length && plotVisuals[plotIndex].visual != null)
                plotVisuals[plotIndex].visual.SetActive(true);

            return true;
        }

        /// <summary>"다음 날" 판정</summary>
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
                    p.state = PlotState.Mature;
                }
                else if (today == worst)
                {
                    // 망함: 비워주고 비주얼 끄기
                    p = new Plot();
                    if (i < plotVisuals.Length && plotVisuals[i].visual != null)
                        plotVisuals[i].visual.SetActive(false);
                }

                plots[i] = p;
            }
        }

        /// <summary>수확 → 판매 → 밭 비움</summary>
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

                    // 수확 시 비주얼 꺼주기
                    if (i < plotVisuals.Length && plotVisuals[i].visual != null)
                        plotVisuals[i].visual.SetActive(false);
                }
                plots[i] = p;
            }
            if (earned > 0 && econ != null) econ.AddMoney(earned);
            return earned;
        }

        WeatherType GetWorst(WeatherType preferred)
        {
            if (worstLookup != null && worstLookup.TryGetValue(preferred, out var w)) return w;
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
