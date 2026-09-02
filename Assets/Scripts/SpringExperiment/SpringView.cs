using UnityEngine;

public sealed class SpringView : MonoBehaviour
{
    [SerializeField] private Transform fixedPoint;
    [SerializeField] private Transform mass;
    [SerializeField] private LineRenderer springLine;
    [SerializeField] private int coils = 18;
    [SerializeField] private float coilWidth = 0.12f;
    // World-space scale keeps the full supported range visible in the camera.
    public float PixelsPerMetre => 3.0f;

    public void Build(Transform anchor, Transform weight, Material springMaterial)
    {
        fixedPoint = anchor; mass = weight;
        springLine = gameObject.AddComponent<LineRenderer>(); springLine.material = springMaterial;
        springLine.startWidth = 0.035f; springLine.endWidth = 0.035f; springLine.positionCount = coils * 2 + 2;
    }

    public void Render(float length)
    {
        if (fixedPoint == null || mass == null) return;
        Vector3 top = fixedPoint.position; Vector3 bottom = mass.position + Vector3.up * (mass.localScale.y * 0.5f + 0.045f);
        for (int i = 0; i < springLine.positionCount; i++)
        {
            float t = i / (float)(springLine.positionCount - 1);
            float x = i == 0 || i == springLine.positionCount - 1 ? 0f : (i % 2 == 0 ? -coilWidth : coilWidth);
            springLine.SetPosition(i, Vector3.Lerp(top, bottom, t) + Vector3.right * x);
        }
    }
}
