using UnityEngine;

public class Slime : MonoBehaviour
{
    private Animator animator;
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
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        Chase();
    }

    void Chase()
    {
        // 플레이어 방향으로 이동 (방향을 제대로 계산)
        Vector2 dir = (player.transform.position - transform.position).normalized;

        // 코뿔소가 올바른 방향으로 이동하도록 반영
        transform.Translate(dir * speed * Time.fixedDeltaTime);

        // 크기 설정
        transform.localScale = new Vector3(3, 3, 1); // 크기를 3,3으로 설정

        // 플레이어 방향에 맞는 애니메이션 설정
        if (dir.x > 0)
        {
            // 오른쪽으로 이동 중
            animator.SetBool("isRunning", true);  // Run 애니메이션 시작
            transform.localScale = new Vector3(-3, 3, 1); // 오른쪽으로 스프라이트 표시
        }
        else if (dir.x < 0)
        {
            // 왼쪽으로 이동 중
            animator.SetBool("isRunning", true);  // Run 애니메이션 시작
            transform.localScale = new Vector3(3, 3, 1); // 왼쪽 방향으로 스프라이트 반전
        }
        else
        {
            // 정지 중 (x 방향 이동 없음)
            animator.SetBool("isRunning", false);  // Idle 애니메이션 시작
        }
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
                // 공격 시에는 Run 애니메이션을 실행하고 돌진
                animator.SetBool("isRunning", true);  // Run 애니메이션 시작
                Debug.Log("공격 실행됨");
                player.health -= attack;                 // 플레이어에게 데미지
                if (player.health < 0f) player.health = 0f; // (간단 클램프)
            }
            nextAtk = Time.time + attackDelay;

            // 공격이 끝난 후 다시 Idle 상태로 전환
            animator.SetBool("isRunning", false);  // 공격 후 Idle 상태로 돌아가기
        }
    }
}
