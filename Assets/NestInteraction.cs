using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class NestInteraction : MonoBehaviour, IPointerClickHandler
{
    [Header("Item Prefabs")]
    [SerializeField] private GameObject featherVisualPrefab;
    [SerializeField] private GameObject mossVisualPrefab; // *** 이끼 프리팹 추가 ***

    [Header("Area Collider")]
    [SerializeField] private Collider2D nestAreaCollider;

    [Header("Item Effects")]
    [SerializeField] private float warmthPerFeather = 5f;
    [SerializeField] private float humidityPerMoss = 5f; // *** 이끼 효과 추가 ***

    [Header("UI Elements")]
    [SerializeField] private GameObject removeButtonPrefab;
    [SerializeField] private Canvas parentCanvas;
    [SerializeField] private GameObject editModeDimPanel;
    [SerializeField] private GameObject editModeToolbarPanel; // 툴바 Panel
    [SerializeField] private EditToolbarController toolbarController; // *** 툴바 컨트롤러 참조 추가 ***

    // 내부 관리용 변수
    private EggController eggController;
    private List<GameObject> activeFeatherVisuals = new List<GameObject>();
    private List<GameObject> activeMossVisuals = new List<GameObject>(); // *** 이끼 비주얼 리스트 추가 ***
    public bool IsEditing { get; private set; } = false;
    private Camera mainCamera;
    private bool isAnyVisualInteracting = false; // *** 상호작용 상태 변수 (static 제거) ***

    [System.Obsolete]
    void Start()
    {
        mainCamera = Camera.main;
        eggController = FindObjectOfType<EggController>();
        // 필수 참조 확인
        if (featherVisualPrefab == null) Debug.LogError("Feather Visual Prefab Missing!", this);
        if (mossVisualPrefab == null) Debug.LogError("Moss Visual Prefab Missing!", this); // 이끼 프리팹 확인
        if (nestAreaCollider == null) Debug.LogError("Nest Area Collider Missing!", this);
        if (removeButtonPrefab == null) Debug.LogError("Remove Button Prefab Missing!", this);
        if (parentCanvas == null) Debug.LogError("Parent Canvas Missing!", this);
        if (editModeDimPanel == null) Debug.LogError("Edit Mode Dim Panel Missing!", this);
        if (editModeToolbarPanel == null) Debug.LogError("Edit Mode Toolbar Panel Missing!", this);
        if (toolbarController == null) Debug.LogError("Toolbar Controller Missing!", this); // 툴바 컨트롤러 확인
        if (eggController == null) Debug.LogError("EggController Missing!", this);
        if (InventoryManager.Instance == null) Debug.LogError("InventoryManager Instance Missing!", this);
        if (DataManager.Instance == null) Debug.LogError("DataManager Instance Missing!", this);

        SetEditMode(false);
        StartCoroutine(InitializeVisualsAfterOneFrame());
    }

    private IEnumerator InitializeVisualsAfterOneFrame()
    {
        yield return null;
        if (DataManager.Instance?.CurrentGameData != null)
        {
            GameData data = DataManager.Instance.CurrentGameData;
            // 기존 비주얼 정리
            ClearAllVisuals();
            // 깃털 복원
            // Debug.Log($"초기 둥지 깃털 로드. 개수: {data.featherPositions.Count}.");
            foreach (Vector2 pos in data.featherPositions) { SpawnFeatherVisualAt(new Vector3(pos.x, pos.y, GetItemZPos()), false); }
            // *** 이끼 복원 추가 ***
            // Debug.Log($"초기 둥지 이끼 로드. 개수: {data.mossPositions.Count}.");
            foreach (Vector2 pos in data.mossPositions) { SpawnMossVisualAt(new Vector3(pos.x, pos.y, GetItemZPos()), false); }
        }
        else { /* 데이터 준비 안됨 로그 */ }
    }

    /// <summary> 둥지 클릭 시 (편집 모드에서 아이템 배치) </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 편집 모드가 아니거나, 다른 비주얼 상호작용 중이면 무시
        if (!IsEditing || IsAnyVisualInteracting()) return;
        // 왼쪽 클릭일 때만 배치 시도
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 툴바에서 선택된 아이템 확인
            EditToolbarController.PlacementType selection = toolbarController.CurrentSelection;
            if (selection == EditToolbarController.PlacementType.None)
            {
                // Debug.Log("배치할 아이템이 선택되지 않았습니다."); // 로그 레벨 조절
                return; // 선택된 아이템 없으면 종료
            }

            // 클릭 위치 계산
            Vector3 clickWorldPosition = GetWorldPosFromScreenClick(eventData);
            if (clickWorldPosition == Vector3.positiveInfinity) return; // 위치 계산 실패

            // 클릭 위치 유효성 검사
            if (IsPositionInNestArea(clickWorldPosition))
            {
                // 선택된 아이템 종류에 따라 배치 시도
                if (selection == EditToolbarController.PlacementType.Feather)
                {
                    TryAddItemAt(clickWorldPosition, EditToolbarController.PlacementType.Feather);
                }
                else if (selection == EditToolbarController.PlacementType.Moss)
                {
                     TryAddItemAt(clickWorldPosition, EditToolbarController.PlacementType.Moss);
                }
            }
            else { /* 둥지 영역 아님 로그 */ }
        }
    }

    /// <summary> 지정된 위치에 선택된 아이템 추가 시도 (깃털/이끼 공통 로직) </summary>
    private void TryAddItemAt(Vector3 spawnPosition, EditToolbarController.PlacementType itemType)
    {
        if (InventoryManager.Instance == null || DataManager.Instance == null || eggController == null) return;

        bool itemUsed = false;
        // 아이템 종류에 따라 인벤토리 확인 및 사용
        if (itemType == EditToolbarController.PlacementType.Feather && InventoryManager.Instance.featherCount > 0)
        {
            itemUsed = InventoryManager.Instance.UseFeather();
        }
        else if (itemType == EditToolbarController.PlacementType.Moss && InventoryManager.Instance.mossCount > 0)
        {
             itemUsed = InventoryManager.Instance.UseMoss();
        }
        else
        {
             Debug.Log($"인벤토리에 {itemType}이(가) 없습니다.");
             return; // 해당 아이템 없으면 종료
        }

        // 인벤토리 사용 성공 시
        if (itemUsed)
        {
            // 아이템 종류에 따라 생성 및 효과 적용
            if (itemType == EditToolbarController.PlacementType.Feather)
            {
                 SpawnFeatherVisualAt(spawnPosition, true);
                 eggController.AddWarmth(warmthPerFeather);
                 NotifyFeatherPositionsChanged(); // 저장 요청
            }
            else if (itemType == EditToolbarController.PlacementType.Moss)
            {
                 SpawnMossVisualAt(spawnPosition, true);
                 eggController.AddHumidity(humidityPerMoss); // 습도 증가
                 NotifyMossPositionsChanged(); // 저장 요청
            }
        }
        // UseItem 실패 로그는 InventoryManager에서 처리
    }


    // --- 아이템 제거 요청 처리 ---
    /// <summary> 개별 깃털 제거 요청 처리 </summary>
    public void RequestRemoveFeather(GameObject featherToRemove)
    {
        if (!IsEditing) return; // 편집 모드 체크
        if (InventoryManager.Instance == null || DataManager.Instance == null || eggController == null || featherToRemove == null) return;
        if (activeFeatherVisuals.Remove(featherToRemove))
        {
             InventoryManager.Instance.AddFeathers(1);
             eggController.RemoveWarmth(warmthPerFeather);
             Destroy(featherToRemove);
             NotifyFeatherPositionsChanged(); // 위치 변경 저장
        }
    }
    /// <summary> 개별 이끼 제거 요청 처리 </summary>
    public void RequestRemoveMoss(GameObject mossToRemove) // *** 이끼 제거 함수 추가 ***
    {
        if (!IsEditing) return; // 편집 모드 체크
        if (InventoryManager.Instance == null || DataManager.Instance == null || eggController == null || mossToRemove == null) return;
        if (activeMossVisuals.Remove(mossToRemove)) // 이끼 리스트에서 제거
        {
             InventoryManager.Instance.AddMoss(1); // 이끼 인벤토리 복원
             eggController.RemoveHumidity(humidityPerMoss); // 습도 감소
             Destroy(mossToRemove);
             NotifyMossPositionsChanged(); // 위치 변경 저장
        }
    }

    // --- 비주얼 생성 ---
    private void SpawnFeatherVisualAt(Vector3 worldPosition, bool logCreation = true) { /* ... 이전 코드와 유사 (프리팹, 리스트 변수명 주의) ... */
        if (featherVisualPrefab == null || parentCanvas == null) return;
        GameObject newFeather = Instantiate(featherVisualPrefab, worldPosition, Quaternion.identity);
        NestFeatherVisual visualScript = newFeather.GetComponent<NestFeatherVisual>();
        if (visualScript != null) { /* ... 참조 설정 ... */ visualScript.nestInteractionManager = this; visualScript.removeButtonPrefab = this.removeButtonPrefab; visualScript.parentCanvasRef = this.parentCanvas; }
        activeFeatherVisuals.Add(newFeather);
    }
     private void SpawnMossVisualAt(Vector3 worldPosition, bool logCreation = true) { // *** 이끼 생성 함수 추가 ***
        if (mossVisualPrefab == null || parentCanvas == null) return;
        GameObject newMoss = Instantiate(mossVisualPrefab, worldPosition, Quaternion.identity);
        NestMossVisual visualScript = newMoss.GetComponent<NestMossVisual>(); // NestMossVisual 스크립트 사용
        if (visualScript != null) { /* ... 참조 설정 ... */ visualScript.nestInteractionManager = this; visualScript.removeButtonPrefab = this.removeButtonPrefab; visualScript.parentCanvasRef = this.parentCanvas; }
        activeMossVisuals.Add(newMoss); // 이끼 리스트에 추가
    }


    // --- 위치/상태 업데이트 알림 ---
    public void NotifyFeatherPositionsChanged() { /* ... 이전 코드와 동일 ... */
         if (DataManager.Instance != null) { List<Vector2> pos = activeFeatherVisuals.Where(f => f != null).Select(f => (Vector2)f.transform.position).ToList(); DataManager.Instance.UpdateFeatherPositions(pos); } }
    public void NotifyMossPositionsChanged() { // *** 이끼 위치 알림 함수 추가 ***
         if (DataManager.Instance != null) { List<Vector2> pos = activeMossVisuals.Where(f => f != null).Select(f => (Vector2)f.transform.position).ToList(); DataManager.Instance.UpdateMossPositions(pos); } }


    // --- 편집 모드 및 상호작용 상태 관리 ---
    public bool IsAnyVisualInteracting() { return isAnyVisualInteracting; } // Getter
    public void SetInteractionActive(bool active) { isAnyVisualInteracting = active; } // Setter

    public void SetEditMode(bool isOn) { /* ... 이전 코드와 동일 (CancelAllVisualInteractions 호출 확인) ... */
        if (IsEditing == isOn) return; IsEditing = isOn;
        // Debug.Log($"둥지 편집 모드: {(IsEditing ? "활성화됨" : "비활성화됨")}");
        UpdateVisualsForEditMode(IsEditing);
        if (!IsEditing) { CancelAllVisualInteractions(); }
     }
    private void UpdateVisualsForEditMode(bool isEditing) { /* ... 이전 코드와 동일 (Dim Panel, Toolbar Panel 활성화/비활성화) ... */
        if (editModeDimPanel != null) { editModeDimPanel.SetActive(isEditing); } else { /*...*/ }
        if (editModeToolbarPanel != null) { editModeToolbarPanel.SetActive(isEditing); } else { /*...*/ }
     }
    private void CancelAllVisualInteractions() { /* ... 이전 코드와 동일 (activeFeatherVisuals + activeMossVisuals 순회) ... */
        // Debug.Log("모든 비주얼 상호작용 취소 시도...");
        foreach(GameObject visualGO in activeFeatherVisuals) { visualGO?.GetComponent<NestFeatherVisual>()?.CancelInteraction(); }
        foreach(GameObject visualGO in activeMossVisuals) { visualGO?.GetComponent<NestMossVisual>()?.CancelInteraction(); } // 이끼도 취소
        SetInteractionActive(false); // 중앙 플래그 리셋
        toolbarController?.Deselect(); // 툴바 선택 해제
     }

    // --- Helper 함수들 ---
    public bool IsPositionInNestArea(Vector3 worldPosition) { /* ... 이전과 동일 ... */
        if (nestAreaCollider == null) return false;
        return nestAreaCollider.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y));
     }
     private float GetItemZPos() { return this.transform.position.z - 0.1f; } // Z 좌표 계산 통일
     private void ClearAllVisuals() { // 이름 변경 및 이끼 포함
          for(int i = activeFeatherVisuals.Count - 1; i >= 0; i--) { if(activeFeatherVisuals[i] != null) Destroy(activeFeatherVisuals[i]); }
          activeFeatherVisuals.Clear();
          for(int i = activeMossVisuals.Count - 1; i >= 0; i--) { if(activeMossVisuals[i] != null) Destroy(activeMossVisuals[i]); } // 이끼도 클리어
          activeMossVisuals.Clear();
      }
     private Vector3 GetWorldPosFromScreenClick(PointerEventData eventData) // 헬퍼 함수로 분리
     {
         if (mainCamera == null) { Debug.LogError("카메라 참조 없음!"); return Vector3.positiveInfinity; }
         float targetZ = GetItemZPos();
         Vector3 worldPosition = Vector3.zero;
         Ray ray = mainCamera.ScreenPointToRay(eventData.position);
         if (Mathf.Abs(ray.direction.z) > 0.0001f) { float t = (targetZ - ray.origin.z) / ray.direction.z; worldPosition = ray.GetPoint(t); }
         else { worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, mainCamera.nearClipPlane + Mathf.Abs(targetZ - mainCamera.transform.position.z))); worldPosition.z = targetZ; }
         return worldPosition;
     }
}