using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IKTest : MonoBehaviour
{
    //O is Vector containing initial muscle lengths
    float[] initialLengths;
    int count = 0;

    List<MuscleWithAnim> ragdollMuscles = new List<MuscleWithAnim>();
    List<Rigidbody> rigidbodyList;

    //T is vector representing final muscle lengths such that goal pos is reached
    List<Vector3> finalLengths;

    private bool startJT = false;

    //Transform end effector E (which is supposed to reach target)
    public Transform endEffector;
    public Transform targetPos;
    public Transform ragdollParent;

    public float EPS = 0.1f;
    public float step = 0.01f;
    public int maxIter = 1000;

    
    void Start()
    {
        rigidbodyList = ragdollParent.GetComponentsInChildren<Rigidbody>().ToList();
        ragdollMuscles = ragdollParent.GetComponentsInChildren<MuscleWithAnim>().ToList();
        initialLengths = new float[ragdollMuscles.Count];
        //add all intial muscle lengths to Initial lengths

        //Make sure that this list gets correct values
        for(int i = 0; i < initialLengths.Length; i++)
        {
            foreach (MuscleWithAnim m in ragdollMuscles)
            {
                initialLengths[i] = (m.a1.position - m.a2.position).magnitude;
            }
        }

        foreach (Rigidbody rb in rigidbodyList)
        {
            rb.centerOfMass = new Vector3(0, 0, 0);
            rb.inertiaTensor = new Vector3(1, 1, 1);
            if (rb.GetComponent<Collider>() != null)
            {
                //ignore collision on player layer
                Physics.IgnoreLayerCollision(6, 6);
            }
        }
    }

    void Update()
    {

        //IterateIK();
        //StartIK();

        if (startJT)
        {
            IterateIK();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartIK();
        }

    }

    //Initial conditions
    void StartIK()
    {
        count = 0;
        //start method flag
        startJT = true;

        //get all start muscle lengths
        //put in initialLengths array?
        //so that should not be in start?
        float[] lengths = new float[ragdollMuscles.Count];
        for(int i = 0; i < ragdollMuscles.Count; i++)
        {
            lengths[i] = ((ragdollMuscles[i].a1.position - ragdollMuscles[i].a2.position).magnitude);
        }
    }

    //Do the thing
    void IterateIK()
    {
        if(Mathf.Abs(Vector3.Distance(endEffector.position, targetPos.position)) > EPS)
        {
            JacobianIK();
        }
        else
        {
            startJT = false;
            Debug.Log("Target reached!");
        }

        if(count >= maxIter)
        {
            Debug.Log("max iterations reached");
            startJT = false;
        }
    }

    void JacobianIK()
    {
        //array for necessary length changes
        float[] dO = new float[ragdollMuscles.Count];
        float[] lengthDiff = new float[ragdollMuscles.Count];

        dO = GetDeltaOrientation();
        Debug.Log("HAIIIII: " + dO[0] + " " + dO[1]);

        for(int i = 0; i < dO.Length; i++)
        {
            //add the new muscle lengths/the changes to the lengthDiff[]
            initialLengths[i] += dO[i] * step;
            lengthDiff[i] = dO[i] * step;
        }

        ChangeMuscleLengths(lengthDiff);
        count++;

    }

    float[] GetDeltaOrientation()
    {
        //find dO
        float[,] Jt = GetJacobianTranspose();
        Debug.Log("first Jt: " + Jt[0, 0]);
        Debug.Log("second Jt: " + Jt[1, 0]);

        Vector3 V = targetPos.position - endEffector.position;

        //V = j * dO
        //dO = Jt * V
        float[,] dO = MatrixTools.MultiplyMatrix(Jt, new float[,] { { V.x }, { V.y }, { V.z } });
        Debug.Log("V: " + V);
        Debug.Log("first dO: " + dO[0, 0]);
        Debug.Log("second dO: " + dO[1, 0]);
        //???
        return new float[] { dO[0, 0], dO[1, 0]};


        //Jt = GetJacobianTranspose()
    }

    float[,] GetJacobianTranspose()
    {
        Vector3 J_A = Vector3.Cross((ragdollMuscles[0].a1.position - ragdollMuscles[0].a2.position), 
                                   ((ragdollMuscles[0].a1.position - ragdollMuscles[0].a2.position) - endEffector.position));
        Vector3 J_B = Vector3.Cross((ragdollMuscles[1].a1.position - ragdollMuscles[1].a2.position), 
                                   (ragdollMuscles[1].a1.position - ragdollMuscles[1].a2.position - endEffector.position));


        Debug.Log("first muscle" + (ragdollMuscles[0].a1.position - ragdollMuscles[0].a2.position).magnitude);
        Debug.Log("second muscle" + (ragdollMuscles[1].a1.position - ragdollMuscles[1].a2.position).magnitude);
        Debug.Log("end effector muscle" + (ragdollMuscles[ragdollMuscles.Count - 1].a1.position - ragdollMuscles[ragdollMuscles.Count - 1].a2.position).magnitude);
        Debug.Log("EYYY" + ((ragdollMuscles[ragdollMuscles.Count - 1].a1.position) - endEffector.position));

        float[,] matrix = new float[2, 3];

        //populate the matrix with J_A and J_B values
        matrix = MatrixTools.PopulateMatrix(matrix, new Vector3[] { J_A, J_B });

        //return the transposed matrix
        return MatrixTools.TransposeMatrix(matrix);
    }

    float CalculateCurrentMuscleLength(Transform a1, Transform a2)
    {
        //do I also want direction?
        return (a1.position - a2.position).magnitude;
    }

    void ChangeMuscleLengths(float[] lengthChanges)
    {
        float[] changes = new float[lengthChanges.Length];

        for (int i = 0; i < ragdollMuscles.Count; i++)
        {
            float currentLength = CalculateCurrentMuscleLength(ragdollMuscles[i].a1, ragdollMuscles[i].a2);
            //this array does not contain anything right now?
            float newLength = lengthChanges[i]; //length changes are calculated in JacobianIK()
            changes[i] = lengthChanges[i] + currentLength;
            //array with new muscle lengths?
            //activate the muscle using value of new length
            //length changes is a float, get this to muscle length?

            //get the activation value from the length change
            //activate the muscle
            ragdollMuscles[i].targetLength = newLength;
            Debug.Log("NEW LENGTH: " + newLength);
            ragdollMuscles[i].Activate();
        }
    }
}
