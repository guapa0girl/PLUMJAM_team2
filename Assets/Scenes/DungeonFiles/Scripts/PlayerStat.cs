using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat instance;
    // 다른 스크립트에서 접근하기 쉽도록 정적 instance 선언

    public float maxHealth;   // 최대체력
    public float health;      // 현재체력
    public float healthRegen; // 초당 체력재생
    public float speed;       // 플레이어 이동 속도
    public float power;       // 공격력 배수

    void Awake()
    {
        // 플레이어는 단 하나뿐이므로, 자신을 대표 instance로 설정
        instance = this;
        ResetStat();
    }

    void ResetStat()
    {
        maxHealth = 100;
        health = maxHealth;
        healthRegen = 1;
        speed = 3;
        power = 1;
    }
}