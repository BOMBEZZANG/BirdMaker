using UnityEngine;
using UnityEngine.EventSystems; // 클릭 인터페이스 사용
using System.Collections;       // 코루틴 사용

/// <summary>
/// 알의 상태(온기, 습도, 성장)를 관리하고 부화 로직을 처리합니다.
/// 둥지 건설 트리거 역할도 수행합니다. (알 클릭 시)
/// DataManager와 연동하여 상태를 저장/로드합니다.
/// </summary>
public class EggController : MonoBehaviour, IPointerClickHandler // 클릭 감지 인터페이스
{
    [Header("Egg State")]
    [Tooltip("알의 현재 온기")]
    [SerializeField] private float currentWarmth;
    [Tooltip("목표 온기 (밸런싱용, 직접 사용X)")]
    [SerializeField] private float targetWarmth = 100f; // 예시
    [Tooltip("온기 최대값")]
    [SerializeField] private float maxWarmth = 150f;
    [Tooltip("온기 최소값")]
    [SerializeField] private float minWarmth = -10f;

    [Header("Humidity")]
    [Tooltip("알의 현재 습도")]
    [SerializeField] private float currentHumidity;
    [Tooltip("목표 습도 (밸런싱용, 직접 사용X)")]
    [SerializeField] private float targetHumidity = 50f; // 예시
    [Tooltip("습도 최대값")]
    [SerializeField] private float maxHumidity = 100f;
    [Tooltip("습도 최소값")]
    [SerializeField] private float minHumidity = 0f;

    // *** 성장 및 부화 관련 변수 추가 ***
    [Header("Growth & Hatching")]
    [Tooltip("현재 누적된 성장 포인트 (저장/로드됨)")]
    [SerializeField] private float currentGrowthPoints = 0f;
    [Tooltip("부화에 필요한 총 성장 포인트")]
    [SerializeField] private float requiredGrowthPoints = 300f; // 예: 300
    [Tooltip("최적 조건에서 초당 얻는 성장 포인트")]
    [SerializeField] private float growthPointsPerSecond = 1f; // 예: 1
    [Tooltip("성장을 위한 온기 최적 범위 최소값")]
    [SerializeField] private float optimalWarmthMin = 80f;
    [Tooltip("성장을 위한 온기 최적 범위 최대값")]
    [SerializeField] private float optimalWarmthMax = 110f;
    [Tooltip("성장을 위한 습도 최적 범위 최소값")]
    [SerializeField] private float optimalHumidityMin = 40f;
    [Tooltip("성장을 위한 습도 최적 범위 최대값")]
    [SerializeField] private float optimalHumidityMax = 60f;
    [Tooltip("알이 부화했는지 여부 (저장/로드됨)")]
    [SerializeField] private bool hasHatched = false;
    [Tooltip("부화 후 나타날 병아리 오브젝트 (Inspector 연결 필요)")]
    [SerializeField] private GameObject chickVisual;
    [Tooltip("부화 전 알 오브젝트 (자기 자신 또는 자식, Inspector 연결 필요)")]
    [SerializeField] private GameObject eggVisual; // 알 비주얼


    [Header("Nest Building")]
    [Tooltip("둥지 건설에 필요한 나뭇가지 개수")]
    [SerializeField] private int branchesNeededForNest = 10;
    [Tooltip("둥지 게임 오브젝트 (Inspector 연결 필요)")]
    [SerializeField] private GameObject nestObject;

    // 내부 상태
    private bool isInitialized = false; // 데이터 로딩 및 초기화 완료 여부

    // === Unity 생명주기 함수 ===

    void Start()
    {
        // 게임 시작 시 비주얼 초기 상태 설정 (로드 전 기본값 기준)
        if(eggVisual != null) eggVisual.SetActive(!hasHatched);
        if(chickVisual != null) chickVisual.SetActive(hasHatched);

        // 코루틴을 통해 한 프레임 뒤 데이터 로드 및 상태 복원 시도
        StartCoroutine(InitializeStateAfterOneFrame());
    }

