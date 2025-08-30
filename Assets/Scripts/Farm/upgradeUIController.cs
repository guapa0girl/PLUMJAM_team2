using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Game.Systems;   // UpgradeOfferManager, SkillSystem
using Game.Core;      // Economy, SceneBridge (or SceneFlow)
using Game.Data;      // SkillDef


// ─────────────────────────────────────────────────────────────
// File    : UpgradeUIController.cs
// Purpose : 업그레이드 씬의 UI 컨트롤러.
//           - UpgradeOfferManager가 뽑아준 3개 스킬 선택지를 버튼에 표시
//           - 플레이어가 1개 선택하면 ApplyChoice 호출 → 성공 시 던전으로 이동
//           - 월요일 토큰이 0이면 선택지 없음(건너뛰기 버튼으로 던전 이동만 가능)
// Depends : UpgradeOfferManager(오퍼/적용), SkillSystem(레벨/쿨다운), Economy(돈표시)
// Notes   : offer/skills/economy가 비어 있으면 Awake에서 자동 배선 시도.
//           offer가 null이면 선택지가 생성되지 않으므로 업그레이드 자체가 불가.
// ─────────────────────────────────────────────────────────────

public class UpgradeUIController : MonoBehaviour
{
    [Header("Refs (비워두면 자동으로 찾음)")]
    [SerializeField] UpgradeOfferManager offer;
    [SerializeField] SkillSystem skills;
    [SerializeField] Economy economy;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI headerText;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] Button[] choiceButtons;              // size=3
    [SerializeField] TextMeshProUGUI[] choiceLabels;      // size=3
    [SerializeField] Button skipOrContinueButton;

    IReadOnlyList<SkillDef> current;
    void Awake()
    {
        // --- offer ---
        if (!offer)
        {
            if (GameRoot.Instance && GameRoot.Instance.upgrade)
                offer = GameRoot.Instance.upgrade;
            if (!offer) // ★ GameRoot에 못 꽂혔으면 씬에서라도 찾기
                offer = FindObjectOfType<UpgradeOfferManager>(includeInactive: true);
        }

        // --- skills / economy 도 동일 패턴 권장 ---
        if (!skills)
        {
            if (GameRoot.Instance && GameRoot.Instance.skills) skills = GameRoot.Instance.skills;
            if (!skills) skills = FindObjectOfType<SkillSystem>(true);
        }
        if (!economy)
        {
            if (GameRoot.Instance && GameRoot.Instance.economy) economy = GameRoot.Instance.economy;
            if (!economy) economy = FindObjectOfType<Economy>(true);
        }
    }


    void OnEnable()
    {
        // 이벤트 구독 가드
        if (offer != null)
        {
            offer.OnOfferGenerated += HandleOfferGenerated;
            offer.OnUpgradeResult += HandleUpgradeResult;
        }
        else
        {
            Debug.LogWarning("[UpgradeUI] offer가 null 입니다. 선택지 표시 없이 진행만 가능합니다.");
        }
    }

    void Start()
    {
        // UI 널 가드 + 버튼 리스너
        if (choiceButtons != null)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int idx = i;
                if (choiceButtons[i] != null)
                    choiceButtons[i].onClick.AddListener(() => OnClickChoice(idx));
            }
        }
        if (skipOrContinueButton) skipOrContinueButton.onClick.AddListener(OnClickContinue);

        RefreshMoney();

        // 토큰 있으면 선택지 생성 (없으면 아무 일도 안 함)
        if (offer != null) offer.PrepareOfferIfAvailable();
        else headerText?.SetText("업그레이드(선택지 없음) — 계속을 눌러 진행");
    }

    void OnDisable()
    {
        if (offer != null)
        {
            offer.OnOfferGenerated -= HandleOfferGenerated;
            offer.OnUpgradeResult -= HandleUpgradeResult;
        }
    }

    // ----- 이벤트 핸들러 -----
    void HandleOfferGenerated(IReadOnlyList<SkillDef> list, int tokens)
    {
        current = list;
        if (headerText) headerText.text = $"업그레이드 선택 ({tokens}회 남음)";

        // 3개 슬롯 채우기 (널 가드)
        for (int i = 0; i < (choiceButtons?.Length ?? 0); i++)
        {
            bool has = current != null && i < current.Count && current[i] != null;

            if (choiceButtons[i]) choiceButtons[i].interactable = has;

            if (choiceLabels != null && i < choiceLabels.Length && choiceLabels[i])
                choiceLabels[i].text = has ? BuildLabel(current[i]) : "-";
        }
    }

    void HandleUpgradeResult(SkillDef def, bool ok)
    {
        RefreshMoney();

        if (ok)
        {
            if (headerText) headerText.text = $"{def.displayName} 업그레이드 성공!";
            // 바로 던전으로
            Game.Core.SceneBridge.GoToDungeon();
        }
        else
        {
            if (headerText) headerText.text = "업그레이드 실패(잔액 부족?) — 계속을 눌러 던전으로";
            // 계속 버튼으로 던전 진입
        }
    }

    // ----- UI 액션 -----
    void OnClickChoice(int index)
    {
        if (offer == null || current == null) return;
        offer.ApplyChoice(index);
    }

    void OnClickContinue()
    {
        Game.Core.SceneBridge.GoToDungeon();
    }

    // ----- 헬퍼 -----
    void RefreshMoney()
    {
        if (moneyText && economy) moneyText.text = $"₩ {economy.money}";
    }
    string BuildLabel(SkillDef def)
    {
        int level = 0;

        if (skills?.skills != null && def != null)
        {
            foreach (var s in skills.skills)
            {
                if (s?.def != null && s.def.skillId == def.skillId) // ★ id로 비교
                {
                    level = s.level;
                    break;
                }
            }
        }

        int next = Mathf.Min(level + 1, def.levelCap);
        int cost = def.upgradeCostBase * (level + 1);
        return $"{def.displayName}\nLv.{level} → Lv.{next}\nCost: ₩{cost}";
    }
}

