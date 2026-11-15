
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerRespawn player =
            other.GetComponentInParent<PlayerRespawn>();

        if (player != null)
        {
            player.Die();
        }
    }
}
