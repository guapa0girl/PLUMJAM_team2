using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Devil : MonoBehaviour
{
    // Components
    private Animator animator;
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    // Stats
    public float MaxHealth = 100f;
    public float health = 100f;

    // Movement
    [Header("Speeds")]
    public float idleWalkSpeed = 1.2f;   // Idle �� õõ�� �ȱ� �ӵ�
    public float runDashSpeed = 6.0f;    // Run �� ��� �ӵ�
    public float runTurnAssist = 10f;    // Run �� ��ǥ �������� ȸ�� ����(���� ����)
    public float idleSmooth = 6f;        // Idle �� �ӵ� ����(�ε巴��)

    // Pattern (Idle 5s -> Run 2s �ݺ�)
    [Header("Pattern")]
    public float idleDuration = 5f;
    public float runDuration = 2f;
    private bool isRunning = false;

    // Attack
    [Header("Attack")]
    public float attack = 5f;            // �⺻ ������
    public float attackRunBonus = 0f;    // Run�� �� �߰� ������(�ʿ� ������ 0)
    public float attackDelay = 0.5f;     // ���� �� ������ ����
    private float nextAtk;

    // Target
    public PlayerStat player;

    // Visual
    [Header("Visual")]
    public Vector3 fixedScale = new Vector3(3f, 3f, 1f); // �׻� ������ ũ��

    void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // �ݵ�� PlayerStat �̱����� �غ�� �־�� �մϴ�.
        player = PlayerStat.instance;

        // Rigidbody2D ���� ����
        // Body Type: Dynamic
        // Gravity Scale: 0
        // Linear Drag: 0~0.5 (���� ���� ���ϸ� 0~0.2 ����)
        // Interpolate: Interpolate (�ε巴��)
        // Collision: �� �ݶ��̴��� IsTrigger üũ (OnTriggerStay2D ����)
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        transform.localScale = fixedScale;

        // Idle 5�� �� Run 2�� �ݺ� �ڷ�ƾ ����
        StartCoroutine(RunIdleLoop());
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // ���� ��ǥ ����
        Vector2 toPlayer = (player.transform.position - transform.position);
        Vector2 dir = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.zero;

        // ��������Ʈ �¿� ���� (�⺻ ������ �ٶ󺸴� ��������Ʈ ����)
        if (sr != null) sr.flipX = (dir.x > 0f);

        if (isRunning)
        {
            // RUN ����: ���������� �޸��� (���� ����)
            // ��ǥ �ӵ��� �ε巴�� ���� -> ������ �����鼭�� ��ǥ�� ����
            Vector2 targetVel = dir * runDashSpeed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVel, runTurnAssist * Time.fixedDeltaTime);
        }
        else
        {
            // IDLE ����: õõ�� �ȱ� (���� ����, ���� �����ϵ� �� �ΰ�)
            Vector2 targetVel = dir * idleWalkSpeed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVel, idleSmooth * Time.fixedDeltaTime);
        }

        // �ִϸ����� �Ķ���� ����ȭ
        if (animator != null) animator.SetBool("isRunning", isRunning);

        // �ð��� ũ�� ����(�ܺο��� �ǵ���� ����)
        if (transform.localScale != fixedScale) transform.localScale = fixedScale;
    }

    IEnumerator RunIdleLoop()
    {
        while (true)
        {
            // Idle
            isRunning = false;
            yield return new WaitForSeconds(idleDuration);

            // Run
            isRunning = true;
            yield return new WaitForSeconds(runDuration);
        }
    }

    // �÷��̾�� ��� ������ ���¿� ���� ���� ����
    void OnTriggerStay2D(Collider2D col)
    {
        if (player == null) return;
        if (!col.CompareTag("Player")) return;

        if (Time.time >= nextAtk)
        {
            float dmg = attack + (isRunning ? attackRunBonus : 0f);
            player.health -= dmg;
            if (player.health < 0f) player.health = 0f;

            nextAtk = Time.time + attackDelay;
        }
    }
}
