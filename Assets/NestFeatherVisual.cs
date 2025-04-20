using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class NestFeatherVisual : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    // ... (기존 변수 선언들: nestInteractionManager, removeButtonPrefab, parentCanvasRef, etc.) ...
    public NestInteraction nestInteractionManager;
    public GameObject removeButtonPrefab;
    public Canvas parentCanvasRef;
    [Header("Interaction Settings")]
    [SerializeField] private float longPressDuration = 0.5f;
    [SerializeField] private Vector2 removeButtonScreenOffset = new Vector2(30, 30);
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
    private static bool isAnyInteractionActive = false;

    // --- Static Methods ---
    public static bool IsAnyDraggingOrInteracting() { return isAnyInteractionActive; }
    public static void ClearInteractionFlag() { isAnyInteractionActive = false; }


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer != null) originalSortOrder = spriteRenderer.sortingOrder;
        mainCamera = Camera.main;
        HideRemoveButton();
    }

    // --- Pointer Handlers ---
    public void OnPointerDown(PointerEventData eventData) { /* ... 이전 코드와 동일 ... */
        if (nestInteractionManager == null || !nestInteractionManager.IsEditing) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (IsAnyDraggingOrInteracting() && currentRemoveButton == null) return;
        if (currentRemoveButton != null) { HideRemoveButton(); return; }
        isLongPressPossible = true;
        isLongPressDetected = false;
        if (longPressCoroutine != null) StopCoroutine(longPressCoroutine);
        longPressCoroutine = StartCoroutine(LongPressCheck(eventData));
    }
    public void OnBeginDrag(PointerEventData eventData) { /* ... 이전 코드와 동일 ... */
        if (nestInteractionManager == null || !nestInteractionManager.IsEditing || eventData.button != PointerEventData.InputButton.Left || IsAnyDraggingOrInteracting() || isLongPressDetected) { eventData.pointerDrag = null; return; }
        isLongPressPossible = false;
        if (longPressCoroutine != null) { StopCoroutine(longPressCoroutine); longPressCoroutine = null; }
        isDragging = true;
        isAnyInteractionActive = true;
        originalPositionWorld = transform.position;
        Vector3 pointerWorldPos = GetWorldPosFromScreen(eventData.position);
        dragOffsetWorld = originalPositionWorld - pointerWorldPos;
        if(spriteRenderer != null) spriteRenderer.sortingOrder = originalSortOrder + 10;
        HideRemoveButton();
    }
    public void OnDrag(PointerEventData eventData) { /* ... 이전 코드와 동일 ... */
        if (!isDragging || eventData.button != PointerEventData.InputButton.Left || nestInteractionManager == null || !nestInteractionManager.IsEditing) return;
        Vector3 pointerWorldPos = GetWorldPosFromScreen(eventData.position);
        transform.position = new Vector3(pointerWorldPos.x + dragOffsetWorld.x, pointerWorldPos.y + dragOffsetWorld.y, originalPositionWorld.z);
    }
    public void OnEndDrag(PointerEventData eventData) { /* ... 이전 코드와 동일 ... */
        if (!isDragging || eventData.button != PointerEventData.InputButton.Left || nestInteractionManager == null || !nestInteractionManager.IsEditing) return;
        isDragging = false;
        if(spriteRenderer != null) spriteRenderer.sortingOrder = originalSortOrder;
        bool droppedOnTrash = IsPointerOverTag(eventData, "TrashArea");
        if (droppedOnTrash) { isAnyInteractionActive = false; nestInteractionManager.RequestRemoveFeather(this.gameObject); }
        else if (nestInteractionManager.IsPositionInNestArea(transform.position)) { isAnyInteractionActive = false; nestInteractionManager.NotifyFeatherPositionsChanged(); }
        else { transform.position = originalPositionWorld; isAnyInteractionActive = false; }
    }
    public void OnPointerUp(PointerEventData eventData) { /* ... 이전 코드와 동일 ... */
        if (nestInteractionManager == null || !nestInteractionManager.IsEditing || eventData.button != PointerEventData.InputButton.Left) return;
        if (longPressCoroutine != null) { StopCoroutine(longPressCoroutine); longPressCoroutine = null; }
        isLongPressPossible = false;
        if (!isDragging && !isLongPressDetected) { isAnyInteractionActive = false; }
    }

    // --- Long Press Logic ---
    private IEnumerator LongPressCheck(PointerEventData eventData) { /* ... 이전 코드와 동일 ... */
        // Debug.Log($"[{gameObject.name}] LongPressCheck 코루틴 시작됨.");
        float pressStartTime = Time.time;
        Vector2 startScreenPos = eventData.position;
        while (nestInteractionManager.IsEditing && isLongPressPossible && Input.GetMouseButton(0)) {
             if (Time.time < pressStartTime + longPressDuration) { if (Vector2.Distance(Input.mousePosition, startScreenPos) > (Screen.width * 0.02f)) { isLongPressPossible = false; yield break; } yield return null; continue; }
             if (!isLongPressDetected) { isLongPressDetected = true; isLongPressPossible = false; isAnyInteractionActive = true; longPressCoroutine = null; ShowRemoveButton(); yield break; }
             yield break;
        }
        isLongPressPossible = false; longPressCoroutine = null;
    }

    // --- Button Show/Hide ---
    private void ShowRemoveButton() { /* ... 이전 코드와 동일 ... */
        // Debug.Log($"[{gameObject.name}] ShowRemoveButton 함수 호출됨.");
        HideRemoveButton();
        if (removeButtonPrefab != null && parentCanvasRef != null && nestInteractionManager != null && mainCamera != null) {
            currentRemoveButton = Instantiate(removeButtonPrefab, parentCanvasRef.transform);
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            currentRemoveButton.transform.position = screenPos + (Vector3)removeButtonScreenOffset;
            RemoveButtonHandler handler = currentRemoveButton.GetComponent<RemoveButtonHandler>();
            if (handler != null) { handler.Initialize(this.gameObject, nestInteractionManager); }
            else { Debug.LogError("..."); Destroy(currentRemoveButton); ClearInteractionFlag(); }
        } else { Debug.LogWarning("..."); ClearInteractionFlag(); }
     }
    private void HideRemoveButton() { /* ... 이전 코드와 동일 ... */
        bool wasButtonActive = (currentRemoveButton != null);
        if (currentRemoveButton != null) { Destroy(currentRemoveButton); currentRemoveButton = null; }
        if(wasButtonActive) { ClearInteractionFlag(); }
        isLongPressDetected = false; isLongPressPossible = false;
        if (longPressCoroutine != null) { StopCoroutine(longPressCoroutine); longPressCoroutine = null; }
    }

    // --- Helper Methods ---
    private Vector3 GetWorldPosFromScreen(Vector2 screenPos) { /* ... 이전 코드와 동일 ... */
        if (mainCamera == null) return transform.position;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 10f));
        worldPos.z = (originalPositionWorld == Vector3.zero && transform.position != Vector3.zero) ? transform.position.z : originalPositionWorld.z;
        return worldPos;
    }
     private bool IsPointerOverTag(PointerEventData eventData, string tag) { /* ... 이전 코드와 동일 ... */
         List<RaycastResult> results = new List<RaycastResult>();
         EventSystem.current.RaycastAll(eventData, results);
         foreach (RaycastResult result in results) { if (result.gameObject.CompareTag(tag)) { return true; } }
         return false;
     }

    // *** 새로 추가: 외부에서 상호작용 강제 취소 함수 ***
    public void CancelInteraction()
    {
         // Debug.Log($"[{gameObject.name}] Interaction Cancelled Externally.");
         HideRemoveButton(); // 버튼 숨기고 관련 상태 초기화
         if (isDragging) // 드래그 중이었다면
         {
             isDragging = false;
             ClearInteractionFlag(); // static 플래그 해제
             if(spriteRenderer != null) spriteRenderer.sortingOrder = originalSortOrder; // 정렬 순서 복구
             transform.position = originalPositionWorld; // 원위치
         }
         isLongPressPossible = false; // 다음 상호작용 위해 리셋
         if (longPressCoroutine != null) { StopCoroutine(longPressCoroutine); longPressCoroutine = null;} // 코루틴 중지
    }


    // OnDestroy
    void OnDestroy() { HideRemoveButton(); if(isAnyInteractionActive && (isDragging || isLongPressDetected)) ClearInteractionFlag(); }
}