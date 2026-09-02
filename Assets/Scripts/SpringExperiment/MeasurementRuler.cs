using UnityEngine;

public sealed class MeasurementRuler : MonoBehaviour
{
    private LineRenderer line;
    private Transform anchor;
    private Transform mass;
    public bool Visible { get; private set; } = true;

    public void Build(Transform top, Transform weight)
    {
        anchor = top; mass = weight; line = gameObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default")); line.startColor = Color.white; line.endColor = Color.white;
        line.startWidth = line.endWidth = 0.012f; line.positionCount = 2;
    }

    public void SetVisible(bool value) { Visible = value; gameObject.SetActive(value); }
    public void Render(float naturalLength, float currentLength)
    {
        if (!Visible || anchor == null || mass == null) return;
        Vector3 a = anchor.position + Vector3.right * 0.52f; Vector3 b = mass.position + Vector3.up * 0.32f + Vector3.right * 0.52f;
        line.SetPosition(0, a); line.SetPosition(1, b);
    }
}
