using UnityEngine;

public sealed class ForceVector : MonoBehaviour
{
    [SerializeField] private Color color = Color.white;
    [SerializeField] private float headSize = 0.09f;
    private LineRenderer shaft;
    private Transform head;

    public void Build(Color forceColor)
    {
        color = forceColor;
        shaft = gameObject.AddComponent<LineRenderer>();
        shaft.positionCount = 2; shaft.startWidth = 0.025f; shaft.endWidth = 0.025f;
        shaft.material = new Material(Shader.Find("Sprites/Default")); shaft.startColor = color; shaft.endColor = color;
        // Unity's built-in PrimitiveType has no Cone in supported editor versions.
        // A short cylinder provides a clear, compatible arrow tip in this 2D-style view.
        var cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder); cone.name = "ArrowHead"; cone.transform.SetParent(transform);
        head = cone.transform; head.localScale = new Vector3(headSize, headSize * 1.8f, headSize);
        var renderer = cone.GetComponent<Renderer>(); renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit")); renderer.material.color = color;
        Destroy(cone.GetComponent<Collider>());
    }

    public void Set(Vector3 origin, Vector3 direction, float magnitude, bool visible)
    {
        if (shaft == null) return;
        gameObject.SetActive(visible); if (!visible) return;
        Vector3 end = origin + direction.normalized * magnitude;
        shaft.SetPosition(0, origin); shaft.SetPosition(1, end);
        head.position = end; head.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
    }
}
