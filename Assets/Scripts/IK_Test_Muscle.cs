using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IK_Test_Muscle : MonoBehaviour
{
    public GameObject target;
    //Vector3 target;
    public int countMax = 1000;

    public float EPS = 0.01f;
    public float step = 0.015f;

    private float[] angles;
    private int count = 0;
    private bool startJT_Method_Flag = false;

    List<Rigidbody> rigidbodyList;
    public Transform ragdoll;

    //List of muscles
    List<MuscleWithAnim> ragdollMuscles = new List<MuscleWithAnim>();
    public List<MuscleWithAnim[]> musclePairs = new List<MuscleWithAnim[]>();

    //Support polygon things
    /*SupportPolygonGenerator supportPolyGenObj;
    public GameObject supportPolyGen;
    Collider supportPolyCol;*/

    public Transform[] rightEdges;
    public Transform[] leftEdges;

    private Vector3 CoMHit = Vector3.zero;
    private Vector3 CoM = Vector3.zero;
    Vector3 targetPos = Vector3.zero;

    [SerializeField]
    private MuscleJoint[] pairs;

    // Start is called before the first frame update
    void Start()
    {
        ragdollMuscles = ragdoll.GetComponentsInChildren<MuscleWithAnim>().ToList();
        rigidbodyList = ragdoll.GetComponentsInChildren<Rigidbody>().ToList();

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

        //jointRots = new GameObject[joints.Length - 1];

        /*for (int i = 0; i < jointRots.Length; i++)
        {
            GameObject tmp = new GameObject(joints[i + 1].name + "_Rot");
            tmp.transform.position = joints[i + 1].transform.position;
            tmp.transform.parent = joints[i].transform;
            jointRots[i] = tmp;
        }*/

        //Get CoM
        /*CoM = CalculateCenterOfMass();
        Debug.Log("COM POS: " + CoM.x + CoM.y + CoM.z);

        supportPolyGenObj = supportPolyGen.GetComponent<SupportPolygonGenerator>();
        supportPolyGenObj.GenerateNewPolygon();
        supportPolyCol = supportPolyGenObj.GetComponent<Collider>();*/
        
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

        //CoM - Support poly things
        /*CoM = CalculateCenterOfMass();
        CoMHit = GetCoMHit();*/
        //target = GetCenterOfColldier(supportPolyCol);
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
        Debug.Log("angles array: " + angles.Length);
    }

    private void iterate_IK()
    {
        Transform endeffector = rigidbodyList.Last().transform;
        Debug.Log(rigidbodyList.Last().name);
        if (Mathf.Abs(Vector3.Distance(endeffector.position, target.transform.position)) > EPS)
        {
            JacobianIK();
        }
        else
        {
            Debug.Log("Cycle Count: " + count.ToString());
            Debug.Log("TARGET REAACVHED!!!");
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
        //rotateLinks2(angleDiff);
        //rotateLinksTest2(angleDiff);
        rotateLinks2(angleDiff);

        count++;
    }

    private float[] GetDeltaOrientation()
    {
        //varje index skillnaden för varje joint
        float[,] Jt = GetJacobianTranspose();

        Vector3 V = (target.transform.position - rigidbodyList.Last().transform.position);
        //Vector3 V = new Vector3(GetCoMHit().x, 0.0f, GetCoMHit().z) - new Vector3(target.x, 0.0f, target.z);
        //multiplicera min jacobian tyranspose med v

        //dO = Jt * V;
        float[,] dO = MatrixTools.MultiplyMatrix(Jt, new float[,] { { V.x }, { V.y }, { V.z } });
        //varje float är en kolumn i matrisen
        //varför förasta vädet i varje kolumn?
        //testa ta ut alla värden? 
        return new float[] { dO[0, 0], dO[1,0], dO[2,0] };
    }

    private float[,] GetJacobianTranspose()
    {
        //Matrix rules: num of columns in one has to equal num of rows in the other

        //måste endra detta för min matris
        //one line for each joint
        //forward right left för varje joint
        //tre per joint
        //ta ut alla nummer per joint

        List<Vector3> tempCrosses = new List<Vector3>();
        for (int i = 0; i < pairs.Length; i++)
        {   //YZ
            Transform body1 = pairs[i].musclesYZ[0].rb1.transform;
            Transform body2 = pairs[i].musclesYZ[0].rb2.transform;
            Transform endEffector = rigidbodyList.Last().transform;
            Debug.Log(rigidbodyList.Last().name);
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
            Debug.Log("PAIRS: " + i);
            //Send the three new angels to the responsible muscleJoint and hope it can work some magic...
            ActivateMusclesJoint(new float[] { angleDiff[i * 3 + 0], angleDiff[i * 3 + 1], angleDiff[i * 3 + 2] }, pairs[i]);
            /*Vector3 crossAxis = Vector3.Cross(pairs[i].musclesYZ[0].rb1.transform.right, (pairs[i].musclesYZ[0].rb1.transform.position - pairs[i].musclesYZ[0].rb1.transform.position).normalized);

            if (angleDiff[i] != 0)
                pairs[i].musclesYZ[0].rb1.transform.RotateAround(pairs[i].musclesYZ[0].rb1.transform.position, crossAxis, angleDiff[i]);

            if (i < rigidbodyList.Count - 2)
                updateLinkPos(i, joints[i].transform.position, crossAxis, angleDiff[i]);
            if (i >= joints.Length - 2) // end effector
                joints[i + 1].transform.position = jointRots[i].transform.position;*/
        }
    }

    private void ActivateMusclesJoint(float[] angles, MuscleJoint joint)
    {
        //YZ
        if (joint.musclesYZ != null && joint.musclesYZ.Length > 0)
        {
            joint.musclesYZ[0].targetLength = angles[0] <= 0 ? 0.1f : GetMuscleLengthFromAngle(joint.musclesYZ[0], 179);
            joint.musclesYZ[1].targetLength = angles[0] >= 0 ? 0.1f : GetMuscleLengthFromAngle(joint.musclesYZ[1], 179);
            joint.musclesYZ[0].Activate();
            joint.musclesYZ[1].Activate();
            Debug.Log("TARGET 0: " + joint.musclesYZ[0].targetLength);
            Debug.Log("TARGET 1: " + joint.musclesYZ[1].targetLength);
        }

        //ZX
        if (joint.musclesZX != null && joint.musclesZX.Length > 0)
        {
            joint.musclesZX[0].targetLength = angles[1] <= 0 ? 0.1f : GetMuscleLengthFromAngle(joint.musclesZX[0], 179);
            joint.musclesZX[1].targetLength = angles[1] >= 0 ? 0.1f : GetMuscleLengthFromAngle(joint.musclesZX[1], 179);
            joint.musclesZX[0].Activate();
            joint.musclesZX[1].Activate();
            Debug.Log("ZX 0 ACTIVATED");
            Debug.Log("ZX 1 ACTIVATED");
        }

        //XY
        if(joint.musclesXY != null && joint.musclesXY.Length > 0)
        {
            joint.musclesXY[0].targetLength = angles[2] <= 0 ? 0.1f : GetMuscleLengthFromAngle(joint.musclesXY[0], 179);
            joint.musclesXY[1].targetLength = angles[2] >= 0 ? 0.1f : GetMuscleLengthFromAngle(joint.musclesXY[1], 179);
            joint.musclesXY[0].Activate();
            joint.musclesXY[1].Activate();
            Debug.Log("TARGET 2: " + joint.musclesXY[0].targetLength);
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

        return triangleSideA;
    }
    /*
    private void ActivateMuscles2(float[] angles, MuscleJoint join)
    {
        for(int i = 0; i < pairs.Length - 3; i++)
        {
            MuscleJoint p = pairs[i];
            MuscleJoint p2 = pairs[i + 3];
            //activate 2 pairs per joint

            Debug.Log(i);
            if(i == index)
            {
                if (angle < 0)
                {
                    MuscleWithAnim toActivate = p.muscles[0];
                    MuscleWithAnim toActivate2 = p2.muscles[0];
                    //also activate pairs i + 2
                    float length = GetMuscleLengthFromAngle(toActivate, angle);
                    float length2 = GetMuscleLengthFromAngle(toActivate2, angle);
                    toActivate.targetLength = length;
                    toActivate2.targetLength = length2;
                    toActivate.Activate();
                    toActivate2.Activate();
                    Debug.Log("JOINTINDEX: " + index + "MUSCLE: " + i + "AND: " + (i + 3) + "ACTIVATED");
                }
                else
                {
                    MuscleWithAnim toActivate = p.muscles[1];
                    MuscleWithAnim toActivate2 = p2.muscles[1];
                    float length = GetMuscleLengthFromAngle(toActivate, angle);
                    float length2 = GetMuscleLengthFromAngle(toActivate2, angle);
                    toActivate.targetLength = length;
                    toActivate2.targetLength = length2;
                    toActivate.Activate();
                    toActivate2.Activate();
                    Debug.Log("JOINTINDEX: " + index + "MUSCLE: " + i + "AND: " + (i + 3) + "ACTIVATED");

                }  
            }
        }
    }
    */



    //----------SUPPORT POLYGON THINGS--------------//

    /*public Vector3 CalculateCenterOfMass()
    {
        CoM = Vector3.zero;
        float c = 0f;

        foreach (Rigidbody body in rigidbodyList)
        {
            CoM += body.worldCenterOfMass * body.mass;
            c += body.mass;
        }

        CoM /= c;
        Debug.DrawRay(CoM, Vector3.down * 2f, Color.cyan);
        return CoM;
    }

    private Vector3 GetCoMHit()
    {
        RaycastHit hit;

        if (Physics.Raycast(CoM, Vector3.down, out hit, Mathf.Infinity))
        {
            if (hit.collider != null)
            {
                return hit.point;
            }
        }
        return Vector3.zero;
    }

    private Vector3 GetCenterOfColldier(Collider c)
    {
        c = supportPolyGenObj.GetComponent<Collider>();
        Vector3 c_Center = c.bounds.center;

        return c_Center;
    }*/
}

[System.Serializable]
public struct MuscleJoint
{
    [SerializeField]
    public MuscleWithAnim[] musclesYZ;
    public MuscleWithAnim[] musclesZX;
    public MuscleWithAnim[] musclesXY;
}
