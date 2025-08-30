using UnityEngine;
using UnityEngine.UI;     // ★ UI Image
using Game.Data;

// ─────────────────────────────────────────────────────────────
// File    : WeatherSystem.cs
// Namespace : Game.Systems
// Purpose : (진짜 날씨와 예보 분리)
//   - 내일·모레 등 '진짜 날씨(truth)'를 미리 확정해 보관
//   - 예보는 정해진 확률(거짓말 확률)로 진실/거짓 섞어 제공
//   - 하루가 시작될 때 Today를 truth에서 확정하고 큐를 전진
//   - ★ ResolveTodayFromForecastAndAdvance() 끝에서 UI 이미지 자동 갱신
// API     : GetForecast(daysAhead) → UI용 예보(0=내일,1=모레)
//           ResolveTodayFromForecastAndAdvance() → 오늘 확정+큐 전진(+UI 갱신)
// Notes   : lieProbability=0.2 → 예보가 20% 확률로 거짓말.
// ─────────────────────────────────────────────────────────────
namespace Game.Systems
{
    public class WeatherSystem : MonoBehaviour
    {
        // 예보(확률) 한 칸
        [System.Serializable]
        public struct Forecast
        {
            [Range(0, 1)] public float heat;
            [Range(0, 1)] public float rain;
            [Range(0, 1)] public float snow;
            [Range(0, 1)] public float cloud;

            public void Normalize()
            {
                float s = heat + rain + snow + cloud;
                if (s <= 0f) { heat = 1f; rain = snow = cloud = 0f; return; }
                heat /= s; rain /= s; snow /= s; cloud /= s;
            }
        }

        [Header("State")]
        public WeatherType Today { get; private set; }

        [Header("Truth & Forecast Settings")]
        [Tooltip("UI에 보여줄 예보 일수(예: 2면 내일/모레)")]
        [Min(1)] public int forecastHorizonDays = 2;

        [Tooltip("예보가 거짓말할 확률(예: 0.2 = 20%)")]
        [Range(0f, 1f)] public float lieProbability = 0.2f;

        [Tooltip("진짜 날씨(Truth) 생성용 기후 분포")]
        public Forecast climatology = new Forecast { heat = 0.25f, rain = 0.25f, snow = 0.25f, cloud = 0.25f };

        // [0]=내일, [1]=모레, ...
        [SerializeField] private WeatherType[] truthQueue;    // 진짜 날씨 큐
        [SerializeField] private Forecast[] forecastQueue; // 예보 큐(UI 표시용)

        // ─────────────────────────────────────────────
        [Header("Forecast UI (Drag & Drop)")]
        [Tooltip("내일 예보 아이콘을 표시할 Image")]
        public Image imgTomorrow;
        [Tooltip("모레 예보 아이콘을 표시할 Image")]
        public Image imgDayAfter;

        [Tooltip("날씨별 아이콘 스프라이트")]
        public Sprite heatSprite;
        public Sprite rainSprite;
        public Sprite snowSprite;
        public Sprite cloudSprite;

        [Tooltip("스프라이트 적용 후 SetNativeSize 호출 여부")]
        public bool setNativeSize = false;
        // ─────────────────────────────────────────────

        void Awake()
        {
            InitQueues();
            // 시작 시 한 번 UI 표시
            UpdateForecastImages();
        }

        void InitQueues()
        {
            if (forecastHorizonDays < 1) forecastHorizonDays = 1;

            truthQueue = new WeatherType[forecastHorizonDays];
            forecastQueue = new Forecast[forecastHorizonDays];

            RollTruthHorizon();          // 내일/모레 등 '진짜' 먼저 확정
            RebuildForecastFromTruth();  // 그걸 기반으로 예보 생성(거짓말 확률 반영)
        }

        /// <summary>내일~모레(=horizon)까지 '진짜 날씨'를 새로 뽑는다.</summary>
        void RollTruthHorizon()
        {
            for (int i = 0; i < truthQueue.Length; i++)
                truthQueue[i] = RollTruthOneDay();
        }

        /// <summary>truthQueue를 기준으로 예보 큐를 다시 만든다.</summary>
        void RebuildForecastFromTruth()
        {
            for (int i = 0; i < forecastQueue.Length; i++)
                forecastQueue[i] = MakeForecastFromTruth(truthQueue[i]);
        }

        /// <summary>UI: (=내일) 0, (=모레) 1의 예보(확률)를 반환</summary>
        public Forecast GetForecast(int daysAhead)
        {
            daysAhead = Mathf.Clamp(daysAhead, 0, forecastQueue.Length - 1);
            return forecastQueue[daysAhead];
        }

        /// <summary>
        /// 하루 시작 시 호출:
        /// 1) Today ← truthQueue[0]로 확정
        /// 2) truth/forecast 두 큐를 한 칸 전진하고 맨 뒤를 새로 채움
        /// 3) ★ UI 이미지 갱신 (RunManager.NextDay()에서 이 함수를 호출하면 자동 반영)
        /// </summary>
        public WeatherType ResolveTodayFromForecastAndAdvance()
        {
            // 1) 오늘 확정 (이미 '내일'로 확정해 둔 진짜 값)
            Today = truthQueue[0];

            // 2) 큐 전진
            ShiftQueues();

            // 3) UI 갱신
            UpdateForecastImages();

            return Today;
        }

