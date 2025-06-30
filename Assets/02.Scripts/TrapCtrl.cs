using UnityEngine;
public class TrapCtrl : MonoBehaviour
{
    private Animator anim;
    private bool isTriggered = false;
    private bool isAnimating = false;
    private GameObject targetPlayer = null;
    private float triggerDelay = 1.0f;
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
    }

    void Update()
    {
        // 트리거 후 1초가 지나면 애니메이션 실행
        if (isTriggered && !isAnimating)
        {
            triggerTimer += Time.deltaTime;
            if (triggerTimer >= triggerDelay)
            {
                isAnimating = true;
                anim.SetBool("Trap", true); // bool 파라미터로 트리거
            }
        }

        // 애니메이션이 끝났는지 체크 (Trap 파라미터를 false로 돌림)
        if (isAnimating)
        {
            isAnimating = false;
            anim.SetBool("Trap", false);
            isTriggered = false; // 다시 트리거 가능
        }
    }

    void LateUpdate()
    {
        if (spriteRenderer.sprite != lastSprite)
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

    void OnTriggerEnter2D(Collider2D coll)
    {
        // 애니메이션이 실행 중이 아닐 때만 트리거
        if (!isTriggered && !isAnimating && (coll.CompareTag("Player1") || coll.CompareTag("Player2")))
        {
            isTriggered = true;
            triggerTimer = 0f;
            targetPlayer = coll.gameObject;
            Debug.Log("Trap triggered by: " + targetPlayer.name);
        }
    }

    void OnTriggerStay2D(Collider2D coll)
    {
        // 애니메이션 실행 중에만 데미지
        if (isAnimating && (coll.CompareTag("Player1") || coll.CompareTag("Player2")))
        {
            Player player = coll.GetComponent<Player>();
            if (player != null && !player.isDead)
            {
                player.TakeDamage(20f);
                // 데미지는 한 번만 주고 싶으면 아래 주석 해제
                // isAnimating = false;
            }
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject == targetPlayer)
        {
            targetPlayer = null;
        }
    }
}