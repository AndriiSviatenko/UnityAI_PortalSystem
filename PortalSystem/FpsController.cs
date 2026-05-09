using UnityEngine;
using UnityEngine.InputSystem;

namespace PortalSystem
{
    /// <summary>
    /// A Rigidbody-based First Person Controller designed to work with the Portal system.
    /// It uses the Unity Input System (Project-wide actions).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Teleportable))]
    public class FpsController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float groundDrag = 5f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Look Settings")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float verticalLookLimit = 85f;

        private Rigidbody _rb;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;

        private float _xRotation;
        private bool _isGrounded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            
            // Basic Rigidbody setup for FPS
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Initialize Input Actions from Project-Wide Actions
            if (InputSystem.actions != null)
            {
                _moveAction = InputSystem.actions.FindAction("Move");
                _lookAction = InputSystem.actions.FindAction("Look");
                _jumpAction = InputSystem.actions.FindAction("Jump");
            }

            // Lock Cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
            HandleJump();
        }

        private void FixedUpdate()
        {
            CheckGround();
            HandleMovement();
        }

        private void HandleLook()
        {
            if (_lookAction == null || cameraTransform == null) return;

            Vector2 lookValue = _lookAction.ReadValue<Vector2>() * mouseSensitivity;

            // Vertical rotation (Camera)
            _xRotation -= lookValue.y;
            _xRotation = Mathf.Clamp(_xRotation, -verticalLookLimit, verticalLookLimit);
            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

            // Horizontal rotation (Player Body)
            transform.Rotate(Vector3.up * lookValue.x);
        }

        private void HandleMovement()
        {
            if (_moveAction == null) return;

            Vector2 input = _moveAction.ReadValue<Vector2>();
            
            // Calculate movement direction relative to player orientation
            Vector3 moveDir = transform.forward * input.y + transform.right * input.x;

            if (_isGrounded)
            {
                _rb.linearDamping = groundDrag;
                // Apply force for movement
                _rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force);
            }
            else
            {
                _rb.linearDamping = 0;
                // Apply less force in air
                _rb.AddForce(moveDir.normalized * moveSpeed * 10f * 0.2f, ForceMode.Force);
            }

            // Limit speed
            Vector3 flatVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                _rb.linearVelocity = new Vector3(limitedVel.x, _rb.linearVelocity.y, limitedVel.z);
            }
        }

        private void HandleJump()
        {
            if (_jumpAction != null && _jumpAction.WasPressedThisFrame() && _isGrounded)
            {
                // Reset Y velocity for consistent jump height
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
                _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
        }

        private void CheckGround()
        {
            // Simple raycast for grounding
            // Assumes a player height of 2 units (capsule)
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
        }
    }
}
