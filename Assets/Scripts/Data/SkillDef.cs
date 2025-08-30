// SkillDef.cs
// ─────────────────────────────────────────────────────────────
// File    : SkillDef.cs
// Namespace : Game.Data
// Purpose : 전투 스킬의 “정의 데이터”를 담는 ScriptableObject.
//           (이 클래스 자체는 로직 최소화, 수치/설정만 보관)
// Defines : class SkillDef : ScriptableObject
// Fields  : skillId(고유ID), displayName(표시명), synergy(시너지 날씨),
//           baseMultiplier(기본 배율), perLevelBonus(레벨당 증가),
//           levelCap(최대 레벨), upgradeCostBase(업그레이드 기본 비용)
// Used By : Game.Systems.SkillSystem (배율 계산/업그레이드 비용 산정)
// Notes   : 데이터 드리븐 설계 — 밸런서가 에셋만 수정해도 동작이 바뀜.
//           에셋 생성: Project 창 → Create → Game → Skill
// ─────────────────────────────────────────────────────────────

using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// 전투 스킬의 “정의 데이터”.
    /// - 시너지 날씨에서 배율이 커지고, 레벨에 따라 추가 보너스가 붙습니다.
    /// - 실제 적용/업그레이드 로직은 Game.Systems.SkillSystem에서 처리합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Skill", menuName = "Game/Skill", order = 0)]
    public class SkillDef : ScriptableObject
    {
        [Header("Identity")]
        public string skillId;               // 고유 ID
        public string displayName;           // UI에 표시될 스킬 이름

        [Header("Synergy")]
        public WeatherType synergy;          // 날씨 시너지 (예: Fire 스킬은 Heat 날씨에서 강화)

        [Header("Power")]
        public float baseMultiplier = 1f;    // 기본 배율
        public float perLevelBonus = 0.1f;   // 레벨당 배율 증가

        [Header("Cooldown")]
        public float cooldownTime = 5f;      // 기본 쿨타임

        [Header("Attack Type")]
        public AttackType attackType;        // 공격 타입 (근접, 원거리, 범위 등)

        public enum AttackType
        {
            Melee,        // 근접 공격
            Ranged,       // 원거리 공격
            AreaOfEffect  // 범위 공격
        }

        [Header("Elemental Type")]
        public ElementalType elementalType;  // 스킬 속성 (예: Fire, Water, Ice, Wind)

        public enum ElementalType
        {
            Fire,          // 화염
            Water,         // 물
            Ice,           // 얼음
            Wind,          // 바람
            BasicAttack    // 기본공격
        }

        [Header("Effects")]
        public GameObject effectPrefab;       // 이펙트 프리팹 (GameObject)

        [Header("Limits & Cost")]
        public int levelCap = 5;             // 최대 레벨
        public int upgradeCostBase = 50;     // 기본 업그레이드 비용

        // 공격력 계산 (날씨 시너지 적용)
        public float GetAttackPower(WeatherType today)
        {
            float attackPower = baseMultiplier;
            float synergyMultiplier = 1.2f; // 기본 배율

            // 날씨에 따른 시너지 배율
            if (today == WeatherType.Heat && synergy == WeatherType.Heat)
            {
                synergyMultiplier = 1.5f;
            }
            else if (today == WeatherType.Rain && synergy == WeatherType.Rain)
            {
                synergyMultiplier = 1.3f;
            }
            else if (today == WeatherType.Snow && synergy == WeatherType.Snow)
            {
                synergyMultiplier = 1.2f;
            }
            else if (today == WeatherType.Cloud && synergy == WeatherType.Cloud)
            {
                synergyMultiplier = 1.1f;
            }

            attackPower *= synergyMultiplier;
            return attackPower;
        }
    }
}