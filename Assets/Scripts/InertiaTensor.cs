using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InertiaTensor : MonoBehaviour
{


    /*private float ReturnInertia(int col)
    {
        float h = HEIGHT;
        float radius = RADIUS;
        float totalMass = MASS;
        if (col == 0 || col == 1)
            return (1f / 12f) * totalMass * math.pow(h, 2) + (1f / 4f) * totalMass * math.pow(radius, 2);
        else if (col == 2)
            return (1f / 2f) * totalMass * math.pow(radius, 2);
        else
        {
            return 0;
        }
    }
    // Initial inertia tensor at t0
    inertiaObj = new float3x3(
        new float3(ReturnInertia(0), 0, 0),
        new float3(0, ReturnInertia(1), 0),
        new float3(0, 0, ReturnInertia(2))
    );
// inertia tensor at t
inertia = math.mul(rotationMatrix, math.mul(inertiaObj, math.transpose(rotationMatrix)));*/
}
