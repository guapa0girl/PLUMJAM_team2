using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    void FixedUpdate()
    {
        // 물리 연산과 싱크를 맞추기 위해 FixedUpdate 사용
        Move();
    }

    void Move()
    {
        float xInput = Input.GetAxisRaw("Horizontal"); // 가로 입력을 받아옴
        float yInput = Input.GetAxisRaw("Vertical");   // 세로 입력을 받아옴
        float spd = PlayerStat.instance.speed * Time.fixedDeltaTime; // 한 프레임당 이동 속도
        transform.Translate(new Vector2(xInput, yInput).normalized * spd);
    }
}