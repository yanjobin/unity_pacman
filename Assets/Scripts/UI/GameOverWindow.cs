using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverWindow : MonoBehaviour
{
    public static GameOverWindow INSTANCE { get; private set; }

    private void Awake()
    {
        INSTANCE = this;

        transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        transform.Find("BtnRetry").GetComponent<Button_UI>().ClickFunc = () => GameManager.INSTANCE.NewGame();

        transform.Find("NewHighscore").gameObject.SetActive(false);

        Hide();
    }

    public void Show(bool newHighscore)
    {
        UpdateHighscore();
        transform.Find("NewHighscore").gameObject.SetActive(newHighscore);
        transform.Find("ScoreText").GetComponent<Text>().text = ScoreManager.Score.ToString();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdateHighscore()
    {
        transform.Find("Highscore").GetComponent<Text>().text = ScoreManager.GetHighscore().ToString();
    }
}
