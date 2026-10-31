
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GoalTrigger : MonoBehaviour
{
    [Header("Win Effect")]
    [SerializeField] private ParticleSystem winVFX;

    [SerializeField, Min(0.1f)]
    private float maxVFXWaitTime = 5f;

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

        player.PlayWinSound();
        StartCoroutine(PlayWinSequence());
    }

    private IEnumerator PlayWinSequence()
    {
        Time.timeScale = 0f;

        if (winVFX != null)
        {
            ParticleSystem[] systems = winVFX.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem system in systems)
            {
                var main = system.main;
                main.useUnscaledTime = true;
            }

            winVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            winVFX.Play(true);

            float elapsed = 0f;

            while (winVFX.IsAlive(true) && elapsed < maxVFXWaitTime)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (winVFX.IsAlive(true))
            {
                winVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();

        if (uiManager != null)
        {
            uiManager.ShowWin();
        }
    }
}
