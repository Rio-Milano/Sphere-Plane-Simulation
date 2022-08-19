using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : BaseEntity
{
    //sphere related info
    public float m_radius;
    public Vector3 m_velocity;
    public float m_mass;

    public void InitSphere(float mass, float radius, Vector3 velocity, Vector3 position, Color color)
    {
        m_mass = mass;
        m_radius = radius;
        m_velocity = velocity;
        transform.position = position;

        m_entityData.m_color = color;
        m_entityData.m_collisionType = EntityData.CollisionType.Sphere;
    }

    private bool DynamicToStaticSphereCollision(Sphere otherSphere)
    {
        //vector from centre of this sphere to centre of other sphere
        Vector3 A = otherSphere.transform.position - this.transform.position;
        //vector of motion
        Vector3 V = this.m_velocity*Time.deltaTime;
        //sphere A radius
        float r1 = this.m_radius;
        //sphere B radius
        float r2 = otherSphere.m_radius;
        //find the angle between A and V in degrees
        float theta = Mathf.Acos(Vector3.Dot(A.normalized, V.normalized)) * Mathf.Deg2Rad;
        //find the distance(D) between centres of spheres at closets approach along the path of V
        float d = Mathf.Sin(theta) * A.magnitude;
        //float r1+r2 = sum of radii
        float r1r2 = r1 + r2;
        //if d is less than the sum of r1r2 then a collision is possible but not certain as V may not be large enough
        if (d < r1r2)
        {
            //|A| - |d| = Vc + e where sphereA pos + Vc is the collision point + e is the closest point to sphereB pos
            //find e
            float e = Mathf.Sqrt(Mathf.Pow(r1r2, 2) - Mathf.Pow(d, 2));
            //find vc
            float vcMag = Mathf.Cos(theta) * A.magnitude - e;



            //current position
            Vector3 collisionPointA = transform.position + m_velocity.normalized * vcMag;

            if (m_entityData.m_hasCollided && Vector3.Distance(transform.position, collisionPointA) > Vector3.Distance(transform.position, m_entityData.m_collidePoint))
                return false;

            m_entityData.m_collideWith = otherSphere;
            m_entityData.m_collidePoint = collisionPointA;
            
            return true;
        }
        return false;

    }

    private bool QuadraticFormula(float A, float B, float C, out float resultA, out float resultB)
    {
        resultA = 0.0f;
        resultB = 0.0f;


        float d = Mathf.Pow(B, 2) - 4 * A * C;
        if(d > 0.0f)
        {
            float divisor = 2 * A;
            float root = Mathf.Sqrt(d);

            resultA = (-B + root) / divisor;
           resultB = (-B - root) / divisor;
            return true;
        }

        return false;

    }
    private bool DynamicToDynamicSphereCollision(Sphere otherSphere)
    {
        //variables for this algorithm
        Vector3 P1 = this.transform.position;//the start position of sphere 1
        Vector3 P2 = otherSphere.transform.position;//the start position of sphere 2
        
        Vector3 V1 = this.m_velocity*Time.deltaTime;//the velocity of sphere 1
        Vector3 V2 = otherSphere.m_velocity * Time.deltaTime;//the velocity of sphere 2
        
        float R1 = this.m_radius;//the radius of sphere 1
        float R2 = otherSphere.m_radius;//the radius of sphere 2

        //sphere A is moving to Apos + Avel and sphere B is moving Bpos + Vel. If the 2 collide there will be a point along the movement where the distance
        //between the 2 spheres is = R1+R2. We need to find a value t such that |aPos + t(aVel) - bPos + t(bVel| == R1+R2


        float deltaXP = P1.x - P2.x;
        float deltaXV = V1.x - V2.x;

        float deltaYP = P1.y - P2.y;
        float deltaYV = V1.y - V2.y;

        float deltaZP = P1.z - P2.z;
        float deltaZV = V1.z - V2.z;

        float A = Mathf.Pow(deltaXV, 2) + Mathf.Pow(deltaYV, 2) + Mathf.Pow(deltaZV, 2);
        float B = 2 * deltaXP * deltaXV + 2 * deltaYP * deltaYV + 2 * deltaZP * deltaZV;
        float C = Mathf.Pow(deltaXP, 2) + Mathf.Pow(deltaYP, 2) + Mathf.Pow(deltaZP, 2) - Mathf.Pow(R1 + R2, 2);

        float result1;
        float result2;



        if (QuadraticFormula(A, B, C, out result1, out result2))
        {
            float t = Mathf.Min(result1, result2);

            Vector3 collidePoint = P1 + t * V1;
            if (m_entityData.m_hasCollided && Vector3.Distance(transform.position, collidePoint) > Vector3.Distance(transform.position, m_entityData.m_collidePoint))
                return false;

            m_entityData.m_collideWith = otherSphere;
            m_entityData.m_collidePoint = collidePoint;

            return true;

        }

        return false;
    }

    override protected bool IsCollidingWithSphere(Sphere otherSphere, float dt)
    {
        float thisSphereVelocityMagnitude = m_velocity.magnitude;
        float otherSphereVelocityMagnitude = otherSphere.m_velocity.magnitude;


        //if this sphere is moving and otherSphere is stationary 
        if (thisSphereVelocityMagnitude != 0 && otherSphereVelocityMagnitude == 0 || thisSphereVelocityMagnitude == 0 && otherSphereVelocityMagnitude != 0)
            return DynamicToStaticSphereCollision(otherSphere);
        //if both spheres are moving
        else if (thisSphereVelocityMagnitude != 0 && otherSphereVelocityMagnitude != 0)
            return DynamicToDynamicSphereCollision(otherSphere);

        return false;
    }
    override protected bool IsCollidingWithPlane(Plane plane, float dt)
    {
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = m_entityData.m_color;
        Gizmos.DrawSphere(transform.position, m_radius);
    }



}
