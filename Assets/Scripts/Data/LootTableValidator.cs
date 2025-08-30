// Assets/Editor/LootTableValidator.cs
using UnityEditor;
using UnityEngine;
using Game.Data;

[CustomEditor(typeof(LootTable))]
public class LootTableValidator : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var lt = (LootTable)target;
        if (lt == null) return;  // 안전장치

        if (lt.entries == null || lt.entries.Length == 0)
        {
            EditorGUILayout.HelpBox("엔트리가 비어있습니다.", MessageType.Warning);
            return;
        }

        foreach (var e in lt.entries)
        {
            if (e == null)
            {
                EditorGUILayout.HelpBox("엔트리 슬롯이 null입니다.", MessageType.Error);
                continue;
            }

            if (e.seed == null)
                EditorGUILayout.HelpBox("Seed 필드가 비어 있습니다.", MessageType.Error);

            if (e.min > e.max)
                EditorGUILayout.HelpBox($"min({e.min}) > max({e.max})", MessageType.Error);

            if (e.weight <= 0)
                EditorGUILayout.HelpBox("weight는 0보다 커야 합니다.", MessageType.Warning);
        }

        EditorGUILayout.HelpBox("※ 이 테이블은 '몬스터 처치 전용 씨앗 드랍' 용도입니다.", MessageType.Info);
    }
}
