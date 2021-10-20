using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    private Animator _animator;
    private CharacterController _characterController;
    private Vector3 _currentMovement;
    private Vector2 _currentMovementInput;
    private Vector3 _currentRunMovement;
    private bool _isMovementPressed;
    private int _isRunningHash;
    private bool _isRunPressed;
    private int _isWalkingHash;
    private PlayerInput _playerInput;
    private float _rotationFactorPerFrame = 1.0f;
    private float _runMultiplier = 3f;
    private float _groundedGravity = -0.5f;
    private float _gravity = -9.8f;
    private bool _isJumpPressed = false;
    private float _maxJumpHeight = 1.0f;
    private float _maxJumpTime = 0.5f;
    private float _initialJumpVelocity;
    private bool _isJumping = false;

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");

        // Move
        _playerInput.CharacterControls.Move.started += OnMovementInput;
        _playerInput.CharacterControls.Move.performed += OnMovementInput;
        _playerInput.CharacterControls.Move.canceled += OnMovementInput;

        // Run
        _playerInput.CharacterControls.Run.started += OnRun;
        _playerInput.CharacterControls.Run.canceled += OnRun;

        // Jump
        _playerInput.CharacterControls.Jump.started += OnJump;
        _playerInput.CharacterControls.Jump.canceled += OnJump;
        SetupJumpVariables();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
        Debug.Log(_isJumpPressed);
    }

    private void Update()
    {
        HandleRotation();
        HandleAnimation();

        if (_isRunPressed)
            _characterController.Move(_currentRunMovement * Time.deltaTime);
        else
            _characterController.Move(_currentMovement * Time.deltaTime);
        HandleGravity();
        HandleJump();
    }

    private void SetupJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
    }
    
    private void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }

    private void HandleAnimation()
    {
        var isWalking = _animator.GetBool(_isWalkingHash);
        var isRunning = _animator.GetBool(_isRunningHash);

        if (_isMovementPressed && isWalking == false)
        {
            _animator.SetBool(_isWalkingHash, true);
        }
        else if (_isMovementPressed == false && isWalking)
        {
            _animator.SetBool(_isWalkingHash, false);
        }

        if ((_isMovementPressed && _isRunPressed) && isRunning == false)
        {
            _animator.SetBool(_isRunningHash, true);
        }
        else if ((_isMovementPressed && _isRunPressed) == false && isRunning)
        {
            _animator.SetBool(_isRunningHash, false);
        }
    }

    private void HandleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = _currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = _currentMovement.z;

        Quaternion currentRotation = transform.rotation;


        if (_isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame);
        }
    }

    private void HandleGravity()
    {
        if (_characterController.isGrounded)
        {
            _currentMovement.y = _groundedGravity;
            _currentRunMovement.y = _groundedGravity;
        }
        else if (_characterController.isGrounded == false)
        {
            _currentMovement.y += _gravity;
            _currentRunMovement.y += _gravity;
        }
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x;
        _currentMovement.z = _currentMovementInput.y;


        _currentRunMovement.x = _currentMovementInput.x * _runMultiplier;
        _currentRunMovement.z = _currentMovementInput.y * _runMultiplier;

        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void HandleJump()
    {
        if (_isJumping == false && _characterController.isGrounded && _isJumpPressed)
        {
            _isJumping = true;
            _currentMovement.y = _initialJumpVelocity;
            _currentRunMovement.y = _initialJumpVelocity;
        }
    }
}