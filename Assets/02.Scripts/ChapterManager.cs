using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChapterManager : MonoBehaviour
{
    [System.Serializable]
    public class StageSlot
    {
        public GameObject slotObject; // ��ü ���� ������Ʈ
        public Image lockImage;       // Lock �̹���
        public string sceneName;      // �ش� �������� �� �̸�
    }

    public StageSlot[] stages; // Inspector���� �������� ���� �迭�� �Ҵ�
    public GameObject confirmPanel; // Ȯ�� �г�
    public Text confirmText;        // "�� ���������� �̵��ұ��?" ��
    private int selectedIndex = 0;
    private bool isConfirming = false;

    // ����: PlayerPrefs�� �������� Ŭ���� ���� ���� ("Stage1_Clear" == 1)
    private bool IsStageCleared(int idx)
    {
        if (idx == 0) return true; // ù ���������� �׻� ����
        return PlayerPrefs.GetInt($"Stage{idx}_Clear", 0) == 1;
    }

    void Start()
    {
        UpdateStageLocks();
        UpdateSelectionUI();
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    void Update()
    {
        if (isConfirming)
        {
            // Ȯ�� �гο��� Space: ����, ESC: ���
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(stages[selectedIndex].sceneName);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                isConfirming = false;
                if (confirmPanel != null) confirmPanel.SetActive(false);
            }
            return;
        }

        // �¿� �̵�
        int prevIndex = selectedIndex;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) selectedIndex--;
        if (Input.GetKeyDown(KeyCode.RightArrow)) selectedIndex++;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, stages.Length - 1);

        if (prevIndex != selectedIndex)
            UpdateSelectionUI();

        // ���� Ȯ��
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // ��� ������ ���������� ���� ����
            if (!stages[selectedIndex].lockImage.gameObject.activeSelf)
            {
                isConfirming = true;
                if (confirmPanel != null)
                {
                    confirmPanel.SetActive(true);
                    if (confirmText != null)
                        confirmText.text = $"{stages[selectedIndex].sceneName} ���������� �̵��ұ��?\n[Space: Ȯ�� / ESC: ���]";
                }
            }
        }
    }

    void UpdateStageLocks()
    {
        for (int i = 0; i < stages.Length; i++)
        {
            bool unlocked = IsStageCleared(i);
            if (stages[i].lockImage != null)
                stages[i].lockImage.gameObject.SetActive(!unlocked);
        }
    }

    void UpdateSelectionUI()
    {
        // ���õ� ���Կ��� ���̶���Ʈ ȿ�� ��(��: ���� ����)
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].slotObject != null)
            {
                Image img = stages[i].slotObject.GetComponent<Image>();
                if (img != null)
                    img.color = (i == selectedIndex) ? Color.yellow : Color.white;
            }
        }
    }
}
