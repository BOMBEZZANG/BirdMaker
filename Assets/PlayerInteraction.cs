using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 또는 using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("집 트리거에 설정된 태그입니다.")]
    [SerializeField] private string houseTag = "HouseTrigger"; // 집 태그
    [Tooltip("자원 노드 트리거에 설정된 태그입니다.")]
    [SerializeField] private string resourceTag = "ResourceNode"; // 자원 태그
    [Tooltip("상호작용을 위한 키 코드입니다.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Scene Transition")]
    [Tooltip("전환할 둥지 씬의 이름입니다.")]
    [SerializeField] private string nestSceneName = "NestViewScene";

    [Header("UI Feedback (Optional)")]
    [Tooltip("상호작용 안내 문구를 표시할 UI 컴포넌트입니다.")]
    [SerializeField] private Text interactionPromptText; // 또는 TextMeshProUGUI

    // 내부 상태 변수
    private bool canInteract = false;
    private GameObject currentInteractableObject = null; // 현재 상호작용 가능한 오브젝트 저장

    void Awake() // Start 대신 Awake 사용 권장
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (canInteract && Input.GetKeyDown(interactionKey) && currentInteractableObject != null)
        {
            // 어떤 오브젝트와 상호작용하는지 태그로 구분
            if (currentInteractableObject.CompareTag(houseTag))
            {
                Debug.Log($"집과 상호작용. '{nestSceneName}' 씬 로드.");
                LoadTargetScene(nestSceneName);
            }
            else if (currentInteractableObject.CompareTag(resourceTag))
            {
                Debug.Log("자원 노드와 상호작용.");
                InteractWithResource(currentInteractableObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 집 또는 자원 노드 태그를 가진 오브젝트인지 확인
        if (other.CompareTag(houseTag) || other.CompareTag(resourceTag))
        {
            Debug.Log($"상호작용 가능 영역 진입: {other.gameObject.name} (태그: {other.tag})");
            canInteract = true;
            currentInteractableObject = other.gameObject; // 상호작용 대상 저장

            if (interactionPromptText != null)
            {
                // 안내 문구 설정 (필요 시 태그별로 다르게)
                interactionPromptText.text = $"Press '{interactionKey}' to interact";
                interactionPromptText.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // 나가는 오브젝트가 현재 상호작용 대상이었는지 확인
        if (other.gameObject == currentInteractableObject)
        {
            Debug.Log($"상호작용 가능 영역 이탈: {other.gameObject.name}");
            canInteract = false;
            currentInteractableObject = null;

            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 지정된 자원 노드와 상호작용합니다.
    /// </summary>
    private void InteractWithResource(GameObject resourceObject)
    {
        // 자원 노드 오브젝트에서 BranchNode 스크립트를 가져옵니다.
        BranchNode branchNode = resourceObject.GetComponent<BranchNode>();
        if (branchNode != null)
        {
            branchNode.Collect(); // BranchNode의 Collect 함수 호출
            // 자원 수집 후에는 상호작용 상태 해제 (다시 접근해야 하도록)
            canInteract = false;
            currentInteractableObject = null;
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"'{resourceObject.name}' 오브젝트에서 BranchNode 스크립트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 지정된 이름의 씬을 로드합니다.
    /// </summary>
    private void LoadTargetScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("전환할 씬 이름이 비어있습니다!");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}