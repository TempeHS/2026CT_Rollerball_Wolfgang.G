using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    
    public float speed = 20f;
    private Rigidbody ProjectileRb;

    void Start()
    {
        ProjectileRb = GetComponent<Rigidbody>();
        ProjectileRb.linearVelocity = transform.forward * speed;
    }


private void OnCollisionEnter(Collision collision)
    {
        // Losing condition: touching any enemy ends the run.
        if (collision.gameObject.CompareTag("Enemy"))
    {
        Destroy(collision.gameObject);
        Destroy(gameObject);
    } else
    {
        Destroy(gameObject);
    }


    }
}
