using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MuscleAnimationController : MonoBehaviour
{
    public MuscleAnimation animationObject;
    public Transform ragdoll;
    public GameObject showCoM;
    public Transform[] footEdges;

    //Start of the animation timeline
    public float animationHead = 0.0f;

    Vector3 CoM = Vector3.zero;
    List<MuscleWithAnim> ragdollMuscles = new List<MuscleWithAnim>();
    List<Rigidbody> rigidbodyList;
    bool isBalanced;
    
    void Start()
    {
        Debug.Log(footEdges[0].transform.position);
        rigidbodyList = ragdoll.GetComponentsInChildren<Rigidbody>().ToList();
        ragdollMuscles = ragdoll.GetComponentsInChildren<MuscleWithAnim>().ToList();
        CalculateSupportPolygon();
    }

    void FixedUpdate()
    {
        CalculateCenterOfMass();
        showCoM.transform.position = CoM;

        bool balanced1 = IsCoMBalanced(CoM, footEdges[0].position, footEdges[1].position, footEdges[2].position);
        bool balanced2 = IsCoMBalanced(CoM, footEdges[0].position, footEdges[3].position, footEdges[2].position);
        if (balanced1 || balanced2) isBalanced = true;
        Debug.Log(isBalanced);

        //Set target length according to anim curve and activate muscles
        foreach (MuscleAnimationStruct mStruct in animationObject.muscles)
        {
            foreach(MuscleWithAnim m in ragdollMuscles)
            {
                if(m.name == mStruct.muscleName)
                {
                    m.targetLength = mStruct.animCurve.Evaluate(animationHead);
                    m.Activate();
                }
            }
        }
        animationHead += Time.fixedDeltaTime;
    }

    //Check if CoM is balanced
    //TODO: Change to SAT to support all shapes
    //Or raycast on collider

    Vector3 CalculateCenterOfMass()
    {
        float c = 0f;

        foreach (Rigidbody body in rigidbodyList)
        {
            CoM += body.worldCenterOfMass * body.mass;
            c += body.mass;
        }

        CoM /= c;
        return CoM;
    }

    void CalculateSupportPolygon()
    {
        Vector3 line1 = footEdges[0].transform.position - footEdges[1].transform.position;
        Vector3 line2 = footEdges[1].transform.position - footEdges[2].transform.position;
        Vector3 line3 = footEdges[2].transform.position - footEdges[3].transform.position;
        Vector3 line4 = footEdges[3].transform.position - footEdges[0].transform.position;

        Debug.DrawLine(footEdges[0].position, footEdges[1].position, Color.magenta, 5.0f);
        Debug.DrawLine(footEdges[1].position, footEdges[2].position, Color.magenta, 5.0f);
        Debug.DrawLine(footEdges[2].position, footEdges[3].position, Color.magenta, 5.0f);
        Debug.DrawLine(footEdges[3].position, footEdges[0].position, Color.magenta, 5.0f);
    }

    float Sign(Vector3 c, Vector3 p1, Vector3 p2)
    {
        return (c.x - p2.x) * (p1.y - p2.y) - (p2.x - p1.x) * (c.y - p2.y);
    }

    bool IsCoMBalanced(Vector3 c, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float d1, d2, d3;
        bool positive, negative;

        d1 = Sign(c, v1, v2);
        d2 = Sign(c, v2, v3);
        d3 = Sign(c, v3, v1);

        negative = (d1 < 0) || (d2 < 0) || (d3 < 0);
        positive = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(negative && positive);
    }
}

