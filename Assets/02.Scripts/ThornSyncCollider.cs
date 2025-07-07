using UnityEngine;
using System.Collections.Generic; // List�� ����ϱ� ���� �ʿ��մϴ�.

public class ThornSyncCollider : MonoBehaviour
{
    private SpriteRenderer sr;
    private PolygonCollider2D poly;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        poly = GetComponent<PolygonCollider2D>();
        // �ʱ�ȭ �� �� �� ����ȭ
        SyncColliderToSprite();
    }

    // �� �Լ��� �ִϸ��̼� �̺�Ʈ�� ȣ���� ���Դϴ�.
    public void SyncColliderToSprite()
    {
        if (sr != null && poly != null && sr.sprite != null)
        {
            List<Vector2> points = new List<Vector2>();
            // GetPhysicsShape�� ù ��° ���ڴ� pathIndex�Դϴ�.
            // ��κ��� ��������Ʈ�� 0�� �н��� �����ϴ�.
            sr.sprite.GetPhysicsShape(0, points);
            poly.SetPath(0, points.ToArray());
            // Debug.Log($"Collider updated for sprite: {sr.sprite.name}"); // ������
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