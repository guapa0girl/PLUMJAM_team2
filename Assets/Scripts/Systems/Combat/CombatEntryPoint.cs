// CombatEntryPoint.cs
using UnityEngine;
using System.Collections;
using Game.Core;             // FlowContext
using Game.Systems.Combat;   // CombatSystem

public class CombatEntryPoint : MonoBehaviour
{
    public CombatSystem combat;    // GameRoot�� CombatSystem �巡��
    public Game.Core.Inventory inventory; // GameRoot�� Inventory �巡��

    IEnumerator Start()
    {
        // FlowContext���� ��/������ �б�
        var room = FlowContext.NextRoom;
        var mult = FlowContext.NextDropMult;

        if (combat != null && inventory != null && room != null)
            yield return combat.PlayOneClear(room, inventory, mult);

        // ���� ���� �� Farm���� ���� (���ϸ� ��� �г� ���� �̵�)
        SceneFlow.GoToFarm();
    }
}
