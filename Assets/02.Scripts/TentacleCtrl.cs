using UnityEngine;

public class TentacleCtrl : MonoBehaviour
{
    bool isAttacking = false;

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if(!isAttacking && (coll.CompareTag("Player1") || coll.CompareTag("Player2")))
        {
            Player player = GetComponent<Player>();
            player.TakeDamage(20);
            isAttacking = true;
        }

    }

}
