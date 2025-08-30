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
        [SerializeField] public List<OwnedSkill> skills; // ������ ��ų ���
        private WeatherSystem weatherSystem;

        void Start()
        {
            weatherSystem = FindObjectOfType<WeatherSystem>();  // ���� �ý����� ã��
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) UseSkill(0);
            foreach (var skill in skills)
            {
                skill.currentCooldown = Mathf.Max(0, skill.currentCooldown - Time.deltaTime); // ��Ÿ�� ����
            }
        }

        // ��ų ��� �Լ� (��Ÿ�� üũ �� ���ݷ� ���)
        public void UseSkill(int skillIndex)
        {
            var skill = skills[skillIndex];

            if (skill.isOnCooldown) return; // ��Ÿ�� ���̶�� ��� �Ұ�

            float attackPower = skill.def.GetAttackPower(weatherSystem.Today); // ������ ���� ��ȭ�� ���ݷ� ���
            ExecuteSkill(skillIndex, attackPower); // ��ų ����

            skill.currentCooldown = skill.def.cooldownTime; // ��Ÿ�� ����
            
        }
        public bool TryUpgrade(Economy econ, string skillId)
        {
            weatherSystem = FindObjectOfType<WeatherSystem>();  // ���� �ý����� ã��
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) UseSkill(0);
            foreach (var skill in skills)
            {
                skill.currentCooldown = Mathf.Max(0, skill.currentCooldown - Time.deltaTime); // ��Ÿ�� ����
            }
        }

        // ��ų ��� �Լ� (��Ÿ�� üũ �� ���ݷ� ���)
        public void UseSkill(int skillIndex)
        {
            var skill = skills[skillIndex];

            if (skill.isOnCooldown) return; // ��Ÿ�� ���̶�� ��� �Ұ�

            float attackPower = skill.def.GetAttackPower(weatherSystem.Today); // ������ ���� ��ȭ�� ���ݷ� ���
            ExecuteSkill(skillIndex, attackPower); // ��ų ����

            skill.currentCooldown = skill.def.cooldownTime; // ��Ÿ�� ����
        }

        // ��ų ���� (���� ��Ŀ� ���� ����)
        void ExecuteSkill(int skillIndex, float attackPower)
        {
            var skill = skills[skillIndex];

            // ���� ��Ŀ� ���� ����
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

            // ����Ʈ ����
            InstantiateSkillEffect(skill.def.effectPrefab);
        }

        // ���� ���� ó��
        void ExecuteMeleeAttack(float attackPower)
        {
            Debug.Log($"Executing Melee Attack with {attackPower} attack power.");
            // ���� ���� ���� �ڵ�
        }

        // ���Ÿ� ���� ó��
        void ExecuteRangedAttack(float attackPower)
        {
            Debug.Log($"Executing Ranged Attack with {attackPower} attack power.");
            // ���Ÿ� ���� ���� �ڵ�
        }

        // ���� ���� ó��
        void ExecuteAOEAttack(float attackPower)
        {
            Debug.Log($"Executing AOE Attack with {attackPower} attack power.");
            // ���� �� ���鿡�� ���ظ� ������ �ڵ�
        }

        // ����Ʈ �ν��Ͻ�ȭ (����Ʈ �������� ���� ������Ʈ�� ����)
        void InstantiateSkillEffect(GameObject effectPrefab)
        {
            if (effectPrefab != null)
            {
                // ���÷� �÷��̾��� ��ġ�� ����Ʈ�� ���� (�ʿ��� ��� ��ġ�� ���� ���� ����)
                Instantiate(effectPrefab, transform.position, Quaternion.identity);
            }
        }

        // ��ų ���� (���� ��Ŀ� ���� ����)
        void ExecuteSkill(int skillIndex, float attackPower)
        {
            var skill = skills[skillIndex];

            // ���� ��Ŀ� ���� ����
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

            // ����Ʈ ����
            InstantiateSkillEffect(skill.def.effectPrefab);
        }

        // ���� ���� ó��
        void ExecuteMeleeAttack(float attackPower)
        {
            Debug.Log($"Executing Melee Attack with {attackPower} attack power.");
            // ���� ���� ���� �ڵ�
        }

        // ���Ÿ� ���� ó��
        void ExecuteRangedAttack(float attackPower)
        {
            Debug.Log($"Executing Ranged Attack with {attackPower} attack power.");
            // ���Ÿ� ���� ���� �ڵ�
        }

        // ���� ���� ó��
        void ExecuteAOEAttack(float attackPower)
        {
            Debug.Log($"Executing AOE Attack with {attackPower} attack power.");
            // ���� �� ���鿡�� ���ظ� ������ �ڵ�
        }

        // ����Ʈ �ν��Ͻ�ȭ (����Ʈ �������� ���� ������Ʈ�� ����)
        void InstantiateSkillEffect(GameObject effectPrefab)
        {
            if (effectPrefab != null)
            {
                // ���÷� �÷��̾��� ��ġ�� ����Ʈ�� ���� (�ʿ��� ��� ��ġ�� ���� ���� ����)
                Instantiate(effectPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}