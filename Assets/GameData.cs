using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // 인벤토리 및 둥지 상태
    public int branches;
    public int feathers;
    public int moss;
    public bool nestBuilt;
    public int playerMoney;
    public bool hasThermometer;
    public bool hasHygrometer;

    // 배치된 아이템 위치
    public List<Vector2> featherPositions;
    public List<Vector2> mossPositions;

    // 둥지 환경 상태
    public float nestTemperature;
    public float nestHumidity;

    // *** 알 성장 및 부화 상태 (eggGrowthPoints 다시 추가) ***
    public float eggGrowthPoints; // 현재 누적된 성장 포인트
    public bool eggHasHatched;   // 알이 부화했는지 여부

    // 기본값 생성자
    public GameData()
    {
        branches = 0; feathers = 0; moss = 0;
        nestBuilt = false; playerMoney = 10;
        hasThermometer = false; hasHygrometer = false;
        featherPositions = new List<Vector2>();
        mossPositions = new List<Vector2>();
        nestTemperature = 15f; // 기본 주변 온도
        nestHumidity = 40f;    // 기본 주변 습도
        eggGrowthPoints = 0f; // *** 성장 포인트 초기화 추가 ***
        eggHasHatched = false;
    }
}