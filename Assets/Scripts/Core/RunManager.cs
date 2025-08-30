using UnityEngine;
// ��������������������������������������������������������������������������������������������������������������������������
// File: RunManager.cs
// Purpose : 4�� ���� ��ü ����(�� ����, ������Ȯ��, ���� 2ȸ, ��/����/���ӿ���).
// Defines : class RunManager : MonoBehaviour
// Fields  : day(1~28), weekTribute(�� ���� ��), Weather/Combat/Farm/Economy ����
// API     : StartRun()  �� ����(����/�� �� horizon) �ʱ� �Ѹ�
//           NextDay()   �� ���� Ȯ��(���� ���)���� ������Ʈ������/������󳳡濹�� ť �ڵ� ����
// Used By : UI/���� ���� ����(��ư/�ڵ� ���� Ʈ����).
// Notes   : day%7==0 �� ���� �� GameOver, day>28 �� Ending ó��.
//           UI�� weather.GetForecast(0/1)�� ����/�� ������ ǥ��.
// ��������������������������������������������������������������������������������������������������������������������������
using System.Collections;
using Game.Core;
using Game.Systems;           // WeatherSystem
using Game.Systems.Combat;    // CombatSystem
using Game.Data;

namespace Game.Core
{
    public class RunManager : MonoBehaviour
    {
        [Header("Systems")]
        public WeatherSystem weather;
        public CombatSystem combat;
        public FarmingSystem farm;
        public Economy economy;
        public Inventory inventory;

        [Header("Run Config")]
        public int day = 1;            // 1 ~ 28
        public int weekTribute = 500;  // �� ���� �󳳱�

        public void StartRun()
        {
            // ����(����/�� �� horizon) �ʱ�ȭ
            // WeatherSystem.Awake���� �̹� ä���ٸ� ���� ����������, ��������� ȣ���� �д�.
            weather.RollForecastHorizon();

            // UI: ����/�� ������ ȭ�鿡 �����ϰ� ������ ���⼭ �о� �ݿ�
            // var fTomorrow = weather.GetForecast(0);
            // var fDayAfter = weather.GetForecast(1);
            // TODO: UI ������Ʈ
        }

        public IEnumerator NextDay()
        {
            // 1) "���� ����"�� ������� ���� ���� Ȯ�� + ���� ť ����(�� ����)
            var today = weather.ResolveTodayFromForecastAndAdvance();

            // 2) ��� ������Ʈ(��ȣ ������ ����, �ƴϸ� ���)
            farm.OnNewDay(today);

            // 3) ������ �Ϸ� 2���� ���
            //    UI���� �� ���� �� yield return combat.PlayOneClear(room, inventory, dropMult);
            //    �� ������ ���� ����(CombatSystem ���� ����)

            // 4) ���Ϳ��� �Ǹ�/���׷��̵�
            // farm.HarvestAndSell(economy);
            // skillSystem.TryUpgrade(...);

            // 5) �ָ� ��
            if (day % 7 == 0)
            {
                if (!economy.TrySpend(weekTribute))
                {
                    GameOver();
                    yield break;
                }
            }

            // 6) ��¥ ����/����
            day++;
            if (day > 28)
            {
                Ending();
                yield break;
            }

            // 7) UI: ������ ����(���ο� ����/��)�� �ٽ� ����
            // var fTomorrow = weather.GetForecast(0);
            // var fDayAfter = weather.GetForecast(1);
            // TODO: UI ������Ʈ

            yield return null;
        }

        void GameOver() { /* �� ��ȯ/�г� ���� */ }
        void Ending() { /* ���� ó�� */ }
    }
}
