using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundCtrl : MonoBehaviour
{
    GameObject player1;
    GameObject player2;
    float startY = -1.9f;   //��׶����� ���� y ���� ��ġ
    float startX = -2.2f;   //��׶����� ���� x ���� ��ġ
    float scroll = 0.0001f;    //��׶��尡 ���� �ö󰡴� �ӵ�

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
