using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMgr : MonoBehaviour
{
    [SerializeField] private GameObject gateObject; // ����Ʈ ������Ʈ �Ҵ�
    [SerializeField] private GameObject gameOverPanel; // GameOver �г� ������Ʈ �Ҵ�

    //--- �̱��� ����
    public static GameMgr Inst = null;

    private int playerInGateCount = 0;
    private int totalPlayers = 2; // �÷��̾� ��
                                  // ��ư ������Ʈ�� �����Ϳ��� �Ҵ��� �� �ֵ��� public���� ����
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
    }

    void Update()
    {
        // "Enemy" �±׸� ���� ������Ʈ�� ������ ����Ʈ ������Ʈ Ȱ��ȭ
        if (gateObject != null && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            if (!gateObject.activeSelf)
                gateObject.SetActive(true);
        }
    }

    // ����Ʈ�� �÷��̾ ����� �� ȣ��
    public void OnPlayerEnterGate(GameObject player)
    {
        if (player != null)
        {
            player.SetActive(false); // �÷��̾� ��Ȱ��ȭ(�����)
            playerInGateCount++;

            if (playerInGateCount >= totalPlayers)
            {
                SceneManager.LoadScene("Stage_2");
            }
        }
    }

    // �÷��̾ �׾��� �� ȣ��
    public void OnPlayerDead()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0.0f;
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

}

