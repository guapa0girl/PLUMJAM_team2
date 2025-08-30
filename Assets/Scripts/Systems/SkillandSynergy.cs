using UnityEngine;
using Game.Data;
using Game.Systems;

public class SkillandSynergy : MonoBehaviour
{
    [System.Serializable]
    public class Skill
    {
        [Header("Identity & Numbers")]
        public string skillName;
        public float baseAttackPower = 1f;
        public float cooldownTime = 0.5f;
        [HideInInspector] public float currentCooldown;
        public bool isOnCooldown => currentCooldown > 0f;

        [Header("Type")]
        public SkillType skillType;            // BasicAttack, Fire, Water, Ice, Wind
        public AttackMode attackMode;          // Melee, Ranged, AreaOfEffect

        [Header("Effect")]
        public GameObject effectPrefab;

        [Header("Ranges")]
        public float meleeRange = 1.6f;
        public float aoeRadius = 2.8f;
        public float rangedDistance = 10f;

        [Header("Auto Cast")]
        public bool autoCast = true;           // 쿨타임 끝나면 자동 발동

        // 시너지 적용(매칭 날씨에서 x1.2)
        public float GetAttackPower(WeatherType today)
        {
            float attackPower = baseAttackPower;
            const float synergyMultiplier = 1.2f;

            if (today == WeatherType.Heat && skillType == SkillType.Fire) attackPower *= synergyMultiplier;
            else if (today == WeatherType.Rain && skillType == SkillType.Water) attackPower *= synergyMultiplier;
            else if (today == WeatherType.Snow && skillType == SkillType.Ice) attackPower *= synergyMultiplier;
            else if (today == WeatherType.Cloud && skillType == SkillType.Wind) attackPower *= synergyMultiplier;

            return attackPower;
        }

        public void ResetCooldown() => currentCooldown = cooldownTime;
        public void UpdateCooldown() => currentCooldown = Mathf.Max(0f, currentCooldown - Time.deltaTime);
    }

    public enum SkillType { BasicAttack, Fire, Water, Ice, Wind }
    public enum AttackMode { Melee, Ranged, AreaOfEffect }

    [Header("Skills")]
    public Skill[] skills;

    [Header("Combat Settings")]
    public Transform attackOrigin;
    public LayerMask enemyMask = ~0;
    public float knockbackForce = 6f;

    private WeatherSystem weatherSystem;

    void Awake()
    {
        if (!attackOrigin) attackOrigin = transform;
    }

    void Start()
    {
        weatherSystem = FindObjectOfType<WeatherSystem>(); // 없어도 null-safe 처리됨
        // 원하면 시작 시 바로 발동하려면 아래 주석 해제
        // foreach (var s in skills) s.currentCooldown = 0f;
        // 동시 발동을 피하고 싶으면 초기 위상 랜덤화도 가능:
        // foreach (var s in skills) s.currentCooldown = Random.Range(0f, s.cooldownTime);
    }

    void Update()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            var s = skills[i];
            s.UpdateCooldown();

