using UnityEngine;

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
            var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
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

            if (Input.GetButtonDown("Jump"))
            {
                jumpBuffered = true;
            }
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
