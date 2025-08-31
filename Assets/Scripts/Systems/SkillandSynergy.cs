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

<<<<<<< HEAD
=======
        [Header("Level (0=Off, 1~5)")]
        [Range(0, 5)] public int level = 1;                 // 0=ºñÈ°¼º, 1~5 »ç¿ë
        [Tooltip("·¹º§´ç ¹èÀ² °¡»ê(¿¹: 0.15 = +15%/·¹º§)")]
        public float perLevelBonus = 0.15f;                 // (1 + perLevelBonus*(level-1))

>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
        [Header("Type")]
        public SkillType skillType;            // BasicAttack, Fire, Water, Ice, Wind
        public AttackMode attackMode;          // Melee, Ranged, AreaOfEffect

        [Header("Effect")]
        public GameObject effectPrefab;
        public bool spawnOnCast = false;       // ½ÃÀüÀÚ À§Ä¡ ÀÌÆåÆ®
        public bool spawnOnHit = true;         // ÇÇ°İ ÁöÁ¡ ÀÌÆåÆ®

<<<<<<< HEAD
=======
        [HideInInspector] public int lastCastFrame = -1;    // ÇÑ ÇÁ·¹ÀÓ Áßº¹ Ä³½ºÆ® ¹æÁö

>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
        [Header("Ranges")]
        public float meleeRange = 1.6f;
        public float aoeRadius = 2.8f;
        public float rangedDistance = 10f;

        [Header("Auto Cast")]
<<<<<<< HEAD
        public bool autoCast = true;           // ì¿¨íƒ€ì„ ëë‚˜ë©´ ìë™ ë°œë™

        // ì‹œë„ˆì§€ ì ìš©(ë§¤ì¹­ ë‚ ì”¨ì—ì„œ x1.2)
        public float GetAttackPower(WeatherType today)
        {
            float attackPower = baseAttackPower;
            const float synergyMultiplier = 1.2f;
=======
        public bool autoCast = true;           // ÄğÅ¸ÀÓ ³¡³ª¸é ÀÚµ¿ ¹ßµ¿

        // ½Ã³ÊÁö+·¹º§ ¹èÀ² Àû¿ë
        public float GetAttackPower(WeatherType today)
        {
            float levelMul = (level <= 0) ? 0f : 1f + perLevelBonus * (level - 1);
            float attackPower = baseAttackPower * levelMul;
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)

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
<<<<<<< HEAD
        weatherSystem = FindObjectOfType<WeatherSystem>(); // ì—†ì–´ë„ null-safe ì²˜ë¦¬ë¨
        // ì›í•˜ë©´ ì‹œì‘ ì‹œ ë°”ë¡œ ë°œë™í•˜ë ¤ë©´ ì•„ë˜ ì£¼ì„ í•´ì œ
        // foreach (var s in skills) s.currentCooldown = 0f;
        // ë™ì‹œ ë°œë™ì„ í”¼í•˜ê³  ì‹¶ìœ¼ë©´ ì´ˆê¸° ìœ„ìƒ ëœë¤í™”ë„ ê°€ëŠ¥:
=======
        weatherSystem = FindObjectOfType<WeatherSystem>(); // ¾ø¾îµµ null-safe Ã³¸®µÊ
        // ½ÃÀÛ À§»ó ·£´ıÈ­ ¿¹½Ã:
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
        // foreach (var s in skills) s.currentCooldown = Random.Range(0f, s.cooldownTime);
    }

    void Update()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            var s = skills[i];
            s.UpdateCooldown();

<<<<<<< HEAD
            // ì¿¨íƒ€ì„ì´ ëë‚¬ê³  ìë™ì‹œì „ì´ë©´ ì¦‰ì‹œ ë°œë™
