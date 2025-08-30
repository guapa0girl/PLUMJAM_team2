using UnityEngine;
using Game.Data;

public class SkillandSynergy : MonoBehaviour
{
    [System.Serializable]
    public class Skill
    {
        public string skillName;               // ��ų �̸�
        public float baseAttackPower;          // �⺻ ���ݷ�
        public float cooldownTime;             // ��Ÿ��
        public float currentCooldown;          // ���� ��Ÿ��
        public SkillType skillType;            // ��ų Ÿ�� (��: �⺻ ����, Fire, Ice ��)
        public bool isOnCooldown => currentCooldown > 0; // ��Ÿ�� üũ

        // �ó��� ���� (������ 1.1�� ����)
        public float GetAttackPower(WeatherType today)
        {
            float attackPower = baseAttackPower;

            // ��� ������ ���� 1.2 ������ ����
            float synergyMultiplier = 1.2f;

            // ������ ���� �ó��� 
            if (today == WeatherType.Heat && skillType == SkillType.Fire)
            {
                attackPower *= synergyMultiplier;
            }
            else if (today == WeatherType.Rain && skillType == SkillType.Water)
            {
                attackPower *= synergyMultiplier;
            }
            else if (today == WeatherType.Snow && skillType == SkillType.Ice) 
            {
                attackPower *= synergyMultiplier;
            }
            else if (today == WeatherType.Cloud && skillType == SkillType.Wind) 
            {
                attackPower *= synergyMultiplier;
            }

            return attackPower;
        }

        public void ResetCooldown() => currentCooldown = cooldownTime;
        public void UpdateCooldown() => currentCooldown = Mathf.Max(0, currentCooldown - Time.deltaTime);
    }

    public enum SkillType
    {
        BasicAttack,    // �⺻ ����
        Fire,           // Fire ��ų
        Water,          // Water ��ų
        Ice,            // Ice ��ų 
        Wind            // Wind ��ų
    }

    public Skill[] skills; // ���� ��ų��
    private WeatherSystem weatherSystem;

    void Start()
    {
        weatherSystem = FindObjectOfType<WeatherSystem>();  // ���� �ý����� ã��
    }

    void Update()
    {
        foreach (var skill in skills)
        {
            skill.UpdateCooldown(); // ��Ÿ�� ������Ʈ
        }
    }

    // ��ų ��� �Լ�
    public void UseSkill(int skillIndex)
    {
        if (skills[skillIndex].isOnCooldown) return;  // ��Ÿ�� ���̶�� ��� �� ��

        float attackPower = skills[skillIndex].GetAttackPower(weatherSystem.Today); // ���� �ݿ��� ���ݷ� ���
        ExecuteSkill(skillIndex, attackPower);  // ��ų ����

        skills[skillIndex].ResetCooldown(); // ��Ÿ�� ����
    }

    void ExecuteSkill(int skillIndex, float attackPower)
    {
        // ��ų�� �����ϰ� ���ݷ��� ������� ���ظ� ������ ����
        Debug.Log($"Executing {skills[skillIndex].skillName} with {attackPower} attack power.");
        // ��: ���� ���ظ� ������ �ڵ�
    }
}