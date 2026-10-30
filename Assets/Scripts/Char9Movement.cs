
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Char9Movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    [Header("Jump")]
    public float jumpHeight = 2f;

    [Header("Legacy Animation")]
    public Animation characterAnimation;
    public float animationFadeTime = 0.12f;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    private CharacterController controller;
    private Vector3 velocity;

    private bool isDead = false;
    private bool isInteracting = false;
    private bool isJumping = false;

    private float interactionEndTime;
    private string currentAnimation = "";

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (characterAnimation == null)
        {
            characterAnimation = GetComponentInChildren<Animation>();
        }

        if (characterAnimation == null)
        {
            Debug.LogError("Char9Movement: Không tìm thấy Animation component!");
            return;
        }

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

        if (isInteracting && Time.time >= interactionEndTime)
        {
            isInteracting = false;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (!isInteracting && direction.magnitude > 0.1f)
        {
            controller.Move(direction * moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (isGrounded && !isInteracting && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
            PlayAnimation("Jump");
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (controller.isGrounded && velocity.y <= 0f)
        {
            isJumping = false;
        }

        UpdateAnimation(direction);

        if (Input.GetKeyDown(interactKey) && controller.isGrounded && !isInteracting)
        {
            Interact();
        }
    }

    private void UpdateAnimation(Vector3 direction)
    {
        if (isDead)
        {
            PlayAnimation("Death");
            return;
        }

        if (isInteracting)
        {
            PlayAnimation("Interact");
            return;
        }

        if (isJumping || !controller.isGrounded)
        {
            PlayAnimation("Jump");
            return;
        }

        if (direction.magnitude > 0.1f)
        {
            PlayAnimation("Run");
        }
        else
        {
            PlayAnimation("Idle");
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
        PlayAnimation("Death");
    }

    private void SetupAnimation(string animationName, WrapMode wrapMode)
    {
        if (characterAnimation == null) return;

        AnimationState state = characterAnimation[animationName];

        if (state == null)
        {
            Debug.LogWarning("Char9Movement: Thiếu animation " + animationName);
            return;
        }

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
