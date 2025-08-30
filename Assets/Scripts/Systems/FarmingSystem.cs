using UnityEngine;
// ��������������������������������������������������������������������������������������������������������������������������
// File: FarmingSystem.cs
// Purpose : ȭ��(Plot) ���� ������Ʈ: �ɱ�/����/����/��Ȯ/�Ǹ� ����.
// Defines : class FarmingSystem : MonoBehaviour
// Fields  : List<Plot> (occupied, seed, daysGrown, dead)
// API     : Plant(SeedDef, index)        �� �ɱ�
//           OnNewDay(WeatherType)        �� ���� ������ ���� ����/��� ����
//           HarvestAndSell(Economy)      �� ���� �۹� �Ǹ�, ���� ��ȯ
// Used By : RunManager(�ϴ��� ����), Economy(�� ����).
// Notes   : ������ �̽� �� ��硱 ��Ģ�� ��ȹ ������. ��ȭ ��/��ų�� ���� Ȯ�� ����.
// ��������������������������������������������������������������������������������������������������������������������������

public class FarmingSystem : MonoBehaviour
{
    [System.Serializable]
    public class Plot
    {
        public bool occupied; public SeedDef seed; public int daysGrown; public bool dead;
    }
    public List<Plot> plots = new();

    public void Plant(SeedDef seed, int plotIndex)
    {
        var p = plots[plotIndex]; p.occupied = true; p.seed = seed; p.daysGrown = 0; p.dead = false; plots[plotIndex] = p;
    }

    public void OnNewDay(WeatherType today)
    {
        for (int i = 0; i < plots.Count; i++)
        {
            var p = plots[i];
            if (!p.occupied || p.dead) continue;
            if (p.seed.preferred == today) p.daysGrown++;
            else p.dead = true; // �� ������ ���(��ȹ���)
            plots[i] = p;
        }
    }

    public int HarvestAndSell(Economy econ)
    {
        int earned = 0;
        for (int i = 0; i < plots.Count; i++)
        {
            var p = plots[i];
            if (p.occupied && !p.dead && p.daysGrown >= p.seed.growDays)
            {
                earned += p.seed.sellPrice;
                p = new Plot(); // �� ȭ��
            }
            plots[i] = p;
        }
        econ.AddMoney(earned);
        return earned;
    }
}
