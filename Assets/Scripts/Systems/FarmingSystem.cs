using UnityEngine;
using System.Collections.Generic;
using Game.Data;
using Game.Core;

namespace Game.Systems
{
    // ─────────────────────────────────────────────────────────────
    // File    : FarmingSystem.cs
    // Purpose : 밭 4칸 운영 + 시각 표시 + 수확/판매.
    // Changes :
    //   - Economy 참조 자동 배선(GameRoot → Inspector → Find).
    //   - HarvestAndSell() 무인자 버전 추가(내부 Economy 사용).
    //   - 수확 시 시각 요소 전부 OFF 후 'plot'만 켜서 잔상 안 남게 처리.
    //   - (옵션) 픽업용으로 날씨 타일에 Trigger Collider 자동 부착.
    // ─────────────────────────────────────────────────────────────
    public class FarmingSystem : MonoBehaviour
    {
        public enum PlotState { Empty, Planted, Mature }

        [System.Serializable]
        public class Plot
        {
            public PlotState state;
            public SeedDef   seed;   // SeedDef.preferred : WeatherType
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

        // 최악 날씨 표기 : FarmTile-plot 1..4
        [Header("Worst tiles (각 플롯의 FarmTile-plot 1..4)")]
        public GameObject[] plotWorstGOs = new GameObject[4];

        [Header("Planting Rule")]
        [Min(1)] public int seedsPerBlock = 1;

        [Header("Worst Weather Mapping")]
        public WorstMapEntry[] worstMap = new WorstMapEntry[]
        {
            new WorstMapEntry{ forPreferred = WeatherType.Heat,  worst = WeatherType.Cloud },
            new WorstMapEntry{ forPreferred = WeatherType.Rain,  worst = WeatherType.Snow  },
            new WorstMapEntry{ forPreferred = WeatherType.Snow,  worst = WeatherType.Rain  },
            new WorstMapEntry{ forPreferred = WeatherType.Cloud, worst = WeatherType.Heat  },
        };
        [System.Serializable] public struct WorstMapEntry { public WeatherType forPreferred; public WeatherType worst; }

        [Header("Startup")]
        [Tooltip("게임 시작 시 네 플롯을 전부 'plot' 표시 + 심기 가능(Empty) 상태로 초기화")]
        public bool showPlotsOnStart = true;

        [Header("Money")]
        [SerializeField] Economy economy; // 비우면 Awake에서 자동 배선

        [Header("Pickup")]
        [Tooltip("날씨 GO(heat/rain/snow/cloud)에 Trigger Collider가 없으면 자동으로 추가")]
        public bool autoAddPickupCollider = true;

        Dictionary<WeatherType, WeatherType> worstLookup;

        void Awake()
        {
            // Economy 자동 배선
            if (!economy)
            {
                if (GameRoot.Instance && GameRoot.Instance.economy)
                    economy = GameRoot.Instance.economy;
                else
                    economy = FindObjectOfType<Economy>(includeInactive: true);
            }

            // 최악 날씨 룩업 테이블 구성
            worstLookup = new Dictionary<WeatherType, WeatherType>();
            foreach (var e in worstMap) worstLookup[e.forPreferred] = e.worst;

            // 초기엔 모든 시각 오브젝트 OFF
            for (int i = 0; i < 4; i++)
            {
                Set(plotVisuals, i, false);
                Set(heatGOs, i, false);
                Set(rainGOs, i, false);
                Set(snowGOs, i, false);
                Set(cloudGOs, i, false);
                Set(plotWorstGOs, i, false);
            }

            // 픽업용 Trigger Collider 자동 추가(옵션)
            if (autoAddPickupCollider)
            {
                int n = 4;
                for (int i = 0; i < n; i++)
                {
                    EnsureTriggerCollider(SafeGet(heatGOs,  i));
                    EnsureTriggerCollider(SafeGet(rainGOs,  i));
                    EnsureTriggerCollider(SafeGet(snowGOs,  i));
                    EnsureTriggerCollider(SafeGet(cloudGOs, i));
                }
            }
        }

        void Start()
        {
            if (!showPlotsOnStart) return;
            for (int i = 0; i < plots.Count && i < 4; i++)
                EnsurePlantable(i); // 상태 Empty + plot 표시
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

            p.seed  = seed;
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
                var worst     = GetWorst(preferred);

                if (today == preferred)
                {
                    // 즉시 성숙: 성숙 비주얼(해당 날씨 타일)로 전환
                    p.state = PlotState.Mature;
                    ShowWeather(i, preferred);
                }
                else if (today == worst)
                {
                    // 최악: 즉시 실패, 빈 플롯으로 초기화 + plot 표시
                    EnsurePlantable(i);
                    p = plots[i]; // 구조체 재할당 반영
                }
                // 보통: 그대로 유지(씨앗만 표시)

                plots[i] = p;
            }
        }

        // ─────────────────────────────────────────────
        // 수확/판매 (권장) : 내부 Economy 사용
        // ─────────────────────────────────────────────
        public int HarvestAndSell() => HarvestAndSellInternal(economy);

        // 하위호환: 외부 Economy를 넘기고 싶을 때
        public int HarvestAndSell(Economy econ) => HarvestAndSellInternal(econ);

