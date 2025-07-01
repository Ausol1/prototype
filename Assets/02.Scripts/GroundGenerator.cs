using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundGenerator : MonoBehaviour
{
    GameObject player1;
    GameObject player2;
    float destroyDistance = 20.0f;  // 플레이어 아래쪽으로 10m

    public GameObject[] Lanes;      // 3개 레인(구름) 오브젝트
    public GameObject VinePrefab;   // Vine 프리팹
    public GameObject TrapPrefab;   // Trap 프리팹 추가

    void Start()
    {
        player1 = GameObject.FindWithTag("Player1"); // 플레이어 태그 사용 권장
        player2 = GameObject.FindWithTag("Player2"); // 플레이어 태그 사용 권장
    }

    void Update()
    {
        if (player1 == null && player2 == null) return;
        Vector3 player1Pos = player1.transform.position;
        Vector3 player2Pos = player2.transform.position;

        // 플레이어로부터 10m 아래를 벗어나면 제거
        if (transform.position.y < player1Pos.y - destroyDistance || transform.position.y < player2Pos.y - destroyDistance)
            Destroy(gameObject);
    }

    // 이전 구름의 활성 레인 정보를 받아서, 양 옆 연결성 보장
    public bool[] SetHideLane(int hideCount, bool[] prevActive)
    {
        // 0~2번까지 3개 레인 준비
        List<int> active = new List<int>();
        for (int i = 0; i < Lanes.Length; i++)
        {
            active.Add(i);
            Lanes[i].SetActive(true); // 먼저 모두 활성화
        }

        // 이전 구름에서 비활성 레인 양 옆은 반드시 활성화
        HashSet<int> mustActive = new HashSet<int>();
        for (int i = 0; i < prevActive.Length; i++)
        {
            if (!prevActive[i])
            {
                if (i > 0) mustActive.Add(i - 1);
                if (i < prevActive.Length - 1) mustActive.Add(i + 1);
            }
        }

        // hideCount만큼 랜덤하게 비활성화 (단, mustActive는 남김)
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

        // 남은 레인(활성화된 곳)에 Vine, Trap 랜덤 생성
        for (int i = 0; i < Lanes.Length; i++)
        {
            if (Lanes[i].activeSelf)
            {
                float rand = Random.value;
                if (rand < 0.2f) // 20% 확률로 Vine
                {
                    SpawnVine(Lanes[i].transform.position);
                }
                else if (rand < 0.3f) // 10% 확률로 Trap (0.2~0.3)
                {
                    SpawnTrap(Lanes[i].transform.position);
                }
            }
        }

        // 현재 활성 상태 반환
        bool[] nowActive = new bool[Lanes.Length];
        for (int i = 0; i < Lanes.Length; i++)
            nowActive[i] = Lanes[i].activeSelf;
        return nowActive;
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