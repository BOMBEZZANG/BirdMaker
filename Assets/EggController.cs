using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class EggController : MonoBehaviour, IPointerClickHandler
{
    [Header("Egg State")]
    [SerializeField] private float currentWarmth;
    [SerializeField] private float targetWarmth = 100f;
    [Tooltip("알이 버틸 수 있는 최대 온기 (예시)")]
    [SerializeField] private float maxWarmth = 150f; // 최대 온기 설정
    [Tooltip("알의 최소 온기 (예시)")]
    [SerializeField] private float minWarmth = -10f; // 최소 온기 설정

    [Header("Nest Building")]
    [SerializeField] private int branchesNeededForNest = 10;
    [SerializeField] private GameObject nestObject;

    void Start()
    {
        StartCoroutine(InitializeStateAfterOneFrame());
    }

     private IEnumerator InitializeStateAfterOneFrame()
    {
        yield return null; // 한 프레임 대기

        if (DataManager.Instance != null && DataManager.Instance.CurrentGameData != null)
        {
            currentWarmth = DataManager.Instance.CurrentGameData.eggWarmth;
            Debug.Log($"EggController: 데이터 로드 완료 (Warmth: {currentWarmth})");
        }
        else
        {
            Debug.LogWarning("DataManager/데이터 준비 안됨. 기본 온기(0) 사용.");
            currentWarmth = 0f;
            DataManager.Instance?.UpdateEggData(currentWarmth);
        }
        // 둥지 상태 복원
        bool nestAlreadyBuilt = InventoryManager.Instance != null && InventoryManager.Instance.isNestBuilt;
        if (nestObject != null) { nestObject.SetActive(nestAlreadyBuilt); /* ...로그...*/ }
        // ... (기존 Start 로직) ...
    }


    /// <summary>
    /// 알에 온기를 더합니다. 최대 온기를 넘지 않도록 제한합니다.
    /// </summary>
    public void AddWarmth(float amount)
    {
         if (InventoryManager.Instance != null && !InventoryManager.Instance.isNestBuilt) { /*...둥지 없음 경고...*/ return; }
         if (amount <= 0) return;

         float previousWarmth = currentWarmth; // 변경 전 값 기록 (선택적)
         currentWarmth += amount;
         // 최대 온기 제한 적용
         currentWarmth = Mathf.Min(currentWarmth, maxWarmth); // Mathf.Min 사용

         // DataManager 업데이트 (실제 변경된 값으로)
         DataManager.Instance?.UpdateEggData(currentWarmth);
         Debug.Log($"[{this.gameObject.name}] 온기 +{amount}! 현재 온기: {currentWarmth} (이전: {previousWarmth})");
         if (currentWarmth >= maxWarmth) Debug.LogWarning("최대 온기에 도달했습니다!");
    }

    // *** 새로 추가: 온기 제거 함수 ***
    /// <summary>
    /// 알에서 온기를 제거합니다. 최소 온기 아래로 내려가지 않도록 제한합니다.
    /// </summary>
    public void RemoveWarmth(float amount)
    {
         if (InventoryManager.Instance != null && !InventoryManager.Instance.isNestBuilt) { /*...둥지 없음 경고...*/ return; }
         if (amount <= 0) return;

         float previousWarmth = currentWarmth; // 변경 전 값 기록 (선택적)
         currentWarmth -= amount;
         // 최소 온기 제한 적용
         currentWarmth = Mathf.Max(currentWarmth, minWarmth); // Mathf.Max 사용

         // DataManager 업데이트 (실제 변경된 값으로)
         DataManager.Instance?.UpdateEggData(currentWarmth);
         Debug.Log($"[{this.gameObject.name}] 온기 -{amount}! 현재 온기: {currentWarmth} (이전: {previousWarmth})");
         if (currentWarmth <= minWarmth) Debug.LogWarning("최소 온기에 도달했습니다!");
    }

    // OnPointerClick 함수 (기존과 동일 - 알 클릭은 둥지 건설 트리거 역할만 함)
     public void OnPointerClick(PointerEventData eventData)
     {
         if (eventData.button != PointerEventData.InputButton.Left) return;
         Debug.Log($"[{this.gameObject.name}] 알 클릭됨!");
         if (InventoryManager.Instance == null) { /*...*/ return; }

         if (InventoryManager.Instance.isNestBuilt) { Debug.Log("둥지는 이미 지어져 있습니다."); }
         else { /*... 둥지 건설 시도 로직 (기존과 동일) ...*/
             if (InventoryManager.Instance.branchCount >= branchesNeededForNest) {
                 if (InventoryManager.Instance.UseBranches(branchesNeededForNest)) {
                     InventoryManager.Instance.SetNestBuilt(true);
                     if (nestObject != null) { nestObject.SetActive(true); }
                     else { Debug.LogError("Nest Object 미연결!"); }
                 }
             } else { Debug.Log($"둥지 만들기에 나뭇가지 부족..."); }
         }
     }

     public float GetCurrentWarmth() { return currentWarmth; }
}