=======
            // 0·¹º§Àº ºñÈ°¼º
            if (s.level <= 0) continue;

            // ÄğÅ¸ÀÓÀÌ ³¡³µ°í ÀÚµ¿½ÃÀüÀÌ¸é Áï½Ã ¹ßµ¿
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
            if (s.autoCast && !s.isOnCooldown)
                UseSkill(i);
        }
    }

    // (¿øÇÏ¸é UI¿¡¼­ ¼öµ¿ È£Ãâµµ °¡´É)
    public void UseSkill0() { UseSkill(0); }
    public void UseSkill1() { UseSkill(1); }
    public void UseSkill2() { UseSkill(2); }
    public void UseSkill3() { UseSkill(3); }
    public void UseSkill4() { UseSkill(4); }

    public void UseSkill(int skillIndex)
    {
<<<<<<< HEAD
        if (skills == null || skillIndex < 0 || skillIndex >= skills.Length) { Debug.LogWarning("ì˜ëª»ëœ ìŠ¤í‚¬ ì¸ë±ìŠ¤"); return; }
        var s = skills[skillIndex];
        if (s.isOnCooldown) return;

        var today = weatherSystem ? weatherSystem.Today : default;
        float attackPower = s.GetAttackPower(today);
=======
        if (skills == null || skillIndex < 0 || skillIndex >= skills.Length) { Debug.LogWarning("Àß¸øµÈ ½ºÅ³ ÀÎµ¦½º"); return; }
        var s = skills[skillIndex];

        // 0·¹º§ ¶Ç´Â ÄğÅ¸ÀÓ ÁßÀÌ¸é ½ºÅµ
        if (s.level <= 0 || s.isOnCooldown) return;

        // °°Àº ÇÁ·¹ÀÓ Áßº¹ È£Ãâ ¹æÁö
        if (s.lastCastFrame == Time.frameCount) return;

        var today = weatherSystem ? weatherSystem.Today : default;
        float attackPower = s.GetAttackPower(today);
        if (attackPower <= 0f) return; // ·¹º§ 0 µî
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)

        ExecuteSkill(s, attackPower);
        s.ResetCooldown(); // ë‹¤ì‹œ ì¿¨íƒ€ì„ ì‹œì‘
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

<<<<<<< HEAD
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ íƒ€ê²© í˜ì´ë¡œë“œ â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
=======
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Å¸°İ ÆäÀÌ·Îµå ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
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

<<<<<<< HEAD
=======
        // ¿ø¼Ò º¸Á¤(µ¥¹ÌÁö´Â ÀÌ¹Ì ·¹º§ ¹İ¿µµÊ)
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
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

<<<<<<< HEAD
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ ê³µê²© êµ¬í˜„ â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
=======
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ °ø°İ ±¸Çö ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
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
<<<<<<< HEAD
        var hit2D = Physics2D.Raycast(o, dir, s.rangedDistance, enemyMask);
        if (hit2D.collider)
=======
        float skin = 0.1f;                         // ÀÚ±â Äİ¶óÀÌ´õ »ìÂ¦ ¾Õ¿¡¼­ ¹ß»ç
        Vector3 start = o + dir * skin;

        var hits2D = Physics2D.RaycastAll(start, dir, s.rangedDistance - skin, enemyMask);
        foreach (var h in hits2D)
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
        {
            var go = hit2D.rigidbody ? hit2D.rigidbody.gameObject : hit2D.collider.gameObject;
            ApplyHit(go, s, power, o);
            return;
        }
        if (Physics.Raycast(o, dir, out RaycastHit hit3D, s.rangedDistance, enemyMask))
            ApplyHit(hit3D.collider.gameObject, s, power, o);
    }

<<<<<<< HEAD
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

=======
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
    void SpawnEffectAt(Vector3 pos, GameObject prefab)
    {
        if (!prefab) return;
        var go = Instantiate(prefab, pos, Quaternion.identity);

<<<<<<< HEAD
        // íŒŒí‹°í´ì´ë©´ ì¬ìƒ ê¸¸ì´ì— ë§ì¶° ì œê±°, ì•„ë‹ˆë©´ 0.5ì´ˆ í›„ ì œê±°
=======
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
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
<<<<<<< HEAD
=======
    }

    // ÀÚ±â ÀÚ½Å È÷Æ® ¹æÁö
    bool IsSelf(GameObject go)
    {
        if (!go) return false;
        return go == gameObject || go.transform.root == transform.root;
>>>>>>> parent of 6c4798a (Merge branch 'main' into SkillAttack)
    }

    void OnDrawGizmosSelected()
    {
        if (!attackOrigin) return;
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(attackOrigin.position, 1.6f);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(attackOrigin.position, 2.8f);
        Gizmos.color = Color.white; Gizmos.DrawLine(attackOrigin.position, attackOrigin.position + attackOrigin.right * 10f);
    }
}