            // 쿨타임이 끝났고 자동시전이면 즉시 발동
            if (s.autoCast && !s.isOnCooldown)
                UseSkill(i);
        }

        // 수동 입력이 필요 없으니 테스트 입력은 제거/주석 처리
        // if (Input.GetKeyDown(KeyCode.Alpha1)) UseSkill(0);
    }

    // (원하면 UI에서 수동 호출도 가능)
    public void UseSkill0() { UseSkill(0); }
    public void UseSkill1() { UseSkill(1); }
    public void UseSkill2() { UseSkill(2); }
    public void UseSkill3() { UseSkill(3); }
    public void UseSkill4() { UseSkill(4); }

    public void UseSkill(int skillIndex)
    {
        if (skills == null || skillIndex < 0 || skillIndex >= skills.Length) { Debug.LogWarning("잘못된 스킬 인덱스"); return; }
        var s = skills[skillIndex];
        if (s.isOnCooldown) return;

        var today = weatherSystem ? weatherSystem.Today : default;
        float attackPower = s.GetAttackPower(today);

        ExecuteSkill(s, attackPower);
        s.ResetCooldown(); // 다시 쿨타임 시작
    }

    void ExecuteSkill(Skill s, float power)
    {
        switch (s.attackMode)
        {
            case AttackMode.Melee: ExecuteMelee(s, power); break;
            case AttackMode.Ranged: ExecuteRanged(s, power); break;
            case AttackMode.AreaOfEffect: ExecuteAOE(s, power); break;
        }
        SpawnEffectAt(attackOrigin.position, s.effectPrefab);
        Debug.Log($"Auto-cast {s.skillName} ({s.skillType}/{s.attackMode}) power={power}");
    }

    // ───────────── 타격 페이로드 ─────────────
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
            case SkillType.Wind:
                break;
        }

        var rb2 = enemy.GetComponent<Rigidbody2D>();
        if (rb2) rb2.AddForce(dir * payload.knockback, ForceMode2D.Impulse);
        var rb3 = enemy.GetComponent<Rigidbody>();
        if (rb3) rb3.AddForce(dir * payload.knockback, ForceMode.Impulse);

        enemy.SendMessage("OnHit", payload, SendMessageOptions.DontRequireReceiver);
        SpawnEffectAt(enemy.transform.position, s.effectPrefab);
    }

    // ───────────── 공격 구현 ─────────────
    void ExecuteMelee(Skill s, float power)
    {
        Vector3 o = attackOrigin.position;
        var hits2D = Physics2D.OverlapCircleAll(o, s.meleeRange, enemyMask);
        foreach (var h in hits2D)
        {
            var go = h.attachedRigidbody ? h.attachedRigidbody.gameObject : h.gameObject;
            ApplyHit(go, s, power, o);
        }
        var hits3D = Physics.OverlapSphere(o, s.meleeRange, enemyMask);
        foreach (var c in hits3D) ApplyHit(c.gameObject, s, power, o);
    }

    void ExecuteRanged(Skill s, float power)
    {
        Vector3 o = attackOrigin.position;
        Vector3 dir = attackOrigin.right;
        var hit2D = Physics2D.Raycast(o, dir, s.rangedDistance, enemyMask);
        if (hit2D.collider)
        {
            var go = hit2D.rigidbody ? hit2D.rigidbody.gameObject : hit2D.collider.gameObject;
            ApplyHit(go, s, power, o);
            return;
        }
        if (Physics.Raycast(o, dir, out RaycastHit hit3D, s.rangedDistance, enemyMask))
            ApplyHit(hit3D.collider.gameObject, s, power, o);
    }

    void ExecuteAOE(Skill s, float power)
    {
        Vector3 o = attackOrigin.position;
        var hits2D = Physics2D.OverlapCircleAll(o, s.aoeRadius, enemyMask);
        foreach (var h in hits2D)
        {
            var go = h.attachedRigidbody ? h.attachedRigidbody.gameObject : h.gameObject;
            ApplyHit(go, s, power, o);
        }
        var hits3D = Physics.OverlapSphere(o, s.aoeRadius, enemyMask);
        foreach (var c in hits3D) ApplyHit(c.gameObject, s, power, o);
    }

    void SpawnEffectAt(Vector3 pos, GameObject prefab)
    {
        if (!prefab) return;
        var go = Instantiate(prefab, pos, Quaternion.identity);

        // 파티클이면 재생 길이에 맞춰 제거, 아니면 0.5초 후 제거
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

    void OnDrawGizmosSelected()
    {
        if (!attackOrigin) return;
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(attackOrigin.position, 1.6f);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(attackOrigin.position, 2.8f);
        Gizmos.color = Color.white; Gizmos.DrawLine(attackOrigin.position, attackOrigin.position + attackOrigin.right * 10f);
    }
}