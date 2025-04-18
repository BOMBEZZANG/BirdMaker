using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // 나뭇가지 개수 (기존)
    public int branchCount { get; private set; } = 0;

    // *** 새로 추가: 둥지가 건설되었는지 여부 ***
    public bool isNestBuilt { get; private set; } = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Debug.Log("InventoryManager 초기화 완료."); // 로그 레벨 조절
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // 나뭇가지 추가 함수 (기존)
    public void AddBranches(int amount)
    {
        if (amount <= 0) return;
        branchCount += amount;
        Debug.Log($"나뭇가지 {amount}개 획득! 현재 개수: {branchCount}");
    }

    // *** 수정: 나뭇가지 '1개' 사용 함수 (이름 명확화) ***
    // 이제 둥지 건설에 여러 개를 사용하므로, 1개 사용 함수는 필요 없을 수 있거나
    // 나중에 다른 용도로 사용될 수 있습니다. 일단 주석 처리하거나 삭제 가능.
    /*
    public bool UseBranch()
    {
        if (branchCount > 0)
        {
            branchCount--;
            Debug.Log($"나뭇가지 1개 사용. 남은 개수: {branchCount}");
            return true;
        }
        else
        {
            Debug.LogWarning("사용할 나뭇가지가 없습니다.");
            return false;
        }
    }
    */

    // *** 새로 추가: 지정된 개수만큼 나뭇가지 사용 함수 ***
    /// <summary>
    /// 지정된 개수만큼 나뭇가지를 사용합니다. 충분한 개수가 있어야만 사용됩니다.
    /// </summary>
    /// <param name="amountToUse">사용할 나뭇가지 개수</param>
    /// <returns>사용에 성공하면 true, 개수가 부족하면 false</returns>
    public bool UseBranches(int amountToUse)
    {
        if (amountToUse <= 0) return false; // 0개 이하는 사용할 수 없음

        if (branchCount >= amountToUse)
        {
            branchCount -= amountToUse;
            Debug.Log($"나뭇가지 {amountToUse}개 사용. 남은 개수: {branchCount}");
            return true;
        }
        else
        {
            Debug.LogWarning($"나뭇가지 {amountToUse}개가 필요하지만, {branchCount}개만 가지고 있습니다.");
            return false;
        }
    }

    // *** 새로 추가: 둥지 건설 상태 설정 함수 ***
    /// <summary>
    /// 둥지 건설 상태를 설정합니다.
    /// </summary>
    /// <param name="built">true면 건설 완료, false면 미완료</param>
    public void SetNestBuilt(bool built)
    {
        isNestBuilt = built;
        if (built)
        {
            Debug.Log("둥지가 성공적으로 건설되었습니다!");
        }
    }
}