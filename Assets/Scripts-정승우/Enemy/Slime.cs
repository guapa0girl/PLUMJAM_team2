using UnityEngine;

public class Slime : MonoBehaviour
{
    private Animator animator;
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
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        Chase();
    }

    void Chase()
    {
        // �÷��̾� �������� �̵� (������ ����� ���)
        Vector2 dir = (player.transform.position - transform.position).normalized;

        // �ڻԼҰ� �ùٸ� �������� �̵��ϵ��� �ݿ�
        transform.Translate(dir * speed * Time.fixedDeltaTime);

        // ũ�� ����
        transform.localScale = new Vector3(3, 3, 1); // ũ�⸦ 3,3���� ����

        // �÷��̾� ���⿡ �´� �ִϸ��̼� ����
        if (dir.x > 0)
        {
            // ���������� �̵� ��
            animator.SetBool("isRunning", true);  // Run �ִϸ��̼� ����
            transform.localScale = new Vector3(-3, 3, 1); // ���������� ��������Ʈ ǥ��
        }
        else if (dir.x < 0)
        {
            // �������� �̵� ��
            animator.SetBool("isRunning", true);  // Run �ִϸ��̼� ����
            transform.localScale = new Vector3(3, 3, 1); // ���� �������� ��������Ʈ ����
        }
        else
        {
            // ���� �� (x ���� �̵� ����)
            animator.SetBool("isRunning", false);  // Idle �ִϸ��̼� ����
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
            // ������ ������ Ÿ�̹��� ��� ����
            if (player != null)
            {
                // ���� �ÿ��� Run �ִϸ��̼��� �����ϰ� ����
                animator.SetBool("isRunning", true);  // Run �ִϸ��̼� ����
                Debug.Log("���� �����");
                player.health -= attack;                 // �÷��̾�� ������
                if (player.health < 0f) player.health = 0f; // (���� Ŭ����)
            }
            nextAtk = Time.time + attackDelay;

            // ������ ���� �� �ٽ� Idle ���·� ��ȯ
            animator.SetBool("isRunning", false);  // ���� �� Idle ���·� ���ư���
        }
    }
}
