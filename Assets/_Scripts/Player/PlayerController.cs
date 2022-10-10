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

public class PlayerController : MonoBehaviour
{
    #region Variables

    public static PlayerController Instance;

    [Header("References")]
    [SerializeField]
    private Rigidbody2D _playerRigidbody2D;

    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private SpriteRenderer _playerSpriteRenderer;
    [SerializeField] private GameObject _playerDisplay;
    private PlayerInputs _playerInputs;

    [Header("GroundCheck")]
    [SerializeField, Range(-5, 5)]
    private float _groundOffset;

    [SerializeField, Range(0.1f, 2)] private float _groundRadius;
    [SerializeField] private LayerMask _groundMask;
    private readonly Collider2D[] _collidersGround = new Collider2D[1];
    private bool _isGrounded;
    private float _timeSinceGrounded;

    [Header("Movement")]
    [SerializeField, Range(1, 50)]
    private float _moveSpeed;

    private Vector2 _currentInputs;
    private float _lastNonNullX;

    [Header("Jump")]
    [SerializeField, Tooltip("The timer between two jumps.")]
    [Range(1, 50)]
    private float _timeMinBetweenJump;

    [SerializeField, Range(1, 50)] private float _jumpForce;

    [SerializeField, Range(-40, -1),
     Tooltip("When the velocity reaches this value, the events that the fall triggers begin.")]
    private float _velocityFallMin;

    [SerializeField, Range(0.1f, 10)]
    [Tooltip("The gravity when the player press the jump input for a long time.")]
    private float _gravityUpJump;

    [SerializeField, Range(0.1f, 10)]
    [Tooltip("The gravity when the player press the jump input once.")]
    private float _gravity = 1;

    [SerializeField, Range(0.1f, 3)] private float _jumpInputTimer;
    [SerializeField, Range(0.01f, 0.99f)] private float _coyoteTime;
    private float _timerNoJump;
    private float _timerSinceJumpPressed;
    private bool _inputJump;
    private bool _isJumping;
    private bool _hasJump;

    [Header("Dash")]
    [SerializeField, Range(1, 30)]
    private float _dashForce;

    [SerializeField, Range(0.1f, 6),
     Tooltip("When the character is dashing, we divide the controller influence by this number.")]
    private float _controllerMalusDash;

    [SerializeField] private int _dashes = 3;
    [SerializeField] private SpriteRenderer[] _dashBalls;
    [SerializeField] private GameObject _arrowDirection;
    public float DashTimer;
    public int _remainingDashes;
    private bool _inputDash;
    private bool _isDashing;
    private Vector2 _dashInputValue;
    private Vector3 _joystickDirection;
    private float _joystickAngleFromRight;

    [Header("FX")] [SerializeField] private GameObject _fxJump;
    [SerializeField] private GameObject _fxLand;
    [SerializeField] private GameObject _fxWalk;
    [SerializeField] private GameObject _fxDash;
    private GameObject _stockFXDash;
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private Color[] _changeColorDarkZone;
    public bool _canResetDash = false;

    #endregion

    private void Awake()
    {
        Instance = this;
        _playerInputs = new PlayerInputs();
        _remainingDashes = _dashes;
    }

    private void Start()
    {
        PlayerManager.Instance.InsideDarkZone += InsideDarkZone;
        PlayerManager.Instance.OutsideDarkZone += OutsideDarkZone;
    }

    private void Update()
    {
        HandleInput();
        RefreshDash();
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
        PlayerManager.Instance.InsideDarkZone -= InsideDarkZone;
        PlayerManager.Instance.OutsideDarkZone -= OutsideDarkZone;
    }

