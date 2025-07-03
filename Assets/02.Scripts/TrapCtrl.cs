using UnityEngine;

public class TrapCtrl : MonoBehaviour
{
    private Animator anim;
    private bool isTriggered = false; // 트랩 발동 준비 상태 (딜레이 시작)
    private bool isAnimating = false; // 트랩 애니메이션 실행 중 상태
    private GameObject targetPlayer = null; // 어떤 플레이어가 트리거했는지 저장 (선택 사항)
    private float triggerDelay = 1.0f; // 딜레이 시간 (1초)
    private float triggerTimer = 0f;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Sprite lastSprite;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("TrapCtrl: Animator 컴포넌트를 찾을 수 없습니다! GameObject에 Animator가 할당되었는지 확인하세요.", this);
        }
    }

    void Update()
    {
        // 트랩 발동 준비 상태이고 애니메이션이 실행 중이 아닐 때만 타이머 증가
        // isTriggered가 true가 되면 플레이어가 콜라이더를 벗어나도 계속 타이머가 증가함
        if (isTriggered && !isAnimating)
        {
            triggerTimer += Time.deltaTime;
            Debug.Log($"TrapCtrl Update: triggerTimer={triggerTimer:F2}, triggerDelay={triggerDelay:F2}"); // 디버그용

            if (triggerTimer >= triggerDelay)
            {
                isAnimating = true; // 애니메이션 실행 상태로 변경
                if (anim != null)
                {
                    anim.SetBool("Trap", true); // Animator에 "Trap" 파라미터 활성화
                    Debug.Log("Trap animation STARTED for: " + (targetPlayer != null ? targetPlayer.name : "Unknown Player"));
                }
                // 여기서 isTriggered를 false로 만들지 않음. 애니메이션 종료 후 OnTrapAnimEnd()에서 리셋.
            }
        }
    }

    // 애니메이션 이벤트에서 호출할 함수 (애니메이션 클립 끝에 이벤트로 연결)
    public void OnTrapAnimEnd()
    {
        Debug.Log("OnTrapAnimEnd() called by Animation Event!");
        if (anim != null)
        {
            anim.SetBool("Trap", false); // Animator의 "Trap" 파라미터 비활성화 (원래 상태로 돌아옴)
        }
        isAnimating = false;    // 애니메이션 종료 상태로 변경
        isTriggered = false;    // 트랩 발동 준비 상태 리셋 (다음 발동을 위해)
        triggerTimer = 0f;      // 타이머 리셋
        targetPlayer = null;    // 대상 플레이어 정보 초기화
        Debug.Log("Trap reset. isTriggered: " + isTriggered + ", isAnimating: " + isAnimating);
    }

    void LateUpdate()
    {
        if (spriteRenderer.sprite != null && spriteRenderer.sprite != lastSprite)
        {
            UpdateColliderToSprite();
            lastSprite = spriteRenderer.sprite;
        }
    }

    void UpdateColliderToSprite()
    {
        if (spriteRenderer.sprite == null) return;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        boxCollider.size = spriteBounds.size;
        boxCollider.offset = spriteBounds.center;
    }

    // 플레이어가 트랩 콜라이더에 처음 진입했을 때 호출
    void OnTriggerEnter2D(Collider2D coll)
    {
        // 트랩이 이미 발동 준비 중이거나 애니메이션이 실행 중이 아닐 때만 발동
        if (!isTriggered && !isAnimating && (coll.CompareTag("Player1") || coll.CompareTag("Player2")))
        {
            isTriggered = true; // 트랩 발동 준비 상태로 변경
            triggerTimer = 0f;  // 타이머 초기화
            targetPlayer = coll.gameObject; // 트리거한 플레이어 저장
            Debug.Log("Trap triggered by (ENTER): " + targetPlayer.name);
        }
    }

    // 플레이어가 트랩 콜라이더 내부에 머무르는 동안 계속 호출
    void OnTriggerStay2D(Collider2D coll)
    {
        // 애니메이션이 실행 중일 때만 데미지 처리
        if (isAnimating && (coll.CompareTag("Player1") || coll.CompareTag("Player2")))
        {
            Player player = coll.GetComponent<Player>();
            if (player != null && !player.isDead)
            {
                // 데미지는 OnTriggerStay2D에서 매 프레임 들어갈 수 있으므로,
                // 플레이어 스크립트에서 데미지 쿨타임을 관리하는 것이 좋습니다.
                player.TakeDamage(20f);
            }
        }
    }

    // 플레이어가 트랩 콜라이더를 벗어났을 때 호출
    void OnTriggerExit2D(Collider2D coll)
    {
    }
}