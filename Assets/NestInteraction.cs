using UnityEngine;
using UnityEngine.EventSystems; // IPointerClickHandler, PointerEventData 사용
using System.Collections.Generic;
using System.Collections; // Coroutine 사용
using System.Linq; // List 변환 사용

/// <summary>
/// 둥지 오브젝트에 부착되어 깃털 추가/제거 상호작용 및 시각 효과를 관리합니다.
/// 둥지 클릭 시 클릭 위치에 깃털을 추가합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NestInteraction : MonoBehaviour, IPointerClickHandler // 둥지 자체 클릭 감지
{
    [Header("Feather Visuals")]
    [Tooltip("둥지에 배치될 깃털 시각 오브젝트의 프리팹")]
    [SerializeField] private GameObject featherVisualPrefab;
    [Tooltip("깃털이 배치될 영역을 정의하는 콜라이더 (이 오브젝트의 콜라이더 또는 다른 콜라이더)")]
    [SerializeField] private Collider2D nestAreaCollider;

    [Header("Warmth Settings")]
    [Tooltip("깃털 1개당 알에게 전달/제거할 온기량")]
    [SerializeField] private float warmthPerFeather = 5f;

    [Header("UI Elements")]
    [Tooltip("깃털 제거 버튼 프리팹")]
    [SerializeField] private GameObject removeButtonPrefab;
    [Tooltip("UI 요소(제거 버튼 등)가 배치될 부모 Canvas")]
    [SerializeField] private Canvas parentCanvas;

    // 내부 관리용 변수
    private EggController eggController;
    private List<GameObject> activeFeatherVisuals = new List<GameObject>();
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
        if (eggController == null) Debug.LogError("EggController Missing!", this);
        if (InventoryManager.Instance == null) Debug.LogError("InventoryManager Instance Missing!", this);
        if (DataManager.Instance == null) Debug.LogError("DataManager Instance Missing!", this);

        StartCoroutine(InitializeFeathersAfterOneFrame());
    }

    /// <summary>
    /// 저장된 데이터 기준으로 초기 깃털 위치 복원
    /// </summary>
    private IEnumerator InitializeFeathersAfterOneFrame()
    {
        yield return null; // 데이터 로드 기다림
        if (DataManager.Instance != null && DataManager.Instance.CurrentGameData != null)
        {
            List<Vector2> loadedPositions = DataManager.Instance.CurrentGameData.featherPositions;
            // Debug.Log($"초기 둥지 깃털 로드. 개수: {loadedPositions.Count}. 비주얼 생성 시작.");
            ClearAllFeatherVisuals();
            foreach (Vector2 pos in loadedPositions)
            {
                SpawnFeatherVisualAt(pos, false); // 저장된 위치에 생성
            }
        }
        else { /* ... 데이터 준비 안됨 로그 ... */ }
    }

    /// <summary> 둥지 클릭 시 (깃털 추가) </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 다른 깃털 상호작용 중이면 무시
        if (NestFeatherVisual.IsAnyDraggingOrInteracting()) return;

        // 왼쪽 클릭일 때만 깃털 추가 시도
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 필수 매니저 확인
            if (InventoryManager.Instance == null || DataManager.Instance == null || eggController == null || mainCamera == null)
            {
                 Debug.LogError("필수 매니저 또는 카메라 참조 없음 - OnPointerClick");
                 return;
            }

            // 인벤토리에 깃털이 있는지 확인
            if (InventoryManager.Instance.featherCount > 0)
            {
                // 클릭된 화면 좌표를 월드 좌표로 변환
                // Z값은 둥지보다 약간 앞에 오도록 설정
                float targetZ = this.transform.position.z - 0.1f;
                Vector3 clickWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, mainCamera.WorldToScreenPoint(transform.position).z));
                clickWorldPosition.z = targetZ;

                // 클릭된 위치가 유효한 둥지 영역 내부인지 확인
                if (IsPositionInNestArea(clickWorldPosition))
                {
                    // 인벤토리에서 깃털 사용 시도
                    if (InventoryManager.Instance.UseFeather())
                    {
                        // 클릭된 위치에 깃털 생성
                        SpawnFeatherVisualAt(clickWorldPosition, true);
                        // 온기 추가
                        eggController.AddWarmth(warmthPerFeather);
                        // 변경된 깃털 위치 리스트 저장 요청
                        NotifyFeatherPositionsChanged();
                        // Debug.Log($"깃털 추가됨 at {clickWorldPosition}");
                    }
                    // UseFeather 실패 로그는 InventoryManager에서 처리
                }
                else
                {
                    Debug.Log("클릭한 위치가 둥지 영역 내부가 아닙니다.");
                    // TODO: 사용자에게 피드백 (예: 클릭 효과 없음)
                }
            }
            else
            {
                Debug.Log("플레이어가 가진 깃털이 없습니다.");
                // TODO: 사용자에게 피드백
            }
        }
    }

    // TryAddFeather 함수는 이제 OnPointerClick 내부 로직으로 통합되었으므로 제거됨

    /// <summary> 개별 깃털(NestFeatherVisual) 제거 요청 처리 </summary>
    public void RequestRemoveFeather(GameObject featherToRemove)
    {
        if (InventoryManager.Instance == null || DataManager.Instance == null || eggController == null || featherToRemove == null) return;

        if (activeFeatherVisuals.Remove(featherToRemove)) // 리스트에서 제거 성공 시 데이터 처리
        {
             // 데이터 상 깃털 개수는 저장된 featherPositions.Count로 암묵적으로 관리됨
             // 따라서 DataManager의 깃털 개수 직접 감소는 불필요

             InventoryManager.Instance.AddFeathers(1); // 인벤토리 복원
             eggController.RemoveWarmth(warmthPerFeather); // 온기 감소
             Destroy(featherToRemove); // 오브젝트 파괴
             NotifyFeatherPositionsChanged(); // 위치 변경 알림 -> 저장
             // Debug.Log($"깃털 비주얼 [{featherToRemove.name}] 제거 완료.");
        }
         else { Debug.LogWarning($"제거 요청된 깃털 [{featherToRemove.name}]이(가) activeFeatherVisuals 리스트에 없습니다."); }
    }


    // *** 수정: SpawnFeatherVisualAt 파라미터 Vector3로 변경 ***
    /// <summary>
    /// 지정된 월드 좌표에 깃털 프리팹 인스턴스를 생성하고 배치합니다.
    /// </summary>
    /// <param name="worldPosition">깃털을 생성할 월드 좌표</param>
    /// <param name="logCreation">생성 로그 출력 여부</param>
    private void SpawnFeatherVisualAt(Vector3 worldPosition, bool logCreation = true)
    {
        if (featherVisualPrefab == null || parentCanvas == null) { /* ... 참조 없음 로그 ... */ return; }

        // Z 좌표는 함수 인자로 받은 worldPosition의 것을 사용하거나, 여기서 재조정 가능
        // worldPosition.z = this.transform.position.z - 0.1f; // 필요 시 Z 재조정

        GameObject newFeather = Instantiate(featherVisualPrefab, worldPosition, Quaternion.identity);

        NestFeatherVisual visualScript = newFeather.GetComponent<NestFeatherVisual>();
        if (visualScript != null)
        {
            visualScript.nestInteractionManager = this;
            visualScript.removeButtonPrefab = this.removeButtonPrefab;
            visualScript.parentCanvasRef = this.parentCanvas;
        }
         else { /* 스크립트 없음 경고 */ }

        activeFeatherVisuals.Add(newFeather);
        // if(logCreation) Debug.Log($"깃털 비주얼 생성됨: {newFeather.name} at {worldPosition}");
    }

    /// <summary> 지정된 위치가 둥지 영역 내에 있는지 확인 </summary>
    public bool IsPositionInNestArea(Vector3 worldPosition)
    {
        if (nestAreaCollider == null) { Debug.LogWarning("IsPositionInNestArea: Nest Area Collider is not assigned!"); return false; }
        return nestAreaCollider.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y));
    }

    /// <summary> 현재 깃털 위치 리스트를 DataManager에 업데이트 요청 </summary>
    public void NotifyFeatherPositionsChanged()
    {
        if (DataManager.Instance != null)
        {
            // 현재 activeFeatherVisuals 리스트 오브젝트들의 위치(Vector2)만 추출
            // 오브젝트가 파괴된 경우를 대비해 null 체크 후 위치 추출
            List<Vector2> currentPositions = activeFeatherVisuals
                                                .Where(f => f != null) // 파괴되지 않은 것만 필터링
                                                .Select(f => (Vector2)f.transform.position) // Vector3를 Vector2로 변환
                                                .ToList();
            DataManager.Instance.UpdateFeatherPositions(currentPositions);
            // Debug.Log($"Feather positions updated in DataManager. Count: {currentPositions.Count}");
        }
    }

    // GetRandomValidPositionInNest 함수는 이제 새 깃털 추가 시 사용하지 않으므로,
    // 필요 없다면 삭제하거나, 다른 용도(초기 배치?)가 있다면 남겨둘 수 있습니다.
    // 여기서는 남겨두겠습니다.
    /// <summary> 둥지 영역 내부의 유효한 랜덤 위치 찾기 </summary>
    private Vector3 GetRandomValidPositionInNest()
    {
        if (nestAreaCollider == null) return Vector3.positiveInfinity;
        Bounds bounds = nestAreaCollider.bounds;
        int maxAttempts = 100;
        for (int i = 0; i < maxAttempts; i++) {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 randomPoint = new Vector2(randomX, randomY);
            if (nestAreaCollider.OverlapPoint(randomPoint)) {
                float zPos = this.transform.position.z - 0.1f;
                return new Vector3(randomX, randomY, zPos);
            }
        }
        Debug.LogWarning($"둥지 영역 내부 랜덤 위치 찾기 실패 (시도: {maxAttempts}회).");
        return Vector3.positiveInfinity;
    }


    /// <summary> 현재 활성화된 모든 깃털 비주얼 제거 </summary>
    private void ClearAllFeatherVisuals() {
         for(int i = activeFeatherVisuals.Count - 1; i >= 0; i--) { if(activeFeatherVisuals[i] != null) Destroy(activeFeatherVisuals[i]); }
         activeFeatherVisuals.Clear();
     }
}