    private void OnDrawGizmos()
    {
        Vector3 position = transform.position;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(position + Vector3.up * _groundOffset, _groundRadius);
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

    private void RefreshDash()
    {
        if (_remainingDashes <= -1)
            _remainingDashes = 0;
    }

    private void DecreaseDashRemaining(InputAction.CallbackContext obj)
    {
        if (_isGrounded) return;

        if (_remainingDashes > -1)
            _remainingDashes--;
        else
            return;

        AudioManager.Instance.PlaySound("PlayerDash");
        foreach (SpriteRenderer ball in _dashBalls)
        {
            DashBallsController controller = ball.GetComponent<DashBallsController>();
            if (!controller.IsCharged) continue;
            ball.enabled = false;
            controller.IsCharged = false;
            //controller.InitiateTimer(_dashTimer);
            controller.LaunchTimer();
            _canResetDash = true;
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

            bool isFlip = _playerDisplay.gameObject.GetComponent<SpriteRenderer>().flipX;
            if (!_hasJump) //Launch FX_Jump and sound
            {
                Transform transferPos = transform;
                GameObject go = Instantiate(_fxJump, transferPos);
                float flipOrNot = isFlip ? 1 : -1;
                go.transform.localScale = new Vector3(flipOrNot, 1, 1);
                AudioManager.Instance.PlaySound("PlayerJump");
                _hasJump = true;
            }

            if (_playerRigidbody2D.velocity.y < 0)
            {
                _playerRigidbody2D.gravityScale = _gravity;
                _isDashing = false;
                _isJumping = false;
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
            Instantiate(_fxLand, _transferPos);
            AudioManager.Instance.PlaySound("PlayerLand");
        }

        _isGrounded = currentGrounded;
    }

    private void HandleDash(InputAction.CallbackContext obj)
    {
        if (_isGrounded || _remainingDashes < 0) return;

        _trail.enabled = true;
        _isDashing = true;

        Destroy(_stockFXDash);
        GameObject go = Instantiate(_fxDash, _arrowDirection.transform);
        _stockFXDash = go;

        _dashInputValue = _playerInputs.Player.FireDirection.ReadValue<Vector2>();
        _playerRigidbody2D.velocity = -_dashInputValue * (_dashForce * 10);


        _joystickDirection = -_dashInputValue.normalized;
        _joystickAngleFromRight = Vector3.Angle(_dashInputValue, Vector3.right);

        switch (_joystickAngleFromRight)
        {
            case < 45f:
                _playerAnimator.Play("DashSide");
                break;
            case > 135f:
                _playerAnimator.Play("DashSide");
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
        if (!_isGrounded && !_isDashing && !_isJumping && _playerAnimator.velocity.y > 0)
        {
            _playerRigidbody2D.velocity = (_playerRigidbody2D.velocity * 0.99f +
                                           new Vector2(_currentInputs.x * _moveSpeed, _playerRigidbody2D.velocity.y) *
                                           0.01f);
        }
        else
        {
            if (!_isDashing)
                _playerRigidbody2D.velocity = new Vector2(_currentInputs.x * _moveSpeed, _playerRigidbody2D.velocity.y);
        }

        if (_currentInputs != Vector2.zero)
            _fxWalk.SetActive(true);
        else
            _fxWalk.SetActive(false);

        //Movement ArrowDir
        _dashInputValue = _playerInputs.Player.FireDirection.ReadValue<Vector2>();
        if (_dashInputValue != Vector2.zero)
        {
            _arrowDirection.SetActive(true);
            float angle = Mathf.Atan2(_dashInputValue.y, _dashInputValue.x) * Mathf.Rad2Deg;
            _arrowDirection.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else
        {
            if (_stockFXDash != null)
                Destroy(_stockFXDash);
            _arrowDirection.SetActive(false);
        }
    }


    //const float _alphaDarkZone = .6f;
    private void InsideDarkZone()
    {
        //Color _changeAlpha = _playerDisplay.GetComponent<SpriteRenderer>().color;
        //_changeAlpha.a = _alphaDarkZone;
        //_playerDisplay.GetComponent<SpriteRenderer>().color = _changeAlpha;
        _playerDisplay.GetComponent<SpriteRenderer>().color = _changeColorDarkZone[1];
        _canResetDash = false;
    }

    private void OutsideDarkZone()
    {
        _playerDisplay.GetComponent<SpriteRenderer>().color = _changeColorDarkZone[0];
        _canResetDash = true;
    }
}