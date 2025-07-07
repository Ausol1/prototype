using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossCtrl : MonoBehaviour
{
    [Header("Boss Core Stats")]
    public float maxHp = 500f;
    public float currentHp;
    public float phase2HpThreshold = 200f; // 2������ ��ȯ ü��
    public float moveSpeed = 3f;
    public float touchDamage = 30f; // �÷��̾�� ����� �� �ִ� ������

    [Header("Player Detection")]
    public Transform player1Transform; // �÷��̾� 1 Ʈ������ (�ν����Ϳ��� �Ҵ� ����)
    public Transform player2Transform; // �÷��̾� 2 Ʈ������ (�ν����Ϳ��� �Ҵ� ����)
    private Transform currentTargetTransform; // ���� ���� ��� �÷��̾��� Transform
    private Transform furthestPlayerTransform; // ���� �� �÷��̾��� Transform (Thorn ��ų��)

    // Player ������Ʈ�� ĳ���Ͽ� isDead ���¸� Ȯ���� �� �ֵ��� �߰�
    private Player player1Component;
    private Player player2Component;

    [Header("Attack Settings")]
    public float attackRangeX = 2f; // X��ǥ ���� ���� ����
    public Collider2D attackCollider; // ���� �� Ȱ��ȭ�� �ݶ��̴� (��: ���� ���� ����)

    [Header("Skill Settings")]
    public float minSkillInterval = 7f; // �ּ� ��ų �ߵ� �ֱ�
    public float maxSkillInterval = 10f; // �ִ� ��ų �ߵ� �ֱ�
    private float nextSkillTime;
    private bool isExecutingSkill = false; // ���� �̵��� ���ߴ� Ư�� ��ų(����, ��, �뽬)�� �ߵ� ������ ����

    [Header("Thorn Skill")]
    public GameObject thornPrefab;
    public List<Transform> thornSpawners = new List<Transform>(); // 3���� Thorn Spawner
    public float thornSpawnDelay = 0.1f; // �� Thorn ������ ���� ������
    public float thornAnimDuration = 2.0f; // Thorn �ִϸ��̼� ���� ���� �ð� (Animator���� ��Ȯ�� ���� Ȯ�� �� ����)

    [Header("Fire Skill")]
    public Collider2D fireTrigger; // Fire ��ų �� Ȱ��ȭ�� Ʈ����
    public float fireAnimDuration = 1.5f; // Fire �ִϸ��̼� ���� ���� �ð�

    [Header("Dash Skill")]
    public float dashDistanceX = 10f; // �뽬 �̵� �Ÿ� (X�� ����)
    public float dashPerMoveDuration = 0.5f; // �� �뽬 �������� ���� �ð� (Dash �ִϸ��̼ǰ� ����� ��)
    private int dashCount = 0; // �뽬 ��ų �� �� �� �뽬�ߴ��� (Player1 -> Player2)
    public float dashReadyAnimDuration = 1.0f; // DashReady �ִϸ��̼� ���� ���� �ð�
    public float dashPauseBetweenDashes = 0.5f; // �� �뽬 ������ ª�� ������

    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D mainCollider; // ������ �� �ݶ��̴� (�÷��̾� ���� ��������)

    private Vector3 initialLocalScale; // ������ �ʱ� ���� ������ ����

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCollider = GetComponent<BoxCollider2D>();

        if (attackCollider != null) attackCollider.enabled = false;
        if (fireTrigger != null) fireTrigger.enabled = false;

        // �ν����Ϳ��� �Ҵ���� �ʾҴٸ� �±׷� ã�� Player ������Ʈ�� ĳ��
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

        initialLocalScale = transform.localScale; // �ʱ� ���� ������ ����
    }

    void Start()
    {
        currentHp = maxHp;
        SetNextSkillTime();

        if (player1Transform == null || player2Transform == null)
        {
            Debug.LogWarning("Player 1 �Ǵ� Player 2 Ʈ�������� �Ҵ���� �ʾҽ��ϴ�! �÷��̾� ������ ����� �۵����� ���� �� �ֽ��ϴ�.", this);
        }
    }

    void Update()
    {
        UpdateTargetPlayer(); // ���� ���� ��� �÷��̾� ������Ʈ

        // �׾��� ���� �ƹ��͵� ���� ���� (���� ����)
        // if (currentHp <= 0) return; // �ּ� �����Ͽ� ���� ���� ���� �߰� ����

        if (!isExecutingSkill) // Ư�� ��ų(����, ��, �뽬) ���� �ƴ� ���� �Ϲ� �̵� �� ��ų ��Ÿ�� üũ
        {
            HandleMovementAndAttackDecision();
            CheckSkillCooldown(); // ��ų ��Ÿ�� üũ
        }
        else // Ư�� ��ų ���� ���� ��
        {
            // Dash �ִϸ��̼� ������ ���� �̵��� ��� (DashRoutine���� ����)
            // �� ���� ��ų (Attack, Thorn, Fire, DashReady) �߿��� �̵��� ����
            // NOTE: SkillLayer �ε����� 1���̶�� GetCurrentAnimatorStateInfo(1)�� �����ؾ� �մϴ�.
            // (���� ��ȭ���� 1������ �����Ͽ� �ذ�Ǽ̴ٰ� �ϼ����Ƿ� 1�� ����)
            if (!anim.GetCurrentAnimatorStateInfo(1).IsName("Stage4_BossDash"))
            {
                rb.linearVelocity = Vector2.zero; // �̵� �ߴ�
                anim.SetBool("isWalk", false);
            }
        }

        FlipSprite();
    }

    void UpdateTargetPlayer()
    {
        // ���� �÷��̾�� null�� ó���Ͽ� ���� ��󿡼� ����
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
            return; // ������ �÷��̾ ������ ����
        }

        float distToP1 = (player1Transform != null) ? Vector2.Distance(transform.position, player1Transform.position) : float.MaxValue;
        float distToP2 = (player2Transform != null) ? Vector2.Distance(transform.position, player2Transform.position) : float.MaxValue;

        // �÷��̾ �� �� ������ ���
        if (player1Transform == null)
        {
            currentTargetTransform = player2Transform;
            furthestPlayerTransform = player2Transform; // ���� �� ���� ���� �� �÷��̾�ε� ����
        }
        else if (player2Transform == null)
        {
            currentTargetTransform = player1Transform;
            furthestPlayerTransform = player1Transform; // ���� �� ���� ���� �� �÷��̾�ε� ����
        }
        // �÷��̾ �� �� ��� ����ִ� ���
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

        // �Ϲ� �̵� �� ���� ��ȯ
        if (rb.linearVelocity.x > 0.1f)
        {
            transform.localScale = new Vector3(initialLocalScale.x, initialLocalScale.y, initialLocalScale.z); // ������
        }
        else if (rb.linearVelocity.x < -0.1f)
        {
            transform.localScale = new Vector3(-initialLocalScale.x, initialLocalScale.y, initialLocalScale.z); // ����
        }
        // �뽬 �߿��� rb.linearVelocity�� 0�� �� �����Ƿ�, ���� target ������ ���� ������ ���� �߰�
        // NOTE: SkillLayer �ε����� 1���̶�� GetCurrentAnimatorStateInfo(1)�� �����ؾ� �մϴ�.
        else if (isExecutingSkill && anim.GetCurrentAnimatorStateInfo(1).IsName("Stage4_BossDash") && currentTargetTransform != null)
        {
            if (currentTargetTransform.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(-initialLocalScale.x, initialLocalScale.y, initialLocalScale.z); // ����
            }
            else
            {
                transform.localScale = new Vector3(initialLocalScale.x, initialLocalScale.y, initialLocalScale.z); // ������
            }
        }
    }

    // --- �ִϸ��̼� �̺�Ʈ (Animator Controller�� ����) ---
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
        // �ڷ�ƾ���� isExecutingSkill ����
    }

    public void AnimationEvent_DashReadyEnd()
    {
        // Animator���� Transition���� ó��
    }

    public void AnimationEvent_DashEnd()
    {
        // DashRoutine���� isExecutingSkill�� false�� �����մϴ�.
    }


    // --- ����/��ų �ڷ�ƾ ---

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
        // ��� �÷��̾ ������ ��ų �ߵ����� ����
        if (currentTargetTransform == null) return;

        if (Time.time >= nextSkillTime && !isExecutingSkill)
        {
            ChooseAndExecuteSkill();
            SetNextSkillTime();
        }
    }

    void ChooseAndExecuteSkill()
    {
        isExecutingSkill = true; // Ư�� ��ų �ߵ� ���� �÷���, �̵� �� �ٸ� ��ų ����

        // ������ �÷��̾ ������ ��ų �������� ���� (currentTargetTransform�� null�� ��)
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

        Vector3 targetPos = furthestPlayerTransform.position; // �� ���� Coroutine ���� ������ �����˴ϴ�.

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
        yield return null; // �� ������ ����Ͽ� Ʈ���� ���� ����

        while (dashCount < 2)
        {
            anim.SetTrigger("isDash");
            Debug.Log($"[DashSkill Debug] Dash {dashCount + 1} Trigger 'isDash' set at {Time.time}");

            float waitTimer = 0f;
            float currentDashMaxWaitTime = 0.5f; // �� ���� �ٿ��� �׽�Ʈ�غ�����.
            while (!anim.GetCurrentAnimatorStateInfo(1).IsName("Stage4_BossDash") && waitTimer < currentDashMaxWaitTime)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }

            if (waitTimer >= currentDashMaxWaitTime)
            {
                Debug.LogError($"Failed to transition to Stage4_BossDash animation state for Dash {dashCount + 1} within {currentDashMaxWaitTime}s. Check Animator transitions.");
                isExecutingSkill = false;
                // DashSkillRoutine�� ������ ����ǹǷ�, isDashFinished Ʈ���Ÿ� ���⼭ �ߵ����� �ʽ��ϴ�.
                // (���� ����� ���� �ߵ��ǵ��� ���� ����)
                yield break;
            }
            Debug.Log($"[DashSkill Debug] Dash {dashCount + 1} entered Stage4_BossDash state at {Time.time}");

            Transform target = null;
            // �÷��̾� ��� �� Ÿ�� ���� ���� �߰�
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
            // ���� Ÿ�� �÷��̾ �׾��ٸ�, �ٸ� ����ִ� �÷��̾ �ִ��� Ȯ��
            if (target == null)
            {
                if (dashCount == 0 && p2Alive) target = player2Transform; // 1��° �뽬�ε� P1�� �׾����� P2��
                else if (dashCount == 1 && p1Alive) target = player1Transform; // 2��° �뽬�ε� P2�� �׾����� P1����
                // ���� �׷��� Ÿ���� ������ �� �÷��̾� ��� ���� ����
                if (target == null)
                {
                    Debug.LogWarning($"Both players are dead. Skipping remaining dashes.");
                    break; // ���� ����
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

            yield return new WaitForSeconds(0.42f); // Dash �ִϸ��̼��� ���� ���̸�ŭ ���

            dashCount++;

            if (dashCount < 2)
            {
                yield return new WaitForSeconds(dashPauseBetweenDashes);
            }
        }

        // ��� �뽬�� ���������� �Ϸ�� ��쿡�� ���� ���� Ʈ���� �ߵ�
        if (dashCount == 2) // dashCount�� 2�� �Ǿ��� ���� ���������� �Ϸ�� ��
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
            if (player != null && !player.isDead) // ���� �÷��̾�Դ� ������ ���� ����
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