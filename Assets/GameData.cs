using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // 기존 데이터
    public int branches;
    public int feathers;
    public int moss; // 인벤토리 개수
    public bool nestBuilt;
    public float eggWarmth;
    public float eggHumidity;
    public List<Vector2> featherPositions;
    public List<Vector2> mossPositions;
    public float eggGrowthPoints;
    public bool eggHasHatched;
    public int playerMoney; // 플레이어 재화

    // *** 새로 추가: 도구 보유 여부 ***
    public bool hasThermometer;
    public bool hasHygrometer;

    // 기본값 생성자
    public GameData()
    {
        branches = 0; feathers = 0; moss = 0;
        nestBuilt = false; eggWarmth = 0f; eggHumidity = 50f;
        featherPositions = new List<Vector2>();
        mossPositions = new List<Vector2>();
        eggGrowthPoints = 0f; eggHasHatched = false;
        playerMoney = 10; // *** 초기 자금 약간 지급 (예: 10) ***
        hasThermometer = false; // 도구 초기 상태
        hasHygrometer = false;  // 도구 초기 상태
    }
}