    /// <summary>
    /// 매 프레임 호출되어 성장 조건을 확인하고 진행합니다.
    /// </summary>
    void Update()
    {
        // 초기화 전, 둥지 없거나, 이미 부화했으면 성장 로직 실행 안 함
        if (!isInitialized || InventoryManager.Instance == null || !InventoryManager.Instance.isNestBuilt || hasHatched)
        {
            return;
        }

        // 최적 조건 확인
        bool isWarmthOptimal = currentWarmth >= optimalWarmthMin && currentWarmth <= optimalWarmthMax;
        bool isHumidityOptimal = currentHumidity >= optimalHumidityMin && currentHumidity <= optimalHumidityMax;

        // 두 조건 모두 만족 시 성장 포인트 증가
        if (isWarmthOptimal && isHumidityOptimal)
        {
            float previousGrowth = currentGrowthPoints; // 변경 확인용
            currentGrowthPoints += growthPointsPerSecond * Time.deltaTime; // 시간에 비례하여 증가

            // 최대값(부화 필요값)을 넘지 않도록 제한
            currentGrowthPoints = Mathf.Min(currentGrowthPoints, requiredGrowthPoints);

            // 포인트가 실제로 변경되었을 때만 DataManager 업데이트
            if (Mathf.Approximately(previousGrowth, currentGrowthPoints) == false)
            {
                 DataManager.Instance?.UpdateEggGrowth(currentGrowthPoints);
                 // Debug.Log($"성장 포인트 증가: {currentGrowthPoints:F1} / {requiredGrowthPoints}"); // 소수점 한자리까지 표시 (로그 필요 시)
            }

            // 부화 조건 체크 (정확히 같거나 큰지 비교)
            if (currentGrowthPoints >= requiredGrowthPoints)
            {
                Hatch(); // 부화 함수 호출
            }
        }
        // else // 최적 조건 아닐 때
        // {
        //     // TODO: 필요 시 성장 멈춤 외 다른 패널티 구현 (예: 포인트 감소)
        //     // Debug.Log($"최적 조건 아님 - W: {currentWarmth}({isWarmthOptimal}), H: {currentHumidity}({isHumidityOptimal})");
        // }
    }


    // === 초기화 및 데이터 로드 ===

    /// <summary> 한 프레임 뒤 데이터 로드 및 초기 상태 설정 </summary>
    private IEnumerator InitializeStateAfterOneFrame()
    {
        yield return null; // 한 프레임 대기

        // DataManager로부터 데이터 로드
        if (DataManager.Instance?.CurrentGameData != null)
        {
            GameData data = DataManager.Instance.CurrentGameData;
            currentWarmth = data.eggWarmth;
            currentHumidity = data.eggHumidity;
            currentGrowthPoints = data.eggGrowthPoints; // 성장 포인트 로드
            hasHatched = data.eggHasHatched;     // 부화 상태 로드
            // Debug.Log($"EggController: 데이터 로드 완료 (W: {currentWarmth:F1}, H: {currentHumidity:F1}, G: {currentGrowthPoints:F1}, Hatched: {hasHatched})");
        }
        else
        {
            Debug.LogWarning("DataManager/데이터 준비 안됨. EggController 기본값 사용.");
            // 기본값 설정
            currentWarmth = 0f;
            currentHumidity = 50f;
            currentGrowthPoints = 0f;
            hasHatched = false;
            // 새 게임 상태 DataManager에 반영
            DataManager.Instance?.UpdateEggData(currentWarmth);
            DataManager.Instance?.UpdateEggHumidity(currentHumidity);
            DataManager.Instance?.UpdateEggGrowth(currentGrowthPoints);
            DataManager.Instance?.UpdateEggHatchedStatus(hasHatched);
        }

        // 로드된 부화 상태에 따라 비주얼 다시 설정
        if(eggVisual != null) eggVisual.SetActive(!hasHatched);
        if(chickVisual != null) chickVisual.SetActive(hasHatched);

        // 둥지 상태 복원
        bool nestAlreadyBuilt = InventoryManager.Instance != null && InventoryManager.Instance.isNestBuilt;
        if (nestObject != null) { nestObject.SetActive(nestAlreadyBuilt); }
        else { if(nestAlreadyBuilt) Debug.LogError("Nest Object 미연결!"); }

        isInitialized = true; // 초기화 완료 플래그 설정
    }

    // === 상태 변경 함수들 ===

