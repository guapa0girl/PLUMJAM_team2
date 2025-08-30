using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    void FixedUpdate()
    {
        // ���� ����� ��ũ�� ���߱� ���� FixedUpdate ���
        Move();
    }

    void Move()
    {
        float xInput = Input.GetAxisRaw("Horizontal"); // ���� �Է��� �޾ƿ�
        float yInput = Input.GetAxisRaw("Vertical");   // ���� �Է��� �޾ƿ�
        float spd = PlayerStat.instance.speed * Time.fixedDeltaTime; // �� �����Ӵ� �̵� �ӵ�
        transform.Translate(new Vector2(xInput, yInput).normalized * spd);
    }
}