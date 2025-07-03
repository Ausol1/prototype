using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundWaveGenerator : MonoBehaviour
{
    public GameObject GroundWavePrefabA; // 땅 프리팹 A
    public GameObject GroundWavePrefabB; // 땅 프리팹 B
    public GameObject WallPrefab;        // 벽 프리팹 추가
    public float groundSpacing = 2.5f;   // 땅층 간격
    public float createRange = 10.0f;    // 플레이어 위로 생성할 범위

    private float nextGroundHeight = -10.5f;
    private GameObject player1;
    private GameObject player2;

    // 이전 구름의 레인 활성 상태 저장 (처음엔 모두 true)
    private bool[] prevActiveLanes = new bool[] { true, true, true };

    void Start()
    {
        player1 = GameObject.FindWithTag("Player1");
        player2 = GameObject.FindWithTag("Player2");
    }

    void Update()
    {
        if (player1 == null && player2 == null) return;
        Vector3 player1Pos = player1.transform.position;
        Vector3 player2Pos = player2.transform.position;

        // 플레이어 위로 일정 높이까지 땅 생성
        while (nextGroundHeight < player1Pos.y + createRange || nextGroundHeight < player2Pos.y + createRange)
        {
            SpawnGroundWave(nextGroundHeight);
            nextGroundHeight += groundSpacing;
        }
    }

    void SpawnGroundWave(float height)
    {
        // 두 종류 중 랜덤으로 프리팹 선택
        GameObject prefab = (Random.value < 0.4f) ? GroundWavePrefabA : GroundWavePrefabB;
        GameObject groundWave = Instantiate(prefab);
        groundWave.transform.position = new Vector3(0.0f, height, 0.0f);

        // 20% 확률 + y좌표 10f 이상일 때만 벽 생성
        if (WallPrefab != null && height >= 10f && Random.value < 0.3f)
        {
            GameObject wall = Instantiate(WallPrefab);
            wall.transform.position = new Vector3(0.0f, height, 0.0f);
        }

        // GroundGenerator의 SetHideLane 사용 (이전 레인 정보 전달)
        GroundGenerator laneGen = groundWave.GetComponent<GroundGenerator>();
        if (laneGen != null)
        {
            int hideCount = Random.Range(0, 3); // 0~2개 레인 비활성화
            prevActiveLanes = laneGen.SetHideLane(hideCount, prevActiveLanes);
        }
    }
}