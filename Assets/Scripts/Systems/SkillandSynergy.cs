using UnityEngine;
using Game.Data;
using Game.Systems;
<<<<<<< Updated upstream
using Game.Core;
=======
>>>>>>> Stashed changes

public class SkillandSynergy : MonoBehaviour
{
    [System.Serializable]
    public class Skill
    {
<<<<<<< Updated upstream
        public string skillName;               // 스킬 이름
        public float baseAttackPower;          // 기본 공격력
        public float cooldownTime;             // 쿨타임
        public float currentCooldown;          // 현재 쿨타임
        public SkillType skillType;            // 스킬 타입 (예: 기본 공격, Fire, Ice 등)
        public bool isOnCooldown => currentCooldown > 0; // 쿨타임 체크

        // 시너지 적용 (배율은 1.1로 고정)
=======
        [Header("Identity & Numbers")]
        public string skillName;
        public float baseAttackPower = 1f;
        public float cooldownTime = 0.5f;
        [HideInInspector] public float currentCooldown;
        public bool isOnCooldown => currentCooldown > 0f;

        [Header("Level (0=Off, 1~5)")]
        [Range(0, 5)] public int level = 1;                 // 0=��Ȱ��, 1~5 ���
        [Tooltip("������ ���� ����(��: 0.15 = +15%/����)")]
        public float perLevelBonus = 0.15f;                 // (1 + perLevelBonus*(level-1))

        [Header("Type")]
        public SkillType skillType;            // BasicAttack, Fire, Water, Ice, Wind
        public AttackMode attackMode;          // Melee, Ranged, AreaOfEffect

        [Header("Effect")]
        public GameObject effectPrefab;
        public bool spawnOnCast = false;       // ������ ��ġ ����Ʈ
        public bool spawnOnHit = true;         // �ǰ� ���� ����Ʈ

        [HideInInspector] public int lastCastFrame = -1;    // �� ������ �ߺ� ĳ��Ʈ ����

        [Header("Ranges")]
        public float meleeRange = 1.6f;
        public float aoeRadius = 2.8f;
        public float rangedDistance = 10f;

        [Header("Auto Cast")]
        public bool autoCast = true;           // ��Ÿ�� ������ �ڵ� �ߵ�

        // �ó���+���� ���� ����
>>>>>>> Stashed changes
        public float GetAttackPower(WeatherType today)
        {
            // ���� ����: 0�����̸� 0
            float levelMul = (level <= 0) ? 0f : 1f + perLevelBonus * (level - 1);
            float attackPower = baseAttackPower * levelMul;

<<<<<<< Updated upstream
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
=======
            const float synergyMultiplier = 1.2f;
            if (today == WeatherType.Heat && skillType == SkillType.Fire) attackPower *= synergyMultiplier;
            else if (today == WeatherType.Rain && skillType == SkillType.Water) attackPower *= synergyMultiplier;
            else if (today == WeatherType.Snow && skillType == SkillType.Ice) attackPower *= synergyMultiplier;
            else if (today == WeatherType.Cloud && skillType == SkillType.Wind) attackPower *= synergyMultiplier;
>>>>>>> Stashed changes

            return attackPower;
        }

        public void ResetCooldown() => currentCooldown = cooldownTime;
        public void UpdateCooldown() => currentCooldown = Mathf.Max(0f, currentCooldown - Time.deltaTime);
    }

<<<<<<< Updated upstream
    public enum SkillType
    {
        BasicAttack,    // 기본 공격
        Fire,           // Fire 스킬
        Water,          // Water 스킬
        Ice,            // Ice 스킬 
        Wind            // Wind 스킬
    }

    public Skill[] skills; // 여러 스킬들
=======
    public enum SkillType { BasicAttack, Fire, Water, Ice, Wind }
    public enum AttackMode { Melee, Ranged, AreaOfEffect }

    [Header("Skills")]
    public Skill[] skills;

    [Header("Combat Settings")]
    public Transform attackOrigin;
    public LayerMask enemyMask = ~0;
    public float knockbackForce = 6f;

>>>>>>> Stashed changes
    private WeatherSystem weatherSystem;

    void Awake()
    {
        if (!attackOrigin) attackOrigin = transform;
    }

