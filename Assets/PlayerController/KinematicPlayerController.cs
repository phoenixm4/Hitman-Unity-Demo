using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public sealed class KinematicPlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string moveActionMap = "Player";
    [SerializeField] private string moveActionName = "Move";
    [SerializeField, Min(0f)] private float moveSpeed = 4f;
    [SerializeField, Min(0f)] private float acceleration = 30f;
    [SerializeField, Min(0f)] private float deceleration = 40f;
    [SerializeField] private float gravity = -25f;
    [SerializeField, Min(0f)] private float groundSnapDistance = 0.5f;
    [SerializeField, Min(0f)] private float turnSpeed = 720f;
    [SerializeField] private Transform movementCamera;

    private CharacterController characterController;
    private Animator animator;
    private InputAction moveAction;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private int speedParameterHash;
    private bool hasSpeedParameter;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        speedParameterHash = Animator.StringToHash("Speed");

        if (inputActions == null)
        {
            Debug.LogError("KinematicPlayerController requires the project's InputActionAsset.", this);
            enabled = false;
            return;
        }

        moveAction = inputActions.FindAction($"{moveActionMap}/{moveActionName}", false);
        if (moveAction == null)
        {
            Debug.LogError($"Input action '{moveActionMap}/{moveActionName}' was not found.", this);
            enabled = false;
            return;
        }

        foreach (var parameter in animator.parameters)
        {
            if (parameter.nameHash == speedParameterHash &&
                parameter.type == AnimatorControllerParameterType.Float)
            {
                hasSpeedParameter = true;
                break;
            }
        }

        animator.applyRootMotion = false;
        verticalVelocity = -2f;
    }

    private void Start()
    {
        if (groundSnapDistance <= 0f)
            return;

        characterController.Move(Vector3.down * groundSnapDistance);
        if (characterController.isGrounded)
            verticalVelocity = -2f;
    }

    private void OnEnable()
    {
        moveAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        if (deltaTime <= 0f)
            return;

        var input = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        var moveDirection = GetMoveDirection(input);
        var targetVelocity = moveDirection * moveSpeed;
        var rate = moveDirection.sqrMagnitude > 0f ? acceleration : deceleration;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * deltaTime);

        if (moveDirection.sqrMagnitude > 0f)
        {
            var targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * deltaTime);
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * deltaTime;

        var previousPosition = transform.position;
        var motion = horizontalVelocity + Vector3.up * verticalVelocity;
        characterController.Move(motion * deltaTime);

        var actualDisplacement = transform.position - previousPosition;
        actualDisplacement.y = 0f;
        if (hasSpeedParameter)
            animator.SetFloat(speedParameterHash, actualDisplacement.magnitude / deltaTime);
    }

    private Vector3 GetMoveDirection(Vector2 input)
    {
        var cameraTransform = movementCamera;
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        var forward = cameraTransform != null
            ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up)
            : Vector3.forward;

        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;
        else
            forward.Normalize();

        var right = Vector3.Cross(Vector3.up, forward);
        return Vector3.ClampMagnitude(right * input.x + forward * input.y, 1f);
    }
}
