using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{

    public Transform player;
    private NavMeshAgent navMeshAgent;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            GameObeject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
            }
        }
        GameObject playerObj = GameObject.FindWithTag("Player");
        
    }


    void Update()
    {
        if(player == null)
        {
            GameObeject p = GameObject.FindWithTag("Player");
            if (p != null)
            navMeshAgent.SetDestination(GameObject.FindWithTag("Player"));
        
        }
        
    }


    
}
