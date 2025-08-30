// SeedDef.cs
using UnityEngine;
// ��������������������������������������������������������������������������������������������������������������������������
// File: SeedDef.cs
// Purpose : ����/�۹��� �뷱�� �����͸� ScriptableObject�� ����.
// Defines : class SeedDef : ScriptableObject
// Fields  : seedId, displayName, preferred(��ȣ ����), growDays, sellPrice
// Used By : FarmingSystem(����/����/��Ȯ), LootTable(��� ���), ����/�κ��丮.
// Author  : ������ �ֵ� ����(��Ƽ��Ʈ/�뷱���� �����Ϳ��� ���� ����).
// ��������������������������������������������������������������������������������������������������������������������������

[CreateAssetMenu(menuName = "Game/Seed")]
public class SeedDef : ScriptableObject
{
    public string seedId;
    public string displayName;
    public WeatherType preferred;   // ������ ����
    public int growDays;            // �����ϼ�
    public int sellPrice;           // ���� �۹� �ǸŰ�
}
