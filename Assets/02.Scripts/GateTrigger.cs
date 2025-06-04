using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            GameMgr.Inst.OnPlayerEnterGate(coll.gameObject);
        }
    }
}