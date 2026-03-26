using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using TMPro;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    // Movement and turning variables.
    public float TrueSpeed = 0;
    public float speed; 
    public float turnSpeed = 12f;
    public float sprintMultiplier = 1.75f;
    public float jumpForce = 7f;
    private bool isSprinting;


    // Stamina system variables
    public float MaxStamina = 100f;
    public float staminaDrainRate = 10f;
    public float staminaRegenRate= 15f;
    public float staminaRegenDelay = 2f;
    public float currentStamina;
    private float regenDelayTimer;
    

    // UI elements.
    public TextMeshProUGUI countText;
    public TextMeshProUGUI EnemyCountText;
    public GameObject winTextObject;
    public GameObject loseTextObject;
    private GameObject restartButton;
    public Slider staminaSlider;

    // Enemy spawning setup.
    public GameObject DTEnemy;
    public float enemySpawnY = 0.5f;
    public Vector2 enemySpawnXRange = new Vector2(-20f, 20f);
    public Vector2 enemySpawnZRange = new Vector2(-20f, 20f);
    public float minEnemySpawnDistanceFromPlayer = 5f;


    public int count;
    public int EnemyCount;
    public int delayedCount;
    private Rigidbody rb;
    private float movementX;
    private float movementY;
    private Camera mainCamera;

    public bool IsMovementInputPressed => (movementX * movementX + movementY * movementY) > 0.0001f;

    bool IsSprintInputHeld()
    {
        // Prefer direct keyboard state when available to avoid sticky action callbacks.
        bool keyboardSprintHeld = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        if (Keyboard.current != null)
        {
            return keyboardSprintHeld;
        }

        // Fallback for non-keyboard devices that use the Sprint action callback.
        return isSprinting;
    }

    bool IsGrounded()
    {
        float rayLength = 0.6f;
        return Physics.Raycast(transform.position, Vector3.down, rayLength);
    }

    
    void Start()
    {
        // Spawn the initial wave of enemies.
        Instantiate(DTEnemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
        Instantiate(DTEnemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
        Instantiate(DTEnemy, GetRandomEnemySpawnPosition(), Quaternion.identity);
        Debug.Log("Enemies Spawned: 3");

        // Initialize UI.
        rb = GetComponent<Rigidbody>();
        Debug.Log("Rigibody Initialized");
        mainCamera = Camera.main;
        Debug.Log("Camera Initialized");
        SetCountText ();
        Debug.Log("Set Count Text");
        winTextObject.SetActive(false);
        Debug.Log("Set Win Text to false");
        loseTextObject.SetActive(false);
        Debug.Log("Set Lose Text to false");
        restartButton = GameObject.Find("RestartButton");
        var label = restartButton.GetComponentInChildren<TMP_Text>(true);
        Debug.Log("Found Restart Button");
        label.text = "Restart";
        Debug.Log("Renamed Restart Button");
        restartButton.SetActive(false);
        Debug.Log("Set Restart Button to false");
        SetGameplayCursorLock(true);
        Debug.Log("Locked Cursor");


        // Set player starting stats/variables
        count = 0;
        delayedCount = count;
        speed = TrueSpeed;
        Debug.Log("Set Count to 0");
        EnemyCount = 3;
        Debug.Log("Set Enemy Count to 3");
        SetEnemyCountText();
        Debug.Log("Set Enemy Count Text");
        speed = TrueSpeed;
        Debug.Log("Set Speed to True Speed");
        currentStamina = MaxStamina;
        Debug.Log("Current Stamina set to Max Stamina");
        staminaSlider.maxValue = MaxStamina;
        Debug.Log("Set Stamina Slider Max Value");
        staminaSlider.value = MaxStamina;
        Debug.Log("Set Stamina Slider Value to Max Stamina");
        regenDelayTimer = 1f;
        Debug.Log("Regeneration Delay Timer set to 1 second");

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

   void OnJump(InputValue value)
   {
       // Apply an upward force
       if (value.isPressed && IsGrounded())
       {
           rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
       }
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
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Enemy");
            foreach(GameObject go in gos)
            Destroy(go);

        Destroy(gameObject);
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

    public void Update()
    {
        // Ask the shared helper whether sprint is being held this frame.
        bool sprintInputHeld = IsSprintInputHeld();

        if (sprintInputHeld && currentStamina > 0f)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            regenDelayTimer = staminaRegenDelay;
        }
        else
        {
            // Wait briefly after sprinting ends, then refill up to the maximum.
            if (regenDelayTimer > 0f)
            {
                regenDelayTimer -= Time.deltaTime;
            }
            else if (currentStamina < MaxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }

        // Clamp value so it stays between 0 and max
        currentStamina = Mathf.Clamp(currentStamina, 0, MaxStamina);
        staminaSlider.value = currentStamina;
    }


    private void FixedUpdate()
    {
        // Build a movement direction relative to the camera's horizontal facing
        Transform cameraTransform = mainCamera != null ? mainCamera.transform : null;
        Vector3 camForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 camRight   = cameraTransform != null ? cameraTransform.right : Vector3.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 movement = camForward * movementY + camRight * movementX;

        bool canSprint = IsSprintInputHeld() && currentStamina > 0f;
        float currentSpeed = canSprint ? speed * sprintMultiplier : speed;
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 currentHorizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        Vector3 targetHorizontalVelocity = movement * currentSpeed;
        
        // Acceleration/Deceleration when turning and moving
        float acceleration = 0.20f;
        Vector3 newHorizontalVelocity = Vector3.Lerp(
            currentHorizontalVelocity,
            targetHorizontalVelocity,
            acceleration
        );

        rb.linearVelocity = new Vector3(newHorizontalVelocity.x, currentVelocity.y, newHorizontalVelocity.z);

        // Face the direction of movement
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
        if (other.gameObject.CompareTag("SpeedPickUp")) 
        {
            other.gameObject.SetActive(false);
            currentStamina = MaxStamina;
        }
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
        loseTextObject.gameObject.SetActive(true);
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
