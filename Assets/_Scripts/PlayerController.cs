using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private new Collider2D collider;
        [SerializeField] private SpriteRenderer sr;

        [SerializeField] private float speed;

        [SerializeField]
        private float jumpPower; //This variable will determine the initial velocity to apply when jumping.

        [SerializeField]
        private float dashPower; //This variable will determine the initial velocity to apply when jumping.

        [SerializeField] [Range(1f, 5f)]
        private float
            jumpFallGravityMultiplier; //This variable will determine the gravity scale applied to the Rigidbody when the player is falling after performing a jump.

        [SerializeField]
        private float
            groundCheckHeight; //This variable will determine the height of the box (OverlapBox method) that we’ll be using below the player to determine if the it’s on the ground or not.

        [SerializeField]
        private LayerMask
            groundMask; //This variable will allow us to check for colliders that are within the ground layer when using the OverlapBox method.

        [SerializeField]
        private LayerMask
            wallMask; //This variable will allow us to check for colliders that are within the wall layer when using the OverlapBox method.

        [SerializeField]
        private float
            disableGroundCheckTime; //This variable will determine how much time does the ground check gets disabled when jumping in order to avoid resetting the jumping bool value.

        private Vector2
            boxCenter; //This variable will indicate the central coordinate of the box that checks if the player is on the ground or not.

        private Vector2 boxSize; // This variable will indicate the size (width, height) of the same box.
        private bool dashing; // This variable will indicate if the player is currently dashing.
        private bool doubleJumpEnable = true; // This variable will indicate if the double jump is enabled or not.
        private Vector2 fireInput;
        private bool groundCheckEnabled = true; // This variable will indicate if the ground check is enabled or not.
        private float initialGravityScale; // This variable will store the initial gravity scale value of the Rigidbody.
        private int jump;
        private bool jumping; // This variable will indicate if the player is currently jumping.
        private Vector2 moveInput;
        private PlayerInputs playerInputs;

        private WaitForSeconds
            wait; // This variable will be used to wait within a coroutine before enabling the ground check again.

        private void Awake()
        {
            playerInputs = new PlayerInputs();

            initialGravityScale = rb.gravityScale;
            wait = new WaitForSeconds(disableGroundCheckTime);
            playerInputs.Player.Jump.performed += Jump;
        }

        private void FixedUpdate()
        {
            Move();
            HandleGravity();
        }

        private void OnEnable()
        {
            playerInputs.Player.Enable();
        }

        private void OnDisable()
        {
            playerInputs.Player.Disable();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = jumping ? Color.red : Color.green;
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }

        private void Jump(InputAction.CallbackContext obj)
        {
            if (jump >= 1)
                ImpactJump();
            else
                NormalJump();
        }

        private void NormalJump()
        {
            if (!IsGrounded()) return;
            rb.AddForce(Vector2.up * (jumpPower * 10), ForceMode2D.Impulse);
            jumping = true;
            jump++;
            sr.color = Color.red;
            StartCoroutine(EnableGroundCheckAfterJump());
        }

        private void ImpactJump()
        {
            if (!doubleJumpEnable) return;

            fireInput = playerInputs.Player.FireDirection.ReadValue<Vector2>();
            dashing = true;
            rb.AddForce(-fireInput * (dashPower * 10), ForceMode2D.Impulse);
            doubleJumpEnable = false;
            StartCoroutine(EnableGroundCheckAfterJump());
        }

        private IEnumerator EnableGroundCheckAfterJump()
        {
            groundCheckEnabled = false;
            yield return wait;
            groundCheckEnabled = true;
        }

        private bool IsGrounded()
        {
            Bounds bounds = collider.bounds;
            boxCenter = new Vector2(bounds.center.x, bounds.center.y) +
                        Vector2.down * (bounds.extents.y + groundCheckHeight / 2);
            boxSize = new Vector2(bounds.size.x, groundCheckHeight);
            Collider2D groundBox = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundMask);

            return groundBox;
        }

        private bool IsWalled()
        {
            Bounds bounds = collider.bounds;
            boxCenter = new Vector2(bounds.center.x, bounds.center.y) +
                        Vector2.down * (bounds.extents.y + groundCheckHeight / 2);
            boxSize = new Vector2(bounds.size.x, groundCheckHeight);
            Collider2D wallBox = Physics2D.OverlapBox(boxCenter, boxSize, 0f, wallMask);

            return wallBox;
        }

        private void HandleGravity()
        {
            if (IsWalled())
            {
                Debug.Log("Walled !");
                // ResetJumpingValue();
            }
            
            if (groundCheckEnabled && IsGrounded())
                ResetJumpingValue();
            else if (jumping && rb.velocity.y < 0)
            {
                dashing = false;
                rb.gravityScale = initialGravityScale * jumpFallGravityMultiplier;
            }
            else
                rb.gravityScale = initialGravityScale;
        }

        private void ResetJumpingValue()
        {
            jumping = false;
            doubleJumpEnable = true;
            jump = 0;
            sr.color = Color.green;
        }

        private void Move()
        {
            moveInput = playerInputs.Player.Move.ReadValue<Vector2>();

            if (!dashing)
                rb.velocity = new Vector2(moveInput.x * speed, rb.velocity.y);
        }
    }
}