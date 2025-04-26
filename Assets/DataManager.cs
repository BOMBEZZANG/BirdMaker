using UnityEngine;
using System.IO; // 파일 입출력(File, Path) 사용을 위해 필수
using System.Collections.Generic; // List 사용을 위해 필수

/// <summary>
/// 게임 데이터의 저장 및 로드를 관리하는 싱글톤 클래스.
/// GameData 객체를 통해 게임 상태를 관리하고 JSON으로 저장/로드합니다.
/// </summary>
public class DataManager : MonoBehaviour
{
    // 싱글톤 인스턴스: 외부에서는 DataManager.Instance 로 접근
    public static DataManager Instance { get; private set; }

    // 현재 게임 상태 데이터를 담는 변수: 외부에서는 읽기 전용으로 접근
    public GameData CurrentGameData { get; private set; }

    // 저장 파일 경로 변수
    private string saveFilePath;

    // 초기화 완료 플래그
    public bool IsDataManagerInitialized { get; private set; } = false;

    // === Unity 생명주기 함수 ===

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null) // 아직 전역 인스턴스가 없다면
        {
            Instance = this; // 이 인스턴스를 전역 인스턴스로 설정
            DontDestroyOnLoad(gameObject); // 씬 전환 시 이 오브젝트가 파괴되지 않도록 설정

            // 저장 파일 경로 설정
            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            // Debug.Log($"데이터 저장 경로: {saveFilePath}");

            LoadGameData(); // 게임 시작 시 저장된 데이터 로드 시도

            // Awake 끝에서 초기화 완료 설정 (LoadGameData는 동기적으로 실행됨)
            IsDataManagerInitialized = true;
            // Debug.Log("[DataManager] Initialized flag set.");
        }
        else if (Instance != this) // 이미 다른 인스턴스가 존재한다면
        {
            // 중복 인스턴스 파괴
            Debug.LogWarning("[DataManager] 중복 인스턴스 발견. 새로 생성된 인스턴스를 파괴합니다.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 애플리케이션 종료 시 자동 저장
    /// </summary>
    void OnApplicationQuit()
    {
        SaveGameData();
    }

    // === 저장 및 로드 핵심 함수 ===

    /// <summary>
    /// 파일에서 게임 데이터를 로드합니다. 오류 시 새 데이터를 생성합니다.
    /// </summary>
    private void LoadGameData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                CurrentGameData = JsonUtility.FromJson<GameData>(json);
                if (CurrentGameData == null) throw new System.Exception("Parsed data is null.");
                // 리스트 null 체크
                if (CurrentGameData.featherPositions == null) CurrentGameData.featherPositions = new List<Vector2>();
                if (CurrentGameData.mossPositions == null) CurrentGameData.mossPositions = new List<Vector2>();

                // *** 로그 수정: 알 온습도 대신 둥지 온습도 표시 ***
                Debug.Log($"게임 데이터 로드 성공. Money: {CurrentGameData.playerMoney}, HasThermo: {CurrentGameData.hasThermometer}, HasHygro: {CurrentGameData.hasHygrometer}, Branches: {CurrentGameData.branches}, Feathers: {CurrentGameData.feathers}, Moss: {CurrentGameData.moss}, NestBuilt: {CurrentGameData.nestBuilt}, NestTemp: {CurrentGameData.nestTemperature:F1}, NestHumid: {CurrentGameData.nestHumidity:F1}, Growth: {CurrentGameData.eggGrowthPoints:F1}, Hatched: {CurrentGameData.eggHasHatched}, PlacedFeathers: {CurrentGameData.featherPositions.Count}, PlacedMoss: {CurrentGameData.mossPositions.Count}");
            }
            catch (System.Exception e) { Debug.LogError($"데이터 로드 실패: {e.Message}. 새 데이터 생성."); CreateNewGameData(); }
        }
        else { Debug.Log("저장된 게임 데이터 없음. 새 게임 데이터 생성."); CreateNewGameData(); }
    }

    /// <summary>
    /// 현재 게임 데이터를 JSON 파일로 저장합니다.
    /// </summary>
    public void SaveGameData()
    {
        if (CurrentGameData == null) { Debug.LogWarning("저장할 게임 데이터(CurrentGameData)가 null입니다."); return; }
        try { string json = JsonUtility.ToJson(CurrentGameData, true); File.WriteAllText(saveFilePath, json); }
        catch (System.Exception e) { Debug.LogError($"데이터 저장 실패: {e.Message}"); }
    }

    /// <summary> 새로운 기본 게임 데이터를 생성합니다. </summary>
    private void CreateNewGameData() { CurrentGameData = new GameData(); }

    // === 외부에서 데이터 업데이트를 요청하는 함수들 ===

    /// <summary> 인벤토리 아이템 개수 업데이트 </summary>
    public void UpdateInventoryData(int branches, int feathers, int moss) { if (CurrentGameData != null) { CurrentGameData.branches = branches; CurrentGameData.feathers = feathers; CurrentGameData.moss = moss; } }
    /// <summary> 둥지 건설 상태 업데이트 </summary>
    public void UpdateNestStatus(bool isBuilt) { if (CurrentGameData != null) { CurrentGameData.nestBuilt = isBuilt; } }
    /// <summary> 배치된 깃털 위치 리스트 업데이트 </summary>
    public void UpdateFeatherPositions(List<Vector2> positions) { if (CurrentGameData != null) { CurrentGameData.featherPositions = new List<Vector2>(positions ?? new List<Vector2>()); } }
    /// <summary> 배치된 이끼 위치 리스트 업데이트 </summary>
    public void UpdateMossPositions(List<Vector2> positions) { if (CurrentGameData != null) { CurrentGameData.mossPositions = new List<Vector2>(positions ?? new List<Vector2>()); } }
    /// <summary> 알 성장 포인트 업데이트 (EggController가 사용) </summary>
    public void UpdateEggGrowth(float points) { if (CurrentGameData != null) { CurrentGameData.eggGrowthPoints = points; } }
    /// <summary> 알 부화 상태 업데이트 (EggController가 사용) </summary>
    public void UpdateEggHatchedStatus(bool hatched) { if (CurrentGameData != null) { CurrentGameData.eggHasHatched = hatched; } }
    /// <summary> 플레이어 재화 업데이트 </summary>
    public void UpdatePlayerMoney(int money) { if (CurrentGameData != null) { CurrentGameData.playerMoney = Mathf.Max(0, money); } }
    /// <summary> 특정 도구 보유 상태 업데이트 (온도계) </summary>
    public void SetHasThermometer(bool has) { if(CurrentGameData != null) CurrentGameData.hasThermometer = has; }
    /// <summary> 특정 도구 보유 상태 업데이트 (습도계) </summary>
    public void SetHasHygrometer(bool has) { if(CurrentGameData != null) CurrentGameData.hasHygrometer = has; }

    // --- 제거된 함수들 ---
    // public void UpdateEggData(float warmth) // 제거됨
    // public void UpdateEggHumidity(float humidity) // 제거됨

    // *** 추가/확인된 함수: 둥지 환경 업데이트 ***
    /// <summary>
    /// 둥지의 현재 온도와 습도를 업데이트합니다. (NestEnvironmentManager에서 호출)
    /// </summary>
    public void UpdateNestEnvironment(float temperature, float humidity)
    {
         if (CurrentGameData != null)
         {
             CurrentGameData.nestTemperature = temperature;
             CurrentGameData.nestHumidity = humidity;
             // Debug.Log($"DataManager Updated Nest Environment: Temp={temperature:F1}, Humid={humidity:F1}");
         }
    }
}