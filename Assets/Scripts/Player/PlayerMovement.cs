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
    #region Inspector parameters
    [Header("Gravity")]
    //[HideInInspector] public float gravityStrength;
    //[HideInInspector] public float gravityScale;
    //[Space(5)]
    [SerializeField] private float _fallGravityMultiplier;
    [SerializeField] private float _maxFallSpeed;
    [Space(5)]
    [SerializeField] private float _fastFallGravityMultiplier;
    [SerializeField] private float _maxFastFallSpeed;
    /** NOTE: asdfa **/

    [Space(20)]

    [Header("Run")]
    [SerializeField] private float _runMaxSpeed;
    [SerializeField] private float _runAcceleration;
    /* ??? */ private float _runAccelAmount;
    [SerializeField] private float _runDeceleration;
    /* ??? */ private float _runDecelAmount;
    [Space(5)]
    [SerializeField] [Range(0f, 1)] private float _accelInAirMultiplier;
    [SerializeField] [Range(0f, 1)] private float _decelInAirMultiplier;
    [Space(5)]
    [SerializeField] private bool _doConserveMomentum = true;

    [Space(20)]

    [Header("Jump")]
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _jumpTimeToApex;
    private float _jumpForce;
    [SerializeField] private float _jumpCutGravityMultiplier;
    [SerializeField] [Range(0f, 1)] private float _jumpHangGravityMultiplier;
    [SerializeField] private float _jumpHangSpeedThreshold;
    [Space(0.5f)]
    [SerializeField] private float _jumpHangAccelerationMultiplier;
    [SerializeField] private float _jumpHangMaxSpeedMultiplier;

    [Space(20)]

    [Header("Assists")]
    [SerializeField] [Range(0.01f, 0.5f)] private float _coyoteTime;
    [SerializeField] [Range(0.01f, 0.5f)] private float _jumpInputBufferTime;
    #endregion

    public bool IsJumpCut { get; set; } = false;

    // Components.
    private Rigidbody2D _rb;
    #endregion

    private void Awake()
    {
        // Get component references.
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //
    }

    private void Update()
    {
        // handle some stuff
    }

    private void FixedUpdate()
    {
        // handle physics stuff
    }

    /// <summary>
    /// Makes the player run based on the given raw input vector.
    /// </summary>
    /// <param name="moveInputX">The 'Horizontal' raw axis.</param>
    public void Run(float moveInputX)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Makes the player jump.
    /// </summary>
    /// <returns><c>true</c> if the player jumped (was able to), <c>false</c> otherwise.</returns>
    public bool Jump()
    {
        throw new NotImplementedException();
    }
}
