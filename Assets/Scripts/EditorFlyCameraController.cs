using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lets the fallback camera simulate headset movement while testing in the
/// Unity Editor. This component is never attached to an XREAL XR camera.
/// </summary>
[DisallowMultipleComponent]
public sealed class EditorFlyCameraController : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float movementSpeed = 2.5f;
    [SerializeField, Min(0.1f)] private float sprintMultiplier = 3f;
    [SerializeField, Min(0.01f)] private float lookSensitivity = 0.12f;

    private float pitch;
    private float yaw;

    private void Awake()
    {
        Vector3 angles = transform.eulerAngles;
        pitch = angles.x > 180f ? angles.x - 360f : angles.x;
        yaw = angles.y;
    }

    private void Update()
    {
#if UNITY_EDITOR
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null)
        {
            return;
        }

        bool isLooking = mouse.rightButton.isPressed;
        Cursor.lockState = isLooking ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLooking;
        if (!isLooking)
        {
            return;
        }

        Vector2 lookDelta = mouse.delta.ReadValue();
        yaw += lookDelta.x * lookSensitivity;
        pitch = Mathf.Clamp(pitch - lookDelta.y * lookSensitivity, -89f, 89f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        float horizontal = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
        float forward = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
        float vertical = (keyboard.eKey.isPressed ? 1f : 0f) - (keyboard.qKey.isPressed ? 1f : 0f);

        Vector3 direction = transform.right * horizontal + transform.forward * forward + Vector3.up * vertical;
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        float speed = movementSpeed * (keyboard.leftShiftKey.isPressed ? sprintMultiplier : 1f);
        transform.position += direction * speed * Time.unscaledDeltaTime;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
#endif
    }
}
