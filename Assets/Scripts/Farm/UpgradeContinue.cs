// ��������������������������������������������������������������������������������������������������������������������������
// File    : UpgradeContinue.cs
// Purpose : ���׷��̵� UI���� "���/��������" ��ư�� ������ ���� �������� �̵�
// ��������������������������������������������������������������������������������������������������������������������������
using UnityEngine;
using Game.Core;

public class UpgradeContinue : MonoBehaviour
{
    public void ContinueToDungeon()
    {
        SceneBridge.GoToDungeon();
    }
}
