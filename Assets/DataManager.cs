using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 게임 데이터의 저장 및 로드를 관리하는 싱글톤 클래스.
/// </summary>
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    public GameData CurrentGameData { get; private set; }
    private string saveFilePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            // Debug.Log($"데이터 저장 경로: {saveFilePath}");
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadGameData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                CurrentGameData = JsonUtility.FromJson<GameData>(json);
                if (CurrentGameData == null) // Json 파싱 실패 시 null일 수 있음
                {
                     throw new System.Exception("Failed to parse save data.");
                }
                // 로드 시 리스트 null 체크
                if (CurrentGameData.featherPositions == null)
                {
                    CurrentGameData.featherPositions = new List<Vector2>();
                }
                Debug.Log($"게임 데이터 로드 성공. Moss: {CurrentGameData.moss}, Humidity: {CurrentGameData.eggHumidity}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"데이터 로드 실패: {e.Message}. 새 데이터 생성.");
                CurrentGameData = new GameData(); // 실패 시 새 데이터
            }
        }
        else
        {
            Debug.Log("저장된 데이터 없음. 새 게임 데이터 생성.");
            CurrentGameData = new GameData(); // 저장 파일 없으면 새 데이터
        }
        // 로드/생성 후 초기 상태 반영 (각 매니저 Start에서 처리)
    }

    public void SaveGameData()
    {
        // CurrentGameData는 다른 매니저들이 업데이트 해준 최신 상태
        if (CurrentGameData == null) {
             Debug.LogError("저장할 게임 데이터(CurrentGameData)가 없습니다!");
             return;
        }
        try
        {
            string json = JsonUtility.ToJson(CurrentGameData, true);
            File.WriteAllText(saveFilePath, json);
            // Debug.Log("게임 데이터 저장 성공.");
        }
        catch (System.Exception e) { Debug.LogError($"데이터 저장 실패: {e.Message}"); }
    }

    // --- 데이터 업데이트 함수들 ---

    // *** 수정: moss 파라미터 추가 ***
    public void UpdateInventoryData(int branches, int feathers, int moss)
    {
        if (CurrentGameData != null)
        {
             CurrentGameData.branches = branches;
             CurrentGameData.feathers = feathers;
             CurrentGameData.moss = moss; // 이끼 개수 업데이트
        }
    }
    public void UpdateNestStatus(bool isBuilt)
    {
        if (CurrentGameData != null) { CurrentGameData.nestBuilt = isBuilt; }
    }
    public void UpdateEggData(float warmth) // 온기 업데이트 함수 이름 변경 고려 (UpdateEggWarmth)
    {
        if (CurrentGameData != null) { CurrentGameData.eggWarmth = warmth; }
    }
    // *** 새로 추가: 알 습도 업데이트 함수 ***
    public void UpdateEggHumidity(float humidity)
    {
        if (CurrentGameData != null) { CurrentGameData.eggHumidity = humidity; }
    }
    public void UpdateFeatherPositions(List<Vector2> positions)
    {
         if (CurrentGameData != null) { CurrentGameData.featherPositions = new List<Vector2>(positions); }
    }

    void OnApplicationQuit() { SaveGameData(); }
}