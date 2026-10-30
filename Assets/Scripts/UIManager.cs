using UnityEngine;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject lossPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Player")]
    [SerializeField] private Char9Movement player;

    private bool isPaused;
    private bool lossShown;
    private bool winShown;

    private void Start()
    {
        Time.timeScale = 1f;

        if (winPanel != null) winPanel.SetActive(false);
        if (lossPanel != null) lossPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (player == null) player = FindObjectOfType<Char9Movement>();
    }

    private void Update()
    {
        if (winShown || lossShown) return;

        if (player != null && player.IsDead)
        {
            ShowLoss();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) TogglePause();
    }

    public void ShowWin()
    {
        if (winShown || lossShown) return;
        if (player != null && player.IsDead) return;

        winShown = true;
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (lossPanel != null) lossPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ShowLoss()
    {
        if (lossShown || winShown) return;

        lossShown = true;
        isPaused = false;

        if (winPanel != null) winPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (lossPanel != null) lossPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void TogglePause()
    {
        if (lossShown || winShown) return;

        isPaused = !isPaused;
        if (pausePanel != null) pausePanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        if (lossShown || winShown) return;

        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;
    }
}
