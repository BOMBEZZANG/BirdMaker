using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 또는 using TMPro;

/// <summary>
/// 플레이어와 상호작용 가능한 오브젝트들(집, 자원, 씬 이동 트리거, 상점 등)과의 상호작용을 처리합니다.
/// 플레이어 게임 오브젝트에 부착되어야 합니다.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Tags")]
    [SerializeField] private string houseTag = "HouseTrigger";
    [SerializeField] private string resourceTag = "ResourceNode";
    [SerializeField] private string featherTag = "FeatherNode";
    [SerializeField] private string mossTag = "MossNode";
    [SerializeField] private string townEntranceTag = "TownEntranceTrigger";
    [SerializeField] private string explorationEntranceTag = "ExplorationEntranceTrigger";
    [SerializeField] private string shopTag = "ShopTrigger";
    [SerializeField] private string shopkeeperTag = "ShopkeeperTrigger";
    [SerializeField] private string townReturnTag = "TownReturnTrigger";

    [Header("Interaction Key")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Scene Names")]
    [SerializeField] private string nestSceneName = "NestViewScene";
    [SerializeField] private string townSceneName = "TownViewScene";
    [SerializeField] private string explorationSceneName = "ExplorationViewScene";
    [SerializeField] private string shopSceneName = "ShopScene";

    [Header("UI Feedback (Optional)")]
    [SerializeField] private Text interactionPromptText;

    private bool canInteract = false;
    private GameObject currentInteractableObject = null;
    // UIManager 참조 제거됨

    void Start()
    {
        if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
    }

    [System.Obsolete]
    void Update()
    {
        // 상호작용 가능 상태 + 키 입력 + 대상 존재 확인
        if (canInteract && Input.GetKeyDown(interactionKey) && currentInteractableObject != null)
        {
            string tag = currentInteractableObject.tag;
            // *** 추가된 로그: 어떤 태그와 상호작용 시도하는지 확인 ***
            Debug.Log($"Interaction attempt with tag: [{tag}] on object: {currentInteractableObject.name}");

            // 태그에 따라 분기
            if (tag == houseTag) { LoadTargetScene(nestSceneName); }
            else if (tag == resourceTag || tag == featherTag || tag == mossTag) { InteractWithResource(currentInteractableObject); }
            // *** 추가된 로그: TownEntrance 태그 확인 직전 ***
            else if (tag == townEntranceTag)
            {
                // *** 추가된 로그: TownEntrance 태그 매치 성공 ***
                Debug.Log("TownEntranceTrigger tag matched. Calling LoadTargetScene...");
                LoadTargetScene(townSceneName);
            }
            else if (tag == explorationEntranceTag) { LoadTargetScene(explorationSceneName); }
            else if (tag == shopTag) { LoadTargetScene(shopSceneName); }
            else if (tag == shopkeeperTag) { InteractWithShopkeeper(); }
            else if (tag == townReturnTag) { LoadTargetScene(townSceneName); }
            else
            {
                Debug.LogWarning($"Unhandled interactable tag: [{tag}]");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // *** 추가된 로그: 트리거 진입 시 태그 상세 확인 ***
        Debug.Log($"OnTriggerEnter2D: Entered trigger of {other.gameObject.name} with tag: [{other.tag}]");

        bool isInteractable = other.CompareTag(houseTag) || other.CompareTag(resourceTag) ||
                              other.CompareTag(featherTag) || other.CompareTag(mossTag) ||
                              other.CompareTag(townEntranceTag) || other.CompareTag(explorationEntranceTag) ||
                              other.CompareTag(shopTag) || other.CompareTag(shopkeeperTag) ||
                              other.CompareTag(townReturnTag);

        if (isInteractable)
        {
             if (!canInteract) {
                Debug.Log($"Interactable object [{other.gameObject.name}] detected. Setting canInteract=true.");
                canInteract = true;
                currentInteractableObject = other.gameObject;
                if (interactionPromptText != null) { UpdateInteractionPrompt(other.tag); interactionPromptText.gameObject.SetActive(true); }
             }
        }
        // *** 추가된 로그: 상호작용 불가능 태그 확인 ***
        // else { Debug.Log($"Tag [{other.tag}] is not interactable."); }
    }

    void OnTriggerExit2D(Collider2D other)
    {
         if (other.gameObject == currentInteractableObject) // Check if we are exiting the object we were interacting with
        {
            // *** 추가된 로그: 트리거 이탈 확인 ***
            Debug.Log($"OnTriggerExit2D: Exited trigger of {other.gameObject.name}. Resetting interaction state.");
            canInteract = false;
            currentInteractableObject = null;
            if (interactionPromptText != null) { interactionPromptText.gameObject.SetActive(false); }
        }
    }

    private void InteractWithResource(GameObject resourceObject) {
        bool collected = false;
        BranchNode bNode = resourceObject.GetComponent<BranchNode>(); if(bNode != null){bNode.Collect(); collected = true;}
        if (!collected) { FeatherNode fNode = resourceObject.GetComponent<FeatherNode>(); if(fNode != null){fNode.Collect(); collected = true;} }
        if (!collected) { MossNode mNode = resourceObject.GetComponent<MossNode>(); if(mNode != null){mNode.Collect(); collected = true;} }

        if (collected) {
            canInteract = false; currentInteractableObject = null;
            if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
        } else if (!collected) { Debug.LogWarning($"'{resourceObject.name}' - No valid resource script found."); }
    }

    private void LoadTargetScene(string sceneName) {
        if (string.IsNullOrEmpty(sceneName)) { Debug.LogError("Scene name to load is empty!"); return; }
        // *** 추가된 로그: 저장 및 로드 호출 확인 ***
        Debug.Log($"LoadTargetScene: Attempting to save data before loading scene '{sceneName}'...");
        DataManager.Instance?.SaveGameData();
        Debug.Log($"LoadTargetScene: Calling SceneManager.LoadScene('{sceneName}').");
        SceneManager.LoadScene(sceneName);
    }

    [System.Obsolete]
    private void InteractWithShopkeeper() {
        Debug.Log("InteractWithShopkeeper: Attempting to open shop UI...");
        ShopUI shop = FindObjectOfType<ShopUI>();
        if (shop != null) { shop.Open(); }
        else { Debug.LogError("Could not find ShopUI in the current scene!"); }
    }

    private void UpdateInteractionPrompt(string tag) {
        if (interactionPromptText == null) return;
        string prompt = $"Press '{interactionKey}'";
        if (tag == houseTag) prompt += " to Enter Nest";
        else if (tag == townEntranceTag) prompt += " to Enter Town";
        else if (tag == explorationEntranceTag) prompt += " to Leave Town";
        else if (tag == shopTag) prompt += " to Enter Shop";
        else if (tag == shopkeeperTag) prompt += " to Talk";
        else if (tag == townReturnTag) prompt += " to Leave Shop";
        else if (tag == resourceTag) prompt += " to Collect Branch";
        else if (tag == featherTag) prompt += " to Collect Feather";
        else if (tag == mossTag) prompt += " to Collect Moss";
        else prompt += " to Interact";
        interactionPromptText.text = prompt;
    }
}