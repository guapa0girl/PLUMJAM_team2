using UnityEngine;
using UnityEngine.UI;     // �� UI Image
using Game.Data;

// ��������������������������������������������������������������������������������������������������������������������������
// File    : WeatherSystem.cs
// Namespace : Game.Systems
// Purpose : (��¥ ������ ���� �и�)
//   - ���ϡ��� �� '��¥ ����(truth)'�� �̸� Ȯ���� ����
//   - ������ ������ Ȯ��(������ Ȯ��)�� ����/���� ���� ����
//   - �Ϸ簡 ���۵� �� Today�� truth���� Ȯ���ϰ� ť�� ����
//   - �� ResolveTodayFromForecastAndAdvance() ������ UI �̹��� �ڵ� ����
// API     : GetForecast(daysAhead) �� UI�� ����(0=����,1=��)
//           ResolveTodayFromForecastAndAdvance() �� ���� Ȯ��+ť ����(+UI ����)
// Notes   : lieProbability=0.2 �� ������ 20% Ȯ���� ������.
// ��������������������������������������������������������������������������������������������������������������������������
namespace Game.Systems
{
    public class WeatherSystem : MonoBehaviour
    {
        // ����(Ȯ��) �� ĭ
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
        [Tooltip("UI�� ������ ���� �ϼ�(��: 2�� ����/��)")]
        [Min(1)] public int forecastHorizonDays = 2;

        [Tooltip("������ �������� Ȯ��(��: 0.2 = 20%)")]
        [Range(0f, 1f)] public float lieProbability = 0.2f;

        [Tooltip("��¥ ����(Truth) ������ ���� ����")]
        public Forecast climatology = new Forecast { heat = 0.25f, rain = 0.25f, snow = 0.25f, cloud = 0.25f };

        // [0]=����, [1]=��, ...
        [SerializeField] private WeatherType[] truthQueue;    // ��¥ ���� ť
        [SerializeField] private Forecast[] forecastQueue; // ���� ť(UI ǥ�ÿ�)

        // ������������������������������������������������������������������������������������������
        [Header("Forecast UI (Drag & Drop)")]
        [Tooltip("���� ���� �������� ǥ���� Image")]
        public Image imgTomorrow;
        [Tooltip("�� ���� �������� ǥ���� Image")]
        public Image imgDayAfter;

        [Tooltip("������ ������ ��������Ʈ")]
        public Sprite heatSprite;
        public Sprite rainSprite;
        public Sprite snowSprite;
        public Sprite cloudSprite;

        [Tooltip("��������Ʈ ���� �� SetNativeSize ȣ�� ����")]
        public bool setNativeSize = false;
        // ������������������������������������������������������������������������������������������

        void Awake()
        {
            InitQueues();
            // ���� �� �� �� UI ǥ��
            UpdateForecastImages();
        }

        void InitQueues()
        {
            if (forecastHorizonDays < 1) forecastHorizonDays = 1;

            truthQueue = new WeatherType[forecastHorizonDays];
            forecastQueue = new Forecast[forecastHorizonDays];

            RollTruthHorizon();          // ����/�� �� '��¥' ���� Ȯ��
            RebuildForecastFromTruth();  // �װ� ������� ���� ����(������ Ȯ�� �ݿ�)
        }

        /// <summary>����~��(=horizon)���� '��¥ ����'�� ���� �̴´�.</summary>
        void RollTruthHorizon()
        {
            for (int i = 0; i < truthQueue.Length; i++)
                truthQueue[i] = RollTruthOneDay();
        }

        /// <summary>truthQueue�� �������� ���� ť�� �ٽ� �����.</summary>
        void RebuildForecastFromTruth()
        {
            for (int i = 0; i < forecastQueue.Length; i++)
                forecastQueue[i] = MakeForecastFromTruth(truthQueue[i]);
        }

        /// <summary>UI: (=����) 0, (=��) 1�� ����(Ȯ��)�� ��ȯ</summary>
        public Forecast GetForecast(int daysAhead)
        {
            daysAhead = Mathf.Clamp(daysAhead, 0, forecastQueue.Length - 1);
            return forecastQueue[daysAhead];
        }

        /// <summary>
        /// �Ϸ� ���� �� ȣ��:
        /// 1) Today �� truthQueue[0]�� Ȯ��
        /// 2) truth/forecast �� ť�� �� ĭ �����ϰ� �� �ڸ� ���� ä��
        /// 3) �� UI �̹��� ���� (RunManager.NextDay()���� �� �Լ��� ȣ���ϸ� �ڵ� �ݿ�)
        /// </summary>
        public WeatherType ResolveTodayFromForecastAndAdvance()
        {
            // 1) ���� Ȯ�� (�̹� '����'�� Ȯ���� �� ��¥ ��)
            Today = truthQueue[0];

            // 2) ť ����
            ShiftQueues();

            // 3) UI ����
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

            // �� '��¥' ���� �� �ش� ���� ����
            var newTruth = RollTruthOneDay();
            truthQueue[n - 1] = newTruth;
            forecastQueue[n - 1] = MakeForecastFromTruth(newTruth);
        }

        // ����������������������������������������������������������������������������������
        // ��¥ ����/���� ���� ����
        // ����������������������������������������������������������������������������������

        /// <summary>���� ������ ������� '��¥ ����' 1��ġ�� ����</summary>
        WeatherType RollTruthOneDay()
        {
            var baseDist = climatology;
            baseDist.Normalize();
            return Sample(baseDist);
        }

        /// <summary>������ Ȯ���� ���� truth�� ���߰ų�(����) Ʋ����(����) ������ �����.</summary>
        Forecast MakeForecastFromTruth(WeatherType truth)
        {
            bool lie = Random.value < lieProbability;
            WeatherType predicted = lie ? RandomOtherType(truth) : truth;

            // ����: 1-hot ����
            return OneHot(predicted);

            // �ʿ� ��, ��¦ ���� ������:
            // return Peaked(predicted, 0.85f);
        }

        // ����������������������������������������������������������������������������������
        // UI ������Ʈ
        // ����������������������������������������������������������������������������������
        /// <summary>����/�� ������ �о� ������ �̹����� ����</summary>
        public void UpdateForecastImages()
        {
            // �̹���/��������Ʈ �Ҵ� �� �Ǿ� ������ ������ ��ŵ
            if (!imgTomorrow && !imgDayAfter) return;

            // ���� / �� ����
            var f0 = GetForecast(0);
            var f1 = (forecastHorizonDays >= 2) ? GetForecast(1) : f0; // �𷹰� ������ ���� ����

            // ������ ����
            if (imgTomorrow) Apply(imgTomorrow, SpriteFor(ForecastMode(f0)));
            if (imgDayAfter) Apply(imgDayAfter, SpriteFor(ForecastMode(f1)));
        }

        void Apply(Image img, Sprite spr)
        {
            if (!img || !spr) return;
            img.sprite = spr;
            if (setNativeSize) img.SetNativeSize();
        }

        // Ȯ�� �� �ִ�(���)�� WeatherType����
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

        // ����������������������������������������������������������������������������������
        // ��ƿ��Ƽ
        // ����������������������������������������������������������������������������������

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
