using UnityEngine;
using System.Collections.Generic;
using Game.Data;
using Game.Core;

namespace Game.Systems
{
    // SkillSystem.cs
    // ─────────────────────────────────────────────────────────────
    // File: SkillSystem.cs
    // Purpose : 보유 스킬 레벨과 날씨 시너지에 따른 전투 배율 계산/업그레이드.
    // Defines : class SkillSystem : MonoBehaviour
    // Fields  : List<OwnedSkill> (def, level)
    // API     : GetCombatMultiplier(WeatherType) → 날씨별 총 배율 반환
    //           TryUpgrade(Economy, skillId)     → 비용 차감 후 레벨 업
    // Used By : Combat 계산식(데미지/공속/드랍 가중치 등 프로젝트 규칙에 적용).
    // Notes   : 스킬의 효과 범위(곱/합, cap)는 팀 규칙 문서와 동기화할 것.
    // ─────────────────────────────────────────────────────────────

    public class SkillSystem : MonoBehaviour
    {
        [System.Serializable] public class OwnedSkill { public SkillDef def; public int level; }
        public List<OwnedSkill> skills;

        public float GetCombatMultiplier(WeatherType today)
        {
            float m = 1f;
            foreach (var s in skills)
            {
                if (s.def.synergy == today)
                {
                    m *= s.def.baseMultiplier + s.level * s.def.perLevelBonus;
                }
            }
            return m;
        }

        public bool TryUpgrade(Economy econ, string skillId)
        {
            var s = skills.Find(x => x.def.skillId == skillId);
            if (s == null || s.level >= s.def.levelCap) return false;
            int cost = s.def.upgradeCostBase * (s.level + 1);
            if (!econ.TrySpend(cost)) return false;
            s.level++;
            return true;
        }
    }
}