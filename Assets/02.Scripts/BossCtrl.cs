using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossCtrl : MonoBehaviour
{
    [Header("Boss Core Stats")]
    public float maxHp = 500f;
    public float currentHp;
    public float phase2HpThreshold = 200f; // 2페이즈 전환 체력
    public float moveSpeed = 3f;
    public float touchDamage = 30f; // 플레이어와 닿았을 때 주는 데미지

    [Header("Player Detection")]
    public Transform player1Transform; // 플레이어 1 트랜스폼 (인스펙터에서 할당 권장)
    public Transform player2Transform; // 플레이어 2 트랜스폼 (인스펙터에서 할당 권장)
    private Transform currentTargetTransform; // 현재 추적 대상 플레이어의 Transform
    private Transform furthestPlayerTransform; // 가장 먼 플레이어의 Transform (Thorn 스킬용)

    // Player 컴포넌트를 캐싱하여 isDead 상태를 확인할 수 있도록 추가
    private Player player1Component;
    private Player player2Component;

    [Header("Attack Settings")]
    public float attackRangeX = 2f; // X좌표 기준 공격 범위
    public Collider2D attackCollider; // 공격 시 활성화될 콜라이더 (예: 근접 공격 판정)

    [Header("Skill Settings")]
    public float minSkillInterval = 7f; // 최소 스킬 발동 주기
    public float maxSkillInterval = 10f; // 최대 스킬 발동 주기
    private float nextSkillTime;
    private bool isExecutingSkill = false; // 보스 이동을 멈추는 특수 스킬(가시, 불, 대쉬)이 발동 중인지 여부

    [Header("Thorn Skill")]
    public GameObject thornPrefab;
    public List<Transform> thornSpawners = new List<Transform>(); // 3개의 Thorn Spawner
    public float thornSpawnDelay = 0.1f; // 각 Thorn 스포너 간의 딜레이
    public float thornAnimDuration = 2.0f; // Thorn 애니메이션 예상 지속 시간 (Animator에서 정확한 길이 확인 후 설정)

    [Header("Fire Skill")]
    public Collider2D fireTrigger; // Fire 스킬 시 활성화될 트리거
    public float fireAnimDuration = 1.5f; // Fire 애니메이션 예상 지속 시간

    [Header("Dash Skill")]
    public float dashDistanceX = 10f; // 대쉬 이동 거리 (X축 기준)
    public float dashPerMoveDuration = 0.5f; // 각 대쉬 움직임의 지속 시간 (Dash 애니메이션과 맞춰야 함)
    private int dashCount = 0; // 대쉬 스킬 중 몇 번 대쉬했는지 (Player1 -> Player2)
    public float dashReadyAnimDuration = 1.0f; // DashReady 애니메이션 예상 지속 시간
    public float dashPauseBetweenDashes = 0.5f; // 각 대쉬 사이의 짧은 딜레이

    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D mainCollider; // 보스의 주 콜라이더 (플레이어 접촉 데미지용)

    private Vector3 initialLocalScale; // 보스의 초기 로컬 스케일 저장

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCollider = GetComponent<BoxCollider2D>();

        if (attackCollider != null) attackCollider.enabled = false;
        if (fireTrigger != null) fireTrigger.enabled = false;

        // 인스펙터에서 할당되지 않았다면 태그로 찾고 Player 컴포넌트도 캐싱
        if (player1Transform == null)
        {
            GameObject p1Obj = GameObject.FindWithTag("Player1");
            if (p1Obj != null)
            {
                player1Transform = p1Obj.transform;
                player1Component = p1Obj.GetComponent<Player>();
            }
        }
        else
        {
            player1Component = player1Transform.GetComponent<Player>();
        }

        if (player2Transform == null)
        {
            GameObject p2Obj = GameObject.FindWithTag("Player2");
            if (p2Obj != null)
            {
                player2Transform = p2Obj.transform;
                player2Component = p2Obj.GetComponent<Player>();
            }
        }
        else
        {
            player2Component = player2Transform.GetComponent<Player>();
        }

        initialLocalScale = transform.localScale; // 초기 로컬 스케일 저장
    }

    void Start()
    {
        currentHp = maxHp;
        SetNextSkillTime();

        if (player1Transform == null || player2Transform == null)
        {
            Debug.LogWarning("Player 1 또는 Player 2 트랜스폼이 할당되지 않았습니다! 플레이어 추적이 제대로 작동하지 않을 수 있습니다.", this);
        }
    }

    void Update()
    {
        UpdateTargetPlayer(); // 현재 추적 대상 플레이어 업데이트

        // 죽었을 때는 아무것도 하지 않음 (추후 구현)
        // if (currentHp <= 0) return; // 주석 해제하여 보스 죽음 로직 추가 가능

        if (!isExecutingSkill) // 특수 스킬(가시, 불, 대쉬) 중이 아닐 때만 일반 이동 및 스킬 쿨타임 체크
        {
            HandleMovementAndAttackDecision();
            CheckSkillCooldown(); // 스킬 쿨타임 체크
        }
        else // 특수 스킬 실행 중일 때
        {
            // Dash 애니메이션 상태일 때만 이동을 허용 (DashRoutine에서 제어)
            // 그 외의 스킬 (Attack, Thorn, Fire, DashReady) 중에는 이동을 멈춤
            // NOTE: SkillLayer 인덱스가 1번이라면 GetCurrentAnimatorStateInfo(1)로 변경해야 합니다.
            // (이전 대화에서 1번으로 변경하여 해결되셨다고 하셨으므로 1로 변경)
            if (!anim.GetCurrentAnimatorStateInfo(1).IsName("Stage4_BossDash"))
            {
                rb.linearVelocity = Vector2.zero; // 이동 중단
                anim.SetBool("isWalk", false);
            }
        }

        FlipSprite();
    }

    void UpdateTargetPlayer()
    {
        // 죽은 플레이어는 null로 처리하여 추적 대상에서 제외
        if (player1Component != null && player1Component.isDead)
        {
            player1Transform = null;
            player1Component = null;
        }
        if (player2Component != null && player2Component.isDead)
        {
            player2Transform = null;
            player2Component = null;
        }

        if (player1Transform == null && player2Transform == null)
        {
            currentTargetTransform = null;
            furthestPlayerTransform = null;
            return; // 추적할 플레이어가 없으면 종료
        }

        float distToP1 = (player1Transform != null) ? Vector2.Distance(transform.position, player1Transform.position) : float.MaxValue;
        float distToP2 = (player2Transform != null) ? Vector2.Distance(transform.position, player2Transform.position) : float.MaxValue;

        // 플레이어가 한 명만 남았을 경우
        if (player1Transform == null)
        {
            currentTargetTransform = player2Transform;
            furthestPlayerTransform = player2Transform; // 남은 한 명을 가장 먼 플레이어로도 설정
        }
        else if (player2Transform == null)
        {
            currentTargetTransform = player1Transform;
            furthestPlayerTransform = player1Transform; // 남은 한 명을 가장 먼 플레이어로도 설정
        }
        // 플레이어가 두 명 모두 살아있는 경우
        else
        {
            if (distToP1 <= distToP2)
            {
                currentTargetTransform = player1Transform;
                furthestPlayerTransform = player2Transform;
            }
            else
            {
                currentTargetTransform = player2Transform;
                furthestPlayerTransform = player1Transform;
            }
        }
    }

    void HandleMovementAndAttackDecision()
    {
        if (currentTargetTransform == null)
        {
            anim.SetBool("isWalk", false);
            anim.SetBool("isAttack", false);
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distanceX = Mathf.Abs(transform.position.x - currentTargetTransform.position.x);

        if (!isExecutingSkill)
        {
            if (distanceX <= attackRangeX)
            {
                if (!anim.GetBool("isAttack"))
                {
                    StartCoroutine(AttackRoutine());
                }
            }
            else
            {
                MoveTowardsPlayer(currentTargetTransform);
                anim.SetBool("isWalk", true);
                anim.SetBool("isAttack", false);
            }
        }
    }

    void MoveTowardsPlayer(Transform target)
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    void FlipSprite()
    {
        Vector3 currentScale = transform.localScale;

        // 일반 이동 중 방향 전환
        if (rb.linearVelocity.x > 0.1f)
        {
            transform.localScale = new Vector3(initialLocalScale.x, initialLocalScale.y, initialLocalScale.z); // 오른쪽
        }
        else if (rb.linearVelocity.x < -0.1f)
        {
            transform.localScale = new Vector3(-initialLocalScale.x, initialLocalScale.y, initialLocalScale.z); // 왼쪽
        }
        // 대쉬 중에는 rb.linearVelocity가 0일 수 있으므로, 직접 target 방향을 보고 뒤집는 로직 추가
        // NOTE: SkillLayer 인덱스가 1번이라면 GetCurrentAnimatorStateInfo(1)로 변경해야 합니다.
        else if (isExecutingSkill && anim.GetCurrentAnimatorStateInfo(1).IsName("Stage4_BossDash") && currentTargetTransform != null)
        {
            if (currentTargetTransform.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(-initialLocalScale.x, initialLocalScale.y, initialLocalScale.z); // 왼쪽
            }
            else
            {
                transform.localScale = new Vector3(initialLocalScale.x, initialLocalScale.y, initialLocalScale.z); // 오른쪽
            }
        }
    }

    // --- 애니메이션 이벤트 (Animator Controller에 연결) ---
    public void AnimationEvent_AttackStart()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            Debug.Log("Attack Collider Enabled");
        }
    }

    public void AnimationEvent_AttackEnd()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
            Debug.Log("Attack Collider Disabled");
        }
        anim.SetBool("isAttack", false);
    }

    public void AnimationEvent_FireStart()
    {
        if (fireTrigger != null)
        {
            fireTrigger.enabled = true;
            Debug.Log($"[FireSkill Debug] FireTrigger ENABLED by AnimationEvent_FireStart at {Time.time}");
        }
        else
        {
            Debug.LogWarning("[FireSkill Debug] Fire Trigger is NULL in AnimationEvent_FireStart!");
        }
    }

    public void AnimationEvent_FireEnd()
    {
        if (fireTrigger != null)
        {
            fireTrigger.enabled = false;
            Debug.Log($"[FireSkill Debug] Fire Trigger DISABLED by AnimationEvent_FireEnd at {Time.time}");
        }
        else
        {
            Debug.LogWarning("[FireSkill Debug] Fire Trigger is NULL in AnimationEvent_FireEnd!");
        }
    }

    public void AnimationEvent_ThornSpawn()
    {
        StartCoroutine(SpawnThornsRoutine());
    }

    public void AnimationEvent_ThornEnd()
    {
        // 코루틴에서 isExecutingSkill 관리
    }

    public void AnimationEvent_DashReadyEnd()
    {
        // Animator에서 Transition으로 처리
    }

    public void AnimationEvent_DashEnd()
    {
        // DashRoutine에서 isExecutingSkill을 false로 설정합니다.
    }


    // --- 공격/스킬 코루틴 ---

    IEnumerator AttackRoutine()
    {
        anim.SetBool("isAttack", true);
        anim.SetBool("isWalk", false);
        yield return null;
    }

    void SetNextSkillTime()
    {
        nextSkillTime = Time.time + Random.Range(minSkillInterval, maxSkillInterval);
    }

    void CheckSkillCooldown()
    {
        // 모든 플레이어가 죽으면 스킬 발동하지 않음
        if (currentTargetTransform == null) return;

        if (Time.time >= nextSkillTime && !isExecutingSkill)
        {
            ChooseAndExecuteSkill();
            SetNextSkillTime();
        }
    }

    void ChooseAndExecuteSkill()
    {
        isExecutingSkill = true; // 특수 스킬 발동 시작 플래그, 이동 및 다른 스킬 방지

        // 추적할 플레이어가 없으면 스킬 선택하지 않음 (currentTargetTransform이 null일 때)
        if (currentTargetTransform == null)
        {
            isExecutingSkill = false;
            return;
        }

        int skillChoice = Random.Range(0, 3); // 0: Thorn, 1: Fire, 2: Dash

        switch (skillChoice)
        {
            case 0:
                StartCoroutine(ThornSkillRoutine());
                break;
            case 1:
                StartCoroutine(FireSkillRoutine());
                break;
            case 2:
                StartCoroutine(DashSkillRoutine());
                break;
        }
    }

    IEnumerator ThornSkillRoutine()
    {
        isExecutingSkill = true;
        anim.SetTrigger("isThorn");
        yield return new WaitForSeconds(thornAnimDuration);
        isExecutingSkill = false;
    }

    IEnumerator SpawnThornsRoutine()
    {
        if (furthestPlayerTransform == null)
        {
            Debug.LogWarning("ThornSkill: Furthest player not found or dead!");
            yield break;
        }

        Vector3 targetPos = furthestPlayerTransform.position; // 이 값은 Coroutine 시작 시점에 고정됩니다.

        for (int i = 0; i < thornSpawners.Count; i++)
        {
            if (thornSpawners[i] != null && thornPrefab != null)
            {
                GameObject thorn = Instantiate(thornPrefab, thornSpawners[i].position, Quaternion.identity);
                Debug.Log($"Thorn spawned from Spawner {i + 1}");
                yield return new WaitForSeconds(thornSpawnDelay);
            }
        }
    }

    IEnumerator FireSkillRoutine()
    {
        isExecutingSkill = true;
        anim.SetTrigger("isFire");
        Debug.Log($"[FireSkill Debug] FireSkillRoutine STARTED at {Time.time}. Trigger 'isFire' set.");
        yield return new WaitForSeconds(fireAnimDuration);
        isExecutingSkill = false;
        Debug.Log($"[FireSkill Debug] FireSkillRoutine ENDED at {Time.time}. isExecutingSkill set to false.");
    }

    IEnumerator DashSkillRoutine()
    {
        isExecutingSkill = true;
        dashCount = 0;

        anim.ResetTrigger("isDashFinished");
        yield return null; // 한 프레임 대기하여 트리거 리셋 적용

        while (dashCount < 2)
        {
            anim.SetTrigger("isDash");
            Debug.Log($"[DashSkill Debug] Dash {dashCount + 1} Trigger 'isDash' set at {Time.time}");

            float waitTimer = 0f;
            float currentDashMaxWaitTime = 0.5f; // 이 값을 줄여서 테스트해보세요.
            while (!anim.GetCurrentAnimatorStateInfo(1).IsName("Stage4_BossDash") && waitTimer < currentDashMaxWaitTime)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }

            if (waitTimer >= currentDashMaxWaitTime)
            {
                Debug.LogError($"Failed to transition to Stage4_BossDash animation state for Dash {dashCount + 1} within {currentDashMaxWaitTime}s. Check Animator transitions.");
                isExecutingSkill = false;
                // DashSkillRoutine이 비정상 종료되므로, isDashFinished 트리거를 여기서 발동하지 않습니다.
                // (정상 종료될 때만 발동되도록 로직 변경)
                yield break;
            }
            Debug.Log($"[DashSkill Debug] Dash {dashCount + 1} entered Stage4_BossDash state at {Time.time}");

            Transform target = null;
            // 플레이어 사망 시 타겟 제외 로직 추가
            bool p1Alive = (player1Component != null && !player1Component.isDead);
            bool p2Alive = (player2Component != null && !player2Component.isDead);

            if (dashCount == 0 && p1Alive)
            {
                target = player1Transform;
            }
            else if (dashCount == 1 && p2Alive)
            {
                target = player2Transform;
            }
            // 만약 타겟 플레이어가 죽었다면, 다른 살아있는 플레이어가 있는지 확인
            if (target == null)
            {
                if (dashCount == 0 && p2Alive) target = player2Transform; // 1번째 대쉬인데 P1이 죽었으면 P2로
                else if (dashCount == 1 && p1Alive) target = player1Transform; // 2번째 대쉬인데 P2가 죽었으면 P1으로
                // 만약 그래도 타겟이 없으면 두 플레이어 모두 죽은 상태
                if (target == null)
                {
                    Debug.LogWarning($"Both players are dead. Skipping remaining dashes.");
                    break; // 루프 종료
                }
            }


            if (target != null)
            {
                float targetX = target.position.x;
                float currentX = transform.position.x;

                float dashDirection = Mathf.Sign(targetX - currentX);
                Vector2 dashTargetPos = new Vector2(currentX + dashDirection * dashDistanceX, transform.position.y);

                Vector2 startPos = transform.position;
                float timer = 0f;

                while (timer < dashPerMoveDuration)
                {
                    rb.MovePosition(Vector2.Lerp(startPos, dashTargetPos, timer / dashPerMoveDuration));
                    timer += Time.deltaTime;
                    yield return null;
                }
                rb.MovePosition(dashTargetPos);

                Debug.Log($"[DashSkill Debug] Dash {dashCount + 1} move completed at {Time.time}");
            }
            else
            {
                Debug.LogWarning($"Dash target {(dashCount == 0 ? "Player1" : "Player2")} not found or dead. Skipping dash {dashCount + 1}.");
            }

            yield return new WaitForSeconds(0.42f); // Dash 애니메이션의 실제 길이만큼 대기

            dashCount++;

            if (dashCount < 2)
            {
                yield return new WaitForSeconds(dashPauseBetweenDashes);
            }
        }

        // 모든 대쉬가 정상적으로 완료된 경우에만 최종 종료 트리거 발동
        if (dashCount == 2) // dashCount가 2가 되었을 때만 성공적으로 완료된 것
        {
            anim.SetTrigger("isDashFinished");
            Debug.Log($"[DashSkill Debug] All dashes completed at {Time.time}");
        }
        else
        {
            Debug.LogWarning($"[DashSkill Debug] DashRoutine ended prematurely (dashCount: {dashCount}). 'isDashFinished' not triggered.");
        }


        isExecutingSkill = false;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && !player.isDead) // 죽은 플레이어에게는 데미지 주지 않음
            {
                player.TakeDamage(touchDamage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log($"Boss took {damage} damage. Current HP: {currentHp}");

        if (currentHp <= phase2HpThreshold && currentHp > 0)
        {
            // StartPhase2();
        }

        if (currentHp <= 0)
        {
            // BossDefeat();
        }
    }
}