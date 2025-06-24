using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMgr : MonoBehaviour
{
    [SerializeField] private GameObject gateObject; 
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gamePausePanel;

    //--- 싱글턴 패턴
    public static GameMgr Inst = null;

    private int playerInGateCount = 0;
    private int totalPlayers = 2; // 플레이어 수
    private int deadPlayerCount = 0;

    public int currentStage = 1;

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
        if (gateObject != null && GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && GameObject.FindGameObjectsWithTag("SnallMonster").Length == 0)
        {
            if (!gateObject.activeSelf)
                gateObject.SetActive(true);
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePausePanel != null)
            {
                gamePausePanel.SetActive(!gamePausePanel.activeSelf);
                Time.timeScale = gamePausePanel.activeSelf ? 0.0f : 1.0f; 
            }
        }
    }

    // 게이트에 플레이어가 닿았을 때 호출
    public void OnPlayerEnterGate(GameObject player)
    {
        if (player != null)
        {
            player.SetActive(false); // 플레이어 비활성화(사라짐)
            playerInGateCount++;

            if (playerInGateCount >= totalPlayers && currentStage == 1)
            {
                SceneManager.LoadScene("Stage_2");
                currentStage = 2; 
            }
            else if (playerInGateCount >= totalPlayers && currentStage == 2)
            {
                SceneManager.LoadScene("Stage_3");
                currentStage = 3;
            }
            else if (playerInGateCount >= totalPlayers && currentStage == 3)
            {
                SceneManager.LoadScene("Stage_4");
                currentStage = 4; 
            }
            else if (playerInGateCount >= totalPlayers && currentStage == 4)
            {
                SceneManager.LoadScene("TitleScene"); 
            }
        }
    }

    // 플레이어가 죽었을 때 호출
    public void OnPlayerDead()
    {
        deadPlayerCount++;
        if (deadPlayerCount >= 2)
        {
            GameOver();
        }
    }

    public void OnPlayerRevive()
    {
        if (deadPlayerCount > 0)
            deadPlayerCount--;
    }
    private void GameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // GameOver 패널 활성화
            Time.timeScale = 0.0f; // 게임 일시정지
        }
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

    public void OnStart()
    { 
        if (gamePausePanel != null)
            gamePausePanel.SetActive(false); 
        Time.timeScale = 1.0f; // 게임 시작 시 시간 흐름 재개
    }

}

