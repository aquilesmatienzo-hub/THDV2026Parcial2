
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRespawn : MonoBehaviour
{
    [Header("Checkpoints del nivel")]
    [SerializeField] private Checkpoint[] checkpoints;

    private Vector3 respawnPosition;
    private Rigidbody rb;

    private int currentCheckpointIndex = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        respawnPosition = transform.position;
    }

    public void SetCheckpoint(Vector3 position)
    {
        // Identificar el checkpoint que se activo.
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (Vector3.Distance(
                checkpoints[i].transform.position,
                position
            ) < 2f)
            {
                // No retroceder a checkpoints anteriores.
                if (i <= currentCheckpointIndex)
                    return;

                currentCheckpointIndex = i;
                respawnPosition = position;

                Debug.Log(
                    "Checkpoint activado: " + (i + 1)
                );

                return;
            }
        }

        // Mantener compatibilidad con el sistema anterior.
        respawnPosition = position;
        Debug.Log("Checkpoint activado");
    }

    public void Die()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = respawnPosition;

        Physics.SyncTransforms();

        Debug.Log("Jugador reaparecio");
    }
}
