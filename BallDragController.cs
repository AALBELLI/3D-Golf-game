using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class BallDragController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Rigidbody rb;

    [Header("Power Settings")]
    [SerializeField] private float powerMultiplier = 10f;
    [SerializeField] private float maxDragDistance = 5f;
    [SerializeField] private float minVelocityToShootAgain = 0.15f;

    [Header("Plane Settings")]
    [SerializeField] private float rayPlaneHeightOffset = 0f;

    [Header("UI")]
    [SerializeField] private Slider powerBar;

    private Vector3 dragStartWorld;
    private Vector3 dragCurrentWorld;
    private bool isDragging = false;

    private Plane dragPlane;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null || rb == null)
            return;

        // Only allow aiming when the ball is basically stopped
        if (rb.linearVelocity.magnitude > minVelocityToShootAgain)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            dragPlane = new Plane(Vector3.up, transform.position + Vector3.up * rayPlaneHeightOffset);

            if (GetMouseWorldPoint(out dragStartWorld))
            {
                isDragging = true;
                dragCurrentWorld = dragStartWorld;
            }
        }

    if (Input.GetMouseButton(0) && isDragging)
        {
        if (GetMouseWorldPoint(out Vector3 worldPoint))
        {
        dragCurrentWorld = worldPoint;

        Vector3 dragVector = dragStartWorld - dragCurrentWorld;
        dragVector.y = 0f;

        float powerPercent = Mathf.Clamp01(dragVector.magnitude / maxDragDistance);

        if (powerBar != null)
        {
            powerBar.value = powerPercent;
        }
        }
    }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            if (GetMouseWorldPoint(out Vector3 releasePoint))
            {
                dragCurrentWorld = releasePoint;
            }

            ShootBall();
            isDragging = false;
        }
    }

    private bool GetMouseWorldPoint(out Vector3 worldPoint)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float enter;

        if (dragPlane.Raycast(ray, out enter))
        {
            worldPoint = ray.GetPoint(enter);
            return true;
        }

        worldPoint = Vector3.zero;
        return false;
    }

    private void ShootBall()
    {
        Vector3 dragVector = dragStartWorld - dragCurrentWorld;

        // Keep movement flat on the ground
        dragVector.y = 0f;

        // Limit max power
        dragVector = Vector3.ClampMagnitude(dragVector, maxDragDistance);

        // Stop tiny accidental flicks
        if (dragVector.magnitude < 0.1f)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 force = dragVector * powerMultiplier;
        rb.AddForce(force, ForceMode.Impulse);

        if (powerBar != null)
            {
        powerBar.value = 0f;
            }
        
    }

    private void OnDrawGizmos()
    {
        if (!isDragging)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(dragStartWorld, 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(dragCurrentWorld, 0.2f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(dragStartWorld, dragCurrentWorld);
    }
}