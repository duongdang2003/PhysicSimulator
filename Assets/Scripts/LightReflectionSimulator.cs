using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class LightReflectionSimulator : MonoBehaviour
{
    [SerializeField] private Transform ground;
    [SerializeField] private Transform reflectionPlane;
    [SerializeField] private Material pointMaterial;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float pointSize = 0.2f;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI angleText;

    private Vector3[] points = new Vector3[2];
    private int pointCount = 0;
    private GameObject[] pointSpheres = new GameObject[2];
    private LineRenderer[] lineRenderers = new LineRenderer[2];
    private Plane groundPlane;
    private Plane reflectionPlaneObj;

    private void Start()
    {
        if (ground == null)
        {
            Debug.LogError("Ground object is not assigned!");
            return;
        }

        if (reflectionPlane == null)
        {
            Debug.LogError("Reflection plane is not assigned!");
            return;
        }

        groundPlane = new Plane(ground.up, ground.position);
        reflectionPlaneObj = new Plane(reflectionPlane.up, reflectionPlane.position);

        if (instructionText != null)
        {
            instructionText.text = "Click thể đặt điểm thứ nhất";
        }
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        // Check if click is over UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            instructionText.text = "Không thể đặt ở đây";
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        // Check if click is on the ground plane
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 clickPoint = ray.origin + ray.direction * distance;
            AddPoint(clickPoint);
        }
        else
        {
            instructionText.text = "";
        }
    }

    private void AddPoint(Vector3 point)
    {
        if (pointCount >= 2)
        {
            ResetPoints();
        }

        points[pointCount] = point;

        CreatePointSphere(pointCount, point);

        if (pointCount == 0)
        {
            instructionText.text = "Click để đặt điểm thứ 2";
        }
        else if (pointCount == 1)
        {
            DrawLightRays();
            instructionText.text = "Kết quả";
        }

        pointCount++;
    }

    private void CreatePointSphere(int index, Vector3 position)
    {
        if (pointSpheres[index] != null)
            Destroy(pointSpheres[index]);

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * pointSize;

        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.material = pointMaterial != null ? pointMaterial : new Material(Shader.Find("Standard"));
        renderer.material.color = index == 0 ? Color.red : Color.blue;

        Collider collider = sphere.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        pointSpheres[index] = sphere;
    }

    private void DrawLightRays()
    {
        Vector3 point1 = points[0];
        Vector3 point2 = points[1];

        // Validate: Point 2 must be lower than Point 1 (Y position)
        if (point2.y >= point1.y)
        {
            instructionText.text = "Click lại để đặt điểm thứ hai";
            angleText.text = "Điểm thứ hai phải thấp hơn điểm thứ nhất";

            return;
        }

        // Direct ray from point 1 toward point 2
        Vector3 incidentDirection = (point2 - point1).normalized;

        // Find intersection with reflection plane at (0,0,0)
        Vector3 reflectionPoint = GetPlaneIntersection(point1, incidentDirection, reflectionPlaneObj);

        // Calculate reflected ray using plane normal
        Vector3 planeNormal = reflectionPlaneObj.normal;
        Vector3 reflectedDirection = Vector3.Reflect(incidentDirection, planeNormal);
        Vector3 reflectedEndPoint = reflectionPoint + reflectedDirection * 50f;

        // Draw rays
        DrawLineRenderer(0, point1, reflectionPoint, Color.yellow); // Incident ray
        DrawLineRenderer(1, reflectionPoint, reflectedEndPoint, Color.cyan); // Reflected ray

        // Calculate angles (angle from ray to normal)
        float incidentAngle = Vector3.Angle(-incidentDirection, planeNormal);
        float reflectionAngle = Vector3.Angle(reflectedDirection, planeNormal);

        angleText.text = $"Góc tới: {incidentAngle:F1}° \n Góc phản xạ: {reflectionAngle:F1}°";
    }

    private Vector3 GetPlaneIntersection(Vector3 origin, Vector3 direction, Plane plane)
    {
        Ray ray = new Ray(origin, direction);
        if (plane.Raycast(ray, out float distance))
        {
            return origin + direction * distance;
        }
        return origin;
    }

    private void DrawLineRenderer(int index, Vector3 start, Vector3 end, Color color)
    {
        if (lineRenderers[index] == null)
        {
            GameObject lineObj = new GameObject($"LightRay_{index}");
            lineObj.transform.parent = transform;
            lineRenderers[index] = lineObj.AddComponent<LineRenderer>();

            if (lineMaterial != null)
                lineRenderers[index].material = lineMaterial;
            else
                lineRenderers[index].material = new Material(Shader.Find("Standard"));

            lineRenderers[index].startWidth = 0.1f;
            lineRenderers[index].endWidth = 0.1f;
            lineRenderers[index].positionCount = 2;
        }

        lineRenderers[index].SetPosition(0, start);
        lineRenderers[index].SetPosition(1, end);
        lineRenderers[index].startColor = color;
        lineRenderers[index].endColor = color;

    }

    private void ResetPoints()
    {
        pointCount = 0;
        for (int i = 0; i < 2; i++)
        {
            if (pointSpheres[i] != null)
                Destroy(pointSpheres[i]);
            if (lineRenderers[i] != null)
                Destroy(lineRenderers[i].gameObject);
        }
    }
}
