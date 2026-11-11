
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerScriptAnimator : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 lastDirection = Vector2.down;
    private string currentAnimation = "";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        Vector2 direction = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) direction.y += 1f;
        if (Keyboard.current.sKey.isPressed) direction.y -= 1f;
        if (Keyboard.current.aKey.isPressed) direction.x -= 1f;
        if (Keyboard.current.dKey.isPressed) direction.x += 1f;

        bool moving = direction.sqrMagnitude > 0f;

        if (moving)
            lastDirection = direction;

        Vector2 facing = moving ? direction : lastDirection;

        string animationName;

        if (Mathf.Abs(facing.x) > Mathf.Abs(facing.y))
        {
            // Reutilizamos los sprites originales de Right
            animationName = moving ? "Walk_Right" : "Idle_Right";

            // Espejo horizontal solo cuando mira a la izquierda
            spriteRenderer.flipX = facing.x < 0f;
        }
        else if (facing.y > 0f)
        {
            animationName = moving ? "Walk_Up" : "Idle_Up";
            spriteRenderer.flipX = false;
        }
        else
        {
            animationName = moving ? "Walk_Down" : "Idle_Down";
            spriteRenderer.flipX = false;
        }

        if (currentAnimation != animationName)
        {
            animator.Play(animationName);
            currentAnimation = animationName;
        }
    }
}
