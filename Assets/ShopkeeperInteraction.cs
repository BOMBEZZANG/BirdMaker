using UnityEngine;
using UnityEngine.EventSystems; // IPointerClickHandler 사용

/// <summary>
/// 상점 주인 NPC 오브젝트에 부착됩니다.
/// 클릭 시 상점 UI를 엽니다.
/// </summary>
[RequireComponent(typeof(Collider2D))] // 클릭 감지를 위해 Collider2D 필요
public class ShopkeeperInteraction : MonoBehaviour, IPointerClickHandler // 클릭 인터페이스 구현
{
    private ShopUI shopUIInstance; // 상점 UI 참조 캐싱

    [System.Obsolete]
    void Start()
    {
        // 씬에서 ShopUI 인스턴스를 찾아 캐싱 (씬에 하나만 있다고 가정)
        shopUIInstance = FindObjectOfType<ShopUI>(true); // 비활성화된 오브젝트도 찾기
        if (shopUIInstance == null)
        {
            Debug.LogError("ShopUI 인스턴스를 씬에서 찾을 수 없습니다!", this);
        }
    }

    /// <summary>
    /// 이 오브젝트(콜라이더 영역)가 클릭되었을 때 호출됩니다.
    /// </summary>
    [System.Obsolete]
    public void OnPointerClick(PointerEventData eventData)
    {
        // 왼쪽 클릭일 때만
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("상점 주인 클릭됨 -> 상점 UI 열기 시도.");
            // 캐싱된 참조 또는 실시간으로 찾아서 Open() 호출
             ShopUI shopToOpen = shopUIInstance ?? FindObjectOfType<ShopUI>(true);

            if (shopToOpen != null)
            {
                shopToOpen.Open(); // 상점 UI 열기
            }
            else
            {
                Debug.LogError("ShopUI를 열 수 없습니다! 씬에 ShopUI 인스턴스가 없습니다.", this);
            }
        }
    }
}