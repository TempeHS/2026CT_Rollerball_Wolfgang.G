//using UnityEngine;

//public class CameraController : MonoBehaviour
//{
    //public GameObject player;
    //private Vector3 offset;

    //void Start()
    //{
       // offset = transform.position - player.transform.position; 
    //}

    
   // void LateUpdate()
    //{
        //transform.position = player.transform.position + offset; 
        
   // }
//}

using UnityEngine;
using System.Collections;

public class CamRotation : MonoBehaviour 
{
    private float x;
    private float y;
    private Vector3 rotateValue;
    private Vector3 offset;
    private UnityEngine.AI.NavMeshAgent navMeshAgent;
    private Transform thisPlayer;
    public GameObject player;

    void Start()
    {
        offset = transform.position - player.transform.position; 
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
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

    void LateUpdate()
    {
        y = Input.GetAxis("Mouse X");
        x = Input.GetAxis("Mouse Y");
        Debug.Log(x + ":" + y);
        rotateValue = new Vector3(x, y * -1, 0);
        transform.eulerAngles = transform.eulerAngles - rotateValue;
        transform.position = player.transform.position + offset;

        
    }
}
