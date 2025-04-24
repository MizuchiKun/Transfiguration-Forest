using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Fields
    // Components.
    private PlayerMovement _movement;
    private Vector2 _moveInput = new Vector2();

    // State variables.
    private bool _jumpPressed = false;
    private bool _jumpReleased = false;
    private bool _jumpSuccessful = false;
    #endregion

    private void Awake()
    {
        // Get component references.
        _movement = GetComponent<PlayerMovement>();

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
        _jumpPressed = Input.GetButtonDown("Jump");
        _jumpReleased = Input.GetButtonUp("Jump");
    }

    private void FixedUpdate()
    {
        _movement.Run(_moveInput.x);
        if (_jumpPressed)
            _jumpSuccessful = _movement.Jump();
        else if (_jumpReleased)
            _movement.IsJumpCut = true;
    }
}
