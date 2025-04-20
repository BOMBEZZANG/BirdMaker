using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class NestInteraction : MonoBehaviour, IPointerClickHandler
{
    // ... (기존 Header 및 변수 선언: featherVisualPrefab, nestAreaCollider, warmthPerFeather) ...
    [Header("Feather Visuals")]
    [SerializeField] private GameObject featherVisualPrefab;
    [SerializeField] private Collider2D nestAreaCollider;

    [Header("Warmth Settings")]
    [SerializeField] private float warmthPerFeather = 5f;

    [Header("UI Elements")]
    [SerializeField] private GameObject removeButtonPrefab;
    [SerializeField] private Canvas parentCanvas;
    // *** 새로 추가: 편집 모드 시각 효과용 ***
    [Tooltip("편집 모드 시 활성화될 배경 어둡게 하는 Panel")]
    [SerializeField] private GameObject editModeDimPanel; // Inspector에서 연결 필요!

    // 내부 관리용 변수
    private EggController eggController;
    private List<GameObject> activeFeatherVisuals = new List<GameObject>();

    // *** 새로 추가: 편집 모드 상태 변수 ***
    /// <summary> 현재 둥지 편집 모드인지 여부 </summary>
    public bool IsEditing { get; private set; } = false; // 초기값은 false (보기 모드)

    [System.Obsolete]
    void Start()
    {
        eggController = FindObjectOfType<EggController>();
        // 필수 참조 확인
        // ... (기존 null 체크들) ...
        if (editModeDimPanel == null) Debug.LogError("Edit Mode Dim Panel이 연결되지 않았습니다!", this); // Dim Panel 체크 추가

        // 시작 시 편집 모드 비활성화 상태 및 시각 효과 적용
        SetEditMode(false); // 초기 상태 설정
        // editModeDimPanel?.SetActive(false); // SetEditMode 내부에서 처리

        StartCoroutine(InitializeFeathersAfterOneFrame());
    }

    private IEnumerator InitializeFeathersAfterOneFrame()
    {
        yield return null;
        if (DataManager.Instance != null && DataManager.Instance.CurrentGameData != null)
        {
            List<Vector2> loadedPositions = DataManager.Instance.CurrentGameData.featherPositions;
            ClearAllFeatherVisuals();
            foreach (Vector2 pos in loadedPositions)
            {
                SpawnFeatherVisualAt(pos, false);
            }
        }
        else { /* ... 데이터 준비 안됨 로그 ... */ }
    }

    /// <summary> 둥지 클릭 시 (깃털 추가 - 이제 편집 모드가 아닐 때만 작동하도록 수정 필요) </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 편집 모드이거나 다른 깃털 상호작용 중이면 둥지 클릭(깃털 추가) 무시
        if (IsEditing || NestFeatherVisual.IsAnyDraggingOrInteracting()) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
             TryAddFeather();
        }
    }

    /// <summary> 깃털 추가 시도 </summary>
    private void TryAddFeather()
    {
         if (InventoryManager.Instance == null || DataManager.Instance == null || eggController == null) return;
         if (InventoryManager.Instance.featherCount > 0)
         {
             if (InventoryManager.Instance.UseFeather())
             {
                 Vector3 spawnPos = GetRandomValidPositionInNest();
                 if (spawnPos != Vector3.positiveInfinity)
                 {
                     SpawnFeatherVisualAt(spawnPos, true);
                     eggController.AddWarmth(warmthPerFeather);
                     NotifyFeatherPositionsChanged();
                 } else { /*... 위치 찾기 실패 로그 ...*/ InventoryManager.Instance.AddFeathers(1); }
             }
         }
         else { /*... 깃털 없음 로그 ...*/ }
    }

    /// <summary> 개별 깃털 제거 요청 처리 </summary>
    public void RequestRemoveFeather(GameObject featherToRemove)
    {
        // 이 함수는 편집 모드 여부와 관계없이 호출될 수 있음 (NestFeatherVisual에서 모드 체크 후 호출 가정)
        if (InventoryManager.Instance == null || DataManager.Instance == null || eggController == null || featherToRemove == null) return;
        if (activeFeatherVisuals.Remove(featherToRemove))
        {
             InventoryManager.Instance.AddFeathers(1);
             eggController.RemoveWarmth(warmthPerFeather);
             Destroy(featherToRemove);
             NotifyFeatherPositionsChanged();
        }
         else { /* ... 리스트 없음 경고 ... */ }
    }

    private void SpawnFeatherVisualAt(Vector3 worldPosition, bool logCreation = true)
    {
        if (featherVisualPrefab == null || parentCanvas == null) return;

        GameObject newFeather = Instantiate(featherVisualPrefab, worldPosition, Quaternion.identity);
        NestFeatherVisual visualScript = newFeather.GetComponent<NestFeatherVisual>();
        if (visualScript != null)
        {
            visualScript.nestInteractionManager = this;
            visualScript.removeButtonPrefab = this.removeButtonPrefab;
            visualScript.parentCanvasRef = this.parentCanvas;
        }
        activeFeatherVisuals.Add(newFeather);
        // if(logCreation) Debug.Log($"깃털 비주얼 생성됨...");
    }

    public bool IsPositionInNestArea(Vector3 worldPosition) { /* ... 이전과 동일 ... */
        if (nestAreaCollider == null) return false;
        return nestAreaCollider.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y));
    }

    public void NotifyFeatherPositionsChanged() { /* ... 이전과 동일 ... */
        if (DataManager.Instance != null) {
            List<Vector2> currentPositions = activeFeatherVisuals.Where(f => f != null).Select(f => (Vector2)f.transform.position).ToList();
            DataManager.Instance.UpdateFeatherPositions(currentPositions);
        }
    }

    /// <summary> 둥지 영역 내부의 유효한 랜덤 위치 찾기 (깃털 추가 시 사용) </summary>
    private Vector3 GetRandomValidPositionInNest()
    {
        // Use the class field 'nestAreaCollider'
        if (nestAreaCollider == null)
        {
             Debug.LogError("GetRandomValidPositionInNest: Nest Area Collider is not assigned!");
             return Vector3.positiveInfinity; // Return failure indication
        }

        // Use the class field 'nestAreaCollider'
        Bounds bounds = nestAreaCollider.bounds;
        int maxAttempts = 100;
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 randomPoint = new Vector2(randomX, randomY);

            // Use the class field 'nestAreaCollider' here too!
            if (nestAreaCollider.OverlapPoint(randomPoint))
            {
                float zPos = this.transform.position.z - 0.1f;
                return new Vector3(randomX, randomY, zPos);
            }
        }
        Debug.LogWarning($"둥지 영역({nestAreaCollider.name}) 내부 랜덤 위치 찾기 실패 (시도: {maxAttempts}회).");
        return Vector3.positiveInfinity;
    }

    private void ClearAllFeatherVisuals() { /* ... 이전과 동일 ... */
         for(int i = activeFeatherVisuals.Count - 1; i >= 0; i--) { if(activeFeatherVisuals[i] != null) Destroy(activeFeatherVisuals[i]); }
         activeFeatherVisuals.Clear();
     }


    // *** 새로 추가: 편집 모드 설정 및 시각 효과 처리 함수 ***
    /// <summary>
    /// 둥지 편집 모드를 설정/해제합니다. UI 토글 버튼의 OnValueChanged 이벤트에 연결됩니다.
    /// </summary>
    /// <param name="isOn">true이면 편집 모드 활성화, false이면 비활성화</param>
    public void SetEditMode(bool isOn)
    {
        IsEditing = isOn;
        Debug.Log($"둥지 편집 모드: {(IsEditing ? "활성화됨" : "비활성화됨")}");

        // 시각 효과 업데이트
        UpdateVisualsForEditMode(IsEditing);

        // TODO: 편집 모드 변경 시 필요한 추가 로직 (예: 툴바 표시/숨기기, 깃털 상호작용 활성/비활성)
        // 이 부분은 다음 단계에서 구현합니다.
    }

    /// <summary>
    /// 편집 모드 상태에 따라 시각적 효과(예: 배경 어둡게 하기)를 업데이트합니다.
    /// </summary>
    /// <param name="isEditing">현재 편집 모드 상태</param>
private void UpdateVisualsForEditMode(bool isEditing)
  {
      if (editModeDimPanel != null)
      {
          // *** ADD LOGS ***
          Debug.Log($"UpdateVisualsForEditMode called. Setting Dim Panel '{editModeDimPanel.name}' Active: {isEditing}");
          editModeDimPanel.SetActive(isEditing);
          // Optional: Check if it actually became active/inactive
          // Debug.Log($"Dim Panel Is Now Active: {editModeDimPanel.activeSelf}");
      }
      else
      {
           // This log should not appear if the Start() check passed, but good for sanity
           Debug.LogError("UpdateVisualsForEditMode: editModeDimPanel reference is NULL!");
      }
  }
}