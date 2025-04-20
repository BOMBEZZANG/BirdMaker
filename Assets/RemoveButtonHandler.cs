using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RemoveButtonHandler : MonoBehaviour
{
    private GameObject featherToRemove; // 또는 itemToRemove 로 일반화 가능
    private NestInteraction nestInteractionManager;

    public void Initialize(GameObject targetItem, NestInteraction manager)
    {
        featherToRemove = targetItem; // 이름은 feather지만 이끼에도 사용됨
        nestInteractionManager = manager;
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnRemoveButtonClick);
    }

    private void OnRemoveButtonClick()
    {
        // Debug.Log($"제거 버튼 클릭됨: [{featherToRemove?.name}] 제거 요청.");
        if (nestInteractionManager != null && featherToRemove != null)
        {
             // 제거 대상이 깃털인지 이끼인지 확인하고 맞는 함수 호출
             if (featherToRemove.GetComponent<NestFeatherVisual>() != null)
             {
                 nestInteractionManager.RequestRemoveFeather(featherToRemove);
             }
             else if (featherToRemove.GetComponent<NestMossVisual>() != null)
             {
                  nestInteractionManager.RequestRemoveMoss(featherToRemove);
             }
             else { Debug.LogError("알 수 없는 아이템 타입 제거 요청!", featherToRemove);}

             // *** 중요: Static 플래그 해제 제거됨 ***
             // NestFeatherVisual.ClearInteractionFlag(); // 제거
        }
        else { /* 참조 없음 로그 */ }

        // 버튼 자신 파괴
        Destroy(gameObject);
    }
}