using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float MaxHealth = 50f;
    public float health = 50f;
    public float speed = 2f;
    public float attack = 5f;
    public float attackDelay = 1f;

    private float nextAtk;
    public PlayerStat player;

    Rigidbody2D rb2d;
    bool knockbackLock;              // �˹� �� �̵� ��� ����
    float knockbackStopTime = 0.15f; // �˹� ��� �ð�

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (health <= 0f && MaxHealth > 0f) health = MaxHealth;
        if (rb2d) { rb2d.gravityScale = 0f; rb2d.interpolation = RigidbodyInterpolation2D.Interpolate; }
    }

    void Start()
    {
        player = PlayerStat.instance;
    }

    void FixedUpdate()
    {
        Chase();
    }

    void Chase()
    {
        if (player == null || knockbackLock) return;

        Vector2 pos = rb2d ? rb2d.position : (Vector2)transform.position;
        Vector2 target = player.transform.position;
        Vector2 dir = (target - pos).normalized;

        if (rb2d)
            rb2d.MovePosition(pos + dir * speed * Time.fixedDeltaTime); // ���� ���� �̵�(����)
        else
            transform.Translate((Vector3)(dir * speed * Time.fixedDeltaTime), Space.World);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) Attack();
    }

    void Attack()
    {
        if (nextAtk > Time.time) return;
        if (player != null)
        {
            player.health -= attack;
            if (player.health < 0f) player.health = 0f;
        }
        nextAtk = Time.time + attackDelay;
    }

    // SkillandSynergy�� OnHit(payload)�� �޾Ƽ� ó��
    public void OnHit(SkillandSynergy.HitPayload p)
    {
        health -= p.damage;
        if (health <= 0f) { Destroy(gameObject); return; }

        // �˹� ���� + ��� �̵� ����
        if (rb2d)
        {
            rb2d.linearVelocity = Vector2.zero; // ���� ���� ����
            rb2d.AddForce(p.hitDirection * p.knockback, ForceMode2D.Impulse);
        }

        StopAllCoroutines();
        StartCoroutine(CoKnockbackLock());

        // (���ο�/����/��Ʈ ���� ������� �ʿ�� �߰�)
    }

    IEnumerator CoKnockbackLock()
    {
        knockbackLock = true;
        yield return new WaitForSeconds(knockbackStopTime);
        knockbackLock = false;
    }
}