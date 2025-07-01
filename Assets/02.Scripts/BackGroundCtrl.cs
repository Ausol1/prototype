using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BackGroundCtrl : MonoBehaviour
{
    GameObject player1; 
    GameObject player2;
    public float startY = -18.4f;   //��׶����� ���� y ���� ��ġ
    float startX = -0.3f;   
    public float scroll = 0.01f;    //��׶��尡 ���� �ö󰡴� ����
    void Start()
    {
        player1 = GameObject.Find("Player1");
        player2 = GameObject.Find("Player2");
    }

    void Update()
    {
        float avgY = (player1.transform.position.y + player2.transform.position.y) / 2f;
        float bgY = startY + avgY * scroll; // scroll�� 0.01�̸� �÷��̾�� �ξ� �� �ö�

        transform.position = new Vector3(startX, bgY, 0.0f);
    }
}