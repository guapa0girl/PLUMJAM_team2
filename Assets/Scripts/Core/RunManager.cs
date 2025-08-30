using UnityEngine;
// ��������������������������������������������������������������������������������������������������������������������������
// File: RunManager.cs
// Purpose : 4�� ���� ��ü ����(�� ����, ������Ȯ��, ���� 2ȸ, ��/����/���ӿ���).
// Defines : class RunManager : MonoBehaviour
// Fields  : day(1~28), weekTribute(�� ���� ��), Weather/Combat/Farm/Economy ����
// API     : StartRun()  �� ù ���� �Ѹ�
//           NextDay()   �� ���� Ȯ������ ������Ʈ������/������󳳡������ ����
// Used By : UI/���� ���� ����(��ư/�ڵ� ���� Ʈ����).
// Notes   : day%7==0 �� ���� �� GameOver, day>28 �� Ending ó��.
// ��������������������������������������������������������������������������������������������������������������������������
using System.Collections;
using Game.Core;
using Game.Systems;
using Game.Systems.Combat;
using Game.Data;

namespace Game.Core
{
    // RunManager.cs
    public class RunManager : MonoBehaviour
    {
        public WeatherSystem weather;
        public CombatSystem combat;
        public FarmingSystem farm;
        public Economy economy;

        public int day = 1;            // 1 ~ 28
        public int weekTribute = 500;  // ��: �� ���� �󳳱�
        public Inventory inventory;

        public void StartRun()
        {
            weather.RollTomorrowForecast(); // ù ȭ�鿡 ���� ����
        }

        public IEnumerator NextDay()
        {
            // ���� �� ���� Ȯ��
            var today = weather.ResolveTodayFromForecast(); // ������ ������
            farm.OnNewDay(today);

            // ������ �Ϸ� 2���� ���
            // UI���� �� ���� �� combat.PlayOneClear(room,...)

            // ���Ϳ��� �Ǹ�/���׷��̵�
            // farm.HarvestAndSell(economy);

            // �ָ� ��
            if (day % 7 == 0)
            {
                if (!economy.TrySpend(weekTribute))
                {
                    GameOver(); yield break;
                }
            }

            day++;
            if (day > 28) { Ending(); yield break; }

            // ������ ���� �غ�
            weather.RollTomorrowForecast();
            yield return null;
        }

        void GameOver() { /* �� ��ȯ/�г� ���� */ }
        void Ending() { /* ���� ó�� */ }
    }
}