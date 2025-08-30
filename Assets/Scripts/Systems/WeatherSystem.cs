using UnityEngine;
using Game.Data;

// ��������������������������������������������������������������������������������������������������������������������������
// File: WeatherSystem.cs
// Purpose : ���� ����(Ȯ��) ����, ���� ���� Ȯ��, UI ����� ������ ����.
// Defines : class WeatherSystem : MonoBehaviour
// Fields  : Today(���� Ȯ�� ����), TomorrowForecast(���� Ȯ�� ����)
// API     : RollTomorrowForecast()  �� ���� ���� ����
//           ResolveTodayFromForecast() �� ���� Ȯ���� ���� ���� Ȯ��
// Used By : RunManager(�� ����), FarmingSystem(����/���� ����), SkillSystem(����).
// Notes   : ���� ��Ȯ��/�л��� ��ų/����� �����ϴ� Ȯ�� ����Ʈ ����.
// ��������������������������������������������������������������������������������������������������������������������������


// WeatherSystem.cs
public class WeatherSystem : MonoBehaviour
{
    public struct Forecast { public float heat; public float rain; public float snow; public float cloud; }
    public WeatherType Today { get; private set; }
    public Forecast TomorrowForecast { get; private set; }

    public void RollTomorrowForecast()
    {
        // ��: �� 70%, ���� 20%, �ٶ� 10%, �� 0% ������ ���� ����
        // �����Ϳ��� ä���ְų� ���� �������� ����
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