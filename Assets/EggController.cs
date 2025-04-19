using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class EggController : MonoBehaviour, IPointerClickHandler
{
    [Header("Egg State")]
    [Tooltip("알의 현재 온기")]
    [SerializeField] private float currentWarmth;
    [Tooltip("목표 온기")]
    [SerializeField] private float targetWarmth = 100f;
    [Tooltip("최대 온기")]
    [SerializeField] private float maxWarmth = 150f;
    [Tooltip("최소 온기")]
    [SerializeField] private float minWarmth = -10f;

    // *** 새로 추가: 습도 관련 변수 ***
    [Header("Humidity")]
    [Tooltip("알의 현재 습도")]
    [SerializeField] private float currentHumidity; // 초기값은 Start에서 로드
    [Tooltip("목표 습도")]
    [SerializeField] private float targetHumidity = 50f; // 예시 목표치
    [Tooltip("최대 습도")]
    [SerializeField] private float maxHumidity = 100f; // 예시 최대치
    [Tooltip("최소 습도")]
    [SerializeField] private float minHumidity = 0f;   // 예시 최소치

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

        // DataManager로부터 데이터 로드
        if (DataManager.Instance != null && DataManager.Instance.CurrentGameData != null)
        {
            GameData data = DataManager.Instance.CurrentGameData;
            currentWarmth = data.eggWarmth;
            currentHumidity = data.eggHumidity; // *** 습도 로드 ***
            Debug.Log($"EggController: 데이터 로드 완료 (Warmth: {currentWarmth}, Humidity: {currentHumidity})");
        }
        else
        {
            Debug.LogWarning("DataManager/데이터 준비 안됨. EggController 기본값 사용.");
            currentWarmth = 0f;
            currentHumidity = 50f; // 습도 기본값
            // 새 게임 상태 DataManager에 반영
            DataManager.Instance?.UpdateEggData(currentWarmth);
            DataManager.Instance?.UpdateEggHumidity(currentHumidity);
        }

        // 둥지 상태 복원
        bool nestAlreadyBuilt = InventoryManager.Instance != null && InventoryManager.Instance.isNestBuilt;
        if (nestObject != null) { nestObject.SetActive(nestAlreadyBuilt); /* ...로그...*/ }
        else { if(nestAlreadyBuilt) Debug.LogError("Nest Object 미연결!"); }
    }


    /// <summary> 알에 온기를 더합니다. </summary>
    public void AddWarmth(float amount)
    {
         if (InventoryManager.Instance != null && !InventoryManager.Instance.isNestBuilt) return; // 둥지 필요
         if (amount <= 0) return;
         float previousWarmth = currentWarmth;
         currentWarmth = Mathf.Clamp(currentWarmth + amount, minWarmth, maxWarmth); // Min/Max Clamp
         // 변경되었을 때만 DataManager 업데이트 (선택적 최적화)
         if(Mathf.Approximately(previousWarmth, currentWarmth) == false)
            DataManager.Instance?.UpdateEggData(currentWarmth);
         Debug.Log($"[{this.gameObject.name}] 온기 +{amount}! 현재 온기: {currentWarmth}");
         if (currentWarmth >= maxWarmth) Debug.LogWarning("최대 온기에 도달했습니다!");
    }

    /// <summary> 알에서 온기를 제거합니다. </summary>
    public void RemoveWarmth(float amount)
    {
         if (InventoryManager.Instance != null && !InventoryManager.Instance.isNestBuilt) return; // 둥지 필요
         if (amount <= 0) return;
         float previousWarmth = currentWarmth;
         currentWarmth = Mathf.Clamp(currentWarmth - amount, minWarmth, maxWarmth); // Min/Max Clamp
         if(Mathf.Approximately(previousWarmth, currentWarmth) == false)
            DataManager.Instance?.UpdateEggData(currentWarmth);
         Debug.Log($"[{this.gameObject.name}] 온기 -{amount}! 현재 온기: {currentWarmth}");
         if (currentWarmth <= minWarmth) Debug.LogWarning("최소 온기에 도달했습니다!");
    }

    // *** 새로 추가: 습도 추가 함수 ***
    /// <summary> 알에 습도를 더합니다. </summary>
    public void AddHumidity(float amount)
    {
         if (InventoryManager.Instance != null && !InventoryManager.Instance.isNestBuilt) return; // 둥지 필요
         if (amount <= 0) return;
         float previousHumidity = currentHumidity;
         currentHumidity = Mathf.Clamp(currentHumidity + amount, minHumidity, maxHumidity); // Min/Max Clamp
         if(Mathf.Approximately(previousHumidity, currentHumidity) == false)
             DataManager.Instance?.UpdateEggHumidity(currentHumidity); // DataManager 업데이트
         Debug.Log($"[{this.gameObject.name}] 습도 +{amount}! 현재 습도: {currentHumidity}");
         if (currentHumidity >= maxHumidity) Debug.LogWarning("최대 습도에 도달했습니다!");
    }

    // *** 새로 추가: 습도 제거 함수 ***
    /// <summary> 알에서 습도를 제거합니다. </summary>
    public void RemoveHumidity(float amount)
    {
         if (InventoryManager.Instance != null && !InventoryManager.Instance.isNestBuilt) return; // 둥지 필요
         if (amount <= 0) return;
         float previousHumidity = currentHumidity;
         currentHumidity = Mathf.Clamp(currentHumidity - amount, minHumidity, maxHumidity); // Min/Max Clamp
         if(Mathf.Approximately(previousHumidity, currentHumidity) == false)
            DataManager.Instance?.UpdateEggHumidity(currentHumidity); // DataManager 업데이트
         Debug.Log($"[{this.gameObject.name}] 습도 -{amount}! 현재 습도: {currentHumidity}");
         if (currentHumidity <= minHumidity) Debug.LogWarning("최소 습도에 도달했습니다!");
    }


    /// <summary> 알 클릭 시 (둥지 건설 트리거) </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
         if (eventData.button != PointerEventData.InputButton.Left) return;
         if (InventoryManager.Instance == null) { Debug.LogError("InventoryManager 인스턴스 없음!"); return; }

         if (InventoryManager.Instance.isNestBuilt) { Debug.Log("둥지는 이미 지어져 있습니다."); }
         else { // 둥지 건설 시도
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
    public float GetCurrentHumidity() { return currentHumidity; } // 습도 값 반환 함수 추가
}