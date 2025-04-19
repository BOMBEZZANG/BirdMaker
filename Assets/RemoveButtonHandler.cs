using UnityEngine;
using UnityEngine.UI; // 기본 UI 버튼 사용 시

/// <summary>
/// 깃털 옆에 나타나는 '제거' 버튼에 부착됩니다.
/// 클릭 시 지정된 깃털의 제거를 NestInteraction에게 요청하고, 상호작용 플래그를 해제합니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class RemoveButtonHandler : MonoBehaviour
{
    private GameObject featherToRemove; // 제거할 대상 깃털 오브젝트
    private NestInteraction nestInteractionManager; // NestInteraction 참조

    /// <summary>
    /// 이 버튼을 생성할 때 외부(NestFeatherVisual)에서 호출하여 초기 설정을 합니다.
    /// </summary>
    public void Initialize(GameObject targetFeather, NestInteraction manager)
    {
        featherToRemove = targetFeather;
        nestInteractionManager = manager;

        Button button = GetComponent<Button>();
        // 리스너 중복 추가 방지
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnRemoveButtonClick);
    }

    /// <summary>
    /// 제거 버튼이 클릭되었을 때 호출될 함수
    /// </summary>
    private void OnRemoveButtonClick()
    {
        // Debug.Log($"제거 버튼 클릭됨: [{featherToRemove?.name}] 제거 요청."); // 로그 레벨 조절
        // 유효한 참조가 있을 때만 제거 요청
        if (nestInteractionManager != null && featherToRemove != null)
        {
            nestInteractionManager.RequestRemoveFeather(featherToRemove);
        }
        else
        {
             Debug.LogError("제거 요청 실패: Manager 또는 Feather 참조가 없습니다.");
        }

        // *** 중요: 제거 버튼 클릭 후 static 플래그 해제 ***
        NestFeatherVisual.ClearInteractionFlag();

        // 버튼 자신 파괴 (깃털은 RequestRemoveFeather에서 파괴됨)
        Destroy(gameObject);
    }

    // OnDestroy에서 리스너 제거는 보통 자동으로 되지만 명시적으로 할 수도 있습니다.
    // void OnDestroy() { Button btn = GetComponent<Button>(); if(btn != null) btn.onClick.RemoveAllListeners(); }
}