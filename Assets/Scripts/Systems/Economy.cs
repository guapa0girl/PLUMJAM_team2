using UnityEngine;
// Economy.cs
public class Economy : MonoBehaviour
{
    public int money;
    public void AddMoney(int v) => money += v;
    public bool TrySpend(int v) { if (money < v) return false; money -= v; return true; }
}

// SaveSystem.cs
[System.Serializable]
public class SaveData
{
    public int day; public int money;
    // 인벤토리/플롯/스킬 등 필요한 필드 추가
}

public static class SaveSystem
{
    public static void Save(SaveData d)
    {
        var json = JsonUtility.ToJson(d);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, "save.json"), json);
    }
    public static SaveData Load()
    {
        var p = System.IO.Path.Combine(Application.persistentDataPath, "save.json");
        if (!System.IO.File.Exists(p)) return null;
        return JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(p));
    }
}
