using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ExperimentUI : MonoBehaviour
{
    [Header("UI references")]
    [SerializeField] private TextMeshProUGUI readout;
    [SerializeField] private Button decreaseMassButton;
    [SerializeField] private Button increaseMassButton;
    [SerializeField] private Button decreaseStiffnessButton;
    [SerializeField] private Button increaseStiffnessButton;
    [SerializeField] private Button toggleForcesButton;
    [SerializeField] private Button toggleRulerButton;
    [SerializeField] private Button resetButton;

    private SpringExperiment experiment;

    public void Bind(SpringExperiment owner)
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("[ExperimentUI] Assign all UI references in the Inspector before binding.", this);
            return;
        }

        experiment = owner;
        decreaseMassButton.onClick.RemoveAllListeners();
        decreaseMassButton.onClick.AddListener(() => ChangeMass(-0.05f, "− Khối lượng"));
        increaseMassButton.onClick.RemoveAllListeners();
        increaseMassButton.onClick.AddListener(() => ChangeMass(0.05f, "+ Khối lượng"));
        decreaseStiffnessButton.onClick.RemoveAllListeners();
        decreaseStiffnessButton.onClick.AddListener(() => ChangeStiffness(-5f, "− Độ cứng"));
        increaseStiffnessButton.onClick.RemoveAllListeners();
        increaseStiffnessButton.onClick.AddListener(() => ChangeStiffness(5f, "+ Độ cứng"));
        toggleForcesButton.onClick.RemoveAllListeners();
        toggleForcesButton.onClick.AddListener(experiment.ToggleForces);
        toggleRulerButton.onClick.RemoveAllListeners();
        toggleRulerButton.onClick.AddListener(experiment.ToggleRuler);
        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(experiment.ResetExperiment);
    }

    public bool HasRequiredReferences()
    {
        return readout != null && decreaseMassButton != null && increaseMassButton != null &&
               decreaseStiffnessButton != null && increaseStiffnessButton != null &&
               toggleForcesButton != null && toggleRulerButton != null && resetButton != null;
    }

    public void UpdateReadout(SpringPhysics p) { if (readout == null) return; readout.text = $"Khối lượng:       {p.MassKg * 1000f:0} g\nĐộ cứng:          {p.Stiffness:0} N/m\n\nChiều dài tự nhiên: {p.NaturalLength * 100f:0.0} cm\nChiều dài hiện tại:  {p.CurrentLength * 100f:0.0} cm\nĐộ biến dạng:       {p.Extension * 100f:0.0} cm\n\nTrọng lực P:        {p.Weight:0.00} N\nLực đàn hồi Fđh:    {p.ElasticForce:0.00} N"; }
    private void ChangeMass(float delta, string buttonName) { Debug.Log($"[SpringExperiment][Button] {buttonName} clicked; delta={delta:0.00} kg"); experiment.ChangeMass(delta); }
    private void ChangeStiffness(float delta, string buttonName) { Debug.Log($"[SpringExperiment][Button] {buttonName} clicked; delta={delta:0.0} N/m"); experiment.ChangeStiffness(delta); }
}
