using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotateSpeed = 36f;
    public float speedVariance = 8f;

    float actualRotateSpeed;

    void Start()
    {
        actualRotateSpeed = rotateSpeed + Random.Range(-speedVariance, speedVariance);

        // Random starting angle so spawned pickups do not line up visually.
        transform.Rotate(0f, Random.Range(0f, 360f), 0f, Space.Self);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0f, actualRotateSpeed, 0f) * Time.deltaTime);
    }
}
