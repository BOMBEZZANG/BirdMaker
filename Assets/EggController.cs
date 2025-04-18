using UnityEngine;

/// <summary>
/// 알의 상태(온기)를 관리하는 기본 스크립트
/// </summary>
public class EggController : MonoBehaviour
{
    // === 상태 변수 ===
    // [Tooltip("알의 현재 온기 레벨입니다.")] // Inspector에 마우스를 올리면 설명 표시
    [SerializeField] // private 변수지만 Inspector에서 보이고 싶을 때 사용
    private float currentWarmth = 0f;

    // [Tooltip("알이 부화하거나 최적 상태를 유지하기 위한 목표 온기입니다.")]
    [SerializeField]
    private float targetWarmth = 100f; // 예시 목표치 (나중에 게임 디자인에 맞게 수정)

    // === Unity 생명주기 함수 ===

    /// <summary>
    /// 게임 오브젝트가 처음 활성화될 때 한 번 호출됩니다.
    /// 초기화 로직에 사용됩니다.
    /// </summary>
    void Start()
    {
        // 게임 시작 시 초기 온기 값을 설정할 수 있습니다. (현재는 0으로 시작)
        // currentWarmth = 0f; // 이미 위에서 초기화되어 있음

        // 디버그 로그를 통해 초기 상태를 확인합니다. (게임 실행 시 Console 창에 표시됨)
        Debug.Log($"[{this.gameObject.name}] 알 오브젝트가 생성되었습니다. 현재 온기: {currentWarmth}");
    }

    /// <summary>
    /// 매 프레임마다 호출됩니다.
    /// 게임 로직 업데이트(상태 변화 등)에 사용됩니다. (현재는 비워둠)
    /// </summary>
    void Update()
    {
        // 추후 여기에 온도가 시간에 따라 자연스럽게 감소하는 로직 등을 추가할 수 있습니다.
        // 예시: if (currentWarmth > 0) currentWarmth -= Time.deltaTime * 0.5f; // 초당 0.5씩 감소
    }

    // === 공개 함수 (외부에서 호출하여 알의 상태를 변경) ===

    /// <summary>
    /// 알에 온기를 더합니다. (나중에 플레이어 상호작용 등으로 호출)
    /// </summary>
    /// <param name="amount">더할 온기의 양</param>
    public void AddWarmth(float amount)
    {
        if (amount <= 0) return; // 0 이하의 값은 무시

        currentWarmth += amount;
        // Optional: 온기가 목표치를 초과하지 않도록 제한할 수 있습니다.
        // currentWarmth = Mathf.Min(currentWarmth, targetWarmth * 1.2f); // 예: 목표치의 120%까지만

        Debug.Log($"[{this.gameObject.name}] 온기 +{amount}! 현재 온기: {currentWarmth}");

        // 나중에 여기에 온기가 특정 수치에 도달했을 때의 로직(부화 조건 체크 등) 추가 가능
        // CheckHatchingCondition();
    }

    /// <summary>
    /// 현재 온기 값을 외부에서 읽어갈 수 있도록 하는 함수(Getter)입니다.
    /// </summary>
    /// <returns>현재 온기 값</returns>
    public float GetCurrentWarmth()
    {
        return currentWarmth;
    }

    // === 비공개 헬퍼 함수 (내부 로직) ===

    // /// <summary>
    // /// 부화 조건을 확인하는 함수 (나중에 구현)
    // /// </summary>
    // private void CheckHatchingCondition()
    // {
    //     if (currentWarmth >= targetWarmth)
    //     {
    //         Debug.Log($"[{this.gameObject.name}] 부화 조건을 충족했습니다!");
    //         // 여기에 부화 관련 로직 호출 (예: HatchSequence())
    //     }
    // }
}