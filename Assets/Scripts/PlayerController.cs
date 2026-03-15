using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using TMPro;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    public float TrueSpeed = 0;
    public float speed; 
    public TextMeshProUGUI countText;
    public TextMeshProUGUI EnemyCountText;
    public GameObject winTextObject;

    // Enemy spawning setup.
    public GameObject DTEnemy;
    public float enemySpawnY = 0.5f;
    public Vector2 enemySpawnXRange = new Vector2(-20f, 20f);
    public Vector2 enemySpawnZRange = new Vector2(-20f, 20f);
    public float minEnemySpawnDistanceFromPlayer = 5f;

    private GameObject restartButton;
    public int count;
    public int EnemyCount;
    public int delayedCount;
    private Rigidbody rb;
    private float movementX;
    private float movementY;

    
    void Start()
    {
        // Spawn the initial wave of enemies.
        Instantiate(DTEnemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
        Instantiate(DTEnemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
        Instantiate(DTEnemy, GetRandomEnemySpawnPosition(), Quaternion.identity);

        // Initialize player state and UI.
        rb = GetComponent <Rigidbody>(); 
        count = 0;
        EnemyCount = 3;
        delayedCount = count; 
        speed = TrueSpeed;
        SetCountText ();
        SetEnemyCountText();
        winTextObject.SetActive(false);
        restartButton = GameObject.Find("RestartButton");
        var label = restartButton.GetComponentInChildren<TMP_Text>(true);
        label.text = "Restart";
        restartButton.SetActive(false);
    }

    void OnMove (InputValue movementValue)
   {
    // Read movement input each frame from the Input System action.
    Vector2 movementVector = movementValue.Get<Vector2>(); 
    movementX = movementVector.x; 
    movementY = movementVector.y;
   
   }

   void SetCountText ()
   {
       // Update pickup count and trigger the win state once target is reached.
        countText.text = "Count: " + count.ToString();
        if (count >= 81) 
        {
            winTextObject.SetActive(true);
            restartButton.SetActive(true);
            Destroy(GameObject.FindGameObjectWithTag("Enemy"));
        }
   }
    
    void SetEnemyCountText ()
   {
           // Keep enemy counter text in sync with tracked enemy count.
        EnemyCountText.text = "Enemies: " + EnemyCount.ToString();
   }

    public void RegisterEnemySpawned()
    {
        // Called by this controller whenever a new enemy is spawned.
        EnemyCount = EnemyCount + 1;
        SetEnemyCountText();
    }

    public void RegisterEnemyDestroyed()
    {
        // Clamp to zero so UI never shows negative enemy counts.
        EnemyCount = Mathf.Max(0, EnemyCount - 1);
        SetEnemyCountText();
    }

    private void FixedUpdate() 
   {
           // Apply force in physics step so movement stays consistent with Rigidbody simulation.
        Vector3 movement = new Vector3 (movementX, 0.0f, movementY);
        rb.AddForce(movement * speed); 
   }

    void OnTriggerEnter(Collider other) 
   {
        // Normal pickup: increase score and periodically add a new enemy.
        if (other.gameObject.CompareTag("PickUp")) 
        {
            other.gameObject.SetActive(false);
            count = count + 1;
            SetCountText ();
            if (count >= delayedCount + 3)
            {
                delayedCount = count;
                Instantiate(DTEnemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
                RegisterEnemySpawned();
            }
        }

        // Speed pickup: apply temporary speed boost.
        if (other.gameObject.CompareTag("SpeedPickUp")) 
        {
            other.gameObject.SetActive(false);
            StartCoroutine(WaitAndDeactivate());
        }
    }

    IEnumerator WaitAndDeactivate()
    {
        // Temporary buff that always returns to base speed.
        speed = speed * 2;
        yield return new WaitForSeconds(3f);
        speed = TrueSpeed;

    }

    Vector3 GetRandomEnemySpawnPosition()
    {
        // Try multiple random points on the NavMesh and prefer one far enough from the player.
        const int maxAttempts = 40;
        Vector3 playerPosition = transform.position;
        Vector3 bestCandidate = playerPosition;
        float bestDistance = -1f;

        float navMeshSampleRadius = 2.5f;
        int walkableMask = NavMesh.AllAreas;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(enemySpawnXRange.x, enemySpawnXRange.y);
            float randomZ = Random.Range(enemySpawnZRange.x, enemySpawnZRange.y);
            Vector3 candidate = new Vector3(randomX, enemySpawnY, randomZ);

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleRadius, walkableMask))
            {
                // Skip points that are off the walkable NavMesh.
                continue;
            }

            Vector3 navMeshPoint = hit.position;
            float distanceToPlayer = Vector3.Distance(navMeshPoint, playerPosition);

            if (distanceToPlayer >= minEnemySpawnDistanceFromPlayer)
            {
                // Valid spawn that respects minimum distance.
                return navMeshPoint;
            }

            if (distanceToPlayer > bestDistance)
            {
                bestDistance = distanceToPlayer;
                bestCandidate = navMeshPoint;
            }
        }

        if (NavMesh.SamplePosition(playerPosition, out NavMeshHit fallbackHit, 5f, walkableMask))
        {
            // Fallback to nearest valid NavMesh point around the player.
            return fallbackHit.position;
        }

        // Last-resort fallback if no NavMesh point is available.
        return new Vector3(playerPosition.x, enemySpawnY, playerPosition.z);
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Losing condition: touching any enemy ends the run.
        if (collision.gameObject.CompareTag("Enemy"))
    {
            
            Destroy(gameObject); 
            winTextObject.gameObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
            restartButton.SetActive(true);
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Enemy");
            foreach(GameObject go in gos)
            Destroy(go);
    }

        // Losing condition: falling into the death volume also ends the run.
        if (collision.gameObject.CompareTag("DeathBox"))
    {
            Destroy(gameObject); 
            winTextObject.gameObject.SetActive(true);
            restartButton.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Enemy");
            foreach(GameObject go in gos)
            Destroy(go);
    }
    }

}
