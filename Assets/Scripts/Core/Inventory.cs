using UnityEngine;
using TMPro;
using Game.Data;

namespace Game.Core
{
    public class Inventory : MonoBehaviour
    {
        [Header("���� ���� (�����Ϳ��� ���� ����)")]
        public int sunSeedCount = 0;
        public int cloudSeedCount = 0;
        public int rainSeedCount = 0;
        public int snowSeedCount = 0;

        [Header("���� ���� ���� (SeedDef �巡�� ����)")]
        public SeedDef seedSun;
        public SeedDef seedCloud;
        public SeedDef seedRain;
        public SeedDef seedSnow;

        [Header("UI Texts (TextMeshProUGUI)")]
        public TextMeshProUGUI seed_sun_text;
        public TextMeshProUGUI seed_cloud_text;
        public TextMeshProUGUI seed_rain_text;
        public TextMeshProUGUI seed_snow_text;

        void Start()
        {
            UpdateUI(); // ������ �� ����
        }

        public int Count(SeedDef def)
        {
            if (def == null) return 0;
            if (def == seedSun) return sunSeedCount;
            if (def == seedCloud) return cloudSeedCount;
            if (def == seedRain) return rainSeedCount;
            if (def == seedSnow) return snowSeedCount;
            return 0;
        }

        public void AddSeed(SeedDef def, int count)
        {
            if (def == null || count <= 0) return;

            if (def == seedSun) sunSeedCount += count;
            else if (def == seedCloud) cloudSeedCount += count;
            else if (def == seedRain) rainSeedCount += count;
            else if (def == seedSnow) snowSeedCount += count;

            UpdateUI();
        }

        public bool TryConsume(SeedDef def, int count)
        {
            if (def == null || count <= 0) return false;

            if (def == seedSun && sunSeedCount >= count)
            {
                sunSeedCount -= count; UpdateUI(); return true;
            }
            if (def == seedCloud && cloudSeedCount >= count)
            {
                cloudSeedCount -= count; UpdateUI(); return true;
            }
            if (def == seedRain && rainSeedCount >= count)
            {
                rainSeedCount -= count; UpdateUI(); return true;
            }
            if (def == seedSnow && snowSeedCount >= count)
            {
                snowSeedCount -= count; UpdateUI(); return true;
            }

            return false;
        }

        void UpdateUI()
        {
            if (seed_sun_text) seed_sun_text.text = $"{sunSeedCount}";
            if (seed_cloud_text) seed_cloud_text.text = $"{cloudSeedCount}";
            if (seed_rain_text) seed_rain_text.text = $"{rainSeedCount}";
            if (seed_snow_text) seed_snow_text.text = $"{snowSeedCount}";
        }

#if UNITY_EDITOR
        // �ν����� ���� �ٲ�� �ڵ� �ݿ�
        void OnValidate()
        {
            // �÷��� ���� ���� UI ������Ʈ
            if (Application.isPlaying)
            {
                UpdateUI();
            }
        }
#endif
    }
}
