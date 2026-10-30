using UnityEngine;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject lossPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Player")]
    [SerializeField] private Char9Movement player;

    private bool isPaused;
    private bool lossShown;

    private void Start()
    {
        Time.timeScale = 1f;

        if (lossPanel != null) lossPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (player == null) player = FindObjectOfType<Char9Movement>();
    }

    private void Update()
    {
        if (player != null && player.IsDead)
        {
            ShowLoss();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) TogglePause();
    }

    public void ShowLoss()
    {
        if (lossShown) return;

        lossShown = true;
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (lossPanel != null) lossPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void TogglePause()
    {
        if (lossShown) return;

        isPaused = !isPaused;
        if (pausePanel != null) pausePanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        if (lossShown) return;

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
