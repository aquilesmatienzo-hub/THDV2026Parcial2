
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRespawn : MonoBehaviour
{
    [Header("Checkpoints del nivel")]
    [SerializeField] private Checkpoint[] checkpoints;

    [Header("Proteccion al reaparecer")]
    [SerializeField] private float invulnerabilityTime = 1f;

    private Vector3 respawnPosition;
    private Rigidbody rb;

    private int currentCheckpointIndex = -1;
    private float invulnerableUntil;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        respawnPosition = transform.position;
    }

    public bool SetCheckpoint(
        Checkpoint checkpoint,
        Vector3 position)
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == checkpoint)
            {
                if (i <= currentCheckpointIndex)
                    return false;

                currentCheckpointIndex = i;
                respawnPosition = position;

                Debug.Log("Checkpoint activado: " + (i + 1));
                return true;
            }
        }

        return false;
    }

    public void Die()
    {
        // Evitar perder varias vidas por una misma caida.
        if (Time.time < invulnerableUntil)
            return;

        if (LevelManager.Instance == null)
            return;

        invulnerableUntil =
            Time.time + invulnerabilityTime;

        bool canRespawn = LevelManager.Instance.LoseLife();

        if (!canRespawn)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = respawnPosition;
        Physics.SyncTransforms();

        Debug.Log("Jugador reaparecio");
    }
}
