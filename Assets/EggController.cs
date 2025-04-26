using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 알의 성장 및 부화 로직을 관리합니다.
/// 둥지 환경(온도, 습도) 조건은 NestEnvironmentManager에서 읽어옵니다.
/// </summary>
public class EggController : MonoBehaviour, IPointerClickHandler
{
    // [Header("Egg State")] // 온기/습도 변수 제거됨
    // [SerializeField] private float currentWarmth; ... 등 제거

    [Header("Growth & Hatching")]
    [Tooltip("현재 누적된 성장 포인트 (저장/로드됨)")]
    [SerializeField] private float currentGrowthPoints = 0f;
    [Tooltip("부화에 필요한 총 성장 포인트")]
    [SerializeField] private float requiredGrowthPoints = 300f;
    [Tooltip("최적 조건에서 초당 얻는 성장 포인트")]
    [SerializeField] private float growthPointsPerSecond = 1f;
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
    [Tooltip("부화 전 알 오브젝트 (Inspector 연결 필요)")]
    [SerializeField] private GameObject eggVisual;

    [Header("Nest Building Trigger")] // 둥지 건설은 여전히 알 클릭으로 시작
    [SerializeField] private int branchesNeededForNest = 10;
    [SerializeField] private GameObject nestObject; // 둥지 건설 시 활성화될 오브젝트 참조

    private bool isInitialized = false; // 초기화 완료 플래그

    void Start()
    {
        // 비주얼 초기 상태 설정 (로드 전)
        if(eggVisual != null) eggVisual.SetActive(!hasHatched);
        if(chickVisual != null) chickVisual.SetActive(hasHatched);

        StartCoroutine(InitializeStateAfterOneFrame());
    }

    /// <summary> 매 프레임 성장 조건 확인 및 진행 </summary>
    void Update()
    {
        // 초기화 전, 둥지 없거나, 이미 부화했거나, 환경 매니저 준비 안 됐으면 실행 안 함
        if (!isInitialized || InventoryManager.Instance == null || !InventoryManager.Instance.isNestBuilt || hasHatched || NestEnvironmentManager.Instance == null || !NestEnvironmentManager.Instance.IsInitialized)
        {
            return;
        }

        // *** 수정: NestEnvironmentManager에서 현재 온도/습도 읽어오기 ***
        float currentTemperature = NestEnvironmentManager.Instance.CurrentTemperature;
        float currentNestHumidity = NestEnvironmentManager.Instance.CurrentHumidity; // 변수 이름 변경

        // 최적 조건 확인
        bool isWarmthOptimal = currentTemperature >= optimalWarmthMin && currentTemperature <= optimalWarmthMax;
        bool isHumidityOptimal = currentNestHumidity >= optimalHumidityMin && currentNestHumidity <= optimalHumidityMax;

        // 조건 만족 시 성장
        if (isWarmthOptimal && isHumidityOptimal)
        {
            float previousGrowth = currentGrowthPoints;
            currentGrowthPoints += growthPointsPerSecond * Time.deltaTime;
            currentGrowthPoints = Mathf.Min(currentGrowthPoints, requiredGrowthPoints);

            // *** 수정: 성장 포인트 저장 요청 ***
            if (Mathf.Approximately(previousGrowth, currentGrowthPoints) == false)
            {
                 // DataManager에 직접 저장 요청 (이제 DataManager에 UpdateEggGrowth 없음)
                 // -> 성장은 EggController 내부 상태로만 두고, 저장 시점에만 반영? 또는 DataManager에 필드 유지?
                 // --> 혼란 방지 위해 GameData에 eggGrowthPoints 유지하고 DataManager 통해 저장
                 DataManager.Instance?.UpdateEggGrowth(currentGrowthPoints); // DataManager에 UpdateEggGrowth 다시 추가 필요!
                 // Debug.Log($"성장 포인트 증가: {currentGrowthPoints:F1} / {requiredGrowthPoints}");
            }

            // 부화 조건 체크
            if (currentGrowthPoints >= requiredGrowthPoints)
            {
                Hatch();
            }
        }
    }