        // 핵심 처리: 성숙 작물의 sellPrice 합산 → AddMoney, 그리고 확실히 비주얼 OFF
        int HarvestAndSellInternal(Economy econ)
        {
            int earned = 0;

            for (int i = 0; i < plots.Count; i++)
            {
                var p = plots[i];
                if (p.state == PlotState.Mature && p.seed != null)
                {
                    earned += Mathf.Max(0, p.seed.sellPrice);

                    // ★ 수확 후: 무조건 다시 심기 가능 상태 + plot 표시
                    //   내부적으로 ShowWorst가 씨앗/날씨 타일을 전부 꺼주기 때문에
                    //   "수확했는데 타일이 남아있는" 문제가 발생하지 않음.
                    EnsurePlantable(i);
                }
            }

            if (econ != null) econ.AddMoney(earned);
            else Debug.LogWarning("[FarmingSystem] Economy 참조 없음: GameRoot/Inspector 연결 필요");

            return earned;
        }

        // ─────────────────────────────────────────────
        // (큐 전진 전) 내일 예보 프리뷰
        // ─────────────────────────────────────────────
        public void ApplyForecastToPlots(WeatherType forecastTomorrow)
        {
            for (int i = 0; i < plots.Count; i++)
            {
                var p = plots[i];

                if (p.state == PlotState.Empty)   { ShowWorst(i);     continue; }
                if (p.state == PlotState.Mature)  { /* 그대로 */      continue; }
                if (p.seed == null)               { ShowWorst(i);     continue; }

                var preferred = p.seed.preferred;
                var worst     = GetWorst(preferred);

                if      (forecastTomorrow == preferred) ShowWeather(i, preferred);
                else if (forecastTomorrow == worst)     ShowWorst(i);
                else                                     ShowSeedOnly(i);
            }
        }

        // ─────────────────────────────────────────────
        // 심기 가능 보장(Empty로 초기화 + plot 표시)
        // ─────────────────────────────────────────────
        void EnsurePlantable(int i)
        {
            if (i < 0 || i >= plots.Count) return;
            var p = plots[i];
            p.state = PlotState.Empty;
            p.seed  = null;
            plots[i] = p;

            ShowWorst(i); // 씨앗/날씨 전부 OFF + plot on
        }

        public bool IsPlantable(int i)
            => (i >= 0 && i < plots.Count && plots[i].state == PlotState.Empty);

        // ─────────────── 표시 제어(플롯별) ───────────────
        void ShowSeedOnly(int i)
        {
            Set(plotWorstGOs, i, false);
            Set(heatGOs,  i, false);
            Set(rainGOs,  i, false);
            Set(snowGOs,  i, false);
            Set(cloudGOs, i, false);
            Set(plotVisuals, i, true);   // 씨앗 on
        }

        void ShowWeather(int i, WeatherType wt)
        {
            Set(plotVisuals,  i, false);  // 씨앗 off
            Set(plotWorstGOs, i, false);
            Set(heatGOs,  i, wt == WeatherType.Heat);
            Set(rainGOs,  i, wt == WeatherType.Rain);
            Set(snowGOs,  i, wt == WeatherType.Snow);
            Set(cloudGOs, i, wt == WeatherType.Cloud);
        }

        void ShowWorst(int i)
        {
            // 씨앗/날씨 전부 off, plotWorst on (없으면 경고)
            Set(plotVisuals,  i, false);
            Set(heatGOs,  i, false);
            Set(rainGOs,  i, false);
            Set(snowGOs,  i, false);
            Set(cloudGOs, i, false);
            var worstGo = SafeGet(plotWorstGOs, i);
            if (worstGo) worstGo.SetActive(true);
            else Debug.LogWarning($"[FarmingSystem] plotWorstGOs[{i}] 미할당 — 'FarmTile-plot {i + 1}'를 연결하세요.");
        }

        // ─────────────── 유틸 ───────────────
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
                case WeatherType.Heat:  return WeatherType.Cloud;
                case WeatherType.Rain:  return WeatherType.Snow;
                case WeatherType.Snow:  return WeatherType.Rain;
                case WeatherType.Cloud: return WeatherType.Heat;
                default:                return WeatherType.Cloud;
            }
        }

        /// <summary>전달된 GO가 '작물(날씨 타일)'인지 여부. 씨앗/plot은 제외.</summary>
        public bool IsCropObject(GameObject go)
        {
            if (!go) return false;
            for (int i = 0; i < 4; i++)
            {
                if (go == SafeGet(heatGOs,  i)) return true;
                if (go == SafeGet(rainGOs,  i)) return true;
                if (go == SafeGet(snowGOs,  i)) return true;
                if (go == SafeGet(cloudGOs, i)) return true;
            }
            return false;
        }

        // ─────────────── Collider 보강(옵션) ───────────────
        void EnsureTriggerCollider(GameObject go)
        {
            if (!go) return;

            // 이미 있는 Collider2D면 Trigger로 설정
            var col = go.GetComponent<Collider2D>();
            if (col) { col.isTrigger = true; return; }

            // 없으면 BoxCollider2D 추가
            var bc = go.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;

            // 대략적인 사이즈 맞추기(스프라이트가 있으면 bounds 기준)
            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) bc.size = sr.bounds.size;
        }
    }
}
