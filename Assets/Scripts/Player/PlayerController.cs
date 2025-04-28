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

    // Input variables.
    private Vector2 _moveInput = new Vector2();
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
        //
    }

    private void Update()
    {
        // Handle input.
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("Jump"))
            _movement.OnJumpInputDown();
        if (Input.GetButtonUp("Jump"))
            _movement.OnJumpInputUp();

        // Checks.
        if (_rb.linearVelocity.x != 0)
            CheckDirectionToFace();
    }

    private void FixedUpdate()
    {
        _movement.Run(_moveInput.x);
    }

    /// <summary>
    /// Checks in which direction the player should face, and then faces them that way (if they aren't already).
    /// </summary>
    private void CheckDirectionToFace()
    {
        // I feel just re-assigning isMovingRight every frame would actually be better,
        // at least performance wise. But who cares about (prematurely micro-optimised) performance?
        bool isMovingRight = _rb.linearVelocity.x > 0;
        if (isMovingRight != IsFacingRight)
        {
            TurnAnimation();
            IsFacingRight = isMovingRight;
        }
    }

    #region Animations.
    /// <summary>
    /// Switches to the other direction variant (left/right) of the current animaiton.
    /// </summary>
    private void TurnAnimation()
    {
        // switch to the other version (left/right) of the current animation,
        // but (maybe) make sure to stay on the same frame of the animation, and to keep the progress to the next frame (milliseconds left or something)
    }
    #endregion
}
