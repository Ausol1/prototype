using UnityEngine;

public class SlimeController : MonoBehaviour
{
    // [Header("HP")] ���� - ü�� �ý��� �ʿ� ����

    [Header("Target & Range")]
    public Transform player1; // �÷��̾� 1 Ʈ������
    public Transform player2; // �÷��̾� 2 Ʈ������
    private Transform target; // ���� ���� ��� �÷��̾�
    public float traceRangeX = 10f; // �÷��̾ �����ϴ� X ����
    public float traceRangeY = 5f;  // �÷��̾ �����ϴ� Y ����
    public float attackRange = 1f;  // �������� ���� ���� (�����ؼ� ���)
    public float attackDamage = 10f; // �÷��̾�� �� ������
    public float attackDgeSmall = 5f; // ���� �������� �÷��̾�� �� ������ (�п��� ������)

    [Header("Move - Jump Only")]
    public float jumpForce = 8f; // ���� ����
    public float jumpInterval = 1.5f; // ���� �ֱ�
    private float jumpTimer; // ���� �������� ���� �ð�
    public float moveSpeed = 3f; // ���� ������ �����ϴ� ���� �ӵ�

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // �ִϸ��̼��� �ִٸ�

    [Header("Ground Check")]
    public Transform groundCheck; // ���� üũ�� ���� Transform (������ �� �Ʒ� �ڽ����� ��ġ)
    public float groundCheckDistance = 0.1f; // ���� ���� �Ÿ�
    public LayerMask groundLayer; // ���� ���̾�
    private bool isGrounded; // ���� ���鿡 ��� �ִ���

    [Header("Split Logic")]
    public GameObject smallSlimePrefab; // �п��� ���� ������ ������ (���⿡ �ٽ� SlimeController�� �پ��־�� ��)
    public int minSplitCount = 2; // �ּ� �п� ����
    public int maxSplitCount = 3; // �ִ� �п� ����
    public float splitSizeMultiplier = 0.7f; // �п��� �������� ũ�� ����

    private bool isSplitSlime = false; // �� �������� �п��� ������ ���� ���������� ����

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        jumpTimer = jumpInterval;

        if (!isSplitSlime)
        {
            gameObject.tag = "Enemy";
        }