    void Start()
    {
<<<<<<< Updated upstream
        weatherSystem = FindObjectOfType<WeatherSystem>();  // 날씨 시스템을 찾음
=======
        weatherSystem = FindObjectOfType<WeatherSystem>(); // ��� null-safe ó����
        // ���� ���� ����ȭ ����:
        // foreach (var s in skills) s.currentCooldown = Random.Range(0f, s.cooldownTime);
>>>>>>> Stashed changes
    }

    void Update()
    {
        for (int i = 0; i < skills.Length; i++)
        {
<<<<<<< Updated upstream
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
=======
            var s = skills[i];
            s.UpdateCooldown();

            // 0������ ��Ȱ��
            if (s.level <= 0) continue;

            // ��Ÿ���� ������ �ڵ������̸� ��� �ߵ�
            if (s.autoCast && !s.isOnCooldown)
                UseSkill(i);
        }
    }

    // (���ϸ� UI���� ���� ȣ�⵵ ����)
    public void UseSkill0() { UseSkill(0); }
    public void UseSkill1() { UseSkill(1); }
    public void UseSkill2() { UseSkill(2); }
    public void UseSkill3() { UseSkill(3); }
    public void UseSkill4() { UseSkill(4); }

    public void UseSkill(int skillIndex)
    {
        if (skills == null || skillIndex < 0 || skillIndex >= skills.Length) { Debug.LogWarning("�߸��� ��ų �ε���"); return; }
        var s = skills[skillIndex];

        // 0���� �Ǵ� ��Ÿ�� ���̸� ��ŵ
        if (s.level <= 0 || s.isOnCooldown) return;

        // ���� ������ �ߺ� ȣ�� ����
        if (s.lastCastFrame == Time.frameCount) return;

        var today = weatherSystem ? weatherSystem.Today : default;
        float attackPower = s.GetAttackPower(today);
        if (attackPower <= 0f) return; // ���� 0 ��

        ExecuteSkill(s, attackPower);
        s.ResetCooldown();
        s.lastCastFrame = Time.frameCount;
>>>>>>> Stashed changes
    }

    void ExecuteSkill(Skill s, float power)
    {
<<<<<<< Updated upstream
        // 스킬을 실행하고 공격력을 기반으로 피해를 입히는 로직
        Debug.Log($"Executing {skills[skillIndex].skillName} with {attackPower} attack power.");
        // 예: 적에 피해를 입히는 코드
=======
        switch (s.attackMode)
        {
            case AttackMode.Melee: ExecuteMelee(s, power); break;
            case AttackMode.Ranged: ExecuteRanged(s, power); break;
            case AttackMode.AreaOfEffect: ExecuteAOE(s, power); break;
        }

        if (s.spawnOnCast)
            SpawnEffectAt(attackOrigin.position, s.effectPrefab);

        Debug.Log($"Auto-cast {s.skillName} L{s.level} ({s.skillType}/{s.attackMode}) power={power}");
    }

    // �������������������������� Ÿ�� ���̷ε� ��������������������������
    [System.Serializable]
    public class HitPayload
    {
        public float damage;
        public SkillType element;
        public Vector3 hitDirection;
        public float knockback;
        public float dotDps;
        public float dotDuration;
        public float slowMultiplier;
        public float slowDuration;
        public float freezeDuration;
    }

    void ApplyHit(GameObject enemy, Skill s, float baseDamage, Vector3 fromPos)
    {
        Vector3 dir = (enemy.transform.position - fromPos).normalized;
        float damage = baseDamage;

        var payload = new HitPayload
        {
            damage = damage,
            element = s.skillType,
            hitDirection = dir,
            knockback = (s.skillType == SkillType.Wind) ? knockbackForce * 1.5f : knockbackForce
        };

        // ���� ����(�������� �̹� ���� �ݿ���)
        switch (s.skillType)
        {
            case SkillType.Fire:
                payload.damage = damage * 1.1f;
                payload.dotDps = Mathf.Max(1f, payload.damage * 0.25f);
                payload.dotDuration = 2f;
                break;
            case SkillType.Water:
                payload.damage = damage * 0.95f;
                payload.slowMultiplier = 0.5f;
                payload.slowDuration = 2f;
                break;
            case SkillType.Ice:
                payload.freezeDuration = 1f;
                break;
        }

        var rb2 = enemy.GetComponent<Rigidbody2D>();
        if (rb2) rb2.AddForce(dir * payload.knockback, ForceMode2D.Impulse);
        var rb3 = enemy.GetComponent<Rigidbody>();
        if (rb3) rb3.AddForce(dir * payload.knockback, ForceMode.Impulse);

        enemy.SendMessage("OnHit", payload, SendMessageOptions.DontRequireReceiver);

        if (s.spawnOnHit)
            SpawnEffectAt(enemy.transform.position, s.effectPrefab);
    }

