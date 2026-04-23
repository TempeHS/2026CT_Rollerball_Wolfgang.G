using UnityEngine;

public class EnemyDestroyed : MonoBehaviour
{
    void OnDestroy()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            pc.RegisterEnemyDestroyed();
        }
    }
}
