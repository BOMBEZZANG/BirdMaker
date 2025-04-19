using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 또는 using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private string houseTag = "HouseTrigger";
    [SerializeField] private string resourceTag = "ResourceNode"; // 나뭇가지 태그
    [SerializeField] private string featherTag = "FeatherNode";   // *** 깃털 태그 추가 ***
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Scene Transition")]
    [SerializeField] private string nestSceneName = "NestViewScene";

    [Header("UI Feedback (Optional)")]
    [SerializeField] private Text interactionPromptText; // 또는 TextMeshProUGUI

    private bool canInteract = false;
    private GameObject currentInteractableObject = null;

    void Awake() { /* ... 기존 코드 ... */ }

    void Update()
    {
        if (canInteract && Input.GetKeyDown(interactionKey) && currentInteractableObject != null)
        {
            if (currentInteractableObject.CompareTag(houseTag))
            {
                LoadTargetScene(nestSceneName);
            }
            // *** 자원 노드 상호작용 통합 ***
            else if (currentInteractableObject.CompareTag(resourceTag) || currentInteractableObject.CompareTag(featherTag))
            {
                InteractWithResource(currentInteractableObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // *** 깃털 태그도 확인하도록 수정 ***
        if (other.CompareTag(houseTag) || other.CompareTag(resourceTag) || other.CompareTag(featherTag))
        {
             if (!canInteract)
             {
                Debug.Log($"상호작용 가능 영역 진입: {other.gameObject.name} (태그: {other.tag})");
                canInteract = true;
                currentInteractableObject = other.gameObject;

                if (interactionPromptText != null)
                {
                    // 프롬프트 텍스트는 필요 시 더 구체적으로 설정 가능
                    interactionPromptText.text = $"Press '{interactionKey}' to interact";
                    interactionPromptText.gameObject.SetActive(true);
                }
            }
             // else { ... 이미 상호작용 중 로그 ... }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // *** 깃털 태그도 확인하도록 수정 ***
         if (other.gameObject == currentInteractableObject && (other.CompareTag(houseTag) || other.CompareTag(resourceTag) || other.CompareTag(featherTag)))
        {
            Debug.Log($"상호작용 가능 영역({currentInteractableObject?.name}) 이탈."); // Null 조건 연산자 사용
            canInteract = false;
            currentInteractableObject = null;

            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 지정된 자원 노드와 상호작용합니다. (수정됨 - 자원 타입 구분)
    /// </summary>
    private void InteractWithResource(GameObject resourceObject)
    {
        bool collected = false; // 수집 성공 여부 플래그

        // 나뭇가지 노드인지 확인
        BranchNode branchNode = resourceObject.GetComponent<BranchNode>();
        if (branchNode != null)
        {
            Debug.Log($"'{resourceObject.name}' (나뭇가지) 수집 처리 시작.");
            branchNode.Collect();
            collected = true;
        }

        // 깃털 노드인지 확인 (나뭇가지가 아닐 경우)
        if (!collected) // 나뭇가지가 아니었을 때만 깃털 확인
        {
            FeatherNode featherNode = resourceObject.GetComponent<FeatherNode>();
            if (featherNode != null)
            {
                Debug.Log($"'{resourceObject.name}' (깃털) 수집 처리 시작.");
                featherNode.Collect();
                collected = true;
            }
        }

        // 수집에 성공했다면 UI 숨김 (실패 시 경고 로그는 각 노드에서 처리)
        if (collected && interactionPromptText != null)
        {
             interactionPromptText.gameObject.SetActive(false);
        }
        else if (!collected)
        {
             Debug.LogWarning($"'{resourceObject.name}' 오브젝트에서 인식 가능한 자원 노드 스크립트(BranchNode, FeatherNode)를 찾을 수 없습니다.");
        }
    }
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