using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float MaxHealth = 100f;
    public float health = 100f;
    public float speed = 2f;
    public float attack = 10f;
    public float attackDelay = 0.8f;

    [Header("Refs")]
    public PlayerStat player;

    [Header("Knockback")]
    [SerializeField] private float knockbackStopTime = 0.15f; // 넉백 락 시간
    private bool knockbackLock = false;

    // 내부 상태
    private float nextAtk = 0f;
    private Rigidbody2D rb2d;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (player == null) player = PlayerStat.instance;
    }

    void FixedUpdate()
    {
        if (!knockbackLock) Chase();
    }

    void Chase()
    {
        if (player == null) return;

        Vector2 dir = (player.transform.position - transform.position).normalized;

        // 물리 일관성: Rigidbody2D 이동 우선
        if (rb2d != null)
            rb2d.MovePosition(rb2d.position + dir * speed * Time.fixedDeltaTime);
        else
            transform.Translate(dir * speed * Time.fixedDeltaTime);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            Attack();
    }

    void Attack()
    {
        if (player == null) return;

        if (Time.time >= nextAtk)
        {
            // Debug.Log("공격");
            player.health -= attack;
            if (player.health < 0f) player.health = 0f;

            nextAtk = Time.time + attackDelay;
        }
    }

    // SkillandSynergy에서 넘겨주는 피격 데이터 사용
    public void OnHit(SkillandSynergy.HitPayload p)
    {
        health -= p.damage;
        if (health <= 0f) { Destroy(gameObject); return; }

        // 넉백
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero; // 즉시 정지
            rb2d.AddForce(p.hitDirection * p.knockback, ForceMode2D.Impulse);
        }

        StopAllCoroutines();
        StartCoroutine(CoKnockbackLock());
    }

    IEnumerator CoKnockbackLock()
    {
        knockbackLock = true;
        yield return new WaitForSeconds(knockbackStopTime);
        knockbackLock = false;
    }
}
