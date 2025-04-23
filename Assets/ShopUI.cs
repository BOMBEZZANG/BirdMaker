using UnityEngine;
using UnityEngine.UI; // Button 등 사용
using TMPro; // TextMeshPro 사용 시

/// <summary>
/// 상점 UI 패널의 표시 내용 업데이트 및 버튼 상호작용을 처리합니다.
/// Inspector에서 가격 오버라이드 및 ItemData, UI 요소 연결이 필요합니다.
/// </summary>
public class ShopUI : MonoBehaviour // ShopUIPanel 게임 오브젝트에 부착
{
    [Header("Data Assets (Assign ItemData Assets)")]
    [SerializeField] private ItemData branchData;
    [SerializeField] private ItemData featherData;
    [SerializeField] private ItemData mossData;
    [SerializeField] private ItemData thermometerData;
    [SerializeField] private ItemData hygrometerData;

    // --- 가격 오버라이드 설정 ---
    // (-1 또는 0 이하로 두면 ItemData의 기본값 사용)
    [Header("Price Overrides (Optional)")]
    [Tooltip("나뭇가지 판매 가격 오버라이드 (-1 이면 기본값 사용)")]
    [SerializeField] private int overrideSellPriceBranch = -1;
    [Tooltip("깃털 판매 가격 오버라이드 (-1 이면 기본값 사용)")]
    [SerializeField] private int overrideSellPriceFeather = -1;
    [Tooltip("이끼 판매 가격 오버라이드 (-1 이면 기본값 사용)")]
    [SerializeField] private int overrideSellPriceMoss = -1;
    [Tooltip("온도계 구매 가격 오버라이드 (-1 이면 기본값 사용)")]
    [SerializeField] private int overrideBuyPriceThermo = -1;
    [Tooltip("습도계 구매 가격 오버라이드 (-1 이면 기본값 사용)")]
    [SerializeField] private int overrideBuyPriceHygro = -1;


    [Header("UI References (Assign UI Elements)")]
    [SerializeField] private TextMeshProUGUI moneyText;
    // Sell Item References
    [SerializeField] private TextMeshProUGUI branchCountText;
    [SerializeField] private TextMeshProUGUI featherCountText;
    [SerializeField] private TextMeshProUGUI mossCountText;
    [SerializeField] private Button sellBranchButton;
    [SerializeField] private Button sellFeatherButton;
    [SerializeField] private Button sellMossButton;
    [SerializeField] private TextMeshProUGUI sellBranchPriceText; // 판매 가격 표시용
    [SerializeField] private TextMeshProUGUI sellFeatherPriceText;// 판매 가격 표시용
    [SerializeField] private TextMeshProUGUI sellMossPriceText;   // 판매 가격 표시용
    // Buy Item References
    [SerializeField] private Button buyThermometerButton;
    [SerializeField] private Button buyHygrometerButton;
    [SerializeField] private TextMeshProUGUI buyThermoPriceText;  // 구매 가격 표시용
    [SerializeField] private TextMeshProUGUI buyHygroPriceText;   // 구매 가격 표시용
    // Close Button
    [SerializeField] private Button closeButton;

    /// <summary> 이 UI 패널 활성화 시 </summary>
    void OnEnable()
    {
        // 이벤트 구독
        if (InventoryManager.Instance != null) { InventoryManager.Instance.OnInventoryUpdated += UpdateShopUI; }
        else { Debug.LogError("ShopUI 활성화 시 InventoryManager 인스턴스 없음!"); }
        UpdateShopUI(); // UI 즉시 업데이트
        // TODO: 게임 시간 정지 등
    }

    /// <summary> 이 UI 패널 비활성화 시 </summary>
    void OnDisable()
    {
        // 이벤트 구독 해제
        if (InventoryManager.Instance != null) { InventoryManager.Instance.OnInventoryUpdated -= UpdateShopUI; }
        // TODO: 게임 시간 재개 등
    }

    /// <summary> UI 요소 초기화 및 버튼 리스너 설정 </summary>
    void Start()
    {
        // null 참조 방지를 위해 Start에서 리스너 설정
        InitializeButtonListeners();
        // 초기 가격 텍스트 설정
        UpdatePriceTexts();
    }

