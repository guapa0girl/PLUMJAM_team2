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
            public SeedDef seed; // SeedDef.preferred : WeatherType
        }

        [System.Serializable]
        public class PlotVisual
        {
            [Tooltip("�⺻/���� ǥ�ÿ�(�÷Ժ� 1��): FarmTile-seed #")]
            public GameObject visual;
        }

        [Header("Plots (���� 4ĭ)")]
        public List<Plot> plots = new List<Plot>(4) { new Plot(), new Plot(), new Plot(), new Plot() };

        [Header("Plots Visuals (�⺻/����)")]
        public PlotVisual[] plotVisuals = new PlotVisual[4];

        // ���� ���� ������Ʈ 16�� (4�� �� 4�÷�)
        [Header("Per-Plot Weather GOs (�� 16��)")]
        [Tooltip("�� �÷��� Heat ������Ʈ 4�� (FarmTile-heat 1..4)")]
        public GameObject[] heatGOs = new GameObject[4];
        [Tooltip("�� �÷��� Rain ������Ʈ 4�� (FarmTile-rain 1..4)")]
        public GameObject[] rainGOs = new GameObject[4];
        [Tooltip("�� �÷��� Snow ������Ʈ 4�� (FarmTile-snow 1..4)")]
        public GameObject[] snowGOs = new GameObject[4];
        [Tooltip("�� �÷��� Cloud ������Ʈ 4�� (FarmTile-cloud 1..4)")]
        public GameObject[] cloudGOs = new GameObject[4];

        // �־� ���� ǥ�� ? FarmTile-plot 1..4
        [Header("Worst tiles (�� �÷��� FarmTile-plot 1..4)")]
        public GameObject[] plotWorstGOs = new GameObject[4];

        [Header("Planting Rule")]
        [Min(1)] public int seedsPerBlock = 1;

        [Header("Worst Weather Mapping")]
        public WorstMapEntry[] worstMap = new WorstMapEntry[]
        {
            new WorstMapEntry{ forPreferred = WeatherType.Heat,  worst = WeatherType.Cloud },
            new WorstMapEntry{ forPreferred = WeatherType.Rain,  worst = WeatherType.Snow },
            new WorstMapEntry{ forPreferred = WeatherType.Snow,  worst = WeatherType.Rain },
            new WorstMapEntry{ forPreferred = WeatherType.Cloud, worst = WeatherType.Heat },
        };
        [System.Serializable] public struct WorstMapEntry { public WeatherType forPreferred; public WeatherType worst; }

        [Header("Startup")]
        [Tooltip("���� ���� �� �� �÷��� ���� 'plot' ǥ�� + �ɱ� ����(Empty) ���·� �ʱ�ȭ")]
        public bool showPlotsOnStart = true;

        [Header("Pickup")]
        [Tooltip("�۹� GO(heat/rain/snow/cloud)�� Trigger Collider�� ������ �ڵ����� �߰�")]
        public bool autoAddPickupCollider = true;

        Dictionary<WeatherType, WeatherType> worstLookup;

        void Awake()
        {
            worstLookup = new Dictionary<WeatherType, WeatherType>();
            foreach (var e in worstMap) worstLookup[e.forPreferred] = e.worst;

            // �ϴ� ��� ����
            for (int i = 0; i < 4; i++)
            {
                Set(plotVisuals, i, false);
                Set(heatGOs, i, false);
                Set(rainGOs, i, false);
                Set(snowGOs, i, false);
                Set(cloudGOs, i, false);
                Set(plotWorstGOs, i, false);
            }

            // �۹� GO�鿡 Trigger Collider �ڵ� ����
            if (autoAddPickupCollider)
            {
                for (int i = 0; i < 4; i++)
                {
                    EnsureTriggerCollider(heatGOs[i]);
                    EnsureTriggerCollider(rainGOs[i]);
                    EnsureTriggerCollider(snowGOs[i]);
                    EnsureTriggerCollider(cloudGOs[i]);
                }
            }
        }

        void Start()
        {
            if (!showPlotsOnStart) return;
            for (int i = 0; i < plots.Count && i < 4; i++)
            {
                EnsurePlantable(i); // ���� Empty + plotWorst ON
            }
        }

        // ������������������������������������������������������������������������������������������
        // �ɱ�
        // ������������������������������������������������������������������������������������������
        public bool TryPlant(Inventory inv, SeedDef seed, int plotIndex)
        {
            if (seed == null || plotIndex < 0 || plotIndex >= plots.Count) return false;
            var p = plots[plotIndex];
            if (p.state != PlotState.Empty) return false;               // Empty�� �ɱ� ����
            if (inv == null || !inv.TryConsume(seed, seedsPerBlock)) return false;

            p.seed = seed;
            p.state = PlotState.Planted;
            plots[plotIndex] = p;

            ShowSeedOnly(plotIndex); // ���Ѹ� ǥ��
            return true;
        }

        // ������������������������������������������������������������������������������������������
        // �Ϸ� ����(���� ���� ����)
        // ������������������������������������������������������������������������������������������
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
                    // ? ����: ���� ��ȯ + ���� �۹� GO�� �ݵ�� �Ѽ� '�浹 ����' ���·� �����
                    p.state = PlotState.Mature;
                    ShowWeather(i, preferred);
                }
                else if (today == worst)
                {
                    // ? �־�: ������ �ٽ� �ɱ� ���� ���·� ��ȯ + plot�� ǥ��
                    EnsurePlantable(i);
                    p = plots[i]; // ����ȭ
                }

                plots[i] = p;
            }
        }

        // ������������������������������������������������������������������������������������������
        // ��Ȯ �� Ÿ�Ժ� ���� ���� �� Economy�� ���� ���� �� �ٽ� �ɱ� ���� ����
        // ������������������������������������������������������������������������������������������
        public int HarvestAndSell(Economy econ)
        {
            int heat = 0, rain = 0, snow = 0, cloud = 0;

            for (int i = 0; i < plots.Count; i++)
            {
                var p = plots[i];
                if (p.state == PlotState.Mature && p.seed != null)
                {
                    switch (p.seed.preferred)
                    {
                        case WeatherType.Heat: heat++; break;
                        case WeatherType.Rain: rain++; break;
                        case WeatherType.Snow: snow++; break;
                        case WeatherType.Cloud: cloud++; break;
                    }

                    // ? ��Ȯ ��: ������ �ٽ� �ɱ� ���� ���� + plot ǥ��
                    EnsurePlantable(i);
                }
            }

            if (econ == null) return 0;
            int earned = econ.PayForCounts(heat, rain, snow, cloud); // Economy�� money�� ���� ����
            return earned; // UI/�α׿�
        }

        // ������������������������������������������������������������������������������������������
        // (ť ���� ��) ���� ���� ������
        // ������������������������������������������������������������������������������������������
        /// Empty �� plot ����(���� �ǻ�Ƴ� ����)
        public void ApplyForecastToPlots(WeatherType forecastTomorrow)
        {
            for (int i = 0; i < plots.Count; i++)
            {
                var p = plots[i];

                if (p.state == PlotState.Empty) { ShowWorst(i); continue; }
                if (p.state == PlotState.Mature) continue;
                if (p.seed == null) { ShowWorst(i); continue; }

                var preferred = p.seed.preferred;
                var worst = GetWorst(preferred);

                if (forecastTomorrow == preferred) ShowWeather(i, preferred);
                else if (forecastTomorrow == worst) ShowWorst(i);
                else ShowSeedOnly(i);
            }
        }

        // ������������������������������������������������������������������������������������������
        // ? �ɱ� ���� ���� �Լ� (�䱸���� �ٽ�)
        // ������������������������������������������������������������������������������������������
        void EnsurePlantable(int i)
        {
            if (i < 0 || i >= plots.Count) return;
            var p = plots[i];
            p.state = PlotState.Empty;
            p.seed = null;
            plots[i] = p;

            ShowWorst(i); // ȭ�鿣 plot�� ����
        }

        public bool IsPlantable(int i)
        {
            return (i >= 0 && i < plots.Count && plots[i].state == PlotState.Empty);
        }

        // ���������������������������������� ǥ�� ����(�÷� �ε�����) ����������������������������������
        void ShowNone(int i)
        {
            Set(plotVisuals, i, false);
            Set(heatGOs, i, false);
            Set(rainGOs, i, false);
            Set(snowGOs, i, false);
            Set(cloudGOs, i, false);
            Set(plotWorstGOs, i, false);
        }

        void ShowSeedOnly(int i)
        {
            Set(plotWorstGOs, i, false);
            Set(heatGOs, i, false);
            Set(rainGOs, i, false);
            Set(snowGOs, i, false);
            Set(cloudGOs, i, false);
            Set(plotVisuals, i, true);   // ���� on
        }

        void ShowWeather(int i, WeatherType wt)
        {
            Set(plotVisuals, i, false);  // ���� off
            Set(plotWorstGOs, i, false);
            Set(heatGOs, i, wt == WeatherType.Heat);
            Set(rainGOs, i, wt == WeatherType.Rain);
            Set(snowGOs, i, wt == WeatherType.Snow);
            Set(cloudGOs, i, wt == WeatherType.Cloud);
        }

        void ShowWorst(int i)
        {
            // ����/���� ���� off, plotWorst on (������ ���)
            Set(plotVisuals, i, false);
            Set(heatGOs, i, false);
            Set(rainGOs, i, false);
            Set(snowGOs, i, false);
            Set(cloudGOs, i, false);
            var worstGo = SafeGet(plotWorstGOs, i);
            if (worstGo) worstGo.SetActive(true);
            else Debug.LogWarning($"[FarmingSystem] plotWorstGOs[{i}] ���Ҵ� ? 'FarmTile-plot {i + 1}' ���� ����");
        }

        // ���������������������������������� ��ƿ ����������������������������������
        void Set(PlotVisual[] arr, int i, bool on)
        {
            if (arr == null || i < 0 || i >= arr.Length) return;
            var v = arr[i]?.visual;
            if (v && v.activeSelf != on) v.SetActive(on);
        }
        void Set(GameObject[] arr, int i, bool on)
        {
            var go = SafeGet(arr, i);
            if (go && go.activeSelf != on) go.SetActive(on);
        }
        GameObject SafeGet(GameObject[] arr, int i)
        {
            if (arr == null || i < 0 || i >= arr.Length) return null;
            return arr[i];
        }

        WeatherType GetWorst(WeatherType preferred)
        {
            if (worstLookup != null && worstLookup.TryGetValue(preferred, out var w)) return w;
            switch (preferred)
            {
                case WeatherType.Heat: return WeatherType.Cloud;
                case WeatherType.Rain: return WeatherType.Snow;
                case WeatherType.Snow: return WeatherType.Rain;
                case WeatherType.Cloud: return WeatherType.Heat;
                default: return WeatherType.Cloud;
            }
        }

        /// <summary>
        /// �÷��̾� �浹 ������ ����: ���޵� GO�� '�۹�(���� Ÿ��)'���� ����.
        /// ����(FarmTile-seed)�� plotWorst�� ����.
        /// </summary>
        public bool IsCropObject(GameObject go)
        {
            if (!go) return false;
            for (int i = 0; i < 4; i++)
            {
                if (go == SafeGet(heatGOs, i)) return true;
                if (go == SafeGet(rainGOs, i)) return true;
                if (go == SafeGet(snowGOs, i)) return true;
                if (go == SafeGet(cloudGOs, i)) return true;
            }
            return false;
        }

        // ���������������������������������� Collider ���� ����������������������������������
        void EnsureTriggerCollider(GameObject go)
        {
            if (!go) return;

            // �̹� �ݶ��̴� ������ Trigger�� ��ȯ��
            var col = go.GetComponent<Collider2D>();
            if (col)
            {
                col.isTrigger = true;
                return;
            }

            // ������ �ڽ� �ݶ��̴� �߰�
            var bc = go.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;

            // �뷫���� ������ ����(������)
            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                // BoxCollider2D.size�� ���� ����, bounds.size�� ���� ����������
                // ������ 1 ���ؿ��� ����� �ٻ�ġ�� ����
                bc.size = sr.bounds.size;
            }
        }
    }
}
