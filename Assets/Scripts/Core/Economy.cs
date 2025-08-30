using UnityEngine;
namespace Game.Core {

    public class Economy : MonoBehaviour
    {
        public int money = 100;
        public void AddMoney(int v) => money += v;
        public bool TrySpend(int v) { if (money < v) return false; money -= v; return true; }
        }
    }