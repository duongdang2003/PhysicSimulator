using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LeverUI : MonoBehaviour
{
    [SerializeField] private TMP_Text readoutText;
    [SerializeField] private Slider leftMassSlider;
    [SerializeField] private Slider rightMassSlider;
    [SerializeField] private Slider leftDistanceSlider;
    [SerializeField] private Slider rightDistanceSlider;
    [SerializeField] private Button toggleForcesButton;
    [SerializeField] private Button toggleRulerButton;
    [SerializeField] private Button resetButton;

    private LeverExperiment experiment;

    public void Bind(LeverExperiment owner)
    {
        experiment = owner;
        if (leftMassSlider != null) leftMassSlider.onValueChanged.AddListener(experiment.SetLeftMass);
        if (rightMassSlider != null) rightMassSlider.onValueChanged.AddListener(experiment.SetRightMass);
        if (leftDistanceSlider != null) leftDistanceSlider.onValueChanged.AddListener(experiment.SetLeftDistance);
        if (rightDistanceSlider != null) rightDistanceSlider.onValueChanged.AddListener(experiment.SetRightDistance);
        if (toggleForcesButton != null) toggleForcesButton.onClick.AddListener(experiment.ToggleForces);
        if (toggleRulerButton != null) toggleRulerButton.onClick.AddListener(experiment.ToggleRuler);
        if (resetButton != null) resetButton.onClick.AddListener(experiment.ResetExperiment);
    }

    public void Refresh(LeverSnapshot s)
    {
        if (leftMassSlider != null) leftMassSlider.SetValueWithoutNotify(s.leftMassKg);
        if (rightMassSlider != null) rightMassSlider.SetValueWithoutNotify(s.rightMassKg);
        if (leftDistanceSlider != null) leftDistanceSlider.SetValueWithoutNotify(s.leftDistance);
        if (rightDistanceSlider != null) rightDistanceSlider.SetValueWithoutNotify(s.rightDistance);
        if (readoutText == null) return;
        string state = s.state == LeverState.Balanced ? "CÂN BẰNG" : s.state == LeverState.TiltingLeft ? "NGHIÊNG SANG TRÁI" : "NGHIÊNG SANG PHẢI";
        readoutText.text = $"Khối lượng trái: {s.leftMassKg * 1000f:0} g\nKhối lượng phải: {s.rightMassKg * 1000f:0} g\n\nKhoảng cách d1: {s.leftDistance * 100f:0} cm\nKhoảng cách d2: {s.rightDistance * 100f:0} cm\n\nLực F1: {s.leftForce:0.00} N\nLực F2: {s.rightForce:0.00} N\nMômen M1: {s.leftTorque:0.00} Nm\nMômen M2: {s.rightTorque:0.00} Nm\n\nTrạng thái: {state}";
    }

    private void OnDestroy()
    {
        if (leftMassSlider != null) leftMassSlider.onValueChanged.RemoveAllListeners();
        if (rightMassSlider != null) rightMassSlider.onValueChanged.RemoveAllListeners();
        if (leftDistanceSlider != null) leftDistanceSlider.onValueChanged.RemoveAllListeners();
        if (rightDistanceSlider != null) rightDistanceSlider.onValueChanged.RemoveAllListeners();
        if (toggleForcesButton != null) toggleForcesButton.onClick.RemoveAllListeners();
        if (toggleRulerButton != null) toggleRulerButton.onClick.RemoveAllListeners();
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
    }
}
