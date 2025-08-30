using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;     // m/s
    public float stopDamp = 0.9f;    // 입력 없을 때 감쇠

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;
    Animator anim;
    AudioSource audioSource;

    Vector2 input;            // -1..1
    Vector2 computedVelocity; // 우리가 계산한 ‘실제’ 속도
    Vector2 lastPos;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();

        // 탑다운 기본 세팅
        rigid.gravityScale = 0f;
        rigid.freezeRotation = true;
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        lastPos = rigid.position;
    }

    void Update()
    {
        // 입력
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        input = new Vector2(h, v).normalized;

        // 방향 스프라이트 (좌우만)
        if (h != 0) spriteRenderer.flipX = h < 0;

        // 애니메이션 (계산된 속도 기반)
        if (anim)
        {
            anim.SetBool("isWalking", computedVelocity.magnitude > 0.3f);
            // 필요하면 파라미터 더:
            // anim.SetFloat("Speed", computedVelocity.magnitude);
            // anim.SetFloat("VelX", computedVelocity.x);
            // anim.SetFloat("VelY", computedVelocity.y);
        }
    }

    void FixedUpdate()
    {
        // 목표 이동량
        Vector2 desired = input * moveSpeed * Time.fixedDeltaTime;

        // 입력 없을 때 살짝 감쇠(미끄럼 줄이기)
        if (input.sqrMagnitude < 0.001f)
            desired *= stopDamp;

        // 실제 이동 (물리 충돌 OK)
        Vector2 next = rigid.position + desired;
        rigid.MovePosition(next);

        // ‘실제 속도’ 계산해서 저장 (애니/로직에서 사용)
        computedVelocity = (rigid.position - lastPos) / Time.fixedDeltaTime;
        lastPos = rigid.position;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Portal"))
        {
            switch (other.name)  // 포탈 오브젝트 이름으로 분기
            {
                case "Portal_East":
                    SceneManager.LoadScene("Dungeon");
                    break;
            }
        }
    }
}

