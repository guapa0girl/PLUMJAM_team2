// CombatEntryPoint.cs
using UnityEngine;
using System.Collections;
using Game.Core;             // FlowContext
using Game.Systems.Combat;   // CombatSystem

public class CombatEntryPoint : MonoBehaviour
{
    public CombatSystem combat;    // GameRoot의 CombatSystem 드래그
    public Game.Core.Inventory inventory; // GameRoot의 Inventory 드래그

    IEnumerator Start()
    {
        // FlowContext에서 룸/드랍배수 읽기
        var room = FlowContext.NextRoom;
        var mult = FlowContext.NextDropMult;

        if (combat != null && inventory != null && room != null)
            yield return combat.PlayOneClear(room, inventory, mult);

        // 전투 종료 후 Farm으로 복귀 (원하면 결과 패널 띄우고 이동)
        SceneFlow.GoToFarm();
    }
}
