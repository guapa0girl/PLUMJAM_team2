using UnityEngine;
using UnityEngine.SceneManagement;

public static class ForceTitleAtStart
{
    // 앱이 켜질 때(Play 누르거나 빌드 실행) 제일 먼저 호출됨
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        const string TITLE = "Title"; //씬 이름
        var active = SceneManager.GetActiveScene().name;
        if (active != TITLE)
        {
            // Title 씬을 첫 화면으로 강제 로드
            SceneManager.LoadScene(TITLE);
        }
    }
}