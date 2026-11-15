
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private bool activated;

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        PlayerRespawn player =
            other.GetComponentInParent<PlayerRespawn>();

        if (player != null)
        {
            Vector3 position = spawnPoint != null
                ? spawnPoint.position
                : transform.position;

            if (player.SetCheckpoint(this, position))
            {
                activated = true;

                Debug.Log("Checkpoint alcanzado");
            }
        }
    }
}
