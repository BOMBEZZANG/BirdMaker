using UnityEngine;

/// <summary>
/// 게임 내 아이템의 기본 정보를 담는 ScriptableObject 정의
/// </summary>
[CreateAssetMenu(fileName = "New ItemData", menuName = "Game Data/Item Data")] // 에셋 생성 메뉴 추가
public class ItemData : ScriptableObject // MonoBehaviour 대신 ScriptableObject 상속
{
    [Header("Item Info")]
    public string itemName = "New Item";
    public Sprite itemIcon;
    [TextArea] public string description = "Item Description";

    [Header("Pricing")]
    public int buyPrice = 10; // 플레이어가 상점에서 구매할 때 가격 (못 사는 아이템은 0 또는 높은 값)
    public int sellPrice = 1;  // 플레이어가 상점에 판매할 때 가격 (못 파는 아이템은 0)

    [Header("Type")]
    public bool isStackable = true; // 여러 개 겹쳐질 수 있는지 여부 (나뭇가지 O, 온도계 X)
    public bool isTool = false;     // 도구 아이템인지 여부 (온도계, 습도계 등)
    // 필요에 따라 다른 타입 추가 가능 (예: enum ItemType { Resource, Tool, Consumable })
}