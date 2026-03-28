using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float mouseSensitivity = 3f;
    public float distance = 5f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    private float _yaw;
    private float _pitch;

    void Start()
    {
        // Initialise yaw/pitch from the camera's starting orientation.
        _yaw   = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void LateUpdate()
    {
        if (player != null)
        {
        _yaw   += Input.GetAxis("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        transform.position  = player.transform.position + rotation * new Vector3(0f, 0f, -distance);
        transform.rotation  = rotation;
        }
    }
}

// using UnityEngine;
// using System.Collections;

// public class CamRotation : MonoBehaviour 
// {
//     private float x;
//     private float y;
//     private Vector3 rotateValue;
//     private Vector3 offset;
//     private UnityEngine.AI.NavMeshAgent navMeshAgent;
//     private Transform thisPlayer;
//     public GameObject player;

//     void Start()
//     {
//         offset = transform.position - player.transform.position; 
//         navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
//         GameObject p = GameObject.FindWithTag("Player");
//         thisPlayer = p.transform;
//         navMeshAgent.SetDestination(thisPlayer.position);
//     }

//     void Update()
//     {
//         if(thisPlayer != null)
//         {
//             GameObject p = GameObject.FindWithTag("Player");
//             thisPlayer = p.transform;
//             navMeshAgent.SetDestination(thisPlayer.position);
//         }
        
//     }

//     void LateUpdate()
//     {
//         y = Input.GetAxis("Mouse X");
//         x = Input.GetAxis("Mouse Y");
//         Debug.Log(x + ":" + y);
//         rotateValue = new Vector3(x, y * -1, 0);
//         transform.eulerAngles = transform.eulerAngles - rotateValue;
//         transform.position = player.transform.position + offset;

        
//     }
// }
