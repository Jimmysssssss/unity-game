using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace CrystalQuest
{
    /// <summary>
    /// Handles WASD/arrow-key movement, jumping, and gravity for the player avatar.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeed = 6.5f;

        [SerializeField]
        private float turnSpeed = 480f;

        [SerializeField]
        private float gravity = -20f;

        [SerializeField]
        private float jumpHeight = 2.1f;

        private CharacterController controller;
        private Vector3 velocity;
        private bool jumpBuffered;
        private bool inputEnabled = true;

        public void DisableInput()
        {
            inputEnabled = false;
            velocity = Vector3.zero;
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!inputEnabled)
            {
                return;
            }

            ReadInput();
            ApplyGravity();
            MoveCharacter();
        }

        private void ReadInput()
        {
            Vector2 input = ReadMovementInput();
            Vector3 direction = new Vector3(input.x, 0f, input.y);

            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            if (direction.sqrMagnitude > 0.0001f)
            {
                var cameraForward = Camera.main != null
                    ? Vector3.Scale(Camera.main.transform.forward, new Vector3(1f, 0f, 1f)).normalized
                    : Vector3.forward;
                var cameraRight = Camera.main != null
                    ? Camera.main.transform.right
                    : Vector3.right;
                cameraRight.y = 0f;
                cameraRight.Normalize();

                Vector3 moveDirection = (cameraForward * direction.z + cameraRight * direction.x);
                moveDirection.y = 0f;
                controller.Move(moveDirection * moveSpeed * Time.deltaTime);

                var lookRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
            }
            if (ReadJumpInput())
            {
                jumpBuffered = true;
            }
        }

        private Vector2 ReadMovementInput()
        {
            Vector2 input = Vector2.zero;

#if ENABLE_LEGACY_INPUT_MANAGER
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (input == Vector2.zero)
            {
                if (Keyboard.current != null)
                {
                    float x = 0f;
                    float y = 0f;

                    if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    {
                        x -= 1f;
                    }

                    if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    {
                        x += 1f;
                    }

                    if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                    {
                        y -= 1f;
                    }

                    if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    {
                        y += 1f;
                    }

                    input = new Vector2(x, y);
                }

                if (Gamepad.current != null)
                {
                    Vector2 stick = Gamepad.current.leftStick.ReadValue();
                    if (stick.sqrMagnitude > input.sqrMagnitude)
                    {
                        input = stick;
                    }
                }
            }
#endif

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            return input;
        }

        private bool ReadJumpInput()
        {
            bool jumpPressed = false;

#if ENABLE_LEGACY_INPUT_MANAGER
            jumpPressed |= Input.GetButtonDown("Jump");
#endif

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Keyboard.current != null)
            {
                jumpPressed |= Keyboard.current.spaceKey.wasPressedThisFrame;
            }

            if (Gamepad.current != null)
            {
                jumpPressed |= Gamepad.current.buttonSouth.wasPressedThisFrame;
            }
#endif

            return jumpPressed;
        }

        private void ApplyGravity()
        {
            if (controller.isGrounded)
            {
                velocity.y = -2f;
                if (jumpBuffered)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }

            jumpBuffered = false;
        }

        private void MoveCharacter()
        {
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
