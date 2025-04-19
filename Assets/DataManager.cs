using UnityEngine;
using System.IO;
using System.Collections.Generic; // List 사용 위해 추가

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
            // Debug.Log($"데이터 저장 경로: {saveFilePath}"); // 필요 시 활성화
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
                // 로드 시 featherPositions 리스트가 null이면 빈 리스트로 초기화 (안전장치)
                if (CurrentGameData.featherPositions == null)
                {
                    CurrentGameData.featherPositions = new List<Vector2>();
                }
                Debug.Log($"게임 데이터 로드 성공. Feather Positions Count: {CurrentGameData.featherPositions.Count}");
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
    }

    public void SaveGameData()
    {
        try
        {
            // 이제 CurrentGameData의 featherPositions는 NestInteraction에서 최신 상태로 업데이트 해줘야 함
            string json = JsonUtility.ToJson(CurrentGameData, true);
            File.WriteAllText(saveFilePath, json);
            // Debug.Log("게임 데이터 저장 성공."); // 필요 시 활성화
        }
        catch (System.Exception e) { Debug.LogError($"데이터 저장 실패: {e.Message}"); }
    }

    // --- 다른 스크립트에서 현재 게임 데이터를 업데이트하는 함수들 ---

    public void UpdateInventoryData(int branches, int feathers)
    {
        if (CurrentGameData != null) { CurrentGameData.branches = branches; CurrentGameData.feathers = feathers; }
    }
    public void UpdateNestStatus(bool isBuilt)
    {
        if (CurrentGameData != null) { CurrentGameData.nestBuilt = isBuilt; }
    }
     public void UpdateEggData(float warmth)
    {
        if (CurrentGameData != null) { CurrentGameData.eggWarmth = warmth; }
    }

    // *** 수정: 둥지 깃털 개수 대신 위치 리스트 업데이트 함수 ***
    /// <summary>
    /// 현재 둥지에 배치된 깃털들의 위치 리스트를 업데이트합니다.
    /// </summary>
    /// <param name="positions">현재 깃털들의 Vector2 위치 리스트</param>
    public void UpdateFeatherPositions(List<Vector2> positions)
    {
         if (CurrentGameData != null)
         {
            // 리스트 자체를 교체하거나 내용을 복사할 수 있음. 여기서는 교체.
            CurrentGameData.featherPositions = new List<Vector2>(positions); // 새 리스트로 복사하여 할당
            // Debug.Log($"DataManager: 깃털 위치 업데이트됨. 개수: {CurrentGameData.featherPositions.Count}"); // 필요 시 활성화
         }
    }

    void OnApplicationQuit() { SaveGameData(); }
}