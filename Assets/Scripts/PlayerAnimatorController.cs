using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;
    private int isMovingHash;
    private int isAimingHash;
    private int isShootingHash;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
            return;
        }

        isMovingHash = Animator.StringToHash("IsPlayerMoving");
        isAimingHash = Animator.StringToHash("IsPlayerAiming");
        isShootingHash = Animator.StringToHash("IsPlayerShooting");

        animator.SetBool(isMovingHash, false);
        animator.SetBool(isAimingHash, false);
        animator.SetBool(isShootingHash, false);
    }

    void OnMove(InputValue movementValue)
    {
        if (animator == null) return;

        Vector2 movement = movementValue.Get<Vector2>();
        bool moving = movement.sqrMagnitude > 0.000001f;
        animator.SetBool(isMovingHash, moving);
    }

    void OnClick(InputValue value)
    {
        if (animator == null)
        {
            return;
        }
        if (value.isPressed)
        {
            bool shooting = value.isPressed;
            animator.SetBool(isShootingHash, shooting);
            StartCoroutine(ShootingCooldown());   
        }
    }

    private System.Collections.IEnumerator ShootingCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool(isShootingHash, false);
    }

    void OnRightClick(InputValue value)
    {
        if (animator == null) 
        {
            return;
        }
        bool aiming = value.isPressed;
        animator.SetBool(isAimingHash, aiming);
    }

    void OnDisable()
    {
        if (animator == null) return;
        animator.SetBool(isMovingHash, false);
        animator.SetBool(isAimingHash, false);
    }
}