        void ShiftQueues()
        {
            int n = truthQueue.Length;
            for (int i = 0; i < n - 1; i++)
            {
                truthQueue[i] = truthQueue[i + 1];
                forecastQueue[i] = forecastQueue[i + 1];
            }

            // 새 '진짜' 생성 및 해당 예보 생성
            var newTruth = RollTruthOneDay();
            truthQueue[n - 1] = newTruth;
            forecastQueue[n - 1] = MakeForecastFromTruth(newTruth);
        }

        // ─────────────────────────────────────────
        // 진짜 날씨/예보 생성 로직
        // ─────────────────────────────────────────

        /// <summary>기후 분포를 기반으로 '진짜 날씨' 1일치를 샘플</summary>
        WeatherType RollTruthOneDay()
        {
            var baseDist = climatology;
            baseDist.Normalize();
            return Sample(baseDist);
        }

        /// <summary>거짓말 확률에 따라 truth를 맞추거나(진실) 틀리게(거짓) 예보를 만든다.</summary>
        Forecast MakeForecastFromTruth(WeatherType truth)
        {
            bool lie = Random.value < lieProbability;
            WeatherType predicted = lie ? RandomOtherType(truth) : truth;

            // 심플: 1-hot 예보
            return OneHot(predicted);

            // 필요 시, 살짝 퍼진 분포로:
            // return Peaked(predicted, 0.85f);
        }

        // ─────────────────────────────────────────
        // UI 업데이트
        // ─────────────────────────────────────────
        /// <summary>내일/모레 예보를 읽어 아이콘 이미지를 갱신</summary>
        public void UpdateForecastImages()
        {
            // 이미지/스프라이트 할당 안 되어 있으면 조용히 스킵
            if (!imgTomorrow && !imgDayAfter) return;

            // 내일 / 모레 예보
            var f0 = GetForecast(0);
            var f1 = (forecastHorizonDays >= 2) ? GetForecast(1) : f0; // 모레가 없으면 내일 복사

            // 아이콘 적용
            if (imgTomorrow) Apply(imgTomorrow, SpriteFor(ForecastMode(f0)));
            if (imgDayAfter) Apply(imgDayAfter, SpriteFor(ForecastMode(f1)));
        }

        void Apply(Image img, Sprite spr)
        {
            if (!img || !spr) return;
            img.sprite = spr;
            if (setNativeSize) img.SetNativeSize();
        }

        // 확률 중 최대(모드)를 WeatherType으로
        WeatherType ForecastMode(Forecast f)
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

        Sprite SpriteFor(WeatherType t)
        {
            switch (t)
            {
                case WeatherType.Heat: return heatSprite;
                case WeatherType.Rain: return rainSprite;
                case WeatherType.Snow: return snowSprite;
                default: return cloudSprite;
            }
        }

        // ─────────────────────────────────────────
        // 유틸리티
        // ─────────────────────────────────────────

        Forecast OneHot(WeatherType type)
        {
            Forecast f = default;
            switch (type)
            {
                case WeatherType.Heat: f.heat = 1f; break;
                case WeatherType.Rain: f.rain = 1f; break;
                case WeatherType.Snow: f.snow = 1f; break;
                default: f.cloud = 1f; break;
            }
            return f;
        }

        Forecast Peaked(WeatherType type, float peak = 0.85f)
        {
            peak = Mathf.Clamp01(peak);
            float rest = (1f - peak) / 3f;
            Forecast f = new Forecast { heat = rest, rain = rest, snow = rest, cloud = rest };
            switch (type)
            {
                case WeatherType.Heat: f.heat = peak; break;
                case WeatherType.Rain: f.rain = peak; break;
                case WeatherType.Snow: f.snow = peak; break;
                default: f.cloud = peak; break;
            }
            f.Normalize();
            return f;
        }

        WeatherType RandomOtherType(WeatherType truth)
        {
            int t = TypeToIndex(truth);
            int[] pool = new int[3];
            int k = 0;
            for (int i = 0; i < 4; i++)
                if (i != t) pool[k++] = i;

            int pick = pool[Random.Range(0, 3)];
            return IndexToType(pick);
        }

        static WeatherType Sample(Forecast f)
        {
            float r = Random.value;
            float acc = 0f;

            acc += f.heat; if (r <= acc) return WeatherType.Heat;
            acc += f.rain; if (r <= acc) return WeatherType.Rain;
            acc += f.snow; if (r <= acc) return WeatherType.Snow;
            return WeatherType.Cloud;
        }

        static int TypeToIndex(WeatherType t)
        {
            switch (t)
            {
                case WeatherType.Heat: return 0;
                case WeatherType.Rain: return 1;
                case WeatherType.Snow: return 2;
                default: return 3;
            }
        }

        static WeatherType IndexToType(int idx)
        {
            switch (idx)
            {
                case 0: return WeatherType.Heat;
                case 1: return WeatherType.Rain;
                case 2: return WeatherType.Snow;
                default: return WeatherType.Cloud;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("DEBUG/Rebuild Horizon (Truth+Forecast)")]
        void Debug_RebuildHorizon()
        {
            RollTruthHorizon();
            RebuildForecastFromTruth();
            UpdateForecastImages();
            UnityEngine.Debug.Log("[WeatherSystem] Rebuilt truth & forecast horizon + UI updated.");
        }
#endif
    }
}
