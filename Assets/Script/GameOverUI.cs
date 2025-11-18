using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI I { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI survivalTimeText;
    [SerializeField] private TextMeshProUGUI restartHintText;

    private void Awake()
    {
        I = this;

        // 시작할 때 게임 오버 패널 숨김
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }


    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 생존 시간 표시
        if (survivalTimeText != null && GameTimer.I != null)
        {
            string timeStr = GameTimer.I.GetFormattedTime();
            survivalTimeText.text = $"생존 시간: {timeStr}";
        }

        // 재시작 안내 메시지
        if (restartHintText != null)
        {
            restartHintText.text = "Space 또는 Enter를 눌러 재시작";
        }
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}
