using UnityEngine;
using UnityEngine.InputSystem;

public sealed class LeverController : MonoBehaviour
{
    [SerializeField] private LeverExperiment experiment;
    [SerializeField] private Camera experimentCamera;
    [SerializeField] private float minimumDistance = 0.2f;
    [SerializeField] private float maximumDistance = 1.3f;

    private Transform draggedWeight;
    private Plane dragPlane;

    private void Update()
    {
        if (experimentCamera == null || experiment == null) return;
        if (Mouse.current == null) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = experimentCamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && (hit.transform == experiment.View.LeftWeight || hit.transform == experiment.View.RightWeight))
            {
                draggedWeight = hit.transform;
                dragPlane = new Plane(Vector3.forward, draggedWeight.position);
            }
        }
        if (Mouse.current.leftButton.isPressed && draggedWeight != null)
        {
            Ray ray = experimentCamera.ScreenPointToRay(mousePosition);
            if (dragPlane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                float offset = point.x - experiment.View.Fulcrum.position.x;
                float value = Mathf.Clamp(Mathf.Abs(offset) / experiment.View.WorldUnitsPerMetre, minimumDistance, maximumDistance);
                if (draggedWeight == experiment.View.LeftWeight) experiment.SetLeftDistance(value);
                else experiment.SetRightDistance(value);
            }
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame) draggedWeight = null;
    }
}
