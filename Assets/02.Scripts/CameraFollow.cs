using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player1;  // �÷��̾� 1
    public Transform player2;  // �÷��̾� 2
    public float smoothSpeed = 0.125f;  // ī�޶� �̵� �ӵ�
    public Vector3 offset;  // ī�޶��� ������ (�÷��̾���� �Ÿ�)

    public float leftLimit = -5f;  // ���� ī�޶� �̵� ����
    public float rightLimit = 5f;  // ������ ī�޶� �̵� ����

    public Camera mainCamera;  // ���� ī�޶�


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
}