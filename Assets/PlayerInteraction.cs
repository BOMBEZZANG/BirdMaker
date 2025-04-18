using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Scene 관리를 위해 필수

/// <summary>
/// 플레이어의 상호작용(집 들어가기 등) 및 관련 씬 전환을 처리합니다.
/// 이 스크립트는 플레이어 게임 오브젝트에 부착되어야 합니다.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("상호작용 가능한 오브젝트를 식별하는 태그입니다.")]
    [SerializeField] private string interactableTag = "HouseTrigger"; // 집 오브젝트의 트리거 콜라이더에 설정한 태그

    [Tooltip("상호작용을 실행할 키 코드입니다.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E; // E 키로 상호작용

    [Header("Scene Transition")]
    [Tooltip("전환될 대상 씬(둥지 씬)의 이름입니다. 빌드 세팅에 포함되어야 합니다.")]
    [SerializeField] private string targetSceneName = "NestViewScene"; // 전환할 씬 이름 정확히 입력

    [Header("UI Feedback (Optional)")]
    [Tooltip("상호작용 안내 문구를 표시할 UI TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    // 내부 상태 변수
    private bool canInteract = false; // 현재 상호작용 가능한 상태인지 여부

    /// <summary>
    /// 스크립트 인스턴스가 로드될 때 한 번 호출됩니다.
    /// </summary>
    void Awake() // Awake 또는 Start에서 null 체크 부분도 확인
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
        else
        {
             // 경고 메시지도 타입에 맞게 수정 (선택 사항)
            Debug.LogWarning($"[{this.gameObject.name}] Interaction Prompt Text (TextMeshProUGUI)가 PlayerInteraction 스크립트에 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 매 프레임마다 호출됩니다. 키 입력 감지에 사용합니다.
    /// </summary>
    void Update()
    {
        // 상호작용 가능 상태이고, 지정된 상호작용 키를 눌렀다면
        if (canInteract && Input.GetKeyDown(interactionKey))
        {
            Debug.Log($"상호작용 키 '{interactionKey}' 입력됨. '{targetSceneName}' 씬 로드를 시도합니다.");
            LoadTargetScene(); // 씬 로드 함수 호출
        }
    }

    /// <summary>
    /// 다른 Collider2D(IsTrigger=true)의 영역에 진입했을 때 호출됩니다.
    /// </summary>
    /// <param name="other">진입한 영역의 Collider2D 정보</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 진입한 영역의 태그가 설정한 상호작용 태그와 일치하는지 확인
        if (other.CompareTag(interactableTag))
        {
            Debug.Log($"상호작용 가능 영역 진입: {other.gameObject.name}");
            canInteract = true; // 상호작용 가능 상태로 변경

            // 안내 문구가 있다면 표시합니다.
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(true);
                // 필요하다면 텍스트 내용 설정
                // interactionPromptText.text = $"Press '{interactionKey}' to enter {other.gameObject.name}";
            }
        }
    }

    /// <summary>
    /// 다른 Collider2D(IsTrigger=true)의 영역에서 빠져나왔을 때 호출됩니다.
    /// </summary>
    /// <param name="other">빠져나온 영역의 Collider2D 정보</param>
    void OnTriggerExit2D(Collider2D other)
    {
        // 빠져나온 영역의 태그가 설정한 상호작용 태그와 일치하는지 확인
        if (other.CompareTag(interactableTag))
        {
            Debug.Log($"상호작용 가능 영역 이탈: {other.gameObject.name}");
            canInteract = false; // 상호작용 불가능 상태로 변경

            // 안내 문구가 있다면 숨깁니다.
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 지정된 이름의 씬을 로드합니다.
    /// </summary>
    private void LoadTargetScene()
    {
        // 씬 이름이 유효한지 확인
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("전환할 씬 이름(Target Scene Name)이 비어있습니다! PlayerInteraction 스크립트의 Inspector 설정을 확인하세요.");
            return;
        }

        // SceneManager를 사용하여 씬 로드
        // 중요: targetSceneName으로 지정된 씬이 반드시 빌드 세팅에 추가되어 있어야 합니다!
        SceneManager.LoadScene(targetSceneName);
    }
}