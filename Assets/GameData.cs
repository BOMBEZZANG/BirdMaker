using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 상태 저장을 위한 데이터 구조체(클래스)
/// </summary>
[System.Serializable]
public class GameData
{
    // 기존 데이터
    public int branches;
    public int feathers; // 플레이어 인벤토리 깃털
    public bool nestBuilt;
    public float eggWarmth;
    public List<Vector2> featherPositions; // 둥지 안 깃털 위치 리스트

    // *** 새로 추가된 데이터 ***
    public int moss; // 플레이어 인벤토리 이끼 개수
    public float eggHumidity; // 알의 현재 습도

    // 기본값 생성자 (새 게임 시작 시 사용)
    public GameData()
    {
        branches = 0;
        feathers = 0;
        moss = 0; // 이끼 초기값
        nestBuilt = false;
        eggWarmth = 0f;
        eggHumidity = 50f; // 습도 초기값 (예시: 중간값 50)
        featherPositions = new List<Vector2>();
    }
}