using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player1;  // 플레이어 1
    public Transform player2;  // 플레이어 2
    public float smoothSpeed = 0.125f;  // 카메라 이동 속도
    public Vector3 offset;  // 카메라의 오프셋 (플레이어와의 거리)

    public float leftLimit = -5f;  // 왼쪽 카메라 이동 제한
    public float rightLimit = 5f;  // 오른쪽 카메라 이동 제한

    public Camera mainCamera;  // 메인 카메라

    void LateUpdate()
    {
        // 두 캐릭터의 중간 위치 계산
        Vector3 midpoint = (player1.position + player2.position) / 2;

        // 카메라는 중간 위치에 오프셋을 더한 위치로 이동
        Vector3 desiredPosition = midpoint + offset;

        // 카메라의 X 위치가 일정 범위 내에서만 이동하도록 제한
        float clampedX = Mathf.Clamp(desiredPosition.x, leftLimit, rightLimit);

        // Y는 고정된 값으로 설정
        Vector3 finalPosition = new Vector3(clampedX, transform.position.y, transform.position.z);

        // 카메라 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, finalPosition, smoothSpeed);

        // 화면 밖으로 나가지 않도록 캐릭터의 위치 제한
        RestrictPlayerPosition();
    }

    // 캐릭터가 화면 밖으로 나가지 않도록 제한하는 함수
    void RestrictPlayerPosition()
    {
        // 카메라 뷰포트를 기준으로 캐릭터의 위치를 제한
        Vector3 player1ViewportPos = mainCamera.WorldToViewportPoint(player1.position);
        Vector3 player2ViewportPos = mainCamera.WorldToViewportPoint(player2.position);

        // 플레이어 1이 화면 밖으로 나가지 않도록 제한
        if (player1ViewportPos.x < 0.02f)
        {
            player1.position = mainCamera.ViewportToWorldPoint(new Vector3(0.02f, player1ViewportPos.y, player1ViewportPos.z));
        }
        else if (player1ViewportPos.x > 0.98f)
        {
            player1.position = mainCamera.ViewportToWorldPoint(new Vector3(0.98f, player1ViewportPos.y, player1ViewportPos.z));
        }

        // 플레이어 2가 화면 밖으로 나가지 않도록 제한
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