using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerController : MonoBehaviour
{
    #region Variables

    [Header("References")]
    [SerializeField] private Rigidbody2D _playerRigidbody2D;
    [SerializeField] private Collider2D _playerCollider;
    [SerializeField] private SpriteRenderer _playerSpriteRenderer;
    private PlayerInputs _playerInputs;

    [Header("GroundCheck")]
    [SerializeField] private float _groundOffset;
    [SerializeField] private float _groundRadius;
    [SerializeField] private LayerMask _groundMask;
    private Collider2D[] _collidersGround = new Collider2D[1];
    private bool _isGrounded;
    private float _timeSinceGrounded;
    
    [Header("Movement")] 
    [SerializeField] private float _moveSpeed;
    private Vector2 _currentInputs;
    private float _lastNonNullX;
    
    [Header("Jump")]
    [SerializeField] private float _timeMinBetweenJump;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _velocityFallMin;
    [SerializeField] private float _gravityUpJump;
    [SerializeField] private float _gravity = 1;
    [SerializeField] private float _jumpInputTimer;
    [SerializeField] private float _coyoteTime;
    private float _timerNoJump;
    private float _timerSinceJumpPressed;
    private bool _inputJump;
    
    [Header("Wall")]
    [SerializeField] private float _wallOffset;
    [SerializeField] private float _wallRadius;
    [SerializeField] private LayerMask _wallMask;
    private Collider2D[] _collidersWall = new Collider2D[1];
    private bool _isWalled;
    private bool _hasThePos;
    private Vector3 _pos;

    #endregion
    private void Awake()
    {
        _playerInputs = new PlayerInputs();
    }

    private void HandleInput()
    {
        _currentInputs = _playerInputs.Player.Move.ReadValue<Vector2>();
        _inputJump = _playerInputs.Player.Jump.IsPressed();
        if(_inputJump == _playerInputs.Player.Jump.WasPressedThisFrame())
            _timerSinceJumpPressed = 0;

        if (_currentInputs.x != 0f)
        {
            _lastNonNullX = _currentInputs.x;
        }
    }
    
    private void HandleJump()
    {
        Debug.Log("Jump");
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
            if (_playerRigidbody2D.velocity.y < 0)
                _playerRigidbody2D.gravityScale = _playerRigidbody2D.velocity.y < 0 ? _gravity : _inputJump ? _gravityUpJump : _gravity;
        }
        else
        {
            _playerRigidbody2D.gravityScale = _gravity;
        }


        if (_playerRigidbody2D.velocity.y < _velocityFallMin)
        {
            _playerRigidbody2D.velocity = new Vector2(_playerRigidbody2D.velocity.x, _velocityFallMin);
        }
    }

    private void HandleWallCollision()
    {
        _playerRigidbody2D.gravityScale = _isWalled && !_isGrounded ? 0 : _gravity;
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

    private void HandleWalled()
    {
        Vector2 point = transform.position + new Vector3(Mathf.Sign(_lastNonNullX), 0) * _wallOffset;
        bool currentWalled = Physics2D.OverlapCircleNonAlloc(point, _wallRadius, _collidersWall, _wallMask) > 0;
        
        _isWalled = currentWalled;
    }

    private void OnDrawGizmos()
    {
        Vector3 position = transform.position;
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(position + Vector3.up * _groundOffset, _groundRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position + new Vector3(Mathf.Sign(_lastNonNullX), 0) * _wallOffset, _wallRadius);
    }

    private void HandleMovement()
    {
        _playerRigidbody2D.velocity = new Vector2(_currentInputs.x * _moveSpeed, _playerRigidbody2D.velocity.y);
    }
    
    private void Update()
    {
        HandleInput();
        HandleGrounded();
        HandleWalled();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        HandleWallCollision();
    }
    
    private void OnEnable()
    {
        _playerInputs.Player.Enable();
    }

    private void OnDisable()
    {
        _playerInputs.Player.Disable();
    }
}
