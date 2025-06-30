using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChapterManager : MonoBehaviour
{
    [System.Serializable]
    public class StageSlot
    {
        public GameObject slotObject; // 전체 슬롯 오브젝트
        public Image lockImage;       // Lock 이미지
        public string sceneName;      // 해당 스테이지 씬 이름
    }

    public StageSlot[] stages; // Inspector에서 스테이지 슬롯 배열로 할당
    public GameObject confirmPanel; // 확인 패널
    public Text confirmText;        // "이 스테이지로 이동할까요?" 등
    private int selectedIndex = 0;
    private bool isConfirming = false;

    // 예시: PlayerPrefs로 스테이지 클리어 여부 저장 ("Stage1_Clear" == 1)
    private bool IsStageCleared(int idx)
    {
        if (idx == 0) return true; // 첫 스테이지는 항상 오픈
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
            // 확인 패널에서 Space: 진입, ESC: 취소
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

        // 좌우 이동
        int prevIndex = selectedIndex;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) selectedIndex--;
        if (Input.GetKeyDown(KeyCode.RightArrow)) selectedIndex++;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, stages.Length - 1);

        if (prevIndex != selectedIndex)
            UpdateSelectionUI();

        // 선택 확인
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 잠금 해제된 스테이지만 선택 가능
            if (!stages[selectedIndex].lockImage.gameObject.activeSelf)
            {
                isConfirming = true;
                if (confirmPanel != null)
                {
                    confirmPanel.SetActive(true);
                    if (confirmText != null)
                        confirmText.text = $"{stages[selectedIndex].sceneName} 스테이지로 이동할까요?\n[Space: 확인 / ESC: 취소]";
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
        // 선택된 슬롯에만 하이라이트 효과 등(예: 색상 변경)
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
