using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlow : MonoBehaviour
{
    public static GameFlow I { get; private set; }
    public bool IsRunning { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!IsRunning && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            if (IsGameOver)
            {
                Restart();
            }
            else
            {
                StartRun();
            }
        }
    }

    public void StartRun()
    {
        IsGameOver = false;
        IsRunning = true;

        // Ÿ�̸� ����
        if (GameTimer.I != null)
        {
            GameTimer.I.ResetTimer();
        }
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsRunning = false;
        IsGameOver = true;
        Debug.Log("[GameFlow] Game Over! IsGameOver: " + IsGameOver + ", IsRunning: " + IsRunning);

        // 게임 오버 UI 표시
        if (GameOverUI.I != null)
        {
            GameOverUI.I.ShowGameOver();
        }
    }

    public void Restart()
    {
        // 싱글톤 인스턴스 리셋
        I = null;

        // 현재 GameObject 파괴 (DontDestroyOnLoad 해제)
        Destroy(gameObject);

        // 씬 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}