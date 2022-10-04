using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public enum Direction
{
    Left,
    Right
}

public class NewPlayerController : MonoBehaviour
{
    #region Variables

    [Header("References")] [SerializeField]
    private Rigidbody2D _playerRigidbody2D;

    [SerializeField] private Collider2D _playerCollider;
    [SerializeField] private SpriteRenderer _playerSpriteRenderer;
    private PlayerInputs _playerInputs;

    [Header("GroundCheck")] [SerializeField] [Range(-5, 5)]
    private float _groundOffset;

    [SerializeField] [Range(0.1f, 2)] private float _groundRadius;
    [SerializeField] private LayerMask _groundMask;
    private readonly Collider2D[] _collidersGround = new Collider2D[1];
    private bool _isGrounded;
    private float _timeSinceGrounded;

    [Header("Movement")] [SerializeField] [Range(1, 50)]
    private float _moveSpeed;

    private Vector2 _currentInputs;
    private float _lastNonNullX;

    [Header("Jump")] [SerializeField] [Tooltip("The timer between two jumps.")] [Range(1, 50)]
    private float _timeMinBetweenJump;

    [SerializeField] [Range(1, 50)] private float _jumpForce;

    [SerializeField]
    [Range(-40, -1)]
    [Tooltip("When the velocity reaches this value, the events that the fall triggers begin.")]
    private float _velocityFallMin;

    [SerializeField] [Range(0.1f, 10)] [Tooltip("The gravity when the player press the jump input for a long time.")]
    private float _gravityUpJump;

    [SerializeField] [Range(0.1f, 10)] [Tooltip("The gravity when the player press the jump input once.")]
    private float _gravity = 1;

    [SerializeField] [Range(0.1f, 3)] private float _jumpInputTimer;
    [SerializeField] [Range(0.01f, 0.99f)] private float _coyoteTime;
    private float _timerNoJump;
    private float _timerSinceJumpPressed;
    private bool _inputJump;

    [Header("Dash")] [SerializeField] [Range(1, 30)]
    private float _dashForce;

    [SerializeField]
    [Range(0.1f, 6)]
    [Tooltip("When the character is dashing, we divide the controller influence by this number.")]
    private float _controllerMalusDash;

    private bool _dashInput;
    private bool _isDashing;
    private bool _doubleJumpEnable = true;
    private Vector2 _dashInputValue;

    [Header("Wall")] [SerializeField] [Range(1, 50)]
    private float _wallOffset;
    
    [SerializeField] private Vector2 _wallSize;
    [SerializeField] private LayerMask _wallMask;
    [SerializeField] [Range(1, 10)] private float _descendSpeed;
    [SerializeField] [Range(1, 50)] private float _wallJumpForce;
    private readonly Collider2D[] _collidersWall = new Collider2D[1];
    private bool _isWalled;
    private bool _hasThePos;
    private Vector3 _pos;
    private Direction _wallJumpDirection;
    private Vector2 _wallPos;
    private bool _isWallJumping;

    #endregion
    
    private void Awake()
    {
        _playerInputs = new PlayerInputs();
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        HandleWalled();
        HandleGrounded();
        HandleMovement();
        HandleJump();
        HandleWallCollision();
        HandleDash();
    }

    private void OnEnable()
    {
        _playerInputs.Player.Enable();
    }