    public void AddWarmth(float amount) {
         if (InventoryManager.Instance == null || !InventoryManager.Instance.isNestBuilt) return;
         if (amount <= 0) return;
         float previousWarmth = currentWarmth;
         currentWarmth = Mathf.Clamp(currentWarmth + amount, minWarmth, maxWarmth);
         if(Mathf.Approximately(previousWarmth, currentWarmth) == false) DataManager.Instance?.UpdateEggData(currentWarmth);
         // Debug.Log($"[{this.gameObject.name}] 온기 +{amount}! 현재 온기: {currentWarmth}");
         if (currentWarmth >= maxWarmth) Debug.LogWarning("최대 온기에 도달했습니다!");
    }
    public void RemoveWarmth(float amount) {
         if (InventoryManager.Instance == null || !InventoryManager.Instance.isNestBuilt) return;
         if (amount <= 0) return;
         float previousWarmth = currentWarmth;
         currentWarmth = Mathf.Clamp(currentWarmth - amount, minWarmth, maxWarmth);
         if(Mathf.Approximately(previousWarmth, currentWarmth) == false) DataManager.Instance?.UpdateEggData(currentWarmth);
         // Debug.Log($"[{this.gameObject.name}] 온기 -{amount}! 현재 온기: {currentWarmth}");
         if (currentWarmth <= minWarmth) Debug.LogWarning("최소 온기에 도달했습니다!");
     }
    public void AddHumidity(float amount) {
         if (InventoryManager.Instance == null || !InventoryManager.Instance.isNestBuilt) return;
         if (amount <= 0) return;
         float previousHumidity = currentHumidity;
         currentHumidity = Mathf.Clamp(currentHumidity + amount, minHumidity, maxHumidity);
         if(Mathf.Approximately(previousHumidity, currentHumidity) == false) DataManager.Instance?.UpdateEggHumidity(currentHumidity);
         // Debug.Log($"[{this.gameObject.name}] 습도 +{amount}! 현재 습도: {currentHumidity}");
         if (currentHumidity >= maxHumidity) Debug.LogWarning("최대 습도에 도달했습니다!");
     }
    public void RemoveHumidity(float amount) {
         if (InventoryManager.Instance == null || !InventoryManager.Instance.isNestBuilt) return;
         if (amount <= 0) return;
         float previousHumidity = currentHumidity;
         currentHumidity = Mathf.Clamp(currentHumidity - amount, minHumidity, maxHumidity);
         if(Mathf.Approximately(previousHumidity, currentHumidity) == false) DataManager.Instance?.UpdateEggHumidity(currentHumidity);
         // Debug.Log($"[{this.gameObject.name}] 습도 -{amount}! 현재 습도: {currentHumidity}");
         if (currentHumidity <= minHumidity) Debug.LogWarning("최소 습도에 도달했습니다!");
     }


    // --- 부화 처리 ---

    /// <summary> 부화 로직을 실행합니다. </summary>
    private void Hatch()
    {
        if (hasHatched) return; // 중복 실행 방지

        hasHatched = true; // 부화 상태로 변경
        DataManager.Instance?.UpdateEggHatchedStatus(hasHatched); // 데이터 저장 요청
        Debug.Log("***** 알 부화!!! *****");

        // 시각적 변화
        if (eggVisual != null) eggVisual.SetActive(false); // 알 숨기기
        else Debug.LogWarning("Hatch: Egg Visual is not assigned.");

        if (chickVisual != null) chickVisual.SetActive(true); // 병아리 보이기
        else Debug.LogWarning("Hatch: Chick Visual is not assigned.");

        // TODO: 부화 효과음 재생
        // TODO: 부화 파티클 효과 재생
        // TODO: 병아리 등장 애니메이션 (필요 시)
        // TODO: 관련 UI 업데이트 (예: 알 상태 UI -> 병아리 상태 UI)
        // TODO: 게임 플레이 상태 변경 (알 관리 -> 병아리 관리)
    }


    // --- 기타 함수들 ---

    /// <summary> 알 클릭 시 (둥지 건설 트리거 - 부화 후에는 다른 기능 가능) </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
         if (eventData.button != PointerEventData.InputButton.Left) return;
         if (InventoryManager.Instance == null) { return; }

         // 부화 후 알 클릭 시 다른 동작? (예: 병아리 상태 보기)
         if (hasHatched)
         {
             Debug.Log("병아리를 클릭했습니다. (추후 상호작용 추가)");
             return;
         }

         // 부화 전: 둥지 건설 로직
         if (InventoryManager.Instance.isNestBuilt) { Debug.Log("둥지는 이미 지어져 있습니다."); }
         else { /*... 둥지 건설 시도 ...*/
             if (InventoryManager.Instance.branchCount >= branchesNeededForNest) {
                 if (InventoryManager.Instance.UseBranches(branchesNeededForNest)) {
                     InventoryManager.Instance.SetNestBuilt(true);
                     if (nestObject != null) { nestObject.SetActive(true); } else { /*...*/ }
                 }
             } else { /*...*/ }
         }
     }

    // 현재 값 반환 함수들
    public float GetCurrentWarmth() { return currentWarmth; }
    public float GetCurrentHumidity() { return currentHumidity; }
    public float GetCurrentGrowthPercent() { return (requiredGrowthPoints > 0) ? (currentGrowthPoints / requiredGrowthPoints) * 100f : 0f; } // 성장률(%) 반환
    public bool IsHatched() { return hasHatched; } // 부화 상태 반환
}