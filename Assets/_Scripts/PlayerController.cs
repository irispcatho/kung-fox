using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D playerRigidbody;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private SpriteRenderer playerSpriteRenderer;
        private PlayerInputs playerInputs;
        private WaitForSeconds wait;

        [Header("Movement")]
        [SerializeField] private float speed;
        [SerializeField] private float groundCheckHeight;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private float disableGroundCheckTime;
        private Vector2 moveInput;
        private bool groundCheckEnabled = true;
        private float initialGravityScale;

        [Header("Jump")]
        [SerializeField] private float jumpPower;
        [SerializeField] [Range(1f, 5f)] private float jumpFallGravityMultiplier;
        private Vector2 boxSize;
        private Vector2 boxCenter;
        private int jump;
        private bool jumping;

        [Header("Dash")]
        [SerializeField] private float dashPower;
        private bool doubleJumpEnable = true;
        private bool dashing;
        private Vector2 fireInput;

        private void Awake()
        {
            playerInputs = new PlayerInputs();

            initialGravityScale = playerRigidbody.gravityScale;
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
            playerRigidbody.AddForce(Vector2.up * (jumpPower * 10), ForceMode2D.Impulse);
            jumping = true;
            jump++;
            playerSpriteRenderer.color = Color.red;
            StartCoroutine(EnableGroundCheckAfterJump());
        }

        private void ImpactJump()
        {
            if (!doubleJumpEnable) return;

            fireInput = playerInputs.Player.FireDirection.ReadValue<Vector2>();
            dashing = true;
            playerRigidbody.AddForce(-fireInput * (dashPower * 10), ForceMode2D.Impulse);
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
            Bounds bounds = playerCollider.bounds;
            boxCenter = new Vector2(bounds.center.x, bounds.center.y) +
                        Vector2.down * (bounds.extents.y + groundCheckHeight / 2);
            boxSize = new Vector2(bounds.size.x, groundCheckHeight);
            Collider2D groundBox = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundMask);

            return groundBox;
        }

        private bool IsWalled()
        {
            Bounds bounds = playerCollider.bounds;
            boxCenter = new Vector2(bounds.center.x, bounds.center.y) +
                        Vector2.down * (bounds.extents.y + groundCheckHeight / 2);
            boxSize = new Vector2(bounds.size.x, groundCheckHeight);
            Collider2D wallBox = Physics2D.OverlapBox(boxCenter, boxSize, 0f, wallMask);

            return wallBox;
        }

        private void HandleGravity()
        {
            if (IsWalled()) Debug.Log("Walled !");

            if (groundCheckEnabled && IsGrounded())
            {
                ResetJumpingValue();
            }
            else if (jumping && playerRigidbody.velocity.y < 0)
            {
                dashing = false;
                playerRigidbody.gravityScale = initialGravityScale * jumpFallGravityMultiplier;
            }
            else
            {
                playerRigidbody.gravityScale = initialGravityScale;
            }
        }

        private void ResetJumpingValue()
        {
            jumping = false;
            doubleJumpEnable = true;
            jump = 0;
            playerSpriteRenderer.color = Color.green;
        }

        private void Move()
        {
            moveInput = playerInputs.Player.Move.ReadValue<Vector2>();

            if (!dashing)
                playerRigidbody.velocity = new Vector2(moveInput.x * speed, playerRigidbody.velocity.y);
        }
    }
}