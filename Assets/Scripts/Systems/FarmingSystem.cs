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
            [Tooltip("기본/씨앗 표시용(플롯별 1개): FarmTile-seed #")]
            public GameObject visual;
        }

        [Header("Plots (고정 4칸)")]
        public List<Plot> plots = new List<Plot>(4) { new Plot(), new Plot(), new Plot(), new Plot() };

        [Header("Plots Visuals (기본/씨앗)")]
        public PlotVisual[] plotVisuals = new PlotVisual[4];

        // 날씨 전용 오브젝트 16개 (4종 × 4플롯)
        [Header("Per-Plot Weather GOs (총 16개)")]
        [Tooltip("각 플롯의 Heat 오브젝트 4개 (FarmTile-heat 1..4)")]
        public GameObject[] heatGOs = new GameObject[4];
        [Tooltip("각 플롯의 Rain 오브젝트 4개 (FarmTile-rain 1..4)")]
        public GameObject[] rainGOs = new GameObject[4];
        [Tooltip("각 플롯의 Snow 오브젝트 4개 (FarmTile-snow 1..4)")]
        public GameObject[] snowGOs = new GameObject[4];
        [Tooltip("각 플롯의 Cloud 오브젝트 4개 (FarmTile-cloud 1..4)")]
        public GameObject[] cloudGOs = new GameObject[4];

        // 최악 날씨 표기 ? FarmTile-plot 1..4
        [Header("Worst tiles (각 플롯의 FarmTile-plot 1..4)")]
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
        [Tooltip("게임 시작 시 네 플롯을 전부 'plot' 표시 + 심기 가능(Empty) 상태로 초기화")]
        public bool showPlotsOnStart = true;

        [Header("Pickup")]
        [Tooltip("작물 GO(heat/rain/snow/cloud)에 Trigger Collider가 없으면 자동으로 추가")]
        public bool autoAddPickupCollider = true;

        Dictionary<WeatherType, WeatherType> worstLookup;

        void Awake()
        {
            worstLookup = new Dictionary<WeatherType, WeatherType>();
            foreach (var e in worstMap) worstLookup[e.forPreferred] = e.worst;

            // 일단 모두 끄기
            for (int i = 0; i < 4; i++)
            {
                Set(plotVisuals, i, false);
                Set(heatGOs, i, false);
                Set(rainGOs, i, false);
                Set(snowGOs, i, false);
                Set(cloudGOs, i, false);
                Set(plotWorstGOs, i, false);
            }

            // 작물 GO들에 Trigger Collider 자동 보장
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
                EnsurePlantable(i); // 상태 Empty + plotWorst ON
            }
        }

        // ─────────────────────────────────────────────
        // 심기
        // ─────────────────────────────────────────────
        public bool TryPlant(Inventory inv, SeedDef seed, int plotIndex)
        {
            if (seed == null || plotIndex < 0 || plotIndex >= plots.Count) return false;
            var p = plots[plotIndex];
            if (p.state != PlotState.Empty) return false;               // Empty만 심기 가능
            if (inv == null || !inv.TryConsume(seed, seedsPerBlock)) return false;

            p.seed = seed;
            p.state = PlotState.Planted;
            plots[plotIndex] = p;

            ShowSeedOnly(plotIndex); // 씨앗만 표시
            return true;
        }

        // ─────────────────────────────────────────────
        // 하루 진행(오늘 실제 날씨)
        // ─────────────────────────────────────────────
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
                    // ? 성숙: 상태 전환 + 실제 작물 GO를 반드시 켜서 '충돌 가능' 상태로 만든다
                    p.state = PlotState.Mature;
                    ShowWeather(i, preferred);
                }
                else if (today == worst)
                {
                    // ? 최악: 무조건 다시 심기 가능 상태로 전환 + plot만 표시
                    EnsurePlantable(i);
                    p = plots[i]; // 동기화
                }

                plots[i] = p;
            }
        }

        // ─────────────────────────────────────────────
        // 수확 → 타입별 개수 집계 → Economy에 전담 지급 → 다시 심기 가능 보장
        // ─────────────────────────────────────────────
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

                    // ? 수확 후: 무조건 다시 심기 가능 상태 + plot 표시
                    EnsurePlantable(i);
                }
            }

            if (econ == null) return 0;
            int earned = econ.PayForCounts(heat, rain, snow, cloud); // Economy가 money에 직접 누적
            return earned; // UI/로그용
        }

        // ─────────────────────────────────────────────
        // (큐 전진 전) 내일 예보 프리뷰
        // ─────────────────────────────────────────────
        /// Empty → plot 유지(씨앗 되살아남 방지)
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

        // ─────────────────────────────────────────────
        // ? 심기 가능 보장 함수 (요구사항 핵심)
        // ─────────────────────────────────────────────
        void EnsurePlantable(int i)
        {
            if (i < 0 || i >= plots.Count) return;
            var p = plots[i];
            p.state = PlotState.Empty;
            p.seed = null;
            plots[i] = p;

            ShowWorst(i); // 화면엔 plot만 남김
        }

        public bool IsPlantable(int i)
        {
            return (i >= 0 && i < plots.Count && plots[i].state == PlotState.Empty);
        }

        // ───────────────── 표시 제어(플롯 인덱스별) ─────────────────
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
            Set(plotVisuals, i, true);   // 씨앗 on
        }

        void ShowWeather(int i, WeatherType wt)
        {
            Set(plotVisuals, i, false);  // 씨앗 off
            Set(plotWorstGOs, i, false);
            Set(heatGOs, i, wt == WeatherType.Heat);
            Set(rainGOs, i, wt == WeatherType.Rain);
            Set(snowGOs, i, wt == WeatherType.Snow);
            Set(cloudGOs, i, wt == WeatherType.Cloud);
        }

        void ShowWorst(int i)
        {
            // 씨앗/날씨 전부 off, plotWorst on (없으면 경고)
            Set(plotVisuals, i, false);
            Set(heatGOs, i, false);
            Set(rainGOs, i, false);
            Set(snowGOs, i, false);
            Set(cloudGOs, i, false);
            var worstGo = SafeGet(plotWorstGOs, i);
            if (worstGo) worstGo.SetActive(true);
            else Debug.LogWarning($"[FarmingSystem] plotWorstGOs[{i}] 미할당 ? 'FarmTile-plot {i + 1}' 연결 권장");
        }

        // ───────────────── 유틸 ─────────────────
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
        /// 플레이어 충돌 판정을 위해: 전달된 GO가 '작물(날씨 타일)'인지 여부.
        /// 씨앗(FarmTile-seed)과 plotWorst는 제외.
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

        // ───────────────── Collider 보장 ─────────────────
        void EnsureTriggerCollider(GameObject go)
        {
            if (!go) return;

            // 이미 콜라이더 있으면 Trigger로 전환만
            var col = go.GetComponent<Collider2D>();
            if (col)
            {
                col.isTrigger = true;
                return;
            }

            // 없으면 박스 콜라이더 추가
            var bc = go.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;

            // 대략적인 사이즈 보정(있으면)
            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                // BoxCollider2D.size는 로컬 단위, bounds.size는 월드 단위이지만
                // 스케일 1 기준에선 충분히 근사치로 동작
                bc.size = sr.bounds.size;
            }
        }
    }
}
