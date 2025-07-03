using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundWaveGenerator : MonoBehaviour
{
    public GameObject GroundWavePrefabA; // �� ������ A
    public GameObject GroundWavePrefabB; // �� ������ B
    public GameObject WallPrefab;        // �� ������ �߰�
    public float groundSpacing = 2.5f;   // ���� ����
    public float createRange = 10.0f;    // �÷��̾� ���� ������ ����

    private float nextGroundHeight = -10.5f;
    private GameObject player1;
    private GameObject player2;

    // ���� ������ ���� Ȱ�� ���� ���� (ó���� ��� true)
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

        // �÷��̾� ���� ���� ���̱��� �� ����
        while (nextGroundHeight < player1Pos.y + createRange || nextGroundHeight < player2Pos.y + createRange)
        {
            SpawnGroundWave(nextGroundHeight);
            nextGroundHeight += groundSpacing;
        }
    }

    void SpawnGroundWave(float height)
    {
        // �� ���� �� �������� ������ ����
        GameObject prefab = (Random.value < 0.4f) ? GroundWavePrefabA : GroundWavePrefabB;
        GameObject groundWave = Instantiate(prefab);
        groundWave.transform.position = new Vector3(0.0f, height, 0.0f);

        // 20% Ȯ�� + y��ǥ 10f �̻��� ���� �� ����
        if (WallPrefab != null && height >= 10f && Random.value < 0.3f)
        {
            GameObject wall = Instantiate(WallPrefab);
            wall.transform.position = new Vector3(0.0f, height, 0.0f);
        }

        // GroundGenerator�� SetHideLane ��� (���� ���� ���� ����)
        GroundGenerator laneGen = groundWave.GetComponent<GroundGenerator>();
        if (laneGen != null)
        {
            int hideCount = Random.Range(0, 3); // 0~2�� ���� ��Ȱ��ȭ
            prevActiveLanes = laneGen.SetHideLane(hideCount, prevActiveLanes);
        }
    }
}