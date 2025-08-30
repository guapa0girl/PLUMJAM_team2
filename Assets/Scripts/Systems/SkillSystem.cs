using UnityEngine;
using System.Collections.Generic;
using Game.Data;
using Game.Core;

namespace Game.Systems
{
    // SkillSystem.cs
    // ��������������������������������������������������������������������������������������������������������������������������
    // File: SkillSystem.cs
    // Purpose : ���� ��ų ������ ���� �ó����� ���� ���� ���� ���/���׷��̵�.
    // Defines : class SkillSystem : MonoBehaviour
    // Fields  : List<OwnedSkill> (def, level)
    // API     : GetCombatMultiplier(WeatherType) �� ������ �� ���� ��ȯ
    //           TryUpgrade(Economy, skillId)     �� ��� ���� �� ���� ��
    // Used By : Combat ����(������/����/��� ����ġ �� ������Ʈ ��Ģ�� ����).
    // Notes   : ��ų�� ȿ�� ����(��/��, cap)�� �� ��Ģ ������ ����ȭ�� ��.
    // ��������������������������������������������������������������������������������������������������������������������������

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