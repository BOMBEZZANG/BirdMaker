using UnityEngine;
using System.Collections.Generic; // List 사용 위해 추가

/// <summary>
/// 게임 상태 저장을 위한 데이터 구조체(클래스)
/// </summary>
[System.Serializable]
public class GameData
{
    // 저장할 데이터 필드들
    public int branches;
    public int feathers; // 플레이어 인벤토리 깃털
    // *** 중요: 인벤토리 이끼 개수 변수 ***
    public int moss;     // 플레이어 인벤토리 이끼 개수
    public bool nestBuilt;
    public float eggWarmth;
    public float eggHumidity; // 알의 현재 습도
    public List<Vector2> featherPositions; // 둥지 안 깃털 위치 리스트
    public List<Vector2> mossPositions; // 둥지 안 이끼 위치 리스트

    // 기본값 생성자 (새 게임 시작 시 사용)
    public GameData()
    {
        branches = 0;
        feathers = 0;
        moss = 0; // *** 이끼 개수 초기화 추가 ***
        nestBuilt = false;
        eggWarmth = 0f;
        eggHumidity = 50f; // 습도 초기값
        featherPositions = new List<Vector2>();
        mossPositions = new List<Vector2>();
    }
}