using System;
using System.ComponentModel;
using UnityEngine;

/// <summary>
/// A script that handles the players basic movement, like running (humand and fox form) and jumping. <br/>
/// Based on the script by Dawnosaur
/// (<see href="https://youtu.be/KbtcEVCM7bw">YouTube</see>, <see href="https://github.com/DawnosaurDev/platformer-movement/blob/a36d45404df20f2b54b6421e56f80cc1c37de86f/Scripts/With%20Experimental%20Dash/PlayerMovementWithDash.cs">GitHub</see>).
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    #region Fields
    #region Gravity
    /* ??? */
    private float _gravityStrength;
    /* ??? */
    private float _gravityScale;
    [Header("Gravity")]
    [Space(5)]
    [SerializeField] private float _fallGravityMultiplier;
    [SerializeField] private float _maxFallSpeed;
    #endregion

    [Space(20)]

    #region Run fields
    [Header("Run")]
    [SerializeField] private float _runMaxSpeed;
    [SerializeField] private float _runAcceleration;
    [SerializeField] private float _runDeceleration;
    private float _runAccelAmount; /* ??? */
    private float _runDecelAmount; /* ??? */
    [Space(5)]
    [SerializeField][Range(0f, 1)] private float _accelInAirMultiplier;
    [SerializeField][Range(0f, 1)] private float _decelInAirMultiplier;
    [Space(5)]
    [SerializeField] private bool _doConserveMomentum = true;
    private const float StopSpeedThreshold = 0.01f;
    #endregion

    #region Jump fields
    [Header("Jump")]
    [Space(20)]
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _jumpTimeToApex;
    private float _jumpForce;
    [SerializeField] private float _jumpCutGravityMultiplier;
    [SerializeField][Range(0f, 1)] private float _jumpHangGravityMultiplier;
    [SerializeField] private float _jumpHangSpeedThreshold;
    [Space(0.5f)]
    [SerializeField] private float _jumpHangAccelerationMultiplier;
    [SerializeField] private float _jumpHangMaxSpeedMultiplier;
    #endregion

    [Space(20)]

    #region Assist fields
    [Header("Assists")]
    [SerializeField] [Range(0.01f, 0.5f)] private float _coyoteTime;
    [SerializeField] [Range(0.01f, 0.5f)] private float _jumpInputBufferTime;
    private float _lastGroundedTime = 0f;
    private float _lastJumpPressTime = 0f;
    #endregion

    #region State variables
    public bool IsFacingRight { get; private set; } = true;
    public bool IsJumping { get; private set; } = false;
    private bool _isJumpFalling = false;
    private bool _isJumpCut = false;
    #endregion

    #region Check variables
    #region Collision check variables
    // Does any of this even need to be serialisable or in the inspector? . . .
    [HideInInspector] [SerializeField] private Transform _groundCheckPoint;
    [Header("Checks")]
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.97f, 0.1f);
    [SerializeField] private LayerMask _groundCheckLayers;
    // Å´ Will only be needed once fox form will be implemented.
    //private Transform _ceilingCheckPoint;
    //private Vector2 _ceilingCheckSize;
    #endregion
    #endregion

    #region General components
    private Rigidbody2D _rb;
    #endregion
    #endregion

    private void Awake()
    {
        // Get component references.
        _rb = GetComponent<Rigidbody2D>();
        SetGroundCheckPoint();
    }

    private void Start()
    {
        SetGravityScale(_gravityScale);
        IsFacingRight = true;
    }

    private void Update()
    {
        // Timers.
        _lastGroundedTime -= Time.deltaTime;
        _lastJumpPressTime -= Time.deltaTime;

        if (IsStandingOnGround())
            _lastGroundedTime = _coyoteTime;

        #region Jump checks.
        bool isFalling = _rb.linearVelocity.y < 0;
        if (IsJumping && isFalling)
        {
            IsJumping = false;
            _isJumpFalling = true;
        }

        if (IsGrounded() && !IsJumping)
        {
            _isJumpCut = false;
            _isJumpFalling = false;
        }

        // Jump.
        if (CanJump() && JumpInputBuffered())
        {
            IsJumping = true;
            _isJumpCut = false;
            _isJumpFalling = false;
            Jump();
        }
        #endregion

        #region Gravity.
        // Use different gravity scales under different circumstances.
        // Inter alia to adjust the jump curve.
        bool isAirborneDueToJump = (IsJumping || _isJumpFalling);
        bool isJumpHanging = Mathf.Abs(_rb.linearVelocity.y) < _jumpHangSpeedThreshold;
        if (_isJumpCut)
        {
            SetGravityScale(_jumpCutGravityMultiplier * _gravityScale);
            _rb.linearVelocityY = Mathf.Max(_rb.linearVelocity.y, -_maxFallSpeed);
        }
        else if (isAirborneDueToJump && isJumpHanging)
        {
            SetGravityScale(_jumpHangGravityMultiplier * _gravityScale);
        }
        else if (isFalling)
        {
            SetGravityScale(_fallGravityMultiplier * _gravityScale);
            _rb.linearVelocityY = Mathf.Max(_rb.linearVelocity.y, -_maxFallSpeed);
        }
        else
        {
            // Default gravity if standing on a platform or moving upwards.
            SetGravityScale(_gravityScale);
        }
        #endregion
    }

    private void FixedUpdate()
    {
        //
    }

    /// <summary>
    /// Makes the player run based on the given raw input. <br/>
    /// <br/>
    /// Call it in a FixedUpdate() method, since it uses forces!
    /// </summary>
    /// <param name="moveInputX">The 'Horizontal' raw axis input.</param>
    public void Run(float moveInputX)
    {
        float targetSpeed = moveInputX * _runMaxSpeed;

        #region Calculate accelRate.
        float accelRate;
        bool isGrounded = IsGrounded();
        bool doAccelerate = Mathf.Abs(targetSpeed) > StopSpeedThreshold;  // Maybe rename doAccelerate.
        if (isGrounded)
            accelRate = (doAccelerate) ? _runAccelAmount : _runDecelAmount;
        else
            accelRate = (doAccelerate)
                        ? _runAccelAmount * _accelInAirMultiplier
                        : _runDecelAmount * _decelInAirMultiplier;
        #endregion

        #region Add bonus (horizontal) acceleration at jump apex.
        // *Allegedly* makes the jump feel a bit more bouncy, responsive and natural.
        bool isAirborneDueToJump = (IsJumping || _isJumpFalling);
        bool isJumpHanging = Mathf.Abs(_rb.linearVelocity.y) < _jumpHangSpeedThreshold;
        if (isAirborneDueToJump && isJumpHanging)
        {
            accelRate *= _jumpHangAccelerationMultiplier;
            targetSpeed *= _jumpHangMaxSpeedMultiplier;
        }
        #endregion

        #region Conserve momentum.
        // (Don't slow down player if faster than targetSpeed in target direction.).
        bool isFasterThanTargetSpeed = Mathf.Abs(_rb.linearVelocity.x) > Mathf.Abs(targetSpeed);
        bool isMovingInTargetDirection = Mathf.Sign(_rb.linearVelocity.x) == Mathf.Sign(targetSpeed);
        if (_doConserveMomentum && !isGrounded && doAccelerate
            && isFasterThanTargetSpeed && isMovingInTargetDirection)
        {
            accelRate = 0;
        }
        #endregion

        #region Calculate runForce and apply it.
        float targetSpeedDiff = targetSpeed - _rb.linearVelocity.x;
        float runForce = targetSpeedDiff * accelRate;
        _rb.AddForce(runForce * Vector2.right, ForceMode2D.Force);
        #endregion
    }

    /// <summary>
    /// Makes the player jump.
    /// </summary>
    public void Jump()
    {
        // Prevent multiple jumps from one button press.
        _lastJumpPressTime = 0;
        _lastGroundedTime = 0;

        #region Perform jump.
        // Set vertical velocity to 0 if falling so every jump will be the same height.
        float jumpForce = _jumpForce;
        bool isFalling = _rb.linearVelocity.y < 0;
        if (isFalling)
            _rb.linearVelocityY = 0 ;
            //jumpForce -= _rb.linearVelocity.y;

        _rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
        #endregion
    }

    #region Check methods
    /// <summary>
    /// Checks whether the player is grounded.
    /// </summary>
    /// <returns>True if the player is grounded, false otherwise.</returns>
    public bool IsGrounded()
    {
        // (Maybe rename IsGrounded() and IsStandingOnGround().).
        return _lastGroundedTime > 0;
    }

    /// <summary>
    /// Checks whether the player is standing on ground.
    /// </summary>
    /// <returns>True if they're standing on ground, false otherwise.</returns>
    private bool IsStandingOnGround()
    {
        // (Maybe rename IsGrounded() and IsStandingOnGround().).
        return Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundCheckLayers);
    }

    /// <summary>
    /// Checks whether the player can jump, using coyote time (and indirectly IsGrounded()).
    /// </summary>
    /// <returns>True if the player can jump, false otherwise.</returns>
    private bool CanJump()
    {
        return !IsJumping && IsGrounded();
    }

    /// <summary>
    /// Whether the player has a jump input buffered (which triggers when next grounded).
    /// </summary>
    /// <returns>True when they have a buffered input, false otherwise.</returns>

    private bool JumpInputBuffered()
    {
        return _lastJumpPressTime > 0;
    }
    #endregion

    #region Miscellaneous methods.
    public void OnJumpInputDown()
    {
        _lastJumpPressTime = _jumpInputBufferTime;
    }

    public void OnJumpInputUp()
    {
        bool isMovingUp = _rb.linearVelocity.y > 0;
        bool canJumpCut = IsJumping && isMovingUp;
        if (canJumpCut)
            _isJumpCut = true;
    }

    private void SetGravityScale(float gravityScale)
    {
        _rb.gravityScale = gravityScale;
    }

    private void SetGroundCheckPoint()
    {
        if (_groundCheckPoint == null)
            _groundCheckPoint = transform.Find("GroundCheck");
    }
    #endregion

    #region Editor methods.
    private void OnDrawGizmosSelected()
    {
        // Draw gizmos for collision check boxes.
        SetGroundCheckPoint();
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
    }

    private void OnValidate()
    {
        // The desiered gravity (strength).
        // ""Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2).""
        _gravityStrength = -(2 * _jumpHeight) / (_jumpTimeToApex * _jumpTimeToApex);
        // Calculate corresponding gravity scale (so gravityStrength relative to the project's gravity setting).
        _gravityScale = _gravityStrength / Physics2D.gravity.y;

        // No bloody clue what this Å´ magic formula does, or how it works. Certainly makes no physical sense, afaik.
        // ""Calculate our run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed.""
        _runAccelAmount = (50 * _runAcceleration) / _runMaxSpeed;
        _runDecelAmount = (50 * _runDeceleration) / _runMaxSpeed;

        // Once again, what is this formula? m/s^2 * s != N . . .
        // ""Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex).""
        _jumpForce = Mathf.Abs(_gravityStrength) * _jumpTimeToApex;

        // Clamp variable ranges.
        _runAcceleration = Mathf.Clamp(_runAcceleration, 0.01f, _runMaxSpeed);
        _runDeceleration = Mathf.Clamp(_runDeceleration, 0.01f, _runMaxSpeed);
    }
    #endregion
}
