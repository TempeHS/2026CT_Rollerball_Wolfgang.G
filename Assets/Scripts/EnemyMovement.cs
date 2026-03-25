using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    // Movement
    public float TrueSpeed = 8f;
    public float desiredSpeed;

    // Shared AI tuning
    public float maxPredictionTime = 1.5f;
    public float guardingRange = 25f;
    public float cacheUpdateInterval = 2f;

    // Guarding tuning
    public float chaseRange = 3.5f;           
    public float orbitRadius = 2f;           
    public float orbitSpeed = 1.5f;           

    private float cacheUpdateTimer = 0f;

    private enum BehaviorType { Regular, Predictive, Guarding }
    private enum GuardState { Orbiting, Chasing }

    private BehaviorType currentBehaviour;
    private GuardState currentGuardState = GuardState.Orbiting;

    private NavMeshAgent navMeshAgent;
    private Transform thisPlayer;
    private Rigidbody playerRigidbody;
    private GameObject[] cachedPickups;

    private Transform guardedPickup;
    private float orbitPhase;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj == null || navMeshAgent == null)
        {
            enabled = false;
            return;
        }

        thisPlayer = playerObj.transform;
        playerRigidbody = playerObj.GetComponent<Rigidbody>();

        desiredSpeed = TrueSpeed;
        navMeshAgent.speed = desiredSpeed;

        // 40% Regular, 30% Predictive, 30% Guarding
        float randomValue = Random.value;
        if (randomValue < 0.4f)
            currentBehaviour = BehaviorType.Regular;
        else if (randomValue < 0.7f)
            currentBehaviour = BehaviorType.Predictive;
        else
            currentBehaviour = BehaviorType.Guarding;

        // Random offset so all guards do not orbit in sync
        orbitPhase = Random.value * Mathf.PI * 2f;

        UpdatePickupCache();
        AcquireGuardPickup();
    }

    void Update()
    {
        if (thisPlayer == null || navMeshAgent == null) return;

        cacheUpdateTimer += Time.deltaTime;
        if (cacheUpdateTimer >= cacheUpdateInterval)
        {
            UpdatePickupCache();
            AcquireGuardPickup();
            cacheUpdateTimer = 0f;
        }

        Vector3 target = GetTargetPosition();
        navMeshAgent.SetDestination(target);
        navMeshAgent.speed = desiredSpeed;
    }

    void UpdatePickupCache()
    {
        cachedPickups = GameObject.FindGameObjectsWithTag("PickUp");
    }

    void AcquireGuardPickup()
    {
        if (currentBehaviour != BehaviorType.Guarding) return;

        // Keep current target if still valid and in range
        if (guardedPickup != null && guardedPickup.gameObject.activeSelf)
        {
            float dist = Vector3.Distance(transform.position, guardedPickup.position);
            if (dist <= guardingRange) return;
        }

        guardedPickup = FindNearestPickupTransform();
    }

    Transform FindNearestPickupTransform()
    {
        if (cachedPickups == null || cachedPickups.Length == 0) return null;

        float closestDistance = float.MaxValue;
        Transform best = null;

        foreach (GameObject pickup in cachedPickups)
        {
            if (pickup == null || !pickup.activeSelf) continue;

            float distance = Vector3.Distance(transform.position, pickup.transform.position);
            if (distance < guardingRange && distance < closestDistance)
            {
                closestDistance = distance;
                best = pickup.transform;
            }
        }

        return best;
    }

    Vector3 GetTargetPosition()
    {
        switch (currentBehaviour)
        {
            case BehaviorType.Regular:
                return thisPlayer.position;

            case BehaviorType.Predictive:
                if (playerRigidbody != null)
                {
                    float speed = playerRigidbody.linearVelocity.magnitude;
                    if (speed > 1.0f)
                    {
                        float distanceToPlayer = Vector3.Distance(transform.position, thisPlayer.position);
                        float timeToReach = Mathf.Min(distanceToPlayer / navMeshAgent.speed, maxPredictionTime);
                        return thisPlayer.position + playerRigidbody.linearVelocity * timeToReach;
                    }
                }
                return thisPlayer.position;

            case BehaviorType.Guarding:
                return GetGuardTargetPosition();

            default:
                return thisPlayer.position;
        }
    }

    Vector3 GetGuardTargetPosition()
    {
        if (guardedPickup == null || !guardedPickup.gameObject.activeSelf)
        {
            guardedPickup = FindNearestPickupTransform();
            if (guardedPickup == null)
            {
                return thisPlayer.position;
            }
        }

        float playerToPickupDistance = Vector3.Distance(thisPlayer.position, guardedPickup.position);

        // Only two states: Orbit when far, Chase when close
        if (playerToPickupDistance <= chaseRange)
            currentGuardState = GuardState.Chasing;
        else
            currentGuardState = GuardState.Orbiting;

        switch (currentGuardState)
        {
            case GuardState.Orbiting:
                return GetOrbitPoint(guardedPickup.position);

            case GuardState.Chasing:
                return thisPlayer.position;

            default:
                return thisPlayer.position;
        }
    }

    Vector3 GetOrbitPoint(Vector3 pickupPosition)
    {
        float t = Time.time * orbitSpeed + orbitPhase;
        Vector3 offset = new Vector3(Mathf.Cos(t), 0f, Mathf.Sin(t)) * orbitRadius;
        return pickupPosition + offset;
    }
}