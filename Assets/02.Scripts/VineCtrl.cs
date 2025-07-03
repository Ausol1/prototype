using UnityEngine;

public class VineCtrl : MonoBehaviour
{
    public Sprite cutVineSprite;         // �߸� ���� ��������Ʈ
    public Transform StonePos;           // ���� ���� ��ġ
    public GameObject StonePrefab;       // ������ ���� ������
    private bool iscut = false; // ������ �߷ȴ��� ����

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!iscut&&other.gameObject.CompareTag("Sawtooth"))
        {
            iscut = true; // ������ �߷����� ǥ��
            // ��������Ʈ ����
            if (cutVineSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = cutVineSprite;
            }

            // Stone ����
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