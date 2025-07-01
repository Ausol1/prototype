using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BackGroundCtrl : MonoBehaviour
{
    GameObject player1; 
    GameObject player2;
    public float startY = -18.4f;   //백그라운드의 시작 y 높이 위치
    float startX = -0.3f;   
    public float scroll = 0.01f;    //백그라운드가 위로 올라가는 비율
    void Start()
    {
        player1 = GameObject.Find("Player1");
        player2 = GameObject.Find("Player2");
    }

    void Update()
    {
        float avgY = (player1.transform.position.y + player2.transform.position.y) / 2f;
        float bgY = startY + avgY * scroll; // scroll이 0.01이면 플레이어보다 훨씬 덜 올라감

        transform.position = new Vector3(startX, bgY, 0.0f);
    }
}