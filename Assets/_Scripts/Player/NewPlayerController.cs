using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public enum Direction
{
    Left,
    Right
}

public class NewPlayerController : MonoBehaviour
{
    #region Variables

    public static NewPlayerController Instance;
    [Header("References")]
    [SerializeField] private Rigidbody2D _playerRigidbody2D;
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private SpriteRenderer _playerSpriteRenderer;
    [SerializeField] private GameObject _playerDisplay;
    private PlayerInputs _playerInputs;

    [Header("GroundCheck")]
    [SerializeField, Range(-5, 5)] private float _groundOffset;
    [SerializeField, Range(0.1f, 2)] private float _groundRadius;
    [SerializeField] private LayerMask _groundMask;
    private readonly Collider2D[] _collidersGround = new Collider2D[1];
    private bool _isGrounded;
    private float _timeSinceGrounded;

    [Header("Movement")]
    [SerializeField, Range(1, 50)] private float _moveSpeed;
    [SerializeField] private Vector2 _offsetCollisionBox;
    [SerializeField] private Vector2 _collisionBox;
    private Vector2 _currentInputs;
    private float _lastNonNullX;
    private int _boxDetecter;
    private RaycastHit2D[] _hitResults = new RaycastHit2D[1];

    [Header("Jump")]
    [SerializeField, Tooltip("The timer between two jumps.")][Range(1, 50)] private float _timeMinBetweenJump;
    [SerializeField, Range(1, 50)] private float _jumpForce;
    [SerializeField, Range(-40, -1), Tooltip("When the velocity reaches this value, the events that the fall triggers begin.")] private float _velocityFallMin;
    [SerializeField, Range(0.1f, 10)][Tooltip("The gravity when the player press the jump input for a long time.")] private float _gravityUpJump;
    [SerializeField, Range(0.1f, 10)][Tooltip("The gravity when the player press the jump input once.")] private float _gravity = 1;
    [SerializeField, Range(0.1f, 3)] private float _jumpInputTimer;
    [SerializeField, Range(0.01f, 0.99f)] private float _coyoteTime;
    private float _timerNoJump;
    private float _timerSinceJumpPressed;
    private bool _inputJump;
    private bool _isJumping;
    private bool _hasJump;

    [Header("Dash")]
    [SerializeField, Range(1, 30)] private float _dashForce;
    [SerializeField, Range(0.1f, 6), Tooltip("When the character is dashing, we divide the controller influence by this number.")] private float _controllerMalusDash;
    [SerializeField] private int _dashes = 3;
    [SerializeField] private SpriteRenderer[] _dashBalls;
    [SerializeField] private float _dashTimer;
    public int _remainingDashes;
    private bool _inputDash;
    private bool _isDashing;
    private Vector2 _dashInputValue;
    private Vector3 _joystickDirection;
    private float _joystickAngleFromRight;

    [Header("FX")]
    [SerializeField] private GameObject fx_Jump;
    [SerializeField] private GameObject fx_Land;
    [SerializeField] private GameObject fx_Walk;
    [SerializeField] private TrailRenderer _trail;


    #endregion

    private void Awake()
    {
        Instance = this;
        _playerInputs = new PlayerInputs();
        _remainingDashes = _dashes;
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        HandleGrounded();
        HandleMovement();
        HandleJump();
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
        Vector3 position = transform.position;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(position + Vector3.up * _groundOffset, _groundRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position + new Vector3(_offsetCollisionBox.x * _lastNonNullX, 0), _collisionBox);
    }

    private void HandleInput()
    {
        _currentInputs = _playerInputs.Player.Move.ReadValue<Vector2>();
        _inputJump = _playerInputs.Player.Jump.IsPressed();
        _inputDash = _playerInputs.Player.Dash.IsPressed();

        _playerInputs.Player.Dash.performed += DecreaseDashRemaining;
        _playerInputs.Player.Dash.performed += HandleDash;

        if (_inputJump == _playerInputs.Player.Jump.WasPressedThisFrame())
            _timerSinceJumpPressed = 0;

        if (_currentInputs.x != 0f) _lastNonNullX = _currentInputs.x;
        HandleAnimationParameters();
    }

    private void DecreaseDashRemaining(InputAction.CallbackContext obj)
    {
        if (_remainingDashes > 0)
            _remainingDashes--;

        foreach (SpriteRenderer ball in _dashBalls)
        {
            DashBallsController controller = ball.GetComponent<DashBallsController>();
            if (!controller.IsCharged) continue;
            ball.enabled = false;
            controller.IsCharged = false;
            controller.InitiateTimer(_dashTimer);
            return;
        }
    }

    private void HandleAnimationParameters()
    {
        // walk
        _playerAnimator.SetInteger("InputX", (int)_currentInputs.x);
        _playerSpriteRenderer.flipX = _lastNonNullX <= -1;

        // jump
        _playerAnimator.SetBool("Jump", _isJumping);
        _playerAnimator.SetFloat("VelocityY", _playerRigidbody2D.velocity.y);

        _playerAnimator.SetBool("IsGrounded", _isGrounded);

        // dash
        _playerAnimator.SetBool("IsDashing", _inputDash);
    }

