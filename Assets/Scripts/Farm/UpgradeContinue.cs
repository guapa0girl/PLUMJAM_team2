// ─────────────────────────────────────────────────────────────
// File    : UpgradeContinue.cs
// Purpose : 업그레이드 UI에서 "계속/던전으로" 버튼이 누르면 실제 던전으로 이동
// ─────────────────────────────────────────────────────────────
using UnityEngine;
using Game.Core;

public class UpgradeContinue : MonoBehaviour
{
    public void ContinueToDungeon()
    {
        SceneBridge.GoToDungeon();
    }
}
