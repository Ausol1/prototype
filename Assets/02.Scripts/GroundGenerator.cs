using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundGenerator : MonoBehaviour
{
    GameObject player1;
    GameObject player2;
    float destroyDistance = 20.0f;  // �÷��̾� �Ʒ������� 10m

    public GameObject[] Lanes;      // 3�� ����(����) ������Ʈ
    public GameObject VinePrefab;   // Vine ������
    public GameObject TrapPrefab;   // Trap ������ �߰�

    void Start()
    {
        player1 = GameObject.FindWithTag("Player1"); // �÷��̾� �±� ��� ����
        player2 = GameObject.FindWithTag("Player2"); // �÷��̾� �±� ��� ����
    }

    void Update()
    {
        if (player1 == null && player2 == null) return;
        Vector3 player1Pos = player1.transform.position;
        Vector3 player2Pos = player2.transform.position;

        // �÷��̾�κ��� 10m �Ʒ��� ����� ����
        if (transform.position.y < player1Pos.y - destroyDistance || transform.position.y < player2Pos.y - destroyDistance)
            Destroy(gameObject);
    }

    // ���� ������ Ȱ�� ���� ������ �޾Ƽ�, �� �� ���Ἲ ����
    public bool[] SetHideLane(int hideCount, bool[] prevActive)
    {
        // 0~2������ 3�� ���� �غ�
        List<int> active = new List<int>();
        for (int i = 0; i < Lanes.Length; i++)
        {
            active.Add(i);
            Lanes[i].SetActive(true); // ���� ��� Ȱ��ȭ
        }

        // ���� �������� ��Ȱ�� ���� �� ���� �ݵ�� Ȱ��ȭ
        HashSet<int> mustActive = new HashSet<int>();
        for (int i = 0; i < prevActive.Length; i++)
        {
            if (!prevActive[i])
            {
                if (i > 0) mustActive.Add(i - 1);
                if (i < prevActive.Length - 1) mustActive.Add(i + 1);
            }
        }

        // hideCount��ŭ �����ϰ� ��Ȱ��ȭ (��, mustActive�� ����)
        List<int> candidates = new List<int>(active);
        foreach (var idx in mustActive)
            candidates.Remove(idx);

        int toHide = Mathf.Min(hideCount, candidates.Count);
        for (int i = 0; i < toHide; i++)
        {
            int ran = Random.Range(0, candidates.Count);
            Lanes[candidates[ran]].SetActive(false);
            active.Remove(candidates[ran]);
            candidates.RemoveAt(ran);
        }

        // ���� ����(Ȱ��ȭ�� ��)�� Vine, Trap ���� ����
        for (int i = 0; i < Lanes.Length; i++)
        {
            if (Lanes[i].activeSelf)
            {
                float rand = Random.value;
                if (VinePrefab !=null&&rand < 0.2f) // 20% Ȯ���� Vine
                {
                    SpawnVine(Lanes[i].transform.position);
                }
                else if (rand < 0.3f) // 10% Ȯ���� Trap (0.2~0.3)
                {
                    SpawnTrap(Lanes[i].transform.position);
                }
            }
        }

        // ���� Ȱ�� ���� ��ȯ
        bool[] nowActive = new bool[Lanes.Length];
        for (int i = 0; i < Lanes.Length; i++)
            nowActive[i] = Lanes[i].activeSelf;
        return nowActive;
    }

    void SpawnVine(Vector3 pos)
    {
        float randomX = Random.Range(-1f, 1f);
        GameObject go = Instantiate(VinePrefab);
        go.transform.position = new Vector3(pos.x + randomX, pos.y - 0.3f, pos.z);
    }

    void SpawnTrap(Vector3 pos)
    {
        GameObject go = Instantiate(TrapPrefab);
        go.transform.position = pos + Vector3.up * 0.1f;
    }
}