using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Game.Systems;   // UpgradeOfferManager, SkillSystem
using Game.Core;      // Economy, SceneBridge (or SceneFlow)
using Game.Data;      // SkillDef

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
        // 자동 배선: GameRoot 우선, 없으면 FindObjectOfType로 찾기
        if (!offer) offer = GameRoot.Instance ? GameRoot.Instance.upgrade : FindObjectOfType<UpgradeOfferManager>(true);
        if (!skills) skills = GameRoot.Instance ? GameRoot.Instance.skills : FindObjectOfType<SkillSystem>(true);
        if (!economy) economy = GameRoot.Instance ? GameRoot.Instance.economy : FindObjectOfType<Economy>(true);
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
        // 현재 레벨 찾기
        int level = 0;
        if (skills?.skills != null)
        {
            foreach (var s in skills.skills)
                if (s != null && s.def == def) { level = s.level; break; }
        }
        int next = Mathf.Min(level + 1, def.levelCap);
        int cost = def.upgradeCostBase * (level + 1);
        return $"{def.displayName}\nLv.{level} → Lv.{next}\nCost: ₩{cost}";
    }
}
