using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    // Movement
    public float TrueSpeed = 8f;
    public float desiredSpeed;

    // Shared AI tuning
    public float maxPredictionTime = 1.5f;  

    private enum BehaviorType { Regular, Predictive }

    private BehaviorType currentBehaviour;

    private NavMeshAgent navMeshAgent;
    private Transform thisPlayer;
    private Rigidbody playerRigidbody;


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

        // 60% Regular, 40% Predictive
        float randomValue = Random.value;
        if (randomValue < 0.6f)
            currentBehaviour = BehaviorType.Regular;
        else
            currentBehaviour = BehaviorType.Predictive;

    }

    void Update()
    {
        if (thisPlayer == null || navMeshAgent == null) return;

        Vector3 target = GetTargetPosition();
        navMeshAgent.SetDestination(target);
        navMeshAgent.speed = desiredSpeed;
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

            default:
                return thisPlayer.position;
        }
    }

    void OnDestroy()
    {
        // Clamp to zero so UI never shows negative enemy counts.
        EnemyCount = Mathf.Max(0, PlayerController.EnemyCount - 1);
    }
}