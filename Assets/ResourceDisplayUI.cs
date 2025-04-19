using UnityEngine;
using TMPro; // TextMeshPro 사용 시 필수
// using UnityEngine.UI; // 기본 Text 사용 시

/// <summary>
/// 인벤토리의 자원 개수를 표시하는 UI를 업데이트합니다.
/// InventoryManager의 OnInventoryUpdated 이벤트를 구독하여 변경 시 자동으로 UI를 갱신합니다.
/// </summary>
public class ResourceDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("나뭇가지 개수를 표시할 TextMeshProUGUI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI branchCountText; // 기본 Text 사용 시 private Text branchCountText;
    [Tooltip("깃털 개수를 표시할 TextMeshProUGUI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI featherCountText;// 기본 Text 사용 시 private Text featherCountText;
    [Tooltip("이끼 개수를 표시할 TextMeshProUGUI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI mossCountText;   // 기본 Text 사용 시 private Text mossCountText;

    /// <summary>
    /// 이 컴포넌트가 활성화될 때 호출됩니다.
    /// InventoryManager의 이벤트에 구독합니다.
    /// </summary>
    void OnEnable()
    {
        // InventoryManager 인스턴스가 생성된 후 이벤트 구독 시도
        // 약간의 지연을 주거나 Start에서 처리하는 것이 더 안정적일 수 있음
        StartCoroutine(SubscribeToInventoryUpdates());
    }

    /// <summary>
    /// 이 컴포넌트가 비활성화될 때 호출됩니다.
    /// 메모리 누수 방지를 위해 이벤트를 구독 해제합니다.
    /// </summary>
    void OnDisable()
    {
        // InventoryManager 인스턴스가 아직 존재하면 이벤트 구독 해제
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryUpdated -= UpdateUI;
            // Debug.Log("InventoryManager 이벤트 구독 해제됨."); // 필요 시 로그
        }
    }

    /// <summary>
    /// InventoryManager가 준비될 때까지 기다렸다가 이벤트를 구독하는 코루틴
    /// </summary>
    private System.Collections.IEnumerator SubscribeToInventoryUpdates()
    {
        // InventoryManager 인스턴스가 생성될 때까지 한 프레임씩 대기 (안전장치)
        while (InventoryManager.Instance == null)
        {
            yield return null;
        }

        // 인스턴스가 준비되면 이벤트 구독 및 즉시 UI 업데이트
        // Debug.Log("InventoryManager 인스턴스 찾음. 이벤트 구독 및 UI 업데이트 시작."); // 필요 시 로그
        InventoryManager.Instance.OnInventoryUpdated += UpdateUI;
        UpdateUI(); // 초기 UI 업데이트
    }


    /// <summary>
    /// InventoryManager의 데이터가 변경될 때 호출되어 UI 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateUI()
    {
        // InventoryManager 인스턴스가 유효한지 다시 한번 확인 (안전장치)
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("UpdateUI 호출 시 InventoryManager 인스턴스가 없습니다.");
            // 기본값 또는 "N/A" 표시
            if(branchCountText != null) branchCountText.text = "N/A";
            if(featherCountText != null) featherCountText.text = "N/A";
            if(mossCountText != null) mossCountText.text = "N/A";
            return;
        }

        // 각 자원 개수를 읽어와 UI 텍스트 업데이트
        if (branchCountText != null)
        {
            branchCountText.text = InventoryManager.Instance.branchCount.ToString();
        }
        if (featherCountText != null)
        {
            featherCountText.text = InventoryManager.Instance.featherCount.ToString();
        }
        if (mossCountText != null)
        {
            mossCountText.text = InventoryManager.Instance.mossCount.ToString();
        }
         // Debug.Log("자원 UI 업데이트 완료."); // 필요 시 로그
    }
}