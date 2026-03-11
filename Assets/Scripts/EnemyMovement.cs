using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    public float TrueSpeed = 8f;
    public float desiredSpeed;
    private NavMeshAgent navMeshAgent;
    private Transform thisPlayer;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        GameObject p = GameObject.FindWithTag("Player");
        thisPlayer = p.transform;
        navMeshAgent.SetDestination(thisPlayer.position);
        desiredSpeed = TrueSpeed;
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = desiredSpeed;
        }
               
    }


    void Update()
    {
        if(thisPlayer != null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            thisPlayer = p.transform;
            navMeshAgent.SetDestination(thisPlayer.position);
        }
                if (navMeshAgent != null)
        {
            navMeshAgent.speed = desiredSpeed;
        }
        
    }

       void OnTriggerEnter(Collider other) 
   {
        if (other.gameObject.CompareTag("SpeedPickUp")) 
        {
            other.gameObject.SetActive(false);
            StartCoroutine(WaitAndDeactivate());
        }
    }

    IEnumerator WaitAndDeactivate()
    {
        desiredSpeed = desiredSpeed * 2;
        yield return new WaitForSeconds(3f);
        desiredSpeed = TrueSpeed;

    }


    
}
