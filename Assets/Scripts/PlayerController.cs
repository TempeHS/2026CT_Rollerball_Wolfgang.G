using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    public float TrueSpeed = 0;
    public float speed; 
    public TextMeshProUGUI countText;
    public TextMeshProUGUI EnemyCountText;
    public GameObject winTextObject;
    public GameObject Enemy;
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
        Instantiate(Enemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
        Instantiate(Enemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
        Instantiate(Enemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
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
    Vector2 movementVector = movementValue.Get<Vector2>(); 
    movementX = movementVector.x; 
    movementY = movementVector.y;
   
   }

   void SetCountText ()
   {
        countText.text = "Count: " + count.ToString();
        if (count >= 23) 
        {
            winTextObject.SetActive(true);
            restartButton.SetActive(true);
            Destroy(GameObject.FindGameObjectWithTag("Enemy"));
        }
   }
    
    void SetEnemyCountText ()
   {
        EnemyCountText.text = "Enemies: " + EnemyCount.ToString();
   }

    public void RegisterEnemySpawned()
    {
        EnemyCount = EnemyCount + 1;
        SetEnemyCountText();
    }

    public void RegisterEnemyDestroyed()
    {
        EnemyCount = Mathf.Max(0, EnemyCount - 1);
        SetEnemyCountText();
    }

    private void FixedUpdate() 
   {
        Vector3 movement = new Vector3 (movementX, 0.0f, movementY);
        rb.AddForce(movement * speed); 
   }

    void OnTriggerEnter(Collider other) 
   {
        if (other.gameObject.CompareTag("PickUp")) 
        {
            other.gameObject.SetActive(false);
            count = count + 1;
            SetCountText ();
            if (count >= delayedCount + 3)
            {
                delayedCount = count;
                Instantiate(Enemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
                RegisterEnemySpawned();
            }
        }

        if (other.gameObject.CompareTag("SpeedPickUp")) 
        {
            other.gameObject.SetActive(false);
            StartCoroutine(WaitAndDeactivate());
        }
    }

    IEnumerator WaitAndDeactivate()
    {
        speed = speed * 2;
        yield return new WaitForSeconds(3f);
        speed = TrueSpeed;

    }

    Vector3 GetRandomEnemySpawnPosition()
    {
        const int maxAttempts = 40;
        Vector3 playerPosition = transform.position;
        Vector3 bestCandidate = new Vector3(playerPosition.x, enemySpawnY, playerPosition.z);
        float bestDistance = -1f;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(enemySpawnXRange.x, enemySpawnXRange.y);
            float randomZ = Random.Range(enemySpawnZRange.x, enemySpawnZRange.y);
            Vector3 candidate = new Vector3(randomX, enemySpawnY, randomZ);
            float distanceToPlayer = Vector3.Distance(candidate, playerPosition);

            if (distanceToPlayer >= minEnemySpawnDistanceFromPlayer)
            {
                return candidate;
            }

            if (distanceToPlayer > bestDistance)
            {
                bestDistance = distanceToPlayer;
                bestCandidate = candidate;
            }
        }

        return bestCandidate;
    }


    private void OnCollisionEnter(Collision collision)
    {
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
