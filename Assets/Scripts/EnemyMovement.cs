using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{

    private NavMeshAgent navMeshAgent;
    private Transform thisPlayer;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        GameObject p = GameObject.FindWithTag("Player");
        thisPlayer = p.transform;
        navMeshAgent.SetDestination(thisPlayer.position);
               
    }


    void Update()
    {
        if(thisPlayer != null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            thisPlayer = p.transform;
            navMeshAgent.SetDestination(thisPlayer.position);
        }
        
    }


    
}
