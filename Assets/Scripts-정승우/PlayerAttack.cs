using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attack = 8f;        // �⺻ ���ݷ�
    public float attackDelay = 0.5f; // ���� ����(��)
    private float nextAtk;           // ���� ���� �ð�

    [Header("Hitbox (rectangle in front)")]
    public float length = 2.0f;      // ���� ����
    public float width = 1.2f;      // �¿� ��
    public float forwardOffset = 0f; // 0�̸� length * 0.5 �ڵ�
    public string enemyTag = "Enemy"; // (����) ��Ʈ ������Ʈ �±׷� �˻�

    [Header("Attack Sprite (�ɼ�)")]
    public GameObject attackSpritePrefab; // ���簢�� ������ ������ ��������Ʈ ������
    public float spriteLifetime = 0.06f;  // ǥ�� �ð�
    public bool scaleSpriteToHitbox = true; // ��Ʈ�ڽ� ũ�⿡ �°� ������
    public float spriteLengthScale = 1f;    // length�� ���� ����
    public float spriteWidthScale = 1f;    // width�� ���� ����
    public Vector3 spriteExtraOffset = Vector3.zero; // �̼� ��ġ ����(�ʿ� ��)

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
        // ���� = transform.right ����(����)
        Vector2 fwd = transform.right;
        if (fwd.sqrMagnitude < 1e-4f) fwd = Vector2.right;

        Vector2 center = (Vector2)transform.position + fwd * (forwardOffset != 0f ? forwardOffset : length * 0.5f);
        Vector2 size = new Vector2(length, width);
        float angleDeg = transform.eulerAngles.z;

        // 1) ������ ���� (�ߺ� Enemy ó�� ������ ����)
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angleDeg);
        float dmg = attack * (PlayerStat.instance ? PlayerStat.instance.power : 1f);

        var processed = new HashSet<Enemy>();

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (!col) continue;

            // �� �ڽ�/�θ� ��� Enemy�� �־ ã��
            var e = col.GetComponent<Enemy>()
                 ?? col.GetComponentInParent<Enemy>()
                 ?? col.GetComponentInChildren<Enemy>();
            if (e == null) continue;

            // (����) �±� �˻�: Enemy ��ũ��Ʈ�� ���� ������Ʈ(��Ʈ)�� ���� �˻�
            if (!string.IsNullOrEmpty(enemyTag) && !e.gameObject.CompareTag(enemyTag))
                continue;

            if (processed.Contains(e)) continue; // ���� Enemy ���� �ݶ��̴� �ߺ� Ÿ�� ����
            processed.Add(e);

            e.health -= dmg;
            if (e.health <= 0f)
            {
                e.health = 0f;

                // �� ���� ��� ����: �ı� ���� ��� �õ�
                var table = e.GetComponent<SeedDropperTable>()
                          ?? e.GetComponentInChildren<SeedDropperTable>()
                          ?? e.GetComponentInParent<SeedDropperTable>();
                if (table) table.TryDrop();

                Destroy(e.gameObject);
            }
        }

        // 2) ��������Ʈ ǥ��(�ɼ�)
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