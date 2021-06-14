using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IK_Test_Muscle : MonoBehaviour
{
    public GameObject target;
    //Vector3 target;
    public int countMax;
    public float EPS;
    public float step;

    private float[] angles;
    private int count = 0;
    private bool startJT_Method_Flag = false;

    public List<Rigidbody> rigidbodyList;
    public Transform ragdoll;

    //List of muscles
    List<MuscleWithAnim> ragdollMuscles = new List<MuscleWithAnim>();
    public List<MuscleWithAnim[]> musclePairs = new List<MuscleWithAnim[]>();

    public Transform[] rightEdges;
    public Transform[] leftEdges;

    Vector3 targetPos = Vector3.zero;

    [SerializeField]
    private MuscleJoint[] pairs;

    // Start is called before the first frame update
    void Start()
    {
        ragdollMuscles = ragdoll.GetComponentsInChildren<MuscleWithAnim>().ToList();

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
        Vector3 targetPos = target.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        targetPos = target.transform.position;
        if (startJT_Method_Flag)
        {
            iterate_IK();
        }
        start_IK();
    }

    private void start_IK()
    {
        count = 0;
        startJT_Method_Flag = true;

        List<float> tempAngels = new List<float>();
        for(int i=0;i< pairs.Length; i++)
        {   //YZ
            Vector3 body1 = pairs[i].musclesYZ[0].rb1.transform.position;
            Vector3 body2 = pairs[i].musclesYZ[0].rb2.transform.position;
            tempAngels.Add(calculateAngle(Vector3.right, body2, body1));
            tempAngels.Add(calculateAngle(Vector3.up, body2, body1));
            tempAngels.Add(calculateAngle(Vector3.forward, body2, body1));
        }
        angles = tempAngels.ToArray();
    }

    private void iterate_IK()
    {
        Transform endeffector = rigidbodyList.Last().transform;
        if (Mathf.Abs(Vector3.Distance(endeffector.position, target.transform.position)) > EPS)
        {
            JacobianIK();
        }
        else
        {
            Debug.Log("Cycle Count: " + count.ToString());
            startJT_Method_Flag = false;
        }
        if (count >= countMax)
        {
            Debug.Log("Hit Cycle Count: " + count.ToString());
            startJT_Method_Flag = false;
        }
    }

    private void JacobianIK()
    {
        float[] dO = new float[angles.Length];
        float[] angleDiff = new float[angles.Length];

        dO = GetDeltaOrientation();
        for (int i = 0; i < dO.Length; i++)
        {
            angles[i] += dO[i] * step;
            angleDiff[i] = dO[i] * step;
        }

        // update angles
        rotateLinks2(angleDiff);

        count++;
    }

    private float[] GetDeltaOrientation()
    {
        float[,] Jt = GetJacobianTranspose();

        Vector3 V = (target.transform.position - rigidbodyList.Last().transform.position);

        //dO = Jt * V;
        float[,] dO = MatrixTools.MultiplyMatrix(Jt, new float[,] { { V.x }, { V.y }, { V.z } });
        List<float> jointAngles = new List<float>();
        for (int i = 0; i < pairs.Length * 3; i++)
        {
            jointAngles.Add(dO[i, 0]);
        }
        return jointAngles.ToArray();
        //return new float[] { dO[0, 0], dO[1,0], dO[2,0] };
    }

    private float[,] GetJacobianTranspose()
    {
        //Matrix rules: num of columns in one has to equal num of rows in the other

        List<Vector3> tempCrosses = new List<Vector3>();
        for (int i = 0; i < pairs.Length; i++)
        {   //YZ
            Transform body1 = pairs[i].musclesYZ[0].rb1.transform;
            Transform body2 = pairs[i].musclesYZ[0].rb2.transform;
            Transform endEffector = rigidbodyList.Last().transform;
            
            tempCrosses.Add(Vector3.Cross(body1.right, (endEffector.position - body1.position)));
            tempCrosses.Add(Vector3.Cross(body1.up, (endEffector.position - body1.position)));
            tempCrosses.Add(Vector3.Cross(body1.forward, (endEffector.position - body1.position)));
        }

        float[,] matrix = new float[3, tempCrosses.Count];
        matrix = MatrixTools.PopulateMatrix(matrix, tempCrosses.ToArray());
        return MatrixTools.TransposeMatrix(matrix);
    }

    private float calculateAngle(Vector3 axis, Vector3 pos1, Vector3 pos2)
    {
        float value = 0f;
        value = Vector3.Angle(axis, (pos1 - pos2).normalized);
        Vector3 cross = Vector3.Cross(axis, (pos1 - pos2).normalized);
        if (cross.z < 0)
            value = -value;

        return value;
    }

    private void rotateLinks2(float[] angleDiff)
    {
        float[] displayAngles = new float[angleDiff.Length];
        Transform endEffector = rigidbodyList.Last().transform;
        for (int i = 0; i < pairs.Length; i++)
        {   //YZ
            Transform body1 = pairs[i].musclesYZ[0].rb1.transform;
            Transform body2 = pairs[i].musclesYZ[0].rb2.transform;
            float currAngleYZ = calculateAngle(Vector3.right, body2.position, body1.position);
            float currAngleZX = calculateAngle(Vector3.up, body2.position, body1.position);
            float currAngleXY = calculateAngle(Vector3.forward, body2.position, body1.position);

            //For debugging i guess
            displayAngles[i * 3 + 0] = angleDiff[i * 3 + 0] + currAngleYZ;
            displayAngles[i * 3 + 1] = angleDiff[i * 3 + 1] + currAngleZX;
            displayAngles[i * 3 + 2] = angleDiff[i * 3 + 2] + currAngleXY;
            
            //Send the three new angels to the responsible muscleJoint
            ActivateMusclesJoint(new float[] { angleDiff[i * 3 + 0], angleDiff[i * 3 + 1], angleDiff[i * 3 + 2] }, pairs[i]);
        }
    }

    private void ActivateMusclesJoint(float[] angles, MuscleJoint joint)
    {
        //YZ
        if (joint.musclesYZ != null && joint.musclesYZ.Length > 0)
        {
            if(angles[0] > 0)
            {
                joint.musclesYZ[0].targetLength = GetMuscleLengthFromAngle(joint.musclesYZ[0], angles[0]);
                joint.musclesYZ[0].Activate();
            }
            else
            {
                joint.musclesYZ[1].targetLength = GetMuscleLengthFromAngle(joint.musclesYZ[1], Mathf.Abs(angles[0]));
                joint.musclesYZ[1].Activate();
            }
        }

        //ZX
        if (joint.musclesZX != null && joint.musclesZX.Length > 0)
        {
            if (angles[1] > 0)
            {
                joint.musclesZX[0].targetLength = GetMuscleLengthFromAngle(joint.musclesZX[0], angles[1]);
                joint.musclesZX[0].Activate();
            }
            else
            {
                joint.musclesZX[1].targetLength = GetMuscleLengthFromAngle(joint.musclesZX[1], Mathf.Abs(angles[1]));
                joint.musclesZX[1].Activate();
            }
        }

        //XY
        if(joint.musclesXY != null && joint.musclesXY.Length > 0)
        {

            if (angles[2] > 0)
            {
                joint.musclesXY[0].targetLength = GetMuscleLengthFromAngle(joint.musclesXY[0], angles[2]);
                joint.musclesXY[0].Activate();
            }
            else
            {
                joint.musclesXY[1].targetLength = GetMuscleLengthFromAngle(joint.musclesXY[1], Mathf.Abs(angles[2]));
                joint.musclesXY[1].Activate();
            }
        }
    }
    private float GetMuscleLengthFromAngle(MuscleWithAnim m, float newAngle)
    {
        Rigidbody rb1 = m.rb1;
        Transform a1 = m.a1;
        Transform a2 = m.a2;

        Vector3 jointPos = (a1.position + a2.position) / 2f;
        float triangleSideB = (jointPos - a1.position).magnitude;
        float triangleSideC = (jointPos - a2.position).magnitude;
        float angle = newAngle;

        //find hypotenuse using law of cosines
        //a^2 = b^2 + c^2 - 2bc cos(A)
        float triangleSideASqr = Mathf.Pow(triangleSideB, 2) + Mathf.Pow(triangleSideC, 2) - (2f * triangleSideB * triangleSideC * Mathf.Cos(angle));
        float triangleSideA = Mathf.Sqrt(triangleSideASqr);
        if (triangleSideA <= 0) triangleSideA = 0f;
        if (triangleSideA >= 1) triangleSideA = 1;
        if (float.IsNaN(triangleSideA))
        {
            triangleSideA = 0;
        }

        return triangleSideA;
    }
}

[System.Serializable]
public struct MuscleJoint
{
    [SerializeField]
    public MuscleWithAnim[] musclesYZ;
    public MuscleWithAnim[] musclesZX;
    public MuscleWithAnim[] musclesXY;
}
