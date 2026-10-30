
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GoalTrigger : MonoBehaviour
{
    [Header("Win Effect")]
    [SerializeField] private ParticleSystem winVFX;

    [Header("UI")]
    [SerializeField] private UIManager uiManager;

    private bool hasWon;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasWon) return;

        Char9Movement player = other.GetComponentInParent<Char9Movement>();
        if (player == null || player.IsDead) return;

        hasWon = true;
        StartCoroutine(PlayWinSequence());
    }

    private IEnumerator PlayWinSequence()
    {
        Time.timeScale = 0f;

        if (winVFX != null)
        {
            var main = winVFX.main;
            main.useUnscaledTime = true;

            winVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            winVFX.Play(true);

            yield return new WaitUntil(() => !winVFX.IsAlive(true));
        }

        if (uiManager != null) uiManager.ShowWin();
    }
}
