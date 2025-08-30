using UnityEngine;

[CreateAssetMenu(fileName = "Seed_", menuName = "Game/Seed Data")]
public class SeedData : ScriptableObject
{
    public string seedName;  // ¾¾¾Ñ ÀÌ¸§ (Heatseed, Rainseed, Windseed, Snowseed)
    public Sprite icon;      // ¾¾¾Ñ ¾ÆÀÌÄÜ
}