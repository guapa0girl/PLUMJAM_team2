using UnityEngine;
using TMPro;

namespace Game.Core
{
    public class Economy : MonoBehaviour
    {
        [Header("State")]
        public int money;  // 여기에만 누적

        [Header("Type Payouts (개당 지급액)")]
        public int heatPayout = 1;
        public int rainPayout = 3;
        public int snowPayout = 4;
        public int cloudPayout = 2;

        [Header("UI")]
        [Tooltip("숫자만 표시할 TextMeshProUGUI를 드래그하세요")]
        public TextMeshProUGUI moneyTMP;

        // 내부 캐시: 값이 바뀌었을 때만 갱신
        int _lastShown = int.MinValue;

        void Awake() { RefreshText(true); }
        void OnEnable() { RefreshText(true); }

        // 누가 직접 money 값을 바꿔도 반영되도록 보수적으로 체크
        void Update()
        {
            if (money != _lastShown) RefreshText();
        }

        // ── 돈 입출/정산 ───────────────────────────
        public void AddMoney(int v)
        {
            if (v <= 0) return;
            money += v;
            RefreshText();
        }

        public bool TrySpend(int v)
        {
            if (v < 0) return false;
            if (money < v) return false;
            money -= v;
            RefreshText();
            return true;
        }

        /// <summary>타입별 개수(heat/rain/snow/cloud)로 정산하여 money에 누적</summary>
        public int PayForCounts(int heatCount, int rainCount, int snowCount, int cloudCount)
        {
            int h = Mathf.Max(0, heatCount);
            int r = Mathf.Max(0, rainCount);
            int s = Mathf.Max(0, snowCount);
            int c = Mathf.Max(0, cloudCount);

            int total =
                h * Mathf.Max(0, heatPayout) +
                r * Mathf.Max(0, rainPayout) +
                s * Mathf.Max(0, snowPayout) +
                c * Mathf.Max(0, cloudPayout);

            if (total > 0)
            {
                money += total;
                RefreshText();
            }
            return total;
        }

        // ── 내부: 숫자만 딱 출력 ───────────────────
        void RefreshText(bool force = false)
        {
            if (!moneyTMP) return;
            if (!force && money == _lastShown) return;

            moneyTMP.text = money.ToString(); // 숫자만!
            _lastShown = money;
        }
    }
}
