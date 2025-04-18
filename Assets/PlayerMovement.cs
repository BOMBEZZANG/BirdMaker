using UnityEngine;

/// <summary>
/// 플레이어 캐릭터의 기본적인 상하좌우 이동을 처리하는 스크립트 (탑뷰)
/// Rigidbody2D (Kinematic)를 사용하여 이동합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))] // Rigidbody2D 컴포넌트가 이 스크립트에 필수임을 명시
public class PlayerMovement : MonoBehaviour
{
    // === 이동 관련 변수 ===
    [Header("Movement Settings")] // Inspector에 헤더 추가
    [Tooltip("플레이어의 초당 이동 속도입니다.")]
    [SerializeField] // Inspector에서 값을 수정할 수 있도록 함
    private float moveSpeed = 5f;

    // === 컴포넌트 참조 변수 ===
    private Rigidbody2D rb;          // 플레이어의 Rigidbody2D 컴포넌트를 저장할 변수
    private Vector2 movementInput;  // 매 프레임 계산될 사용자의 입력 및 이동 방향

    // === Unity 생명주기 함수 ===

    /// <summary>
    /// 스크립트 인스턴스가 처음 로드될 때 호출됩니다. (Start보다 먼저 실행)
    /// 주로 컴포넌트 참조 및 초기 설정에 사용됩니다.
    /// </summary>
    [System.Obsolete]
    void Awake()
    {
        // 이 게임오브젝트에 부착된 Rigidbody2D 컴포넌트를 찾아서 rb 변수에 할당합니다.
        rb = GetComponent<Rigidbody2D>();

        // Rigidbody2D 컴포넌트가 제대로 할당되었는지, 설정이 올바른지 확인합니다.
        if (rb != null)
        {
            // 탑뷰 이동에 적합하도록 Rigidbody 설정을 코드에서 다시 한번 확인/적용합니다.
            // (Inspector에서 실수로 변경했더라도 여기서 보정)
            if (!rb.isKinematic)
            {
                Debug.LogWarning($"[{this.gameObject.name}] Rigidbody2D의 Body Type이 Kinematic이 아닙니다. Kinematic으로 변경합니다.");
                rb.isKinematic = true;
            }
            if (rb.gravityScale != 0)
            {
                Debug.LogWarning($"[{this.gameObject.name}] Rigidbody2D의 Gravity Scale이 0이 아닙니다. 0으로 변경합니다.");
                rb.gravityScale = 0;
            }
            rb.freezeRotation = true; // 이동 시 물리적으로 회전하지 않도록 Z축 회전 고정
        }
        else
        {
            // Rigidbody2D가 없다면 에러 로그를 출력합니다. (RequireComponent 덕분에 보통 이럴 일은 없음)
            Debug.LogError($"[{this.gameObject.name}] 필수 컴포넌트인 Rigidbody2D를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 매 프레임마다 호출됩니다.
    /// 주로 사용자 입력 감지, 애니메이션 상태 전환 등 즉각적인 반응이 필요한 로직에 사용됩니다.
    /// </summary>
    void Update()
    {
        // 사용자 입력 받기 (Legacy Input Manager 기준)
        // GetAxisRaw는 -1(좌/하), 0(입력 없음), 1(우/상) 중 하나를 반환합니다. (즉각적인 반응)
        float moveX = Input.GetAxisRaw("Horizontal"); // 키보드 A/D 또는 좌/우 화살표
        float moveY = Input.GetAxisRaw("Vertical");   // 키보드 W/S 또는 위/아래 화살표

        // 입력 값을 Vector2로 만들고 정규화(normalize)합니다.
        // 정규화 이유: 대각선으로 이동 시 속도가 빨라지는 것을 방지 (벡터 길이가 1이 됨)
        movementInput = new Vector2(moveX, moveY).normalized;

        // --- 나중에 애니메이션 로직을 여기에 추가할 수 있습니다 ---
        // 예: animator.SetFloat("MoveX", moveX);
        //     animator.SetFloat("MoveY", moveY);
        //     animator.SetBool("IsMoving", movementInput.magnitude > 0);
    }

    /// <summary>
    /// 고정된 시간 간격(기본 0.02초)마다 호출됩니다. (Edit > Project Settings > Time > Fixed Timestep)
    /// Rigidbody 등 물리 관련 계산 및 이동 처리는 여기서 하는 것이 안정적입니다.
    /// </summary>
    void FixedUpdate()
    {
        // Rigidbody2D 컴포넌트가 있다면 이동 로직을 실행합니다.
        if (rb != null)
        {
            // 현재 위치(rb.position)에서 계산된 방향(movementInput)으로
            // 속도(moveSpeed)와 고정 시간(Time.fixedDeltaTime)을 곱한 만큼 이동합니다.
            // Time.fixedDeltaTime을 곱하는 이유: 프레임 속도에 관계없이 일정한 속도로 이동하게 함
            rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
        }
    }
}