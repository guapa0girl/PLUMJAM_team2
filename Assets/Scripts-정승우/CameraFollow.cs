using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                     // 추적할 오브젝트
    public Vector3 offset = new Vector3(0, 0, -10); // 목표와 벌어짐(보정치)
    public float cameraSpeed = 5f;

    void FixedUpdate()
    {
        Vector3 targetPos = target.position + offset; // 실제 목표 위치
                                                      // 현재 위치에서 목표지점로 부드럽게 이동 구현
        Vector3 resultPos = Vector3.Lerp(transform.position, targetPos, cameraSpeed * Time.fixedDeltaTime);
        transform.position = resultPos; // 이동
    }
}