using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// NestViewScene에서 현재 '둥지 환경'의 온도와 습도를 표시하는 UI를 관리합니다.
/// 도구 보유 여부에 따라 값 표시를 제어합니다.
/// NestEnvironmentManager의 이벤트를 구독하여 환경 변화에 반응합니다.
/// </summary>
public class EnvironmentDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI temperatureText;
    [SerializeField] private TextMeshProUGUI humidityText;

    // 참조 변수
    private InventoryManager inventoryManager;
    private NestEnvironmentManager environmentManager;
    private bool isInitialized = false;

    void Start()
    {
        // 참조 찾기 시도
        inventoryManager = InventoryManager.Instance;
        environmentManager = NestEnvironmentManager.Instance;

        // 필수 참조 확인
        if (environmentManager == null || inventoryManager == null || temperatureText == null || humidityText == null)
        {
            Debug.LogError("EnvironmentDisplayUI: 필수 참조(NestEnvManager, InventoryManager, Text UI) 중 하나 이상 없음!", this);
            SetTextSafe(temperatureText, "Error!"); 
            SetTextSafe(humidityText, "Error!");
            enabled = false; 
            return;
        }
        
        StartCoroutine(InitializeAndStartUpdates());
    }

    /// <summary> 매니저 준비 대기 및 초기화 </summary>
    private IEnumerator InitializeAndStartUpdates()
    {
        // 모든 매니저의 초기화를 기다림
        float timeout = Time.time + 5f;
        while ((DataManager.Instance == null || !DataManager.Instance.IsDataManagerInitialized ||
                InventoryManager.Instance == null || !InventoryManager.Instance.IsInventoryInitialized ||
                NestEnvironmentManager.Instance == null || !NestEnvironmentManager.Instance.IsInitialized)
                && Time.time < timeout)
        { 
            yield return null; 
        }

        // 최종 확인
        if (InventoryManager.Instance == null || !InventoryManager.Instance.IsInventoryInitialized ||
            NestEnvironmentManager.Instance == null || !NestEnvironmentManager.Instance.IsInitialized)
        { 
            Debug.LogError("EnvironmentDisplayUI: Managers did not initialize! Disabling.", this); 
            enabled = false; 
            yield break; 
        }

        // 안전하게 참조 재할당
        inventoryManager = InventoryManager.Instance;
        environmentManager = NestEnvironmentManager.Instance;
        
        // *** 추가: 환경 변화 이벤트 구독 ***
        environmentManager.OnEnvironmentChanged += UpdateDisplay;
        
        // 인벤토리 변화 이벤트도 구독 (도구 구매 등으로 UI 변경 가능성)
        inventoryManager.OnInventoryUpdated += UpdateDisplay;

        Debug.Log("EnvironmentDisplayUI: 초기화 완료 및 이벤트 구독.");
        isInitialized = true;
        
        // 초기 UI 업데이트 명시적 호출
        UpdateDisplay();
    }

    /// <summary>
    /// 컴포넌트 비활성화 시 이벤트 구독 해제
    /// </summary>
    void OnDisable()
    {
        // 이벤트 구독 해제
        if (environmentManager != null)
            environmentManager.OnEnvironmentChanged -= UpdateDisplay;
            
        if (inventoryManager != null)
            inventoryManager.OnInventoryUpdated -= UpdateDisplay;
    }

    /// <summary>
    /// 이제 이벤트 기반 업데이트를 사용하므로 Update 메서드 제거
    /// </summary>
    // void Update() 메서드 제거됨

    /// <summary> UI 표시 업데이트 (이벤트 핸들러로 호출됨) </summary>
    private void UpdateDisplay()
    {
        if (!isInitialized || inventoryManager == null || environmentManager == null) 
        {
            Debug.LogWarning("EnvironmentDisplayUI: UpdateDisplay 호출되었으나 초기화 안됨");
            return;
        }

        bool hasThermo = inventoryManager.HasThermometer;
        bool hasHygro = inventoryManager.HasHygrometer;

        float currentTemp = environmentManager.CurrentTemperature;
        float currentHumid = environmentManager.CurrentHumidity;

        // 디버깅 로그
        Debug.Log($"[EnvironmentUI] UpdateDisplay 실행: Thermo={hasThermo}, Hygro={hasHygro}, Temp={currentTemp:F1}, Humid={currentHumid:F1}");

        // 온도 표시
        if (temperatureText != null) {
            temperatureText.text = hasThermo ? $"온도: {currentTemp:F1} °C" : "온도: ??? °C";
        }
        
        // 습도 표시
        if (humidityText != null) {
            humidityText.text = hasHygro ? $"습도: {currentHumid:F1} %" : "습도: ??? %";
        }
    }

    /// <summary> TextMeshProUGUI 텍스트 안전하게 설정 (null 체크 포함) </summary>
    private void SetTextSafe(TextMeshProUGUI textElement, string value)
    {
        if(textElement != null) textElement.text = value;
    }
}