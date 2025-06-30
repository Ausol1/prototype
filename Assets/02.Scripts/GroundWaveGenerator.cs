using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundWaveGenerator : MonoBehaviour
{
    public GameObject GroundWavePrefabA; // 땅 프리팹 A
    public GameObject GroundWavePrefabB; // 땅 프리팹 B
    public float groundSpacing = 2.5f;   // 땅층 간격
    public float createRange = 10.0f;    // 플레이어 위로 생성할 범위

    private float nextGroundHeight = -10.5f;
    private GameObject player1;
    private GameObject player2;

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
        GameObject prefab = (Random.value < 0.5f) ? GroundWavePrefabA : GroundWavePrefabB;
        GameObject groundWave = Instantiate(prefab);
        groundWave.transform.position = new Vector3(0.0f, height, 0.0f);

        // GroundGenerator의 SetHideLane 사용
        GroundGenerator laneGen = groundWave.GetComponent<GroundGenerator>();
        if (laneGen != null)
        {
            int hideCount = Random.Range(0, 3); // 0~2개 레인 비활성화
            laneGen.SetHideLane(hideCount);
        }
    }
}