using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerOrbitCameraInput : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private CinemachinePanTilt panTilt;
    [SerializeField, Min(0f)] private float sensitivity = 0.12f;
    [SerializeField, Min(0f)] private float initialOrbitHeight = 3f;
    [SerializeField] private Vector2 orbitElevationRange = new Vector2(-30f, 65f);
    [SerializeField] private Vector2 lookPitchRange = new Vector2(-70f, 70f);

    private bool cursorLocked;

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogError("PlayerOrbitCameraInput requires a player Transform.", this);
            enabled = false;
            return;
        }

        orbitalFollow ??= GetComponent<CinemachineOrbitalFollow>();
        panTilt ??= GetComponent<CinemachinePanTilt>();
        if (orbitalFollow == null || panTilt == null)
        {
            Debug.LogError("PlayerOrbitCameraInput requires Cinemachine Orbital Follow and Pan Tilt components.", this);
            enabled = false;
            return;
        }

        var yaw = Mathf.DeltaAngle(0f, target.eulerAngles.y);
        var radius = Mathf.Max(orbitalFollow.Radius, 0.01f);
        var initialElevation = Mathf.Asin(Mathf.Clamp(initialOrbitHeight / radius, -1f, 1f)) * Mathf.Rad2Deg;

        orbitalFollow.TrackerSettings.BindingMode = Unity.Cinemachine.TargetTracking.BindingMode.WorldSpace;
        orbitalFollow.HorizontalAxis.Value = orbitalFollow.HorizontalAxis.ClampValue(yaw);
        orbitalFollow.VerticalAxis.Range = orbitElevationRange;
        orbitalFollow.VerticalAxis.Value = orbitalFollow.VerticalAxis.ClampValue(initialElevation);

        panTilt.ReferenceFrame = CinemachinePanTilt.ReferenceFrames.World;
        panTilt.RecenterTarget = CinemachinePanTilt.RecenterTargetModes.AxisCenter;
        panTilt.PanAxis.Value = panTilt.PanAxis.ClampValue(yaw);
        panTilt.TiltAxis.Range = lookPitchRange;
        panTilt.TiltAxis.Value = 0f;
    }

    private void Start()
    {
        SetCursorLocked(false);
    }

    private void Update()
    {
        if (!Application.isFocused)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetCursorLocked(false);

        var mouse = Mouse.current;
        var shouldLook = mouse != null && mouse.rightButton.isPressed;
        if (cursorLocked != shouldLook)
            SetCursorLocked(shouldLook);

        if (!cursorLocked || mouse == null)
        {
            return;
        }

        var delta = mouse.delta.ReadValue();
        var yaw = orbitalFollow.HorizontalAxis;
        yaw.Value = yaw.ClampValue(yaw.Value + delta.x * sensitivity);
        orbitalFollow.HorizontalAxis = yaw;

        var elevation = orbitalFollow.VerticalAxis;
        elevation.Value = elevation.ClampValue(elevation.Value + delta.y * sensitivity);
        orbitalFollow.VerticalAxis = elevation;

        var pan = panTilt.PanAxis;
        pan.Value = pan.ClampValue(pan.Value + delta.x * sensitivity);
        panTilt.PanAxis = pan;

        var tilt = panTilt.TiltAxis;
        tilt.Value = tilt.ClampValue(tilt.Value - delta.y * sensitivity);
        panTilt.TiltAxis = tilt;
    }

    private void OnDisable()
    {
        SetCursorLocked(false);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SetCursorLocked(false);
    }

    private void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
