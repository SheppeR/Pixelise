using MessagePack;
using Network;
using Pixelise.Core.Network;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using World;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 5f;

        public float sprintSpeed = 8f;
        public float jumpHeight = 1.6f;
        public float gravity = -20f;

        [Header("Look")]
        public float mouseSensitivity = 2f;

        public Transform cameraTransform;

        [Header("Network")]
        public LiteNetClient network;

        [SerializeField]
        private BlockInteractor blockInteractor;

        private float cameraPitch;

        private CharacterController controller;
        private PlayerInputActions input;
        private bool jumpQueued;
        private Vector2 lookInput;

        private Vector2 moveInput;
        private bool sprintHeld;

        private float verticalVelocity;

        private void Awake()
        {
            gameObject.SetActive(false);
            WorldEvents.RegisterLocalPlayer(gameObject);

            controller = GetComponent<CharacterController>();
            input = new PlayerInputActions();

            if (cameraTransform == null)
            {
                cameraTransform = GetComponentInChildren<Camera>().transform;
            }

            if (blockInteractor == null)
            {
                blockInteractor = GetComponent<BlockInteractor>();
            }
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
            SendMovement();
        }

        private void OnEnable()
        {
            input.Player.Enable();

            input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            input.Player.Move.canceled += _ => moveInput = Vector2.zero;

            input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            input.Player.Look.canceled += _ => lookInput = Vector2.zero;

            input.Player.Jump.performed += _ => jumpQueued = true;

            input.Player.Sprint.performed += _ => sprintHeld = true;
            input.Player.Sprint.canceled += _ => sprintHeld = false;

            input.Player.Attack.performed += _ => OnAttack();
            input.Player.Use.performed += _ => OnUse();
        }

        private void OnDisable()
        {
            if (input != null)
                input.Player.Disable();
        }

        private void HandleMovement()
        {
            var grounded = controller.isGrounded;
            if (grounded && verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            var speed = sprintHeld ? sprintSpeed : walkSpeed;

            var move =
                transform.right * moveInput.x +
                transform.forward * moveInput.y;

            controller.Move(move * speed * Time.deltaTime);

            if (jumpQueued && grounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            jumpQueued = false;

            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        private void HandleLook()
        {
            cameraPitch -= lookInput.y * mouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);

            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
        }

        private void OnAttack()
        {
            blockInteractor.Break();
        }

        private void OnUse()
        {
            blockInteractor.Use();
        }

        // ========================
        // NETWORK SYNC
        // ========================
        private void SendMovement()
        {
            if (network == null)
            {
                return;
            }

            network.Send(new NetPacket
            {
                Type = PacketType.PlayerMove,
                Payload = MessagePackSerializer.Serialize(
                    new PlayerMovePacket
                    {
                        Position = transform.position.ToCore(),
                        Yaw = transform.eulerAngles.y
                    })
            });
        }
    }
}