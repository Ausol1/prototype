using UnityEngine;

public class GameMgr : MonoBehaviour
{
    //Application.targetFrameRate = 60;

    //--- �̱��� ����
    public static GameMgr Inst = null;
    private void Awake()
    {
        Inst = this;
    }
  
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
