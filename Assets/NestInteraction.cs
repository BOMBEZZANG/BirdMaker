using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Random = UnityEngine.Random; // GetRandomValidPositionInNest 에서 사용
using System.Collections;
using System.Linq; // List 변환 위해 추가

/// <summary>
/// 둥지 오브젝트에 부착되어 깃털 추가/제거 상호작용 및 시각 효과, 편집 모드를 관리합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NestInteraction : MonoBehaviour, IPointerClickHandler
{
    [Header("Feather Visuals")]
    [Tooltip("둥지에 배치될 깃털 시각 오브젝트의 프리팹")]
    [SerializeField] private GameObject featherVisualPrefab;
    [Tooltip("깃털이 배치될 영역을 정의하는 콜라이더")]
    [SerializeField] private Collider2D nestAreaCollider;

    [Header("Warmth Settings")]
    [Tooltip("깃털 1개당 알에게 전달/제거할 온기량")]
    [SerializeField] private float warmthPerFeather = 5f;

    [Header("UI Elements")]
    [Tooltip("깃털 제거 버튼 프리팹")]
    [SerializeField] private GameObject removeButtonPrefab;
    [Tooltip("UI 요소(제거 버튼 등)가 배치될 부모 Canvas")]
    [SerializeField] private Canvas parentCanvas;
    [Tooltip("편집 모드 시 활성화될 배경 어둡게 하는 Panel")]
    [SerializeField] private GameObject editModeDimPanel;
    // *** 변수 선언 확인 ***
    [Tooltip("편집 모드 시 활성화될 툴바 Panel")]
    [SerializeField] private GameObject editModeToolbarPanel; // Inspector에서 연결 필요!

    // 내부 관리용 변수
    private EggController eggController;
    private List<GameObject> activeFeatherVisuals = new List<GameObject>();
    public bool IsEditing { get; private set; } = false; // 편집 모드 상태
    private Camera mainCamera; // 카메라 캐싱

    [System.Obsolete]
    void Start()
    {
        mainCamera = Camera.main; // 메인 카메라 캐싱
        eggController = FindObjectOfType<EggController>();
        // 필수 참조 확인
        if (featherVisualPrefab == null) Debug.LogError("Feather Visual Prefab Missing!", this);
        if (nestAreaCollider == null) Debug.LogError("Nest Area Collider Missing!", this);
        if (removeButtonPrefab == null) Debug.LogError("Remove Button Prefab Missing!", this);
        if (parentCanvas == null) Debug.LogError("Parent Canvas Missing!", this);
        if (editModeDimPanel == null) Debug.LogError("Edit Mode Dim Panel Missing!", this);
        // *** editModeToolbarPanel null 체크 추가 ***
        if (editModeToolbarPanel == null) Debug.LogError("Edit Mode Toolbar Panel Missing!", this);
        if (eggController == null) Debug.LogError("EggController Missing!", this);
        if (InventoryManager.Instance == null) Debug.LogError("InventoryManager Instance Missing!", this);
        if (DataManager.Instance == null) Debug.LogError("DataManager Instance Missing!", this);

        SetEditMode(false); // 시작 시 편집 모드 비활성화 및 관련 UI 설정
        StartCoroutine(InitializeFeathersAfterOneFrame()); // 저장된 깃털 복원
    }

    /// <summary> 저장된 데이터 기준으로 초기 깃털 위치 복원 </summary>
    private IEnumerator InitializeFeathersAfterOneFrame()
    {
        yield return null; // 데이터 로드 기다림
        if (DataManager.Instance?.CurrentGameData != null)
        {
            List<Vector2> loadedPositions = DataManager.Instance.CurrentGameData.featherPositions;
            ClearAllFeatherVisuals();
            foreach (Vector2 pos in loadedPositions)
            {
                SpawnFeatherVisualAt(new Vector3(pos.x, pos.y, GetFeatherZPos()), false);
            }
        }
        else { /* 데이터 준비 안됨 로그 */ }
    }

    /// <summary> 둥지 클릭 시 (깃털 추가 - 편집 모드에서만 작동) </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 편집 모드가 아니거나, 다른 깃털 상호작용 중이면 무시
        if (!IsEditing || NestFeatherVisual.IsAnyDraggingOrInteracting()) return;

        // 왼쪽 클릭일 때만 깃털 추가 시도
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (InventoryManager.Instance == null || DataManager.Instance == null || eggController == null || mainCamera == null) return;

            // 클릭된 화면 좌표를 월드 좌표로 변환
            float targetZ = GetFeatherZPos();
            Vector3 clickWorldPosition = Vector3.zero;
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);
            if (Mathf.Abs(ray.direction.z) > 0.0001f) { float t = (targetZ - ray.origin.z) / ray.direction.z; clickWorldPosition = ray.GetPoint(t); }
            else { clickWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, mainCamera.nearClipPlane + Mathf.Abs(targetZ - mainCamera.transform.position.z))); clickWorldPosition.z = targetZ; }

            // 클릭 위치 유효성 검사 후 깃털 추가 시도
            if (IsPositionInNestArea(clickWorldPosition))
            {
                TryAddFeatherAt(clickWorldPosition);
            }
            else { Debug.Log("클릭한 위치가 둥지 영역 내부가 아닙니다."); }
        }
    }

    /// <summary> 지정된 위치에 깃털 추가를 시도합니다. </summary>
    private void TryAddFeatherAt(Vector3 spawnPosition)
    {
         if (InventoryManager.Instance.featherCount > 0)
         {
             if (InventoryManager.Instance.UseFeather())
             {
                 SpawnFeatherVisualAt(spawnPosition, true);
                 eggController.AddWarmth(warmthPerFeather);
                 NotifyFeatherPositionsChanged();
             }
         }
         else { Debug.Log("플레이어가 가진 깃털이 없습니다."); }
    }

    /// <summary> 개별 깃털 제거 요청 처리 </summary>
    public void RequestRemoveFeather(GameObject featherToRemove)
    {
        if (!IsEditing) { /* 편집 모드 아닐 때 로그/처리 */ return; }
        if (InventoryManager.Instance == null || DataManager.Instance == null || eggController == null || featherToRemove == null) return;
        if (activeFeatherVisuals.Remove(featherToRemove))
        {
             InventoryManager.Instance.AddFeathers(1);
             eggController.RemoveWarmth(warmthPerFeather);
             Destroy(featherToRemove);
             NotifyFeatherPositionsChanged();
        }
         else { /* 리스트 없음 경고 */ }
    }

    /// <summary> 지정된 월드 좌표에 깃털 프리팹 인스턴스를 생성하고 배치합니다. </summary>
    private void SpawnFeatherVisualAt(Vector3 worldPosition, bool logCreation = true)
    {
        if (featherVisualPrefab == null || parentCanvas == null) { /* 참조 없음 로그 */ return; }
        GameObject newFeather = Instantiate(featherVisualPrefab, worldPosition, Quaternion.identity);
        NestFeatherVisual visualScript = newFeather.GetComponent<NestFeatherVisual>();
        if (visualScript != null) {
            visualScript.nestInteractionManager = this;
            visualScript.removeButtonPrefab = this.removeButtonPrefab;
            visualScript.parentCanvasRef = this.parentCanvas;
        } else { /* 스크립트 없음 경고 */ }
        activeFeatherVisuals.Add(newFeather);
        // if(logCreation) Debug.Log($"깃털 비주얼 생성됨...");
    }

    /// <summary> 지정된 위치가 둥지 영역 내에 있는지 확인 </summary>
    public bool IsPositionInNestArea(Vector3 worldPosition) {
        if (nestAreaCollider == null) return false;
        return nestAreaCollider.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y));
     }

    /// <summary> 현재 깃털 위치 리스트를 DataManager에 업데이트 요청 </summary>
    public void NotifyFeatherPositionsChanged() {
         if (DataManager.Instance != null) {
             List<Vector2> currentPositions = activeFeatherVisuals.Where(f => f != null).Select(f => (Vector2)f.transform.position).ToList();
             DataManager.Instance.UpdateFeatherPositions(currentPositions);
         }
     }

    // GetRandomValidPositionInNest 함수 (다른 곳에서 사용 안하면 삭제 가능)
    private Vector3 GetRandomValidPositionInNest() {
         if (nestAreaCollider == null) return Vector3.positiveInfinity;
         Bounds bounds = nestAreaCollider.bounds;
         int maxAttempts = 100;
         for (int i = 0; i < maxAttempts; i++) {
             float randomX = Random.Range(bounds.min.x, bounds.max.x);
             float randomY = Random.Range(bounds.min.y, bounds.max.y);
             Vector2 randomPoint = new Vector2(randomX, randomY);
             if (nestAreaCollider.OverlapPoint(randomPoint)) { return new Vector3(randomX, randomY, GetFeatherZPos()); }
         }
         Debug.LogWarning($"둥지 영역 내부 랜덤 위치 찾기 실패...");
         return Vector3.positiveInfinity;
     }

    /// <summary> 현재 활성화된 모든 깃털 비주얼 제거 </summary>
    private void ClearAllFeatherVisuals() {
          for(int i = activeFeatherVisuals.Count - 1; i >= 0; i--) { if(activeFeatherVisuals[i] != null) Destroy(activeFeatherVisuals[i]); }
          activeFeatherVisuals.Clear();
      }

    /// <summary> 깃털이 배치될 Z 좌표 계산 </summary>
    private float GetFeatherZPos() { return this.transform.position.z - 0.1f; }

    // --- 편집 모드 관리 ---
    /// <summary> 둥지 편집 모드 설정 (UI 토글에서 호출) </summary>
    public void SetEditMode(bool isOn)
    {
        if (IsEditing == isOn) return;
        IsEditing = isOn;
        // Debug.Log($"둥지 편집 모드: {(IsEditing ? "활성화됨" : "비활성화됨")}");
        UpdateVisualsForEditMode(IsEditing);
        if (!IsEditing) { CancelAllFeatherInteractions(); } // 편집 모드 종료 시 상호작용 취소
    }

    /// <summary> 편집 모드 시각 효과 및 툴바 업데이트 </summary>
    private void UpdateVisualsForEditMode(bool isEditing)
    {
        if (editModeDimPanel != null) { editModeDimPanel.SetActive(isEditing); }
        else { Debug.LogError("UpdateVisualsForEditMode: editModeDimPanel 참조가 NULL입니다!", this); }

        // *** editModeToolbarPanel 사용 ***
        if (editModeToolbarPanel != null) { editModeToolbarPanel.SetActive(isEditing); }
        else { Debug.LogError("UpdateVisualsForEditMode: editModeToolbarPanel 참조가 NULL입니다!", this); }
    }

    /// <summary> 모든 활성 깃털의 상호작용 상태를 취소합니다. </summary>
    private void CancelAllFeatherInteractions()
    {
        foreach(GameObject featherGO in activeFeatherVisuals)
        {
             NestFeatherVisual visual = featherGO?.GetComponent<NestFeatherVisual>();
             visual?.CancelInteraction();
        }
        NestFeatherVisual.ClearInteractionFlag();
    }
}