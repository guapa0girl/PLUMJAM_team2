using UnityEngine;
using Game.Data;

// ��������������������������������������������������������������������������������������������������������������������������
// File    : WeatherSystem.cs
// Namespace : (��) Game.Systems
// Purpose : N��ġ ���� ����/����(���ϡ��� ǥ��), ���� �ŷڵ�(Ʋ�� Ȯ��) �ݿ�,
//           �Ϸ簡 ���۵� �� ���� ������ Ȯ���ϰ� ������ ������ �ý���.
// Defines : class WeatherSystem : MonoBehaviour
// Fields  : Today(���� Ȯ�� ����), forecastHorizonDays(������ ���� �ϼ�),
//           forecastReliability(���� �ŷڵ� 0~1), forecasts[] (���� ť)
// API     : RollForecastHorizon(), GetForecast(daysAhead),
//           ResolveTodayFromForecastAndAdvance()
// Notes   : �ŷڵ� r: 1�̸� ���� �״��, 0�̸� ����(Ȥ�� ���ĺ���)�� ���� ������.
// ��������������������������������������������������������������������������������������������������������������������������
namespace Game.Systems
{
    public class WeatherSystem : MonoBehaviour
    {
        // ���� 1ĭ(Heat/Rain/Snow/Cloud Ȯ�� ��=1�� ����)
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
        [Tooltip("UI�� ������ ���� �ϼ�(��: 2�� ����/��)")]
        [Min(1)] public int forecastHorizonDays = 2; // �䱸: ����/��
        [Tooltip("���� �ŷڵ�(1=���� �״��, 0=���� ������ �����)")]
        [Range(0f, 1f)] public float forecastReliability = 0.8f;

        [Tooltip("Ŭ���̸���Ʈ(�⺻ ����). 0 �ŷڵ��� �� �� ���� ������ ����. ���� ���� ���")]
        public Forecast climatology = new Forecast { heat = 0.25f, rain = 0.25f, snow = 0.25f, cloud = 0.25f };

        // ���� ť: [0]=����, [1]=��, ...
        [SerializeField] private Forecast[] forecasts;

        void Awake()
        {
            // ���� �ʱ�ȭ
            if (forecasts == null || forecasts.Length != forecastHorizonDays)
                forecasts = new Forecast[forecastHorizonDays];

            RollForecastHorizon();
        }

        /// <summary>����~��(=horizon)���� �� ������ ������.</summary>
        public void RollForecastHorizon()
        {
            for (int i = 0; i < forecasts.Length; i++)
                forecasts[i] = RollOneForecast();
        }

        /// <summary>��: Editor/�����: (=����) 0, (=��) 1 ��ȯ</summary>
        public Forecast GetForecast(int daysAhead)
        {
            daysAhead = Mathf.Clamp(daysAhead, 0, forecasts.Length - 1);
            return forecasts[daysAhead];
        }

        /// <summary>
        /// �Ϸ簡 ���۵� �� ȣ��:
        /// 1) ���� ����(forecasts[0])�� �ŷڵ��� ȥ���� ���� ���� ���� Ȯ��
        /// 2) ���� ť�� ������ ���� �� �ڿ� �� ���� �߰�(�� ��� ����)
        /// </summary>
        public WeatherType ResolveTodayFromForecastAndAdvance() //and advance
        {
            // 1) ���� Ȯ��
            Forecast f = forecasts[0]; f.Normalize();

            // �ŷڵ� r: f' = Lerp(Uniform/Climatology, f, r)
            Forecast baseDist = climatology;
            baseDist.Normalize();
            Forecast mixed = Lerp(baseDist, f, forecastReliability);
            mixed.Normalize();

            Today = Sample(mixed);

            // 2) ���� ť ������ (���ϡ���� �Ҹ�, �� �𷹸� �ڿ� �߰�)
            ShiftAndAppend();

            return Today;
        }

        /// <summary>���� ���� �� ĭ ����(���ϸ� ������ ��Ģ/���� �ݿ�)</summary>
        private Forecast RollOneForecast()
        {
            // TODO: ����/��/�̺�Ʈ�� ���� Ȯ�� ���� ��Ģ�� �־ ��
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
            // ���� Ȯ���� Cloud
            return WeatherType.Cloud;
        }
    }
}
