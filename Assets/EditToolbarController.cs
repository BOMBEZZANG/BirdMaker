using UnityEngine;
using UnityEngine.UI; // 기본 Button 사용 시
using TMPro; // TextMeshPro 사용 시

/// <summary>
/// 편집 모드 툴바의 UI와 상호작용을 관리합니다.
/// 어떤 아이템을 배치할지 선택 상태를 관리합니다.
/// </summary>
public class EditToolbarController : MonoBehaviour
{
    // 배치할 아이템 타입을 나타내는 열거형
    public enum PlacementType { None, Feather, Moss }

    [Header("UI References")]
    [SerializeField] private Button selectFeatherButton;
    [SerializeField] private Button selectMossButton;
    [SerializeField] private TextMeshProUGUI currentSelectionText; // 선택 사항: 현재 선택된 아이템 표시용

    // 현재 선택된 배치 아이템 타입
    public PlacementType CurrentSelection { get; private set; } = PlacementType.None;

    void Start()
    {
        // 버튼 클릭 이벤트에 리스너 연결
        if (selectFeatherButton != null)
            selectFeatherButton.onClick.AddListener(SelectFeather);
        else Debug.LogError("SelectFeatherButton is not assigned!", this);

        if (selectMossButton != null)
            selectMossButton.onClick.AddListener(SelectMoss);
        else Debug.LogError("SelectMossButton is not assigned!", this);

        // 초기 선택 상태 없음으로 설정 및 UI 업데이트
        Deselect();
    }

    void OnDestroy()
    {
        // 메모리 누수 방지를 위해 리스너 제거
        if (selectFeatherButton != null) selectFeatherButton.onClick.RemoveListener(SelectFeather);
        if (selectMossButton != null) selectMossButton.onClick.RemoveListener(SelectMoss);
    }

    /// <summary> 깃털 배치 선택 </summary>
    public void SelectFeather()
    {
        // 이미 깃털이 선택된 상태면 선택 해제
        if (CurrentSelection == PlacementType.Feather) Deselect();
        else SetSelection(PlacementType.Feather);
    }

    /// <summary> 이끼 배치 선택 </summary>
    public void SelectMoss()
    {
         // 이미 이끼가 선택된 상태면 선택 해제
        if (CurrentSelection == PlacementType.Moss) Deselect();
        else SetSelection(PlacementType.Moss);
    }

    /// <summary> 모든 선택 해제 </summary>
    public void Deselect()
    {
        SetSelection(PlacementType.None);
    }

    /// <summary> 선택 상태 변경 및 UI 업데이트 </summary>
    private void SetSelection(PlacementType newSelection)
    {
        CurrentSelection = newSelection;
        Debug.Log($"Toolbar Selection: {CurrentSelection}");

        // 선택된 버튼 시각적 피드백 업데이트 (예: 색상 변경)
        UpdateSelectionVisuals();

        // 선택 상태 텍스트 업데이트
        if (currentSelectionText != null)
        {
            switch (CurrentSelection)
            {
                case PlacementType.Feather: currentSelectionText.text = "선택: 깃털"; break;
                case PlacementType.Moss: currentSelectionText.text = "선택: 이끼"; break;
                default: currentSelectionText.text = "선택 없음"; break;
            }
        }
    }

    /// <summary> 선택된 버튼에 따라 시각적 피드백 업데이트 (예시) </summary>
    private void UpdateSelectionVisuals()
    {
         // 예시: 선택된 버튼은 노란색, 나머지는 기본색으로
         Color normalColor = Color.white; // 버튼 기본 색상
         Color selectedColor = Color.yellow; // 선택 시 색상

         if (selectFeatherButton != null)
             selectFeatherButton.image.color = (CurrentSelection == PlacementType.Feather) ? selectedColor : normalColor;
         if (selectMossButton != null)
             selectMossButton.image.color = (CurrentSelection == PlacementType.Moss) ? selectedColor : normalColor;
    }

    // 게임 오브젝트 비활성화 시 선택 해제 (편집 모드 종료 시 등)
    void OnDisable()
    {
         Deselect();
    }
}