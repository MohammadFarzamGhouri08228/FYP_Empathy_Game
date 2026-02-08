using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//--------------------------------------------------------------------
// CCEdgeCastInfo stores edge detection information for a
// ControlledCapsuleCollider. Kept by the CCState class.
// Updated whenever the ControlledCapsuleCollider is moved.
//--------------------------------------------------------------------
public class CCEdgeCastInfo : CEdgeCastInfo
{
    ControlledCapsuleCollider m_CapsuleCollider;

    Vector3 m_UpDirection;
    Vector3 m_ProposedHeadPoint;
    Vector3 m_WallNormal;
    Vector3 m_EdgeNormal;
    Vector3 m_EdgePoint;
    Transform m_EdgeTransform;

    public void Init(ControlledCapsuleCollider a_CapsuleCollider)
    {
        m_CapsuleCollider = a_CapsuleCollider;
    }

    // Uses side-cast and grounded information to detect an edge
    // (a point where the wall meets a ledge the character could grab).
    public void UpdateWithCollisions()
    {
        m_HasHitEdge = false;

        if (m_CapsuleCollider == null)
            return;

        CSideCastInfo sideInfo = m_CapsuleCollider.GetSideCastInfo();
        if (sideInfo == null || !sideInfo.m_HasHitSide)
            return;

        Vector2 wallNormal = sideInfo.GetSideNormal();
        Vector3 wallPoint = sideInfo.GetSidePoint();

        // Cast upward along the wall to find the top edge
        float radius = m_CapsuleCollider.GetRadius();
        Vector3 castOrigin = wallPoint - (Vector3)(wallNormal * 0.01f);
        castOrigin += m_CapsuleCollider.GetUpDirection() * radius;

        RaycastHit2D upHit = Physics2D.Raycast(
            castOrigin,
            m_CapsuleCollider.GetUpDirection(),
            m_CapsuleCollider.GetLength(),
            m_CapsuleCollider.GetLayerMask()
        );

        if (upHit.collider == null)
        {
            // No ceiling above; cast sideways from above to find the edge
            Vector3 aboveOrigin = castOrigin + m_CapsuleCollider.GetUpDirection() * m_CapsuleCollider.GetLength();
            RaycastHit2D edgeHit = Physics2D.Raycast(
                aboveOrigin,
                -wallNormal,
                radius * 2.0f,
                m_CapsuleCollider.GetLayerMask()
            );

            if (edgeHit.collider != null)
            {
                m_HasHitEdge = true;
                m_UpDirection = m_CapsuleCollider.GetUpDirection();
                m_WallNormal = wallNormal;
                m_EdgeNormal = edgeHit.normal;
                m_EdgePoint = edgeHit.point;
                m_EdgeTransform = edgeHit.transform;
                m_ProposedHeadPoint = new Vector3(m_EdgePoint.x, m_EdgePoint.y, 0) + (Vector3)(wallNormal * radius);
            }
        }
    }

    public override Vector3 GetUpDirection()
    {
        return m_UpDirection;
    }

    public override Vector3 GetProposedHeadPoint()
    {
        return m_ProposedHeadPoint;
    }

    public override Vector3 GetWallNormal()
    {
        return m_WallNormal;
    }

    public override Vector3 GetEdgeNormal()
    {
        return m_EdgeNormal;
    }

    public override Vector3 GetEdgePoint()
    {
        return m_EdgePoint;
    }

    public override Transform GetEdgeTransform()
    {
        return m_EdgeTransform;
    }
}
