using TMPro;
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
    public float m_DamageCool = 1.0f;
    public float m_LavaCool = 0.25f;

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
    public GameObject m_Gun = null;
    public float shootForce = 10.0f;
    public float m_ShootCool = 0.5f;
    float ShootTimer = 0.0f;

    public int m_BulletMaxCount = 12;
    public int m_BulletCurrentCount = 12;
    public float m_ReloadTime = 1.5f;
    bool isReloading = false;

    public TextMeshProUGUI BulletCount;

    //---애니메이션 관련 변수
    SpriteRenderer SpriteRenderer;
    Animator Anim;

    //--- 입력 키 설정
    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode jumpKey;
    private KeyCode shootKey;
    private KeyCode reloadKey;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Anim = GetComponent<Animator>();

        // 플레이어끼리 겹칠 수 있도록 충돌 무시 설정
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject otherPlayer in players)
        {
            if (otherPlayer != this.gameObject)
            {
                Collider2D myCol = GetComponent<Collider2D>();
                Collider2D otherCol = otherPlayer.GetComponent<Collider2D>();
                if (myCol != null && otherCol != null)
                {
                    Physics2D.IgnoreCollision(myCol, otherCol, true);
                }
            }
        }

        if (playerType == PlayerType.Player1)
        {
            leftKey = KeyCode.A;
            rightKey = KeyCode.D;
            jumpKey = KeyCode.W;
            shootKey = KeyCode.F;
            reloadKey = KeyCode.R;
        }
        else if (playerType == PlayerType.Player2)
        {
            leftKey = KeyCode.LeftArrow;
            rightKey = KeyCode.RightArrow;
            jumpKey = KeyCode.UpArrow;
            shootKey = KeyCode.Return;
            reloadKey = KeyCode.RightControl;
        }
    }

    void Update()
    {
        Move();
        Shooting();
        Animation();

        if (m_HpBar != null)
            m_HpBar.fillAmount = m_CurHp / m_MaxHp;

        BulletCount.text = m_BulletCurrentCount + " /  " + m_BulletMaxCount;

        m_DamageCool -= Time.deltaTime;
        m_LavaCool -= Time.deltaTime;
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

        if (isReloading)
            return;

        if (Input.GetKey(shootKey) && ShootTimer <= 0f && m_BulletCurrentCount > 0)
        {
            GameObject bullet = Instantiate(m_BulletPrefab, m_ShootPos.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            Vector2 shootDir = SpriteRenderer.flipX ? Vector2.left : Vector2.right;
            rb.linearVelocity = shootDir * shootForce;
            ShootTimer = m_ShootCool;
            m_BulletCurrentCount--;
        }
        if (Input.GetKeyDown(reloadKey) || m_BulletCurrentCount <= 0)
        {
            isReloading = true;
            Invoke("Reload", m_ReloadTime);
        }
    }
    void Reload()
    {
        m_BulletCurrentCount = m_BulletMaxCount;
        isReloading = false;
    }

    void Animation()
    {
        Anim.SetFloat("Speed", h);
        bool t = h == 0.0f;
        Anim.SetBool("speed", t);

        if (h != 0.0f)
        {
            SpriteRenderer.flipX = h < 0.0f;

            // 총 위치 및 이미지 반전
            Vector3 shootPos = m_ShootPos.localPosition;
            shootPos.x = h > 0f ? Mathf.Abs(shootPos.x) : -Mathf.Abs(shootPos.x);
            m_ShootPos.localPosition = shootPos;

            if (m_Gun != null)
            {
                Vector3 gunScale = m_Gun.transform.localScale;
                Vector3 gunPosition = m_Gun.transform.localPosition;
                gunScale.x = h > 0f ? Mathf.Abs(gunScale.x) : -Mathf.Abs(gunScale.x);
                gunPosition.x = h > 0f ? Mathf.Abs(gunPosition.x) : -Mathf.Abs(gunPosition.x);
                m_Gun.transform.localScale = gunScale;
                m_Gun.transform.localPosition = gunPosition;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag == "EnemyBullet")
        {
            TakeDamage(10);
            Destroy(coll.gameObject);
        }

        if(coll.tag =="Fire")
        {
            TakeDamage(50);
        }
    }

    public void TakeDamage(float a_Value)
    {
        if (m_CurHp <= 0.0f)
            return;

        m_CurHp -= a_Value;
        if (m_CurHp < 0.0f)
            m_CurHp = 0.0f;

        if (m_CurHp <= 0.0f)
        {
            if (GameMgr.Inst != null)
                GameMgr.Inst.OnPlayerDead();
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

                if (normal.y > 0.1f)
                {
                    isGrounded = true;
                    JumpCount = 2;
                    break;
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D coll)
    {
        if (m_DamageCool < 0 && coll.collider.CompareTag("Enemy"))
        {
            TakeDamage(30);
            m_DamageCool = 1;
        }
    }

    private void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Ground"))
            isGrounded = false;
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        if (m_LavaCool < 0 && coll.CompareTag("Lava"))
        {
            TakeDamage(10);
            m_LavaCool = 0.25f;
        }
    }
}


