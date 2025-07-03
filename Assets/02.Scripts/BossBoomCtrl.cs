using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BossBoomCtrl : MonoBehaviour
{
    public float riseSpeed = 4f;
    private float boomTime;
    private bool isExploding = false;
    private bool isboom = false;
    private Animator anim;
    private bool explodedByPlayer = false;
    private bool boomAnimStarted = false;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Sprite lastSprite;
    public Player player;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        
        anim = GetComponent<Animator>();
        boomTime = Random.Range(4f, 5f);
        if (anim != null)
            anim.enabled = false; // 시작 시 애니메이션 비활성화
    }

    void Update()
    {
        if (!isExploding)
        {
            transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);

            boomTime -= Time.deltaTime;
            if (boomTime <= 0f)
            {
                Explode();
            }
        }
        else if (boomAnimStarted && anim != null)
        {
            // 애니메이션이 끝났는지 확인
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                Destroy(gameObject);
            }
        }

        // 애니메이션 실행 중일 때 콜라이더 크기 갱신
        if (spriteRenderer != null && spriteRenderer.sprite != lastSprite)
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

    void Explode()
    {
        if (isExploding) return;
        isExploding = true;
        if (anim != null)
        {
            anim.enabled = true; // 애니메이션 활성화
            anim.SetTrigger("Boom"); // 폭발 애니메이션 실행
            boomAnimStarted = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!isExploding && (coll.CompareTag("Player1") || coll.CompareTag("Player2"))&&!isboom)
        {
            explodedByPlayer = true;
            Explode();
            player = coll.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(20f); 
            }
            isboom = true; // 한번만 폭발하도록 설정
        }
    }
}