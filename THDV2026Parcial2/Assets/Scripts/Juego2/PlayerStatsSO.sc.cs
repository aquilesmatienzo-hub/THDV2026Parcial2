
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerStats",
    menuName = "Game/Player Stats"
)]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Movimiento")]
    [Min(0f)]
    public float moveSpeed = 5f;
}
