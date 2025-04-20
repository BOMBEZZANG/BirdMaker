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

    // === Unity 생명주기 함수 ===

    /// <summary>
    /// 게임 오브젝트가 처음 활성화되기 전에 호출됩니다.
    /// 싱글톤 인스턴스를 설정하고, 저장 경로를 설정하며, 게임 데이터를 로드합니다.
    /// </summary>
    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null) // 아직 전역 인스턴스가 없다면
        {
            Instance = this; // 이 인스턴스를 전역 인스턴스로 설정
            DontDestroyOnLoad(gameObject); // 씬 전환 시 이 오브젝트가 파괴되지 않도록 설정

            // 저장 파일 경로 설정 (Application.persistentDataPath는 플랫폼에 맞는 안전한 저장 경로)
            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            // Debug.Log($"데이터 저장 경로: {saveFilePath}"); // 경로 확인 필요 시 주석 해제

            LoadGameData(); // 게임 시작 시 저장된 데이터 로드 시도
        }
        else if (Instance != this) // 이미 다른 인스턴스가 존재한다면 (씬 중복 로드 등)
        {
            // 중복 생성된 이 인스턴스는 파괴
            Debug.LogWarning("[DataManager] 중복 인스턴스 발견. 새로 생성된 인스턴스를 파괴합니다.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 애플리케이션 종료 시 자동으로 게임 데이터를 저장합니다.
    /// (에디터에서는 Play 모드 중지 시, 빌드된 게임에서는 종료 시 호출됨)
    /// </summary>
    void OnApplicationQuit()
    {
        SaveGameData();
    }

    // === 저장 및 로드 핵심 함수 ===

    /// <summary>
    /// 파일 시스템에서 게임 데이터를 로드합니다.
    /// 파일이 없거나 읽기/파싱 오류 발생 시 기본값으로 새 게임 데이터를 생성합니다.
    /// </summary>
    private void LoadGameData()
    {
        // 저장 파일이 실제로 존재하는지 확인
        if (File.Exists(saveFilePath))
        {
            try
            {
                // 파일에서 JSON 텍스트 읽기
                string json = File.ReadAllText(saveFilePath);
                // JSON 텍스트를 GameData 객체로 변환 (역직렬화)
                CurrentGameData = JsonUtility.FromJson<GameData>(json);

                // JsonUtility는 실패 시 null을 반환할 수 있으므로 null 체크
                if (CurrentGameData == null)
                {
                     throw new System.Exception("저장 데이터를 파싱하는데 실패했거나 데이터가 null입니다.");
                }

                // 로드된 데이터 내부의 리스트가 null일 경우 빈 리스트로 초기화 (안전장치)
                if (CurrentGameData.featherPositions == null)
                    CurrentGameData.featherPositions = new List<Vector2>();
                if (CurrentGameData.mossPositions == null)
                    CurrentGameData.mossPositions = new List<Vector2>();

                // 로드 성공 로그 (더 자세한 정보 포함)
                Debug.Log($"게임 데이터 로드 성공. Branches: {CurrentGameData.branches}, Feathers: {CurrentGameData.feathers}, Moss: {CurrentGameData.moss}, NestBuilt: {CurrentGameData.nestBuilt}, EggWarmth: {CurrentGameData.eggWarmth}, EggHumidity: {CurrentGameData.eggHumidity}, PlacedFeathers: {CurrentGameData.featherPositions.Count}, PlacedMoss: {CurrentGameData.mossPositions.Count}");
            }
            catch (System.Exception e) // 파일 읽기 또는 JSON 파싱 중 오류 발생 시
            {
                Debug.LogError($"데이터 로드 중 오류 발생: {e.Message}. 새 게임 데이터를 생성합니다.");
                CreateNewGameData(); // 오류 발생 시 새 게임 데이터 생성
            }
        }
        else // 저장 파일이 존재하지 않을 경우
        {
            Debug.Log("저장된 게임 데이터 없음. 새 게임 데이터를 생성합니다.");
            CreateNewGameData(); // 새 게임 데이터 생성
        }
    }

    /// <summary>
    /// 현재 게임 상태(CurrentGameData)를 JSON 파일로 저장합니다.
    /// </summary>
    public void SaveGameData()
    {
        // 저장할 데이터 객체가 유효한지 확인
        if (CurrentGameData == null) {
             Debug.LogWarning("저장할 게임 데이터(CurrentGameData)가 null입니다. 저장을 건너<0xEB><0x9C><0x84>니다.");
             return;
        }

        try
        {
            // CurrentGameData 객체를 JSON 문자열로 변환 (true: 가독성 좋게 포맷팅)
            string json = JsonUtility.ToJson(CurrentGameData, true);
            // 파일에 JSON 문자열 쓰기 (기존 파일 있으면 덮어쓰기)
            File.WriteAllText(saveFilePath, json);
            // Debug.Log("게임 데이터 저장 성공."); // 저장 성공 로그 (필요 시 주석 해제)
        }
        catch (System.Exception e) // 파일 쓰기 중 오류 발생 시
        {
            Debug.LogError($"데이터 저장 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// 새로운 기본 게임 데이터를 생성하여 CurrentGameData에 할당합니다.
    /// </summary>
    private void CreateNewGameData()
    {
         CurrentGameData = new GameData(); // GameData 클래스의 생성자 호출 (기본값 설정)
    }

    // === 외부 스크립트에서 데이터 업데이트를 요청하는 함수들 ===
    // 다른 매니저(InventoryManager, EggController 등)에서 상태가 변경될 때마다
    // 이 함수들을 호출하여 DataManager의 CurrentGameData를 최신 상태로 유지합니다.

    /// <summary> 인벤토리 아이템 개수 업데이트 </summary>
    public void UpdateInventoryData(int branches, int feathers, int moss)
    {
        if (CurrentGameData != null)
        {
             CurrentGameData.branches = branches;
             CurrentGameData.feathers = feathers;
             CurrentGameData.moss = moss; // 인벤토리 이끼 개수
             // Debug.Log($"DataManager Updated Inventory: B={branches}, F={feathers}, M={moss}");
        }
    }

    /// <summary> 둥지 건설 상태 업데이트 </summary>
    public void UpdateNestStatus(bool isBuilt)
    {
        if (CurrentGameData != null) { CurrentGameData.nestBuilt = isBuilt; }
    }

    /// <summary> 알 온기 업데이트 </summary>
    public void UpdateEggData(float warmth) // 또는 UpdateEggWarmth
    {
        if (CurrentGameData != null) { CurrentGameData.eggWarmth = warmth; }
    }

    /// <summary> 알 습도 업데이트 </summary>
    public void UpdateEggHumidity(float humidity)
    {
        if (CurrentGameData != null) { CurrentGameData.eggHumidity = humidity; }
    }

    /// <summary> 배치된 깃털 위치 리스트 업데이트 </summary>
    public void UpdateFeatherPositions(List<Vector2> positions)
    {
         if (CurrentGameData != null)
         {
             // 리스트 내용을 복사하여 할당 (원본 리스트 직접 참조 방지)
             CurrentGameData.featherPositions = new List<Vector2>(positions ?? new List<Vector2>());
             // Debug.Log($"DataManager Updated Feather Positions: Count={CurrentGameData.featherPositions.Count}");
         }
    }

    /// <summary> 배치된 이끼 위치 리스트 업데이트 </summary>
    public void UpdateMossPositions(List<Vector2> positions)
    {
         if (CurrentGameData != null)
         {
             CurrentGameData.mossPositions = new List<Vector2>(positions ?? new List<Vector2>());
             // Debug.Log($"DataManager Updated Moss Positions: Count={CurrentGameData.mossPositions.Count}");
         }
    }
}