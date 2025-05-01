using System;
using TMPro.EditorUtilities;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Fields
    // Components.
    private PlayerMovement _movement;
    private Animator _animator;
    private Rigidbody2D _rb;

    // State variables.
    public bool IsFacingRight { get; private set; } = true;
    private Vector2 _currentVelocity = new();
    // Maybe use enum instead of a bunch of bools?
    // And maybe remove them in general, considering they're unused.
    private bool _isIdling = true;
    private bool _isRunning = false;

    // Animation state variables.
    [Range(0.1f, 2f)] public float MaxRunAnimationSpeed = 1f;
    [Range(0.05f, 0.5f)] public float IdleAnimationThreshold = 0.15f;
    private const string IdleAnimation = "Idle";
    private const string RunAnimation = "Run";
    private const string JumpAnimation = "Jump";
    private const string AnimationSuffixRight = "Right";
    private const string AnimationSuffixLeft = "Left";
    private string _currentAnimation = "";

    // Input variables.
    private Vector2 _moveInput = new();
    #endregion

    private void Awake()
    {
        // Get component references.
        _movement = GetComponent<PlayerMovement>();
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        ChangeAnimation(IdleAnimation);
    }

    private void Update()
    {
        #region Handle input
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("Jump"))
            _movement.OnJumpInputDown();
        else if (Input.GetButtonUp("Jump"))
            _movement.OnJumpInputUp();
        #endregion

        #region Animation management
        _currentVelocity = _rb.linearVelocity;

        CheckDirectionToFace();

        #region Choose animation
        bool doIdle = Mathf.Abs(_currentVelocity.x) < IdleAnimationThreshold;
        if (doIdle)
        {
            _isIdling = true;
            _isRunning = false;
            ChangeAnimation(IdleAnimation);
        }
        else
        {
            _isIdling = false;
            if (_movement.IsJumping)
            {
                _isRunning = false;

                // Handle jump animation.
                SetJumpAnimationSheet();
                // Choose frame to display based on vertical velocity.
                SetJumpAnimationFrame();
            }
            else
            {
                _isRunning = true;
                // Adjust animation speed on current velocity
                float animationSpeed = Mathf.Abs(_currentVelocity.x / _movement.RunMaxSpeed);
                ChangeAnimation(RunAnimation, animationSpeed);
            }
        }
        #endregion
        #endregion
    }

    private void FixedUpdate()
    {
        _movement.Run(_moveInput.x);
    }

    #region Animations.
    /// <summary>
    /// Switches to the given animation. It handles the left/right variants, so just specify e.g. <c>AnimationIdle</c>.
    /// </summary>
    /// <param name="newAnimation">The animation to switch to (e.g. <c>AnimationIdle</c>).</param>
    /// <param name="newAnimationSpeed">The speed of the animation.</param>
    private void ChangeAnimation(string newAnimation, float newAnimationSpeed = 1f)
    {
        newAnimation += (IsFacingRight ? AnimationSuffixRight : AnimationSuffixLeft);

        if (newAnimation != _currentAnimation)
        {
            _animator.Play(newAnimation);
            _currentAnimation = newAnimation;
        }

        // Change max run animation speed to fine-tune animation-velocity matchup.
        if (_currentAnimation.StartsWith(RunAnimation))
            newAnimationSpeed *= MaxRunAnimationSpeed;

        _animator.speed = newAnimationSpeed;
    }

    /// <summary>
    /// Sets the left/right jump animation sprite sheet.
    /// </summary>
    private void SetJumpAnimationSheet()
    {
        ChangeAnimation(JumpAnimation);
        //_animator.StopPlayback();
        _animator.StartPlayback();
    }

    private void SetJumpAnimationFrame()
    {
        // Orignally: https://youtu.be/kmRUUK30E6k?t=341.



        // !!! DOESN'T WORK YET! SOME RANDOM CRAP THEY'VE COMMPLETELY CHANGED.
        // !!! ALSO THE ASEPRITE IMPORTER IS FUCKING UP THE NAME OF EVERY LAST FRAME OF ALL ANIMATIONS
        //     AND JUST NAMES IT 'Grass_Tileset' FOR SOME FUCKING REASON.
        // !!! FIX THAT SHIT TOMORROW(?), TOO FUCKING HOT TO DEAL WITH THIS FUCKING BULLSHIT............


        float animationTimestamp = Helpers.Map(_rb.linearVelocity.y,
                                               _movement.JumpForce, -_movement.JumpForce,
                                               0f, 1f);
        _animator.playbackTime = animationTimestamp;
    }

    /// <summary>
    /// Checks in which direction the player should face, and then faces them that way (if they aren't already).
    /// </summary>
    private void CheckDirectionToFace()
    {
        // Was using the player's velocity before but that sometimes cause the player to look the other direction
        // when stopping because there was a slight 'overcorrection' when trying to stop.
        bool supposedToMove = _moveInput.x != 0f;
        if (supposedToMove)
        {
            bool isMoveInputRight = _moveInput.x > 0;
            IsFacingRight = isMoveInputRight;
        }
    }
    #endregion
}
