using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Data;    // SeedDef
using Game.Core;    // Inventory
using Game.Systems; // FarmingSystem

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;     // m/s
    public float stopDamp = 0.9f;    // 입력 없을 때 감쇠

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;
    Animator anim;
    AudioSource audioSource;

    Vector2 input;
    Vector2 computedVelocity;
    Vector2 lastPos;

    // =====================[BATTLE]=======================
    public int battleCount = 0;     // 오늘 전투 횟수
    public int maxBattles = 2;      // 하루 최대 전투 횟수

    // ===================== [FARMING] =====================
    [Header("Farming (씬에 없어도 런타임에 자동 생성됩니다)")]
    public FarmingSystem farming;   // null이면 Awake에서 _FarmingSystem 생성
    public Inventory inventory;     // null이면 Awake에서 _Inventory 생성

    [Header("SeedDefs (없으면 런타임에 CreateInstance로 자동 생성)")]
    public SeedDef seedSun;   // preferred: Heat
    public SeedDef seedCloud; // Cloud
    public SeedDef seedRain;  // Rain
    public SeedDef seedSnow;  // Snow

    int currentPlot = -1;

    // ===================== [UNITY LIFECYCLE] =====================
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();

        // 탑다운 기본 물리 세팅
        rigid.gravityScale = 0f;
        rigid.freezeRotation = true;
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        lastPos = rigid.position;

        // --- FarmingSystem/Inventory 없으면 자동 생성 ---
        if (farming == null)
        {
            var go = new GameObject("_FarmingSystem");
            farming = go.AddComponent<FarmingSystem>();
        }
        if (inventory == null)
        {
            var go = new GameObject("_Inventory");
            inventory = go.AddComponent<Inventory>();
        }

        // --- SeedDef 없으면 런타임 생성 ---
        if (seedSun == null) seedSun = MakeSeed("Seed_Sun", WeatherType.Heat, 10);
        if (seedCloud == null) seedCloud = MakeSeed("Seed_Cloud", WeatherType.Cloud, 10);
        if (seedRain == null) seedRain = MakeSeed("Seed_Rain", WeatherType.Rain, 10);
        if (seedSnow == null) seedSnow = MakeSeed("Seed_Snow", WeatherType.Snow, 10);

        // --- Inventory와 SeedDef 연결 ---
        if (inventory)
        {
            inventory.seedSun = seedSun;
            inventory.seedCloud = seedCloud;
            inventory.seedRain = seedRain;
            inventory.seedSnow = seedSnow;
        }
    }

    void Update()
    {
        // 입력
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        input = new Vector2(h, v).normalized;

        // 방향 스프라이트 (좌우만)
        if (spriteRenderer && h != 0) spriteRenderer.flipX = h < 0;

        // 애니메이션
        if (anim)
        {
            anim.SetBool("isWalking", computedVelocity.magnitude > 0.3f);
        }
    }

    void FixedUpdate()
    {
        // 이동
        Vector2 desired = input * moveSpeed * Time.fixedDeltaTime;
        if (input.sqrMagnitude < 0.001f) desired *= stopDamp;

        rigid.MovePosition(rigid.position + desired);

        // 속도 계산
        computedVelocity = (rigid.position - lastPos) / Time.fixedDeltaTime;
        lastPos = rigid.position;
    }

    // ===================== [TRIGGER] =====================
    void OnTriggerEnter2D(Collider2D other)
    {
        // 포탈
        if (other.CompareTag("Portal"))
        {
            if (battleCount < maxBattles)
            {
                battleCount++;
                switch (other.name)
                {
                    case "Portal_East":
                        Game.Core.SceneBridge.GoToUpgrade("dungeon_rain"); // ← 업그레이드 씬으로 먼저

                        break;
                }
            }
            else
            {
                Debug.Log("오늘 전투 횟수 초과! 포탈 작동 불가");
            }
        }

        // 플롯
        if (other.CompareTag("Plot"))
        {
            // plot 번호는 이름으로 판별 (예: Plot1 → index=0)
            if (other.name.Contains("1")) currentPlot = 0;
            else if (other.name.Contains("2")) currentPlot = 1;
            else if (other.name.Contains("3")) currentPlot = 2;
            else if (other.name.Contains("4")) currentPlot = 3;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Plot")) currentPlot = -1;
    }

    // ===================== [PLANT UI BUTTONS] =====================
    public void OnClick_PlantSun() => TryPlantInternal(seedSun);
    public void OnClick_PlantCloud() => TryPlantInternal(seedCloud);
    public void OnClick_PlantRain() => TryPlantInternal(seedRain);
    public void OnClick_PlantSnow() => TryPlantInternal(seedSnow);

    void TryPlantInternal(SeedDef def)
    {
        if (currentPlot < 0) { Debug.Log("플롯 위에 서 있지 않음"); return; }
        if (farming == null || inventory == null) { Debug.LogWarning("Farming/Inventory 참조 누락"); return; }
        if (def == null) { Debug.LogWarning("SeedDef 누락"); return; }

        bool ok = farming.TryPlant(inventory, def, currentPlot);
        if (!ok)
        {
            Debug.Log("심기 실패: 재고 부족 혹은 이미 심김");
            return;
        }

        Debug.Log($"[Farming] plot {currentPlot} ← {def.name}");
    }

    // ===================== [RUNTIME SEED CREATION] =====================
    SeedDef MakeSeed(string name, WeatherType preferred, int sellPrice)
    {
        var def = ScriptableObject.CreateInstance<SeedDef>();
        def.name = name;
        def.preferred = preferred;
        def.sellPrice = sellPrice;
        return def;
    }
}

