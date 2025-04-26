using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Collider2D))]
public class NestInteraction : MonoBehaviour, IPointerClickHandler
{
    [Header("Item Prefabs")]
    [SerializeField] private GameObject featherVisualPrefab;
    [SerializeField] private GameObject mossVisualPrefab;

    [Header("Area Collider")]
    [Tooltip("깃털/이끼가 배치될 영역을 정의하는 콜라이더")]
    [SerializeField] private Collider2D nestAreaCollider;

    [Header("Item Effects")]
    [Tooltip("깃털 1개당 효과")]
    [SerializeField] private float warmthPerFeather = 5f; // 이 값은 NestEnvironmentManager로 이동 완료됨 (참고용)
    [Tooltip("이끼 1개당 효과")]
    [SerializeField] private float humidityPerMoss = 5f; // 이 값은 NestEnvironmentManager로 이동 완료됨 (참고용)

    [Header("UI Elements")]
    [Tooltip("깃털/이끼 제거 버튼 프리팹")]
    [SerializeField] private GameObject removeButtonPrefab;
    [Tooltip("UI 요소(제거 버튼 등)가 배치될 부모 Canvas")]
    [SerializeField] private Canvas parentCanvas;
    [Tooltip("편집 모드 시 활성화될 배경 어둡게 하는 Panel")]
    [SerializeField] private GameObject editModeDimPanel;
    [Tooltip("편집 모드 시 활성화될 툴바 Panel")]
    [SerializeField] private GameObject editModeToolbarPanel;
    [Tooltip("툴바 UI 컨트롤러 스크립트 참조")]
    [SerializeField] private EditToolbarController toolbarController;

    // 내부 관리용 변수
    // private EggController eggController; // 제거됨
    private List<GameObject> activeFeatherVisuals = new List<GameObject>();
    private List<GameObject> activeMossVisuals = new List<GameObject>();
    public bool IsEditing { get; private set; } = false; // 편집 모드 상태
    private Camera mainCamera; // 카메라 캐싱
    private bool isAnyVisualInteracting = false; // 현재 어떤 비주얼 오브젝트든 상호작용 중인지

    void Start()
    {
        mainCamera = Camera.main;
        // 필수 참조 확인
        if (featherVisualPrefab == null) Debug.LogError("Feather Visual Prefab Missing!", this);
        if (mossVisualPrefab == null) Debug.LogError("Moss Visual Prefab Missing!", this);
        if (nestAreaCollider == null) Debug.LogError("Nest Area Collider Missing!", this);
        if (removeButtonPrefab == null) Debug.LogError("Remove Button Prefab Missing!", this);
        if (parentCanvas == null) Debug.LogError("Parent Canvas Missing!", this);
        if (editModeDimPanel == null) Debug.LogError("Edit Mode Dim Panel Missing!", this);
        if (editModeToolbarPanel == null) Debug.LogError("Edit Mode Toolbar Panel Missing!", this);
        if (toolbarController == null) Debug.LogError("Toolbar Controller Missing!", this);
        if (InventoryManager.Instance == null) Debug.LogError("InventoryManager Instance Missing!", this);
        if (DataManager.Instance == null) Debug.LogError("DataManager Instance Missing!", this);
        if (NestEnvironmentManager.Instance == null) Debug.LogError("NestEnvironmentManager Instance Missing!", this);

        SetEditMode(false); // 시작 시 편집 모드 비활성화
        StartCoroutine(InitializeVisualsAfterOneFrame()); // 저장된 비주얼 복원
    }

    /// <summary> 저장된 데이터 기준으로 초기 비주얼 위치 복원 </summary>
    private IEnumerator InitializeVisualsAfterOneFrame()
    {
        yield return null; // 한 프레임 대기
        // 매니저 초기화 대기
        float timeout = Time.time + 5f;
        while ((DataManager.Instance == null || !DataManager.Instance.IsDataManagerInitialized || NestEnvironmentManager.Instance == null || !NestEnvironmentManager.Instance.IsInitialized) && Time.time < timeout)
        { yield return null; }

        if (DataManager.Instance?.CurrentGameData != null)
        {
            GameData data = DataManager.Instance.CurrentGameData;
            ClearAllVisuals();
            // 깃털 복원
            foreach (Vector2 pos in data.featherPositions) { SpawnFeatherVisualAt(new Vector3(pos.x, pos.y, GetItemZPos()), false); }
            // 이끼 복원
            foreach (Vector2 pos in data.mossPositions) { SpawnMossVisualAt(new Vector3(pos.x, pos.y, GetItemZPos()), false); }
            // 로드 후 환경 즉시 재계산
            NestEnvironmentManager.Instance?.RecalculateEnvironment();
        }
        else { Debug.LogError("DataManager/데이터 준비 안됨. 초기 비주얼 생성 불가."); }
    }

