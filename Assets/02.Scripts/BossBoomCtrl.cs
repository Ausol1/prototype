using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BossBoomCtrl : MonoBehaviour
{
    public float riseSpeed = 4f;
    private float boomTime;
    private bool isExploding = false;
    private Animator anim;
    private bool explodedByPlayer = false;
    private bool boomAnimStarted = false;

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
        boomTime = Random.Range(4f, 5f);
        if (anim != null)
            anim.enabled = false; // ���� �� �ִϸ��̼� ��Ȱ��ȭ
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
            // �ִϸ��̼��� �������� Ȯ��
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                Destroy(gameObject);
            }
        }

        // �ִϸ��̼� ���� ���� �� �ݶ��̴� ũ�� ����
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
            anim.enabled = true; // �ִϸ��̼� Ȱ��ȭ
            anim.SetTrigger("Boom"); // ���� �ִϸ��̼� ����
            boomAnimStarted = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!isExploding && (coll.CompareTag("Player1") || coll.CompareTag("Player2")))
        {
            explodedByPlayer = true;
            Explode();
        }
    }
}