    // === 초기화 및 데이터 로드 ===
    private IEnumerator InitializeStateAfterOneFrame()
    {
        yield return null; // 한 프레임 대기

        // *** 수정: NestEnvironmentManager 초기화도 기다림 ***
        float timeout = Time.time + 5f;
        while ((DataManager.Instance == null || !DataManager.Instance.IsDataManagerInitialized || NestEnvironmentManager.Instance == null || !NestEnvironmentManager.Instance.IsInitialized) && Time.time < timeout)
        {
             yield return null;
        }

        if (DataManager.Instance?.CurrentGameData != null)
        {
            GameData data = DataManager.Instance.CurrentGameData;
            // 온기/습도 로드 제거됨
            // currentWarmth = data.eggWarmth;
            // currentHumidity = data.eggHumidity;
            // GameData에 eggGrowthPoints 다시 추가 필요! -> 추가했다고 가정하고 로드
             currentGrowthPoints = data.eggGrowthPoints;
            hasHatched = data.eggHasHatched;
            Debug.Log($"EggController: 데이터 로드 완료 (Growth: {currentGrowthPoints:F1}, Hatched: {hasHatched})");
        }
        else
        {
            Debug.LogWarning("DataManager/데이터 준비 안됨. EggController 기본값 사용.");
            currentGrowthPoints = 0f; hasHatched = false;
            // 기본값 DataManager에 반영
            DataManager.Instance?.UpdateEggGrowth(currentGrowthPoints);
            DataManager.Instance?.UpdateEggHatchedStatus(hasHatched);
        }

        // 비주얼 설정
        if(eggVisual != null) eggVisual.SetActive(!hasHatched);
        if(chickVisual != null) chickVisual.SetActive(hasHatched);
        // 둥지 상태 복원
        bool nestAlreadyBuilt = InventoryManager.Instance != null && InventoryManager.Instance.isNestBuilt;
        if (nestObject != null) { nestObject.SetActive(nestAlreadyBuilt); }
        else { if(nestAlreadyBuilt) Debug.LogError("Nest Object 미연결!"); }

        isInitialized = true;
    }


    // --- 온기/습도 변경 함수 제거 ---
    // public void AddWarmth(float amount) { ... }
    // public void RemoveWarmth(float amount) { ... }
    // public void AddHumidity(float amount) { ... }
    // public void RemoveHumidity(float amount) { ... }


    // --- 부화 처리 ---
    private void Hatch()
    {
        if (hasHatched) return;
        hasHatched = true;
        DataManager.Instance?.UpdateEggHatchedStatus(hasHatched); // 부화 상태 저장
        // 성장 포인트는 더 이상 필요 없으므로 초기화 또는 유지 (선택)
        // currentGrowthPoints = 0; DataManager.Instance?.UpdateEggGrowth(currentGrowthPoints);
        Debug.Log("***** 알 부화!!! *****");
        if (eggVisual != null) eggVisual.SetActive(false);
        if (chickVisual != null) chickVisual.SetActive(true);
        // TODO: 부화 효과음 등
    }


    // --- 알 클릭 (둥지 건설 트리거) ---
    public void OnPointerClick(PointerEventData eventData) { /* ... 이전 코드와 동일 ... */
         if (eventData.button != PointerEventData.InputButton.Left) return;
         if (InventoryManager.Instance == null) { return; }
         if (hasHatched) { Debug.Log("병아리 클릭 (상호작용 없음)"); return; } // 부화 후 동작 없음
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

    // --- Getter 함수들 ---
    // GetCurrentWarmth, GetCurrentHumidity 제거됨
    public float GetCurrentGrowthPercent() { return (requiredGrowthPoints > 0) ? (currentGrowthPoints / requiredGrowthPoints) * 100f : (hasHatched ? 100f : 0f) ; }
    public bool IsHatched() { return hasHatched; }
}