using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundGenerator : MonoBehaviour
{
    GameObject player1;
    GameObject player2;
    float destroyDistance = 20.0f;  // �÷��̾� �Ʒ������� 10m

    public GameObject[] Lanes;      // 4�� ����(����) ������Ʈ
    public GameObject VinePrefab;   // Vine ������
    public GameObject TrapPrefab;   // Trap ������ �߰�

    void Start()
    {
        player1 = GameObject.FindWithTag("Player1"); // �÷��̾� �±� ��� ����
        player2 = GameObject.FindWithTag("Player2"); // �÷��̾� �±� ��� ����
    }

    void Update()
    {
        if (player1 == null && player2==null) return;
        Vector3 player1Pos = player1.transform.position;
        Vector3 player2Pos = player2.transform.position;

        // �÷��̾�κ��� 10m �Ʒ��� ����� ����
        if (transform.position.y < player1Pos.y - destroyDistance || transform.position.y < player2Pos.y - destroyDistance)
            Destroy(gameObject);
    }

    public void SetHideLane(int hideCount)
    {
        // 0~3������ 4�� ���� �غ�
        List<int> active = new List<int>();
        for (int i = 0; i < Lanes.Length; i++)
        {
            active.Add(i);
        }

        // hideCount��ŭ �����ϰ� ���� ��Ȱ��ȭ
        for (int i = 0; i < hideCount; i++)
        {
            int ran = Random.Range(0, active.Count);
            Lanes[active[ran]].SetActive(false);
            active.RemoveAt(ran);
        }

        // ���� ����(Ȱ��ȭ�� ��)�� Vine, Trap ���� ����
        for (int i = 0; i < Lanes.Length; i++)
        {
            if (Lanes[i].activeSelf)
            {
                float rand = Random.value;
                if (rand < 0.2f) // 30% Ȯ���� Vine
                {
                    SpawnVine(Lanes[i].transform.position);
                }
                else if (rand < 0.3f) // 20% Ȯ���� Trap (0.3~0.5)
                {
                    SpawnTrap(Lanes[i].transform.position);
                }
            }
        }
    }

    void SpawnVine(Vector3 pos)
    {
        GameObject go = Instantiate(VinePrefab);
        go.transform.position = pos + Vector3.down * 0.8f;
    }

    void SpawnTrap(Vector3 pos)
    {
        GameObject go = Instantiate(TrapPrefab);
        go.transform.position = pos + Vector3.up * 0.1f;
    }
}