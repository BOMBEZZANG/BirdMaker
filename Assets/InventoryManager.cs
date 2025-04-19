using UnityEngine;

/// <summary>
/// 플레이어의 인벤토리(아이템 개수 등) 및 주요 상태(둥지 건설 여부)를 관리하는 싱글톤 클래스.
/// 씬 전환 시에도 유지되며, DataManager와 연동하여 데이터를 저장/로드합니다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static InventoryManager Instance { get; private set; }

    // === 아이템 개수 및 상태 변수 ===
    // 값은 DataManager로부터 로드되므로 초기값 설정은 Start()에서 처리합니다.
    public int branchCount { get; private set; }
    public int featherCount { get; private set; }
    public bool isNestBuilt { get; private set; }

    // === Unity 생명주기 함수 ===

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// DataManager가 로드를 완료한 후 호출되어 초기 상태를 설정합니다.
    /// </summary>
    void Start()
    {
        // DataManager 인스턴스가 있는지, 데이터 로드가 완료되었는지 확인
        if (DataManager.Instance != null && DataManager.Instance.CurrentGameData != null)
        {
            LoadDataFromDataManager();
        }
        else
        {
            // DataManager가 준비되지 않은 경우 기본값 사용 (새 게임 시작 시 해당)
            Debug.LogWarning("DataManager 또는 데이터가 준비되지 않아 InventoryManager 기본값 사용.");
            branchCount = 0;
            featherCount = 0;
            isNestBuilt = false;
            // 새 게임 상태를 DataManager에도 반영 (선택적이지만 권장)
            DataManager.Instance?.UpdateInventoryData(branchCount, featherCount);
            DataManager.Instance?.UpdateNestStatus(isNestBuilt);
        }
    }

    /// <summary>
    /// DataManager로부터 데이터를 로드하여 내부 변수를 설정합니다.
    /// </summary>
    private void LoadDataFromDataManager()
    {
        GameData data = DataManager.Instance.CurrentGameData;
        branchCount = data.branches;
        featherCount = data.feathers;
        isNestBuilt = data.nestBuilt;
        Debug.Log($"InventoryManager: 데이터 로드 완료 (Branches: {branchCount}, Feathers: {featherCount}, NestBuilt: {isNestBuilt})");
    }

    // === 나뭇가지 관련 함수 ===

    /// <summary>
    /// 인벤토리에 나뭇가지를 추가하고 DataManager에 변경사항을 알립니다.
    /// </summary>
    public void AddBranches(int amount)
    {
        if (amount <= 0) return;
        branchCount += amount;
        // DataManager의 데이터 업데이트 요청
        DataManager.Instance?.UpdateInventoryData(branchCount, featherCount);
        Debug.Log($"나뭇가지 {amount}개 획득! 현재 개수: {branchCount}");
        // TODO: 인벤토리 UI 업데이트
    }

    /// <summary>
    /// 지정된 개수만큼 나뭇가지를 사용하고 DataManager에 변경사항을 알립니다.
    /// </summary>
    public bool UseBranches(int amountToUse)
    {
        if (amountToUse <= 0) return false;

        if (branchCount >= amountToUse)
        {
            branchCount -= amountToUse;
            // DataManager의 데이터 업데이트 요청
            DataManager.Instance?.UpdateInventoryData(branchCount, featherCount);
            Debug.Log($"나뭇가지 {amountToUse}개 사용. 남은 개수: {branchCount}");
            // TODO: 인벤토리 UI 업데이트
            return true;
        }
        else
        {
            Debug.LogWarning($"나뭇가지 {amountToUse}개가 필요하지만, {branchCount}개만 가지고 있습니다.");
            return false;
        }
    }

    // === 깃털 관련 함수 ===

    /// <summary>
    /// 인벤토리에 깃털을 추가하고 DataManager에 변경사항을 알립니다.
    /// </summary>
    public void AddFeathers(int amount)
    {
        if (amount <= 0) return;
        featherCount += amount;
        // DataManager의 데이터 업데이트 요청
        DataManager.Instance?.UpdateInventoryData(branchCount, featherCount);
        Debug.Log($"깃털 {amount}개 획득! 현재 개수: {featherCount}");
        // TODO: 인벤토리 UI 업데이트
    }

    /// <summary>
    /// 인벤토리에서 깃털 1개를 사용하고 DataManager에 변경사항을 알립니다.
    /// </summary>
    public bool UseFeather()
    {
        if (featherCount > 0)
        {
            featherCount--;
            // DataManager의 데이터 업데이트 요청
            DataManager.Instance?.UpdateInventoryData(branchCount, featherCount);
            Debug.Log($"깃털 1개 사용. 남은 개수: {featherCount}");
            // TODO: 인벤토리 UI 업데이트
            return true;
        }
        else
        {
            Debug.LogWarning("사용할 깃털이 없습니다.");
            return false;
        }
    }

    // === 둥지 상태 관련 함수 ===

    /// <summary>
    /// 둥지 건설 상태를 설정하고 DataManager에 변경사항을 알립니다.
    /// </summary>
    public void SetNestBuilt(bool built)
    {
        // 상태가 실제로 변경될 때만 업데이트
        if (isNestBuilt == built) return;

        isNestBuilt = built;
        // DataManager의 데이터 업데이트 요청
        DataManager.Instance?.UpdateNestStatus(isNestBuilt);
        if (built)
        {
            Debug.Log("InventoryManager: 둥지가 건설되었습니다!");
        }
    }

    // OnApplicationQuit은 DataManager가 처리하므로 여기선 필요 없음
}