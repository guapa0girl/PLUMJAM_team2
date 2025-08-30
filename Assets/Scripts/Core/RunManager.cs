using UnityEngine;
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
