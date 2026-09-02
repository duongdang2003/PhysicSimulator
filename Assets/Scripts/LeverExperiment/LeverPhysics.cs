using System;
using UnityEngine;

public enum LeverState
{
    Balanced,
    TiltingLeft,
    TiltingRight
}

[Serializable]
public readonly struct LeverSnapshot
{
    public readonly float leftMassKg, rightMassKg, leftDistance, rightDistance;
    public readonly float leftForce, rightForce, leftTorque, rightTorque;
    public readonly LeverState state;

    public LeverSnapshot(float leftMassKg, float rightMassKg, float leftDistance, float rightDistance, float leftForce, float rightForce, float leftTorque, float rightTorque, LeverState state)
    {
        this.leftMassKg = leftMassKg; this.rightMassKg = rightMassKg;
        this.leftDistance = leftDistance; this.rightDistance = rightDistance;
        this.leftForce = leftForce; this.rightForce = rightForce;
        this.leftTorque = leftTorque; this.rightTorque = rightTorque; this.state = state;
    }
}

public sealed class LeverPhysics
{
    public const float Gravity = 9.81f;
    public const float MinimumDistance = 0.2f;
    public const float MaximumDistance = 1.3f;
    public float LeftMassKg { get; private set; }
    public float RightMassKg { get; private set; }
    public float LeftDistance { get; private set; }
    public float RightDistance { get; private set; }

    public LeverPhysics(float leftMassKg, float rightMassKg, float leftDistance, float rightDistance)
    {
        SetMasses(leftMassKg, rightMassKg);
        SetDistances(leftDistance, rightDistance);
    }

    public void SetMasses(float left, float right)
    {
        LeftMassKg = Mathf.Clamp(left, 0.05f, 1f);
        RightMassKg = Mathf.Clamp(right, 0.05f, 1f);
    }

    public void SetDistances(float left, float right)
    {
        LeftDistance = Mathf.Clamp(left, MinimumDistance, MaximumDistance);
        RightDistance = Mathf.Clamp(right, MinimumDistance, MaximumDistance);
    }

    public static float CalculateTorque(float force, float distance) => force * distance;

    public LeverSnapshot GetSnapshot()
    {
        float leftForce = LeftMassKg * Gravity;
        float rightForce = RightMassKg * Gravity;
        float leftTorque = CalculateTorque(leftForce, LeftDistance);
        float rightTorque = CalculateTorque(rightForce, RightDistance);
        float difference = leftTorque - rightTorque;
        LeverState state = Mathf.Abs(difference) < 0.01f ? LeverState.Balanced : difference > 0 ? LeverState.TiltingLeft : LeverState.TiltingRight;
        return new LeverSnapshot(LeftMassKg, RightMassKg, LeftDistance, RightDistance, leftForce, rightForce, leftTorque, rightTorque, state);
    }
}
