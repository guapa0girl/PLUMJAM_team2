using UnityEngine;
namespace Game.Data
{
    // 값(숫자)을 고정하면 Unity 직렬화가 안전합니다.
    public enum WeatherType
    {
        Heat = 0,
        Rain = 1,
        Snow = 2,
        Cloud = 3
    }
}