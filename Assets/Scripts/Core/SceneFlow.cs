using UnityEngine;

// SceneFlow.cs
using UnityEngine.SceneManagement;
using Game.Data;
using Game.Core;

public static class SceneFlow
{
    public static void GoToUpgrade(RoomThemeDef room, float dropMult)
    {
        FlowContext.NextRoom = room;
        FlowContext.NextDropMult = dropMult;
        SceneManager.LoadScene("Upgrade");   // �� ���׷��̵� �� �̸�
    }

    public static void GoToDungeon()
    {
        SceneManager.LoadScene("Dungeon");   // �� ���� �� �̸�
    }

    public static void GoToFarm()
    {
        SceneManager.LoadScene("Farm");      // �� �Թ� �� �̸�
    }
}