    /// <summary> 버튼 클릭 리스너를 코드에서 설정 (Inspector 설정보다 안정적) </summary>
    private void InitializeButtonListeners()
    {
        closeButton?.onClick.AddListener(Close); // Close 함수 직접 호출
        // 판매 버튼 리스너
        sellBranchButton?.onClick.AddListener(() => SellItem(branchData));
        sellFeatherButton?.onClick.AddListener(() => SellItem(featherData));
        sellMossButton?.onClick.AddListener(() => SellItem(mossData));
        // 구매 버튼 리스너
        buyThermometerButton?.onClick.AddListener(() => BuyItem(thermometerData));
        buyHygrometerButton?.onClick.AddListener(() => BuyItem(hygrometerData));
    }

    /// <summary> 상점 UI의 가격 표시 텍스트 업데이트 </summary>
    private void UpdatePriceTexts()
    {
        // 판매 가격 표시
        if(sellBranchPriceText != null) sellBranchPriceText.text = $"Sell: {GetSellPrice(branchData)} G";
        if(sellFeatherPriceText != null) sellFeatherPriceText.text = $"Sell: {GetSellPrice(featherData)} G";
        if(sellMossPriceText != null) sellMossPriceText.text = $"Sell: {GetSellPrice(mossData)} G";
        // 구매 가격 표시
        if(buyThermoPriceText != null) buyThermoPriceText.text = $"Buy: {GetBuyPrice(thermometerData)} G";
        if(buyHygroPriceText != null) buyHygroPriceText.text = $"Buy: {GetBuyPrice(hygrometerData)} G";
    }

    /// <summary> 상점 UI의 모든 내용을 현재 게임 데이터 기준으로 업데이트 </summary>
    private void UpdateShopUI()
    {
        if (InventoryManager.Instance == null || DataManager.Instance?.CurrentGameData == null)
        {
             Debug.LogError("ShopUI 업데이트 실패: InventoryManager 또는 DataManager(Data) 없음!");
             return;
        }

        // 재화 표시
        if (moneyText != null) { moneyText.text = $"Money: {InventoryManager.Instance.PlayerMoney}"; }

        // 판매 아이템 개수 및 버튼 상태
        if (branchCountText != null) { branchCountText.text = $"Have: {InventoryManager.Instance.branchCount}"; }
        if (sellBranchButton != null) { sellBranchButton.interactable = (InventoryManager.Instance.branchCount > 0); }

        if (featherCountText != null) { featherCountText.text = $"Have: {InventoryManager.Instance.featherCount}"; }
        if (sellFeatherButton != null) { sellFeatherButton.interactable = (InventoryManager.Instance.featherCount > 0); }

        if (mossCountText != null) { mossCountText.text = $"Have: {InventoryManager.Instance.mossCount}"; }
        if (sellMossButton != null) { sellMossButton.interactable = (InventoryManager.Instance.mossCount > 0); }

        // 구매 아이템 버튼 상태 (보유 여부 및 재화 확인)
        bool canAffordThermo = InventoryManager.Instance.PlayerMoney >= GetBuyPrice(thermometerData);
        bool hasThermo = InventoryManager.Instance.HasThermometer;
        if (buyThermometerButton != null) { buyThermometerButton.interactable = !hasThermo && canAffordThermo; }
        // 버튼 텍스트 변경 (선택 사항)
        // TextMeshProUGUI thermoBtnText = buyThermometerButton?.GetComponentInChildren<TextMeshProUGUI>();
        // if(thermoBtnText != null) thermoBtnText.text = hasThermo ? "Owned" : (canAffordThermo ? "Buy" : "No Money");


        bool canAffordHygro = InventoryManager.Instance.PlayerMoney >= GetBuyPrice(hygrometerData);
        bool hasHygro = InventoryManager.Instance.HasHygrometer;
        if (buyHygrometerButton != null) { buyHygrometerButton.interactable = !hasHygro && canAffordHygro; }
         // TextMeshProUGUI hygroBtnText = buyHygrometerButton?.GetComponentInChildren<TextMeshProUGUI>();
         // if(hygroBtnText != null) hygroBtnText.text = hasHygro ? "Owned" : (canAffordHygro ? "Buy" : "No Money");

        // 가격 텍스트 업데이트 (혹시 가격이 동적으로 변할 경우 대비)
        UpdatePriceTexts();
    }

    /// <summary> 지정된 아이템 1개를 판매합니다. </summary>
    private void SellItem(ItemData itemToSell)
    {
        if (itemToSell == null || InventoryManager.Instance == null) return;

        bool sold = false;
        int sellPrice = GetSellPrice(itemToSell); // 오버라이드 가격 고려

        // 아이템 타입에 따라 인벤토리에서 1개 제거 시도
        if (itemToSell == branchData) sold = InventoryManager.Instance.UseBranches(1);
        else if (itemToSell == featherData) sold = InventoryManager.Instance.UseFeather();
        else if (itemToSell == mossData) sold = InventoryManager.Instance.UseMoss();

        // 판매 성공 시 돈 추가
        if (sold)
        {
            InventoryManager.Instance.AddMoney(sellPrice);
            Debug.Log($"{itemToSell.itemName} 1개를 판매하여 {sellPrice} G 획득!");
        }
        // 실패 로그는 각 Use 함수에서 출력됨
        // UI는 OnInventoryUpdated 이벤트에 의해 자동으로 갱신됨
    }

