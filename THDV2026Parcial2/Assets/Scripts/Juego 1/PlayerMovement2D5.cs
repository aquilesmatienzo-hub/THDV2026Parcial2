
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement2D5 : MonoBehaviour
{
    [Header("Estadisticas")]
    [SerializeField] private PlatformerStatsSO playerStats;

    [Header("Deteccion del suelo")]
    [SerializeField] private float groundCheckDistance = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private float horizontalInput;
    private float coyoteCounter;
    private float jumpBufferCounter;

    private bool isGrounded;
    private bool jumpHeld;
    private bool jumpActive;
    private bool jumpCutApplied;

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

            jumpHeld = Keyboard.current.spaceKey.isPressed;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                jumpBufferCounter = playerStats.jumpBufferTime;
        }
        else
        {
            jumpHeld = false;
        }

        jumpBufferCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (playerStats == null)
            return;

        // Acompañar la plataforma antes de comprobar el suelo.
        if (currentPlatform != null && !jumpActive)
        {
            Vector3 platformDelta =
                currentPlatform.position -
                previousPlatformPosition;

            rb.position += platformDelta;

            previousPlatformPosition =
                currentPlatform.position;
        }

        CheckGround();

        if (isGrounded)
            coyoteCounter = playerStats.coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;

        Vector3 velocity = rb.linearVelocity;

        // Movimiento horizontal del jugador.
        velocity.x = horizontalInput * playerStats.moveSpeed;

        // Evitar rebotes al bajar con una plataforma.
        if (isGrounded &&
            currentPlatform != null &&
            !jumpActive &&
            velocity.y < 0f)
        {
            velocity.y = 0f;
        }

        rb.linearVelocity = velocity;

        // Salto.
        if (jumpBufferCounter > 0f &&
            coyoteCounter > 0f)
        {
            Vector3 jumpVelocity = rb.linearVelocity;
            jumpVelocity.y = 0f;
            rb.linearVelocity = jumpVelocity;

            rb.AddForce(
                Vector3.up * playerStats.jumpForce,
                ForceMode.Impulse
            );

            jumpActive = true;
            jumpCutApplied = false;

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;

            currentPlatform = null;
        }

        // Salto variable.
        if (jumpActive &&
            !jumpHeld &&
            !jumpCutApplied &&
            rb.linearVelocity.y > 0f)
        {
            Vector3 cutVelocity = rb.linearVelocity;

            cutVelocity.y *= playerStats.jumpCutMultiplier;

            rb.linearVelocity = cutVelocity;

            jumpCutApplied = true;
        }

        // Detectar aterrizaje.
        if (jumpActive &&
            isGrounded &&
            rb.linearVelocity.y <= 0f &&
            coyoteCounter > 0f)
        {
            jumpActive = false;
        }
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
            bounds.extents.y -
            radius +
            groundCheckDistance;

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
                platform != null
                    ? platform.transform
                    : null;

            if (currentPlatform != detectedPlatform)
            {
                currentPlatform = detectedPlatform;

                if (currentPlatform != null)
                {
                    previousPlatformPosition =
                        currentPlatform.position;
                }
            }
        }
        else if (!jumpActive)
        {
            // Conservar la referencia durante una separacion breve.
            // Esto ayuda con plataformas descendentes.
            if (currentPlatform != null)
            {
                float separation = Mathf.Abs(
                    rb.position.y -
                    currentPlatform.position.y
                );

                if (separation > 3f)
                    currentPlatform = null;
            }
        }
        else
        {
            currentPlatform = null;
        }
    }
}
