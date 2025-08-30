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
        [Tooltip("코드/세이브에서 쓰는 고유 ID (영문/스네이크케이스 권장)")]
        public string skillId;

        [Tooltip("UI 등에 표시될 스킬 이름")]
        public string displayName;

        [Header("Synergy")]
        [Tooltip("이 날씨일 때 스킬 시너지가 적용됩니다.")]
        public WeatherType synergy;     // 해당 날씨에서 강화

        [Header("Power")]
        [Min(0f)]
        [Tooltip("기본 배율 (레벨 0 기준). 예: 1.0 = 변화 없음")]
        public float baseMultiplier = 1f;

        [Min(0f)]
        [Tooltip("레벨 1 증가 시 추가되는 배율. 예: 0.1 → 레벨당 +10%")]
        public float perLevelBonus = 0.1f;

        [Header("Limits & Cost")]
        [Min(0)]
        [Tooltip("스킬 최대 레벨")]
        public int levelCap = 5;

        [Min(0)]
        [Tooltip("업그레이드 기본 비용(레벨당 증가 규칙은 SkillSystem에서 결정)")]
        public int upgradeCostBase = 50;

    }
}