    private void HandleJump()
    {
        _timerNoJump -= Time.deltaTime;
        _timerSinceJumpPressed += Time.deltaTime;

        if (_inputJump && _playerRigidbody2D.velocity.y <= 0
                       && (_isGrounded || _timeSinceGrounded < _coyoteTime)
                       && _timerNoJump <= 0 && _timerSinceJumpPressed < _jumpInputTimer)
        {
            _playerRigidbody2D.velocity = new Vector2(_playerRigidbody2D.velocity.x, _jumpForce);
            _timerNoJump = _timeMinBetweenJump;
        }

        if (!_isGrounded)
        {
            _isJumping = true;

            if (!_hasJump) //Launch FX_Jump
            {
                var _transferPos = transform;
                GameObject go = Instantiate(fx_Jump, _transferPos);
                var _isFlip = _playerDisplay.gameObject.GetComponent<SpriteRenderer>().flipX;
                float _flipOrNot = _isFlip ? 1 : -1;
                go.transform.localScale = new Vector3(_flipOrNot, 1, 1);
                _hasJump = true;
            }

            if (_playerRigidbody2D.velocity.y < 0)
            {
                _playerRigidbody2D.gravityScale = _gravity;
            }
            else if (_playerRigidbody2D.velocity.y > 0)
            {
                _playerRigidbody2D.gravityScale = _inputJump ? _gravityUpJump : _gravity;
            }
        }
        else if (_isGrounded)
        {
            _playerRigidbody2D.gravityScale = _gravity;
        }

        if (_playerRigidbody2D.velocity.y < _velocityFallMin)
        {
            _playerRigidbody2D.velocity = new Vector2(_playerRigidbody2D.velocity.x, _velocityFallMin);
            _playerRigidbody2D.gravityScale = _gravity;
        }

        if (_isGrounded)
        {
            _trail.enabled = false;
            _isDashing = false;
            _isJumping = false;
            _hasJump = false;
        }
    }

    //private void HandleWallCollision()
    //{
    //    if (_isGrounded || !_isWalled) return;

    //    if (!_hasThePos)
    //    {
    //        _pos = transform.position;
    //        _hasThePos = true;
    //    }

    //    transform.position = new Vector3(_pos.x, transform.position.y, transform.position.z);
    //    StartCoroutine(WallDescent());
    //}

    //private IEnumerator WallDescent()
    //{
    //    yield return new WaitForEndOfFrame();
    //    _pos = new Vector3(_pos.x, _pos.y - _descendSpeed / 100, _pos.z);
    //}

    private void HandleGrounded()
    {
        _timeSinceGrounded += Time.deltaTime;

        Vector2 point = transform.position + Vector3.up * _groundOffset;
        bool currentGrounded = Physics2D.OverlapCircleNonAlloc(point, _groundRadius, _collidersGround, _groundMask) > 0;

        if (!currentGrounded && _isGrounded)
            _timeSinceGrounded = 0;

        if (currentGrounded && !_isGrounded)
        {
            var _transferPos = transform;
            Instantiate(fx_Land, _transferPos);
        }

        _isGrounded = currentGrounded;
    }

    private void HandleDash(InputAction.CallbackContext obj)
    {
        if (_isGrounded || _remainingDashes < 0) return;


        _trail.enabled = true;
        _isDashing = true;

        _dashInputValue = _playerInputs.Player.FireDirection.ReadValue<Vector2>();
        _playerRigidbody2D.velocity = -_dashInputValue * (_dashForce * 10);

        _joystickDirection = -_dashInputValue.normalized;
        _joystickAngleFromRight = Vector3.Angle(_joystickDirection, Vector3.right);

        switch (_joystickAngleFromRight)
        {
            case < 45f:
                _playerAnimator.Play("DashSide");
                break;
            case > 135f:
                _playerAnimator.Play("DashSide");
                // _playerSpriteRenderer.flipX = true;
                break;
            default:
                {
                    switch (_joystickDirection.y)
                    {
                        case > 0f:
                            _playerAnimator.Play("DashUp");
                            break;
                        case < 0f when !_isGrounded:
                            _playerAnimator.Play("DashDown");
                            break;
                    }
                    break;
                }
        }

    }

    //private void HandleWalled()
    //{
    //    Vector3 position = transform.position;
    //    Vector2 point = position + new Vector3(Mathf.Sign(_lastNonNullX), 0) * _wallOffset;
    //    bool currentWalled = Physics2D.OverlapCircleNonAlloc(point, _wallRadius, _collidersWall, _wallMask) > 0;
    //    _isWalled = currentWalled;

    //    if (_isWalled)
    //        _playerRigidbody2D.gravityScale = _wallGravity;

    //    if (_isWalled)
    //    {
    //        if (_collidersWall[0].transform != null)
    //        {
    //            _wallPos = _collidersWall[0].transform.position;
    //            _wallJumpDirection = _wallPos.x < transform.position.x
    //                ? Direction.Right
    //                : Direction.Left;
    //        }
    //    }
    //}

    private void HandleMovement()
    {
        Vector3 position = transform.position + new Vector3(_offsetCollisionBox.x * _lastNonNullX, 0);
        _boxDetecter = Physics2D.BoxCastNonAlloc(position, _collisionBox, 0, Vector2.zero, _hitResults, 0, _groundMask);

        //if (_boxDetecter != 0) return;

        if (!_isDashing && !_isDashing)
        {
            _playerRigidbody2D.velocity = new Vector2(_currentInputs.x * _moveSpeed, _playerRigidbody2D.velocity.y);
        }

        else if (_playerRigidbody2D.velocity.y < _velocityFallMin && !_isDashing)
        {
            _playerRigidbody2D.velocity = new Vector2(_currentInputs.x * _moveSpeed / _controllerMalusDash,
                _playerRigidbody2D.velocity.y);
        }


        if (_currentInputs != Vector2.zero)
            fx_Walk.SetActive(true);
        else
            fx_Walk.SetActive(false);
    }
}