    // �������������������������� ���� ���� ��������������������������
    void ExecuteMelee(Skill s, float power)
    {
        Vector3 o = attackOrigin.position;
        var seen = new System.Collections.Generic.HashSet<GameObject>();

        var hits2D = Physics2D.OverlapCircleAll(o, s.meleeRange, enemyMask);
        foreach (var h in hits2D)
        {
            var go = h.attachedRigidbody ? h.attachedRigidbody.gameObject : h.gameObject;
            if (IsSelf(go)) continue;
            if (seen.Add(go)) ApplyHit(go, s, power, o);
        }

        var hits3D = Physics.OverlapSphere(o, s.meleeRange, enemyMask, QueryTriggerInteraction.Ignore);
        foreach (var c in hits3D)
            if (!IsSelf(c.gameObject) && seen.Add(c.gameObject))
                ApplyHit(c.gameObject, s, power, o);
    }

    void ExecuteAOE(Skill s, float power)
    {
        Vector3 o = attackOrigin.position;
        var seen = new System.Collections.Generic.HashSet<GameObject>();

        var hits2D = Physics2D.OverlapCircleAll(o, s.aoeRadius, enemyMask);
        foreach (var h in hits2D)
        {
            var go = h.attachedRigidbody ? h.attachedRigidbody.gameObject : h.gameObject;
            if (IsSelf(go)) continue;
            if (seen.Add(go)) ApplyHit(go, s, power, o);
        }

        var hits3D = Physics.OverlapSphere(o, s.aoeRadius, enemyMask, QueryTriggerInteraction.Ignore);
        foreach (var c in hits3D)
            if (!IsSelf(c.gameObject) && seen.Add(c.gameObject))
                ApplyHit(c.gameObject, s, power, o);
    }

    void ExecuteRanged(Skill s, float power)
    {
        Vector3 o = attackOrigin.position;
        Vector3 dir = attackOrigin.right;
        float skin = 0.1f;                         // �ڱ� �ݶ��̴� ��¦ �տ��� �߻�
        Vector3 start = o + dir * skin;

        var hits2D = Physics2D.RaycastAll(start, dir, s.rangedDistance - skin, enemyMask);
        foreach (var h in hits2D)
        {
            var go = h.rigidbody ? h.rigidbody.gameObject : (h.collider ? h.collider.gameObject : null);
            if (!go || IsSelf(go)) continue;
            ApplyHit(go, s, power, o);
            return;
        }

        var hits3D = Physics.RaycastAll(start, dir, s.rangedDistance - skin, enemyMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits3D, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var h in hits3D)
        {
            var go = h.collider ? h.collider.gameObject : null;
            if (!go || IsSelf(go)) continue;
            ApplyHit(go, s, power, o);
            return;
        }
    }

    void SpawnEffectAt(Vector3 pos, GameObject prefab)
    {
        if (!prefab) return;
        var go = Instantiate(prefab, pos, Quaternion.identity);

        var ps = go.GetComponent<ParticleSystem>();
        if (ps)
        {
            var main = ps.main;
            Destroy(go, main.duration + main.startLifetimeMultiplier);
        }
        else
        {
            Destroy(go, 0.5f);
        }
    }

    // �� ����! �ڱ� �ڽ� ��Ʈ ���� �Լ��� �ٱ�(�� Ŭ���� ���)�̾�� ��
    bool IsSelf(GameObject go)
    {
        if (!go) return false;
        return go == gameObject || go.transform.root == transform.root;
    }

    void OnDrawGizmosSelected()
    {
        if (!attackOrigin) return;
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(attackOrigin.position, 1.6f);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(attackOrigin.position, 2.8f);
        Gizmos.color = Color.white; Gizmos.DrawLine(attackOrigin.position, attackOrigin.position + attackOrigin.right * 10f);
>>>>>>> Stashed changes
    }
}