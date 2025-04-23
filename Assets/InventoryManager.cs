using UnityEngine;
using System; // Action 사용을 위해 추가

/// <summary>
/// 플레이어의 인벤토리(아이템 개수 등) 및 주요 상태(둥지 건설 여부, 도구 보유)를 관리하는 싱글톤 클래스.
/// 실제 데이터 저장은 DataManager에게 위임하고, 게임 로직에 데이터 접근/변경 인터페이스를 제공합니다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static InventoryManager Instance { get; private set; }

    // === 아이템 개수 및 상태 변수 (DataManager로부터 로드됨) ===
    public int branchCount { get; private set; }
    public int featherCount { get; private set; }
    public int mossCount { get; private set; }
    public bool isNestBuilt { get; private set; }

    // === 데이터 접근 편의를 위한 프로퍼티 (DataManager 값 실시간 읽기) ===
    /// <summary> 현재 플레이어 보유 재화 </summary>
    public int PlayerMoney => DataManager.Instance?.CurrentGameData?.playerMoney ?? 0;
    /// <summary> 온도계 보유 여부 </summary>
    public bool HasThermometer => DataManager.Instance?.CurrentGameData?.hasThermometer ?? false;
    /// <summary> 습도계 보유 여부 </summary>
    public bool HasHygrometer => DataManager.Instance?.CurrentGameData?.hasHygrometer ?? false;

    // 인벤토리 업데이트 이벤트 (UI 갱신용)
    public event Action OnInventoryUpdated;

    // === Unity 생명주기 함수 ===

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else if (Instance != this) { Debug.LogWarning("[InventoryManager] 중복 인스턴스 발견. 파괴합니다."); Destroy(gameObject); }
    }

    /// <summary> DataManager가 로드를 완료한 후 초기 상태 설정 </summary>
    void Start()
    {
        // DataManager로부터 데이터 로드 시도
        if (DataManager.Instance?.CurrentGameData != null) { LoadDataFromDataManager(); }
        else { Debug.LogWarning("DataManager/데이터 준비 안됨. InventoryManager 기본값 사용."); LoadDefaultData(); }
        // 시작 시 UI 업데이트를 위해 이벤트 한번 발생
        OnInventoryUpdated?.Invoke();
    }

    /// <summary> DataManager로부터 데이터 로드 </summary>
    private void LoadDataFromDataManager()
    {
        GameData data = DataManager.Instance.CurrentGameData;
        branchCount = data.branches;
        featherCount = data.feathers;
        mossCount = data.moss;
        isNestBuilt = data.nestBuilt;
        // Debug.Log($"InventoryManager: 데이터 로드 완료 (B:{branchCount}, F:{featherCount}, M:{mossCount}, Built:{isNestBuilt})");
    }

    /// <summary> 새 게임 시작 시 기본값 설정 </summary>
    private void LoadDefaultData()
    {
        branchCount = 0; featherCount = 0; mossCount = 0; isNestBuilt = false;
        // 새 게임 상태 DataManager에 반영 시도
        DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount);
        DataManager.Instance?.UpdateNestStatus(isNestBuilt);
        // 재화, 도구 상태는 GameData 생성자 및 DataManager Update 함수에서 기본값 처리
    }

    // === 아이템/상태 변경 함수 (DataManager 업데이트 및 이벤트 발생 포함) ===

    public void AddBranches(int amount) { if (amount <= 0) return; branchCount += amount; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); }
    public bool UseBranches(int amountToUse) { if (amountToUse <= 0) return false; if (branchCount >= amountToUse) { branchCount -= amountToUse; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); return true; } else { Debug.LogWarning($"나뭇가지 부족! 필요:{amountToUse}, 보유:{branchCount}"); return false; } }
    public void AddFeathers(int amount) { if (amount <= 0) return; featherCount += amount; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); }
    public bool UseFeather() { if (featherCount > 0) { featherCount--; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); return true; } else { Debug.LogWarning("사용할 깃털 없음."); return false; } }
    public void AddMoss(int amount) { if (amount <= 0) return; mossCount += amount; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); }
    public bool UseMoss() { if (mossCount > 0) { mossCount--; DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); OnInventoryUpdated?.Invoke(); return true; } else { Debug.LogWarning("사용할 이끼 없음."); return false; } }
    public void SetNestBuilt(bool built) { if (isNestBuilt == built) return; isNestBuilt = built; DataManager.Instance?.UpdateNestStatus(isNestBuilt); /* Debug.Log($"둥지 상태 변경: {isNestBuilt}"); */ }


    // === 재화 관련 함수 ===
    /// <summary> 플레이어에게 재화를 추가하고 DataManager에 알립니다. </summary>
    public void AddMoney(int amount)
    {
        if (amount <= 0 || DataManager.Instance == null) return;
        int currentMoney = PlayerMoney; // 현재 돈 가져오기 (DataManager 통해)
        int newMoney = currentMoney + amount;
        DataManager.Instance.UpdatePlayerMoney(newMoney); // DataManager에 업데이트 요청
        // Debug.Log($"재화 +{amount}. 현재 재화: {newMoney}"); // 필요 시 로그
        OnInventoryUpdated?.Invoke(); // 재화 변경도 UI 업데이트 트리거
    }

    /// <summary> 플레이어의 재화를 사용(차감)하고 DataManager에 알립니다. </summary>
    /// <returns> 사용에 성공하면 true, 돈이 부족하면 false </returns>
    public bool SpendMoney(int amount)
    {
        if (amount <= 0 || DataManager.Instance == null) return false;
        int currentMoney = PlayerMoney;
        if (currentMoney >= amount) {
            int newMoney = currentMoney - amount;
            DataManager.Instance.UpdatePlayerMoney(newMoney); // DataManager에 업데이트 요청
            // Debug.Log($"재화 -{amount}. 현재 재화: {newMoney}"); // 필요 시 로그
            OnInventoryUpdated?.Invoke(); // 재화 변경도 UI 업데이트 트리거
            return true;
        } else { Debug.LogWarning($"재화 부족! 필요: {amount}, 보유: {currentMoney}"); return false; }
    }

    // === 도구 관련 함수 ===
    /// <summary> 온도계를 획득 처리하고 DataManager에 알립니다. </summary>
    public void AcquireThermometer()
    {
        if(DataManager.Instance != null && !HasThermometer) // DataManager 통해 상태 확인
        {
             DataManager.Instance.SetHasThermometer(true); // DataManager 통해 상태 변경
             Debug.Log("온도계를 획득했습니다!");
             OnInventoryUpdated?.Invoke(); // 도구 획득도 UI 업데이트 트리거 (예: 상점 구매 버튼 비활성화)
        }
    }
     /// <summary> 습도계를 획득 처리하고 DataManager에 알립니다. </summary>
    public void AcquireHygrometer()
    {
        if(DataManager.Instance != null && !HasHygrometer) // DataManager 통해 상태 확인
        {
             DataManager.Instance.SetHasHygrometer(true); // DataManager 통해 상태 변경
             Debug.Log("습도계를 획득했습니다!");
             OnInventoryUpdated?.Invoke(); // UI 업데이트 트리거
        }
    }
}