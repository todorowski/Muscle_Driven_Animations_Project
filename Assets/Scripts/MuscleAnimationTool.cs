using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MuscleAnimationTool : MonoBehaviour
{
    public MuscleAnimation animationObject;
    public Transform ragdollParent;
    public Button keyFrameButton;

    //Start of the animation time line
    public float animationHead = 0.0f;

    //Optimized data
    private List<MuscleWithAnim> characterMuscles = new List<MuscleWithAnim>();
    private Dictionary<MuscleWithAnim, int> muscleIndexDictonary = new Dictionary<MuscleWithAnim, int>();

    void Start()
    {
        //Get all character muscles!
        characterMuscles = ragdollParent.GetComponentsInChildren<MuscleWithAnim>().ToList();

        //Make character stiff
        Rigidbody[] bodies = ragdollParent.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody body in bodies)
        {
            body.drag = float.MaxValue;
            body.angularDrag = float.MaxValue;
        }

        //Initialize animation data
        animationObject.muscles = new List<MuscleAnimationStruct>();
        foreach (MuscleWithAnim muscle in characterMuscles)
        {
            animationObject.muscles.Add(new MuscleAnimationStruct(muscle.name, new AnimationCurve()));
            muscleIndexDictonary[muscle] = animationObject.muscles.Count - 1;
        }

        //UI
        keyFrameButton.onClick.AddListener(createKeyframe);
    }

    //Saves the lengths for every muscle in user created animation pose
    void createKeyframe()
    {
        foreach (MuscleWithAnim m in characterMuscles)
        {
            float length = (m.a1.transform.position - m.a2.transform.position).magnitude;
            animationObject.muscles[muscleIndexDictonary[m]].addKeyframe(animationHead, length);
        }
    }
}
