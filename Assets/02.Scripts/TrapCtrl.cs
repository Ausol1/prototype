using UnityEngine;

public class TrapCtrl : MonoBehaviour
{
    private Animator anim;
    private bool isTriggered = false; // Ʈ�� �ߵ� �غ� ���� (������ ����)
    private bool isAnimating = false; // Ʈ�� �ִϸ��̼� ���� �� ����
    private GameObject targetPlayer = null; // � �÷��̾ Ʈ�����ߴ��� ���� (���� ����)
    private float triggerDelay = 1.0f; // ������ �ð� (1��)
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
            Debug.LogError("TrapCtrl: Animator ������Ʈ�� ã�� �� �����ϴ�! GameObject�� Animator�� �Ҵ�Ǿ����� Ȯ���ϼ���.", this);
        }
    }

    void Update()
    {
        // Ʈ�� �ߵ� �غ� �����̰� �ִϸ��̼��� ���� ���� �ƴ� ���� Ÿ�̸� ����
        // isTriggered�� true�� �Ǹ� �÷��̾ �ݶ��̴��� ����� ��� Ÿ�̸Ӱ� ������
        if (isTriggered && !isAnimating)
        {
            triggerTimer += Time.deltaTime;
            Debug.Log($"TrapCtrl Update: triggerTimer={triggerTimer:F2}, triggerDelay={triggerDelay:F2}"); // ����׿�

            if (triggerTimer >= triggerDelay)
            {
                isAnimating = true; // �ִϸ��̼� ���� ���·� ����
                if (anim != null)
                {
                    anim.SetBool("Trap", true); // Animator�� "Trap" �Ķ���� Ȱ��ȭ
                    Debug.Log("Trap animation STARTED for: " + (targetPlayer != null ? targetPlayer.name : "Unknown Player"));
                }
                // ���⼭ isTriggered�� false�� ������ ����. �ִϸ��̼� ���� �� OnTrapAnimEnd()���� ����.
            }
        }
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ���� �Լ� (�ִϸ��̼� Ŭ�� ���� �̺�Ʈ�� ����)
    public void OnTrapAnimEnd()
    {
        Debug.Log("OnTrapAnimEnd() called by Animation Event!");
        if (anim != null)
        {
            anim.SetBool("Trap", false); // Animator�� "Trap" �Ķ���� ��Ȱ��ȭ (���� ���·� ���ƿ�)
        }
        isAnimating = false;    // �ִϸ��̼� ���� ���·� ����
        isTriggered = false;    // Ʈ�� �ߵ� �غ� ���� ���� (���� �ߵ��� ����)
        triggerTimer = 0f;      // Ÿ�̸� ����
        targetPlayer = null;    // ��� �÷��̾� ���� �ʱ�ȭ
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

    // �÷��̾ Ʈ�� �ݶ��̴��� ó�� �������� �� ȣ��
    void OnTriggerEnter2D(Collider2D coll)
    {
        // Ʈ���� �̹� �ߵ� �غ� ���̰ų� �ִϸ��̼��� ���� ���� �ƴ� ���� �ߵ�
        if (!isTriggered && !isAnimating && (coll.CompareTag("Player1") || coll.CompareTag("Player2")))
        {
            isTriggered = true; // Ʈ�� �ߵ� �غ� ���·� ����
            triggerTimer = 0f;  // Ÿ�̸� �ʱ�ȭ
            targetPlayer = coll.gameObject; // Ʈ������ �÷��̾� ����
            Debug.Log("Trap triggered by (ENTER): " + targetPlayer.name);
        }
    }

    // �÷��̾ Ʈ�� �ݶ��̴� ���ο� �ӹ����� ���� ��� ȣ��
    void OnTriggerStay2D(Collider2D coll)
    {
        // �ִϸ��̼��� ���� ���� ���� ������ ó��
        if (isAnimating && (coll.CompareTag("Player1") || coll.CompareTag("Player2")))
        {
            Player player = coll.GetComponent<Player>();
            if (player != null && !player.isDead)
            {
                // �������� OnTriggerStay2D���� �� ������ �� �� �����Ƿ�,
                // �÷��̾� ��ũ��Ʈ���� ������ ��Ÿ���� �����ϴ� ���� �����ϴ�.
                player.TakeDamage(20f);
            }
        }
    }

    // �÷��̾ Ʈ�� �ݶ��̴��� ����� �� ȣ��
    void OnTriggerExit2D(Collider2D coll)
    {
    }
}