using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
     
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("DeathBox"))
        {
            if (playerController != null)
            {
                playerController.RegisterEnemyDestroyed();
            }

            Destroy(gameObject);
        }
    }

}
