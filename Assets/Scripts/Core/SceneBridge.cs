using UnityEngine;

// ─────────────────────────────────────────────────────────────
// File    : SceneBridge.cs
// Purpose : 씬 간 간단한 데이터 전달(다음 던전 씬 이름) + 전환 헬퍼
// Notes   : 기존 시스템 그대로 두고 "씬 넘어가는 부분"만 끼워 넣기용
// ─────────────────────────────────────────────────────────────
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public static class SceneBridge
    {
        public static string NextDungeonScene;   // 다음에 들어갈 던전 씬 이름
        public static float NextDropMult = 1f;   // 추후 필요시 사용

        public static void GoToUpgrade(string nextDungeonScene, float dropMult = 1f)
        {
            NextDungeonScene = nextDungeonScene;
            NextDropMult = dropMult;
            SceneManager.LoadScene("Upgrade");     // 업그레이드 씬 이름
        }

        public static void GoToDungeon()
        {
            var name = string.IsNullOrEmpty(NextDungeonScene) ? "dungeon_rain" : NextDungeonScene;
            SceneManager.LoadScene(name);
        }
    }
}
