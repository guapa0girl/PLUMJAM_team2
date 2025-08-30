using UnityEngine;
using Game.Data;

public class SkillandSynergy : MonoBehaviour
{
    [System.Serializable]
    public class Skill
    {
        public string skillName;               // 스킬 이름
        public float baseAttackPower;          // 기본 공격력
        public float cooldownTime;             // 쿨타임
        public float currentCooldown;          // 현재 쿨타임
        public SkillType skillType;            // 스킬 타입 (예: 기본 공격, Fire, Ice 등)
        public bool isOnCooldown => currentCooldown > 0; // 쿨타임 체크

        // 시너지 적용 (배율은 1.1로 고정)
        public float GetAttackPower(WeatherType today)
        {
            float attackPower = baseAttackPower;

            // 모든 날씨에 대해 1.2 배율을 적용
            float synergyMultiplier = 1.2f;

            // 날씨에 따른 시너지 
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
        BasicAttack,    // 기본 공격
        Fire,           // Fire 스킬
        Water,          // Water 스킬
        Ice,            // Ice 스킬 
        Wind            // Wind 스킬
    }

    public Skill[] skills; // 여러 스킬들
    private WeatherSystem weatherSystem;

    void Start()
    {
        weatherSystem = FindObjectOfType<WeatherSystem>();  // 날씨 시스템을 찾음
    }

    void Update()
    {
        foreach (var skill in skills)
        {
            skill.UpdateCooldown(); // 쿨타임 업데이트
        }
    }

    // 스킬 사용 함수
    public void UseSkill(int skillIndex)
    {
        if (skills[skillIndex].isOnCooldown) return;  // 쿨타임 중이라면 사용 못 함

        float attackPower = skills[skillIndex].GetAttackPower(weatherSystem.Today); // 날씨 반영된 공격력 계산
        ExecuteSkill(skillIndex, attackPower);  // 스킬 실행

        skills[skillIndex].ResetCooldown(); // 쿨타임 리셋
    }

    void ExecuteSkill(int skillIndex, float attackPower)
    {
        // 스킬을 실행하고 공격력을 기반으로 피해를 입히는 로직
        Debug.Log($"Executing {skills[skillIndex].skillName} with {attackPower} attack power.");
        // 예: 적에 피해를 입히는 코드
    }
}