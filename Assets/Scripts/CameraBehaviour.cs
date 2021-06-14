using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform Target;
    public float SmoothSpeed = 0.125f;
    public Vector3 Offset;
    [SerializeField] public Vector3? Target2;

    public CameraShake CameraShake;
    public bool FollowPlayer;

    private void Start()
    {
        Offset = new Vector3(0, 0, -10);
        // transform.position = Target.position;
    }

    public void TriggerCameraShake(float duration, float magnitude)
    {
        StartCoroutine(CameraShake.Shake(duration, magnitude));
    }

    private void FixedUpdate()
    {
        if (!GameManager_.Instance.IsRunningGame) return;
        if (Target == null)
        {
            Target = GameManager_.Instance.Player.transform;

        }
        Vector3 desiredPosition;
        if (Target2 == null)
        {
            desiredPosition = Target.position + Offset;
            // GetComponent<Camera>().orthographicSize = 4.5f;
        }
        else
        {
            print(Target2);
            var bounds = new Bounds(Target.position, Vector3.zero);
            bounds.Encapsulate((Vector3)Target2);
            desiredPosition = bounds.center + Offset;
            GetComponent<Camera>().orthographicSize = Mathf.Clamp(Vector2.Distance(Target.position, (Vector3)Target2) - 7f, 4.5f, 7f);
        }
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);

        if (FollowPlayer) transform.position = smoothedPosition;

    }
}
