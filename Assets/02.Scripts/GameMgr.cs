using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMgr : MonoBehaviour
{
    [SerializeField] private GameObject gateObject;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gamePausePanel;

    // --- ���������� �� �г� ---
    public GameObject stage1TipPanel;
    public GameObject stage2TipPanel;
    public GameObject stage3TipPanel;
    public GameObject stage4TipPanel;

    private bool isStage1TipActive = false;
    private bool isStage2TipActive = false;
    private bool isStage3TipActive = false;
    private bool isStage4TipActive = false;

    private static bool isStage1FirstLoad = true;
    private static bool isStage2FirstLoad = true;
    private static bool isStage3FirstLoad = true;
    private static bool isStage4FirstLoad = true;

    //--- �̱��� ����
    public static GameMgr Inst = null;

    private int playerInGateCount = 0;
    private int totalPlayers = 2; // �÷��̾� ��
    private int deadPlayerCount = 0;

    public int currentStage = 1;

    public Button restartButton;
    public Button exitButton;
    private void Awake()
    {
        Inst = this;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); // ���� �� ��Ȱ��ȭ
    }
    private void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnClickRestart);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnClickExit);

        // ���������� �� �г� ���� ���� �ø� �����ֱ�
        if (currentStage == 1 && isStage1FirstLoad && stage1TipPanel != null)
        {
            stage1TipPanel.SetActive(true);
            Time.timeScale = 0.0f;
            isStage1TipActive = true;
            isStage1FirstLoad = false;
        }
        else if (currentStage == 2 && isStage2FirstLoad && stage2TipPanel != null)
        {
            stage2TipPanel.SetActive(true);
            Time.timeScale = 0.0f;
            isStage2TipActive = true;
            isStage2FirstLoad = false;
        }
        else if (currentStage == 3 && isStage3FirstLoad && stage3TipPanel != null)
        {
            stage3TipPanel.SetActive(true);
            Time.timeScale = 0.0f;
            isStage3TipActive = true;
            isStage3FirstLoad = false;
        }
        else if (currentStage == 4 && isStage4FirstLoad && stage4TipPanel != null)
        {
            stage4TipPanel.SetActive(true);
            Time.timeScale = 0.0f;
            isStage4TipActive = true;
            isStage4FirstLoad = false;
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(OnClickRestart);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnClickExit);
    }

    void Update()
    {
        // �� �г��� ���������� Space�� �ݱ�
        if (isStage1TipActive && stage1TipPanel != null && stage1TipPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stage1TipPanel.SetActive(false);
                Time.timeScale = 1.0f;
                isStage1TipActive = false;
            }
            return;
        }
        if (isStage2TipActive && stage2TipPanel != null && stage2TipPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stage2TipPanel.SetActive(false);
                Time.timeScale = 1.0f;
                isStage2TipActive = false;
            }
            return;
        }
        if (isStage3TipActive && stage3TipPanel != null && stage3TipPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stage3TipPanel.SetActive(false);
                Time.timeScale = 1.0f;
                isStage3TipActive = false;
            }
            return;
        }
        if (isStage4TipActive && stage4TipPanel != null && stage4TipPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stage4TipPanel.SetActive(false);
                Time.timeScale = 1.0f;
                isStage4TipActive = false;
            }
            return;
        }

        // "Enemy" �±׸� ���� ������Ʈ�� ������ ����Ʈ ������Ʈ Ȱ��ȭ
        if (gateObject != null && GameObject.FindGameObjectsWithTag("Enemy").Length == 0 &&
            GameObject.FindGameObjectsWithTag("SmallMonster").Length == 0 &&
            GameObject.FindGameObjectsWithTag("MiddleBoss").Length == 0)
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

    // ����Ʈ�� �÷��̾ ����� �� ȣ��
    public void OnPlayerEnterGate(GameObject player)
    {
        if (player != null)
        {
            player.SetActive(false); // �÷��̾� ��Ȱ��ȭ(�����)
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

    // �÷��̾ �׾��� �� ȣ��
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
            gameOverPanel.SetActive(true); // GameOver �г� Ȱ��ȭ
            Time.timeScale = 0.0f; // ���� �Ͻ�����
        }
    }

    // RE ��ư (Restart)
    public void OnClickRestart()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Exit ��ư (TitleScene����)
    public void OnClickExit()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("TitleScene");
    }

    public void OnStart()
    { 
        if (gamePausePanel != null)
            gamePausePanel.SetActive(false); 
        Time.timeScale = 1.0f; // ���� ���� �� �ð� �帧 �簳
    }

}

