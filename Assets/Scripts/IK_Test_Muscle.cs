using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IK_Test_Muscle : MonoBehaviour
{
    public GameObject[] joints;
    private GameObject[] jointRots;
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
    private MusclePair[] pairs;

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
    void Update()
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

        float angleA = calculateAngle(Vector3.up, joints[1].transform.position, joints[0].transform.position);
        float angleB = calculateAngle(Vector3.up, joints[2].transform.position, joints[1].transform.position);
        float angleC = calculateAngle(Vector3.up, joints[3].transform.position, joints[2].transform.position);
        angles = new float[] { angleA, angleB, angleC };
    }

    private void iterate_IK()
    {
        
        if (Mathf.Abs(Vector3.Distance(joints[joints.Length - 1].transform.position, target.transform.position)) > EPS)
        {
            JacobianIK();
        }

        /*if (Mathf.Abs(Vector3.Distance(new Vector3(GetCoMHit().x, 0.0f, GetCoMHit().z), new Vector3(target.x, 0.0f, target.z))) > EPS)
        {
            JacobianIK();
        }*/
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
        //rotateLinks(angleDiff);

        count++;
    }

    private float[] GetDeltaOrientation()
    {
        //varje index skillnaden för varje joint
        float[,] Jt = GetJacobianTranspose();

        Vector3 V = (target.transform.position - joints[joints.Length - 1].transform.position);
        //Vector3 V = new Vector3(GetCoMHit().x, 0.0f, GetCoMHit().z) - new Vector3(target.x, 0.0f, target.z);
        //multiplicera min jacobian tyranspose med v

        //dO = Jt * V;
        float[,] dO = MatrixTools.MultiplyMatrix(Jt, new float[,] { { V.x }, { V.y }, { V.z }/*, { V.x }, { V.y }, { V.z }, { V.x }, { V.y }, { V.z }*/ });
        //varje float är en kolumn i matrisen
        //varför förasta vädet i varje kolumn?
        //testa ta ut alla värden? 
        return new float[] { dO[0, 0], dO[1,0], dO[2,0] };
    }

    private float[,] GetJacobianTranspose()
    {
        //måste endra detta för min matris
        //one line for each joint
        //forward right left för varje joint
        //tre per joint
        //ta ut alla nummer per joint
        Vector3 J_A = Vector3.Cross(joints[0].transform.forward, (joints[joints.Length - 1].transform.position - joints[0].transform.position));
        Vector3 J_B = Vector3.Cross(joints[1].transform.forward, (joints[joints.Length - 1].transform.position - joints[1].transform.position));
        Vector3 J_C = Vector3.Cross(joints[2].transform.forward, (joints[joints.Length - 1].transform.position - joints[2].transform.position));

        /*Vector3 J_A1 = Vector3.Cross(joints[0].transform.right, (joints[joints.Length - 1].transform.position - joints[0].transform.position));
        Vector3 J_B1 = Vector3.Cross(joints[1].transform.right, (joints[joints.Length - 1].transform.position - joints[1].transform.position));
        Vector3 J_C1 = Vector3.Cross(joints[2].transform.right, (joints[joints.Length - 1].transform.position - joints[2].transform.position));

        Vector3 J_A2 = Vector3.Cross(-joints[0].transform.right, (joints[joints.Length - 1].transform.position - joints[0].transform.position));
        Vector3 J_B2 = Vector3.Cross(-joints[1].transform.right, (joints[joints.Length - 1].transform.position - joints[1].transform.position));
        Vector3 J_C2 = Vector3.Cross(-joints[2].transform.right, (joints[joints.Length - 1].transform.position - joints[2].transform.position));*/

        //float[,] matrix = new float[9,9];
        float[,] matrix = new float[3, 3];
        matrix = MatrixTools.PopulateMatrix(matrix, new Vector3[] { J_A, J_B, J_C, /*J_A1, J_B1, J_C1, J_A2, J_B2, J_C2*/ });
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

    private void rotateLinks(float[] angleDiff)
    {
        float[] displayAngles = new float[angleDiff.Length];

        for (int i = 0; i < joints.Length - 1; i++)
        {
            Vector3 upDir = joints[i].transform.right;

            Vector3 crossAxis = Vector3.Cross(upDir, (joints[i + 1].transform.position - joints[i].transform.position).normalized);
            float currAngle = calculateAngle(Vector3.up, joints[i + 1].transform.position, joints[i].transform.position);
            float newAngle = angleDiff[i];
            displayAngles[i] = angleDiff[i] + currAngle;

            if (newAngle != 0)
                joints[i].transform.RotateAround(joints[i].transform.position, crossAxis, newAngle);
            //if newAngle >= 0 ActivateMuscles(leftMuscles[i])
            //else ActivateMuscles(rightMuscles[i])


            //if (i < joints.Length - 2)
                //updateLinkPos(i, joints[i].transform.position, crossAxis, angleDiff[i]);
            if (i >= joints.Length - 2) // end effector
                joints[i + 1].transform.position = jointRots[i].transform.position;

            //Debug.Log("joint " + (i + 1).ToString() + ": New angle Value: " + angleDiff[i].ToString());
        }
    }

    private void rotateLinks2(float[] angleDiff)
    {
        float[] displayAngles = new float[angleDiff.Length];

        for (int i = 0; i < joints.Length - 1; i++)
        {
            Vector3 upDir = joints[i].transform.right;

            Vector3 crossAxis = Vector3.Cross(upDir, (joints[i + 1].transform.position - joints[i].transform.position).normalized);
            float currAngle = calculateAngle(Vector3.up, joints[i + 1].transform.position, joints[i].transform.position);
            float newAngle = angleDiff[i];
            displayAngles[i] = angleDiff[i] + currAngle;

            ActivateMuscles2(angleDiff[i], i);
        }
    }

    private void ActivateMuscles2(float angle, int index)
    {
        for(int i = 0; i < pairs.Length; i++)
        {
            MusclePair p = pairs[i];

            Debug.Log(i);
            if(i == index)
            {
                if (angle <= 0)
                {
                    MuscleWithAnim toActivate = p.muscles[0];
                    float length = GetMuscleLengthFromAngle(toActivate, angle);
                    toActivate.targetLength = length;
                    Debug.Log("LENGHT: " + length);
                    toActivate.Activate();
                }
                else
                {
                    MuscleWithAnim toActivate = p.muscles[1];
                    float length = GetMuscleLengthFromAngle(toActivate, angle);
                    toActivate.targetLength = length;
                    Debug.Log("LENGHT: " + length);
                    toActivate.Activate();
                }  
            }
        }
    }

    private float GetMuscleLengthFromAngle(MuscleWithAnim m, float newAngle)
    {
        Rigidbody rb1 = m.GetComponent<MuscleWithAnim>().rb1;

        Transform a1 = m.GetComponent<MuscleWithAnim>().a1;
        Transform a2 = m.GetComponent<MuscleWithAnim>().a2;

        float triangleSideB = (rb1.GetComponent<CharacterJoint>().anchor - a1.position).magnitude;
        float triangleSideC = (rb1.GetComponent<CharacterJoint>().connectedAnchor - a2.position).magnitude;

        float triangleSideA;
        float angle = newAngle;

        //find hypotenuse using law of cosines
        //a^2 = b^2 + c^2 - 2bc cos(A)
        float triangleSideASqr = Mathf.Pow(triangleSideB, 2) + Mathf.Pow(triangleSideC, 2) - (2 * triangleSideB * triangleSideC * Mathf.Cos(angle));
        triangleSideA = Mathf.Sqrt(triangleSideASqr);

        return triangleSideA;
    }

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
public struct MusclePair
{
    [SerializeField]
    public MuscleWithAnim[] muscles;
}
