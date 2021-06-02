using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IK_Test_4 : MonoBehaviour
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

        List<float> tempAngels = new List<float>();
        for (int i = 0; i < rigidbodyList.Count - 1; i++)
        {   //YZ
            Vector3 body1 = rigidbodyList[i].transform.position;
            Vector3 body2 = rigidbodyList[i + 1].transform.position;
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

        //rotateLinks2(angleDiff);
        rotateLinks(angleDiff);

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
        return new float[] { dO[0, 0], dO[1, 0], dO[2, 0] };
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
        for (int i = 0; i < rigidbodyList.Count - 1; i++)
        {   //YZ
            Transform body1 = rigidbodyList[i].transform;
            Transform body2 = rigidbodyList[i + 1].transform;
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

    private void rotateLinks(float[] angleDiff)
    {
        float[] displayAngles = new float[angleDiff.Length];

        for (int i = 0; i < rigidbodyList.Count - 1; i++)
        {
            Vector3 upDir = rigidbodyList[i].transform.right;

            Vector3 crossAxis = Vector3.Cross(upDir, (rigidbodyList[i + 1].transform.position - rigidbodyList[i].transform.position).normalized);
            float currAngle = calculateAngle(Vector3.up, rigidbodyList[i + 1].transform.position, rigidbodyList[i].transform.position);
            float newAngle = angleDiff[i];
            displayAngles[i] = angleDiff[i] + currAngle;

            if (newAngle != 0)
            {
                Debug.Log("NEWANGLE: " + angleDiff[i]);
                Debug.Log("JOINT TO ROTATE: " + rigidbodyList[i]);
                rigidbodyList[i].transform.RotateAround(rigidbodyList[i].transform.position, crossAxis, newAngle);
            }
            else
            {
                Debug.Log("ANGLE IS 0!!!!");
            }
                

            //Debug.Log("joint " + (i + 1).ToString() + ": New angle Value: " + angleDiff[i].ToString());
        }
    }
}


