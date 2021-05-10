using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InertiaTensor : MonoBehaviour
{

    /*private float ReturnInertia(int col)
    {
        float h = HEIGHT; //get height from cylinder
        float radius = RADIUS; //get radius from cylinder
        float totalMass = MASS; //get mass from cylinder
        if (col == 0 || col == 1)
            return (1f / 12f) * totalMass * Math.Pow(h, 2) + (1f / 4f) * totalMass * Math.Pow(radius, 2);
        else if (col == 2)
            return (1f / 2f) * totalMass * Math.Pow(radius, 2);
        else
        {
            return 0;
        }
    }

    private void FixedUpdate()
    {
        //Initial inertia tensor at t0
        inertiaObj = new float3x3(
            new float3(ReturnInertia(0), 0, 0),
            new float3(0, ReturnInertia(1), 0),
            new float3(0, 0, ReturnInertia(2))
        );

        //initial tensor at t
        inertia = Math.mul(rotationMatrix, Math.mul(inertiaObj, Math.transpose(rotationMatrix)));

    }*/
}
