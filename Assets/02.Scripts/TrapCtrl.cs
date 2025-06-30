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
        // Ʈ���� �� 1�ʰ� ������ �ִϸ��̼� ����
        if (isTriggered && !isAnimating)
        {
            triggerTimer += Time.deltaTime;
            if (triggerTimer >= triggerDelay)
            {
                isAnimating = true;
                anim.SetBool("Trap", true); // bool �Ķ���ͷ� Ʈ����
            }
        }

        // �ִϸ��̼��� �������� üũ (Trap �Ķ���͸� false�� ����)
        if (isAnimating)
        {
            isAnimating = false;
            anim.SetBool("Trap", false);
            isTriggered = false; // �ٽ� Ʈ���� ����
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
        // �ִϸ��̼��� ���� ���� �ƴ� ���� Ʈ����
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
        // �ִϸ��̼� ���� �߿��� ������
        if (isAnimating && (coll.CompareTag("Player1") || coll.CompareTag("Player2")))
        {
            Player player = coll.GetComponent<Player>();
            if (player != null && !player.isDead)
            {
                player.TakeDamage(20f);
                // �������� �� ���� �ְ� ������ �Ʒ� �ּ� ����
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