using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private int isMovingHash;

    void Start()
    {
        animator = GetComponent<Animator>();
        isMovingHash = Animator.StringToHash("IsMoving");
        if (animator == null)
            Debug.LogError("Animator component not found!");

        if (animator != null)
            animator.SetBool(isMovingHash, false);
    }

    void OnMove(InputValue movementValue)
    {
        if (animator == null)
            return;

        Vector2 movement = movementValue.Get<Vector2>();
        bool isMoving = movement.sqrMagnitude > 0.0001f;
        animator.SetBool(isMovingHash, isMoving);
    }

    void OnDisable()
    {
        if (animator != null)
            animator.SetBool(isMovingHash, false);
    }
}
