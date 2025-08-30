using UnityEngine;

[DisallowMultipleComponent]
public class SpawnedToken : MonoBehaviour
{
    [HideInInspector] public EnemySpawner owner;

    // 파괴/비활성화될 때 스포너에 보고
    void OnDisable()
    {
        if (owner) owner.NotifyDespawn(this);
    }

    void OnDestroy()
    {
        if (owner) owner.NotifyDespawn(this);
    }
}