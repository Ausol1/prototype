using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Player1"))
        {
            GameMgr.Inst.OnPlayerEnterGate(coll.gameObject);
        }
        if (coll.CompareTag("Player2"))
        {
            GameMgr.Inst.OnPlayerEnterGate(coll.gameObject);
        }
    }
}