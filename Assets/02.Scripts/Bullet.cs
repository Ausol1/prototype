using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 5f; // 총알이 사라지기까지의 최대 시간
    private float timer;
    private Rigidbody2D rb; // 총알의 Rigidbody2D 컴포넌트

    void Start()
    {
        timer = lifeTime;
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 컴포넌트 가져오기

        // 총알의 초기 방향에 따라 Z축 회전 설정
        if (rb != null)
        {
            SetRotationBasedOnDirection(rb.linearVelocity.x);
        }
    }

    void Update()
    {
        // 타이머가 끝나면 총알 파괴
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }

        // 카메라 시야 밖으로 나갔는지 확인
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(transform.position);

        if (viewportPoint.x < -0.1f || viewportPoint.x > 1.1f ||
            viewportPoint.y < -0.1f || viewportPoint.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    // 총알의 X 방향 속도에 따라 Z축 회전 설정
    private void SetRotationBasedOnDirection(float directionX)
    {
        if (directionX < 0) // 왼쪽으로 이동 중
        {
            transform.rotation = Quaternion.Euler(0, 0, 180); // Z축 180도 회전
        }
        else if (directionX > 0) // 오른쪽으로 이동 중
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Z축 0도 회전
        }
        // directionX가 0인 경우는 발사 시엔 거의 없겠지만,
        // 필요하다면 특정 기본 방향을 설정할 수 있습니다.
    }
}