    /// <summary> 둥지 클릭 시 (편집 모드에서 아이템 배치) </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsEditing || IsAnyVisualInteracting()) return; // 편집 모드 + 다른 상호작용 없을 때만
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            EditToolbarController.PlacementType selection = toolbarController.CurrentSelection;
            if (selection == EditToolbarController.PlacementType.None) return; // 선택된 아이템 없으면 무시

            Vector3 clickWorldPosition = GetWorldPosFromScreenClick(eventData);
            // GetWorldPosFromScreenClick 실패 시 처리
            if (clickWorldPosition == Vector3.positiveInfinity) return;

            if (IsPositionInNestArea(clickWorldPosition))
            {
                TryAddItemAt(clickWorldPosition, selection); // 선택된 아이템 배치 시도
            }
            else { Debug.Log("클릭한 위치가 둥지 영역 내부가 아닙니다."); }
        }
    }

    /// <summary> 지정된 위치에 선택된 아이템 추가 시도 </summary>
    private void TryAddItemAt(Vector3 spawnPosition, EditToolbarController.PlacementType itemType)
    {
        if (InventoryManager.Instance == null || NestEnvironmentManager.Instance == null) return; // 매니저 확인

        bool itemUsed = false;
        if (itemType == EditToolbarController.PlacementType.Feather && InventoryManager.Instance.featherCount > 0) { itemUsed = InventoryManager.Instance.UseFeather(); }
        else if (itemType == EditToolbarController.PlacementType.Moss && InventoryManager.Instance.mossCount > 0) { itemUsed = InventoryManager.Instance.UseMoss(); }
        else { Debug.Log($"인벤토리에 {itemType} 없음."); return; }

        if (itemUsed)
        {
            if (itemType == EditToolbarController.PlacementType.Feather) { SpawnFeatherVisualAt(spawnPosition, true); NotifyFeatherPositionsChanged(); }
            else if (itemType == EditToolbarController.PlacementType.Moss) { SpawnMossVisualAt(spawnPosition, true); NotifyMossPositionsChanged(); }
            // 환경 재계산은 Notify... 함수 내부에서 호출됨
        }
    }

    // --- 아이템 제거 요청 처리 ---
    public void RequestRemoveFeather(GameObject featherToRemove)
    {
        if (!IsEditing || InventoryManager.Instance == null || featherToRemove == null) return;
        if (activeFeatherVisuals.Remove(featherToRemove)) { InventoryManager.Instance.AddFeathers(1); Destroy(featherToRemove); NotifyFeatherPositionsChanged(); NestEnvironmentManager.Instance?.RecalculateEnvironment(); }
    }
    public void RequestRemoveMoss(GameObject mossToRemove)
    {
        if (!IsEditing || InventoryManager.Instance == null || mossToRemove == null) return;
        if (activeMossVisuals.Remove(mossToRemove)) { InventoryManager.Instance.AddMoss(1); Destroy(mossToRemove); NotifyMossPositionsChanged(); NestEnvironmentManager.Instance?.RecalculateEnvironment(); }
    }

    // --- 비주얼 생성 ---
    private void SpawnFeatherVisualAt(Vector3 worldPosition, bool logCreation = true) {
        if (featherVisualPrefab == null || parentCanvas == null) return;
        GameObject newFeather = Instantiate(featherVisualPrefab, worldPosition, Quaternion.identity);
        NestFeatherVisual visualScript = newFeather.GetComponent<NestFeatherVisual>();
        if (visualScript != null) { visualScript.nestInteractionManager = this; visualScript.removeButtonPrefab = this.removeButtonPrefab; visualScript.parentCanvasRef = this.parentCanvas; }
        activeFeatherVisuals.Add(newFeather);
     }
    private void SpawnMossVisualAt(Vector3 worldPosition, bool logCreation = true) {
        if (mossVisualPrefab == null || parentCanvas == null) return;
        GameObject newMoss = Instantiate(mossVisualPrefab, worldPosition, Quaternion.identity);
        NestMossVisual visualScript = newMoss.GetComponent<NestMossVisual>();
        if (visualScript != null) { visualScript.nestInteractionManager = this; visualScript.removeButtonPrefab = this.removeButtonPrefab; visualScript.parentCanvasRef = this.parentCanvas; }
        activeMossVisuals.Add(newMoss);
     }

    // --- 위치/상태 업데이트 알림 ---
    public void NotifyFeatherPositionsChanged() {
         if (DataManager.Instance != null) { List<Vector2> pos = activeFeatherVisuals.Where(f => f != null).Select(f => (Vector2)f.transform.position).ToList(); DataManager.Instance.UpdateFeatherPositions(pos); }
         NestEnvironmentManager.Instance?.RecalculateEnvironment();
     }
    public void NotifyMossPositionsChanged() {
         if (DataManager.Instance != null) { List<Vector2> pos = activeMossVisuals.Where(f => f != null).Select(f => (Vector2)f.transform.position).ToList(); DataManager.Instance.UpdateMossPositions(pos); }
         NestEnvironmentManager.Instance?.RecalculateEnvironment();
     }

    // --- 편집 모드 및 상호작용 상태 관리 ---
    public bool IsAnyVisualInteracting() { return isAnyVisualInteracting; }
    public void SetInteractionActive(bool active) { isAnyVisualInteracting = active; }
    public void SetEditMode(bool isOn) { if (IsEditing == isOn) return; IsEditing = isOn; UpdateVisualsForEditMode(IsEditing); if (!IsEditing) { CancelAllVisualInteractions(); } }
    private void UpdateVisualsForEditMode(bool isEditing) { if (editModeDimPanel != null) editModeDimPanel.SetActive(isEditing); if (editModeToolbarPanel != null) editModeToolbarPanel.SetActive(isEditing); }
    private void CancelAllVisualInteractions() { foreach(GameObject go in activeFeatherVisuals) { go?.GetComponent<NestFeatherVisual>()?.CancelInteraction(); } foreach(GameObject go in activeMossVisuals) { go?.GetComponent<NestMossVisual>()?.CancelInteraction(); } SetInteractionActive(false); toolbarController?.Deselect(); }

    // --- Helper 함수들 ---
    /// <summary> 지정된 위치가 둥지 영역 내에 있는지 확인 (수정됨: null 체크 및 반환 경로 확인) </summary>
    public bool IsPositionInNestArea(Vector3 worldPosition)
    {
        if (nestAreaCollider == null)
        {
             Debug.LogWarning("IsPositionInNestArea: Nest Area Collider is not assigned!");
             return false; // 콜라이더 없으면 false 반환
        }
        // OverlapPoint 결과 반환
        return nestAreaCollider.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y));
    }

    /// <summary> 아이템이 배치될 Z 좌표 계산 </summary>
    private float GetItemZPos() { return this.transform.position.z - 0.1f; } // 둥지보다 약간 앞에

    /// <summary> 현재 활성화된 모든 깃털과 이끼 비주얼 제거 </summary>
    private void ClearAllVisuals() {
          for(int i = activeFeatherVisuals.Count - 1; i >= 0; i--) { if(activeFeatherVisuals[i] != null) Destroy(activeFeatherVisuals[i]); }
          activeFeatherVisuals.Clear();
          for(int i = activeMossVisuals.Count - 1; i >= 0; i--) { if(activeMossVisuals[i] != null) Destroy(activeMossVisuals[i]); }
          activeMossVisuals.Clear();
      }

    /// <summary> 클릭된 화면 좌표를 월드 좌표로 변환 (수정됨: null 체크 시 return 추가) </summary>
    private Vector3 GetWorldPosFromScreenClick(PointerEventData eventData)
    {
         if (mainCamera == null) { Debug.LogError("카메라 참조 없음! 월드 좌표를 계산할 수 없습니다."); return Vector3.positiveInfinity; } // 실패 값 반환

         float targetZ = GetItemZPos();
         Vector3 worldPosition = Vector3.zero;
         Ray ray = mainCamera.ScreenPointToRay(eventData.position);
         if (Mathf.Abs(ray.direction.z) > 0.0001f) { float t = (targetZ - ray.origin.z) / ray.direction.z; worldPosition = ray.GetPoint(t); }
         else { worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, mainCamera.nearClipPlane + Mathf.Abs(targetZ - mainCamera.transform.position.z))); worldPosition.z = targetZ; }
         return worldPosition; // 계산된 위치 반환
    }

    // GetRandomValidPositionInNest() // 필요 시 남겨둠
}