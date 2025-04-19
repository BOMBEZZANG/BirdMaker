using UnityEngine;
using System; // Action 사용을 위해 추가

/// <summary>
/// 플레이어의 인벤토리(아이템 개수 등) 및 주요 상태(둥지 건설 여부)를 관리하는 싱글톤 클래스.
/// DataManager와 연동하여 데이터를 저장/로드합니다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static InventoryManager Instance { get; private set; }

    // 아이템 개수 및 상태 변수 (DataManager로부터 로드)
    public int branchCount { get; private set; }
    public int featherCount { get; private set; }
    public int mossCount { get; private set; }
    public bool isNestBuilt { get; private set; }

    // 인벤토리 업데이트 이벤트 (UI 갱신용)
    public event Action OnInventoryUpdated;

    // === Unity 생명주기 함수 ===

    /// <summary>
    /// 싱글톤 인스턴스를 설정하고 씬 전환 시 파괴되지 않도록 합니다.
    /// </summary>
    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            // 인스턴스가 없다면, 이 인스턴스를 싱글톤 인스턴스로 설정
            Instance = this;
            // *** 중요: 씬이 전환될 때 이 게임 오브젝트가 파괴되지 않도록 설정 ***
            DontDestroyOnLoad(gameObject);
            // Debug.Log("[InventoryManager] Awake: Instance assigned and DontDestroyOnLoad called."); // 확인용 로그
        }
        else if (Instance != this)
        {
            // 이미 다른 인스턴스가 존재한다면, 이 인스턴스는 중복이므로 파괴
            Debug.LogWarning("[InventoryManager] Awake: Duplicate instance detected. Destroying self.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// DataManager가 로드를 완료한 후 호출되어 초기 상태를 설정합니다.
    /// </summary>
    void Start()
    {
        // DataManager 인스턴스가 있는지, 데이터 로드가 완료되었는지 확인
        // DataManager가 먼저 실행되도록 실행 순서 설정 필요
        if (DataManager.Instance != null && DataManager.Instance.CurrentGameData != null)
        {
            LoadDataFromDataManager(); // 저장된 데이터 로드
        }
        else
        {
            // DataManager가 준비되지 않은 경우 기본값 사용 (새 게임 시작 시)
            Debug.LogWarning("DataManager 또는 데이터가 준비되지 않아 InventoryManager 기본값 사용.");
            LoadDefaultData(); // 기본값 설정 함수 호출
        }
        // 시작 시 UI 업데이트를 위해 이벤트 한번 발생
        OnInventoryUpdated?.Invoke();
    }

    /// <summary>
    /// DataManager로부터 데이터를 로드하여 내부 변수를 설정합니다.
    /// </summary>
    private void LoadDataFromDataManager()
    {
        GameData data = DataManager.Instance.CurrentGameData;
        branchCount = data.branches;
        featherCount = data.feathers;
        mossCount = data.moss; // 이끼 개수 로드
        isNestBuilt = data.nestBuilt;
        // Debug.Log($"InventoryManager: 데이터 로드 완료 (Branches: {branchCount}, Feathers: {featherCount}, Moss: {mossCount}, NestBuilt: {isNestBuilt})");
    }

    /// <summary>
    /// 새 게임 시작 시 사용할 기본 데이터를 설정합니다.
    /// </summary>
    private void LoadDefaultData()
    {
        branchCount = 0;
        featherCount = 0;
        mossCount = 0;
        isNestBuilt = false;
        // 새 게임 상태를 DataManager에도 반영 시도 (DataManager가 준비되면 덮어쓰여질 수 있음)
        DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount);
        DataManager.Instance?.UpdateNestStatus(isNestBuilt);
    }

    // === 데이터 변경 및 이벤트 발생 함수 ===

    public void AddBranches(int amount)
    {
        if (amount <= 0) return;
        branchCount += amount;
        DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount); // 데이터 매니저 업데이트
        OnInventoryUpdated?.Invoke(); // UI 갱신 등을 위한 이벤트 발생
        // Debug.Log($"나뭇가지 {amount}개 획득! 현재 개수: {branchCount}");
    }

    public bool UseBranches(int amountToUse)
    {
        if (amountToUse <= 0) return false;
        if (branchCount >= amountToUse) {
            branchCount -= amountToUse;
            DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount);
            OnInventoryUpdated?.Invoke();
            // Debug.Log($"나뭇가지 {amountToUse}개 사용...");
            return true;
        } else { Debug.LogWarning($"나뭇가지 {amountToUse}개 필요, {branchCount}개 보유 중."); return false; }
    }

    public void AddFeathers(int amount)
    {
        if (amount <= 0) return;
        featherCount += amount;
        DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount);
        OnInventoryUpdated?.Invoke();
        // Debug.Log($"깃털 {amount}개 획득! 현재 개수: {featherCount}");
    }

    public bool UseFeather()
    {
        if (featherCount > 0) {
            featherCount--;
            DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount);
            OnInventoryUpdated?.Invoke();
            // Debug.Log($"깃털 1개 사용...");
            return true;
        } else { Debug.LogWarning("사용할 깃털이 없습니다."); return false; }
    }

    public void AddMoss(int amount)
    {
        if (amount <= 0) return;
        mossCount += amount;
        DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount);
        OnInventoryUpdated?.Invoke();
        // Debug.Log($"이끼 {amount}개 획득! 현재 개수: {mossCount}");
    }

    public bool UseMoss()
    {
        if (mossCount > 0) {
            mossCount--;
            DataManager.Instance?.UpdateInventoryData(branchCount, featherCount, mossCount);
            OnInventoryUpdated?.Invoke();
            // Debug.Log($"이끼 1개 사용...");
            return true;
        } else { Debug.LogWarning("사용할 이끼가 없습니다."); return false; }
    }

    public void SetNestBuilt(bool built)
    {
        if (isNestBuilt == built) return;
        isNestBuilt = built;
        DataManager.Instance?.UpdateNestStatus(isNestBuilt);
        // if (built) { Debug.Log("InventoryManager: 둥지가 건설되었습니다!"); }
    }
}