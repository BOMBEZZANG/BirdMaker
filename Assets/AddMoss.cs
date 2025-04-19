using UnityEngine;

/// <summary>
/// 수집 가능한 이끼 자원 노드 스크립트.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MossNode : MonoBehaviour
{
    [Tooltip("획득할 이끼의 개수입니다.")]
    [SerializeField] private int mossToGive = 1;

    /// <summary>
    /// 플레이어가 이 노드를 수집할 때 호출됩니다.
    /// </summary>
public void Collect()
{
    Debug.Log($"'{this.gameObject.name}' 수집됨. 이끼 +{mossToGive}");

    // *** 로그 추가: InventoryManager 접근 시도 직전 상태 확인 ***
    Debug.Log($"[MossNode] Collect 호출됨. InventoryManager.Instance is null = {InventoryManager.Instance == null}");

    if (InventoryManager.Instance != null)
    {
        InventoryManager.Instance.AddMoss(mossToGive);
    }
    else
    {
        Debug.LogError("InventoryManager 인스턴스를 찾을 수 없습니다! 아이템이 추가되지 않았습니다.");
    }
    Destroy(gameObject);
}

    // Gizmo (선택 사항)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta; // 이끼는 마젠타색으로 표시 (예시)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) { Gizmos.DrawWireSphere(col.bounds.center, 0.3f); }
        else { Gizmos.DrawSphere(transform.position, 0.2f); }
    }
}