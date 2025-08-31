using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MaxHealth;
    public float health;
    public float speed;
    public float attack;
    public float attackDelay;   // ���� ����
    private float nextAtk;      // ���� ���� Ÿ�̹�
    public PlayerStat player;

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
        // �÷��̾� �������� �̵�
        Vector2 dir = (player.transform.position - transform.position).normalized;
        transform.Translate(dir * speed * Time.fixedDeltaTime);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Attack();
        }
    }
        
    void Attack()
    {
        if (nextAtk <= Time.time)
        {

            // ������ ������ Ÿ�̹��� ��� ����
            if (player != null)
            {
                Debug.Log("���� �����");
                player.health -= attack;                 // �÷��̾�� ������
                if (player.health < 0f) player.health = 0f; // (���� Ŭ����)
            }
            nextAtk = Time.time + attackDelay;

        }

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