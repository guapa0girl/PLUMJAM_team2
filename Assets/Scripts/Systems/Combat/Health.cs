// Assets/Scripts/Systems/Combat/Health.cs

// ��������������������������������������������������������������������������������������������������������������������������
// File: Health.cs
// Layer : Systems (Game.Systems.Combat)
// Purpose : ����/�÷��̾� ���� ü�� ����. ���� ó���� ��� �̺�Ʈ ����.
// Responsibilities :
//   - ���� HP ����, ������ ����(TakeDamage)
//   - 0 ���ϰ� �Ǹ� Die()�� ��� �÷��� ���� �� Died �̺�Ʈ ����
// Events :
//   - event Action<Health> Died  �� �ܺ�(��: MonsterDropper, Spawner)�� ����
// Serialized Fields :
//   - int maxHp
// Notes : �ߺ� ��� ����(isDead), ������ �ּ� 0 ����(Mathf.Max)
// ��������������������������������������������������������������������������������������������������������������������������

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
