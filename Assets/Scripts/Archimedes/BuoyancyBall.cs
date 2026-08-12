using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArchimedesOscillation : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField densityInput;
    [SerializeField] private TMP_InputField gravityInput;
    [SerializeField] private TMP_InputField volumeInput;
    [SerializeField] private TMP_InputField massInput;

    [SerializeField] private TMP_Text resultText;

    [Header("Physics")]
    [SerializeField] private float damping = 8f;

    [SerializeField] private float waterHeight = 0f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    private void Simulate()
    {
        if (!float.TryParse(densityInput.text, out float density))
            return;

        if (!float.TryParse(gravityInput.text, out float gravity))
            return;

        if (!float.TryParse(volumeInput.text, out float volumeDm3))
            return;

        if (!float.TryParse(massInput.text, out float mass))
            return;

        // dm³ -> m³
        float volume = volumeDm3 * 0.001f;

        // Forces
        float buoyancyForce =
            density *
            gravity *
            volume;

        float gravityForce =
            mass *
            gravity;

        // Net force
        float netForce =
            buoyancyForce -
            gravityForce;

        // Damping
        float dampingForce =
            -rb.linearVelocity.y *
            damping;

        // Final force
        float finalForce =
            netForce +
            dampingForce;

        rb.mass = mass;

        rb.AddForce(
            Vector3.up * finalForce,
            ForceMode.Force
        );

        // UI
        resultText.text =
            $"FA = {buoyancyForce:F2} N\n" +
            $"Fg = {gravityForce:F2} N\n\n";

        if (buoyancyForce > gravityForce)
        {
            resultText.text +=
                "Kết quả: Vật nổi";
        }
        else if (buoyancyForce < gravityForce)
        {
            resultText.text +=
                "Kết quả: Vật chìm";
        }
        else
        {
            resultText.text +=
                "Kết quả: Vật cân bằng";
        }
    }
}