        // �÷��̾� Ʈ������ ���� �Ҵ�
        if (player1 == null)
        {
            GameObject p1 = GameObject.FindWithTag("Player1");
            if (p1 != null) player1 = p1.transform;
        }
        if (player2 == null)
        {
            GameObject p2 = GameObject.FindWithTag("Player2");
            if (p2 != null) player2 = p2.transform;
        }
        if (player1 == null || player2 == null)
        {
            Debug.LogWarning("Player 1 �Ǵ� Player 2 Ʈ�������� �Ҵ���� �ʾҽ��ϴ�! �÷��̾� ������ ����� �۵����� ���� �� �ֽ��ϴ�.", this);
        }
    }

    // �ܺο��� ȣ���Ͽ� �� �������� �п��� ���� ���������� ����
    public void SetAsSplitSlime()
    {
        isSplitSlime = true;
        // �п��� �������� û�ұ� ���� ���� ���·� ����
        gameObject.tag = "SmallMonster"; // �±׸� "SmallMonster"�� ����

        // ũ�� ����
        transform.localScale = transform.localScale * splitSizeMultiplier;

        // �п��� �������� �������̳� ������ �����ϰ� �ʹٸ� ���⼭ ���� ����
        // jumpForce *= 0.8f;
        // jumpInterval *= 0.7f;
    }


    private void Update()
    {
        UpdateTarget(); // �÷��̾� ����
        CheckGround(); // ���� Ȯ��
        HandleJumpMove(); // ���� �̵� ����

        // �п��� ������ (SmallMonster �±�)�� ��� �ð��� �ǵ��
        if (isSplitSlime && spriteRenderer != null)
        {
            // ����: ���� ����, ���� ���� �� (���� ������ �������� ǥ��)
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f); // �ణ �����ϰ�
        }
        else if (spriteRenderer != null) // ���� �������� �������ϰ�
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void UpdateTarget()
    {
        if (player1 == null || player2 == null) return;

        // �� �÷��̾���� �Ÿ� ���
        Vector2 pos = transform.position;
        Vector2 p1 = player1.position;
        Vector2 p2 = player2.position;

        float dx1 = Mathf.Abs(pos.x - p1.x);
        float dy1 = Mathf.Abs(pos.y - p1.y);
        float dx2 = Mathf.Abs(pos.x - p2.x);
        float dy2 = Mathf.Abs(pos.y - p2.y);

        bool p1InRange = dx1 <= traceRangeX && dy1 <= traceRangeY;
        bool p2InRange = dx2 <= traceRangeX && dy2 <= traceRangeY;

        if (p1InRange && p2InRange)
        {
            // �� �� ���� ���̸� �� ����� �� ����
            target = (dx1 * dx1 + dy1 * dy1) < (dx2 * dx2 + dy2 * dy2) ? player1 : player2;
        }
        else if (p1InRange)
        {
            target = player1;
        }
        else if (p2InRange)
        {
            target = player2;
        }
        else
        {
            target = null; // �� �� ���� ���̸� ���� �� ��
        }
    }

    private void CheckGround()
    {
        // groundCheck Transform���� �Ʒ��� ����ĳ��Ʈ�� ���� ���� ����
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        // Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red); // ����׿� (�� �信 �ʷ�/���� �� ǥ��)
    }

    private void HandleJumpMove()
    {
        if (target == null) return;

        jumpTimer -= Time.deltaTime; // ���� Ÿ�̸� ����

        // ���鿡 ��� �ְ�, ���� Ÿ�̸Ӱ� 0 ������ ���� ����
        if (isGrounded && jumpTimer <= 0f)
        {
            Vector2 diff = target.position - transform.position;
            int dir = diff.x > 0 ? 1 : -1; // Ÿ�� ���� ���� (����� ������, ������ ����)

            // ������ ��������Ʈ ���� ��ȯ
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = dir < 0; // dir�� �����̸� ��������Ʈ X�� ����
            }

            // Rigidbody�� linearVelocity�� ���� �����Ͽ� ����
            // ���� �ӵ�(moveSpeed * dir)�� ���� ������(jumpForce) ����
            rb.linearVelocity = new Vector2(moveSpeed * dir, jumpForce);

            jumpTimer = jumpInterval; // ���� Ÿ�̸� �缳��

            // �ִϸ��̼��� �ִٸ� ���� �ִϸ��̼� Ʈ����
            // if (animator != null) animator.SetTrigger("Jump");
        }
    }

    // �÷��̾�� ���� �� ������ (�������� ����)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player1"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(attackDamage);
            }
        }
        if (collision.gameObject.CompareTag("Player2"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(attackDamage);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ���� �Ҹ� �ǰ� ��
        if (collision.CompareTag("RocketBullet")) // ���� �Ѿ� �±�
        {
            if (!isSplitSlime) // ���� �п����� ���� (ũ�� "Enemy" �±׸� ����) �����Ӹ� �п�
            {
                int splitCount = Random.Range(minSplitCount, maxSplitCount + 1); // 2~3�� ���� �п�

                for (int i = 0; i < splitCount; i++)
                {
                    // ���� ������ ������ ����
                    GameObject newSmallSlime = Instantiate(smallSlimePrefab, transform.position, Quaternion.identity);
                    SlimeController newSlimeController = newSmallSlime.GetComponent<SlimeController>();

                    if (newSlimeController != null)
                    {
                        newSlimeController.SetAsSplitSlime(); // ���� ������ �������� '�п��� ������'���� ����
                    }

                    // �п� �� �ణ �������� �� ���ϱ� (���� ����, ���� Ƣ�� ����)
                    Rigidbody2D newSlimeRb = newSmallSlime.GetComponent<Rigidbody2D>();
                    if (newSlimeRb != null)
                    {
                        Vector2 randomForce = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized * 3f; // ������ �������� ��
                        newSlimeRb.AddForce(randomForce, ForceMode2D.Impulse);
                    }
                }
                Destroy(gameObject); // ���� ������ �ı� (�п��Ǿ����Ƿ�)
            }
            // �̹� �п��� ���� �������� ���Ͽ� �¾Ƶ� �ƹ� �� ���� (û�ұ�θ� ó��)
            Destroy(collision.gameObject); // ���� �Ѿ� �ı�
        }
    }
}