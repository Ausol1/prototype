using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 5f; // �Ѿ��� ������������ �ִ� �ð�
    private float timer;
    private Rigidbody2D rb; // �Ѿ��� Rigidbody2D ������Ʈ

    void Start()
    {
        timer = lifeTime;
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D ������Ʈ ��������

        // �Ѿ��� �ʱ� ���⿡ ���� Z�� ȸ�� ����
        if (rb != null)
        {
            SetRotationBasedOnDirection(rb.linearVelocity.x);
        }
    }

    void Update()
    {
        // Ÿ�̸Ӱ� ������ �Ѿ� �ı�
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }

        // ī�޶� �þ� ������ �������� Ȯ��
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(transform.position);

        if (viewportPoint.x < -0.1f || viewportPoint.x > 1.1f ||
            viewportPoint.y < -0.1f || viewportPoint.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    // �Ѿ��� X ���� �ӵ��� ���� Z�� ȸ�� ����
    private void SetRotationBasedOnDirection(float directionX)
    {
        if (directionX < 0) // �������� �̵� ��
        {
            transform.rotation = Quaternion.Euler(0, 0, 180); // Z�� 180�� ȸ��
        }
        else if (directionX > 0) // ���������� �̵� ��
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Z�� 0�� ȸ��
        }
        // directionX�� 0�� ���� �߻� �ÿ� ���� ��������,
        // �ʿ��ϴٸ� Ư�� �⺻ ������ ������ �� �ֽ��ϴ�.
    }
}