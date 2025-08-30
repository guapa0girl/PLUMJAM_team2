using UnityEngine;

[CreateAssetMenu(fileName = "Seed_", menuName = "Game/Seed Data")]
public class SeedData : ScriptableObject
{
    public string seedName;  // ���� �̸� (Heatseed, Rainseed, Windseed, Snowseed)
    public Sprite icon;      // ���� ������
}