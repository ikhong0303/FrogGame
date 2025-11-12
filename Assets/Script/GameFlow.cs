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

        // 타이머 리셋
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
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}