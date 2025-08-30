using UnityEngine;
using Game.Data;

// ─────────────────────────────────────────────────────────────
// File    : WeatherSystem.cs
// Namespace : (예) Game.Systems
// Purpose : N일치 예보 생성/보관(내일·모레 표시), 예보 신뢰도(틀릴 확률) 반영,
//           하루가 시작될 때 실제 날씨를 확정하고 예보를 굴리는 시스템.
// Defines : class WeatherSystem : MonoBehaviour
// Fields  : Today(오늘 확정 날씨), forecastHorizonDays(보여줄 예보 일수),
//           forecastReliability(예보 신뢰도 0~1), forecasts[] (예보 큐)
// API     : RollForecastHorizon(), GetForecast(daysAhead),
//           ResolveTodayFromForecastAndAdvance()
// Notes   : 신뢰도 r: 1이면 예보 그대로, 0이면 균일(혹은 기후분포)로 섞여 오차↑.
// ─────────────────────────────────────────────────────────────
namespace Game.Systems
{
    public class WeatherSystem : MonoBehaviour
    {
        // 예보 1칸(Heat/Rain/Snow/Cloud 확률 합=1을 권장)
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

        [Header("Forecast Settings")]
        [Tooltip("UI에 보여줄 예보 일수(예: 2면 내일/모레)")]
        [Min(1)] public int forecastHorizonDays = 2; // 요구: 내일/모레
        [Tooltip("예보 신뢰도(1=예보 그대로, 0=균일 분포에 가까움)")]
        [Range(0f, 1f)] public float forecastReliability = 0.8f;

        [Tooltip("클라이메이트(기본 분포). 0 신뢰도일 때 이 분포 쪽으로 수렴. 비우면 균일 사용")]
        public Forecast climatology = new Forecast { heat = 0.25f, rain = 0.25f, snow = 0.25f, cloud = 0.25f };

        // 예보 큐: [0]=내일, [1]=모레, ...
        [SerializeField] private Forecast[] forecasts;

        void Awake()
        {
            // 최초 초기화
            if (forecasts == null || forecasts.Length != forecastHorizonDays)
                forecasts = new Forecast[forecastHorizonDays];

            RollForecastHorizon();
        }

        /// <summary>내일~모레(=horizon)까지 새 예보를 굴린다.</summary>
        public void RollForecastHorizon()
        {
            for (int i = 0; i < forecasts.Length; i++)
                forecasts[i] = RollOneForecast();
        }

        /// <summary>예: Editor/디버그: (=내일) 0, (=모레) 1 반환</summary>
        public Forecast GetForecast(int daysAhead)
        {
            daysAhead = Mathf.Clamp(daysAhead, 0, forecasts.Length - 1);
            return forecasts[daysAhead];
        }

        /// <summary>
        /// 하루가 시작될 때 호출:
        /// 1) 내일 예보(forecasts[0])를 신뢰도와 혼합해 오늘 실제 날씨 확정
        /// 2) 예보 큐를 앞으로 당기고 맨 뒤에 새 예보 추가(모레 계속 유지)
        /// </summary>
        public WeatherType ResolveTodayFromForecastAndAdvance() //and advance
        {
            // 1) 예보 확정
            Forecast f = forecasts[0]; f.Normalize();

            // 신뢰도 r: f' = Lerp(Uniform/Climatology, f, r)
            Forecast baseDist = climatology;
            baseDist.Normalize();
            Forecast mixed = Lerp(baseDist, f, forecastReliability);
            mixed.Normalize();

            Today = Sample(mixed);

            // 2) 예보 큐 굴리기 (내일→오늘 소모, 새 모레를 뒤에 추가)
            ShiftAndAppend();

            return Today;
        }

        /// <summary>개별 예보 한 칸 생성(원하면 날씨별 규칙/시즌 반영)</summary>
        private Forecast RollOneForecast()
        {
            // TODO: 시즌/룸/이벤트에 따른 확률 생성 규칙을 넣어도 됨
            var f = new Forecast { heat = 0.4f, rain = 0.2f, snow = 0.1f, cloud = 0.3f };
            f.Normalize();
            return f;
        }

        private void ShiftAndAppend()
        {
            for (int i = 0; i < forecasts.Length - 1; i++)
                forecasts[i] = forecasts[i + 1];
            forecasts[forecasts.Length - 1] = RollOneForecast();
        }

        private static Forecast Lerp(Forecast a, Forecast b, float t)
        {
            return new Forecast
            {
                heat = Mathf.Lerp(a.heat, b.heat, t),
                rain = Mathf.Lerp(a.rain, b.rain, t),
                snow = Mathf.Lerp(a.snow, b.snow, t),
                cloud = Mathf.Lerp(a.cloud, b.cloud, t)
            };
        }

        private static WeatherType Sample(Forecast f)
        {
            float r = Random.value; // [0,1)
            float acc = 0f;

            acc += f.heat; if (r <= acc) return WeatherType.Heat;
            acc += f.rain; if (r <= acc) return WeatherType.Rain;
            acc += f.snow; if (r <= acc) return WeatherType.Snow;
            // 남은 확률은 Cloud
            return WeatherType.Cloud;
        }
    }
}
