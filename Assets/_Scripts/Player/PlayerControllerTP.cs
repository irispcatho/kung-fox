using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerTP : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private PhysicsMaterial2D _physicsFriction;
    [SerializeField] private PhysicsMaterial2D _physicsNoFriction;

    [Header("Mouvements")]
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _acceleration;

    [Header("GroundCheck")]
    [SerializeField] private float _groundOffset;
    [SerializeField] private float _groundRadius;
    [SerializeField] LayerMask _groundMask;
    private float _timeSinceGrounded;

    [Header("Jump")]
    [SerializeField] private float _timeMinBetweenJump;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _velocityFallMin;
    [SerializeField] private float _gravityUpJump;
    [SerializeField] private float _gravity = 1;
    [SerializeField] private float _jumpInputTimer;
    [SerializeField] private float _coyoteTime;
    [SerializeField] private float _slopeDetectOffset;
    [SerializeField] private Vector2 _offsetCollisionBox = Vector2.zero;
    [SerializeField] private Vector2 _offsetToReplace = Vector2.zero;

    private Vector2 _collisionBox;
    private RaycastHit2D[] _hitResults = new RaycastHit2D[1];
    private float _timerSinceJumpPressed;
    private Collider2D[] _collidersGround = new Collider2D[1];
    private float _timerNoJump;
    private bool _isGrounded;
    private Vector2 _inputs;
    private bool _inputJump;
    private bool _isOnSlope;
    private float[] directions = new float[2] { -1, 1 };

    private void HandleInputs()
    {
        _inputs.x = Input.GetAxisRaw("Horizontal");
        _inputs.y = Input.GetAxisRaw("Vertical");

        _inputJump = Input.GetKey(KeyCode.UpArrow);
        if (Input.GetKeyDown(KeyCode.UpArrow))
            _timerSinceJumpPressed = 0;
    }

    private void HandleMovements()
    {
        Vector2 velocity = _rb.velocity;
        Vector2 wantedVelocity = new Vector2(_inputs.x * _walkSpeed, velocity.y);
        _rb.velocity = Vector2.MoveTowards(velocity, wantedVelocity, _acceleration * Time.deltaTime);
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

    private void HandleJump()
    {
        _timerNoJump -= Time.deltaTime;
        _timerSinceJumpPressed += Time.deltaTime;

        if (_inputJump && (_rb.velocity.y <= 0 || _isOnSlope) && (_isGrounded || _timeSinceGrounded < _coyoteTime) && _timerNoJump <= 0 && _timerSinceJumpPressed < _jumpInputTimer)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
            _timerNoJump = _timeMinBetweenJump;
        }
        if (!_isGrounded)
        {
            if (_rb.velocity.y < 0)
                _rb.gravityScale = _gravity;
            else
                _rb.gravityScale = _inputJump ? _gravityUpJump : _gravity;
        }
        else
            _rb.gravityScale = _gravity;


        if (_rb.velocity.y < _velocityFallMin)
            _rb.velocity = new Vector2(_rb.velocity.x, _velocityFallMin);
    }

    private void HandleSlope()
    {
        Slope(Vector2.right);
        Slope(Vector2.left);

        _isOnSlope = (Slope(Vector2.right) || Slope(Vector2.left) && (!Slope(Vector2.right) || !Slope(Vector2.left)));

        _collider.sharedMaterial = Mathf.Abs(_inputs.x) < 0.1f && (!Slope(Vector2.right) || !Slope(Vector2.left)) ? _physicsFriction : _physicsNoFriction;
    }

    private bool Slope(Vector2 side)
    {
        Vector3 origin = transform.position + Vector3.up * _groundOffset;
        bool slope = Physics2D.RaycastNonAlloc(origin, side, _hitResults, _slopeDetectOffset, _groundMask) > 0;
        return slope;
    }

    private void HandleCorners()
    {
        for (int i = 0; i < directions.Length; i++)
        {
            float dir = directions[i];

            if (Mathf.Abs(_inputs.x) > 0.1f && Mathf.Abs(Mathf.Sign(dir) - Mathf.Sign(_inputs.x)) < 0.001f && !_isGrounded && !_isOnSlope)
            {
                Vector3 position = transform.position + new Vector3(_offsetCollisionBox.x + dir * _offsetToReplace.x, _offsetCollisionBox.y, 0);

                int result = Physics2D.BoxCastNonAlloc(position, _collisionBox, 0, Vector2.zero, _hitResults, 0, _groundMask);

                if (result > 0)
                {
                    position = transform.position + new Vector3(_offsetCollisionBox.x + dir * _offsetToReplace.x, _offsetCollisionBox.y + _offsetToReplace.y, 0);

                    result = Physics2D.BoxCastNonAlloc(position, _collisionBox, 0, Vector2.zero, _hitResults, 0, _groundMask);

                    if (result == 0)
                    {
                        Debug.Log("replace");
                        transform.position += new Vector3(dir + _offsetToReplace.x, _offsetToReplace.y);

                        if (_rb.velocity.y < 0)
                            _rb.velocity = new Vector2(_rb.velocity.x, 0);
                    }
                }
            }
        }
    }

    private void Update()
    {
        HandleInputs();
    }

    private void FixedUpdate()
    {
        HandleGrounded();
        HandleMovements();
        HandleJump();
        HandleSlope();
        HandleCorners();
    }
}
