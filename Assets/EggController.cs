using UnityEngine;
using UnityEngine.EventSystems;

public class EggController : MonoBehaviour, IPointerClickHandler
{
    [Header("Egg State")]
    [SerializeField] private float currentWarmth = 0f;
    [SerializeField] private float targetWarmth = 100f;

    // *** 새로 추가: 둥지 건설 관련 설정 ***
    [Header("Nest Building")]
    [Tooltip("둥지를 만드는 데 필요한 나뭇가지 개수입니다.")]
    [SerializeField] private int branchesNeededForNest = 10;
    [Tooltip("활성화시킬 둥지 게임 오브젝트입니다.")]
    [SerializeField] private GameObject nestObject; // Inspector에서 연결 필요!

    // ... (Start, Update 함수는 필요 시 유지) ...

    // 온기 추가 함수 (둥지 건설 후 사용될 수 있음)
    public void AddWarmth(float amount)
    {
        // 둥지가 건설된 후에만 온기를 추가하도록 제한할 수도 있습니다.
        if (InventoryManager.Instance != null && InventoryManager.Instance.isNestBuilt)
        {
             if (amount <= 0) return;
             currentWarmth += amount;
             Debug.Log($"[{this.gameObject.name}] 온기 +{amount}! 현재 온기: {currentWarmth}");
             // 부화 조건 체크 등
        } else {
            Debug.LogWarning("둥지가 없으면 온기를 추가할 수 없습니다.");
        }
    }

    // --- 알 클릭 시 로직 변경 ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        Debug.Log($"[{this.gameObject.name}] 알 클릭됨!");

        // 인벤토리 매니저가 있는지 확인
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        // 1. 둥지가 이미 지어졌는지 확인
        if (InventoryManager.Instance.isNestBuilt)
        {
            Debug.Log("둥지는 이미 지어져 있습니다. (추후 다른 상호작용 추가 가능)");
            // 예: 여기에 나중에 알 상태 보기, 먹이 주기 등의 로직 추가
            // AddWarmth(1); // 임시 테스트: 클릭 시 온기 1 증가
        }
        // 2. 둥지가 아직 없다면, 건설 시도
        else
        {
            Debug.Log($"둥지 건설 시도. 필요 나뭇가지: {branchesNeededForNest}, 보유량: {InventoryManager.Instance.branchCount}");
            // 필요한 나뭇가지 개수가 충분한지 확인
            if (InventoryManager.Instance.branchCount >= branchesNeededForNest)
            {
                // 인벤토리에서 나뭇가지 사용 시도
                if (InventoryManager.Instance.UseBranches(branchesNeededForNest))
                {
                    // 나뭇가지 사용에 성공하면 둥지 건설 상태 변경 및 둥지 활성화
                    InventoryManager.Instance.SetNestBuilt(true);

                    // Inspector에서 연결한 둥지 오브젝트 활성화
                    if (nestObject != null)
                    {
                        nestObject.SetActive(true);
                        Debug.Log("둥지 오브젝트를 활성화했습니다.");
                        // 여기에 둥지 건설 완료 효과음, 이펙트 등 추가 가능
                    }
                    else
                    {
                        Debug.LogError("EggController에 Nest Object가 연결되지 않았습니다!");
                    }
                }
                // UseBranches 함수 내부에서 실패 로그를 찍으므로 별도 처리는 불필요
            }
            else
            {
                Debug.Log($"둥지를 만들기에 나뭇가지가 부족합니다. ({InventoryManager.Instance.branchCount}/{branchesNeededForNest})");
                // 여기에 '재료 부족' 피드백 (소리, 메시지 등) 추가 가능
            }
        }
    }
    // --- 클릭 핸들러 끝 ---

    public float GetCurrentWarmth()
    {
        return currentWarmth;
    }
}