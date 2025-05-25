using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TestPlayerController))]
public class TestPreInputBridge : MonoBehaviour
{
    private TestPlayerController controller;
    private PreInputSystem preInputSystem;
    private PlayerInput input;

    public float jumpBufferTime = 0.2f;
    public float dashBufferTime = 0.2f;

    void Awake()
    {
        controller = GetComponent<TestPlayerController>();
        preInputSystem = FindObjectOfType<PreInputSystem>();
        input = new PlayerInput();
    }

    void OnEnable()
    {
        input.Player.Jump.performed += OnJump;
        input.Player.Dash.performed += OnDash;
        input.Enable();
    }

    void OnDisable()
    {
        input.Player.Jump.performed -= OnJump;
        input.Player.Dash.performed -= OnDash;
        input.Disable();
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        Debug.Log("Jump pressed");

        preInputSystem.RegisterCommand(new InputCommand
        {
            Type = InputCommandType.Jump,
            Timestamp = Time.time,
            BufferTime = jumpBufferTime,
            Condition = () => controller.IsGrounded,
            Execute = controller.Jump
        });
    }

    void OnDash(InputAction.CallbackContext ctx)
    {
        Debug.Log("Dash pressed");

        preInputSystem.RegisterCommand(new InputCommand
        {
            Type = InputCommandType.Dash,
            Timestamp = Time.time,
            BufferTime = dashBufferTime,
            Condition = () => controller.IsGrounded && controller.CanDash(),
            Execute = controller.Dash
        });
    }
}
