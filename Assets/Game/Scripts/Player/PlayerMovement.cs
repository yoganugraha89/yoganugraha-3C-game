using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _walkSpeed;
    [SerializeField]
    private InputManager _input;
    private Rigidbody _rigidbody;
    [SerializeField]
    private float _rotationSmoothTime = 0.1f;
    private float _rotationSmoothVelocity;
    [SerializeField]
    private float _sprintSpeed;
    [SerializeField]
    private float _crouchSpeed;
    [SerializeField]
    private float _walkSprintTransition;
    private float _speed;
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private Transform _groundDetector;
    [SerializeField]
    private float _detectorRadius;
    [SerializeField]
    private LayerMask _groundLayer;
    private bool _isGrounded;
    [SerializeField]
    private Vector3 _upperStepOffset;
    [SerializeField]
    private float _stepCheckerDistance;
    [SerializeField]
    private float _stepForce;
    private PlayerStance _playerStance;
    [SerializeField]
    private Transform _climbDetector;
    [SerializeField]
    private float _climbCheckDistance;
    [SerializeField]
    private LayerMask _climbableLayer;
    [SerializeField]
    private Vector3 _climbOffset;
    [SerializeField]
    private float _climbSpeed;
    [SerializeField]
    private Transform _cameraTransform;
    [SerializeField]
    private CameraManager _cameraManager;
    private Animator _animator;
    private CapsuleCollider _collider;
    [SerializeField]
    private float _glideSpeed;
    [SerializeField]
    private float _airDrag;
    [SerializeField]
    private Vector3 _glideRotationSpeed;
    [SerializeField]
    private float _minGlideRotationX;
    [SerializeField]
    private float _maxGlideRotationX;
    private bool _isPunching;
    private int _combo = 0;
    [SerializeField]
    private float _resetComboInterval;
    private Coroutine _resetCombo;
    [SerializeField]
    private Transform _hitDetector;
    [SerializeField]
    private float _hitDetectorRadius;
    [SerializeField]

    private LayerMask _hitLayer;
    [SerializeField]
    private PlayerAudioManager _playerAudioManager;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _speed = _walkSpeed;
        _playerStance = PlayerStance.Stand;
        _animator = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();

        HideAndLockCursor();
    }

    private void Start()
    {
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
        _input.OnJumpInput += Jump;
        _input.OnClimbInput += StartClimb;
        _input.OnCancelClimb += CancelClimb;
        _input.OnCrouchInput += Crouch;
        _input.OnGlideInput += StartGlide;
        _input.OnCancelGlide += CancelGlide;
        _input.OnPunchInput += Punch;
        _cameraManager.OnChangePerspective += ChangePerspective;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
        Glide();
    }

    private void ChangePerspective()
    {
        _animator.SetTrigger("changePerspective");
    }

    private void HideAndLockCursor()
    {
        Debug.Log("HideAndLockCursor");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void StartClimb()
    {
        // Debug.Log("StartClimb");
        bool isInFrontOfClimbingWall = Physics.Raycast(_climbDetector.position, transform.forward, out RaycastHit hit, _climbCheckDistance, _climbableLayer);
        bool isNotClimbing = _playerStance != PlayerStance.Climb;
        // Debug.Log(isInFrontOfClimbingWall);
        // Debug.Log(_isGrounded);
        // Debug.Log(isNotClimbing);
        if (isInFrontOfClimbingWall && _isGrounded && isNotClimbing)
        {
            _animator.SetBool("isClimbing", true);
            _collider.center = Vector3.up * 1.3f;

            // Debug.Log("StartClimb - if");
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            transform.position = hit.point - offset;
            _playerStance = PlayerStance.Climb;
            _rigidbody.useGravity = false;

            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(70);
        }
    }

    private void CancelClimb()
    {
        if (_playerStance == PlayerStance.Climb)
        {
            _animator.SetBool("isClimbing", false);
            _collider.center = Vector3.up * 0.9f;

            _playerStance = PlayerStance.Stand;
            _rigidbody.useGravity = true;
            transform.position -= transform.forward * 1f;

            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(40);
        }
    }

    private void CheckStep()
    {
        bool isHitLowerStep = Physics.Raycast(_groundDetector.position, transform.forward, _stepCheckerDistance);
        bool isHitUpperStep = Physics.Raycast(_groundDetector.position + _upperStepOffset, transform.forward, _stepCheckerDistance);

        if (isHitLowerStep && !isHitUpperStep)
        {
            _rigidbody.AddForce(0, _stepForce, 0);
        }
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);

        _animator.SetBool("isGrounded", _isGrounded);
        if (_isGrounded)
        {
            CancelGlide();
        }
    }
    private void Jump()
    {
        if (_isGrounded)
        {
            _animator.SetTrigger("jump");

            Vector3 jumpDirection = Vector3.up;
            _rigidbody.AddForce(jumpDirection * _jumpForce * Time.deltaTime);
        }
    }

    private void Move(Vector2 axisDirection)
    {
        /*
        Vector3 movementDirection = new Vector3(axisDirection.x, 0, axisDirection.y);
        // Debug.Log(movementDirection);
        _rigidbody.AddForce(movementDirection * _walkSpeed * Time.deltaTime);
        */
        /*
        if (axisDirection.magnitude > 0.1)
        {
            float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            Vector3 movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
            _rigidbody.AddForce(movementDirection * Time.deltaTime * _walkSpeed);
        }
        */
        Vector3 movementDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
        bool isPlayerCrouch = _playerStance == PlayerStance.Crouch;
        bool isPlayerGliding = _playerStance == PlayerStance.Glide;

        if ((isPlayerStanding || isPlayerCrouch) && !_isPunching)
        {
            /*
            if (axisDirection.magnitude >= 0.1)
            {
                // float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg;
                float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                _rigidbody.AddForce(movementDirection * Time.deltaTime * _walkSpeed);
            }
            */
            Vector3 velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _animator.SetFloat("velocity", velocity.magnitude * axisDirection.magnitude);
            _animator.SetFloat("velocityX", velocity.magnitude * axisDirection.x);
            _animator.SetFloat("velocityZ", velocity.magnitude * axisDirection.y);

            switch (_cameraManager.CameraState)
            {
                case CameraState.ThirdPerson:
                    if (axisDirection.magnitude >= 0.1)
                    {
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        _rigidbody.AddForce(movementDirection * Time.deltaTime * _speed);
                    }
                    break;
                case CameraState.FirstPerson:
                    transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
                    Vector3 verticalDirection = axisDirection.y * transform.forward;
                    Vector3 horizontalDirection = axisDirection.x * transform.right;
                    movementDirection = verticalDirection + horizontalDirection;
                    _rigidbody.AddForce(movementDirection * Time.deltaTime * _speed);
                    break;
            }
        }
        else if(isPlayerClimbing)
        {
            Vector3 horizontal = axisDirection.x * transform.right;
            Vector3 vertical = axisDirection.y * transform.up;
            movementDirection= horizontal + vertical;
            _rigidbody.AddForce(movementDirection * Time.deltaTime * _climbSpeed);

            Vector3 velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, 0);
            _animator.SetFloat("climbVelocityX", velocity.magnitude * axisDirection.x);
            _animator.SetFloat("climbVelocityY", velocity.magnitude * axisDirection.y);
        }
        else if (isPlayerGliding)
        {
            Vector3 rotationDegree = transform.rotation.eulerAngles;
            rotationDegree.x += _glideRotationSpeed.x * axisDirection.y * Time.deltaTime;
            rotationDegree.x = Mathf.Clamp(rotationDegree.x, _minGlideRotationX, _maxGlideRotationX);

            rotationDegree.z += _glideRotationSpeed.z * axisDirection.x * Time.deltaTime;

            rotationDegree.y += _glideRotationSpeed.y * axisDirection.x * Time.deltaTime;

            transform.rotation = Quaternion.Euler(rotationDegree);
        }
    }

    private void Sprint(bool isSprint)
    {
        if (isSprint)
        {
            if (_speed < _sprintSpeed)
            {
                _speed = _speed + _walkSprintTransition * Time.deltaTime;
            }
        }
        else
        {
            if (_speed > _walkSpeed)
            {
                _speed = _speed - _walkSprintTransition * Time.deltaTime;
            }
        }
    }

    private void Crouch()
    {
        if(_playerStance == PlayerStance.Stand)
        {
            _playerStance = PlayerStance.Crouch;
            _animator.SetBool("isCrouch", true);
            _speed = _crouchSpeed;

            _collider.height = 1.3f;
            _collider.center = Vector3.up * 0.66f;
        }
        else if (_playerStance == PlayerStance.Crouch)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("isCrouch", false);
            _speed = _walkSpeed;

            _collider.height = 1.8f;
            _collider.center = Vector3.up * 0.9f;
        }
    }

    private void StartGlide()
    {
        if (_playerStance != PlayerStance.Glide && !_isGrounded)
        {
            _animator.SetBool("isGliding", true);
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _playerStance = PlayerStance.Glide;
            _playerAudioManager.PlayGlideSfx();
        }
    }

    private void CancelGlide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            _animator.SetBool("isGliding", false);
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _playerStance = PlayerStance.Stand;
            _playerAudioManager.StopGlideSfx();
        }
    }

    private void Glide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            Vector3 playerRotation = transform.rotation.eulerAngles;
            float lift = playerRotation.x;
            Vector3 upForce = transform.up * (lift + _airDrag);
            Vector3 forwardForce = transform.forward * _glideSpeed;
            Vector3 totalForce = upForce + forwardForce;
            _rigidbody.AddForce(totalForce * Time.deltaTime);
        }
    }

    private void Punch()
    {
        if (!_isPunching && _playerStance == PlayerStance.Stand)
        {
            _isPunching = true;
            if (_combo < 3)
            {
                _combo = _combo + 1;
            }
            else
            {
                _combo = 1;
            }
            _animator.SetInteger("combo", _combo);
            _animator.SetTrigger("punch");
        }
    }

    private void EndPunch()
    {
        _isPunching = false;
        if (_resetCombo != null)
        {
            StopCoroutine(_resetCombo);
        }
        _resetCombo = StartCoroutine(ResetCombo());
    }

    private IEnumerator ResetCombo()
    {
        yield return new WaitForSeconds(_resetComboInterval);
        _combo = 0;
    }

    private void Hit()
    {
        StartCoroutine(HitInterval());
    }
    
    private IEnumerator HitInterval()
    {
        yield return new WaitForSeconds(.65f);

        Collider[] hitObjects = Physics.OverlapSphere(_hitDetector.position, _hitDetectorRadius, _hitLayer);
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].gameObject != null)
            {
                Destroy(hitObjects[i].gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
        _input.OnCrouchInput -= Crouch;
        _input.OnGlideInput -= StartGlide;
        _input.OnCancelGlide -= CancelGlide;
         _input.OnPunchInput -= Punch;
        _cameraManager.OnChangePerspective -= ChangePerspective;
    }
}
