using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10;

    [Header("점프 설정")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float landingDuration = 0.3f;

    [Header("사운드 설정")]
    public AudioSource audioSource;
    public AudioClip gunFireSound;
    public AudioClip fistHitSound;
    public AudioClip swordHitSound;

    [Header("컴포넌트")]
    public Animator animator;
    private CharacterController controller;
    private Camera playerCamera;

    private float currentSpeed;
    private bool isLanding = false;
    private float landingTimer;

    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;



    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleCursorLock();

        CheckGrounded();
        HandleLanding();
        HandleMovement();
        HandleJump();
        UpdateAnimator();

    }


    void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            if (!wasGrounded)
            {
                isLanding = true;
                landingTimer = landingDuration;
            }
        }
    }

    void HandleLanding()
    {
        if (isLanding)
        {
            landingTimer -= Time.deltaTime;
            if (landingTimer <= 0)
                isLanding = false;
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null)
                animator.SetTrigger("jumpTrigger");
        }

        if (!isGrounded)
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMovement()
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 cameraRight = playerCamera.transform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = cameraForward * vertical + cameraRight * horizontal;

            currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        }
        else
        {
            currentSpeed = 0;
        }
    }

    void UpdateAnimator()
    {
        float animatorSpeed = Mathf.Clamp01(currentSpeed / runSpeed);
        animator.SetFloat("Speed", animatorSpeed);
        animator.SetBool("isGrounded", isGrounded);

        bool isFalling = !isGrounded && velocity.y < -0.1f;
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isLanding", isLanding);
    }

    public void SetCursorLock(bool lockCursor)
    {
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }

    public void ToggleCursorLock()
    {
        bool shouldLock = Cursor.lockState != CursorLockMode.Locked;
        SetCursorLock(shouldLock);
    }
}
