using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat instance;

    public float maxHealth;   // 최대체력
    public float health;      // 현재체력
    public float healthRegen; // 초당 체력재생
    public float speed;       // 이동 속도
    public float power;       // 공격력 배수

    public bool isDead;       // 사망 여부

    void Awake()
    {
        instance = this;
        ResetStat();
    }

    void Update()
    {
        if (isDead) return;

        // 체력 재생
        if (healthRegen > 0f && health < maxHealth)
        {
            health += healthRegen * Time.deltaTime;
            if (health > maxHealth) health = maxHealth;
        }

        // 사망 판정
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
        // 필요하면 여기서 플레이어 조작/공격 컴포넌트 비활성화 처리
        // var mv = GetComponent<PlayerMovement>(); if (mv) mv.enabled = false;
        // var atk = GetComponent<PlayerAttack>();  if (atk) atk.enabled = false;
        // 게임 오버 연출/씬 전환은 GameManager 등에서 isDead를 확인해 처리
    }
}