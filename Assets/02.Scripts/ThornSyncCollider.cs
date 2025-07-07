using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 필요합니다.

public class ThornSyncCollider : MonoBehaviour
{
    private SpriteRenderer sr;
    private PolygonCollider2D poly;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        poly = GetComponent<PolygonCollider2D>();
        // 초기화 시 한 번 동기화
        SyncColliderToSprite();
    }

    // 이 함수를 애니메이션 이벤트로 호출할 것입니다.
    public void SyncColliderToSprite()
    {
        if (sr != null && poly != null && sr.sprite != null)
        {
            List<Vector2> points = new List<Vector2>();
            // GetPhysicsShape의 첫 번째 인자는 pathIndex입니다.
            // 대부분의 스프라이트는 0번 패스만 가집니다.
            sr.sprite.GetPhysicsShape(0, points);
            poly.SetPath(0, points.ToArray());
            // Debug.Log($"Collider updated for sprite: {sr.sprite.name}"); // 디버깅용
        }
        else
        {
            // Debug.LogWarning("Cannot sync collider: Missing SpriteRenderer, PolygonCollider2D, or Sprite.");
        }
    }
    public void DestroyThorn()
    {
        Destroy(gameObject);
    }
}