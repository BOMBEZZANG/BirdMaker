using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Coroutine 사용
using System; // Action 사용

/// <summary>
/// 둥지 환경(온도, 습도) 상태를 관리하고 계산하는 싱글톤 클래스.
/// DataManager와 연동하여 상태를 저장/로드합니다.
/// </summary>
public class NestEnvironmentManager : MonoBehaviour
{
    public static NestEnvironmentManager Instance { get; private set; }

    [Header("Environment Base Values")]
    [Tooltip("기본 주변 온도 (둥지만 있을 때)")]
    [SerializeField] private float baseTemperature = 15f;
    [Tooltip("기본 주변 습도 (둥지만 있을 때)")]
    [SerializeField] private float baseHumidity = 40f;

    [Header("Item Effects")]
    [Tooltip("깃털 1개당 온도 증가량")]
    [SerializeField] private float warmthPerFeather = 0.5f; // 개당 효과를 줄임 (밸런싱 필요)
    [Tooltip("이끼 1개당 습도 증가량")]
    [SerializeField] private float humidityPerMoss = 1.0f; // 개당 효과 (밸런싱 필요)

    // 현재 둥지 환경 상태
    private float currentTemperature;
    private float currentHumidity;
    
    // 프로퍼티로 변경하여 값 변경 시 이벤트 발생
    public float CurrentTemperature 
    { 
        get => currentTemperature;
        private set 
        {
            if (Math.Abs(currentTemperature - value) > 0.01f) // 값이 변경되었을 때만
            {
                currentTemperature = value;
                OnEnvironmentChanged?.Invoke(); // 이벤트 발생
            }
        }
    }
    
    public float CurrentHumidity 
    { 
        get => currentHumidity;
        private set 
        {
            if (Math.Abs(currentHumidity - value) > 0.01f) // 값이 변경되었을 때만
            {
                currentHumidity = value;
                OnEnvironmentChanged?.Invoke(); // 이벤트 발생
            }
        }
    }

    // 환경 변화 이벤트 (온도나 습도가 변경될 때 발생)
    public event Action OnEnvironmentChanged;

    public bool IsInitialized { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // DataManager 로드 완료 후 초기값 설정
        StartCoroutine(InitializeEnvironment());
    }

    private IEnumerator InitializeEnvironment()
    {
        // DataManager 초기화 기다림
        float timeout = Time.time + 5f;
        while ((DataManager.Instance == null || !DataManager.Instance.IsDataManagerInitialized) && Time.time < timeout)
        { yield return null; }

        if(DataManager.Instance == null || !DataManager.Instance.IsDataManagerInitialized)
        {
             Debug.LogError("NestEnvironmentManager: DataManager 초기화 실패! 기본값 사용.");
             // 프로퍼티 대신 내부 변수 직접 설정 (이벤트 발생 방지)
             currentTemperature = baseTemperature;
             currentHumidity = baseHumidity;
        }
        else
        {
            // 저장된 값 로드 또는 새 게임 기본값 사용
            currentTemperature = DataManager.Instance.CurrentGameData.nestTemperature;
            currentHumidity = DataManager.Instance.CurrentGameData.nestHumidity;
            Debug.Log($"NestEnvironmentManager: 초기 환경 로드 완료 (Temp:{currentTemperature:F1}, Humid:{currentHumidity:F1})");
        }

        IsInitialized = true;
        
        // 초기화 직후 환경 재계산 (데이터 정합성 확보)
        RecalculateEnvironment();
        
        // 초기화 완료 후 이벤트 발생 (UI 업데이트 유도)
        OnEnvironmentChanged?.Invoke();
        
        Debug.Log("NestEnvironmentManager 초기화 완료 및 이벤트 발생");
    }

    /// <summary>
    /// 현재 배치된 아이템(깃털, 이끼) 개수를 기반으로 둥지 환경을 다시 계산하고 DataManager에 업데이트합니다.
    /// 아이템 추가/제거 시 NestInteraction에서 호출됩니다.
    /// </summary>
    public void RecalculateEnvironment()
    {
        if (DataManager.Instance?.CurrentGameData == null) 
        {
            Debug.LogWarning("NestEnvironmentManager: RecalculateEnvironment 실패 - DataManager 데이터 없음");
            return;
        }

        // 배치된 아이템 개수 가져오기
        int featherCount = DataManager.Instance.CurrentGameData.featherPositions?.Count ?? 0;
        int mossCount = DataManager.Instance.CurrentGameData.mossPositions?.Count ?? 0;

        // 온도 계산 (예시: 기본온도 + 깃털 효과)
        float newTemperature = baseTemperature + (featherCount * warmthPerFeather);
        // TODO: 최대/최소 온도 제한 추가 (필요 시)

        // 습도 계산 (예시: 기본습도 + 이끼 효과)
        float newHumidity = baseHumidity + (mossCount * humidityPerMoss);
        // 습도는 0~100% 범위로 제한
        newHumidity = Mathf.Clamp(newHumidity, 0f, 100f);

        // 계산된 값을 DataManager에 저장 요청
        DataManager.Instance.UpdateNestEnvironment(newTemperature, newHumidity);

        // 프로퍼티를 통해 값 설정 (변경 시 이벤트 발생)
        CurrentTemperature = newTemperature;
        CurrentHumidity = newHumidity;

        Debug.Log($"환경 재계산 완료 - Feathers:{featherCount}, Moss:{mossCount} => Temp:{CurrentTemperature:F1}, Humid:{CurrentHumidity:F1}");
    }
}