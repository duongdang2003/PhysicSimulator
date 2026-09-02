using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using DG.Tweening;

public sealed class SpringExperiment : MonoBehaviour
{
    [Header("Model")][SerializeField] private float naturalLength = 0.20f;
    [SerializeField] private float massKg = 0.20f;
    [SerializeField] private float stiffness = 20f;
    [SerializeField] private Vector3 baseWeightScale = new Vector3(.55f, .55f, .55f);
    [SerializeField] private float weightReferenceMassKg = 0.20f;
    [SerializeField] private float weightMoveDuration = 0.8f;
    [Header("Scene references (optional - auto-created when empty)")]
    [SerializeField] private Camera experimentCamera;
    [SerializeField] private Transform anchor, weight;
    [SerializeField] private SpringView view;
    [SerializeField] private ForceVector gravityVector, elasticVector;
    [SerializeField] private MeasurementRuler ruler;
    [SerializeField] private ExperimentUI ui;
    private SpringPhysics physics;
    private bool showForces = true;
    private Material springMaterial;
    private Tween weightMoveTween;

    private void Start()
    {
        physics = new SpringPhysics(naturalLength, massKg, stiffness);
        if (!ValidateSceneReferences()) return;
        ui.Bind(this);
        Refresh();
    }

    [ContextMenu("Create / repair experiment objects in scene")]
    public void SetupObjectsInScene()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("[SpringExperiment] Setup is Edit Mode only. Stop Play Mode, then run this command again.", this);
            return;
        }
        if (physics == null) physics = new SpringPhysics(naturalLength, massKg, stiffness);
        BuildWorld();
        BuildCanvas();
    }

    private bool ValidateSceneReferences()
    {
        bool valid = true;
        if (experimentCamera == null) { Debug.LogError("[SpringExperiment] Missing Experiment Camera. Run 'Create / repair experiment objects in scene' in Edit Mode.", this); valid = false; }
        if (anchor == null) { Debug.LogError("[SpringExperiment] Missing Anchor reference. Run scene setup in Edit Mode.", this); valid = false; }
        if (weight == null) { Debug.LogError("[SpringExperiment] Missing Weight reference. Run scene setup in Edit Mode.", this); valid = false; }
        if (view == null) { Debug.LogError("[SpringExperiment] Missing SpringView reference. Run scene setup in Edit Mode.", this); valid = false; }
        if (ruler == null) { Debug.LogError("[SpringExperiment] Missing MeasurementRuler reference. Run scene setup in Edit Mode.", this); valid = false; }
        if (gravityVector == null || elasticVector == null) { Debug.LogError("[SpringExperiment] Missing force vector reference(s). Run scene setup in Edit Mode.", this); valid = false; }
        if (ui == null) { Debug.LogError("[SpringExperiment] Missing ExperimentUI reference. Assign it in the Inspector.", this); valid = false; }
        else if (!ui.HasRequiredReferences()) { Debug.LogError("[SpringExperiment] ExperimentUI has missing UI references. Assign them in the Inspector.", ui); valid = false; }
        return valid;
    }

    private void BuildWorld()
    {
        if (anchor == null) anchor = MakeCube("Giá đỡ - điểm treo", new Vector3(-1.3f, 2.4f, 0), new Vector3(3.8f, .18f, .45f), new Color(.18f, .25f, .31f));
        if (GameObject.Find("Cột giá đỡ") == null) MakeCube("Cột giá đỡ", new Vector3(-2.9f, 0.8f, 0), new Vector3(.18f, 3.2f, .45f), new Color(.18f, .25f, .31f));
        if (weight == null) { weight = GameObject.CreatePrimitive(PrimitiveType.Cube).transform; weight.name = "Vật nặng"; weight.localScale = baseWeightScale; weight.GetComponent<Renderer>().material = Mat(new Color(.95f, .43f, .16f)); }
        if (view == null) { view = new GameObject("SpringView").AddComponent<SpringView>(); springMaterial = Mat(new Color(.20f, .80f, .95f)); view.Build(anchor, weight, springMaterial); }
        if (ruler == null) { ruler = new GameObject("MeasurementRuler").AddComponent<MeasurementRuler>(); ruler.Build(anchor, weight); }
        if (gravityVector == null) gravityVector = MakeForce("Trọng lực", new Color(1f, .28f, .22f));
        if (elasticVector == null) elasticVector = MakeForce("Lực đàn hồi", new Color(.25f, 1f, .4f));
        if (GameObject.Find("Lò xo") == null) MakeLabel("Lò xo", new Vector3(-1.0f, 2.18f, -.05f), 0.25f, Color.cyan);
        if (GameObject.Find("Thước đo") == null) MakeLabel("Thước đo", new Vector3(-.55f, 1.0f, -.05f), .2f, Color.white);
        if (experimentCamera == null) experimentCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
        if (experimentCamera == null) experimentCamera = new GameObject("Main Camera").AddComponent<Camera>();
        experimentCamera.tag = "MainCamera"; experimentCamera.transform.position = new Vector3(-1.2f, .9f, -8f); experimentCamera.transform.LookAt(new Vector3(-1.2f, 1.2f, 0)); experimentCamera.orthographic = true; experimentCamera.orthographicSize = 3.7f; experimentCamera.backgroundColor = new Color(.025f, .045f, .08f);
        if (GameObject.Find("Key Light") == null) { var light = new GameObject("Key Light").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.2f; light.transform.rotation = Quaternion.Euler(35, -30, 0); }
    }
    private ForceVector MakeForce(string n, Color c) { var f = new GameObject(n).AddComponent<ForceVector>(); f.Build(c); return f; }
    private void BuildCanvas() { if (EventSystem.current == null) { var events = new GameObject("EventSystem"); events.AddComponent<EventSystem>(); events.AddComponent<InputSystemUIInputModule>(); } if (ui != null) ui.Bind(this); }
    private Transform MakeCube(string n, Vector3 pos, Vector3 scale, Color color) { var t = GameObject.CreatePrimitive(PrimitiveType.Cube).transform; t.name = n; t.position = pos; t.localScale = scale; t.GetComponent<Renderer>().material = Mat(color); return t; }
    private void MakeLabel(string text, Vector3 pos, float size, Color color) { var t = new GameObject(text).AddComponent<TextMesh>(); t.text = text; t.fontSize = 48; t.characterSize = size; t.color = color; t.anchor = TextAnchor.MiddleCenter; t.transform.position = pos; }
    private Material Mat(Color c) { var m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color = c; return m; }
    private void Refresh(bool animateWeight = false)
    {
        Vector3 targetPosition = new Vector3(anchor.position.x, anchor.position.y - physics.CurrentLength * view.PixelsPerMetre, 0);
        UpdateWeightSize();

        if (animateWeight)
        {
            weightMoveTween?.Kill();
            weightMoveTween = weight.DOMove(targetPosition, Mathf.Max(0.01f, weightMoveDuration))
                .SetEase(Ease.OutCubic)
                .SetTarget(weight)
                .OnUpdate(UpdateVisuals)
                .OnComplete(UpdateVisuals);
        }
        else if (weightMoveTween == null || !weightMoveTween.IsActive())
        {
            weight.position = targetPosition;
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        view.Render(physics.CurrentLength);
        ruler.Render(physics.NaturalLength, physics.CurrentLength);
        float arrow = 0.38f + Mathf.Clamp(physics.Weight / 10f, .05f, .35f);
        Vector3 o = weight.position + Vector3.right * .42f;
        gravityVector.Set(o, Vector3.down, arrow, showForces);
        elasticVector.Set(weight.position - Vector3.right * .42f, Vector3.up, arrow, showForces);
        ui.UpdateReadout(physics);
    }

    private void UpdateWeightSize() { float referenceMass = Mathf.Max(0.01f, weightReferenceMassKg); float volumeRatio = physics.MassKg / referenceMass; float linearScale = Mathf.Pow(volumeRatio, 1f / 3f); weight.localScale = baseWeightScale * linearScale; }
    public void ChangeMass(float delta) { float before = physics.MassKg; physics.SetMass(before + delta); Debug.Log($"[SpringExperiment][Model] Mass: {before:0.000} -> {physics.MassKg:0.000} kg | P={physics.Weight:0.000} N | Δl={physics.Extension * 100f:0.00} cm"); Refresh(true); }
    public void ChangeStiffness(float delta) { float before = physics.Stiffness; physics.SetStiffness(before + delta); Debug.Log($"[SpringExperiment][Model] Stiffness: {before:0.0} -> {physics.Stiffness:0.0} N/m | Δl={physics.Extension * 100f:0.00} cm"); Refresh(true); }
    public void ToggleForces() { showForces = !showForces; Debug.Log($"[SpringExperiment][Button] Toggle forces clicked; visible={showForces}"); Refresh(); }
    public void ToggleRuler() { ruler.SetVisible(!ruler.Visible); Debug.Log($"[SpringExperiment][Button] Toggle ruler clicked; visible={ruler.Visible}"); Refresh(); }
    public void ResetExperiment() { physics.SetMass(.20f); physics.SetStiffness(20f); showForces = true; ruler.SetVisible(true); Debug.Log("[SpringExperiment][Button] Reset clicked; model restored to 200 g, 20 N/m"); Refresh(true); }
    private void OnDestroy() { weightMoveTween?.Kill(); }
}
