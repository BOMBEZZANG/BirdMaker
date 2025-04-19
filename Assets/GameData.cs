using UnityEngine;
using System.Collections.Generic; // List 사용 위해 추가

/// <summary>
/// 게임 상태 저장을 위한 데이터 구조체(클래스)
/// 둥지 깃털 개수 대신 위치 리스트를 저장합니다.
/// </summary>
[System.Serializable]
public class GameData
{
    // 저장할 데이터 필드들
    public int branches;
    public int feathers; // 플레이어 인벤토리 깃털
    public bool nestBuilt;
    public float eggWarmth;
    // public int nestFeathers; // 개수 대신 위치 리스트 사용
    public List<Vector2> featherPositions; // *** 깃털 위치 리스트 추가 ***

    // 기본값 생성자 (새 게임 시작 시 사용)
    public GameData()
    {
        branches = 0;
        feathers = 0;
        nestBuilt = false;
        eggWarmth = 0f;
        // nestFeathers = 0;
        featherPositions = new List<Vector2>(); // 빈 리스트로 초기화
    }
}