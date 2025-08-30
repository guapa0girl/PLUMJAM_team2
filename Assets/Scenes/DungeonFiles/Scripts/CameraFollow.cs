using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                     // ������ ������Ʈ
    public Vector3 offset = new Vector3(0, 0, -10); // ��ǥ�� ������(����ġ)
    public float cameraSpeed = 5f;

    void FixedUpdate()
    {
        Vector3 targetPos = target.position + offset; // ���� ��ǥ ��ġ
                                                      // ���� ��ġ���� ��ǥ������ �ε巴�� �̵� ����
        Vector3 resultPos = Vector3.Lerp(transform.position, targetPos, cameraSpeed * Time.fixedDeltaTime);
        transform.position = resultPos; // �̵�
    }
}