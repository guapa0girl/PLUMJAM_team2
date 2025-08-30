using UnityEngine;

[DisallowMultipleComponent]
public class SpawnedToken : MonoBehaviour
{
    [HideInInspector] public EnemySpawner owner;
    void OnDisable() { if (owner) owner.NotifyDespawn(this); }
    void OnDestroy() { if (owner) owner.NotifyDespawn(this); }
}