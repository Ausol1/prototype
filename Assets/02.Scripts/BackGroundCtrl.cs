using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundCtrl : MonoBehaviour
{
    GameObject player1;
    GameObject player2;
    float startY = -1.9f;   //백그라운드의 시작 y 높이 위치
    float startX = -2.2f;   //백그라운드의 시작 x 높이 위치
    float scroll = 0.0001f;    //백그라운드가 위로 올라가는 속도

    // Start is called before the first frame update
    void Start()
    {
        player1 = GameObject.Find("Player1");
        player2 = GameObject.Find("Player2");
    }

    // Update is called once per frame
    void Update()
    {
        float scrollPos = startY - (player1.transform.position.y+player2.transform.position.y/2) * scroll;
        if (scrollPos > 12.0f)
            scrollPos = 12.0f;
        else if (scrollPos < -12.0f)
            scrollPos = -12.0f;

        transform.position = new Vector3(startX,
                        (player1.transform.position.y + player2.transform.position.y / 2) + scrollPos, 0.0f);
    }
}