    private void OnDisable()
    {
        _playerInputs.Player.Disable();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer != _wallMask) return;
    }

    private void OnDrawGizmos()
    {
        Vector3 position = transform.position;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(position + Vector3.up * _groundOffset, _groundRadius);

        Gizmos.color = Color.yellow;
        // Gizmos.DrawWireCube(position + new Vector3(Mathf.Sign(_lastNonNullX), 0) * _wallOffset, _size);
        Gizmos.DrawWireCube(position + new Vector3(Mathf.Sign(_lastNonNullX), 0) * _wallOffset, _wallSize);
    }

    private void HandleInput()
    {
        _currentInputs = _playerInputs.Player.Move.ReadValue<Vector2>();
        _inputJump = _playerInputs.Player.Jump.IsPressed();
        _dashInput = _playerInputs.Player.Dash.IsPressed();

        if (_inputJump == _playerInputs.Player.Jump.WasPressedThisFrame())
            _timerSinceJumpPressed = 0;

        if (_currentInputs.x != 0f) _lastNonNullX = _currentInputs.x;
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

        if (_inputJump)
            _doubleJumpEnable = true;

        if (_isWalled && _inputJump)
        {
            _isWallJumping = true;
            Vector2 force = new(_wallJumpDirection == Direction.Right ? _wallJumpForce : -_wallJumpForce, _dashForce);
            _playerRigidbody2D.velocity = force;
        }

        if (!_isGrounded)
        {
            if (_playerRigidbody2D.velocity.y < 0)
                _playerRigidbody2D.gravityScale = _gravity;
            else
                _playerRigidbody2D.gravityScale = _inputJump ? _gravityUpJump : _gravity;
        }
        else
        {
            _playerRigidbody2D.gravityScale = _gravity;
        }

        if (_playerRigidbody2D.velocity.y < _velocityFallMin && !_isWalled && !_isWallJumping)
        {
            _playerRigidbody2D.velocity = new Vector2(_playerRigidbody2D.velocity.x, _velocityFallMin);
            _playerRigidbody2D.gravityScale = _gravity;
        }

        if (_isGrounded)
        {
            _hasThePos = false;
            _isWalled = false;
            _doubleJumpEnable = false;
            _isDashing = false;
            _isWallJumping = false;
        }
    }

    private void HandleWallCollision()
    {
        if (!_isWalled) return;

        if (!_hasThePos)
        {
            _pos = transform.position;
            _hasThePos = true;
        }

        transform.position = _pos;
        StartCoroutine(WallDescent());
    }

    private IEnumerator WallDescent()
    {
        yield return new WaitForEndOfFrame();
        _pos = new Vector3(_pos.x, _pos.y - _descendSpeed / 100, _pos.z);
    }

    private void HandleGrounded()
    {
        _timeSinceGrounded += Time.deltaTime;

        Vector2 point = transform.position + Vector3.up * _groundOffset;
        bool currentGrounded = Physics2D.OverlapCircleNonAlloc(point, _groundRadius, _collidersGround, _groundMask) > 0;

        if (!currentGrounded && _isGrounded)
            _timeSinceGrounded = 0;

        _isGrounded = currentGrounded;
    }

    private void HandleDash()
    {
        if (!_doubleJumpEnable || !_dashInput) return;
        _isDashing = true;
        _dashInputValue = _playerInputs.Player.FireDirection.ReadValue<Vector2>();
        _playerRigidbody2D.velocity = -_dashInputValue * (_dashForce * 10);
        _doubleJumpEnable = false;
    }

    private void HandleWalled()
    {
        Vector3 position = transform.position;
        Vector2 point = position + new Vector3(Mathf.Sign(_lastNonNullX), 0) * _wallOffset;
        bool currentWalled = Physics2D.OverlapBoxNonAlloc(point, _wallSize, 0, _collidersWall, _wallMask) > 0;
        _isWalled = currentWalled;

        if (_isWalled)
            if (_collidersWall[0].transform != null)
            {
                _wallPos = _collidersWall[0].transform.position;
                _wallJumpDirection = _wallPos.x < transform.position.x + _wallSize.x * 2 ? Direction.Right : Direction.Left;
            }
    }

    private void HandleMovement()
    {
        if (!_isDashing && !_isWalled && !_isWallJumping && _)
            _playerRigidbody2D.velocity = new Vector2(_currentInputs.x * _moveSpeed, _playerRigidbody2D.velocity.y);
        else if (_playerRigidbody2D.velocity.y < _velocityFallMin && !_isWallJumping)
            _playerRigidbody2D.velocity = new Vector2(_currentInputs.x * _moveSpeed / _controllerMalusDash,
                _playerRigidbody2D.velocity.y);
    }
}