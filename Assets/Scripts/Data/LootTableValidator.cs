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
        if (lt.entries == null || lt.entries.Length == 0)
            EditorGUILayout.HelpBox("��Ʈ���� ����ֽ��ϴ�.", MessageType.Warning);

        foreach (var e in lt.entries)
        {
            if (e == null) continue;
            if (!e.seed)
                EditorGUILayout.HelpBox("Seed �ʵ尡 ��� �ֽ��ϴ�.", MessageType.Error);
            if (e.min > e.max)
                EditorGUILayout.HelpBox($"min({e.min}) > max({e.max})", MessageType.Error);
            if (e.weight <= 0)
                EditorGUILayout.HelpBox("weight�� 0���� Ŀ�� �մϴ�.", MessageType.Warning);
        }
        EditorGUILayout.HelpBox("�� �� ���̺��� '���� óġ ���� ���� ���' �뵵�Դϴ�.", MessageType.Info);
    }
}
