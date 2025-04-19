using UnityEngine;

/// <summary>
/// 수집 가능한 깃털 자원 노드 스크립트.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FeatherNode : MonoBehaviour
{
    [Tooltip("획득할 깃털의 개수입니다.")]
    [SerializeField] private int feathersToGive = 1;

    /// <summary>
    /// 플레이어가 이 노드를 수집할 때 호출됩니다.
    /// </summary>
    public void Collect()
    {
        Debug.Log($"'{this.gameObject.name}' 수집됨. 깃털 +{feathersToGive}");

        // 인벤토리 매니저를 통해 인벤토리에 깃털 추가
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddFeathers(feathersToGive);
        }
        else
        {
            Debug.LogError("InventoryManager 인스턴스를 찾을 수 없습니다! 아이템이 추가되지 않았습니다.");
        }

        // 수집 후 오브젝트 파괴
        Destroy(gameObject);
    }

    // Gizmo (선택 사항)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan; // 깃털은 하늘색으로 표시 (예시)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) { Gizmos.DrawWireSphere(col.bounds.center, 0.3f); }
        else { Gizmos.DrawSphere(transform.position, 0.2f); }
    }
}