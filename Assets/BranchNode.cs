using UnityEngine;

/// <summary>
/// 수집 가능한 나뭇가지 자원 노드 스크립트.
/// 플레이어와 상호작용 시 자신을 파괴하고 인벤토리에 아이템을 추가합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))] // 충돌 감지를 위해 Collider2D 필요
public class BranchNode : MonoBehaviour
{
    [Tooltip("획득할 나뭇가지의 개수입니다.")]
    [SerializeField] private int branchesToGive = 1;

    /// <summary>
    /// 플레이어가 이 노드를 수집할 때 호출됩니다. (PlayerInteraction 스크립트에서 호출)
    /// </summary>
    public void Collect()
    {
        Debug.Log($"'{this.gameObject.name}' 수집됨. 나뭇가지 +{branchesToGive}");

        // InventoryManager를 통해 인벤토리에 나뭇가지 추가
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddBranches(branchesToGive);
        }
        else
        {
            Debug.LogError("InventoryManager 인스턴스를 찾을 수 없습니다! 아이템이 추가되지 않았습니다.");
            // 이 경우 게임 시작 씬에 InventoryManager 오브젝트가 있는지,
            // 스크립트가 제대로 부착되어 있는지 확인해야 합니다.
        }

        // 수집된 후에는 오브젝트를 파괴하여 사라지게 함
        Destroy(gameObject);
    }

    // (선택 사항) Scene 뷰에서 잘 보이도록 Gizmo 그리기
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // 콜라이더가 있다면 그 범위를 표시, 없다면 기본 아이콘 위치에 점 표시
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
             Gizmos.DrawWireSphere(col.bounds.center, 0.3f);
        } else {
            Gizmos.DrawSphere(transform.position, 0.2f);
        }
    }
}