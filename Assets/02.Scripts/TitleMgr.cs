using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TitleMgr : MonoBehaviour
{
    public Button StartBtn;
    public Button GuideBtn;
    public Button ExitBtn;
    public GameObject GuidePanel;
    public Image FadeImage; // 검은색 이미지(알파 0)로 캔버스에 추가 필요

    private void Start()
    {
        StartBtn.onClick.AddListener(OnClickStartBtn);
        GuideBtn.onClick.AddListener(OnClickGuideBtn);
        ExitBtn.onClick.AddListener(OnClickExitBtn);
    }

    private void OnClickExitBtn()
    {
        Application.Quit();
    }

    private void OnClickGuideBtn()
    {
        GuidePanel.SetActive(true);
    }

    private void OnClickStartBtn()
    {
        StartCoroutine(FadeOutAndLoadScene());
    }

    private IEnumerator FadeOutAndLoadScene()
    {
        float duration = 1.0f;
        float elapsed = 0f;
        Color color = FadeImage.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / duration);
            FadeImage.color = color;
            yield return null;
        }
        SceneManager.LoadScene("Story_1");
    }

    private void Update()
    {

    }
}
