using UnityEngine;
using TMPro;

public class ArchimedesUI : MonoBehaviour
{
    [SerializeField] private ArchimedesSimulation simulation;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField volumeInput; // dm³
    [SerializeField] private TMP_InputField densityInput; // kg/m³
    [SerializeField] private TMP_InputField gravityInput; // m/s²
    [SerializeField] private TMP_InputField massInput; // kg

    [Header("Display Fields")]
    [SerializeField] private TextMeshProUGUI formulaText;
    [SerializeField] private TextMeshProUGUI buoyancyForceText;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private TextMeshProUGUI netForceText;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        // Set default values
        UpdateInputFields(2f, 1000f, 9.8f, 5f);

        // Add listeners for real-time updates
        if (volumeInput) volumeInput.onValueChanged.AddListener(_ => OnInputChanged());
        if (densityInput) densityInput.onValueChanged.AddListener(_ => OnInputChanged());
        if (gravityInput) gravityInput.onValueChanged.AddListener(_ => OnInputChanged());
        if (massInput) massInput.onValueChanged.AddListener(_ => OnInputChanged());
    }

    private void UpdateInputFields(float volume, float density, float gravity, float mass)
    {
        if (volumeInput) volumeInput.text = volume.ToString("F1");
        if (densityInput) densityInput.text = density.ToString("F0");
        if (gravityInput) gravityInput.text = gravity.ToString("F1");
        if (massInput) massInput.text = mass.ToString("F1");
    }

    private void OnInputChanged()
    {
        // Parse input values
        float volume = float.TryParse(volumeInput.text, out float v) ? v : 2f;
        float density = float.TryParse(densityInput.text, out float d) ? d : 1000f;
        float gravity = float.TryParse(gravityInput.text, out float g) ? g : 9.8f;
        float mass = float.TryParse(massInput.text, out float m) ? m : 5f;

        // Update simulation
        simulation.SetExperimentParameters(volume, density, gravity, mass);

        // Update display
        UpdateDisplay(volume, density, gravity, mass);
    }

    private void UpdateDisplay(float volumeDM3, float density, float gravity, float mass)
    {
        // Convert dm³ to m³
        float volumeM3 = volumeDM3 * 0.001f;

        // Calculate forces
        float buoyancyForce = density * gravity * volumeM3;
        float weight = mass * gravity;
        float netForce = buoyancyForce - weight;

        // Update formula display
        if (formulaText)
        {
            formulaText.text = $"<b>Công thức Archimedes:</b>\n" +
                             $"F<sub>A</sub> = ρ × g × V\n" +
                             $"F<sub>A</sub> = {density} × {gravity} × {volumeM3:F6}\n" +
                             $"F<sub>A</sub> = {buoyancyForce:F2} N";
        }

        // Update force displays
        if (buoyancyForceText)
            buoyancyForceText.text = $"<b>Lực đẩy Archimedes:</b>\n" +
                                   $"F<sub>A</sub> = {buoyancyForce:F2} N";

        if (weightText)
            weightText.text = $"<b>Trọng lực:</b>\n" +
                            $"F<sub>g</sub> = m × g = {mass} × {gravity}\n" +
                            $"F<sub>g</sub> = {weight:F2} N";

        if (netForceText)
            netForceText.text = $"<b>Lực ròng:</b>\n" +
                              $"F<sub>net</sub> = F<sub>A</sub> - F<sub>g</sub>\n" +
                              $"F<sub>net</sub> = {netForce:F2} N";

        // Update status
        if (statusText)
        {
            string status = "";
            if (netForce > 0.1f)
                status = "📈 Vật sẽ nổi lên";
            else if (netForce < -0.1f)
                status = "📉 Vật sẽ chìm xuống";
            else
                status = "⚖️ Vật cân bằng";

            statusText.text = status;
        }
    }

    public void RunExperiment()
    {
        OnInputChanged();
        Debug.Log("Bắt đầu mô phỏng Archimedes...");
    }
}
