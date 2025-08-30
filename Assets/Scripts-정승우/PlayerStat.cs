using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat instance;

    public float maxHealth;   // �ִ�ü��
    public float health;      // ����ü��
    public float healthRegen; // �ʴ� ü�����
    public float speed;       // �̵� �ӵ�
    public float power;       // ���ݷ� ���

    public bool isDead;       // ��� ����

    void Awake()
    {
        instance = this;
        ResetStat();
    }

    void Update()
    {
        if (isDead) return;

        // ü�� ���
        if (healthRegen > 0f && health < maxHealth)
        {
            health += healthRegen * Time.deltaTime;
            if (health > maxHealth) health = maxHealth;
        }

        // ��� ����
        if (health <= 0f) Die();
    }

    public void ResetStat()
    {
        maxHealth = 100;
        health = maxHealth;
        healthRegen = 1;
        speed = 3;
        power = 1;
        isDead = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;
        health -= amount;
        if (health < 0f) health = 0f;
        if (health <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f) return;
        health += amount;
        if (health > maxHealth) health = maxHealth;
    }

    void Die()
    {
        isDead = true;
        // �ʿ��ϸ� ���⼭ �÷��̾� ����/���� ������Ʈ ��Ȱ��ȭ ó��
        // var mv = GetComponent<PlayerMovement>(); if (mv) mv.enabled = false;
        // var atk = GetComponent<PlayerAttack>();  if (atk) atk.enabled = false;
        // ���� ���� ����/�� ��ȯ�� GameManager ��� isDead�� Ȯ���� ó��
    }
}