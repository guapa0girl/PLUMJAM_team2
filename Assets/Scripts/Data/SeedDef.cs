// SeedDef.cs
using UnityEngine;
// ─────────────────────────────────────────────────────────────
// File: SeedDef.cs
// Purpose : 씨앗/작물의 밸런스 데이터를 ScriptableObject로 보관.
// Defines : class SeedDef : ScriptableObject
// Fields  : seedId, displayName, preferred(선호 날씨), growDays, sellPrice
// Used By : FarmingSystem(성장/죽음/수확), LootTable(드랍 결과), 상점/인벤토리.
// Author  : 데이터 주도 설계(아티스트/밸런서가 에디터에서 직접 편집).
// ─────────────────────────────────────────────────────────────

[CreateAssetMenu(menuName = "Game/Seed")]
public class SeedDef : ScriptableObject
{
    public string seedId;
    public string displayName;
    public WeatherType preferred;   // 맞으면 성장
    public int growDays;            // 성장일수
    public int sellPrice;           // 성숙 작물 판매가
}
