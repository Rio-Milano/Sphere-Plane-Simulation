using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : BaseEntity
{
    public Vector3 vA, vB, vC, vD;

    public Vector3 GetPlaneNormal()
    {
        Vector3 AB = vB - vA;
        Vector3 AC = vC - vA;
        Vector3 Normal = Vector3.Cross(AB, AC).normalized;
        return Normal;
    }

    Vector3 GetPlaneAB()
    {
        Vector3 AB = vB - vA;
        return AB;
    }

    public void InitPlane(Color planeColor, Vector3 A, Vector3 B, Vector3 C, Vector3 D_forGraphicsOnly)
    {
        this.m_entityData.m_color = planeColor;
        this.m_entityData.m_collisionType = EntityData.CollisionType.Plane;
        /*
         
        B
        |
        |
        |
        A-------C


        */

        this.vA = A;
        this.vB = B;
        this.vC = C;
        this.vD = D_forGraphicsOnly;
    }

    override protected bool IsCollidingWithSphere(Sphere sphere, float dt)
    {
        Vector3 planeSurfaceNormal = GetPlaneNormal();
        Vector3 sphereVelocity = sphere.m_velocity * Time.deltaTime;

        float angleBetweenSurfaceNormalAndInverseVelocity = Vector3.Angle(planeSurfaceNormal, -sphereVelocity.normalized);

        //if the angle is less than 90deg then the sphere is moving towards the plane
        if(angleBetweenSurfaceNormalAndInverseVelocity < 90.0f)
        {
            //an arbitary point on the plane
            Vector3 k = vA;
            //vector k->SpherePos
            Vector3 P = sphere.transform.position - k;
            //angle between surface normal and P
            float q1 = Vector3.Angle(P, planeSurfaceNormal);
            //angle between P and the plane
            float q2 = (90.0f - q1) * Mathf.Deg2Rad;
            //q1 + q2 = 90deg

            //find d, the closest distance between start position of sphere and plane
            float d = Mathf.Sin(q2) * P.magnitude;

            //find s, angle between sphere velocity and inverse plane normal
            float s = Vector3.Angle(sphereVelocity.normalized, -planeSurfaceNormal) * Mathf.Deg2Rad;

            //radius of sphere
            float r = sphere.m_radius;
            //vc distance from current sphere to collision point
            float vcMag = (d - r) / Mathf.Cos(s);
            //if this value is <= thelength of V then this value will give the collision point
            if(vcMag <= (sphereVelocity.magnitude*dt))
            {
                if (m_entityData.m_hasCollided && Vector3.Distance(transform.position, sphere.transform.position + (sphereVelocity.normalized * vcMag)) > Vector3.Distance(transform.position, m_entityData.m_collidePoint))
                    return false;

                sphere.m_entityData.m_collideWith = this;
                sphere.m_entityData.m_hasCollided = true;
                sphere.m_entityData.m_collidePoint = sphere.transform.position + (sphereVelocity.normalized * vcMag);
                return true;
            }

            return false;
        }
        return false;

    }
    override protected bool IsCollidingWithPlane(Plane plane, float dt)
    {
        return false;
    }

    private void OnDrawGizmos()
    {
        Mesh planeMeshFromPoints = new Mesh();

        Gizmos.color = m_entityData.m_color;

        ////bottom left triangle
        //verticies[0] = vA;
        //verticies[1] = vC;
        //verticies[2] = vD;

        ////top right triangle
        //verticies[3] = vA;
        //verticies[4] = vD;
        //verticies[5] = vB;

        planeMeshFromPoints.vertices = new Vector3[4]
        {
            vA, vB, vC, vD
        };

        planeMeshFromPoints.triangles = new int[]
        {
            3, 2, 0, 1, 3, 0
        };

        planeMeshFromPoints.RecalculateNormals();

        Gizmos.DrawMesh(planeMeshFromPoints, transform.position);
    }
}
