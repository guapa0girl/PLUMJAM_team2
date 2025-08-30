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
            public SeedDef seed; // ���� ���� ���� ����
        }

        [System.Serializable]
        public class PlotVisual
        {
            [Tooltip("�� �÷��� Planted/Mature �Ǹ� ���� ������Ʈ")]
            public GameObject visual;
        }

        [Header("Plots (���� 4ĭ)")]
        [Tooltip("�� 4ĭ ����")]
        public List<Plot> plots = new List<Plot>(4) { new Plot(), new Plot(), new Plot(), new Plot() };

        [Header("Plots Visuals")]
        public PlotVisual[] plotVisuals = new PlotVisual[4];

        [Header("Planting Rule")]
        [Min(1), Tooltip("�� ���� �ɴ� �� �ʿ��� ���� ����")]
        public int seedsPerBlock = 1;

        [Header("Worst Weather Mapping")]
        [Tooltip("������ preferred�� �ݴ��� '�־�' ���� ����")]
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

            // ���� �� ��� ���־� ���ֱ�
            for (int i = 0; i < plotVisuals.Length; i++)
            {
                if (plotVisuals[i] != null && plotVisuals[i].visual != null)
                    plotVisuals[i].visual.SetActive(false);
            }
        }

        /// <summary>�κ��丮���� ���� �Ҹ� �� �ɱ�</summary>
        public bool TryPlant(Inventory inv, SeedDef seed, int plotIndex)
        {
            if (seed == null || plotIndex < 0 || plotIndex >= plots.Count) return false;
            var p = plots[plotIndex];
            if (p.state != PlotState.Empty) return false;

            if (inv == null || !inv.TryConsume(seed, seedsPerBlock)) return false;

            p.seed = seed;
            p.state = PlotState.Planted;
            plots[plotIndex] = p;

            // ���־� �ѱ�
            if (plotIndex < plotVisuals.Length && plotVisuals[plotIndex].visual != null)
                plotVisuals[plotIndex].visual.SetActive(true);

            return true;
        }

        /// <summary>"���� ��" ����</summary>
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
                    // ����: ����ְ� ���־� ����
                    p = new Plot();
                    if (i < plotVisuals.Length && plotVisuals[i].visual != null)
                        plotVisuals[i].visual.SetActive(false);
                }

                plots[i] = p;
            }
        }

        /// <summary>��Ȯ �� �Ǹ� �� �� ���</summary>
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

                    // ��Ȯ �� ���־� ���ֱ�
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
