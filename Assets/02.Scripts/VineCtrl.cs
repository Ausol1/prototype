using UnityEngine;

public class VineCtrl : MonoBehaviour
{
    public Sprite cutVineSprite;         // 잘린 덩굴 스프라이트
    public Transform StonePos;           // 스톤 생성 위치
    public GameObject StonePrefab;       // 생성할 스톤 프리팹
    private bool iscut = false; // 덩굴이 잘렸는지 여부

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!iscut&&other.gameObject.CompareTag("Sawtooth"))
        {
            iscut = true; // 덩굴이 잘렸음을 표시
            // 스프라이트 변경
            if (cutVineSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = cutVineSprite;
            }

            // Stone 생성
            if (StonePrefab != null && StonePos != null)
            {
                Instantiate(StonePrefab, StonePos.position, Quaternion.identity);
            }
        }
    }

    void Update()
    {

    }
}