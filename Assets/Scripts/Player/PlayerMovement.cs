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
    /* ??? */
    private float _gravityStrength;
    /* ??? */
    private float _gravityScale;
    [Header("Gravity")]
    [Space(5)]
    [SerializeField] private float _fallGravityMultiplier;
    [SerializeField] private float _maxFallSpeed;

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
    private bool _isJumping = false;
    private bool _isJumpFalling = false;
    private bool _isJumpCut = false;
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
    private float _lastGroundedTime = 0f;
    [SerializeField] [Range(0.01f, 0.5f)] private float _jumpInputBufferTime;
    private float _lastJumpPressTime = 0f;
    #endregion

    #region Input variables

    #endregion

    #region Check variables
    #region Collision check variables
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
        _groundCheckPoint = transform.Find("GroundCheck");
    }

    private void Start()
    {
        //
    }

    private void Update()
    {
        // Timers.
        _lastGroundedTime -= Time.deltaTime;
        _lastJumpPressTime -= Time.deltaTime;

        // Check related stuff.
        if (IsStandingOnGround())
            _lastGroundedTime = _coyoteTime;
    }

    private void FixedUpdate()
    {
        // handle physics stuff, like the gravity in different states
    }

    /// <summary>
    /// Makes the player run based on the given raw input vector. <br/>
    /// <br/>
    /// Call it in a FixedUpdate() method, since it uses forces!
    /// </summary>
    /// <param name="moveInputX">The 'Horizontal' raw axis input.</param>
    public void Run(float moveInputX)
    {
        float targetSpeed = moveInputX * _runMaxSpeed;

        // Calculate accelRate.
        float accelRate;
        bool isGrounded = IsGrounded();
        bool doAccelerate = Mathf.Abs(targetSpeed) > StopSpeedThreshold;  // Maybe rename doAccelerate.
        if (isGrounded)
            accelRate = (doAccelerate) ? _runAccelAmount : _runDecelAmount;
        else
            accelRate = (doAccelerate)
                        ? _runAccelAmount * _accelInAirMultiplier
                        : _runDecelAmount * _decelInAirMultiplier;

        // Add bonus (horizontal) acceleration at jump apex.
        // *Allegedly* makes the jump feel a bit more bouncy, responsive and natural.
        bool isAirborneDueToJump = (_isJumping || _isJumpFalling);
        bool isJumpHanging = Mathf.Abs(_rb.linearVelocity.y) < _jumpHangSpeedThreshold;
        if (isAirborneDueToJump && isJumpHanging)
        {
            accelRate *= _jumpHangAccelerationMultiplier;
            targetSpeed *= _jumpHangMaxSpeedMultiplier;
        }

        // Conserve momentum (don't slow down player if faster than targetSpeed in target direction).
        bool isFasterThanTargetSpeed = Mathf.Abs(_rb.linearVelocity.x) > Mathf.Abs(targetSpeed);
        bool isMovingInTargetDirection = Mathf.Sign(_rb.linearVelocity.x) == Mathf.Sign(targetSpeed);
        if (_doConserveMomentum && !isGrounded && doAccelerate
            && isFasterThanTargetSpeed && isMovingInTargetDirection)
        {
            accelRate = 0;
        }

        // Calculate runForce and apply it.
        float targetSpeedDiff = targetSpeed - _rb.linearVelocity.x;
        float runForce = targetSpeedDiff * accelRate;
        _rb.AddForce(runForce * Vector2.right, ForceMode2D.Force);
    }

    /// <summary>
    /// Makes the player jump.
    /// </summary>
    /// <returns><c>true</c> if the player jumped (was able to), <c>false</c> otherwise.</returns>
    public bool Jump()
    {
        throw new NotImplementedException();
    }

    public void OnJumpInputDown()
    {
        _lastJumpPressTime = _jumpInputBufferTime;

        if (CanJump())
        {
            Jump();
            _isJumping = true;
        }
    }

    public void OnJumpInputUp()
    {
        if (_isJumping)
            _isJumpCut = true;
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
        return !_isJumping && IsGrounded();
    }

    /// <summary>
    /// Whether the player is in coyote time (including standing on ground).
    /// </summary>
    /// <returns>True when they're in coyote time, false otherwise.</returns>

    private bool JumpInputBuffered()
    {
        return _lastJumpPressTime > 0;
    }
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        // Draw gizmos for collision check boxes.
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
    }

    private void OnValidate()
    {
        // Re-calculate some variables when inspector parameters update.

        // No bloody clue what this Å´ magic formula does, or how it works. Certainly makes no physical sense, afaik.
        // [Calculate our run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed.]
        _runAccelAmount = (50 * _runAcceleration) / _runMaxSpeed;
        _runDecelAmount = (50 * _runDeceleration) / _runMaxSpeed;
    }
    #endregion
}
