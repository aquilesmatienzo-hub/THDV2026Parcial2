
using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerRespawn player =
            other.GetComponentInParent<PlayerRespawn>();

        if (player != null && LevelManager.Instance != null)
        {
            LevelManager.Instance.WinLevel();
        }
    }
}
