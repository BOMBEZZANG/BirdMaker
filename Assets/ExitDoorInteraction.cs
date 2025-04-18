using UnityEngine;
using UnityEngine.EventSystems; // Unity의 이벤트 시스템 사용을 위해 필수
using UnityEngine.SceneManagement; // Scene 관리를 위해 필수

/// <summary>
/// 출구 문 오브젝트에 부착되어, 클릭 시 지정된 씬으로 전환합니다.
/// 이 스크립트가 작동하려면 대상 오브젝트에 Collider2D가 있어야 하고,
/// 씬의 Main Camera에는 Physics 2D Raycaster가, 씬에는 EventSystem이 있어야 합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))] // Collider2D 컴포넌트가 필수임을 명시
public class ExitDoorInteraction : MonoBehaviour, IPointerClickHandler // IPointerClickHandler 인터페이스 구현
{
    [Header("Scene Transition Settings")]
    [Tooltip("클릭 시 전환될 대상 씬(탐험 씬)의 이름입니다. 빌드 세팅에 포함되어야 합니다.")]
    [SerializeField] private string targetSceneName = "ExplorationViewScene"; // 전환할 탐험 씬 이름 입력

    /// <summary>
    /// IPointerClickHandler 인터페이스의 메서드입니다.
    /// 이 스크립트가 부착된 오브젝트의 Collider 영역이 클릭되면 자동으로 호출됩니다.
    /// </summary>
    /// <param name="eventData">발생한 클릭 이벤트에 대한 정보</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 어떤 마우스 버튼으로 클릭했는지 확인할 수도 있습니다 (보통 왼쪽 클릭)
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log($"[{this.gameObject.name}] 오브젝트가 마우스 왼쪽 버튼으로 클릭되었습니다. '{targetSceneName}' 씬 로드를 시도합니다.");
            LoadTargetScene(); // 씬 로드 함수 호출
        }
        // else if (eventData.button == PointerEventData.InputButton.Right) { /* 오른쪽 클릭 처리 */ }
    }

    /// <summary>
    /// 지정된 이름의 씬을 로드하는 내부 함수입니다.
    /// </summary>
    private void LoadTargetScene()
    {
        // 대상 씬 이름이 설정되었는지 확인
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"[{this.gameObject.name}] 전환할 씬 이름(Target Scene Name)이 비어있습니다! Inspector 설정을 확인하세요.");
            return;
        }

        // SceneManager를 사용하여 씬 로드
        // 중요: targetSceneName으로 지정된 씬이 반드시 빌드 세팅에 추가되어 있어야 합니다!
        SceneManager.LoadScene(targetSceneName);
    }

    // --- (선택 사항) 마우스 오버 시 시각적 피드백을 위한 코드 ---
    
    // 마우스 포인터가 이 오브젝트의 콜라이더 영역 안으로 들어왔을 때 호출됩니다.
    void OnMouseEnter()
    {
        // 예: 스프라이트 색상을 약간 변경하여 상호작용 가능함을 표시
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.grey; // 약간 어둡게 (예시)
        }
        Debug.Log($"마우스 진입: {this.gameObject.name}");
    }

    // 마우스 포인터가 이 오브젝트의 콜라이더 영역에서 벗어났을 때 호출됩니다.
    void OnMouseExit()
    {
        // 예: 스프라이트 색상을 원래대로 복구
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.white; // 원래 색상 (예시)
        }
         Debug.Log($"마우스 이탈: {this.gameObject.name}");
    }
    
}