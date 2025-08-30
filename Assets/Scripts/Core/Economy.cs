using UnityEngine;
using TMPro;

namespace Game.Core
{
    public class Economy : MonoBehaviour
    {
        [Header("State")]
        public int money;  // ���⿡�� ����

        [Header("Type Payouts (���� ���޾�)")]
        public int heatPayout = 1;
        public int rainPayout = 3;
        public int snowPayout = 4;
        public int cloudPayout = 2;

        [Header("UI")]
        [Tooltip("���ڸ� ǥ���� TextMeshProUGUI�� �巡���ϼ���")]
        public TextMeshProUGUI moneyTMP;

        // ���� ĳ��: ���� �ٲ���� ���� ����
        int _lastShown = int.MinValue;

        void Awake() { RefreshText(true); }
        void OnEnable() { RefreshText(true); }

        // ���� ���� money ���� �ٲ㵵 �ݿ��ǵ��� ���������� üũ
        void Update()
        {
            if (money != _lastShown) RefreshText();
        }

        // ���� �� ����/���� ������������������������������������������������������
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

        /// <summary>Ÿ�Ժ� ����(heat/rain/snow/cloud)�� �����Ͽ� money�� ����</summary>
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

        // ���� ����: ���ڸ� �� ��� ��������������������������������������
        void RefreshText(bool force = false)
        {
            if (!moneyTMP) return;
            if (!force && money == _lastShown) return;

            moneyTMP.text = money.ToString(); // ���ڸ�!
            _lastShown = money;
        }
    }
}
