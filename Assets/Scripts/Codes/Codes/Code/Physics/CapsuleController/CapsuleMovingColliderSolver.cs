using UnityEngine;
using System.Collections.Generic;
//--------------------------------------------------------------------
// Handles interaction with moving colliders (e.g. moving platforms).
// Tracks contact points so the capsule can move along with colliders
// that are in motion.
//--------------------------------------------------------------------
public class CapsuleMovingColliderSolver : MonoBehaviour
{
    struct ColPoint
    {
        public Transform parent;
        public Vector3 point;
        public Vector3 normal;
        public Vector3 previousPosition;
    }

    List<ColPoint> m_ColPoints = new List<ColPoint>();

    public void AddColPoint(Transform a_Parent, Vector3 a_Point, Vector3 a_Normal)
    {
        ColPoint cp = new ColPoint();
        cp.parent = a_Parent;
        cp.point = a_Point;
        cp.normal = a_Normal;
        if (a_Parent != null)
        {
            cp.previousPosition = a_Parent.position;
        }
        m_ColPoints.Add(cp);
    }

    public void ClearColPoints()
    {
        m_ColPoints.Clear();
    }

    public Vector3 GetMovingColliderVelocity()
    {
        Vector3 totalVelocity = Vector3.zero;
        for (int i = 0; i < m_ColPoints.Count; i++)
        {
            if (m_ColPoints[i].parent != null)
            {
                Vector3 delta = m_ColPoints[i].parent.position - m_ColPoints[i].previousPosition;
                totalVelocity += delta;
            }
        }
        return totalVelocity;
    }
}
