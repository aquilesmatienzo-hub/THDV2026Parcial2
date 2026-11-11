
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementParcial2 : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private PlayerStatsSO stats;

    private Rigidbody rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            input.y += 1f;

        if (Keyboard.current.sKey.isPressed)
            input.y -= 1f;

        if (Keyboard.current.aKey.isPressed)
            input.x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            input.x += 1f;

        moveInput = input.normalized;
    }

    private void FixedUpdate()
    {
        if (stats == null)
            return;

        Vector3 direction = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );

        Vector3 targetPosition =
            rb.position +
            direction * stats.moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(targetPosition);
    }
}
