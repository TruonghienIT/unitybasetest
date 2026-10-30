using UnityEngine;
using UnityEngine.EventSystems;

public class FreeCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float targetHeight = 1.6f;
    [SerializeField] private float sensitivity = 2.8f;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Rotation Limits")]
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 65f;

    [Header("Zoom Limits")]
    [SerializeField] private float minDistance = 2.5f;
    [SerializeField] private float maxDistance = 9f;

    private float _yaw;
    private float _pitch = 15f;

    private void Start()
    {
        if (target == null)
        {
            enabled = false;
            return;
        }

        _yaw = transform.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            _yaw += Input.GetAxis("Mouse X") * sensitivity;
            _pitch = Mathf.Clamp(_pitch - Input.GetAxis("Mouse Y") * sensitivity, minPitch, maxPitch);
        }

        distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5f, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 focus = target.position + Vector3.up * targetHeight;
        Vector3 desiredPosition = focus + rotation * Vector3.back * distance;

        float smoothFactor = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothFactor);
        transform.rotation = rotation;
    }
}