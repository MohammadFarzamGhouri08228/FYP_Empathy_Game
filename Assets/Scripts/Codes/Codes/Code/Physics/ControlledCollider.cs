using UnityEngine;
using System.Collections;
//--------------------------------------------------------------------
// Abstract base class for controlled colliders.
// Derived classes (e.g. ControlledCapsuleCollider) implement the
// specifics for their particular shape.
//--------------------------------------------------------------------
public abstract class ControlledCollider : MonoBehaviour
{
    public abstract void UpdateWithVelocity(Vector2 a_Velocity);
    public abstract void SetPosition(Vector3 a_Position);
    public abstract void SetRotation(Quaternion a_Rotation);
    public abstract void UpdateContextInfo();

    // Grounded
    public abstract bool IsGrounded();
    public abstract CGroundedInfo GetGroundedInfo();

    // Side / Wall
    public abstract bool IsCompletelyTouchingWall();
    public abstract bool IsPartiallyTouchingWall();
    public abstract CSideCastInfo GetSideCastInfo();

    // Edge
    public abstract bool IsTouchingEdge();
    public abstract CEdgeCastInfo GetEdgeCastInfo();

    // Rotation
    public abstract bool CanAlignWithNormal(Vector3 a_Normal, RotateMethod a_Method = RotateMethod.FromBottom);
    public abstract void RotateToAlignWithNormal(Vector3 a_Normal, RotateMethod a_Method = RotateMethod.FromBottom);

    // Moving collider helpers
    public abstract void AddColPoint(Transform a_Parent, Vector3 a_Point, Vector3 a_Normal);
    public abstract void ClearColPoints();
}
