using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public enum PlayerType { Player1, Player2 }
    public PlayerType playerType = PlayerType.Player1;

    public enum WeaponType { DefaultGun, RocketLauncher, VacuumCleaner }

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
    public bool isGrounded = false;
    public int JumpCount = 0;

    //--- 총 관련 변수
    public GameObject m_BulletPrefab = null;
    public Transform m_ShootPos;
    public GameObject m_Gun = null;
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
    public float suckRadius = 3f;
    public float suckForce = 10f;
    public float consumeDistance = 0.5f;
    public LayerMask smallMonsterLayer;

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
    private bool isBeingRevived = false;
    private float reviveProgress = 0f;
    public float reviveRequired = 10f; // 스페이스 연타 10회 필요
    private Player otherPlayer; // 다른 플레이어 참조를 Update에서 사용하려면 필요합니다.
    private bool isOverlappingWithOther = false;

    // 부활 UI(선택)
    public Image reviveImage;
    public Image reviveBar;

    // *** 추가된 변수 ***
    public Collider2D mainPlayerCollider; // 플레이어의 주 Collider (몸체)
    public Collider2D reviveDetectionTrigger; // 부활 감지용 Trigger Collider

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

        // Collider들이 inspector에서 할당되었는지 확인합니다.
        if (mainPlayerCollider == null)
        {
            Debug.LogError("mainPlayerCollider가 할당되지 않았습니다. Player GameObject의 주 Collider를 여기에 드래그해주세요.");
            mainPlayerCollider = GetComponent<Collider2D>(); // 기본값으로 첫 Collider를 할당 시도
        }
        if (reviveDetectionTrigger == null)
        {
            Debug.LogError("reviveDetectionTrigger가 할당되지 않았습니다. Player GameObject의 자식에 Is Trigger가 체크된 Collider를 추가하고 여기에 드래그해주세요.");
            // 자식 오브젝트에서 "ReviveTrigger"라는 이름의 Collider2D를 찾거나, 새롭게 추가하는 것을 권장합니다.
            // 예를 들어, 아래처럼 찾을 수 있습니다. (적절한 자식 오브젝트 이름으로 변경)
            // reviveDetectionTrigger = transform.Find("ReviveTriggerObject")?.GetComponent<Collider2D>();
        }


        // Physics2D.IgnoreCollision 로직 수정
        foreach (var otherPlayerComp in FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (otherPlayerComp == this) continue;

            // 현재 플레이어의 메인 Collider와 다른 플레이어의 메인 Collider만 충돌 무시
            if (mainPlayerCollider != null && otherPlayerComp.mainPlayerCollider != null)
            {
                Physics2D.IgnoreCollision(mainPlayerCollider, otherPlayerComp.mainPlayerCollider, true);
            }

            // 다른 플레이어 참조 설정 (여기서는 첫 번째 찾은 다른 플레이어를 할당)
            // 실제 게임에서는 맵에 플레이어가 2명만 존재한다고 가정합니다.
            // 더 견고하게 하려면 GameMgr 등에서 플레이어 참조를 관리하는 것이 좋습니다.
            otherPlayer = otherPlayerComp;
        }

        if (reviveBar != null)
        {
            reviveBar.fillAmount = 0f;
            reviveImage.gameObject.SetActive(false); // 시작 시 숨김
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
                currentMaxBulletCount = 0; // 청소기는 탄창 개념 없음
                currentFireRate = 0; // 청소기는 발사 쿨타임 개념 없음
                currentReloadTime = 0; // 청소기는 재장전 개념 없음
                break;
        }
        currentBulletCount = currentMaxBulletCount; // 시작 시 탄창 가득 채움
        isReloading = false; // 재장전 상태 초기화
        currentShootTimer = 0; // 발사 쿨타임 초기화

        // m_Gun 오브젝트 활성화/비활성화
        // 이 부분은 m_Gun이 각 무기 모델을 담고 있다고 가정합니다.
        // 예를 들어 DefaultGun 모델, RocketLauncher 모델을 m_Gun에 할당하고 필요한 시점에 활성화/비활성화
        // 복잡해지면 IWeapon 인터페이스처럼 각 무기 GameObject를 Player에 할당하고 switch문으로 활성화/비활성화 하는게 더 좋음
        // 지금은 m_Gun이 하나의 오브젝트(Placeholder)라고 가정하고, 실제 총 모델은 애니메이터/스프라이트로 변경될 수 있다고 가정.
        // 만약 m_Gun에 여러 무기 프리팹을 할당해야 한다면, 이 로직은 더 복잡해집니다.
        // 여기서는 m_Gun을 플레이어 손에 붙는 '컨테이너'로 보고, 실제 비주얼은 애니메이션이나 자식 오브젝트로 제어한다고 가정합니다.
    }
    void Update()
    {
        if (isDead)
        {
            // 죽은 플레이어만 부활 UI를 관리
            if (isOverlappingWithOther && otherPlayer != null && !otherPlayer.isDead)
            {
                if (reviveImage != null)
                    reviveImage.gameObject.SetActive(true); // 겹치면 부활 UI 표시

                // 부활 진행
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
                    reviveImage.gameObject.SetActive(false); // 겹치지 않으면 숨김
                    reviveBar.fillAmount = 0f; // 진행도 초기화
                }
            }
            return;
        }
        else // 살아있는 플레이어는 부활 UI 숨김
        {
            if (reviveImage != null)
                reviveImage.gameObject.SetActive(false);
        }

        Move();
        //Shooting();
        Animation();
        // --- 무기 관련 타이머 업데이트 ---
        if (currentShootTimer > 0)
        {
            currentShootTimer -= Time.deltaTime;
        }

        // --- 무기 입력 처리 호출 ---
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
                BulletCount.text = "∞"; // 청소기는 무한대
            }
            else
            {
                BulletCount.text = currentBulletCount + " / " + currentMaxBulletCount;
            }
        }
    }
    // --- 무기 입력 처리 함수 ---
    void HandleWeaponInput()
    {
        // 죽었거나 재장전 중이면 아무것도 못함
        if (isDead || isReloading) return;

        // 플레이어 방향
        Vector2 shootDir = (SpriteRenderer != null && SpriteRenderer.flipX) ? Vector2.left : Vector2.right;

        switch (currentWeaponType)
        {
            case WeaponType.DefaultGun:
                // 발사
                if (Input.GetKey(shootKey) && currentShootTimer <= 0f && currentBulletCount > 0)
                {
                    FireDefaultGun(shootDir);
                }
                // 재장전
                if (Input.GetKeyDown(reloadKey) || (currentBulletCount <= 0 && !isReloading))
                {
                    StartReload();
                }
                break;

            case WeaponType.RocketLauncher:
                // 발사
                if (Input.GetKeyDown(shootKey) && currentShootTimer <= 0f && currentBulletCount > 0) // 로켓은 KeyDown (한 발씩)
                {
                    FireRocketLauncher(shootDir);
                }
                // 재장전
                if (Input.GetKeyDown(reloadKey) || (currentBulletCount <= 0 && !isReloading))
                {
                    StartReload();
                }
                break;

            case WeaponType.VacuumCleaner:
                // 청소기는 발사 버튼을 누르고 있는 동안 작동
                if (Input.GetKey(shootKey))
                {
                    OperateVacuumCleaner();
                }
                // 청소기는 재장전 없음
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

            // 탄창이 0이 되면 자동 재장전 시작
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
        // 청소기는 탄창이나 쿨타임 개념이 없으므로, Input.GetKey(shootKey) 동안 지속적으로 작동
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(m_ShootPos.position, suckRadius, smallMonsterLayer);

        foreach (Collider2D hit in hitColliders)
        {
            // SmallMonster 태그를 가진 몬스터만 처리
            if (hit.CompareTag("SmallMonster"))
            {
                Rigidbody2D monsterRb = hit.GetComponent<Rigidbody2D>();
                if (monsterRb != null)
                {
                    // 플레이어 방향으로 몬스터 끌어당기기
                    Vector2 directionToPlayer = (m_ShootPos.position - hit.transform.position).normalized;
                    monsterRb.AddForce(directionToPlayer * suckForce, ForceMode2D.Force);

                    // 특정 거리 이내로 들어오면 몬스터 제거
                    if (Vector2.Distance(m_ShootPos.position, hit.transform.position) < consumeDistance)
                    {
                        Destroy(hit.gameObject);
                        // 몬스터 제거 시 점수 획득 등의 추가 로직 여기 구현
                    }
                }
            }
        }
    }
    void StartReload()
    {
        if (currentWeaponType == WeaponType.VacuumCleaner) return; // 청소기는 재장전 없음

        if (!isReloading)
        {
            isReloading = true;
            Invoke("ReloadComplete", currentReloadTime);
            Debug.Log("재장전 시작!");
        }
    }
    void ReloadComplete()
    {
        currentBulletCount = currentMaxBulletCount;
        isReloading = false;
        Debug.Log("재장전 완료!");
    }
    void Die()
    {
        isDead = true;
        m_CurHp = 0.0f;
        if (Anim != null)
            Anim.SetTrigger("Die");
        // 부활 게이지 초기화
        reviveProgress = 0f;
        if (reviveBar != null)
            reviveBar.fillAmount = 0f;

        rb.linearVelocity = Vector2.zero; // 죽는 순간 속도 정지

        // 게임 오버 체크는 GameMgr에서 두 명 다 죽었을 때만 호출
        if (GameMgr.Inst != null)
            GameMgr.Inst.OnPlayerDead();
    }

    void Revive()
    {
        isDead = false;
        m_CurHp = m_MaxHp * 0.5f; // 부활 시 체력 절반
        reviveProgress = 0f;
        if (reviveBar != null)
            reviveBar.fillAmount = 0f;
        if (m_Gun != null) m_Gun.SetActive(true);
        if (GameMgr.Inst != null)
            GameMgr.Inst.OnPlayerRevive();
        //if (Anim != null)
        //    Anim.SetTrigger("Revive");
    }

    void Move()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero; // 죽었을 때 즉시 정지
            return;
        }

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

    //void Shooting()
    //{
    //    ShootTimer -= Time.deltaTime;

    //    if (isReloading)
    //        return;

    //    if (Input.GetKey(shootKey) && ShootTimer <= 0f && m_BulletCurrentCount > 0)
    //    {
    //        GameObject bullet = Instantiate(m_BulletPrefab, m_ShootPos.position, Quaternion.identity);
    //        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

    //        Vector2 shootDir = SpriteRenderer.flipX ? Vector2.left : Vector2.right;
    //        rb.linearVelocity = shootDir * shootForce;
    //        ShootTimer = m_ShootCool;
    //        m_BulletCurrentCount--;
    //    }
    //    if (Input.GetKeyDown(reloadKey) || m_BulletCurrentCount <= 0)
    //    {
    //        isReloading = true;
    //        Invoke("Reload", m_ReloadTime);
    //    }
    //}

    //void Reload()
    //{
    //    m_BulletCurrentCount = m_BulletMaxCount;
    //    isReloading = false;
    //}

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

    public void TakeDamage(float a_Value)
    {
        if (m_CurHp <= 0.0f)
            return;

        m_CurHp -= a_Value;
        if (m_CurHp < 0.0f)
            m_CurHp = 0.0f;

        if (m_CurHp <= 0.0f)
        {
            Die();
        }
    }

    // 트리거는 reviveDetectionTrigger에만 반응하도록 설정 (유니티 에디터에서)
    void OnTriggerEnter2D(Collider2D coll)
    {
        // EnemyBullet, Fire, Lava 등은 기존대로 작동
        if (coll.CompareTag("EnemyBullet"))
        {
            TakeDamage(10);
            Destroy(coll.gameObject);
        }
        else if (coll.CompareTag("Fire"))
        {
            TakeDamage(50);
        }
        // 플레이어 트리거 감지 부분 (reviveDetectionTrigger에만 들어갈 수 있도록 에디터에서 설정)
        else if (coll.CompareTag("Player") && coll.gameObject != this.gameObject)
        {
            // 이 콜라이더가 reviveDetectionTrigger 인스턴스와 같은지 확인 (선택 사항, 더 견고하게)
            // 또는 OnTriggerEnter2D가 reviveDetectionTrigger에만 할당되도록 유니티에서 설정
            if (coll == otherPlayer?.mainPlayerCollider) // 다른 플레이어의 메인 콜라이더가 내 트리거에 진입했을 때
            {
                isOverlappingWithOther = true;
                // Debug.Log(this.name + ": 다른 플레이어가 나에게 접근! (트리거)");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.CompareTag("Player") && coll.gameObject != this.gameObject)
        {
            if (coll == otherPlayer?.mainPlayerCollider) // 다른 플레이어의 메인 콜라이더가 내 트리거에서 나갔을 때
            {
                isOverlappingWithOther = false;
                reviveProgress = 0f;
                if (reviveBar != null)
                    reviveBar.fillAmount = 0f;
                // Debug.Log(this.name + ": 다른 플레이어가 나에게서 벗어남! (트리거)");
            }
        }
        // ... 기존 코드(EnemyBullet, Fire 등) 생략 ...
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
}