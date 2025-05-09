using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //--- 주인공 체력 변수
    float m_MaxHp = 200.0f;
    [HideInInspector] public float m_CurHp = 200.0f;
    public Image m_HpBar = null;

    //--- 키보드 이동 관련 변수 
    float h =0.0f;
    public float m_JumpForce = 10.0f;
    public float m_MoveSpeed = 2.6f;
    Vector3 m_DirVec;

    private Rigidbody2D rb;
    private bool isGrounded = false;

    //--- 총알 발사 변수
    public GameObject m_BulletPrefab = null;
    public Transform m_ShootPos;
    public float shootForce = 10.0f;
    public float m_ShootCool = 0.5f;
    float ShootTimer = 0.0f;

    //--- 애니매이션 관련 변수
    SpriteRenderer SpriteRenderer;
    Animator Anim;
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Shooting();
        Animation();
    }

    void Move()
    {
        h = 0.0f;
        if (Input.GetKey(KeyCode.A)) h = -1.0f;
        if (Input.GetKey(KeyCode.D)) h = 1.0f;
        rb.linearVelocity = new Vector2(h * m_MoveSpeed, rb.linearVelocityY);

        if (isGrounded && Input.GetKey(KeyCode.W))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, m_JumpForce);
        }

    }
    void Shooting()
    {
        ShootTimer -= Time.deltaTime;

        if (Input.GetKey(KeyCode.F) && ShootTimer <= 0f)
        {
            GameObject bullet = Instantiate(m_BulletPrefab, m_ShootPos.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            Vector2 shootDir = Vector2.right;
            rb.linearVelocity = shootDir * shootForce;

            ShootTimer = m_ShootCool;
        }
    }

    void Animation()
    {
        Anim.SetFloat("Speed", h);

        if (h != 0.0f )
        {
            SpriteRenderer.flipX = h<0.0f;
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
            isGrounded = true;
    }
    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Ground"))
            isGrounded = false;
    }
}
