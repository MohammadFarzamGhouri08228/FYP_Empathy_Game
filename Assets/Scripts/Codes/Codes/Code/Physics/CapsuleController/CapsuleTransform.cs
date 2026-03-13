using UnityEngine;
using System.Collections;
//--------------------------------------------------------------------
//Class which stores transform information for the ControlledCapsuleCollider class.
//This allows the transform to be treated as a separate object from the complete ControlledCapsuleCollider
//The advantage of this is that other classes can manipulate the transform without altering the ControlledCapsuleCollider, (via CapsuleManipulator) to see if what they want to do is valid for collision
//Can be copied for this purpose.
//--------------------------------------------------------------------
public class CapsuleTransform {
    public ControlledCapsuleCollider m_CapsuleCollider;
    Vector3 m_Position;
    Vector3 m_UpDirection;
    float m_Length;
    CapsuleResizeMethod m_LastResizeMethod;
    bool m_IsCopy;

    public CapsuleTransform()
    {
    }
    
    public CapsuleTransform CreateCopy()
    {
        CapsuleTransform copy = new CapsuleTransform();
        copy.OverrideValues(this);
        copy.m_IsCopy = true;
        return copy;
    }

    public void OverrideValues(CapsuleTransform a_ToCopyFrom)
    {
        m_CapsuleCollider = a_ToCopyFrom.m_CapsuleCollider;
        m_Position = a_ToCopyFrom.m_Position;
        m_UpDirection = a_ToCopyFrom.m_UpDirection;
        m_Length = a_ToCopyFrom.m_Length;
        m_LastResizeMethod = a_ToCopyFrom.m_LastResizeMethod;

        if (!m_IsCopy && m_CapsuleCollider != null)
        {
            m_CapsuleCollider.transform.position = m_Position;
            m_CapsuleCollider.transform.rotation = GetRotation();
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    //Get various properties of a capsuletransform
    //--------------------------------------------------------------------------------------------------------------------------------
    public Vector3 GetPosition()
    {
        return m_Position;
    }

    public Vector3 GetUpCenter(bool a_UseOriginalLength = false)
    {
        if (a_UseOriginalLength && m_Length != m_CapsuleCollider.GetDefaultLength())
        {
            switch (m_LastResizeMethod)
            {
                case CapsuleResizeMethod.FromBottom:
                    if (m_CapsuleCollider.CanBeResized(m_CapsuleCollider.GetDefaultLength(), CapsuleResizeMethod.FromBottom))
                    {
                        return (GetDownCenter() + m_UpDirection * m_CapsuleCollider.GetDefaultLength());
                    }
                    break;
                case CapsuleResizeMethod.FromCenter:
                    return m_Position + m_UpDirection * m_CapsuleCollider.GetDefaultLength() * 0.5f;
            }
        }
        return m_Position + m_UpDirection * m_Length * 0.5f;
    }

    public Vector3 GetDownCenter(bool a_UseOriginalLength = false)
    {
        if (a_UseOriginalLength && m_Length != m_CapsuleCollider.GetDefaultLength())
        {
            switch (m_LastResizeMethod)
            {
                case CapsuleResizeMethod.FromTop:
                    if (m_CapsuleCollider.CanBeResized(m_CapsuleCollider.GetDefaultLength(), CapsuleResizeMethod.FromTop))
                    {
                        return (GetUpCenter() - m_UpDirection * m_CapsuleCollider.GetDefaultLength());
                    }
                    break;
                case CapsuleResizeMethod.FromCenter:
                    return m_Position - m_UpDirection * m_CapsuleCollider.GetDefaultLength() * 0.5f;
            }
        }
        return m_Position - m_UpDirection * m_Length * 0.5f;
    }

    public Vector3 GetUpDirection()
    {
        return m_UpDirection;
    }

    public Vector3 GetRightDirection()
    {
        return Vector3.Cross(m_UpDirection, Vector3.forward);
    }

    public Quaternion GetRotation()
    {
        return Quaternion.LookRotation(Vector3.forward, m_UpDirection);
    }

    public float GetLength()
    {
        return m_Length;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    //Set the various properties of a capsuletransform
    //--------------------------------------------------------------------------------------------------------------------------------
    public void SetPosition(Vector3 a_Position)
    {
        m_Position = a_Position;
        if (!m_IsCopy && m_CapsuleCollider != null)
        {
            m_CapsuleCollider.transform.position = m_Position;
        }
    }

    public void Move(Vector3 a_Movement)
    {
        m_Position += a_Movement;
        if (!m_IsCopy && m_CapsuleCollider != null)
        {
            m_CapsuleCollider.transform.position = m_Position;
        }
    }

    public void SetUpCenter(Vector3 a_Position)
    {
        Vector3 difference = a_Position - GetUpCenter();
        m_Position += difference;
        if (!m_IsCopy && m_CapsuleCollider != null)
        {
            m_CapsuleCollider.transform.position = m_Position;
            m_CapsuleCollider.transform.rotation = GetRotation();
        }
    }

    public void SetDownCenter(Vector3 a_Position)
    {
        Vector3 difference = a_Position - GetDownCenter();
        m_Position += difference;
        if (!m_IsCopy && m_CapsuleCollider != null)
        {
            m_CapsuleCollider.transform.position = m_Position;
            m_CapsuleCollider.transform.rotation = GetRotation();
        }
    }

    public void Rotate(Vector3 a_NewUpDirection, RotateMethod a_Method)
    {
        Vector3 pivot = Vector3.zero;
        switch (a_Method)
        {
            case RotateMethod.FromCenter:
                m_UpDirection = a_NewUpDirection;
                return;
            case RotateMethod.FromBottom:
                pivot = GetDownCenter();
                break;
            case RotateMethod.FromTop:
                pivot = GetUpCenter();
                break;
        }
        Vector3 vectorToCenter = m_Position - pivot;

        Quaternion rotation = Quaternion.FromToRotation(GetUpDirection(), a_NewUpDirection);

        Vector3 targetPosition = pivot + rotation * vectorToCenter;

        m_Position = targetPosition;
        m_UpDirection = a_NewUpDirection;
        if (!m_IsCopy && m_CapsuleCollider != null)
        {
            m_CapsuleCollider.transform.position = m_Position;
            m_CapsuleCollider.transform.rotation = GetRotation();
        }
    }

    public void SetUpDirection(Vector3 a_NewUpDirection)
    {
        m_UpDirection = a_NewUpDirection;
        if (!m_IsCopy && m_CapsuleCollider != null)
        {
            m_CapsuleCollider.transform.rotation = GetRotation();
        }
    }

    public void SetLength(float a_Length, CapsuleResizeMethod a_Method = CapsuleResizeMethod.FromCenter, CapsuleResizeMethod a_RestoreMethod = CapsuleResizeMethod.None)
    {
        switch (a_Method)
        {
            case CapsuleResizeMethod.FromBottom:
                m_Position = GetDownCenter() + GetUpDirection() * a_Length * 0.5f;
                break;
            case CapsuleResizeMethod.FromTop:
                m_Position = GetUpCenter() - GetUpDirection() * a_Length * 0.5f;
                break;
        }
        m_Length = a_Length;
        m_LastResizeMethod = a_Method;
        if (a_RestoreMethod != CapsuleResizeMethod.None)
        {
            m_LastResizeMethod = a_RestoreMethod;
        }
        if (!m_IsCopy && m_CapsuleCollider != null)
        {
            m_CapsuleCollider.transform.position = m_Position;
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    //Can a capsuletransform be manipulated (moved, rotated, resized) without intersecting colliders
    //--------------------------------------------------------------------------------------------------------------------------------
    public bool CanExistHere()
    {
        float radius = m_CapsuleCollider.GetRadius() - m_CapsuleCollider.GetRotateCastMargin();
        Vector2 center = (GetDownCenter() + GetUpCenter()) * 0.5f;
        float height = Vector3.Distance(GetDownCenter(), GetUpCenter()) + 2 * radius;
        Vector2 size = new Vector2(radius * 2, height);
        float angle = Vector2.SignedAngle(Vector2.up, m_UpDirection);
        return (Physics2D.OverlapCapsule(center, size, CapsuleDirection2D.Vertical, angle, m_CapsuleCollider.GetLayerMask()) == null);
    }

    public bool CanMove(Vector3 a_Movement, bool a_UseMargin = false)
    {
        float distanceToTravel = a_Movement.magnitude + m_CapsuleCollider.GetMovementCapsuleCastMargin();
        Vector3 direction = a_Movement.normalized;
        Vector3 margin = -direction * m_CapsuleCollider.GetMovementCapsuleCastMargin();
        float radius = m_CapsuleCollider.GetRadius();
        if (a_UseMargin)
        {
            radius -= m_CapsuleCollider.GetOptionalMoveCastMargin();
        }
        return !CCState.CorrectCapsuleCast(GetDownCenter() + margin, GetUpCenter() + margin, radius, direction, distanceToTravel, m_CapsuleCollider.GetLayerMask());
    }

    public bool CanRotate(Vector3 a_NewUpDirection, RotateMethod a_Method)
    {
        Vector3 pivot = Vector3.zero;
        switch (a_Method)
        {
            case RotateMethod.FromCenter:
                pivot = m_Position;
                break;
            case RotateMethod.FromBottom:
                pivot = GetDownCenter();
                break;
            case RotateMethod.FromTop:
                pivot = GetUpCenter();
                break;
        }
        Vector3 vectorToCenter = m_Position - pivot;
        Vector3 currentNormal = GetUpDirection();

        Quaternion rotation = Quaternion.FromToRotation(currentNormal, a_NewUpDirection);

        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;
        switch (a_Method)
        {
            case RotateMethod.FromCenter:
                start = m_Position - (rotation * vectorToCenter).normalized * m_Length;
                end = m_Position + (rotation * vectorToCenter).normalized * m_Length;
                break;
            case RotateMethod.FromBottom:
                start = GetDownCenter();
                end = GetDownCenter() + (rotation * vectorToCenter).normalized * m_Length;
                break;
            case RotateMethod.FromTop:
                start = GetUpCenter() + (rotation * vectorToCenter).normalized * m_Length;
                end = GetUpCenter();
                break;
        }

        float radius = m_CapsuleCollider.GetRadius() - m_CapsuleCollider.GetRotateCastMargin();
        Vector2 center = (start + end) * 0.5f;
        float height = Vector3.Distance(start, end) + 2 * radius;
        Vector2 size = new Vector2(radius * 2, height);
        float angle = Vector2.SignedAngle(Vector2.up, a_NewUpDirection);

        return (Physics2D.OverlapCapsule(center, size, CapsuleDirection2D.Vertical, angle, m_CapsuleCollider.GetLayerMask()) == null);
    }

    public bool CanBeResized(float a_Length, CapsuleResizeMethod a_Method)
    {
        Vector3 newPosition = m_Position;
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;
        float radius = m_CapsuleCollider.GetRadius(); // - resize cast margin?
        // Original code used m_Radius for first case, and m_Radius - margin for others?
        // Original: FromCenter uses m_Radius. FromBottom/Top use m_Radius - GetResizeCastMargin().
        
        switch (a_Method)
        {
            case CapsuleResizeMethod.FromCenter:
                start = newPosition - m_CapsuleCollider.GetUpDirection() * a_Length * 0.5f;
                end = newPosition + m_CapsuleCollider.GetUpDirection() * a_Length * 0.5f;
                break;
            case CapsuleResizeMethod.FromBottom:
                radius -= m_CapsuleCollider.GetResizeCastMargin();
                start = m_CapsuleCollider.GetDownCenter();
                end = m_CapsuleCollider.GetDownCenter() + m_CapsuleCollider.GetUpDirection() * a_Length;
                break;
            case CapsuleResizeMethod.FromTop:
                radius -= m_CapsuleCollider.GetResizeCastMargin();
                start = m_CapsuleCollider.GetUpCenter();
                end = m_CapsuleCollider.GetUpCenter() - m_CapsuleCollider.GetUpDirection() * a_Length;
                break;
        }
        
        Vector2 center = (start + end) * 0.5f;
        float height = Vector3.Distance(start, end) + 2 * radius;
        Vector2 size = new Vector2(radius * 2, height);
        float angle = Vector2.SignedAngle(Vector2.up, m_UpDirection);

        return (Physics2D.OverlapCapsule(center, size, CapsuleDirection2D.Vertical, angle, m_CapsuleCollider.GetLayerMask()) == null);
    }

    public bool IsBeingSquished(float a_SquishRadius)
    {
        float radius = a_SquishRadius;
        Vector2 center = (GetDownCenter() + GetUpCenter()) * 0.5f;
        float height = Vector3.Distance(GetDownCenter(), GetUpCenter()) + 2 * radius;
        Vector2 size = new Vector2(radius * 2, height);
        float angle = Vector2.SignedAngle(Vector2.up, m_UpDirection);
        return (Physics2D.OverlapCapsule(center, size, CapsuleDirection2D.Vertical, angle, m_CapsuleCollider.GetLayerMask()) != null);
    }
}
