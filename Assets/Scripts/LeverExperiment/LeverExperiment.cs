using UnityEngine;

public sealed class LeverExperiment : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] private float leftMassKg = 0.2f;
    [SerializeField] private float rightMassKg = 0.1f;
    [SerializeField] private float leftDistance = 0.5f;
    [SerializeField] private float rightDistance = 1f;

    [Header("Scene references")]
    [SerializeField] private LeverView view;
    [SerializeField] private LeverUI ui;
    [SerializeField] private ForceVector leftForceVector;
    [SerializeField] private ForceVector rightForceVector;
    [SerializeField] private MeasurementRuler ruler;

    private LeverPhysics physics;
    private bool showForces = true;
    private bool showRuler = true;

    public LeverView View => view;

    private void Start()
    {
        physics = new LeverPhysics(leftMassKg, rightMassKg, leftDistance, rightDistance);
        if (ui != null) ui.Bind(this);
        Refresh(false);
    }

    public void SetLeftMass(float value) { physics.SetMasses(value, physics.RightMassKg); Refresh(true); }
    public void SetRightMass(float value) { physics.SetMasses(physics.LeftMassKg, value); Refresh(true); }
    public void SetLeftDistance(float value) { physics.SetDistances(value, physics.RightDistance); Refresh(true); }
    public void SetRightDistance(float value) { physics.SetDistances(physics.LeftDistance, value); Refresh(true); }
    public void ToggleForces() { showForces = !showForces; Refresh(false); }
    public void ToggleRuler() { showRuler = !showRuler; if (ruler != null) ruler.SetVisible(showRuler); Refresh(false); }

    public void ResetExperiment()
    {
        physics.SetMasses(0.2f, 0.1f);
        physics.SetDistances(0.5f, 1f);
        showForces = true; showRuler = true;
        if (ruler != null) ruler.SetVisible(true);
        Refresh(true);
    }

    private void Refresh(bool animate)
    {
        if (physics == null || view == null) return;
        LeverSnapshot snapshot = physics.GetSnapshot();
        view.Render(snapshot, animate);
        float leftArrow = Mathf.Clamp(snapshot.leftForce * 0.15f, 0.15f, 0.8f);
        float rightArrow = Mathf.Clamp(snapshot.rightForce * 0.15f, 0.15f, 0.8f);
        if (leftForceVector != null) leftForceVector.Set(view.LeftWeight.position, Vector3.down, leftArrow, showForces);
        if (rightForceVector != null) rightForceVector.Set(view.RightWeight.position, Vector3.down, rightArrow, showForces);
        if (ruler != null && showRuler) ruler.Render(snapshot.leftDistance, snapshot.rightDistance);
        if (ui != null) ui.Refresh(snapshot);
    }
}
