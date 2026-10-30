
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Char9Movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float rotationSpeed = 10f;
    public float gravity = -15f;

    [Header("Jump")]
    public float jumpHeight = 2.5f;

    [Header("Legacy Animation")]
    public Animation characterAnimation;
    public float animationFadeTime = 0.12f;

    [Header("Audio")]
    [SerializeField] private AudioClip runSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip interactionSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip doorSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField, Min(0.1f)] private float runSoundInterval = 0.5f;

    private CharacterController controller;
    private Transform cameraTransform;
    private AudioSource audioSource;
    private float verticalSpeed;
    private float nextRunSoundAt;

    private bool isDead = false;
    private bool isInteracting = false;

    private float interactionEndTime;
    private string currentAnimation = "";

    public bool IsDead => isDead;
    public bool CanInteract => !isDead && !isInteracting && controller != null && controller.enabled && controller.isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main != null ? Camera.main.transform : null;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (characterAnimation == null)
        {
            characterAnimation = GetComponentInChildren<Animation>();
        }

        if (characterAnimation == null) return;

        SetupAnimation("Idle", WrapMode.Loop);
        SetupAnimation("Run", WrapMode.Loop);
        SetupAnimation("Jump", WrapMode.Once);
        SetupAnimation("Interact", WrapMode.Once);
        SetupAnimation("Death", WrapMode.ClampForever);

        PlayAnimation("Idle");
    }

    void Update()
    {
        if (isDead) return;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (isInteracting && Time.time >= interactionEndTime)
        {
            isInteracting = false;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 right = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : Vector3.right;
        Vector3 direction = Vector3.ClampMagnitude(right * horizontal + forward * vertical, 1f);

        if (controller.isGrounded)
        {
            if (verticalSpeed < 0f)
            {
                verticalSpeed = -1f;
            }

            if (!isInteracting && Input.GetKeyDown(KeyCode.Space))
            {
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
                PlayAnimation("Jump");
                PlaySound(jumpSound);
            }
        }

        verticalSpeed += gravity * Time.deltaTime;

        Vector3 movement = isInteracting ? Vector3.zero : direction * moveSpeed;
        movement += Vector3.up * verticalSpeed;

        controller.Move(movement * Time.deltaTime);

        if (isDead) return;

        if (!isInteracting && direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        SetRunSound(!isInteracting && controller.isGrounded && direction.sqrMagnitude > 0.001f);

        if (isInteracting)
        {
            PlayAnimation("Interact");
            return;
        }

        if (!controller.isGrounded)
        {
            PlayAnimation("Jump");
        }
        else
        {
            PlayAnimation(direction.sqrMagnitude > 0.001f ? "Run" : "Idle");
        }
    }

    public void Interact()
    {
        if (isDead || isInteracting) return;

        isInteracting = true;
        float duration = 1f;

        if (characterAnimation != null && characterAnimation["Interact"] != null)
        {
            duration = characterAnimation["Interact"].length;
        }

        interactionEndTime = Time.time + duration;
        PlayAnimation("Interact");

        SetRunSound(false);
        PlaySound(interactionSound);
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        isInteracting = false;

        SetRunSound(false);
        PlaySound(deathSound);

        if (characterAnimation != null && characterAnimation["Death"] != null)
        {
            characterAnimation.Stop();
            characterAnimation["Death"].time = 0f;
            characterAnimation.Play("Death", PlayMode.StopAll);
            currentAnimation = "Death";
        }

        controller.enabled = false;
    }

    public void PlayDoorSound()
    {
        PlaySound(doorSound);
    }

    public void PlayWinSound()
    {
        PlaySound(winSound);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        TrapDeath trap = hit.collider.GetComponentInParent<TrapDeath>();
        DeathZone fallZone = hit.collider.GetComponentInParent<DeathZone>();

        if (trap != null) trap.KillPlayer(this);
        if (fallZone != null) fallZone.KillPlayer(this);
    }

    private void SetupAnimation(string animationName, WrapMode wrapMode)
    {
        if (characterAnimation == null) return;

        AnimationState state = characterAnimation[animationName];

        if (state == null) return;

        state.wrapMode = wrapMode;
    }

    private void PlayAnimation(string animationName)
    {
        if (characterAnimation == null || characterAnimation[animationName] == null) return;
        if (currentAnimation == animationName) return;

        characterAnimation.CrossFade(animationName, animationFadeTime);
        currentAnimation = animationName;
    }

    private void SetRunSound(bool shouldPlay)
    {
        if (!shouldPlay || runSound == null)
        {
            nextRunSoundAt = 0f;
            return;
        }

        if (Time.time < nextRunSoundAt) return;

        PlaySound(runSound);
        nextRunSoundAt = Time.time + runSoundInterval;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}
