// ��������������������������������������������������������������������������������������������������������������������������
// File    : GameRoot.cs
// Purpose : ���� �Ŵ����� �̱��� �����̳�. �� �̵� �� ���� + �ߺ� ����.
// Defines : class GameRoot : MonoBehaviour (Singleton)
// Notes   : ���� �ϳ��� �μ���. �ߺ� ���� �� �ڽ��� �ı��մϴ�.
//           Start ȭ�鿡 �� ������ ����� �ٿ��ּ���!
//           �ڽĿ� ���� RunManager/Economy/���� �ڵ����� ã�� �輱�մϴ�.
// ��������������������������������������������������������������������������������������������������������������������������
using UnityEngine;
using Game.Core;
using Game.Systems;
using Game.Systems.Combat;

[DefaultExecutionOrder(-1000)] // ���� ���� �ߵ���
public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }

    [Header("Managers (����: ����θ� �ڵ� Ž��)")]
    public RunManager run;
    public Economy economy;
    public SkillSystem skills;
    public CombatSystem combat;
    public Inventory inventory;
    public UpgradeOfferManager upgrade;

    void Awake()
    {
        // �̱��� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ��� ������ �ڽĿ��� �ڵ� �輱(������/������ ���� true�� ������ �͵� ����)
        if (!run) run = GetComponentInChildren<RunManager>(true);
        if (!economy) economy = GetComponentInChildren<Economy>(true);
        if (!skills) skills = GetComponentInChildren<SkillSystem>(true);
        if (!combat) combat = GetComponentInChildren<CombatSystem>(true);
        if (!inventory) inventory = GetComponentInChildren<Inventory>(true);
        if (!upgrade) upgrade = GetComponentInChildren<UpgradeOfferManager>(true);
    }
}
