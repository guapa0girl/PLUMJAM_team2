// SkillDef.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skill")]
public class SkillDef : ScriptableObject
{
    public string skillId;
    public string displayName;
    public WeatherType synergy;     // 해당 날씨에서 강화
    public float baseMultiplier = 1f;
    public float perLevelBonus = 0.1f;
    public int levelCap = 5;
    public int upgradeCostBase = 50;
}
