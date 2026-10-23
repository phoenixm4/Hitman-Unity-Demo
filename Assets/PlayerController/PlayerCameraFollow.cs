using UnityEngine;

public sealed class PlayerCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField, Min(0f)] private float positionSmoothTime = 0.12f;
    [SerializeField, Min(0f)] private float rotationSharpness = 12f;

    private Vector3 positionVelocity;

    private void Awake()
    {
        if (target != null)
            return;

        Debug.LogError("PlayerCameraFollow requires a target Transform.", this);
        enabled = false;
    }

    private void LateUpdate()
    {
        var desiredPosition = target.position + followOffset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref positionVelocity,
            positionSmoothTime);

        var lookDirection = target.position + lookAtOffset - transform.position;
        if (lookDirection.sqrMagnitude <= 0.0001f)
            return;

        var desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        var interpolation = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, interpolation);
    }
}
