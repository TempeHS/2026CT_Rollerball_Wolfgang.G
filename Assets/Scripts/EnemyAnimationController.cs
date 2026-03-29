using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{   
    private Animator animator;
    private int isEnemyMovingHash;
    private Vector3 lastPosition;


    void Start()
    {
        animator = GetComponent<Animator>();
        isEnemyMovingHash = Animator.StringToHash("IsEnemyMoving");
        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
        }

        if (animator != null)
        {
            animator.SetBool(isEnemyMovingHash, false);
        }

        lastPosition = transform.position;
    }
        

    void Update()
    {
        if (animator == null)
        {
            return;
        }
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, lastPosition);
        bool isEnemyMoving = distance > 0.000001f;

        Debug.Log("Enemy Position: " + currentPosition);
        animator.SetBool(isEnemyMovingHash, isEnemyMoving);

        lastPosition = currentPosition;
    }

    void OnDisable()
    {
        if (animator != null)
        {
            animator.SetBool(isEnemyMovingHash, false);
        }
    }
}
