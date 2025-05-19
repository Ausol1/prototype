using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryMgr : MonoBehaviour
{
    public TextMeshProUGUI fatherText;
    public TextMeshProUGUI sonText;
    public float typingSpeed = 0.1f;
    public Image FadeImage; // ������ �̹���(���� 1)�� ĵ������ �߰� �ʿ�

    // ȭ�ڿ� ��縦 �Բ� ����
    [System.Serializable]
    public struct DialogueLine
    {
        public string speaker; // "father" or "son"
        public string text;
    }

    public DialogueLine[] lines;

    private int index = 0;
    private bool isTyping = false;
    private bool isLineFullyShown = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float duration = 1.0f;
        float elapsed = 0f;
        Color color = FadeImage.color;
        color.a = 1f;
        FadeImage.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsed / duration);
            FadeImage.color = color;
            yield return null;
        }
        color.a = 0f;
        FadeImage.color = color;

        StartTypingLine();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                ShowFullLine();
            }
            else if (isLineFullyShown)
            {
                NextLine();
            }
        }
    }

    void StartTypingLine()
    {
        typingCoroutine = StartCoroutine(TypeLine(lines[index]));
    }

    IEnumerator TypeLine(DialogueLine line)
    {
        isTyping = true;
        isLineFullyShown = false;

        // �ؽ�Ʈ ����
        fatherText.text = "";
        sonText.text = "";

        TextMeshProUGUI targetText = GetTargetText(line.speaker);

        foreach (char c in line.text)
        {
            targetText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        isLineFullyShown = true;
    }

    void ShowFullLine()
    {
        // ��� ��ü ���� �����ֱ�
        DialogueLine line = lines[index];
        GetTargetText(line.speaker).text = line.text;

        isTyping = false;
        isLineFullyShown = true;
    }

    void NextLine()
    {
        index++;
        if (index < lines.Length)
        {
            StartTypingLine();
        }
        else
        {
            StartCoroutine(FadeOutAndLoadScene());
        }
    }

    private IEnumerator FadeOutAndLoadScene()
    {
        float duration = 1.0f;
        float elapsed = 0f;
        Color color = FadeImage.color;
        color.a = 0f;
        FadeImage.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / duration);
            FadeImage.color = color;
            yield return null;
        }
        SceneManager.LoadScene("Stage_1");
    }

    TextMeshProUGUI GetTargetText(string speaker)
    {
        return speaker.ToLower() == "father" ? fatherText : sonText;
    }
}
