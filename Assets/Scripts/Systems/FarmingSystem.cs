using UnityEngine;
// ─────────────────────────────────────────────────────────────
// File: FarmingSystem.cs
// Purpose : 화단(Plot) 상태 업데이트: 심기/성장/죽음/수확/판매 연동.
// Defines : class FarmingSystem : MonoBehaviour
// Fields  : List<Plot> (occupied, seed, daysGrown, dead)
// API     : Plant(SeedDef, index)        → 심기
//           OnNewDay(WeatherType)        → 오늘 날씨에 따른 성장/즉사 판정
//           HarvestAndSell(Economy)      → 성숙 작물 판매, 수익 반환
// Used By : RunManager(일단위 진행), Economy(돈 증가).
// Notes   : “날씨 미스 시 즉사” 규칙은 기획 고정값. 완화 룬/스킬로 예외 확장 가능.
// ─────────────────────────────────────────────────────────────

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
            else p.dead = true; // 안 맞으면 즉사(기획대로)
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
                p = new Plot(); // 빈 화단
            }
            plots[i] = p;
        }
        econ.AddMoney(earned);
        return earned;
    }
}
