using UnityEngine;

public class CameraMgr : MonoBehaviour
{
    // CameraFollow
    public Transform player1;  // �÷��̾� 1
    public Transform player2;  // �÷��̾� 2
    public float smoothSpeed = 0.125f;  // ī�޶� �̵� �ӵ�
    public Vector3 offset;  // ī�޶��� ������ (�÷��̾���� �Ÿ�)

    public float leftLimit = -5f;  // ���� ī�޶� �̵� ����
    public float rightLimit = 5f;  // ������ ī�޶� �̵� ����

    public Camera mainCamera;  // ���� ī�޶�

    // CameraShake
    public static CameraMgr Instance;
    private Vector3 originalPos;

    private void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }
    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines(); // �ߺ� ��鸲 ����
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    void LateUpdate()
    {
        // �� ĳ������ �߰� ��ġ ���
        Vector3 midpoint = (player1.position + player2.position) / 2;

        // ī�޶�� �߰� ��ġ�� �������� ���� ��ġ�� �̵�
        Vector3 desiredPosition = midpoint + offset;

        // ī�޶��� X ��ġ�� ���� ���� �������� �̵��ϵ��� ����
        float clampedX = Mathf.Clamp(desiredPosition.x, leftLimit, rightLimit);

        // Y�� ������ ������ ����
        Vector3 finalPosition = new Vector3(clampedX, transform.position.y, transform.position.z);

        // ī�޶� �ε巴�� �̵�
        transform.position = Vector3.Lerp(transform.position, finalPosition, smoothSpeed);

        // ȭ�� ������ ������ �ʵ��� ĳ������ ��ġ ����
        RestrictPlayerPosition();
    }

    // ĳ���Ͱ� ȭ�� ������ ������ �ʵ��� �����ϴ� �Լ�
    void RestrictPlayerPosition()
    {
        // ī�޶� ����Ʈ�� �������� ĳ������ ��ġ�� ����
        Vector3 player1ViewportPos = mainCamera.WorldToViewportPoint(player1.position);
        Vector3 player2ViewportPos = mainCamera.WorldToViewportPoint(player2.position);

        // �÷��̾� 1�� ȭ�� ������ ������ �ʵ��� ����
        if (player1ViewportPos.x < 0.02f)
        {
            player1.position = mainCamera.ViewportToWorldPoint(new Vector3(0.02f, player1ViewportPos.y, player1ViewportPos.z));
        }
        else if (player1ViewportPos.x > 0.98f)
        {
            player1.position = mainCamera.ViewportToWorldPoint(new Vector3(0.98f, player1ViewportPos.y, player1ViewportPos.z));
        }

        // �÷��̾� 2�� ȭ�� ������ ������ �ʵ��� ����
        if (player2ViewportPos.x < 0.02f)
        {
            player2.position = mainCamera.ViewportToWorldPoint(new Vector3(0.02f, player2ViewportPos.y, player2ViewportPos.z));
        }
        else if (player2ViewportPos.x > 0.98f)
        {
            player2.position = mainCamera.ViewportToWorldPoint(new Vector3(0.98f, player2ViewportPos.y, player2ViewportPos.z));
        }
    }

    private System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.5f, 0.5f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}