using UnityEngine;
using System.Collections.Generic;
using Game.Data;
using Game.Core;

namespace Game.Systems
{
    // ─────────────────────────────────────────────────────────────
    // File: SkillSystem.cs
    // Purpose : 보유 스킬/쿨타임, 날씨 시너지 기반 공격 실행 + 업그레이드.
    // API     : UseSkill(int), TryUpgrade(Economy, skillId)
    // Notes   : SkillDef에 GetAttackPower(WeatherType), cooldownTime,
    //           attackType, effectPrefab가 정의돼 있어야 함.
    // ─────────────────────────────────────────────────────────────
    public class SkillSystem : MonoBehaviour
    {
        // 단축 호출용(옵션)
        public void UseSkill0() { UseSkill(0); }
        public void UseSkill1() { UseSkill(1); }
        public void UseSkill2() { UseSkill(2); }
        public void UseSkill3() { UseSkill(3); }
        public void UseSkill4() { UseSkill(4); }

        [System.Serializable]
        public class OwnedSkill
        {
            public SkillDef def;
            public int level;
            public float currentCooldown;
            public bool isOnCooldown => currentCooldown > 0f;
        }

        [Header("Skills")]
        [SerializeField] public List<OwnedSkill> skills = new();

        // 오늘 날씨 참조(씬에 WeatherSystem 존재 가정)
        private WeatherSystem weatherSystem;

        void Start()
        {
            weatherSystem = FindObjectOfType<WeatherSystem>();
        }

        void Update()
        {
            // 숫자키 1 → 첫 스킬 실행(원하면 2~5 추가)
            if (Input.GetKeyDown(KeyCode.Alpha1)) UseSkill(0);

            // 쿨타임 갱신
            foreach (var s in skills)
                s.currentCooldown = Mathf.Max(0f, s.currentCooldown - Time.deltaTime);
        }

        // ===== gayeonlee-sceneflow 버전 유지 =====
        public void UseSkill(int skillIndex)
        {
            if (skills == null || skillIndex < 0 || skillIndex >= skills.Count) return;
            var skill = skills[skillIndex];
            if (skill == null || skill.def == null) return;

            if (skill.isOnCooldown) return; // 쿨타임이면 사용 불가

            // 날씨 기반 공격력 계산 (SkillDef가 제공)
            var today = weatherSystem ? weatherSystem.Today : default; // WeatherSystem 없으면 default
            float attackPower = skill.def.GetAttackPower(today);

            ExecuteSkill(skillIndex, attackPower); // 실제 실행
            skill.currentCooldown = skill.def.cooldownTime; // 쿨타임 시작
        }

        // ===== 기존 TryUpgrade 유지(경제/레벨업) =====
        public bool TryUpgrade(Economy econ, string skillId)
        {
            if (econ == null || string.IsNullOrEmpty(skillId) || skills == null) return false;

            var s = skills.Find(x => x != null && x.def != null && x.def.skillId == skillId);
            if (s == null || s.def == null) return false;
            if (s.level >= s.def.levelCap) return false;

            int cost = s.def.upgradeCostBase * (s.level + 1);
            if (!econ.TrySpend(cost)) return false;

            s.level++;
            return true;
        }

        // 공격 타입 분기
        void ExecuteSkill(int skillIndex, float attackPower)
        {
            var skill = skills[skillIndex];
            if (skill == null || skill.def == null) return;

            switch (skill.def.attackType)
            {
                case SkillDef.AttackType.Melee:
                    ExecuteMeleeAttack(attackPower);
                    break;
                case SkillDef.AttackType.Ranged:
                    ExecuteRangedAttack(attackPower);
                    break;
                case SkillDef.AttackType.AreaOfEffect:
                    ExecuteAOEAttack(attackPower);
                    break;
                default:
                    Debug.LogWarning("Unknown attack type!");
                    break;
            }

            // 이펙트
            InstantiateSkillEffect(skill.def.effectPrefab);
        }

        // 근접 공격(구체 로직은 프로젝트에 맞게 교체)
        void ExecuteMeleeAttack(float attackPower)
        {
            Debug.Log($"Melee Attack, power={attackPower}");
        }
        // 원거리
        void ExecuteRangedAttack(float attackPower)
        {
            Debug.Log($"Ranged Attack, power={attackPower}");
        }
        // 광역
        void ExecuteAOEAttack(float attackPower)
        {
            Debug.Log($"AOE Attack, power={attackPower}");
        }

        // 이펙트 생성
        void InstantiateSkillEffect(GameObject effectPrefab)
        {
            if (effectPrefab)
                Instantiate(effectPrefab, transform.position, Quaternion.identity);
        }
    }
}
