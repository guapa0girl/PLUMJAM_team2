using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Game.Core;              // Economy, Inventory
using Game.Data;              // SkillDef, RoomThemeDef
using Game.Systems.Combat;    // CombatSystem

namespace Game.Systems
{
    // ─────────────────────────────────────────────────────────────
    // File    : UpgradeOfferManager.cs
    // Purpose : 업그레이드 3선택 제공. 기본은 "돈 기반"으로 입장당 1회 구매.
    //           (옵션) 토큰/월요일 지급을 켜면 기존 토큰 방식도 지원.
    // Events  : OnOfferGenerated(choices, remainingPurchasesOrTokens),
    //           OnUpgradeResult(def, ok)
    // ─────────────────────────────────────────────────────────────
    public class UpgradeOfferManager : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] RunManager run;        // day(1~28) 참조 (토큰 모드일 때만 의미)
        [SerializeField] SkillSystem skills;    // 업그레이드 대상
        [SerializeField] Economy economy;       // 돈 관리
        [SerializeField] CombatSystem combat;   // (선택) 전투 래퍼
        [SerializeField] Inventory inventory;   // (선택) 전투 래퍼

        [Header("Offer Config")]
        [Min(1)] public int choicesPerOffer = 3;    // 제시할 스킬 수 (보통 3)
        public bool autoPickIfNoUI = false;         // UI가 없을 때 자동 선택
        public enum AutoPick { Random, Cheapest }
        public AutoPick autoPickMode = AutoPick.Random;

        [Header("Gating Mode")]
        public bool moneyOnly = true;               // ★ 기본값: 돈 기반 (토큰/월요일 무시)
        public bool onePurchasePerEntry = true;     // 돈 기반일 때 입장당 1회만 구매
        bool consumedThisEntry = false;             // 이번 입장에서 이미 구매했는지

        [Header("Token Mode (옵션)")]
        [Min(0)] public int weeklyTokens = 2;       // 월요일 지급(토큰 모드일 때만)
        public int upgradeTokens = 0;               // 현재 토큰 개수
        public int lastSeenDay = -1;                // 요일 감지

        // UI용 이벤트
        public event Action<IReadOnlyList<SkillDef>, int> OnOfferGenerated;
        public event Action<SkillDef, bool> OnUpgradeResult;

        List<SkillDef> currentChoices;

        void Start()
        {
            if (!moneyOnly) // 토큰 모드에서만 요일/토큰 관리
            {
                if (run != null) lastSeenDay = run.day;
                GiveMondayTokensIfNeeded();
            }
        }

        void Update()
        {
            if (moneyOnly || run == null) return;
            if (run.day != lastSeenDay)
            {
                lastSeenDay = run.day;
                GiveMondayTokensIfNeeded();
            }
        }

        void GiveMondayTokensIfNeeded()
        {
            if (moneyOnly || run == null) return;
            // day: 1,8,15,22 → 월요일
            if (((run.day - 1) % 7) == 0)
                upgradeTokens += weeklyTokens;
        }

        // --- 오퍼 생성 (돈 모드: 항상 생성 / 토큰 모드: 토큰>0일 때만) ---
        public void PrepareOfferIfAvailable()
        {
            if (moneyOnly)
            {
                consumedThisEntry = false; // 입장마다 1회 구매 제한 초기화
                currentChoices = GenerateChoices(choicesPerOffer);
                int remaining = onePurchasePerEntry && consumedThisEntry ? 0 : 1; // 돈 모드 표기용
                OnOfferGenerated?.Invoke(currentChoices, remaining);

                if (autoPickIfNoUI && currentChoices.Count > 0)
                {
                    int idx = AutoPickIndex(currentChoices);
                    ApplyChoice(idx);
                }
                return;
            }

            // --- 토큰 모드 ---
            if (upgradeTokens <= 0) { currentChoices = null; return; }

            currentChoices = GenerateChoices(choicesPerOffer);
            OnOfferGenerated?.Invoke(currentChoices, upgradeTokens);

            if (autoPickIfNoUI && currentChoices.Count > 0)
            {
                int idx = AutoPickIndex(currentChoices);
                ApplyChoice(idx);
            }
        }

        public IReadOnlyList<SkillDef> GetCurrentChoices() => currentChoices;

        // --- 선택 적용(결제 + 레벨업) ---
        public bool ApplyChoice(int index)
        {
            if (currentChoices == null || index < 0 || index >= currentChoices.Count)
            { OnUpgradeResult?.Invoke(null, false); return false; }

            var def = currentChoices[index];
            if (!def || skills?.skills == null) { OnUpgradeResult?.Invoke(def, false); return false; }

            var owned = skills.skills.Find(s => s != null && s.def != null && s.def.skillId == def.skillId);
            if (owned == null || owned.level >= def.levelCap) { OnUpgradeResult?.Invoke(def, false); return false; }

            // --- 돈 모드 ---
            if (moneyOnly)
            {
                if (onePurchasePerEntry && consumedThisEntry)
                { OnUpgradeResult?.Invoke(def, false); return false; }

                int cost = def.upgradeCostBase * (owned.level + 1);
                if (economy == null || !economy.TrySpend(cost))
                { OnUpgradeResult?.Invoke(def, false); return false; }

                owned.level++;
                consumedThisEntry = true;      // 입장당 1회 소비
                currentChoices = null;
                OnUpgradeResult?.Invoke(def, true);
                return true;
            }

            // --- 토큰 모드 ---
            if (upgradeTokens <= 0) { OnUpgradeResult?.Invoke(def, false); return false; }

            int tokenCost = def.upgradeCostBase * (owned.level + 1);
            bool paid = (economy != null && economy.TrySpend(tokenCost)); // 토큰+돈 병행(기존 규칙 유지)
            if (!paid) { OnUpgradeResult?.Invoke(def, false); return false; }

            owned.level++;
            upgradeTokens--;
            currentChoices = null;
            OnUpgradeResult?.Invoke(def, true);
            return true;
        }

        // (선택) 전투 래퍼
        public IEnumerator EnterDungeonWrapper(RoomThemeDef room, float dropMultByWeather = 1f)
        {
            PrepareOfferIfAvailable();
            if (combat != null && inventory != null)
                yield return combat.PlayOneClear(room, inventory, dropMultByWeather);
        }

        // ---------- 내부 유틸 ----------
        List<SkillDef> GenerateChoices(int count)
        {
            var list = new List<SkillDef>();
            if (skills == null || skills.skills == null) return list;

            var pool = new List<SkillDef>();
            foreach (var s in skills.skills)
            {
                if (s?.def == null) continue;
                if (s.level < s.def.levelCap)
                    pool.Add(s.def);
            }
            if (pool.Count == 0) return list;

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int k = UnityEngine.Random.Range(0, pool.Count);
                list.Add(pool[k]);
                pool.RemoveAt(k);
            }
            return list;
        }

        int AutoPickIndex(IReadOnlyList<SkillDef> list)
        {
            if (autoPickMode == AutoPick.Random) return UnityEngine.Random.Range(0, list.Count);

            int best = 0;
            int bestCost = int.MaxValue;
            foreach (var s in skills.skills)
            {
                int idx = list.IndexOf(s.def);
                if (idx < 0) continue;
                int cost = s.def.upgradeCostBase * (s.level + 1);
                if (cost < bestCost) { bestCost = cost; best = idx; }
            }
            return best;
        }
    }

    // 작은 헬퍼: IReadOnlyList.IndexOf
    static class ReadOnlyListExt
    {
        public static int IndexOf<T>(this IReadOnlyList<T> self, T item)
        {
            for (int i = 0; i < self.Count; i++)
                if (EqualityComparer<T>.Default.Equals(self[i], item)) return i;
            return -1;
        }
    }
}
