using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//base collider class that all types of collidables should inherit from
//should sphere and plane collision

public abstract class BaseEntity : MonoBehaviour
{
    //lots of info that a basic entity needs for this simulation
    public struct EntityData
    {
        public enum CollisionType
        {
            //types of collision
            Plane = 0,
            Sphere = 1
        };

        public Color m_color { get; set; }
        public CollisionType m_collisionType;
        
        //Using the transform position and velocity of an object. We can determin wether or not they collide and thus yeild the point at which the collide.
        //This is then stored here.
        public Vector3 m_collidePoint;
        public bool m_hasCollided;
        public BaseEntity m_collideWith;

    }

    //a single instance of entity data per entity
    public EntityData m_entityData;

    //any type should be able to collide with any type, should be called from root of collision checks
    abstract protected bool IsCollidingWithSphere(Sphere sphere, float dt);
    abstract protected bool IsCollidingWithPlane(Plane plane, float dt);

    //this method will be called from the root of all collision checks and entities will be looped one over the other efficiently
    public bool IsColliding(BaseEntity otherEntity, float dt)
    {
        switch (otherEntity.m_entityData.m_collisionType)
        {
            case EntityData.CollisionType.Plane    : return IsCollidingWithPlane((Plane)otherEntity, dt);//if colliding with plane then use plane collision 
            case EntityData.CollisionType.Sphere   : return IsCollidingWithSphere((Sphere)otherEntity, dt);//if colliding with sphere then use sphere collision
            default                     : return false;//if collision type of other entity is not known then dont check for collision

        }
    }

};