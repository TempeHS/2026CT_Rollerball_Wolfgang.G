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
    public float turnSpeed = 12f;
    public float sprintMultiplier = 1.8f;
    private bool isSprinting;
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
    private Camera mainCamera;

    public bool IsMovementInputPressed => (movementX * movementX + movementY * movementY) > 0.0001f;

    
    void Start()
    {
        // Spawn the initial wave of enemies.
        Instantiate(DTEnemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
        Instantiate(DTEnemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
        Instantiate(DTEnemy, GetRandomEnemySpawnPosition(), Quaternion.identity);

        // Initialize player state and UI.
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        // Keep physics from tipping the player while still allowing yaw turning.
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
        SetGameplayCursorLock(true);
    }

    void OnMove (InputValue movementValue)
   {
    // Read movement input each frame from the Input System action.
    Vector2 movementVector = movementValue.Get<Vector2>(); 
    movementX = movementVector.x; 
    movementY = movementVector.y;
   
   }

   void OnSprint(InputValue value)
   {
       // Action callback support (useful for non-keyboard devices).
       isSprinting = value.isPressed;
   }

   void SetCountText ()
   {
       // Update pickup count and trigger the win state once target is reached.
        countText.text = "Count: " + count.ToString();
        if (count >= 81) 
        {
            winTextObject.SetActive(true);
            restartButton.SetActive(true);
            SetGameplayCursorLock(false);
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
        // Build a movement direction relative to the camera's horizontal facing.
        Transform cameraTransform = mainCamera != null ? mainCamera.transform : null;
        Vector3 camForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 camRight   = cameraTransform != null ? cameraTransform.right : Vector3.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 movement = camForward * movementY + camRight * movementX;
        
        // Applies the movement to the player
        bool sprintHeldFromKeyboard = Keyboard.current != null &&
                          (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
        bool sprintHeld = sprintHeldFromKeyboard || (Keyboard.current == null && isSprinting);
        float currentSpeed = sprintHeld ? speed * sprintMultiplier : speed;
        Vector3 targetVelocity = movement * currentSpeed;
        Vector3 currentVelocity = rb.linearVelocity;
        
        // Acceleration/Deceleration when turning and moving.
        float acceleration = 0.20f;
        Vector3 newHorizontalVelocity = Vector3.Lerp(
            new Vector3(currentVelocity.x, 0f, currentVelocity.z),
            targetVelocity,
            acceleration
        );
        
        rb.linearVelocity = newHorizontalVelocity + new Vector3(0f, currentVelocity.y, 0f);

        // Face the direction of travel when there is meaningful movement input.
        if (movement.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
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
        // if (other.gameObject.CompareTag("SpeedPickUp")) 
        // {
        //     other.gameObject.SetActive(false);
        //     StartCoroutine(WaitAndDeactivate());
        // }
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
            LoseGame();
    }

        // Losing condition: falling into the death volume also ends the run.
        if (collision.gameObject.CompareTag("DeathBox"))
    {
            LoseGame();
    }
    }

    void LoseGame()
    {
        winTextObject.gameObject.SetActive(true);
        winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
        restartButton.SetActive(true);
        SetGameplayCursorLock(false);

        GameObject[] gos = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject go in gos)
            Destroy(go);

        Destroy(gameObject);
    }

    void SetGameplayCursorLock(bool shouldLock)
    {
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLock;
    }

}
