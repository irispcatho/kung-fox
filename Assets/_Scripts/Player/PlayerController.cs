using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;


namespace _Scripts
{
    public class PlayerController : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        [SerializeField] private Rigidbody2D _playerRigidbody;
        [SerializeField] private Collider2D _playerCollider;
        [SerializeField] private SpriteRenderer _playerSpriteRenderer;
        private PlayerInputs _playerInputs;
        private WaitForSeconds _wait;

        [Header("Movement")]
        [SerializeField] private float _speed;
        [SerializeField] private float _groundCheckHeight;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private LayerMask _wallMask;
        [SerializeField] private float _disableGroundCheckTime;
        private Vector2 _moveInput;
        private bool _groundCheckEnabled = true;
        private float _initialGravityScale;

        [Header("Jump")]
        [SerializeField] private float _jumpPower;
        [SerializeField] [Range(1f, 5f)] private float _jumpFallGravityMultiplier;
        [SerializeField] [Range(1f, 5f)] private float _jumpGravityMultiplier;
        private Vector2 _boxSize;
        private Vector2 _boxCenter;
        private int _jump;
        private bool _isJumping;

        [Header("Dash")]
        [SerializeField] private float _dashPower;
        private bool _doubleJumpEnable = true;
        private bool _dashing;
        private Vector2 _fireInput;

        [Header("Wall")]
        [SerializeField] private float _descendSpeed;
        private bool _hasThePos; 
        private Vector3 _pos;
        private Vector3 _wallPos;
        private bool _isWallJumping;
        private Direction _wallJumpDirection; 
        #endregion
            
        private void Awake()
        {
            _playerInputs = new PlayerInputs();

            _initialGravityScale = _playerRigidbody.gravityScale;
            _wait = new WaitForSeconds(_disableGroundCheckTime);
            _playerInputs.Player.Jump.performed += Jump;
            _playerInputs.Player.Dash.performed += Dash;
        }

        private void FixedUpdate()
        {
            Move();
            HandleGravity();
            OnTheWall();
        }

        private void OnEnable()
        {
            _playerInputs.Player.Enable();
        }

        private void OnDisable()
        {
            _playerInputs.Player.Disable();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isJumping ? Color.red : Color.green;
            Gizmos.DrawWireCube(_boxCenter, _boxSize);
        }

        private void Jump(InputAction.CallbackContext obj)
        {
            NormalJump();

            if (IsWalled())
                WallJump();
        }

        private void Dash(InputAction.CallbackContext obj) => ImpactJump();
        

        private void NormalJump()
        {
            if (!IsGrounded()) return;
            _playerRigidbody.AddForce(Vector2.up * (_jumpPower * 10), ForceMode2D.Impulse);
            print(_playerSpriteRenderer.bounds.size.y);
            _isJumping = true;
            _jump++;
            _playerSpriteRenderer.color = Color.red;
            StartCoroutine(EnableGroundCheckAfterJump());
        }

        private void ImpactJump()
        {
            if (!_doubleJumpEnable) return;

            _fireInput = _playerInputs.Player.FireDirection.ReadValue<Vector2>();
            _dashing = true;
            _playerRigidbody.AddForce(-_fireInput * (_dashPower * 10), ForceMode2D.Impulse);
            _doubleJumpEnable = false;
            StartCoroutine(EnableGroundCheckAfterJump());
        }

        private IEnumerator EnableGroundCheckAfterJump()
        {
            _groundCheckEnabled = false;
            yield return _wait;
            _groundCheckEnabled = true;
        }

        private bool IsGrounded()
        {
            Bounds bounds = _playerCollider.bounds;
            _boxCenter = new Vector2(bounds.center.x, bounds.center.y) +
                        Vector2.down * (bounds.extents.y + _groundCheckHeight / 2);
            _boxSize = new Vector2(bounds.size.x, _groundCheckHeight);
            Collider2D groundBox = Physics2D.OverlapBox(_boxCenter, _boxSize, 0f, _groundMask);

            return groundBox;
        }

        private bool IsWalled()
        {
            Bounds bounds = _playerCollider.bounds;
            _boxCenter = new Vector2(bounds.center.x, bounds.center.y) +
                        Vector2.down * (bounds.extents.y + _groundCheckHeight / 2f);
            _boxSize = new Vector2(bounds.size.x, _groundCheckHeight);
            Collider2D wallBox = Physics2D.OverlapBox(_boxCenter, _boxSize, 0f, _wallMask);
            
            return wallBox && !IsGrounded() && !_isWallJumping;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.layer == _wallMask) return;
            _wallPos = col.transform.position;
            _wallJumpDirection = _wallPos.x < transform.position.x ? Direction.Right : Direction.Left;
        }

        private void WallJump()
        {
            _isWallJumping = true;
            Vector2 force = _wallJumpDirection == Direction.Right
                ? new Vector2(_dashPower * 10, 0)
                : new Vector2(-_dashPower * 10, 0);
            print(force);
            _playerRigidbody.AddForce(force, ForceMode2D.Impulse);
        }

        private void HandleGravity()
        {
            if (_groundCheckEnabled && IsGrounded() && !IsWalled())
            {
                _hasThePos = false;
                _playerRigidbody.gravityScale = _initialGravityScale;
                ResetJumpingValue();
            }
            else switch (_isJumping)
            {
                case true when _playerRigidbody.velocity.y < 0:
                    _dashing = false;
                    _playerRigidbody.gravityScale = _initialGravityScale * _jumpFallGravityMultiplier;
                    break;
                case true when _playerRigidbody.velocity.y > 0:
                    _playerRigidbody.gravityScale = _initialGravityScale * _jumpGravityMultiplier;
                    break;
                default:
                {
                    if(!_groundCheckEnabled && !IsGrounded() || !IsWalled())
                    {
                        _playerRigidbody.gravityScale = _initialGravityScale;
                    }

                    break;
                }
            }
        }

        private void OnTheWall()
        {
            if (!IsWalled()) return;
            
            if (!_hasThePos)
            {
                _pos = transform.position;
                _hasThePos = true;
            }
            
            transform.position = _pos;
            _playerSpriteRenderer.color = Color.magenta;
            
            StartCoroutine(WallDescent());
            ResetJumpingValue();
        }

        private IEnumerator WallDescent()
        {
            yield return new WaitForEndOfFrame();
            _pos = new Vector3(_pos.x, _pos.y - _descendSpeed / 100, _pos.z);
        }

        private void ResetJumpingValue()
        {
            _isJumping = false;
            _dashing = false;
            _doubleJumpEnable = true;
            _isWallJumping = false;
            _jump = 0;
            if(!IsWalled())
                _playerSpriteRenderer.color = Color.green;
        }

        private void Move()
        {
            _moveInput = _playerInputs.Player.Move.ReadValue<Vector2>();
            
            if (!_dashing && !_isWallJumping && !_isJumping)
                _playerRigidbody.velocity = new Vector2(_moveInput.x * _speed, _playerRigidbody.velocity.y);
            else if(_isJumping)
                _playerRigidbody.velocity = new Vector2(_moveInput.x * _speed / 2, _playerRigidbody.velocity.y);
        }
    }
}

