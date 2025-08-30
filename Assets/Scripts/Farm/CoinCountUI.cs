// Assets/Scripts/Farm/CoinCountUI.cs
using UnityEngine;
using TMPro;
using Game.Core;   // Economy

namespace Game.UI
{
    // ─────────────────────────────────────────────────────────────
    // File    : CoinCountUI.cs
    // Purpose : Economy.money를 TextMeshProUGUI("CoinCount")에 표시/갱신.
    // Notes   : 1) 인스펙터에서 Economy/TMP 할당 or 자동 배선(GameRoot → Find)
    //           2) 이벤트 없이도 값 변할 때만 업데이트(폴링)하므로 가볍게 동작.
    // ─────────────────────────────────────────────────────────────
    [DisallowMultipleComponent]
    public class CoinCountUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] TextMeshProUGUI label; // 이 스크립트를 CoinCount TMP 오브젝트에 붙이면 자동으로 채워짐
        [SerializeField] Economy economy;       // 비우면 자동 배선(GameRoot → Find)

        [Header("Format")]
        [SerializeField] string format = "₩ {0}";

        int lastShown = int.MinValue;

        void Awake()
        {
            if (!label)
                label = GetComponent<TextMeshProUGUI>();

            if (!economy)
            {
                // GameRoot 우선 → 씬에서 검색
                if (GameRoot.Instance && GameRoot.Instance.economy)
                    economy = GameRoot.Instance.economy;
                else
                    economy = FindObjectOfType<Economy>(includeInactive: true);
            }
        }

        void OnEnable()
        {
            Refresh(); // 활성화 시 즉시 동기화
        }

        void Update()
        {
            if (!label || !economy) return;

            // money가 바뀐 경우에만 갱신
            if (economy.money != lastShown)
                Set(economy.money);
        }

        public void Refresh()
        {
            if (!label || !economy) return;
            Set(economy.money);
        }

        void Set(int amount)
        {
            lastShown = amount;
            label.text = string.Format(format, amount);
        }
    }
}
