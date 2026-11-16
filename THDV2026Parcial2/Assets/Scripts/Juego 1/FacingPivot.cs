
using UnityEngine;
using UnityEngine.InputSystem;

public class FacingPivot : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private Transform facingPivot;

    [Header("Animaciones")]
    [SerializeField] private float movementThreshold = 0.1f;

    private string currentAnimation = "";

    private void Update()
    {
        if (animator == null || playerRb == null)
            return;

        // Girar el personaje completo.
        if (facingPivot != null && Keyboard.current != null)
        {
            bool right =
                Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed;

            bool left =
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed;

            if (right && !left)
                facingPivot.localRotation =
                    Quaternion.Euler(0f, 0f, 0f);

            else if (left && !right)
                facingPivot.localRotation =
                    Quaternion.Euler(0f, 180f, 0f);
        }

        // Seleccionar animacion.
        float horizontalSpeed =
            Mathf.Abs(playerRb.linearVelocity.x);

        float verticalSpeed =
            playerRb.linearVelocity.y;

        string nextAnimation;

        if (verticalSpeed > 0.1f)
            nextAnimation = "Jump";
        else if (verticalSpeed < -0.1f)
            nextAnimation = "Fall";
        else if (horizontalSpeed > movementThreshold)
            nextAnimation = "Run";
        else
            nextAnimation = "Idle";

        if (nextAnimation != currentAnimation)
        {
            animator.CrossFade(nextAnimation, 0.08f);
            currentAnimation = nextAnimation;
        }
    }
}
