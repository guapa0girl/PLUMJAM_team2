using UnityEngine;
using Game.Data;

// ─────────────────────────────────────────────────────────────
// File: WeatherSystem.cs
// Purpose : 내일 예보(확률) 생성, 오늘 날씨 확정, UI 노출용 데이터 제공.
// Defines : class WeatherSystem : MonoBehaviour
// Fields  : Today(오늘 확정 날씨), TomorrowForecast(내일 확률 예보)
// API     : RollTomorrowForecast()  → 내일 예보 생성
//           ResolveTodayFromForecast() → 예보 확률로 오늘 날씨 확정
// Used By : RunManager(일 진행), FarmingSystem(성장/죽음 판정), SkillSystem(버프).
// Notes   : 예보 정확도/분산을 스킬/빌드로 조정하는 확장 포인트 존재.
// ─────────────────────────────────────────────────────────────


// WeatherSystem.cs
public class WeatherSystem : MonoBehaviour
{
    public struct Forecast { public float heat; public float rain; public float snow; public float cloud; }
    public WeatherType Today { get; private set; }
    public Forecast TomorrowForecast { get; private set; }

    public void RollTomorrowForecast()
    {
        // 예: 비 70%, 맑음 20%, 바람 10%, 눈 0% 식으로 설정 가능
        // 에디터에서 채워주거나 간단 로직으로 산출
        TomorrowForecast = new Forecast { heat = 0.4f, rain = 0.2f, snow = 0.1f, cloud = 0.3f };
    }
    public WeatherType ResolveTodayFromForecast()
    {
        float r = UnityEngine.Random.value;  // [0..1] :contentReference[oaicite:8]{index=8}
        float acc = 0;
        (WeatherType t, float p)[] list = {
            (WeatherType.Heat, TomorrowForecast.heat),
            (WeatherType.Rain,  TomorrowForecast.rain),
            (WeatherType.Snow,  TomorrowForecast.snow),
            (WeatherType.Cloud,  TomorrowForecast.cloud),
        };
        foreach (var (t, p) in list) { acc += p; if (r <= acc) { Today = t; return Today; } }
        //?
        Today = WeatherType.Heat; return Today;
    }
}