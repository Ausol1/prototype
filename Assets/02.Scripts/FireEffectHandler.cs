using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class FireEffectHandler : MonoBehaviour
{
    public float lifeTime = 1f;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Sprite lastSprite;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);  // ÀÚµ¿ ÆÄ±«
    }

    void LateUpdate()
    {
        if (spriteRenderer.sprite != lastSprite)
        {
            UpdateColliderToSprite();
            lastSprite = spriteRenderer.sprite;
        }
    }

    void UpdateColliderToSprite()
    {
        if (spriteRenderer.sprite == null) return;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        boxCollider.size = spriteBounds.size;
        boxCollider.offset = spriteBounds.center;
    }
}
