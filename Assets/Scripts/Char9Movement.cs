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

    private CharacterController controller;
    private Transform cameraTransform;
    private float verticalSpeed;

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
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        isInteracting = false;

        if (characterAnimation != null && characterAnimation["Death"] != null)
        {
            characterAnimation.Stop();
            characterAnimation["Death"].time = 0f;
            characterAnimation.Play("Death", PlayMode.StopAll);
            currentAnimation = "Death";
        }

        controller.enabled = false;
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
}