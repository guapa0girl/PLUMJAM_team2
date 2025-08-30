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
            public bool isOnCooldown => currentCooldown > 0;
        }

        [Header("Skills")]
        [SerializeField] public List<OwnedSkill> skills; // 보유한 스킬 목록
        private WeatherSystem weatherSystem;

        void Start()
        {
            weatherSystem = FindObjectOfType<WeatherSystem>();  // 날씨 시스템을 찾음
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) UseSkill(0);
            foreach (var skill in skills)
            {
                skill.currentCooldown = Mathf.Max(0, skill.currentCooldown - Time.deltaTime); // 쿨타임 갱신
            }
        }

        // 스킬 사용 함수 (쿨타임 체크 및 공격력 계산)
        public void UseSkill(int skillIndex)
        {
            var skill = skills[skillIndex];

            if (skill.isOnCooldown) return; // 쿨타임 중이라면 사용 불가

            float attackPower = skill.def.GetAttackPower(weatherSystem.Today); // 날씨에 따른 강화된 공격력 계산
            ExecuteSkill(skillIndex, attackPower); // 스킬 실행

            skill.currentCooldown = skill.def.cooldownTime; // 쿨타임 리셋
            
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

        // 스킬 실행 (공격 방식에 따른 실행)
        void ExecuteSkill(int skillIndex, float attackPower)
        {
            var skill = skills[skillIndex];

            // 공격 방식에 따라 실행
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

            // 이펙트 실행
            InstantiateSkillEffect(skill.def.effectPrefab);
        }

        // 근접 공격 처리
        void ExecuteMeleeAttack(float attackPower)
        {
            Debug.Log($"Executing Melee Attack with {attackPower} attack power.");
            // 근접 공격 실행 코드
        }

        // 원거리 공격 처리
        void ExecuteRangedAttack(float attackPower)
        {
            Debug.Log($"Executing Ranged Attack with {attackPower} attack power.");
            // 원거리 공격 실행 코드
        }

        // 범위 공격 처리
        void ExecuteAOEAttack(float attackPower)
        {
            Debug.Log($"Executing AOE Attack with {attackPower} attack power.");
            // 범위 내 적들에게 피해를 입히는 코드
        }

        // 이펙트 인스턴스화 (이펙트 프리팹을 게임 오브젝트로 생성)
        void InstantiateSkillEffect(GameObject effectPrefab)
        {
            if (effectPrefab != null)
            {
                // 예시로 플레이어의 위치에 이펙트를 생성 (필요한 경우 위치나 방향 조정 가능)
                Instantiate(effectPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}