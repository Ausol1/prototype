using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public enum PlayerType { Player1, Player2 }
    public PlayerType playerType = PlayerType.Player1;

    // WeaponType에 SawtoothGun 추가
    public enum WeaponType { DefaultGun, RocketLauncher, VacuumCleaner, SawtoothGun }

    //--- 플레이어 변수
    float m_MaxHp = 100.0f;
    public float m_CurHp = 100.0f;
    public Image m_HpBar = null;
    public TextMeshProUGUI TextHp = null;
    public float m_DamageCool = 1.0f;
    public float m_LavaCool = 0.25f;

    //--- 플레이어 움직임 관련 변수
    float h = 0.0f;
    public float m_JumpForce = 10.0f;
    public float m_MoveSpeed = 2.6f;
    Vector3 m_DirVec;

    private Rigidbody2D rb;

    // --- 바닥 체크용 변수 추가 ---
    [Header("Ground Check")]
    public Transform groundCheck1;
    public Transform groundCheck2;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;
    public bool isGrounded = false;
    private bool isDoubleJumpAvailable = false;

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackUpForce = 3f;

    //--- 총 관련 변수
    public GameObject m_BulletPrefab = null; // 이 변수는 이제 사용되지 않을 수 있지만, 원본 코드를 유지합니다.
    public Transform m_ShootPos;
    public GameObject m_Gun = null;
    public Image m_ReloadImage = null;
    float reloadTimer = 0.0f;
    public float shootForce = 10.0f; // 이 변수는 이제 사용되지 않을 수 있지만, 원본 코드를 유지합니다.
    public float m_ShootCool = 0.5f; // 이 변수는 이제 사용되지 않을 수 있지만, 원본 코드를 유지합니다.
    float ShootTimer = 0.0f; // 이 변수는 이제 사용되지 않을 수 있지만, 원본 코드를 유지합니다.

    // --- 각 총 종류별 스탯 (DefaultGun) ---
    [Header("Default Gun Stats")]
    public GameObject defaultBulletPrefab;
    public float defaultGunShootForce = 10.0f;
    public float defaultGunShootCool = 0.5f;
    public int defaultGunBulletMaxCount = 12;
    public float defaultGunReloadTime = 1.5f;

    // --- 각 총 종류별 스탯 (RocketLauncher) ---
    [Header("Rocket Launcher Stats")]
    public GameObject rocketPrefab;
    public float rocketShootForce = 15.0f;
    public float rocketFireRate = 2.0f;
    public int rocketMaxCount = 3;
    public float rocketReloadTime = 3.0f;

    // --- 각 총 종류별 스탯 (VacuumCleaner) ---
    [Header("Vacuum Cleaner Stats")]
    public float suckRadius = 3f;
    public float suckForce = 10f;
    public float consumeDistance = 0.5f;
    public LayerMask smallMonsterLayer;
    public Image vacuumImage;

    // --- 각 총 종류별 스탯 (SawtoothGun) 추가 ---
    [Header("Sawtooth Gun Stats")]
    public GameObject sawtoothBulletPrefab;
    public float sawtoothGunShootForce = 12.0f;
    public float sawtoothGunFireRate = 0.7f;
    public int sawtoothGunMaxCount = 5;
    public float sawtoothGunReloadTime = 1.8f;

    // --- 현재 무기 상태 ---
    private float currentShootTimer = 0.0f;
    private int currentBulletCount;
    private bool isReloading = false;
    private int currentMaxBulletCount; // 현재 장착된 무기의 최대 탄창 수
    private float currentReloadTime; // 현재 장착된 무기의 재장전 시간
    private float currentFireRate; // 현재 장착된 무기의 발사 속도

    [Header("Weapon Configuration")]
    public WeaponType currentWeaponType;

    public TextMeshProUGUI BulletCount;

    // --- 부활 관련 변수 추가 ---
    public bool isDead = false;
    private bool isBeingRevived = false; // 이 변수는 현재 사용되지 않지만, 원본 코드를 유지합니다.
    private float reviveProgress = 0f;
    public float reviveRequired = 10f; // 스페이스 연타 10회 필요
    private Player otherPlayer; // 다른 플레이어 참조를 Update에서 사용하려면 필요합니다.
    private bool isOverlappingWithOther = false;

    // 부활 UI(선택)
    public Image reviveImage;
    public Image reviveBar;

    // *** 추가된 변수 ***
    public Collider2D mainPlayerCollider; // 플레이어의 주 Collider (몸체)
    public Collider2D reviveDetectionTrigger; // 부활 감지용 Trigger Collider (현재 코드에서는 직접 사용되지 않지만, 에디터 설정용으로 유지)

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

        if (mainPlayerCollider == null)
        {
            Debug.LogError("mainPlayerCollider가 할당되지 않았습니다. Player GameObject의 주 Collider를 여기에 드래그해주세요.");
            mainPlayerCollider = GetComponent<Collider2D>();
        }
        foreach (var otherPlayerComp in FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (otherPlayerComp == this) continue;

            if (mainPlayerCollider != null && otherPlayerComp.mainPlayerCollider != null)
            {
                Physics2D.IgnoreCollision(mainPlayerCollider, otherPlayerComp.mainPlayerCollider, true);
            }
            otherPlayer = otherPlayerComp;
        }

        if (reviveBar != null)
        {
            reviveBar.fillAmount = 0f;
            reviveImage.gameObject.SetActive(false);
        }
        if (m_ReloadImage != null)
        {
            m_ReloadImage.fillAmount = 0f;
            m_ReloadImage.gameObject.SetActive(false);
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

        InitializeWeaponStats();
    }

    void InitializeWeaponStats()
    {
        switch (currentWeaponType)
        {
            case WeaponType.DefaultGun:
                currentMaxBulletCount = defaultGunBulletMaxCount;
                currentFireRate = defaultGunShootCool;
                currentReloadTime = defaultGunReloadTime;
                break;
            case WeaponType.RocketLauncher:
                currentMaxBulletCount = rocketMaxCount;
                currentFireRate = rocketFireRate;
                currentReloadTime = rocketReloadTime;
                break;
            case WeaponType.VacuumCleaner:
                currentMaxBulletCount = 0;
                currentFireRate = 0;
                currentReloadTime = 0;
                break;
            // SawtoothGun 초기화 로직 추가
            case WeaponType.SawtoothGun:
                currentMaxBulletCount = sawtoothGunMaxCount;
                currentFireRate = sawtoothGunFireRate;
                currentReloadTime = sawtoothGunReloadTime;
                break;
        }
        currentBulletCount = currentMaxBulletCount;
        isReloading = false;
        currentShootTimer = 0;

        // vacuumImage 숨김 처리 추가 (무기 변경 시)
        if (vacuumImage != null)
        {
            vacuumImage.gameObject.SetActive(false);
        }
        UpdateBulletUI();
    }

    void Update()
    {
        if (isDead)
        {
            if (isOverlappingWithOther && otherPlayer != null && !otherPlayer.isDead)
            {
                if (reviveImage != null)
                    reviveImage.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    reviveProgress += 1f;
                    if (reviveBar != null)
                        reviveBar.fillAmount = reviveProgress / reviveRequired;
                }
                if (reviveProgress >= reviveRequired)
                {
                    Revive();
                }
            }
            else
            {
                if (reviveImage != null)
                {
                    reviveImage.gameObject.SetActive(false);
                    reviveBar.fillAmount = 0f;
                }
            }
            return;
        }

        // 살아있는 플레이어는 부활 UI 숨김 (죽은 플레이어 로직 위에 있으니 한번 더 확인)
        if (reviveImage != null)
            reviveImage.gameObject.SetActive(false);


        if (isReloading && m_ReloadImage != null)
        {
            reloadTimer += Time.deltaTime;
            m_ReloadImage.fillAmount = Mathf.Clamp01(reloadTimer / currentReloadTime);
            if (reloadTimer >= currentReloadTime)
            {
                m_ReloadImage.fillAmount = 1f;
            }
        }
        else if (m_ReloadImage != null && !isReloading)
        {
            m_ReloadImage.fillAmount = 0f;
        }

        if (vacuumImage != null)
        {
            if (currentWeaponType == WeaponType.VacuumCleaner && !isDead)
            {
                bool isActive = Input.GetKey(shootKey);
                vacuumImage.gameObject.SetActive(isActive);

                if (isActive)
                {
                    Vector3 scale = vacuumImage.rectTransform.localScale;
                    scale.x = (SpriteRenderer != null && SpriteRenderer.flipX) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                    vacuumImage.rectTransform.localScale = scale;

                    Vector3 pos = vacuumImage.rectTransform.localPosition;
                    float baseX = Mathf.Abs(pos.x);
                    pos.x = (SpriteRenderer != null && SpriteRenderer.flipX) ? -baseX : baseX;
                    vacuumImage.rectTransform.localPosition = pos;
                }
            }
            else
            {
                vacuumImage.gameObject.SetActive(false);
            }
        }

        bool wasGrounded = isGrounded;
        bool grounded1 = Physics2D.Raycast(groundCheck1.position, Vector2.down, groundCheckDistance, groundLayer);
        bool grounded2 = Physics2D.Raycast(groundCheck2.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = grounded1 || grounded2;

        if (!wasGrounded && isGrounded)
        {
            isDoubleJumpAvailable = true;
        }

        Move();
        Animation();

        if (currentShootTimer > 0)
        {
            currentShootTimer -= Time.deltaTime;
        }

        HandleWeaponInput();

        if (m_HpBar != null)
            m_HpBar.fillAmount = m_CurHp / m_MaxHp;

        UpdateBulletUI();
        TextHp.text = m_MaxHp.ToString("F0") + " / " + m_CurHp.ToString("F0");

        m_DamageCool -= Time.deltaTime;
        m_LavaCool -= Time.deltaTime;
    }

    void UpdateBulletUI()
    {
        if (BulletCount != null)
        {
            if (currentWeaponType == WeaponType.VacuumCleaner)
            {
                BulletCount.text = "∞";
            }
            else
            {
                BulletCount.text = currentBulletCount + " / " + currentMaxBulletCount;
            }
        }
    }

    void HandleWeaponInput()
    {
        if (isDead || isReloading) return;

        Vector2 shootDir = (SpriteRenderer != null && SpriteRenderer.flipX) ? Vector2.left : Vector2.right;

        switch (currentWeaponType)
        {
            case WeaponType.DefaultGun:
                if (Input.GetKey(shootKey) && currentShootTimer <= 0f && currentBulletCount > 0)
                {
                    FireDefaultGun(shootDir);
                }
                if (Input.GetKeyDown(reloadKey) || (currentBulletCount <= 0 && !isReloading))
                {
                    StartReload();
                }
                break;

            case WeaponType.RocketLauncher:
                if (Input.GetKeyDown(shootKey) && currentShootTimer <= 0f && currentBulletCount > 0)
                {
                    FireRocketLauncher(shootDir);
                }
                if (Input.GetKeyDown(reloadKey) || (currentBulletCount <= 0 && !isReloading))
                {
                    StartReload();
                }
                break;

            case WeaponType.VacuumCleaner:
                if (Input.GetKey(shootKey))
                {
                    OperateVacuumCleaner();
                }
                break;
            // SawtoothGun 발사 및 재장전 로직 추가
            case WeaponType.SawtoothGun:
                if (Input.GetKey(shootKey) && currentShootTimer <= 0f && currentBulletCount > 0)
                {
                    FireSawtoothGun(shootDir);
                }
                if (Input.GetKeyDown(reloadKey) || (currentBulletCount <= 0 && !isReloading))
                {
                    StartReload();
                }
                break;
        }
    }

    void FireDefaultGun(Vector2 direction)
    {
        if (defaultBulletPrefab != null && m_ShootPos != null)
        {
            GameObject bullet = Instantiate(defaultBulletPrefab, m_ShootPos.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = direction * defaultGunShootForce;
            }
            currentBulletCount--;
            currentShootTimer = defaultGunShootCool;

            if (currentBulletCount <= 0)
            {
                StartReload();
            }
        }
    }

    void FireRocketLauncher(Vector2 direction)
    {
        if (rocketPrefab != null && m_ShootPos != null)
        {
            GameObject rocket = Instantiate(rocketPrefab, m_ShootPos.position, Quaternion.identity);
            Rigidbody2D rocketRb = rocket.GetComponent<Rigidbody2D>();
            if (rocketRb != null)
            {
                rocketRb.linearVelocity = direction * rocketShootForce;
            }
            currentBulletCount--;
            currentShootTimer = rocketFireRate;

            if (currentBulletCount <= 0)
            {
                StartReload();
            }
        }
    }

    void OperateVacuumCleaner()
    {
        Vector2 forward = (SpriteRenderer != null && SpriteRenderer.flipX) ? Vector2.left : Vector2.right;
        float halfAngle = 30f;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(m_ShootPos.position, suckRadius, smallMonsterLayer);

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("SmallMonster"))
            {
                Vector2 toTarget = (hit.transform.position - m_ShootPos.position).normalized;
                float angle = Vector2.Angle(forward, toTarget);

                if (angle <= halfAngle)
                {
                    Rigidbody2D monsterRb = hit.GetComponent<Rigidbody2D>();
                    if (monsterRb != null)
                    {
                        Vector2 directionToPlayer = (m_ShootPos.position - hit.transform.position).normalized;
                        monsterRb.AddForce(directionToPlayer * suckForce, ForceMode2D.Force);

                        if (Vector2.Distance(m_ShootPos.position, hit.transform.position) < consumeDistance)
                        {
                            Destroy(hit.gameObject);
                        }
                    }
                }
            }
        }
    }

    // SawtoothGun 발사 함수 추가
    void FireSawtoothGun(Vector2 direction)
    {
        if (sawtoothBulletPrefab != null && m_ShootPos != null)
        {
            GameObject bladeBullet = Instantiate(sawtoothBulletPrefab, m_ShootPos.position, Quaternion.identity);
            Rigidbody2D bladeRb = bladeBullet.GetComponent<Rigidbody2D>();
            if (bladeRb != null)
            {
                bladeRb.linearVelocity = direction * sawtoothGunShootForce;
            }
            currentBulletCount--;
            currentShootTimer = sawtoothGunFireRate;

            if (currentBulletCount <= 0)
            {
                StartReload();
            }
        }
    }

    void StartReload()
    {
        if (currentWeaponType == WeaponType.VacuumCleaner) return;

        if (!isReloading && currentBulletCount < currentMaxBulletCount)
        {
            isReloading = true;
            reloadTimer = 0.0f;
            Invoke("ReloadComplete", currentReloadTime);
            Debug.Log("재장전 시작!");
            if (m_ReloadImage != null)
            {
                m_ReloadImage.gameObject.SetActive(true);
            }
        }
    }

    void ReloadComplete()
    {
        currentBulletCount = currentMaxBulletCount;
        isReloading = false;
        Debug.Log("재장전 완료!");
        if (m_ReloadImage != null)
        {
            m_ReloadImage.gameObject.SetActive(false);
            m_ReloadImage.fillAmount = 0f;
            reloadTimer = 0f;
        }
        UpdateBulletUI();
    }

    void Die()
    {
        isDead = true;
        m_CurHp = 0.0f;
        if (Anim != null)
            Anim.SetTrigger("Die");

        reviveProgress = 0f;
        if (reviveBar != null)
            reviveBar.fillAmount = 0f;

        TextHp.text = m_MaxHp.ToString("F0") + " / " + m_CurHp.ToString("F0");

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;

        if (GameMgr.Inst != null)
            GameMgr.Inst.OnPlayerDead();
    }

    void Revive()
    {
        isDead = false;
        m_CurHp = m_MaxHp * 0.5f;
        reviveProgress = 0f;
        if (reviveBar != null)
            reviveBar.fillAmount = 0f;
        if (reviveImage != null)
            reviveImage.gameObject.SetActive(false);
        if (m_Gun != null) m_Gun.SetActive(true); // 죽었을 때 총 비활성화 로직이 없어서 추가 안함. 필요시 Die()에 m_Gun.SetActive(false) 추가

        // 부활 시 Rigidbody2D 상태 복구
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;


        if (GameMgr.Inst != null)
            GameMgr.Inst.OnPlayerRevive();
    }

    void Move()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        h = 0.0f;
        if (Input.GetKey(leftKey)) h = -1.0f;
        if (Input.GetKey(rightKey)) h = 1.0f;
        rb.linearVelocity = new Vector2(h * m_MoveSpeed, rb.linearVelocity.y);

        if (Input.GetKeyDown(jumpKey))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, m_JumpForce);
                isDoubleJumpAvailable = true;
            }
            else if (isDoubleJumpAvailable)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, m_JumpForce);
                isDoubleJumpAvailable = false;
            }
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

    public void TakeDamage(float a_Value)
    {
        if (m_CurHp <= 0.0f)
            return;

        m_CurHp -= a_Value;
        if (m_CurHp < 0.0f)
            m_CurHp = 0.0f;

        ApplyKnockback();

        if (m_CurHp <= 0.0f)
        {
            Die();
        }
    }

    void ApplyKnockback()
    {
        if (rb == null) return;

        float dir = (SpriteRenderer != null && SpriteRenderer.flipX) ? 1f : -1f;
        Vector2 knockback = new Vector2(dir * knockbackForce, knockbackUpForce);
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("EnemyBullet"))
        {
            TakeDamage(10);
            Destroy(coll.gameObject);
        }
        else if (coll.CompareTag("Fire"))
        {
            TakeDamage(30);
        }
        else if (coll.CompareTag("Player1") && coll.gameObject != this.gameObject)
        {
            if (coll == otherPlayer?.mainPlayerCollider)
            {
                isOverlappingWithOther = true;
            }
        }
        else if (coll.CompareTag("Player2") && coll.gameObject != this.gameObject)
        {
            if (coll == otherPlayer?.mainPlayerCollider)
            {
                isOverlappingWithOther = true;
            }
        }
        if (coll.CompareTag("JumpBoost"))
        {
            m_JumpForce += 5.0f;
        }
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.CompareTag("Player1") && coll.gameObject != this.gameObject)
        {
            if (coll == otherPlayer?.mainPlayerCollider)
            {
                isOverlappingWithOther = false;
                reviveProgress = 0f;
                if (reviveBar != null)
                    reviveBar.fillAmount = 0f;
            }
        }
        if (coll.CompareTag("Player2") && coll.gameObject != this.gameObject)
        {
            if (coll == otherPlayer?.mainPlayerCollider)
            {
                isOverlappingWithOther = false;
                reviveProgress = 0f;
                if (reviveBar != null)
                    reviveBar.fillAmount = 0f;
            }
        }
        if (coll.CompareTag("JumpBoost"))
        {
            m_JumpForce -= 5.0f;
        }
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        if (m_LavaCool < 0 && coll.CompareTag("Lava"))
        {
            TakeDamage(10);
            m_LavaCool = 0.25f;
        }
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {

    }

    private void OnCollisionStay2D(Collision2D coll)
    {

    }

    private void OnCollisionExit2D(Collision2D coll)
    {
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (m_ShootPos != null && currentWeaponType == WeaponType.VacuumCleaner)
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(m_ShootPos.position, suckRadius);
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.1f);
            Gizmos.DrawSphere(m_ShootPos.position, suckRadius);
        }
    }
#endif
}