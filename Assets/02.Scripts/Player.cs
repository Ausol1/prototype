using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Coroutine을 사용하기 위해 추가

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

    [Header("Layer Settings")]
    public string playerAliveLayerName = "Player"; // 살아있을 때의 레이어 이름
    public string playerDeadLayerName = "Player_Dead"; // 죽었을 때의 레이어 이름

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackUpForce = 3f;

    //--- 총 관련 변수 (카타나에서는 사용되지 않음)
    public GameObject m_BulletPrefab = null;
    public Transform m_ShootPos; // 총알/흡입 효과가 시작될 위치
    public GameObject m_Gun = null;
    public Image m_ReloadImage = null;
    float reloadTimer = 0.0f;
    public float shootForce = 10.0f;
    public float m_ShootCool = 0.5f;
    float ShootTimer = 0.0f;

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
    // 흡수 이미지를 가지고 있고 판정을 하는 게임 오브젝트 (VacuumObject)
    public GameObject vacuumObject; // <--- 변경: 이제 이 GameObject가 흡수 이미지와 콜라이더를 가짐
    public float suckRadius = 3f; // 흡입 범위 (Gizmo 용도로 유지, 실제 콜라이더 크기로 조절)
    public float suckForce = 10f; // 흡입력
    public float consumeDistance = 0.5f; // 소멸 거리
    public LayerMask smallMonsterLayer; // 흡수할 대상 레이어

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
    public float katanaColliderActiveDuration = 0.2f; // 카타나 콜라이더가 활성화될 지속 시간
    public Collider2D katanaAttackCollider; // 카타나의 공격 범위 Collider (Is Trigger 체크)
    private bool isAttacking = false; // 카타나 공격 중인지 여부 (코드로 제어)

    // --- 현재 무기 상태 ---
    private float currentShootTimer = 0.0f;
    private int currentBulletCount;
    private bool isReloading = false;
    private int currentMaxBulletCount;
    private float currentReloadTime;
    private float currentFireRate;

    [Header("Weapon Configuration")]
    public WeaponType currentWeaponType;

    public TextMeshProUGUI BulletCount;

    // --- 부활 관련 변수 추가 ---
    public bool isDead = false;
    private bool isBeingRevived = false;
    bool isboom = false;
    private float reviveProgress = 0f;
    public float reviveRequired = 10f;
    private Player otherPlayer;
    private bool isOverlappingWithOther = false;
    private Coroutine blinkCoroutine; // 깜빡임 코루틴 핸들

    // 부활 UI
    public Image reviveImage;
    public Image reviveBar;

    // Collider 참조
    public Collider2D mainPlayerCollider;
    public Collider2D reviveDetectionTrigger; // 부활 감지용 트리거

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
            Debug.LogError($"[{playerType}] mainPlayerCollider가 할당되지 않았습니다. Player GameObject의 주 Collider를 여기에 드래그해주세요.");
            mainPlayerCollider = GetComponent<Collider2D>();
        }
        foreach (var otherPlayerComp in FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (otherPlayerComp == this) continue;

            if (mainPlayerCollider != null && otherPlayerComp.mainPlayerCollider != null)
            {
                // 두 플레이어의 메인 콜라이더는 서로 무시
                Physics2D.IgnoreCollision(mainPlayerCollider, otherPlayerComp.mainPlayerCollider, true);
                Debug.Log($"[{playerType}] {otherPlayerComp.playerType}와(과) 메인 콜라이더 충돌 무시 설정.");
            }
            otherPlayer = otherPlayerComp;
            Debug.Log($"[{playerType}] 다른 플레이어 ({otherPlayer.playerType}) 참조 설정 완료.");
        }

        if (reviveBar != null)
        {
            reviveBar.fillAmount = 0f;
            reviveImage.gameObject.SetActive(false);
            Debug.Log($"[{playerType}] reviveBar 및 reviveImage 초기화 완료.");
        }
        if (m_ReloadImage != null)
        {
            m_ReloadImage.fillAmount = 0f;
            m_ReloadImage.gameObject.SetActive(false);
        }

        // VacuumObject 초기 비활성화
        if (vacuumObject != null) // <--- 변경: vacuumObject 사용
        {
            vacuumObject.SetActive(false);
            Debug.Log($"[{playerType}] VacuumObject 초기 비활성화.");
        }


        // 카타나 공격 콜라이더 초기 비활성화
        if (katanaAttackCollider != null)
        {
            katanaAttackCollider.enabled = false;
            Debug.Log($"[{playerType}] KatanaAttackCollider 초기 비활성화.");
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
        Debug.Log($"[{playerType}] 입력 키 설정 완료.");

        InitializeWeaponStats();
        Debug.Log($"[{playerType}] Start 함수 종료.");
    }

    void InitializeWeaponStats()
    {
        if (Anim != null)
        {
            Anim.SetBool("IsKatanaEquipped", false);
            Anim.SetFloat("Speed", 0);
            Anim.SetBool("speed", true);
        }

        switch (currentWeaponType)
        {
            case WeaponType.DefaultGun:
                currentMaxBulletCount = defaultGunBulletMaxCount;
                currentFireRate = defaultGunShootCool;
                currentReloadTime = defaultGunReloadTime;
                if (m_Gun != null) m_Gun.SetActive(true);
                break;
            case WeaponType.RocketLauncher:
                currentMaxBulletCount = rocketMaxCount;
                currentFireRate = rocketFireRate;
                currentReloadTime = rocketReloadTime;
                if (m_Gun != null) m_Gun.SetActive(true);
                break;
            case WeaponType.VacuumCleaner:
                currentMaxBulletCount = 0; // 진공청소기는 총알 개념 없음
                currentFireRate = 0; // 쿨타임 개념 없음
                currentReloadTime = 0; // 재장전 개념 없음
                if (m_Gun != null) m_Gun.SetActive(true); // 총 모델은 필요하다면 활성화
                break;
            case WeaponType.SawtoothGun:
                currentMaxBulletCount = sawtoothGunMaxCount;
                currentFireRate = sawtoothGunFireRate;
                currentReloadTime = sawtoothGunReloadTime;
                if (m_Gun != null) m_Gun.SetActive(true);
                break;
            case WeaponType.Katana:
                currentMaxBulletCount = 0;
                currentFireRate = katanaAttackCooldown;
                currentReloadTime = 0;
                if (m_Gun != null) m_Gun.SetActive(true);
                if (Anim != null)
                {
                    Anim.SetBool("IsKatanaEquipped", true);
                }
                break;
        }
        currentBulletCount = currentMaxBulletCount;
        isReloading = false;
        isAttacking = false;
        currentShootTimer = 0;

        // VacuumObject 비활성화
        if (vacuumObject != null)
        {
            vacuumObject.SetActive(false);
        }
        if (m_ReloadImage != null)
        {
            m_ReloadImage.gameObject.SetActive(false);
        }
        if (katanaAttackCollider != null && currentWeaponType != WeaponType.Katana)
        {
            katanaAttackCollider.enabled = false;
        }
        UpdateBulletUI();
        Debug.Log($"[{playerType}] 무기 스탯 초기화 완료. 현재 무기: {currentWeaponType}");
    }

    void Update()
    {

        if (isDead)
        {
            if (isOverlappingWithOther && otherPlayer != null && !otherPlayer.isDead)
            {
                if (reviveImage != null && !reviveImage.gameObject.activeSelf)
                {
                    reviveImage.gameObject.SetActive(true);
                    Debug.Log($"[{playerType}] 죽은 플레이어 ({playerType}) 근처에 다른 플레이어({otherPlayer.playerType}) 진입, reviveImage 활성화 시도. isOverlappingWithOther: {isOverlappingWithOther}");
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    reviveProgress += 1f;
                    if (reviveBar != null)
                        reviveBar.fillAmount = reviveProgress / reviveRequired;
                    Debug.Log($"[{playerType}] Space bar pressed. reviveProgress: {reviveProgress}/{reviveRequired}");
                }
                if (reviveProgress >= reviveRequired)
                {
                    Revive();
                    Debug.Log($"[{playerType}] 부활 진행도 충족. Revive() 호출됨.");
                }
            }
            else // 다른 플레이어가 근처에 없거나, 다른 플레이어가 죽었을 때
            {
                if (reviveImage != null && reviveImage.gameObject.activeSelf)
                {
                    reviveImage.gameObject.SetActive(false);
                    reviveBar.fillAmount = 0f;
                    Debug.Log($"[{playerType}] 다른 플레이어 ({otherPlayer?.playerType})가 근처에 없거나 죽었음. reviveImage 비활성화.");
                }
            }

            // 죽은 상태에서는 무기 및 이동 관련 로직을 스킵
            return;
        }

        // isDead가 false일 때만 실행되는 부분
        if (currentWeaponType != WeaponType.Katana)
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
        else
        {
            if (m_ReloadImage != null)
            {
                m_ReloadImage.gameObject.SetActive(false);
            }
        }

        // VacuumObject 활성화/비활성화 및 위치/스케일 조정
        if (vacuumObject != null) // <--- VacuumObject를 직접 사용
        {
            if (currentWeaponType == WeaponType.VacuumCleaner && !isDead)
            {
                bool isActive = Input.GetKey(shootKey);
                vacuumObject.SetActive(isActive); // <--- VacuumObject 활성화/비활성화

                if (isActive)
                {
                    // VacuumObject의 스케일 (방향 뒤집기)
                    Vector3 scale = vacuumObject.transform.localScale;
                    scale.x = (SpriteRenderer != null && SpriteRenderer.flipX) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                    vacuumObject.transform.localScale = scale;

                    // VacuumObject의 위치 (m_ShootPos에 따라) - m_ShootPos의 자식이라면 이 코드는 불필요함
                    // vacuumObject.transform.position = m_ShootPos.position;
                }
            }
            else
            {
                vacuumObject.SetActive(false);
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
            if (currentWeaponType == WeaponType.VacuumCleaner || currentWeaponType == WeaponType.Katana)
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
                // 흡수 로직은 OnTriggerStay2D에서 직접 처리하므로 여기서는 Input.GetKey(shootKey)만 감지합니다.
                // Input.GetKey(shootKey)가 true일 때 vacuumObject가 활성화되므로 별도의 호출이 필요 없습니다.
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
            case WeaponType.Katana:
                if (Input.GetKeyDown(shootKey) && currentShootTimer <= 0f && !isAttacking)
                {
                    AttackKatana();
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

    void AttackKatana()
    {
        currentShootTimer = katanaAttackCooldown;

        if (Anim != null)
        {
            Anim.SetTrigger("KatanaAttack");
        }

        StartCoroutine(KatanaAttackRoutine());
    }

    IEnumerator KatanaAttackRoutine()
    {
        isAttacking = true;
        if (katanaAttackCollider != null)
        {
            katanaAttackCollider.enabled = true;
            Debug.Log($"[{playerType}] 카타나 공격 콜라이더 활성화!");
        }

        yield return new WaitForSeconds(katanaColliderActiveDuration);

        if (katanaAttackCollider != null)
        {
            katanaAttackCollider.enabled = false;
            Debug.Log($"[{playerType}] 카타나 공격 콜라이더 비활성화!");
        }
        isAttacking = false;
    }

    void StartReload()
    {
        if (currentWeaponType == WeaponType.VacuumCleaner || currentWeaponType == WeaponType.Katana) return;

        if (!isReloading && currentBulletCount < currentMaxBulletCount)
        {
            isReloading = true;
            reloadTimer = 0.0f;
            Invoke("ReloadComplete", currentReloadTime);
            Debug.Log($"[{playerType}] 재장전 시작!");
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
        Debug.Log($"[{playerType}] 재장전 완료!");
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
        Debug.Log($"[{playerType}] Die() 함수 호출됨. 현재 HP: {m_CurHp}");
        isDead = true;
        m_CurHp = 0.0f;
        if (Anim != null)
            Anim.SetTrigger("Die");

        reviveProgress = 0f;
        if (reviveBar != null)
            reviveBar.fillAmount = 0f;

        TextHp.text = m_MaxHp.ToString("F0") + " / " + m_CurHp.ToString("F0");

        // --- 사망 시 물리적 처리 (낙하 및 충돌 레이어 변경) ---
        rb.linearVelocity = Vector2.zero; // 현재 선형 속도를 0으로 초기화
        rb.angularVelocity = 0f;        // 현재 각속도를 0으로 초기화
        rb.simulated = true; // Rigidbody2D 시뮬레이션 활성화 (Dynamic일 경우 기본적으로 true)
        Debug.Log($"[{playerType}] Rigidbody2D bodyType을 Dynamic으로 유지. Simulated 상태: {rb.simulated}");

        // 죽었을 때 플레이어 레이어를 변경하여 몬스터와 충돌하지 않도록 함
        gameObject.layer = LayerMask.NameToLayer(playerDeadLayerName);
        Debug.Log($"[{playerType}] 레이어를 {playerDeadLayerName}로 변경했습니다.");

        // 메인 콜라이더는 활성 상태 유지 (바닥 충돌을 위해)
        if (mainPlayerCollider != null)
        {
            mainPlayerCollider.enabled = true; // 이 부분은 원래대로 활성화 유지 (핵심 변경!)
            Debug.Log($"[{playerType}] mainPlayerCollider 활성화 상태 유지.");
        }

        // 부활 감지 트리거는 활성화 유지
        if (reviveDetectionTrigger != null)
        {
            reviveDetectionTrigger.enabled = true;
            Debug.Log($"[{playerType}] reviveDetectionTrigger 활성화 상태: {reviveDetectionTrigger.enabled}");
        }

        // --- 사망 시 모든 관련 이벤트 중단 ---
        CancelInvoke("ReloadComplete");
        isReloading = false;
        if (m_ReloadImage != null)
        {
            m_ReloadImage.gameObject.SetActive(false);
            m_ReloadImage.fillAmount = 0f;
            reloadTimer = 0f;
        }
        if (vacuumObject != null)
        {
            vacuumObject.SetActive(false);
        }
        isAttacking = false;
        if (katanaAttackCollider != null)
        {
            katanaAttackCollider.enabled = false;
        }

        if (GameMgr.Inst != null)
            GameMgr.Inst.OnPlayerDead();

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            Debug.Log($"[{playerType}] 기존 blinkCoroutine 중지.");
        }
        blinkCoroutine = StartCoroutine(BlinkOnDeath());
        Debug.Log($"[{playerType}] BlinkOnDeath 코루틴 시작됨.");
    }

    void Revive()
    {
        Debug.Log($"[{playerType}] Revive() 함수 호출됨. 현재 isDead: {isDead}");
        isDead = false;
        m_CurHp = m_MaxHp * 0.5f;
        reviveProgress = 0f;
        isboom = false;
        Debug.Log($"[{playerType}] isboom 리셋. isboom: {isboom}");

        if (reviveBar != null)
            reviveBar.fillAmount = 0f;
        if (reviveImage != null)
            reviveImage.gameObject.SetActive(false);
        if (m_Gun != null) m_Gun.SetActive(true);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        Debug.Log($"[{playerType}] Rigidbody2D bodyType을 Dynamic으로 설정. Simulated 상태: {rb.simulated}");

        // 부활 시 원래 플레이어 레이어로 되돌림
        gameObject.layer = LayerMask.NameToLayer(playerAliveLayerName);
        Debug.Log($"[{playerType}] 레이어를 {playerAliveLayerName}로 되돌렸습니다.");

        if (mainPlayerCollider != null)
        {
            mainPlayerCollider.enabled = true;
            Debug.Log($"[{playerType}] mainPlayerCollider 다시 활성화.");
        }

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            Debug.Log($"[{playerType}] blinkCoroutine 정지.");
        }
        SetSpriteAlpha(1f);
        Debug.Log($"[{playerType}] 스프라이트 알파 1로 복구.");

        if (GameMgr.Inst != null)
            GameMgr.Inst.OnPlayerRevive();
        Debug.Log($"[{playerType}] Revive() 함수 종료. isDead: {isDead}, m_CurHp: {m_CurHp}");
    }
    void SetSpriteAlpha(float alpha)
    {
        if (SpriteRenderer != null)
        {
            Color c = SpriteRenderer.color;
            c.a = alpha;
            SpriteRenderer.color = c;
        }
        else
        {
            Debug.LogError($"[{playerType}] SpriteRenderer가 null입니다! 알파 값을 설정할 수 없습니다.");
        }
    }

    IEnumerator BlinkOnDeath()
    {
        Debug.Log($"[{playerType}] BlinkOnDeath 코루틴 시작. isDead: {isDead}");
        while (isDead)
        {
            SetSpriteAlpha(0.3f);
            yield return new WaitForSeconds(0.15f);
            SetSpriteAlpha(1f);
            yield return new WaitForSeconds(0.15f);
        }
        Debug.Log($"[{playerType}] BlinkOnDeath 코루틴 종료. isDead: {isDead}");
    }

    void Move()
    {
        if (isDead || isAttacking)
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
                Debug.Log($"[{playerType}] 첫 점프. isGrounded: {isGrounded}");
            }
            else if (isDoubleJumpAvailable)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, m_JumpForce);
                isDoubleJumpAvailable = false;
                Debug.Log($"[{playerType}] 더블 점프. isDoubleJumpAvailable: {isDoubleJumpAvailable}");
            }
        }
    }

    void Animation()
    {
        if (isDead || isAttacking)
        {
            Anim.SetFloat("Speed", 0);
            Anim.SetBool("speed", true);
            return;
        }

        Anim.SetFloat("Speed", Mathf.Abs(h));
        bool t = h == 0.0f;
        Anim.SetBool("speed", t);

        if (Anim != null)
        {
            Anim.SetBool("IsKatanaEquipped", currentWeaponType == WeaponType.Katana);
        }

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

            // VacuumObject의 방향도 플레이어 방향에 따라 뒤집기
            if (vacuumObject != null && currentWeaponType == WeaponType.VacuumCleaner)
            {
                Vector3 vacuumScale = vacuumObject.transform.localScale;
                vacuumScale.x = h > 0f ? Mathf.Abs(vacuumScale.x) : -Mathf.Abs(vacuumScale.x);
                vacuumObject.transform.localScale = vacuumScale;

                // m_ShootPos가 부모가 아니라면 위치도 조정해야 합니다.
                // vacuumObject.transform.position = m_ShootPos.position;
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

        Debug.Log($"[{playerType}] 데미지 {a_Value} 받음. 현재 HP: {m_CurHp}");
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
        Debug.Log($"[{playerType}] 넉백 적용. 방향: {dir}");
    }

    public void ChangeWeapon(WeaponType newWeapon)
    {
        currentWeaponType = newWeapon;
        CancelInvoke("ReloadComplete");
        InitializeWeaponStats();
        Debug.Log($"[{playerType}] 무기 변경: {newWeapon}");
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log($"[{playerType}] OnTriggerEnter2D 진입: {coll.gameObject.name}, Tag: {coll.tag}");

        if (coll.CompareTag("EnemyBullet"))
        {
            TakeDamage(10);
            Destroy(coll.gameObject);
        }
        else if (coll.CompareTag("Fire"))
        {
            TakeDamage(30);
        }
        else if (coll == otherPlayer?.reviveDetectionTrigger)
        {
            if (isDead)
            {
                isOverlappingWithOther = true;
                Debug.Log($"[{playerType}] 다른 플레이어({otherPlayer.playerType})의 reviveDetectionTrigger가 나({playerType})를 감지! isOverlappingWithOther: {isOverlappingWithOther}");
                if (reviveImage != null)
                {
                    reviveImage.gameObject.SetActive(true);
                    reviveBar.fillAmount = reviveProgress / reviveRequired;
                }
            }
        }
        else if (coll.CompareTag("JumpBoost"))
        {
            m_JumpForce += 5.0f;
            Debug.Log($"[{playerType}] JumpBoost 획득. 점프력: {m_JumpForce}");
        }
        else if (currentWeaponType == WeaponType.Katana && isAttacking)
        {
            if (coll.CompareTag("BlockVine"))
            {
                Destroy(coll.gameObject, 0.2f);
            }
        }
        else if (coll.CompareTag("Tentacle"))
        {
            TakeDamage(20);
        }
        else if (coll.CompareTag("Boom") && !isboom)
        {
            TakeDamage(30);
            isboom = true;
            Debug.Log($"[{playerType}] Boom과 충돌! isboom: {isboom}");
        }
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        Debug.Log($"[{playerType}] OnTriggerExit2D 진입: {coll.gameObject.name}, Tag: {coll.tag}");

        if (coll == otherPlayer?.reviveDetectionTrigger)
        {
            isOverlappingWithOther = false;
            reviveProgress = 0f;
            if (reviveBar != null)
                reviveBar.fillAmount = 0f;
            if (reviveImage != null)
                reviveImage.gameObject.SetActive(false);
            Debug.Log($"[{playerType}] 다른 플레이어({otherPlayer.playerType})의 reviveDetectionTrigger에서 벗어남. isOverlappingWithOther: {isOverlappingWithOther}");
        }
        else if (coll.CompareTag("JumpBoost"))
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
            Debug.Log($"[{playerType}] 용암에 의해 데미지 받음.");
        }
        else if (coll.CompareTag("MiddleBoss"))
        {
            TakeDamage(1);
            isDoubleJumpAvailable = true;
            Debug.Log($"[{playerType}] MiddleBoss와 충돌.");
        }
        // Vacuum Cleaner 흡수 로직 (Player 스크립트에서 직접 처리)
        // Vacuum Cleaner 흡수 로직
        else if (currentWeaponType == WeaponType.VacuumCleaner && vacuumObject != null && vacuumObject.activeSelf)
        {
            if (((1 << coll.gameObject.layer) & smallMonsterLayer) != 0)
            {
                if (coll.CompareTag("SmallMonster"))
                {
                    Rigidbody2D monsterRb = coll.GetComponent<Rigidbody2D>();
                    if (monsterRb != null)
                    {
                        // 플레이어의 ShootPos 방향으로 끌어당김
                        Vector2 directionToPlayer = (m_ShootPos.position - coll.transform.position).normalized;
                        // 적용될 힘의 크기 계산
                        Vector2 forceToApply = directionToPlayer * suckForce * Time.deltaTime; // Time.deltaTime을 곱하는 이유 확인

                        monsterRb.AddForce(forceToApply, ForceMode2D.Force);

                        // --- 디버그 로그 추가 ---
                        Debug.Log($"[Vacuum] 몬스터({coll.gameObject.name}) 감지됨.");
                        Debug.Log($"[Vacuum] ShootPos: {m_ShootPos.position}, 몬스터 위치: {coll.transform.position}");
                        Debug.Log($"[Vacuum] 흡입 방향: {directionToPlayer}, 적용 힘: {forceToApply}");
                        Debug.Log($"[Vacuum] 몬스터 질량: {monsterRb.mass}, 현재 속도: {monsterRb.linearVelocity}");
                        // --------------------------

                        // 특정 거리 안에 들어오면 소멸
                        if (Vector2.Distance(m_ShootPos.position, coll.transform.position) < consumeDistance)
                        {
                            Destroy(coll.gameObject);
                            Debug.Log($"[Vacuum] 몬스터({coll.gameObject.name}) 흡수됨. 소멸 거리({consumeDistance}) 도달.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Vacuum] 몬스터 ({coll.gameObject.name})에 Rigidbody2D 컴포넌트가 없습니다!");
                    }
                }
            }
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
    // 에디터에서 흡수 범위를 시각적으로 표시
    void OnDrawGizmosSelected()
    {
        if (m_ShootPos != null && currentWeaponType == WeaponType.VacuumCleaner)
        {
            // Gizmo는 이제 vacuumObject의 콜라이더 크기에 맞춰 그리는 것이 좋습니다.
            // 하지만 여전히 suckRadius를 변수로 가지고 있으니 이를 활용할 수도 있습니다.
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(m_ShootPos.position, suckRadius); // 기존 suckRadius Gizmo

            // VacuumObject의 콜라이더가 있다면 그 콜라이더 크기에 맞게 그릴 수도 있습니다.
            if (vacuumObject != null)
            {
                Collider2D collider = vacuumObject.GetComponent<Collider2D>();
                if (collider != null)
                {
                    Gizmos.color = Color.green; // VacuumObject 콜라이더를 녹색으로 표시
                    if (collider is CircleCollider2D circleCollider)
                    {
                        // VacuumObject의 월드 포지션과 콜라이더 오프셋을 고려
                        Gizmos.DrawWireSphere(vacuumObject.transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
                    }
                    else if (collider is BoxCollider2D boxCollider)
                    {
                        // VacuumObject의 월드 포지션과 콜라이더 오프셋을 고려
                        Gizmos.DrawWireCube(vacuumObject.transform.position + (Vector3)boxCollider.offset, boxCollider.size);
                    }
                }
            }
        }
    }
#endif
}