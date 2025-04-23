using UnityEngine;

/// <summary>
/// 게임 내 주요 UI 패널(상점, 인벤토리 등)의 활성화/비활성화를 관리하는 싱글톤 클래스.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Tooltip("상점 UI Panel 게임 오브젝트")]
    // 필요 시 다른 UI 패널(인벤토리, 메뉴 등) 참조 추가

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        // 시작 시 모든 관리 대상 UI 패널 비활성화 (선택적이지만 권장)
    }

    /// <summary> 상점 UI를 엽니다 (활성화). </summary>

     // 필요 시 다른 UI 열고 닫는 함수 추가 (예: OpenInventory, CloseInventory)
}