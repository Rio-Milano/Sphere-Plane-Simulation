using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CollisionSystem : MonoBehaviour
{
    //this array will hold planes and spheres. The architecture can be extended to use other shapes.
    private BaseEntity[] m_entityArray;
    //using vaues 100x bigger we get more accurate values of dt

    void Start()
    {
        GameObject newGameObject = null;

        //Planes//TL,BL, TR, BR
         newGameObject = new GameObject();
         newGameObject.AddComponent<Plane>().InitPlane(new Color(0, 255, 0), new Vector3(-50, 0, -50), new Vector3(-50, 0, 50), new Vector3(50, 0, -50), new Vector3(50, 0, 50));

        newGameObject = new GameObject();
        newGameObject.AddComponent<Plane>().InitPlane(new Color(255, 255, 0), new Vector3(-50, 10, -50), new Vector3(-50, 0, -50), new Vector3(50, 10, -50), new Vector3(50, 0, -50));

        newGameObject = new GameObject();
        newGameObject.AddComponent<Plane>().InitPlane(new Color(255, 255, 0), new Vector3(-50, 10, 50), new Vector3(-50, 0, 50), new Vector3(-50, 10, -50), new Vector3(-50, 0, -50));

        newGameObject = new GameObject();
        newGameObject.AddComponent<Plane>().InitPlane(new Color(255, 255, 0), new Vector3(50, 0, 50), new Vector3(-50, 0, 50), new Vector3(50, 10, 50), new Vector3(-50, 10, 50));

        newGameObject = new GameObject();
        newGameObject.AddComponent<Plane>().InitPlane(new Color(255, 255, 0), new Vector3(50, 0, -50), new Vector3(50, 0, 50), new Vector3(50, 10, -50), new Vector3(50, 10, 50));



        //Spheres

        newGameObject = new GameObject("Sphere");
        newGameObject.AddComponent<Sphere>().InitSphere(1, 3, new Vector3(-20, 0, -20), new Vector3(5, 5, 3), new Color(255, 255, 255));

        newGameObject = new GameObject("Sphere");
        newGameObject.AddComponent<Sphere>().InitSphere(1, 3, new Vector3(20, 0, 20), new Vector3(5.0f, 5, -30), new Color(255, 255, 255));

        newGameObject = new GameObject("Sphere");
        newGameObject.AddComponent<Sphere>().InitSphere(1, 3, new Vector3(10, 0,-20), new Vector3(-25, 5, -25), new Color(255, 255, 255));

        newGameObject = new GameObject("Sphere");
        newGameObject.AddComponent<Sphere>().InitSphere(1, 6, new Vector3(10, 0, -20), new Vector3(25, 5, 25), new Color(255, 255, 255));

        newGameObject = new GameObject("Sphere");
        newGameObject.AddComponent<Sphere>().InitSphere(1, 4, new Vector3(0, 0, 0), new Vector3(0, 5, 0), new Color(255, 255, 255));


        //Pass the known created entities we have made to the entity array
        m_entityArray = FindObjectsOfType<BaseEntity>();
         
    }

    private bool CheckIntersection(BaseEntity baseSphere)
    { 
        Sphere sphere = baseSphere as Sphere;

        //loop through each entity
        for (int i = 0; i < m_entityArray.Length; i++)
        {
            //get the current entity
            BaseEntity currentEntity = m_entityArray[i];
            if (currentEntity == baseSphere) continue;
               

            switch (currentEntity)
            {
                case Sphere otherSphere:
                    if ((Vector3.Distance(sphere.transform.position, otherSphere.transform.position) - (sphere.m_radius + otherSphere.m_radius)) < 0.0f)
                        return true;

                    break;
            }
        }
        return false;
    }

    public float coefficientOfStaticFriction = 0.9999f;
    private void UpdateEntityPosition(BaseEntity currentEntity)
    {
        switch (currentEntity)
        {
            //only spheres can move at this stage but this can easily be fixed
            case Sphere sphere:

                Vector3 prevPos = sphere.transform.position;

                sphere.transform.position += sphere.m_velocity * Time.deltaTime;


                sphere.m_velocity *= coefficientOfStaticFriction;

                if (CheckIntersection(currentEntity))
                    sphere.transform.position = prevPos;



                break;

        }
    }

    void ResolveEntityCollisions()
    {

        //loop through each entity
        for (int i = 0; i < m_entityArray.Length; i++)
        {
            //get the current entity
            BaseEntity currentEntity = m_entityArray[i];


            if (currentEntity.m_entityData.m_hasCollided)
            {
                switch (currentEntity)
                {
                    case Sphere sphere:
                        //if the sphere is going to meet or pass at the collision point
                        if (Vector3.Distance(sphere.transform.position + sphere.m_velocity * Time.deltaTime, sphere.transform.position) >= Vector3.Distance(sphere.m_entityData.m_collidePoint, sphere.transform.position))
                        {
                            switch (sphere.m_entityData.m_collideWith)
                            {
                                case Plane otherPlane:
                                    {
                                        //sphere.m_velocity = -V1;
                                        sphere.m_velocity = Vector3.Reflect(sphere.m_velocity, otherPlane.GetPlaneNormal());
                                        sphere.transform.position = sphere.m_entityData.m_collidePoint;
                                        sphere.m_entityData.m_hasCollided = false;
                                        
                                        break;
                                    }

                                case Sphere otherSphere:
                                    {
                                        bool applyEntityAdjustments = false;

                                        Vector3 S1 = sphere.m_entityData.m_collidePoint;//centre of sphere 1
                                        Vector3 V1 = sphere.m_velocity;//velocity of sphere 1
                                        float M1 = sphere.m_mass;//mass of sphere 1

                                        Vector3 S2 = otherSphere.m_entityData.m_collidePoint;//centre of sphere 2
                                        Vector3 V2 = otherSphere.m_velocity;//velocity of sphere 1
                                        float M2 = otherSphere.m_mass;//mass of sphere 2

                                        //solve moving to stationary impulse
                                        if (sphere.m_velocity.magnitude != 0 && otherSphere.m_velocity.magnitude == 0)
                                        {
                                            Vector3 forceDirection = (S2 - S1).normalized;//get a normal vector from S1->S2
                                            float angleBetweenfdandV1 = Mathf.Cos(Vector3.Dot(forceDirection, V1.normalized) * Mathf.Deg2Rad);//calculate angle used to calculate energy transfer

                                            Vector3 V2_ = angleBetweenfdandV1 * V1.magnitude * forceDirection;
                                            Vector3 V1_ = V1 - V2_;

                                            sphere.m_velocity = V1_;
                                            otherSphere.m_velocity = V2_;

                                            applyEntityAdjustments = true;
                                        }
                                        //solve for moving to moving impulse
                                        else if (sphere.m_velocity.magnitude != 0 && otherSphere.m_velocity.magnitude != 0)
                                        {
                                            Vector3 fd1 = (S2 - S1).normalized;//normal vector from sphere 1 to sphere 2
                                            Vector3 fd2 = (-fd1).normalized;//normal vector from sphere 2 to sphere 1

                                            float fd1_angle_V1n = Mathf.Cos(Vector3.Dot(fd1, V1.normalized) * Mathf.Deg2Rad);

                                            Vector3 force_1 = (fd1_angle_V1n * (V1 * M1).magnitude * fd1) / M2;
                                            Vector3 force_2 = -force_1;

                                            Vector3 V2_ = V2 + force_1 - force_2;
                                            Vector3 V1_ = M1 * V1 + M2 * V2 - M2 * V2_;

                                            sphere.m_velocity = V1_;
                                            otherSphere.m_velocity = V2_;


                                            applyEntityAdjustments = true;
                                        }

                                        if (applyEntityAdjustments)
                                        {
                                            Color newColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 255); ;
                                            
                                            sphere.transform.position = sphere.m_entityData.m_collidePoint;
                                            sphere.m_entityData.m_hasCollided = false;
                                            sphere.m_entityData.m_color = newColor;


                                            otherSphere.transform.position = otherSphere.m_entityData.m_collidePoint;
                                            otherSphere.m_entityData.m_hasCollided = false;
                                            otherSphere.m_entityData.m_color = newColor;


                                        }

                                        float maxMag = 20.0f;
                                        if (sphere.m_velocity.magnitude > maxMag)
                                            sphere.m_velocity = sphere.m_velocity.normalized * maxMag;
                                        if (otherSphere.m_velocity.magnitude > maxMag)
                                            otherSphere.m_velocity = otherSphere.m_velocity.normalized * maxMag;

                                        break;

                                    }
           

                            }
                          

                        }
                        break;
                }
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < m_entityArray.Length; i++)
        {
            //get the current entity
            BaseEntity currentEntity = m_entityArray[i];
            currentEntity.m_entityData.m_hasCollided = false;

            UpdateEntityPosition(currentEntity);
            //loop other entities
            for (int j = 0; j < m_entityArray.Length; j++)
            {
                //skip self
                if (i == j)
                    continue;

                //get other current entity
                if (currentEntity.IsColliding(m_entityArray[j], Time.deltaTime))
                    currentEntity.m_entityData.m_hasCollided = true;

            }
        }
        ResolveEntityCollisions();
    }


}
