using UnityEngine;

public sealed class KingController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float turnSpeed = 16f;
    [SerializeField] private float mapRadius = 28f;
    [SerializeField] private Transform cameraTransform;

    private IPlayerInputSource inputSource;
    private Vector3 velocity;

    public float MapRadius
    {
        get => mapRadius;
        set => mapRadius = Mathf.Max(1f, value);
    }

    public Transform CameraTransform
    {
        get => cameraTransform;
        set => cameraTransform = value;
    }

    private void Awake()
    {
        inputSource = GetComponent<IPlayerInputSource>();
        if (inputSource == null)
        {
            inputSource = gameObject.AddComponent<KeyboardPlayerInputSource>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        Vector2 input = inputSource?.Move ?? Vector2.zero;
        Vector3 move = GetCameraRelativeMove(input);

        velocity = move * moveSpeed;
        transform.position = ClampToMap(transform.position + velocity * Time.deltaTime);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetCameraRelativeMove(Vector2 input)
    {
        if (input.sqrMagnitude <= 0.0001f)
        {
            return Vector3.zero;
        }

        Transform reference = cameraTransform != null ? cameraTransform : transform;
        Vector3 forward = reference.forward;
        Vector3 right = reference.right;
        forward.y = 0f;
        right.y = 0f;

        if (forward.sqrMagnitude <= 0.0001f || right.sqrMagnitude <= 0.0001f)
        {
            return Vector3.zero;
        }

        forward.Normalize();
        right.Normalize();

        Vector3 move = right * input.x + forward * input.y;
        return move.sqrMagnitude > 1f ? move.normalized : move;
    }

    private Vector3 ClampToMap(Vector3 position)
    {
        Vector2 flat = new Vector2(position.x, position.z);
        if (flat.magnitude <= mapRadius)
        {
            return position;
        }

        flat = flat.normalized * mapRadius;
        return new Vector3(flat.x, position.y, flat.y);
    }
}
