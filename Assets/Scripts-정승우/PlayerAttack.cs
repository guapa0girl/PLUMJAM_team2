using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attack = 8f;        // 기본 공격력
    public float attackDelay = 0.5f; // 공격 간격(초)
    private float nextAtk;           // 다음 공격 시각

    [Header("Hitbox (rectangle in front)")]
    public float length = 2.0f;      // 전방 길이
    public float width = 1.2f;      // 좌우 폭
    public float forwardOffset = 0f; // 0이면 length * 0.5 자동
    public string enemyTag = "Enemy"; // (선택) 루트 오브젝트 태그로 검사

    [Header("Attack Sprite (옵션)")]
    public GameObject attackSpritePrefab; // 직사각형 범위를 보여줄 스프라이트 프리팹
    public float spriteLifetime = 0.06f;  // 표시 시간
    public bool scaleSpriteToHitbox = true; // 히트박스 크기에 맞게 스케일
    public float spriteLengthScale = 1f;    // length에 대한 배율
    public float spriteWidthScale = 1f;    // width에 대한 배율
    public Vector3 spriteExtraOffset = Vector3.zero; // 미세 위치 보정(필요 시)

    void FixedUpdate()
    {
        if (Time.time >= nextAtk)
        {
            AttackRect();
            nextAtk = Time.time + attackDelay;
        }
    }

    void AttackRect()
    {
        // 전방 = transform.right 기준(고정)
        Vector2 fwd = transform.right;
        if (fwd.sqrMagnitude < 1e-4f) fwd = Vector2.right;

        Vector2 center = (Vector2)transform.position + fwd * (forwardOffset != 0f ? forwardOffset : length * 0.5f);
        Vector2 size = new Vector2(length, width);
        float angleDeg = transform.eulerAngles.z;

        // 1) 데미지 판정 (중복 Enemy 처리 방지용 집합)
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angleDeg);
        float dmg = attack * (PlayerStat.instance ? PlayerStat.instance.power : 1f);

        var processed = new HashSet<Enemy>();

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (!col) continue;

            // ★ 자식/부모 어디에 Enemy가 있어도 찾음
            var e = col.GetComponent<Enemy>()
                 ?? col.GetComponentInParent<Enemy>()
                 ?? col.GetComponentInChildren<Enemy>();
            if (e == null) continue;

            // (선택) 태그 검사: Enemy 스크립트가 붙은 오브젝트(루트)에 대해 검사
            if (!string.IsNullOrEmpty(enemyTag) && !e.gameObject.CompareTag(enemyTag))
                continue;

            if (processed.Contains(e)) continue; // 같은 Enemy 여러 콜라이더 중복 타격 방지
            processed.Add(e);

            e.health -= dmg;
            if (e.health <= 0f)
            {
                e.health = 0f;

                // ★ 씨앗 드랍 보장: 파괴 전에 드랍 시도
                var table = e.GetComponent<SeedDropperTable>()
                          ?? e.GetComponentInChildren<SeedDropperTable>()
                          ?? e.GetComponentInParent<SeedDropperTable>();
                if (table) table.TryDrop();

                Destroy(e.gameObject);
            }
        }

        // 2) 스프라이트 표시(옵션)
        if (attackSpritePrefab)
        {
            Vector3 pos = (Vector3)center + spriteExtraOffset;
            Quaternion rot = Quaternion.Euler(0f, 0f, angleDeg);
            var go = Object.Instantiate(attackSpritePrefab, pos, rot);

            if (scaleSpriteToHitbox)
                go.transform.localScale = new Vector3(length * spriteLengthScale, width * spriteWidthScale, 1f);

            if (spriteLifetime > 0f) Object.Destroy(go, spriteLifetime);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector2 fwd = (Vector2)transform.right;
        if (fwd.sqrMagnitude < 1e-4f) fwd = Vector2.right;

        Vector2 center = (Vector2)transform.position + fwd * (forwardOffset != 0f ? forwardOffset : length * 0.5f);
        Vector2 size   = new Vector2(length, width);
        float angleDeg = transform.eulerAngles.z;

        Gizmos.color = Color.white;
        var old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0,0,angleDeg), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, 0.1f));
        Gizmos.matrix = old;
    }
#endif
}