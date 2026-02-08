using UnityEngine;
//--------------------------------------------------------------------
// Stores information about a single collision that occurred during
// a capsule movement update. Kept by ControlledCapsuleCollider.
//--------------------------------------------------------------------
public class CapsuleCollisionOccurrance
{
    public Vector3   m_Point;
    public Vector3   m_Normal;
    public Transform m_Transform;
    public Vector3   m_IncomingVelocity;
    public Vector2   m_IncomingVelocityPure;
    public Vector3   m_OutgoingVelocity;
    public Vector2   m_OutgoingVelocityPure;
    public Vector3   m_VelocityLoss;
    public Vector2   m_VelocityLossPure;
}
