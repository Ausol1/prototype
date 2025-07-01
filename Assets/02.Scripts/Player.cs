using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public enum PlayerType { Player1, Player2 }
    public PlayerType playerType = PlayerType.Player1;

    // WeaponType에 Katana 추가
    public enum WeaponType { DefaultGun, RocketLauncher, VacuumCleaner, SawtoothGun, Katana }

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
    Vector3 m_DirVec; // 사용되지 않지만 원본 유지

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

    //--- 총 관련 변수 (카타나에서는 사용되지 않음)
    public GameObject m_BulletPrefab = null; // 사용되지 않음
    public Transform m_ShootPos; // 카타나 공격 시 팔의 위치 기준점
    public GameObject m_Gun = null; // 카타나 모델 할당에 사용 가능
    public Image m_ReloadImage = null; // 사용되지 않음
    float reloadTimer = 0.0f; // 사용되지 않음
    public float shootForce = 10.0f; // 사용되지 않음
    public float m_ShootCool = 0.5f; // 사용되지 않음
    float ShootTimer = 0.0f; // 사용되지 않음

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

    // --- 각 총 종류별 스탯 (SawtoothGun) ---
    [Header("Sawtooth Gun Stats")]
    public GameObject sawtoothBulletPrefab;
    public float sawtoothGunShootForce = 12.0f;
    public float sawtoothGunFireRate = 0.7f;
    public int sawtoothGunMaxCount = 5;
    public float sawtoothGunReloadTime = 1.8f;

    // --- 카타나 스탯 및 관련 변수 추가 ---
    [Header("Katana Stats")]
    public float katanaAttackCooldown = 0.7f; // 공격 후 쿨타임
    public Collider2D katanaAttackCollider; // 카타나의 공격 범위 Collider (Is Trigger 체크)
    private bool isAttacking = false; // 카타나 공격 중인지 여부 (애니메이션 이벤트로 제어)

    // --- 현재 무기 상태 ---
    private float currentShootTimer = 0.0f; // 총기류 쿨타임, 카타나 공격 쿨타임에 재활용
    private int currentBulletCount; // 카타나에서는 사용되지 않음
    private bool isReloading = false; // 카타나에서는 사용되지 않음
    private int currentMaxBulletCount; // 카타나에서는 사용되지 않음
    private float currentReloadTime; // 카타나에서는 사용되지 않음
    private float currentFireRate; // 카타나에서는 AttackCooldown으로 재활용

    [Header("Weapon Configuration")]
    public WeaponType currentWeaponType;

    public TextMeshProUGUI BulletCount; // 카타나에서는 "∞" 또는 "---" 표시

    // --- 부활 관련 변수 추가 ---
    public bool isDead = false;
    private bool isBeingRevived = false; // 사용되지 않지만 원본 유지
    private float reviveProgress = 0f;
    public float reviveRequired = 10f; // 스페이스 연타 10회 필요
    private Player otherPlayer;
    private bool isOverlappingWithOther = false;

    // 부활 UI
    public Image reviveImage;
    public Image reviveBar;

    // Collider 참조 (죽은 플레이어끼리 충돌 무시, 다른 플레이어 감지용)
    public Collider2D mainPlayerCollider;
    public Collider2D reviveDetectionTrigger; // 현재 코드에서는 직접 사용되지 않지만, 에디터 설정용으로 유지

    //---애니메이션 관련 변수
    SpriteRenderer SpriteRenderer;
    Animator Anim;

    //--- 입력 키 설정
    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode jumpKey;
    private KeyCode shootKey; // 카타나 공격 키로 사용
    private KeyCode reloadKey; // 카타나에서는 사용되지 않음

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
        if (vacuumImage != null)
        {
            vacuumImage.gameObject.SetActive(false);
        }

        // 카타나 공격 콜라이더 초기 비활성화
        if (katanaAttackCollider != null)
        {
            katanaAttackCollider.enabled = false;
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
        // 애니메이션 초기화 (안전하게 모든 관련 Bool을 false로)
        if (Anim != null)
        {
            Anim.SetBool("IsKatanaEquipped", false); // 항상 초기화 시 false로 설정
            Anim.SetFloat("Speed", 0);
            Anim.SetBool("speed", true);
        }

        switch (currentWeaponType)
        {
            case WeaponType.DefaultGun:
                currentMaxBulletCount = defaultGunBulletMaxCount;
                currentFireRate = defaultGunShootCool;
                currentReloadTime = defaultGunReloadTime;
                // 총기류 모델 활성화
                if (m_Gun != null) m_Gun.SetActive(true);
                break;
            case WeaponType.RocketLauncher:
                currentMaxBulletCount = rocketMaxCount;
                currentFireRate = rocketFireRate;
                currentReloadTime = rocketReloadTime;
                if (m_Gun != null) m_Gun.SetActive(true);
                break;
            case WeaponType.VacuumCleaner:
                currentMaxBulletCount = 0;
                currentFireRate = 0;
                currentReloadTime = 0;
                if (m_Gun != null) m_Gun.SetActive(true);
                break;
            case WeaponType.SawtoothGun:
                currentMaxBulletCount = sawtoothGunMaxCount;
                currentFireRate = sawtoothGunFireRate;
                currentReloadTime = sawtoothGunReloadTime;
                if (m_Gun != null) m_Gun.SetActive(true);
                break;
            case WeaponType.Katana:
                // 카타나는 총기 관련 변수 사용 안함
                currentMaxBulletCount = 0;
                currentFireRate = katanaAttackCooldown; // 카타나 공격 쿨타임으로 사용
                currentReloadTime = 0; // 재장전 없음
                // 카타나 장착 시 m_Gun (카타나 모델) 활성화
                if (m_Gun != null) m_Gun.SetActive(true);
                // 카타나 장착 시 애니메이터 파라미터 설정
                if (Anim != null)
                {
                    Anim.SetBool("IsKatanaEquipped", true);
                }
                break;
        }
        currentBulletCount = currentMaxBulletCount;
        isReloading = false;
        isAttacking = false; // 카타나 공격 상태 초기화
        currentShootTimer = 0;

        if (vacuumImage != null)
        {
            vacuumImage.gameObject.SetActive(false);
        }
        if (m_ReloadImage != null) // 재장전 이미지도 무기 변경 시 숨김
        {
            m_ReloadImage.gameObject.SetActive(false);
        }
        // 무기가 카타나가 아닐 때 카타나 공격 콜라이더 비활성화
        if (katanaAttackCollider != null && currentWeaponType != WeaponType.Katana)
        {
            katanaAttackCollider.enabled = false;
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

        // 살아있는 플레이어는 부활 UI 숨김
        if (reviveImage != null)
            reviveImage.gameObject.SetActive(false);

        if (currentWeaponType != WeaponType.Katana) // 카타나는 재장전 UI 사용 안함
        {
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
        }
        else // 카타나를 들었을 때 재장전 이미지는 항상 꺼져있도록 함
        {
            if (m_ReloadImage != null)
            {
                m_ReloadImage.gameObject.SetActive(false);
            }
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
        Animation(); // 애니메이션 함수 호출

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
            if (currentWeaponType == WeaponType.VacuumCleaner || currentWeaponType == WeaponType.Katana) // 카타나도 무한대
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
        if (isDead || isReloading) return; // 카타나 공격 중에도 다른 액션 허용 (isAttacking 체크 제거)

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
            // 카타나 공격 로직 추가
            case WeaponType.Katana:
                // isAttacking 체크를 추가하여 공격 애니메이션 중 중복 공격 방지
                if (Input.GetKeyDown(shootKey) && currentShootTimer <= 0f && !isAttacking)
                {
                    AttackKatana();
                }
                // 카타나는 재장전 키 사용 안함
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

    // 카타나 공격 함수 (애니메이션 트리거만 발동)
    void AttackKatana()
    {
        currentShootTimer = katanaAttackCooldown; // 공격 쿨타임 설정

        if (Anim != null)
        {
            Anim.SetTrigger("KatanaAttack"); // 카타나 공격 애니메이션 트리거
        }
        // Collider 활성화/비활성화는 애니메이션 이벤트에서 처리됩니다.
    }

    // 애니메이션 이벤트용 함수: 카타나 공격 콜라이더 활성화
    public void EnableKatanaCollider()
    {
        if (katanaAttackCollider != null)
        {
            katanaAttackCollider.enabled = true;
            isAttacking = true; // 공격 애니메이션 시작과 함께 플래그 설정
            // Debug.Log("카타나 공격 콜라이더 활성화!");
        }
    }

    // 애니메이션 이벤트용 함수: 카타나 공격 콜라이더 비활성화
    public void DisableKatanaCollider()
    {
        if (katanaAttackCollider != null)
        {
            katanaAttackCollider.enabled = false;
            isAttacking = false; // 공격 애니메이션 종료와 함께 플래그 해제
            // Debug.Log("카타나 공격 콜라이더 비활성화!");
        }
    }

    void StartReload()
    {
        if (currentWeaponType == WeaponType.VacuumCleaner || currentWeaponType == WeaponType.Katana) return; // 청소기, 카타나는 재장전 없음

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
        if (m_Gun != null) m_Gun.SetActive(true);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;

        if (GameMgr.Inst != null)
            GameMgr.Inst.OnPlayerRevive();
    }

    void Move()
    {
        // 죽었거나 카타나 공격 애니메이션 중에는 이동 불가 (isAttacking은 애니메이션 이벤트를 통해 설정)
        // Anim.GetCurrentAnimatorStateInfo(0).IsName("KatanaAttack") 로 현재 재생 중인 애니메이션 상태를 직접 확인
        if (isDead || (currentWeaponType == WeaponType.Katana && Anim.GetCurrentAnimatorStateInfo(0).IsName("KatanaAttack")))
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

    // 애니메이션 제어 함수
    void Animation()
    {
        // 죽었거나 카타나 공격 애니메이션이 재생 중일 때는 다른 애니메이션을 업데이트하지 않습니다.
        if (isDead || (currentWeaponType == WeaponType.Katana && Anim.GetCurrentAnimatorStateInfo(0).IsName("KatanaAttack")))
        {
            // 필요하다면 여기에서 Speed와 speed 파라미터를 강제로 정지 상태로 설정할 수 있습니다.
            // Anim.SetFloat("Speed", 0);
            // Anim.SetBool("speed", true);
            return;
        }

        // 기존의 Speed와 speed 파라미터를 사용하여 Idle/Walk를 제어합니다.
        Anim.SetFloat("Speed", Mathf.Abs(h));
        bool t = h == 0.0f; // h가 0일 때 true (정지), 아니면 false (움직임)
        Anim.SetBool("speed", t);

        // --- 추가: 카타나 장착 여부 애니메이터 파라미터 설정 ---
        // 이 파라미터는 애니메이터에서 Base Layer 내의 상태 전환에 사용됩니다.
        if (Anim != null)
        {
            Anim.SetBool("IsKatanaEquipped", currentWeaponType == WeaponType.Katana);
        }

        // 방향 플립 로직 (기존과 동일)
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

    // WeaponType을 변경하는 공용 함수 (외부 스크립트에서 호출 가능)
    public void ChangeWeapon(WeaponType newWeapon)
    {
        currentWeaponType = newWeapon;
        CancelInvoke("ReloadComplete"); // 혹시 모를 재장전 Invoke 취소
        InitializeWeaponStats(); // 새로운 무기에 맞춰 스탯 및 UI 초기화
        Debug.Log($"무기 변경: {newWeapon}");
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

        // 카타나 공격 중 "Vine" 태그에 닿았을 때
        // isAttacking 플래그는 애니메이션 이벤트를 통해 정확하게 제어됩니다.
        if (currentWeaponType == WeaponType.Katana && isAttacking)
        {
            if (coll.CompareTag("Vine"))
            {
                // Debug.Log($"카타나로 덩굴 '{coll.gameObject.name}'을 잘랐습니다!");
                // VineCtrl을 통해 자르는 로직 호출 (가정)
                // coll.GetComponent<VineCtrl>()?.CutVine(); // 실제 VineCtrl이 있다면 이렇게 호출
                Destroy(coll.gameObject); // 테스트용으로 직접 파괴
            }
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
        // 진공청소기 범위 시각화 (파란색 반투명 원)
        if (m_ShootPos != null && currentWeaponType == WeaponType.VacuumCleaner)
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(m_ShootPos.position, suckRadius);
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.1f);
            Gizmos.DrawSphere(m_ShootPos.position, suckRadius);
        }
        // 카타나 공격 범위 시각화 Gizmos는 콜라이더 직접 사용 방식으로 인해 제거되었습니다.
    }
#endif
}