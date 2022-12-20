using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    class CameraState
    {
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;

        public void SetFromTransform(Transform t)
        {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        public void Translate(Vector3 translation)
        {
            Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

            x += rotatedTranslation.x;
            y += rotatedTranslation.y;
            z += rotatedTranslation.z;
        }

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
        {
            yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

            x = Mathf.Lerp(x, target.x, positionLerpPct);
            y = Mathf.Lerp(y, target.y, positionLerpPct);
            z = Mathf.Lerp(z, target.z, positionLerpPct);
        }

        public void UpdateTransform(Transform t)
        {
            t.eulerAngles = new Vector3(pitch, yaw, roll);
            t.position = new Vector3(x, y, z);
        }

    }

    Vector2 inputMovement;
    Vector2 inputLook;

    private readonly CameraState m_TargetCameraState = new CameraState();
    private readonly CameraState m_InterpolatingCameraState = new CameraState();

    [Tooltip("Whether or not to invert our Y axis for mouse input to rotation."), SerializeField]
    private bool invertY;

    [Header("Movement Settings"), Tooltip("Movement Sensitivity Factor."), Range(0.001f, 1f), SerializeField]
    private float movementSensitivityFactor = 0.1f;

    [Header("Rotation Settings"),
         Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation."), SerializeField]
    private AnimationCurve mouseSensitivityCurve =
            new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    public void Look(InputAction.CallbackContext value)
    {
        inputLook = value.ReadValue<Vector2>();
    }

    public void OnMovement(InputAction.CallbackContext value)
    {
        inputMovement = value.ReadValue<Vector2>();
        
    }

    private void OnEnable()
    {
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
    }

    private void FixedUpdate()
    {

        UpdateTargetCameraStateDirection(inputMovement);
        UpdateTargetCameraStateFromInput(inputLook);

        // Framerate-independent interpolation
        // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / 0.2f) * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / 0.2f) * Time.deltaTime);
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);
        m_InterpolatingCameraState.UpdateTransform(transform);
    }

    private void UpdateTargetCameraStateDirection(Vector2 input)
    {
        if (!invertY)
        {
            input.y *= -1;
        }

        var translation = Vector3.right * input.x * movementSensitivityFactor;
        translation += Vector3.back * input.y * movementSensitivityFactor;
        translation *= Mathf.Pow(2.0f, 3.5f);
        m_TargetCameraState.Translate(translation);
    }

    private void UpdateTargetCameraStateFromInput(Vector2 input)
    {
        if (!invertY)
        {
            input.y *= -1;
        }

        float mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(input.magnitude);

        m_TargetCameraState.yaw += input.x * mouseSensitivityFactor;
        m_TargetCameraState.pitch += input.y * mouseSensitivityFactor;
    }
}