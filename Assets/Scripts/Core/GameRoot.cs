// ─────────────────────────────────────────────────────────────
// File    : GameRoot.cs
// Purpose : 전역 매니저의 싱글톤 컨테이너. 씬 이동 간 유지 + 중복 방지.
// Defines : class GameRoot : MonoBehaviour (Singleton)
// Notes   : 씬에 하나만 두세요. 중복 생성 시 자신을 파괴합니다.
//           Start 화면에 빈 오브제 만들고 붙여주세요!
//           자식에 붙은 RunManager/Economy/…를 자동으로 찾아 배선합니다.
// ─────────────────────────────────────────────────────────────
using UnityEngine;
using Game.Core;
using Game.Systems;
using Game.Systems.Combat;

[DefaultExecutionOrder(-1000)] // 가장 먼저 뜨도록
public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }

    [Header("Managers (선택: 비워두면 자동 탐색)")]
    public RunManager run;
    public Economy economy;
    public SkillSystem skills;
    public CombatSystem combat;
    public Inventory inventory;
    public UpgradeOfferManager upgrade;

    void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 비어 있으면 자식에서 자동 배선(프리팹/계층에 따라 true로 숨겨진 것도 포함)
        if (!run) run = GetComponentInChildren<RunManager>(true);
        if (!economy) economy = GetComponentInChildren<Economy>(true);
        if (!skills) skills = GetComponentInChildren<SkillSystem>(true);
        if (!combat) combat = GetComponentInChildren<CombatSystem>(true);
        if (!inventory) inventory = GetComponentInChildren<Inventory>(true);
        if (!upgrade) upgrade = GetComponentInChildren<UpgradeOfferManager>(true);
    }
}
