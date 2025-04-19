using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class NestFeatherVisual : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (IsAnyDraggingOrInteracting() && currentRemoveButton == null) return; // 다른 상호작용 중이면 무시

        if (currentRemoveButton != null) // 버튼 이미 나와있으면 숨기기
        {
            HideRemoveButton(); // 내부에서 플래그 해제
            return;
        }

        // 롱프레스/드래그 시작 준비
        isLongPressPossible = true;
        isLongPressDetected = false;
        if (longPressCoroutine != null) StopCoroutine(longPressCoroutine);
        longPressCoroutine = StartCoroutine(LongPressCheck(eventData));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || IsAnyDraggingOrInteracting() || isLongPressDetected)
        {
            eventData.pointerDrag = null; return; // 드래그 시작 조건 안 맞으면 취소
        }

        isLongPressPossible = false; // 드래그 시작 시 롱프레스 불가
        if (longPressCoroutine != null) { StopCoroutine(longPressCoroutine); longPressCoroutine = null; }

        isDragging = true;
        isAnyInteractionActive = true; // 상호작용 시작
        originalPositionWorld = transform.position;

        Vector3 pointerWorldPos = GetWorldPosFromScreen(eventData.position);
        dragOffsetWorld = originalPositionWorld - pointerWorldPos;

        if(spriteRenderer != null) spriteRenderer.sortingOrder = originalSortOrder + 10;
        HideRemoveButton(); // 드래그 시작 시 버튼 숨김
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || eventData.button != PointerEventData.InputButton.Left) return;
        Vector3 pointerWorldPos = GetWorldPosFromScreen(eventData.position);
        // Z 좌표 유지하며 위치 업데이트
        transform.position = new Vector3(pointerWorldPos.x + dragOffsetWorld.x, pointerWorldPos.y + dragOffsetWorld.y, originalPositionWorld.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || eventData.button != PointerEventData.InputButton.Left) return;

        isDragging = false;
        // isAnyInteractionActive = false; // 여기서 해제하지 않음!
        if(spriteRenderer != null) spriteRenderer.sortingOrder = originalSortOrder;

        bool droppedOnTrash = IsPointerOverTag(eventData, "TrashArea");

        if (droppedOnTrash)
        {
            isAnyInteractionActive = false; // 상호작용 종료
            nestInteractionManager?.RequestRemoveFeather(this.gameObject);
        }
        else if (nestInteractionManager != null && nestInteractionManager.IsPositionInNestArea(transform.position))
        {
             // *** 위치 변경 알림 추가 ***
             isAnyInteractionActive = false; // 상호작용 종료
             nestInteractionManager.NotifyFeatherPositionsChanged(); // 저장 요청
        }
        else
        {
            transform.position = originalPositionWorld; // 원위치
             isAnyInteractionActive = false; // 상호작용 종료
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (longPressCoroutine != null) // 롱프레스 시간 미달 또는 드래그 없이 뗄 때
        {
            StopCoroutine(longPressCoroutine);
            longPressCoroutine = null;
        }
        isLongPressPossible = false; // 다음 Down을 위해 리셋

        // 버튼이 나와있지 않고, 드래그도 아니었다면 상호작용 플래그 해제
        if (!isDragging && !isLongPressDetected)
        {
            isAnyInteractionActive = false;
        }
        // 롱프레스 후 버튼 뗀 경우는 버튼 유지됨
    }

    // --- Long Press Logic ---
    private IEnumerator LongPressCheck(PointerEventData eventData)
    {
        // Debug.Log($"[{gameObject.name}] LongPressCheck 코루틴 시작됨.");
        float pressStartTime = Time.time;
        Vector2 startScreenPos = eventData.position;

        while (isLongPressPossible && Input.GetMouseButton(0))
        {
             if (Time.time < pressStartTime + longPressDuration) {
                 if (Vector2.Distance(Input.mousePosition, startScreenPos) > (Screen.width * 0.02f)) { isLongPressPossible = false; yield break; }
                 yield return null; continue;
             }
             if (!isLongPressDetected) {
                 // Debug.Log($"[{gameObject.name}] LongPressCheck: 시간 충족됨, 롱프레스 감지 시도.");
                 isLongPressDetected = true;
                 isLongPressPossible = false;
                 isAnyInteractionActive = true; // 버튼 표시로 상호작용 시작
                 longPressCoroutine = null;
                 // Debug.Log($"[{gameObject.name}] 롱프레스 감지됨! 제거 버튼 표시 시도.");
                 ShowRemoveButton();
                 yield break;
             }
             yield break;
        }
        isLongPressPossible = false;
        longPressCoroutine = null;
    }

    // --- Button Show/Hide ---
    private void ShowRemoveButton()
    {
        // Debug.Log($"[{gameObject.name}] ShowRemoveButton 호출됨.");
        HideRemoveButton(); // 기존 버튼 제거

        if (removeButtonPrefab != null && parentCanvasRef != null && nestInteractionManager != null && mainCamera != null) {
            currentRemoveButton = Instantiate(removeButtonPrefab, parentCanvasRef.transform); // Canvas 하위 생성
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            currentRemoveButton.transform.position = screenPos + (Vector3)removeButtonScreenOffset; // 스크린 오프셋 적용
            RemoveButtonHandler handler = currentRemoveButton.GetComponent<RemoveButtonHandler>();
            if (handler != null) { handler.Initialize(this.gameObject, nestInteractionManager); }
            else { /* 핸들러 없음 에러 */ Destroy(currentRemoveButton); isAnyInteractionActive = false; } // 실패 시 플래그 해제
        } else { /* 참조 없음 경고 */ isAnyInteractionActive = false; } // 실패 시 플래그 해제
    }
    private void HideRemoveButton()
    {
        bool wasButtonActive = (currentRemoveButton != null);
        if (currentRemoveButton != null) { Destroy(currentRemoveButton); currentRemoveButton = null; }
        if(wasButtonActive) { isAnyInteractionActive = false; } // 버튼 숨길 때 플래그 해제
        isLongPressDetected = false;
        isLongPressPossible = false;
        if (longPressCoroutine != null) { StopCoroutine(longPressCoroutine); longPressCoroutine = null; }
    }

    // --- Helper Methods ---
    private Vector3 GetWorldPosFromScreen(Vector2 screenPos)
    {
        if (mainCamera == null) return transform.position;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 10f));
        // originalPositionWorld가 초기화 안됐을 경우 대비
        worldPos.z = (originalPositionWorld == Vector3.zero && transform.position != Vector3.zero) ? transform.position.z : originalPositionWorld.z;
        return worldPos;
    }
     private bool IsPointerOverTag(PointerEventData eventData, string tag)
     {
         List<RaycastResult> results = new List<RaycastResult>();
         EventSystem.current.RaycastAll(eventData, results);
         foreach (RaycastResult result in results) { if (result.gameObject.CompareTag(tag)) { return true; } }
         return false;
     }

    // 오브젝트 파괴 시 정리
    void OnDestroy()
    {
        HideRemoveButton(); // 버튼과 코루틴 정리
        // 파괴될 때 자신이 상호작용 중이었다면 static 플래그 해제
        if(isAnyInteractionActive && (isDragging || isLongPressDetected)) ClearInteractionFlag();
    }
}