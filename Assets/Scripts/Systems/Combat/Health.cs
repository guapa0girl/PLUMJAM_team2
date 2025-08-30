// Assets/Scripts/Systems/Combat/Health.cs

// ─────────────────────────────────────────────────────────────
// File: Health.cs
// Layer : Systems (Game.Systems.Combat)
// Purpose : 몬스터/플레이어 공통 체력 관리. 피해 처리와 사망 이벤트 발행.
// Responsibilities :
//   - 현재 HP 관리, 데미지 적용(TakeDamage)
//   - 0 이하가 되면 Die()로 사망 플래그 설정 및 Died 이벤트 발행
// Events :
//   - event Action<Health> Died  → 외부(예: MonsterDropper, Spawner)가 구독
// Serialized Fields :
//   - int maxHp
// Notes : 중복 사망 방지(isDead), 데미지 최소 0 보정(Mathf.Max)
// ─────────────────────────────────────────────────────────────

using System;
using UnityEngine;

namespace Game.Systems.Combat
{
    public class Health : MonoBehaviour
    {
        [SerializeField] int maxHp = 100;
        int hp; bool isDead;
        public event Action<Health> Died;
        void Awake() => hp = maxHp;
        public void TakeDamage(int dmg) { if (isDead) return; hp -= Mathf.Max(0, dmg); if (hp <= 0) Die(); }
        void Die() { if (isDead) return; isDead = true; Died?.Invoke(this); }
    }
}
