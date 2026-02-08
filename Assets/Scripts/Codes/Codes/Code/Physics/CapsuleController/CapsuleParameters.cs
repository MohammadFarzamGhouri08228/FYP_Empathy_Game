using UnityEngine;
using System.Collections;
//--------------------------------------------------------------------
// Partial class definition for ControlledCapsuleCollider.
// Contains serialized parameters for the capsule shape, movement,
// collision detection margins, and runtime state fields.
//--------------------------------------------------------------------
public partial class ControlledCapsuleCollider : ControlledCollider
{
    // --- Shape ---
    [SerializeField] protected float m_Radius = 0.25f;
    [SerializeField] protected float m_Length = 0.5f;
    protected float m_PrevLength;
    [SerializeField] protected float m_DefaultLength = 0.5f;

    // --- Layer ---
    [SerializeField] protected LayerMask m_LayerMask;

    // --- Runtime velocity ---
    protected Vector2 m_Velocity;
    protected Vector2 m_PrevVelocity;

    // --- Collision toggle ---
    [SerializeField] protected bool m_CollisionsActive = true;

    // --- Movement margins ---
    [SerializeField] protected float m_MovementCapsuleCastMargin = 0.001f;
    [SerializeField] protected float m_MinimumViableVelocity = 0.001f;
    [SerializeField] protected float m_OptionalMoveCastMargin = 0.02f;

    // --- Grounded detection ---
    [SerializeField] protected float m_GroundedMargin = 0.02f;
    [SerializeField] protected float m_GroundedCheckDistance = 0.04f;
    [SerializeField] protected float m_MaxGroundedAngle = 60.0f;

    // --- Side / wall detection ---
    [SerializeField] protected float m_SideCastMargin = 0.02f;
    [SerializeField] protected float m_SideCastDistance = 0.04f;
    [SerializeField] protected float m_MaxWallAngle = 100.0f;
    [SerializeField] protected float m_WallCastMargin = 0.02f;
    [SerializeField] protected float m_WallCastDistance = 0.04f;

    // --- Rotate / resize margins ---
    [SerializeField] protected float m_RotateCastMargin = 0.02f;
    [SerializeField] protected float m_ResizeCastMargin = 0.02f;

    // --- Moving collider solver ---
    [SerializeField] protected CapsuleMovingColliderSolver m_CapsuleMovingColliderSolver;

    // ==============================
    //  Getters
    // ==============================
    public float GetRadius()                    { return m_Radius; }
    public float GetDefaultLength()             { return m_DefaultLength; }
    public LayerMask GetLayerMask()             { return m_LayerMask; }
    public Vector2 GetVelocity()                { return m_Velocity; }
    public Vector2 GetPreviousVelocity()        { return m_PrevVelocity; }
    public float GetMaxGroundedAngle()          { return m_MaxGroundedAngle; }
    public float GetMaxWallAngle()              { return m_MaxWallAngle; }
    public float GetGroundedMargin()            { return m_GroundedMargin; }
    public float GetGroundedCheckDistance()      { return m_GroundedCheckDistance; }
    public float GetSideCastMargin()            { return m_SideCastMargin; }
    public float GetSideCastDistance()           { return m_SideCastDistance; }
    public float GetWallCastMargin()            { return m_WallCastMargin; }
    public float GetWallCastDistance()           { return m_WallCastDistance; }
    public float GetMovementCapsuleCastMargin() { return m_MovementCapsuleCastMargin; }
    public float GetOptionalMoveCastMargin()    { return m_OptionalMoveCastMargin; }
    public float GetRotateCastMargin()          { return m_RotateCastMargin; }
    public float GetResizeCastMargin()          { return m_ResizeCastMargin; }
    public bool  GetCollisionsActive()          { return m_CollisionsActive; }

    public void SetCollisionsActive(bool a_Active) { m_CollisionsActive = a_Active; }
}
