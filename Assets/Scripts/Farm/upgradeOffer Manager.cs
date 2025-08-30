using UnityEngine;

// ─────────────────────────────────────────────────────────────
// File    : UpgradeOfferManager.cs
// Namespace : Game.Systems
// Purpose : 던전 입장 시 "3선택 1업그레이드" 제공 + 매주 월요일 업그레이드 토큰 2개 지급
//           시작 자금 100 지급(최초 1회). 비용은 SkillSystem 규칙(선형) 사용.
// Defines : class UpgradeOfferManager : MonoBehaviour
// Public  : PrepareOfferIfAvailable(), ApplyChoice(int), GetCurrentChoices()
//           EnterDungeonWrapper(RoomThemeDef, float) — CombatSystem 코루틴 래퍼(선택 사용)
// Notes   : UI는 OnOfferGenerated 이벤트를 구독해 3개 선택지를 노출하고,
//           플레이어 선택 시 ApplyChoice(index)를 호출하면 된다.
//           초기 비용(50)은 SkillDef.upgradeCostBase로 제어. (에셋에서 50 권장)
// Dependencies : Economy(돈), RunManager(요일/날짜), SkillSystem(업그레이드), CombatSystem(전투)
// ─────────────────────────────────────────────────────────────
using System;
using System.Collections;
using System.Collections.Generic;
using Game.Core;              // Economy, Inventory
using Game.Data;              // SkillDef, RoomThemeDef
using Game.Systems.Combat;    // CombatSystem

namespace Game.Systems
{
    public class UpgradeOfferManager : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] RunManager run;        // day(1~28) 참조용  :contentReference[oaicite:4]{index=4}
        [SerializeField] SkillSystem skills;    // 업그레이드 수행  :contentReference[oaicite:5]{index=5}
        [SerializeField] Economy economy;       // 돈 관리          :contentReference[oaicite:6]{index=6}
        [SerializeField] CombatSystem combat;   // (선택) 전투 래퍼에서 사용
        [SerializeField] Inventory inventory;   // (선택) 전투 래퍼에서 사용

        [Header("Config")]
        [Min(1)] public int choicesPerOffer = 3;   // 제시할 스킬 개수
        [Min(0)] public int weeklyTokens = 2;      // 월요일에 지급할 업그레이드 기회 수
        public int startingMoney = 100;            // 최초 1회 지급
        public bool autoPickIfNoUI = false;        // UI가 없으면 자동 선택할지
        public enum AutoPick { Random, Cheapest }
        public AutoPick autoPickMode = AutoPick.Random;

        [Header("State (debug)")]
        public int upgradeTokens = 0;              // 보유 업그레이드 기회
        public int lastSeenDay = -1;               // 월요일 지급 감지
        List<SkillDef> currentChoices;             // 현재 제시 중인 선택지

        // 던전 입장 시 외부 UI가 구독할 이벤트: (선택지, 남은 토큰)
        public event Action<IReadOnlyList<SkillDef>, int> OnOfferGenerated;
        public event Action<SkillDef, bool> OnUpgradeResult; // (선택한 스킬, 성공여부)

        bool startingGranted = false;

        void Start()
        {
            // 시작 자금 1회 지급
            if (!startingGranted && economy != null)
            {
                economy.AddMoney(startingMoney);   // 돈 +100  :contentReference[oaicite:7]{index=7}
                startingGranted = true;
            }
            // 시작일 기록
            if (run != null) lastSeenDay = run.day;
            // 첫날이 월요일(day=1)이면 즉시 토큰 지급
            GiveMondayTokensIfNeeded();
        }

        void Update()
        {
            // RunManager.day 변화 감지로 "요일" 판정  :contentReference[oaicite:8]{index=8}
            if (run == null) return;
            if (run.day != lastSeenDay)
            {
                lastSeenDay = run.day;
                GiveMondayTokensIfNeeded();
            }
        }

        void GiveMondayTokensIfNeeded()
        {
            if (run == null) return;
            // day: 1,8,15,22 → 월요일
            if (((run.day - 1) % 7) == 0)
            {
                upgradeTokens += weeklyTokens; // 매주 월요일 토큰 2개
            }
        }

        // 던전 입장 직전 호출: 선택지 생성(보유 토큰>0일 때만)
        public void PrepareOfferIfAvailable()
        {
            if (upgradeTokens <= 0) { currentChoices = null; return; }

            currentChoices = GenerateChoices(choicesPerOffer);
            OnOfferGenerated?.Invoke(currentChoices, upgradeTokens);

            if (autoPickIfNoUI && currentChoices.Count > 0)
            {
                int idx = AutoPickIndex(currentChoices);
                ApplyChoice(idx);
            }
        }

        // 외부(UI)에서 현재 선택지 조회
        public IReadOnlyList<SkillDef> GetCurrentChoices() => currentChoices;

        // UI에서 플레이어가 index를 고르면 호출
        public bool ApplyChoice(int index)
        {
            if (upgradeTokens <= 0 || currentChoices == null || index < 0 || index >= currentChoices.Count)
            { OnUpgradeResult?.Invoke(null, false); return false; }

            var def = currentChoices[index];
            if (def == null) { OnUpgradeResult?.Invoke(null, false); return false; }

            // SkillSystem 선형 비용 규칙 사용: upgradeCostBase * (level+1)  :contentReference[oaicite:9]{index=9} :contentReference[oaicite:10]{index=10}
            bool ok = skills != null && economy != null && skills.TryUpgrade(economy, def.skillId);
            OnUpgradeResult?.Invoke(def, ok);

            if (ok)
            {
                upgradeTokens--;
                currentChoices = null; // 소비 후 선택지 폐기
                return true;
            }
            return false;
        }

        // (선택) CombatSystem 코루틴을 감싸서 "입장 시 오퍼 → 전투"까지 한 번에
        public IEnumerator EnterDungeonWrapper(RoomThemeDef room, float dropMultByWeather = 1f)
        {
            // 1) 오퍼 제공(보유 토큰 있을 때만)
            PrepareOfferIfAvailable();
            // 2) 전투 시작(기존 CombatSystem 그대로 사용)
            if (combat != null && inventory != null)
                yield return combat.PlayOneClear(room, inventory, dropMultByWeather);
        }

        // ---------- 내부 유틸 ----------
        List<SkillDef> GenerateChoices(int count)
        {
            var list = new List<SkillDef>();
            if (skills == null || skills.skills == null) return list;

            // 업그레이드 가능 스킬만 필터 (level < cap)
            var pool = new List<SkillDef>();
            foreach (var s in skills.skills)
            {
                if (s?.def == null) continue;
                if (s.level < s.def.levelCap)
                    pool.Add(s.def);
            }
            if (pool.Count == 0) return list;

            // 중복 없이 랜덤 뽑기
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

            // Cheapest: 현재 레벨을 찾아 비용이 가장 낮은 스킬(=baseCost*(level+1)이 최소)
            int best = 0;
            int bestCost = int.MaxValue;
            foreach (var s in skills.skills)
            {
                int idx = list.IndexOf(s.def);
                if (idx < 0) continue;
                int cost = s.def.upgradeCostBase * (s.level + 1);  // 선형 비용  :contentReference[oaicite:11]{index=11}
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
            for (int i = 0; i < self.Count; i++) if (EqualityComparer<T>.Default.Equals(self[i], item)) return i;
            return -1;
        }
    }
}
