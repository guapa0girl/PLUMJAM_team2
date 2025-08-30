using UnityEngine;
namespace Game.Core {

    public class Economy : MonoBehaviour
    {
        public int money;
        public void AddMoney(int v) => money += v;
        public bool TrySpend(int v) { if (money < v) return false; money -= v; return true; }
        }
    }