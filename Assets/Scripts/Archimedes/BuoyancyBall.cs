using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArchimedesSimulation : MonoBehaviour
{
    [Header("Archimedes Variables")]
    [SerializeField] private float liquidDensity = 1000f; // kg/m³
    [SerializeField] private float gravity = 9.8f; // m/s²
    [SerializeField] private float volumeDM3 = 2f; // dm³ (input from user)

    [Header("Object Properties")]
    [SerializeField] private float objectMass = 2f; // kg
    [SerializeField] private float objectDensity = 500f; // kg/m³

    [Header("Water")]
    [SerializeField] private float waterHeight = 0f;
    [SerializeField] private bool isFullySubmerged = true;

    private Rigidbody rb;
    private float submergedVolume; // m³
    private float buoyancyForce; // N
    private float weight; // N
    private float netForce; // N

    public float BuoyancyForce => buoyancyForce;
    public float Weight => weight;
    public float NetForce => netForce;
    public float SubmergedVolume => submergedVolume;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = objectMass;
        Physics.gravity = new Vector3(0, -gravity, 0);
    }

    private void Start()
    {
        // Sync Rigidbody mass with current objectMass value
        if (rb != null)
            rb.mass = objectMass;
    }

    private void FixedUpdate()
    {
        // Check if object is underwater
        bool isUnderWater = transform.position.y < waterHeight;

        if (!isUnderWater)
        {
            rb.linearDamping = 0.1f;
            return;
        }

        // Convert volume from dm³ to m³ (1 dm³ = 0.001 m³)
        submergedVolume = volumeDM3 * 0.001f;

        // F = ρ × g × V (Archimedes' Principle)
        buoyancyForce = liquidDensity * gravity * submergedVolume;
        weight = objectMass * gravity;
        netForce = buoyancyForce - weight;

        // Apply ONLY buoyancy force (weight is already handled by Physics.gravity)
        rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Force);

        // Damping to prevent oscillation
        rb.linearDamping = 2f;
    }

    // Set experiment parameters from UI
    public void SetExperimentParameters(float volumeInDM3, float densityKgM3, float gravityMsSquared, float mass)
    {
        volumeDM3 = volumeInDM3;
        liquidDensity = densityKgM3;
        gravity = gravityMsSquared;
        objectMass = mass;
        rb.mass = objectMass;
        Physics.gravity = new Vector3(0, -gravity, 0);
    }

    // Get calculation results for display
    public void GetResults(out float f_buoyancy, out float f_weight, out float f_net, out float subVolume)
    {
        subVolume = submergedVolume;
        f_buoyancy = buoyancyForce;
        f_weight = weight;
        f_net = netForce;
    }
}