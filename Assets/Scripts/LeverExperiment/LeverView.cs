using DG.Tweening;
using UnityEngine;

public sealed class LeverView : MonoBehaviour
{
    [SerializeField] private Transform leverBeam;
    [SerializeField] private Transform fulcrum;
    [SerializeField] private Transform leftWeight;
    [SerializeField] private Transform rightWeight;
    [SerializeField] private LineRenderer leftWeightConnector;
    [SerializeField] private LineRenderer rightWeightConnector;
    [SerializeField] private float worldUnitsPerMetre = 2f;
    [SerializeField] private float maximumAngle = 20f;
    [SerializeField] private float animationDuration = 0.45f;
    [SerializeField] private Vector3 weightBaseScale = new Vector3(.42f, .42f, .42f);
    [SerializeField] private float referenceMassKg = .2f;

    private Tween rotationTween;

    private void Awake()
    {
        if (leftWeightConnector == null) leftWeightConnector = CreateConnector("Left Weight Connector", new Color(1f, .5f, .25f));
        if (rightWeightConnector == null) rightWeightConnector = CreateConnector("Right Weight Connector", new Color(.45f, 1f, .55f));
    }

    public Transform LeftWeight => leftWeight;
    public Transform RightWeight => rightWeight;
    public Transform Fulcrum => fulcrum;
    public float WorldUnitsPerMetre => worldUnitsPerMetre;

    public void Render(LeverSnapshot snapshot, bool animate)
    {
        float torqueDifference = snapshot.leftTorque - snapshot.rightTorque;
        float angle = Mathf.Clamp(torqueDifference * 8f, -maximumAngle, maximumAngle);
        rotationTween?.Kill();
        if (animate)
            rotationTween = leverBeam.DORotate(new Vector3(0f, 0f, angle), animationDuration)
                .SetEase(Ease.OutCubic)
                .OnUpdate(() => UpdateWeightPositions(leverBeam.eulerAngles.z > 180f ? leverBeam.eulerAngles.z - 360f : leverBeam.eulerAngles.z, snapshot));
        else
        {
            leverBeam.rotation = Quaternion.Euler(0f, 0f, angle);
            UpdateWeightPositions(angle, snapshot);
        }
    }

    private void UpdateWeightPositions(float angle, LeverSnapshot snapshot)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        Vector3 leftOffset = Vector3.left * snapshot.leftDistance * worldUnitsPerMetre + Vector3.down * 0.38f;
        Vector3 rightOffset = Vector3.right * snapshot.rightDistance * worldUnitsPerMetre + Vector3.down * 0.38f;
        float referenceMass = Mathf.Max(.01f, referenceMassKg);
        Vector3 scale = weightBaseScale * Mathf.Pow(snapshot.leftMassKg / referenceMass, 1f / 3f);
        leftWeight.localScale = scale;
        rightWeight.localScale = weightBaseScale * Mathf.Pow(snapshot.rightMassKg / referenceMass, 1f / 3f);
        leftWeight.position = fulcrum.position + rotation * leftOffset;
        rightWeight.position = fulcrum.position + rotation * rightOffset;
        RenderConnectors();
    }

    private void RenderConnectors()
    {
        Vector3 leftWeightPoint = leftWeight.position + Vector3.up * (leftWeight.localScale.y * .5f);
        Vector3 rightWeightPoint = rightWeight.position + Vector3.up * (rightWeight.localScale.y * .5f);
        Vector3 leftBeamPoint = GetVerticalProjectionOnBeam(leftWeightPoint);
        Vector3 rightBeamPoint = GetVerticalProjectionOnBeam(rightWeightPoint);
        SetConnector(leftWeightConnector, leftWeightPoint, leftBeamPoint);
        SetConnector(rightWeightConnector, rightWeightPoint, rightBeamPoint);
    }

    private Vector3 GetVerticalProjectionOnBeam(Vector3 point)
    {
        Vector3 beamDirection = leverBeam.right.normalized;
        float halfBeamLength = leverBeam.lossyScale.x * .5f;
        float alongBeam = Mathf.Abs(beamDirection.x) < .001f
            ? 0f
            : (point.x - leverBeam.position.x) / beamDirection.x;
        alongBeam = Mathf.Clamp(alongBeam, -halfBeamLength, halfBeamLength);
        return leverBeam.position + beamDirection * alongBeam;
    }

    private static void SetConnector(LineRenderer connector, Vector3 start, Vector3 end)
    {
        if (connector == null) return;
        connector.SetPosition(0, start);
        connector.SetPosition(1, end);
    }

    private LineRenderer CreateConnector(string name, Color color)
    {
        GameObject connectorObject = new GameObject(name);
        connectorObject.transform.SetParent(transform, false);
        LineRenderer connector = connectorObject.AddComponent<LineRenderer>();
        connector.positionCount = 2;
        connector.startWidth = .025f;
        connector.endWidth = .025f;
        connector.material = new Material(Shader.Find("Sprites/Default"));
        connector.startColor = color;
        connector.endColor = color;
        connector.sortingOrder = 1;
        return connector;
    }

    private void OnDestroy() => rotationTween?.Kill();
}
