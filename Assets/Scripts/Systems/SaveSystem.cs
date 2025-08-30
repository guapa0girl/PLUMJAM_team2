using System;                     // [Serializable]
using System.IO;                  // File IO
using UnityEngine;

namespace Game.Systems
{
    [Serializable]
    public class SaveData
    {
        public int day;
        public int money;
        // TODO: �κ��丮/�÷�/��ų �� �߰�
    }

    public static class SaveSystem
    {
        public static void Save(SaveData d)
        {
            var json = JsonUtility.ToJson(d);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "save.json"), json);
        }

        public static SaveData Load()
        {
            var p = Path.Combine(Application.persistentDataPath, "save.json");
            if (!File.Exists(p)) return null;
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(p));
        }
    }
}