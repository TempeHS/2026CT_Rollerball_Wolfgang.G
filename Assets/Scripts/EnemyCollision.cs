using UnityEngine;

public class EnemyCollision : MonoBehaviour
{

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("DeathBox"))
        {
            Destroy(gameObject); 
        }
    }

}
