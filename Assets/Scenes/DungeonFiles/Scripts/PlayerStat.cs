using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat instance;
    // �ٸ� ��ũ��Ʈ���� �����ϱ� ������ ���� instance ����

    public float maxHealth;   // �ִ�ü��
    public float health;      // ����ü��
    public float healthRegen; // �ʴ� ü�����
    public float speed;       // �÷��̾� �̵� �ӵ�
    public float power;       // ���ݷ� ���

    void Awake()
    {
        // �÷��̾�� �� �ϳ����̹Ƿ�, �ڽ��� ��ǥ instance�� ����
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