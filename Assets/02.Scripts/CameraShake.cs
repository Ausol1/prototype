using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 originalPos;

    private void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    private void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    private void LateUpdate()
    {
        // 카메라가 움직였을 때 원래 위치 갱신
        if (!isShaking)
        {
            originalPos = transform.localPosition;
        }
    }

    private bool isShaking = false;

    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines(); // 중복 흔들림 방지
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        // 흔들기 시작 시점의 위치를 기준으로 흔들림 적용
        Vector3 shakeOrigin = transform.localPosition;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.5f, 0.5f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = shakeOrigin + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = shakeOrigin;
        isShaking = false;
    }
}
 