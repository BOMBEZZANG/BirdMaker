using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 또는 using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private string houseTag = "HouseTrigger";
    [SerializeField] private string resourceTag = "ResourceNode"; // 나뭇가지 태그
    [SerializeField] private string featherTag = "FeatherNode";   // 깃털 태그
    [SerializeField] private string mossTag = "MossNode";       // *** 이끼 태그 추가 ***
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Scene Transition")]
    [SerializeField] private string nestSceneName = "NestViewScene";

    [Header("UI Feedback (Optional)")]
    [SerializeField] private Text interactionPromptText; // 또는 TextMeshProUGUI

    private bool canInteract = false;
    private GameObject currentInteractableObject = null;

    void Awake() { /* ... 기존 코드 ... */
        if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (canInteract && Input.GetKeyDown(interactionKey) && currentInteractableObject != null)
        {
            if (currentInteractableObject.CompareTag(houseTag)) { LoadTargetScene(nestSceneName); }
            // *** 자원 노드 상호작용 통합 (이끼 포함) ***
            else if (currentInteractableObject.CompareTag(resourceTag) ||
                     currentInteractableObject.CompareTag(featherTag) ||
                     currentInteractableObject.CompareTag(mossTag))
            {
                InteractWithResource(currentInteractableObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // *** 이끼 태그도 확인하도록 수정 ***
        if (other.CompareTag(houseTag) || other.CompareTag(resourceTag) || other.CompareTag(featherTag) || other.CompareTag(mossTag))
        {
             if (!canInteract) {
                // Debug.Log($"상호작용 가능 영역 진입: {other.gameObject.name} (태그: {other.tag})"); // 로그 레벨 조절
                canInteract = true;
                currentInteractableObject = other.gameObject;
                if (interactionPromptText != null) { interactionPromptText.text = $"Press '{interactionKey}' to interact"; interactionPromptText.gameObject.SetActive(true); }
             }
             // else { /* ... 이미 상호작용 중 로그 ... */ }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // *** 이끼 태그도 확인하도록 수정 ***
         if (other.gameObject == currentInteractableObject && (other.CompareTag(houseTag) || other.CompareTag(resourceTag) || other.CompareTag(featherTag) || other.CompareTag(mossTag)))
        {
            // Debug.Log($"상호작용 가능 영역({currentInteractableObject?.name}) 이탈."); // 로그 레벨 조절
            canInteract = false;
            currentInteractableObject = null;
            if (interactionPromptText != null) { interactionPromptText.gameObject.SetActive(false); }
        }
    }

    /// <summary>
    /// 지정된 자원 노드와 상호작용합니다. (수정됨 - 이끼 타입 추가)
    /// </summary>
    private void InteractWithResource(GameObject resourceObject)
    {
        bool collected = false; // 수집 성공 여부 플래그

        // 나뭇가지 노드 확인
        BranchNode branchNode = resourceObject.GetComponent<BranchNode>();
        if (branchNode != null) { branchNode.Collect(); collected = true; }

        // 깃털 노드 확인
        if (!collected) {
            FeatherNode featherNode = resourceObject.GetComponent<FeatherNode>();
            if (featherNode != null) { featherNode.Collect(); collected = true; }
        }

        // *** 이끼 노드 확인 추가 ***
    if (!collected) {
         MossNode mossNode = resourceObject.GetComponent<MossNode>();
         if (mossNode != null) {
             // *** 로그 추가: Collect 호출 직전 상태 확인 ***
             Debug.Log($"[PlayerInteraction] MossNode 발견. Collect 호출 시도. InventoryManager.Instance is null = {InventoryManager.Instance == null}");
             mossNode.Collect();
             collected = true;
         }
    }

        // 결과 처리
        if (collected && interactionPromptText != null) { interactionPromptText.gameObject.SetActive(false); }
        else if (!collected) { Debug.LogWarning($"'{resourceObject.name}' 오브젝트에서 인식 가능한 자원 노드 스크립트를 찾을 수 없습니다."); }
    }

    private void LoadTargetScene(string sceneName) {
        if (string.IsNullOrEmpty(sceneName)) { /* ... 에러 로그 ... */ return; }
        if (DataManager.Instance != null) { DataManager.Instance.SaveGameData(); } // 저장 호출
        SceneManager.LoadScene(sceneName);
    }
}