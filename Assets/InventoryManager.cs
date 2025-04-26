using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// 플레이어의 인벤토리(아이템 개수 등) 및 주요 상태(둥지 건설 여부, 도구 보유)를 관리하는 싱글톤 클래스.
/// DataManager와 연동하며, 주요 상태는 내부 변수에 캐싱하여 사용합니다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public int branchCount { get; private set; }
    public int featherCount { get; private set; }
    public int mossCount { get; private set; }
    public bool isNestBuilt { get; private set; }
    private bool cachedHasThermometer;
    private bool cachedHasHygrometer;

    public int PlayerMoney => DataManager.Instance?.CurrentGameData?.playerMoney ?? 0;
    // *** Getter에 로그 추가 (디버깅용, 확인 후 주석 처리 권장) ***
    public bool HasThermometer {
        get {
            // Debug.Log($"Getter HasThermometer returning: {cachedHasThermometer}");
            return cachedHasThermometer;
        }
    }
    public bool HasHygrometer {
        get {
            // Debug.Log($"Getter HasHygrometer returning: {cachedHasHygrometer}");
            return cachedHasHygrometer;
        }
    }

    public event Action OnInventoryUpdated;
    public bool IsInventoryInitialized { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else if (Instance != this) { Destroy(gameObject); }
    }

    void Start()
    {
        StartCoroutine(InitializeInventory());
    }

    private IEnumerator InitializeInventory()
    {
         float timeout = Time.time + 5f;
         while ((DataManager.Instance == null || !DataManager.Instance.IsDataManagerInitialized) && Time.time < timeout) { yield return null; }

         if(DataManager.Instance == null || !DataManager.Instance.IsDataManagerInitialized) { Debug.LogError("InventoryManager: DataManager 초기화 대기 시간 초과!"); LoadDefaultData(); }
         else if (DataManager.Instance.CurrentGameData != null) { LoadDataFromDataManager(); }
         else { Debug.LogWarning("InventoryManager: DataManager 데이터가 null. 기본값 사용."); LoadDefaultData(); }

         IsInventoryInitialized = true;
         OnInventoryUpdated?.Invoke();
    }

    /// <summary> DataManager로부터 데이터 로드 및 내부 변수(캐시) 업데이트 </summary>
    private void LoadDataFromDataManager()
    {
        GameData data = DataManager.Instance.CurrentGameData;
        branchCount = data.branches;
        featherCount = data.feathers;
        mossCount = data.moss;
        isNestBuilt = data.nestBuilt;
        cachedHasThermometer = data.hasThermometer;
        cachedHasHygrometer = data.hasHygrometer;
        // *** 추가된 로그: 로드 시 캐시된 값 확인 ***
        Debug.Log($"InventoryManager: LoadDataFromDataManager completed. Cached Thermo: {cachedHasThermometer}, Cached Hygro: {cachedHasHygrometer}");
    }

    private void LoadDefaultData() { /* ... 이전과 동일 ... */
        branchCount = 0; featherCount = 0; mossCount = 0; isNestBuilt = false;
        cachedHasThermometer = false; cachedHasHygrometer = false;
        DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount);
        DataManager.Instance?.UpdateNestStatus(isNestBuilt);
        DataManager.Instance?.SetHasThermometer(cachedHasThermometer);
        DataManager.Instance?.SetHasHygrometer(cachedHasHygrometer);
    }

    // --- 아이템/상태 변경 함수 ---
    public void AddBranches(int amount) { if (amount <= 0) return; branchCount += amount; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); }
    public bool UseBranches(int amountToUse) { if (amountToUse <= 0) return false; if (branchCount >= amountToUse) { branchCount -= amountToUse; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); return true; } else { Debug.LogWarning($"나뭇가지 부족! 필요:{amountToUse}, 보유:{branchCount}"); return false; } }
    public void AddFeathers(int amount) { if (amount <= 0) return; featherCount += amount; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); }
    public bool UseFeather() { if (featherCount > 0) { featherCount--; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); return true; } else { Debug.LogWarning("사용할 깃털 없음."); return false; } }
    public void AddMoss(int amount) { if (amount <= 0) return; mossCount += amount; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); }
    public bool UseMoss() { if (mossCount > 0) { mossCount--; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); return true; } else { Debug.LogWarning("사용할 이끼 없음."); return false; } }
    public void SetNestBuilt(bool built) { if (isNestBuilt == built) return; isNestBuilt = built; DataManager.Instance?.UpdateNestStatus(isNestBuilt); }
    public void AddMoney(int amount) { if (amount <= 0 || DataManager.Instance == null) return; DataManager.Instance.UpdatePlayerMoney(PlayerMoney + amount); OnInventoryUpdated?.Invoke(); }
    public bool SpendMoney(int amount) { if (amount <= 0 || DataManager.Instance == null) return false; if (PlayerMoney >= amount) { DataManager.Instance.UpdatePlayerMoney(PlayerMoney - amount); OnInventoryUpdated?.Invoke(); return true; } else { Debug.LogWarning($"재화 부족! 필요: {amount}, 보유: {PlayerMoney}"); return false; } }
    public void AcquireThermometer() { if(DataManager.Instance != null && !cachedHasThermometer) { DataManager.Instance.SetHasThermometer(true); cachedHasThermometer = true; Debug.Log("온도계를 획득했습니다!"); OnInventoryUpdated?.Invoke(); } }
    public void AcquireHygrometer() { if(DataManager.Instance != null && !cachedHasHygrometer) { DataManager.Instance.SetHasHygrometer(true); cachedHasHygrometer = true; Debug.Log("습도계를 획득했습니다!"); OnInventoryUpdated?.Invoke(); } }
}