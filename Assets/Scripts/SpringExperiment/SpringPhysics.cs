using System;
using UnityEngine;

/// Pure spring experiment model. It has no scene or UI dependencies.
[Serializable]
public sealed class SpringPhysics
{
    public const float Gravity = 9.81f;
    public float NaturalLength { get; }
    public float MassKg { get; private set; }
    public float Stiffness { get; private set; }
    public float Weight => MassKg * Gravity;
    public float Extension => Weight / Stiffness;
    public float CurrentLength => NaturalLength + Extension;
    public float ElasticForce => Stiffness * Extension;
    public bool IsBalanced => Mathf.Abs(ElasticForce - Weight) < 0.0001f;

    public SpringPhysics(float naturalLength = 0.20f, float massKg = 0.20f, float stiffness = 20f)
    {
        NaturalLength = Mathf.Max(0.01f, naturalLength);
        SetMass(massKg); SetStiffness(stiffness);
    }

    public void SetMass(float kilograms) => MassKg = Mathf.Clamp(kilograms, 0.05f, 0.50f);
    public void SetStiffness(float newtonPerMetre) => Stiffness = Mathf.Clamp(newtonPerMetre, 5f, 80f);
}
