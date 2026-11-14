
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement2D5 : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ayudas de salto")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Salto variable")]
    [Range(0.1f, 1f)]
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Deteccion del suelo")]
    [SerializeField] private float groundCheckDistance = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private float horizontalInput;
    private float coyoteCounter;
    private float jumpBufferCounter;

    private bool isGrounded;
    private bool jumpCutRequested;
    private bool jumpStarted;

    private Transform currentPlatform;
    private Vector3 previousPlatformPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }

    private void Update()
    {
        horizontalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
                horizontalInput = -1f;

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
                horizontalInput = 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpBufferCounter = jumpBufferTime;
                jumpCutRequested = false;
            }

            if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                jumpCutRequested = true;
            }
        }

        jumpBufferCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        CheckGround();

        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;

        // Acompañar el desplazamiento de la plataforma.
        Vector3 platformVelocity = Vector3.zero;

        if (isGrounded && currentPlatform != null)
        {
            Vector3 displacement =
                currentPlatform.position - previousPlatformPosition;

            platformVelocity =
                displacement / Time.fixedDeltaTime;

            previousPlatformPosition = currentPlatform.position;
        }

        Vector3 velocity = rb.linearVelocity;
        velocity.x = horizontalInput * moveSpeed + platformVelocity.x;

        // Acompañar tambien plataformas verticales.
        if (isGrounded && currentPlatform != null)
            velocity.y = platformVelocity.y;

        rb.linearVelocity = velocity;

        // Salto con Coyote Time y Jump Buffer.
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            Vector3 jumpVelocity = rb.linearVelocity;
            jumpVelocity.y = 0f;
            rb.linearVelocity = jumpVelocity;

            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );

            jumpStarted = true;
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;

            currentPlatform = null;
        }

        // Reducir el salto al soltar espacio.
        if (jumpCutRequested)
        {
            if (jumpStarted && rb.linearVelocity.y > 0f)
            {
                Vector3 cutVelocity = rb.linearVelocity;
                cutVelocity.y *= jumpCutMultiplier;
                rb.linearVelocity = cutVelocity;
            }

            jumpCutRequested = false;
        }

        if (isGrounded && jumpBufferCounter <= 0f)
            jumpStarted = false;
    }

    private void CheckGround()
    {
        Bounds bounds = capsule.bounds;

        Vector3 origin = bounds.center;

        float radius = capsule.radius *
            Mathf.Max(
                transform.lossyScale.x,
                transform.lossyScale.z
            ) * 0.9f;

        float distance =
            bounds.extents.y - radius + groundCheckDistance;

        bool detected = Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out RaycastHit hit,
            distance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        ) && hit.normal.y > 0.5f;

        isGrounded = detected;

        if (isGrounded)
        {
            MovingPlatform platform =
                hit.collider.GetComponentInParent<MovingPlatform>();

            Transform detectedPlatform =
                platform != null ? platform.transform : null;

            if (currentPlatform != detectedPlatform)
            {
                currentPlatform = detectedPlatform;

                if (currentPlatform != null)
                    previousPlatformPosition =
                        currentPlatform.position;
            }
        }
        else
        {
            currentPlatform = null;
        }
    }
}
