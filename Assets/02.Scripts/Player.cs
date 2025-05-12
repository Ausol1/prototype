using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public enum PlayerType { Player1, Player2 }
    public PlayerType playerType = PlayerType.Player1;

    //--- 플레이어 변수
    float m_MaxHp = 200.0f;
    public float m_CurHp = 200.0f;
    public Image m_HpBar = null;
    public float m_DamageCool = 2.0f;

    //--- 플레이어 움직임 관련 변수 
    float h = 0.0f;
    public float m_JumpForce = 10.0f;
    public float m_MoveSpeed = 2.6f;
    Vector3 m_DirVec;

    private Rigidbody2D rb;
    public bool isGrounded = false;
    public int JumpCount = 0;

    //--- 총 관련 변수
    public GameObject m_BulletPrefab = null;
    public Transform m_ShootPos;
    public float shootForce = 10.0f;
    public float m_ShootCool = 0.5f;
    float ShootTimer = 0.0f;

    //---애니메이션 관련 변수
    SpriteRenderer SpriteRenderer;
    Animator Anim;

    //--- 입력 키 설정
    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode jumpKey;
    private KeyCode shootKey;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Anim = GetComponent<Animator>();

        if (playerType == PlayerType.Player1)
        {
            leftKey = KeyCode.A;
            rightKey = KeyCode.D;
            jumpKey = KeyCode.W;
            shootKey = KeyCode.F;
        }
        else if (playerType == PlayerType.Player2)
        {
            leftKey = KeyCode.LeftArrow;
            rightKey = KeyCode.RightArrow;
            jumpKey = KeyCode.UpArrow;
            shootKey = KeyCode.Return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Shooting();
        Animation();

        if (m_HpBar != null)
            m_HpBar.fillAmount = m_CurHp / m_MaxHp;

        m_DamageCool -= Time.deltaTime;
    }

    void Move()
    {
        h = 0.0f;
        if (Input.GetKey(leftKey)) h = -1.0f;
        if (Input.GetKey(rightKey)) h = 1.0f;
        rb.linearVelocity = new Vector2(h * m_MoveSpeed, rb.linearVelocity.y);

        if (Input.GetKeyDown(jumpKey) && JumpCount > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, m_JumpForce);
            JumpCount--;
        }
    }

    void Shooting()
    {
        ShootTimer -= Time.deltaTime;

        if (Input.GetKey(shootKey) && ShootTimer <= 0f)
        {
            GameObject bullet = Instantiate(m_BulletPrefab, m_ShootPos.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            Vector2 shootDir = SpriteRenderer.flipX ? Vector2.left : Vector2.right;
            rb.linearVelocity = shootDir * shootForce;
            ShootTimer = m_ShootCool;
        }
    }

    void Animation()
    {
        Anim.SetFloat("Speed", h);
        bool t = h == 0.0f;
        Anim.SetBool("speed", t);

        if (h != 0.0f)
        {
            SpriteRenderer.flipX = h < 0.0f;

            Vector3 shootPos = m_ShootPos.localPosition;
            if (h > 0f)
                shootPos.x = Mathf.Abs(shootPos.x);
            else if (h < 0f)
                shootPos.x = -Mathf.Abs(shootPos.x);
            m_ShootPos.localPosition = shootPos;
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag == "EnemyBullet")
        {
            TakeDamage(10);
            Destroy(coll.gameObject);
        }


    }

    public void TakeDamage(float a_Value)
    {
        if (m_CurHp <= 0.0f)
            return;

        m_CurHp -= a_Value;
        if (m_CurHp < 0.0f)
            m_CurHp = 0.0f;

        if (m_HpBar != null)
            m_HpBar.fillAmount = m_CurHp / m_MaxHp;

        if (m_CurHp <= 0.0f)
        {
            Time.timeScale = 0.0f;
        }
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Ground"))
        {
            for (int i = 0; i < coll.contacts.Length; i++)
            {
                ContactPoint2D contact = coll.contacts[i];
                Vector2 normal = contact.normal;

                if (normal.y > 0.5f)
                {
                    isGrounded = true;
                    JumpCount = 2;
                    break;
                }
            }
        }
     
        if (m_DamageCool < 0 && coll.collider.CompareTag("Enemy"))
        {
            TakeDamage(30);
            m_DamageCool = 2;
        }

    }

    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Ground"))
            isGrounded = false;
    }
}
