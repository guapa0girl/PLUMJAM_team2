using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MaxHealth;
    public float health;
    public float speed;
    public float attack;
    public float attackDelay;   // 공격 간격
    private float nextAtk;      // 다음 공격 타이밍
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
        // 플레이어 방향으로 이동
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
            // 공격이 가능한 타이밍일 경우 실행
            if (player != null)
            {
                player.health -= attack;                 // 플레이어에게 데미지
                if (player.health < 0f) player.health = 0f; // (간단 클램프)
            }
            nextAtk = Time.time + attackDelay;
        }
    }
}