    /// <summary> 지정된 도구 아이템을 구매합니다. (디버깅 로그 추가) </summary>
    private void BuyItem(ItemData itemToBuy)
    {
         if (itemToBuy == null || InventoryManager.Instance == null) {
             Debug.LogError("BuyItem Error: ItemData 또는 InventoryManager 없음!");
             return;
         }
         // 도구가 아니면 구매 불가 (ItemData 설정 확인)
         if (!itemToBuy.isTool) {
              Debug.LogWarning($"BuyItem Error: {itemToBuy.itemName}은(는) 도구가 아니라 구매할 수 없습니다.");
              return;
         }

         Debug.Log($"BuyItem 시도: {itemToBuy.itemName}"); // 로그 추가

         // 이미 보유했는지 확인
         bool alreadyOwned = false;
         if(itemToBuy == thermometerData && InventoryManager.Instance.HasThermometer) alreadyOwned = true;
         else if(itemToBuy == hygrometerData && InventoryManager.Instance.HasHygrometer) alreadyOwned = true;

         if(alreadyOwned) { Debug.Log($"{itemToBuy.itemName}은(는) 이미 보유하고 있습니다."); return; }

         // 구매 가격 확인 (오버라이드 고려)
         int buyPrice = GetBuyPrice(itemToBuy);
         Debug.Log($" - 가격: {buyPrice}, 현재 보유 재화: {InventoryManager.Instance.PlayerMoney}"); // 로그 추가

         // 돈 사용 시도
         if (InventoryManager.Instance.SpendMoney(buyPrice)) // SpendMoney 내부에서 재화 부족 로그 출력
         {
              Debug.Log($" - 재화 사용 성공!"); // 로그 추가
             // 도구 획득 처리
             if(itemToBuy == thermometerData) InventoryManager.Instance.AcquireThermometer();
             else if(itemToBuy == hygrometerData) InventoryManager.Instance.AcquireHygrometer();

             Debug.Log($"{itemToBuy.itemName}을(를) 성공적으로 구매했습니다!");
             // UI는 OnInventoryUpdated 이벤트에 의해 자동으로 갱신됨
         }
         else {
             Debug.Log($" - 재화 부족으로 구매 실패."); // 로그 추가
         }
    }

    /// <summary> 상점 UI를 닫습니다. </summary>
    public void Close() // CloseShop -> Close 로 변경 (일반성)
    {
        gameObject.SetActive(false);
        // Debug.Log("Shop UI Closed"); // 로그 레벨 조절
    }

    /// <summary> 상점 UI를 엽니다. (외부 호출용) </summary>
    public void Open()
    {
        // 이미 활성화 상태면 추가 동작 없음
        if(gameObject.activeSelf) return;

        gameObject.SetActive(true);
        // UpdateShopUI(); // OnEnable에서 자동으로 호출됨
        // Debug.Log("Shop UI Opened"); // 로그 레벨 조절
    }


    // --- 가격 결정 헬퍼 함수 ---
    /// <summary> 오버라이드를 고려한 판매 가격 반환 </summary>
    private int GetSellPrice(ItemData item)
    {
        if(item == branchData && overrideSellPriceBranch >= 0) return overrideSellPriceBranch;
        if(item == featherData && overrideSellPriceFeather >= 0) return overrideSellPriceFeather;
        if(item == mossData && overrideSellPriceMoss >= 0) return overrideSellPriceMoss;
        // 다른 판매 아이템 오버라이드 추가
        return item != null ? item.sellPrice : 0; // 기본값 반환
    }
    /// <summary> 오버라이드를 고려한 구매 가격 반환 </summary>
    private int GetBuyPrice(ItemData item)
    {
         if(item == thermometerData && overrideBuyPriceThermo >= 0) return overrideBuyPriceThermo;
         if(item == hygrometerData && overrideBuyPriceHygro >= 0) return overrideBuyPriceHygro;
         // 다른 구매 아이템 오버라이드 추가
         return item != null ? item.buyPrice : 0; // 기본값 반환
    }
}