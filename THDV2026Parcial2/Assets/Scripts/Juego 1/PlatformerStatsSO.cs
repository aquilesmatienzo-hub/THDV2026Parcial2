
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlatformerStats",
    menuName = "Juego 1/Platformer Stats"
)]
public class PlatformerStatsSO : ScriptableObject
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float jumpForce = 10f;

    [Header("Salto")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.15f;

    [Range(0.05f, 1f)]
    public float jumpCutMultiplier = 0.2f;
}
