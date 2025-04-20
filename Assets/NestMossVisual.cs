using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

// FeatherVisual과 거의 동일하나, 제거 요청 함수 이름만 다름
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class NestMossVisual : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    // 외부 참조
    public NestInteraction nestInteractionManager;
    public GameObject removeButtonPrefab;
    public Canvas parentCanvasRef;

    [Header("Interaction Settings")]
    [SerializeField] private float longPressDuration = 0.5f;
    [SerializeField] private Vector2 removeButtonScreenOffset = new Vector2(30, 30);

    // 내부 상태 변수 (private)
    private bool isDragging = false;
    private bool isLongPressPossible = false;
    private bool isLongPressDetected = false;
    private Vector3 dragOffsetWorld;
    private Vector3 originalPositionWorld;
    private Coroutine longPressCoroutine;
    private GameObject currentRemoveButton;
    private SpriteRenderer spriteRenderer;
    private int originalSortOrder;
    private Camera mainCamera;

    // 상호작용 상태는 이제 NestInteraction에서 관리 (static 변수 제거)

    void Awake() { /* ... NestFeatherVisual과 동일 ... */
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer != null) originalSortOrder = spriteRenderer.sortingOrder;
        mainCamera = Camera.main;
        HideRemoveButton();
     }

    // --- Pointer Handlers ---
    public void OnPointerDown(PointerEventData eventData) { /* ... NestFeatherVisual과 동일 ... */
        // 편집 모드 + 다른 상호작용 없을 때 + 왼쪽 클릭
        if (nestInteractionManager == null || !nestInteractionManager.IsEditing) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (nestInteractionManager.IsAnyVisualInteracting() && currentRemoveButton == null) return; // Manager 통해 체크
        if (currentRemoveButton != null) { HideRemoveButton(); return; }
        isLongPressPossible = true; isLongPressDetected = false;
        if (longPressCoroutine != null) StopCoroutine(longPressCoroutine);
        longPressCoroutine = StartCoroutine(LongPressCheck(eventData));
    }
    public void OnBeginDrag(PointerEventData eventData) { /* ... NestFeatherVisual과 동일 ... */
        if (nestInteractionManager == null || !nestInteractionManager.IsEditing || eventData.button != PointerEventData.InputButton.Left || nestInteractionManager.IsAnyVisualInteracting() || isLongPressDetected) { eventData.pointerDrag = null; return; }
        isLongPressPossible = false;
        if (longPressCoroutine != null) { StopCoroutine(longPressCoroutine); longPressCoroutine = null; }
        isDragging = true;
        nestInteractionManager.SetInteractionActive(true); // Manager 통해 상태 설정
        originalPositionWorld = transform.position;
        Vector3 pointerWorldPos = GetWorldPosFromScreen(eventData.position);
        dragOffsetWorld = originalPositionWorld - pointerWorldPos;
        if(spriteRenderer != null) spriteRenderer.sortingOrder = originalSortOrder + 10;
        HideRemoveButton();
    }
    public void OnDrag(PointerEventData eventData) { /* ... NestFeatherVisual과 동일 ... */
        if (!isDragging || eventData.button != PointerEventData.InputButton.Left || nestInteractionManager == null || !nestInteractionManager.IsEditing) return;
        Vector3 pointerWorldPos = GetWorldPosFromScreen(eventData.position);
        transform.position = new Vector3(pointerWorldPos.x + dragOffsetWorld.x, pointerWorldPos.y + dragOffsetWorld.y, originalPositionWorld.z);
    }
    public void OnEndDrag(PointerEventData eventData) { /* ... NestFeatherVisual과 동일 ... */
        if (!isDragging || eventData.button != PointerEventData.InputButton.Left || nestInteractionManager == null || !nestInteractionManager.IsEditing) return;
        isDragging = false;
        // isAnyInteractionActive = false; // 여기서 바로 해제하지 않음!
        if(spriteRenderer != null) spriteRenderer.sortingOrder = originalSortOrder;
        bool droppedOnTrash = IsPointerOverTag(eventData, "TrashArea");
        if (droppedOnTrash) {
            nestInteractionManager.SetInteractionActive(false); // 상호작용 종료
            nestInteractionManager.RequestRemoveMoss(this.gameObject); // *** 제거 함수 변경 ***
        } else if (nestInteractionManager.IsPositionInNestArea(transform.position)) {
             nestInteractionManager.SetInteractionActive(false); // 상호작용 종료
             nestInteractionManager.NotifyMossPositionsChanged(); // *** 위치 변경 알림 함수 변경 ***
        } else {
            transform.position = originalPositionWorld; // 원위치
             nestInteractionManager.SetInteractionActive(false); // 상호작용 종료
        }
    }
    public void OnPointerUp(PointerEventData eventData) { /* ... NestFeatherVisual과 동일 ... */
        if (nestInteractionManager == null || !nestInteractionManager.IsEditing || eventData.button != PointerEventData.InputButton.Left) return;
        if (longPressCoroutine != null) { StopCoroutine(longPressCoroutine); longPressCoroutine = null; }
        isLongPressPossible = false;
        if (!isDragging && !isLongPressDetected) { nestInteractionManager.SetInteractionActive(false); } // 상태 해제
    }

    // --- Long Press Logic ---
    private IEnumerator LongPressCheck(PointerEventData eventData) { /* ... NestFeatherVisual과 동일 ... */
        float pressStartTime = Time.time; Vector2 startScreenPos = eventData.position;
        while (nestInteractionManager.IsEditing && isLongPressPossible && Input.GetMouseButton(0)) {
             if (Time.time < pressStartTime + longPressDuration) { if (Vector2.Distance(Input.mousePosition, startScreenPos) > (Screen.width * 0.02f)) { isLongPressPossible = false; yield break; } yield return null; continue; }
             if (!isLongPressDetected) { isLongPressDetected = true; isLongPressPossible = false; nestInteractionManager.SetInteractionActive(true); longPressCoroutine = null; ShowRemoveButton(); yield break; }
             yield break;
        }
        isLongPressPossible = false; longPressCoroutine = null;
    }

    // --- Button Show/Hide ---
    private void ShowRemoveButton() { /* ... NestFeatherVisual과 동일 ... */
        HideRemoveButton();
        if (removeButtonPrefab != null && parentCanvasRef != null && nestInteractionManager != null && mainCamera != null) {
            currentRemoveButton = Instantiate(removeButtonPrefab, parentCanvasRef.transform);
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            currentRemoveButton.transform.position = screenPos + (Vector3)removeButtonScreenOffset;
            RemoveButtonHandler handler = currentRemoveButton.GetComponent<RemoveButtonHandler>();
            if (handler != null) { handler.Initialize(this.gameObject, nestInteractionManager); } // Initialize는 동일하게 사용
            else { /* 에러 로그 */ Destroy(currentRemoveButton); nestInteractionManager.SetInteractionActive(false); }
        } else { /* 경고 로그 */ nestInteractionManager.SetInteractionActive(false); }
     }
    private void HideRemoveButton() { /* ... NestFeatherVisual과 동일 ... */
        bool wasButtonActive = (currentRemoveButton != null);
        if (currentRemoveButton != null) { Destroy(currentRemoveButton); currentRemoveButton = null; }
        if(wasButtonActive && !isDragging) { nestInteractionManager.SetInteractionActive(false); } // Manager 통해 상태 해제
        isLongPressDetected = false; isLongPressPossible = false;
        if (longPressCoroutine != null) { StopCoroutine(longPressCoroutine); longPressCoroutine = null; }
    }

    // --- Helper Methods ---
    private Vector3 GetWorldPosFromScreen(Vector2 screenPos) { /* ... NestFeatherVisual과 동일 ... */
        if (mainCamera == null) return transform.position;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 10f));
        worldPos.z = (originalPositionWorld == Vector3.zero && transform.position != Vector3.zero) ? transform.position.z : originalPositionWorld.z;
        return worldPos;
     }
    private bool IsPointerOverTag(PointerEventData eventData, string tag) { /* ... NestFeatherVisual과 동일 ... */
        List<RaycastResult> results = new List<RaycastResult>(); EventSystem.current.RaycastAll(eventData, results);
        foreach (RaycastResult result in results) { if (result.gameObject.CompareTag(tag)) { return true; } } return false;
     }

    /// <summary> 외부에서 상호작용 강제 취소 </summary>
    public void CancelInteraction() { /* ... NestFeatherVisual과 동일 ... */
         HideRemoveButton();
         if (isDragging) { isDragging = false; nestInteractionManager.SetInteractionActive(false); if(spriteRenderer != null) spriteRenderer.sortingOrder = originalSortOrder; transform.position = originalPositionWorld; }
    }

    // OnDestroy
    void OnDestroy() { /* ... NestFeatherVisual과 동일 ... */
        HideRemoveButton();
        if(nestInteractionManager != null && nestInteractionManager.IsAnyVisualInteracting() && (isDragging || isLongPressDetected)) { nestInteractionManager.SetInteractionActive(false); }
     }
}