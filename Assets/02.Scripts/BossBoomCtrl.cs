using UnityEngine;

public class BossBoomCtrl : MonoBehaviour
{
    public float riseSpeed = 4f;
    private float boomTime;
    private bool isExploding = false;
    private Animator anim;
    private bool explodedByPlayer = false;
    private bool boomAnimStarted = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        boomTime = Random.Range(4f, 5f);
        if (anim != null)
            anim.enabled = false; // 시작 시 애니메이션 비활성화
    }

    void Update()
    {
        if (!isExploding)
        {
            transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);

            boomTime -= Time.deltaTime;
            if (boomTime <= 0f)
            {
                Explode();
            }
        }
        else if (boomAnimStarted && anim != null)
        {
            // 애니메이션이 끝났는지 확인
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }

    void Explode()
    {
        if (isExploding) return;
        isExploding = true;
        if (anim != null)
        {
            anim.enabled = true; // 애니메이션 활성화
            anim.SetTrigger("Boom"); // 폭발 애니메이션 실행
            boomAnimStarted = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!isExploding && (coll.CompareTag("Player1") || coll.CompareTag("Player2")))
        {
            explodedByPlayer = true;
            Explode();
        }
    }
}