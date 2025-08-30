using UnityEngine;
using System.Collections.Generic;
using Game.Data;
using Game.Core;

namespace Game.Systems
{
    // ��������������������������������������������������������������������������������������������������������������������������
    // File: FarmingSystem.cs
    // Purpose : 4�� �� ���� �. ���� ������ "���� ��" ������ ����
    //           ��� ���� / ���� / ���(�� ���) ����.
    //           �� �� �ɱ⿡�� ���� N���� �ʿ�(�ν����� ����).
    // ��������������������������������������������������������������������������������������������������������������������������
    public class FarmingSystem : MonoBehaviour
    {
        public enum PlotState { Empty, Planted, Mature } // Dead�� ��ٷ� Empty ó��

        [System.Serializable]
        public class Plot
        {
            public PlotState state;
            public SeedDef seed;     // ���� ���� ���� ����
        }

        [Header("Plots (���� 4ĭ)")]
        [Tooltip("�� 4ĭ ����")]
        public List<Plot> plots = new List<Plot>(4) { new Plot(), new Plot(), new Plot(), new Plot() };

        [Header("Planting Rule")]
        [Min(1), Tooltip("�� ���� �ɴ� �� �ʿ��� ���� ����")]
        public int seedsPerBlock = 3;

        [Header("Worst Weather Mapping")]
        [Tooltip("������ preferred(�ֻ�)�� �ݴ��� '�־�' ���� ����(������ �� �⺻ ��Ģ ���)")]
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

        /// <summary>�κ��丮���� ������ seedsPerBlock��ŭ �Ҹ��ϰ� �ش� ĭ�� �ɽ��ϴ�.</summary>
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
        /// "���� �� ��ħ"�� ȣ��. ���� ������ ���� �� ĭ ����:
        /// - �ֻ�(Preferred): ��� Mature
        /// - ����: Planted ����(��ȭ ����)
        /// - �־�: ��� ���(Empty)
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
                    p.state = PlotState.Mature; // �ٷ� ��Ȯ ����
                }
                else if (today == worst)
                {
                    // �־�: ��� ����, �� ���
                    p = new Plot(); // Empty�� �ʱ�ȭ
                }
                // ����: �״�� ����

                plots[i] = p;
            }
        }

        /// <summary>������ ĭ�� �Ǹ��ϰ� ���ϴ�. ���� �հ踦 ��ȯ.</summary>
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

            // �⺻ ��Ģ(���): preferred�� ���� �迭 ����, ���Ƿ� �ϳ� ����
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
