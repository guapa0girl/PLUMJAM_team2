using UnityEngine;
using UnityEngine.SceneManagement;

public static class ForceTitleAtStart
{
    // ���� ���� ��(Play �����ų� ���� ����) ���� ���� ȣ���
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        const string TITLE = "Title"; //�� �̸�
        var active = SceneManager.GetActiveScene().name;
        if (active != TITLE)
        {
            // Title ���� ù ȭ������ ���� �ε�
            SceneManager.LoadScene(TITLE);
        }
    }
}