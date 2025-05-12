using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    float m_MaxHp = 100.0f;
    public float m_CurHp = 100.0f;
    public Image m_HpBar = null;

    bool isMark = false;
    float MarkCool = 0.0f;

    public Transform player1;
    public Transform player2;
    public float moveSpeed = 2.0f;

    private Transform targetPlayer;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    void Start()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }
    }

    void Update()
    {
        // 거리 계산
        float distToP1 = Vector2.Distance(transform.position, player1.position);
        float distToP2 = Vector2.Distance(transform.position, player2.position);

        // 가까운 플레이어 선택
        if (distToP1 < distToP2)
        {
            targetPlayer = player1;
        }
        else
        {
            targetPlayer = player2;
        }

        // 따라가기
        Vector2 dir = (targetPlayer.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

        if (isMark)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].color = Color.green;
            }
            MarkCool -= Time.deltaTime;

            if (MarkCool <= 0.0f)
            {
                isMark = false;
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    spriteRenderers[i].color = Color.white;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.tag == "MarkBullet")
        {
            isMark = true;
            MarkCool = 3.0f;
        }

        if (isMark == true && coll.tag == "AllyBullet")
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
            Destroy(gameObject);
        }
    }
}
