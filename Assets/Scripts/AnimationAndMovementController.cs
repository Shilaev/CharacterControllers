using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable All
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

    [SuppressMessage("ReSharper", "ComplexConditionExpression")]
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
    }

    private void Update()
    {
        HandleRotation();
        HandleAnimation();

        if (_isRunPressed)
        {
            _characterController.Move(_currentRunMovement * Time.deltaTime);
        }
        else
        {
            _characterController.Move(_currentMovement * Time.deltaTime);
        }

        _characterController.Move(_currentMovement * Time.deltaTime);
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
}