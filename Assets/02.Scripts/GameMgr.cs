using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMgr : MonoBehaviour
{
    [SerializeField] private GameObject gateObject; // 게이트 오브젝트 할당
    [SerializeField] private GameObject gameOverPanel; // GameOver 패널 오브젝트 할당

    //--- 싱글턴 패턴
    public static GameMgr Inst = null;

    private int playerInGateCount = 0;
    private int totalPlayers = 2; // 플레이어 수
                                  // 버튼 오브젝트를 에디터에서 할당할 수 있도록 public으로 선언
    public Button restartButton;
    public Button exitButton;
    private void Awake()
    {
        Inst = this;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); // 시작 시 비활성화
    }
    private void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnClickRestart);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnClickExit);
    }

    void Update()
    {
        // "Enemy" 태그를 가진 오브젝트가 없으면 게이트 오브젝트 활성화
        if (gateObject != null && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            if (!gateObject.activeSelf)
                gateObject.SetActive(true);
        }
    }

    // 게이트에 플레이어가 닿았을 때 호출
    public void OnPlayerEnterGate(GameObject player)
    {
        if (player != null)
        {
            player.SetActive(false); // 플레이어 비활성화(사라짐)
            playerInGateCount++;

            if (playerInGateCount >= totalPlayers)
            {
                SceneManager.LoadScene("Stage_2");
            }
        }
    }

    // 플레이어가 죽었을 때 호출
    public void OnPlayerDead()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0.0f;
    }

    // RE 버튼 (Restart)
    public void OnClickRestart()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Exit 버튼 (TitleScene으로)
    public void OnClickExit()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("TitleScene");
    }

}

