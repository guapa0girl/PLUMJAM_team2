// Assets/Scripts/Systems/Combat/Health.cs
using System;
using UnityEngine;

namespace Game.Systems.Combat
{
    public class Health : MonoBehaviour
    {
        [SerializeField] int maxHp = 10;
        int hp; bool isDead;
        public event Action<Health> Died;
        void Awake() => hp = maxHp;
        public void TakeDamage(int dmg) { if (isDead) return; hp -= Mathf.Max(0, dmg); if (hp <= 0) Die(); }
        void Die() { if (isDead) return; isDead = true; Died?.Invoke(this); }
    }
}
