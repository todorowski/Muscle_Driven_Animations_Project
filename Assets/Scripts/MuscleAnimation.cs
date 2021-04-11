using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "MuscleAnimation")]
public class MuscleAnimation : ScriptableObject
{
    [SerializeField]
    public List<MuscleAnimationStruct> muscles;
}
[System.Serializable]
public struct MuscleAnimationStruct
{
    public string muscleName;
    public AnimationCurve animCurve;

    public MuscleAnimationStruct(string name, AnimationCurve animation)
    {
        this.muscleName = name;
        this.animCurve = animation;
    }

    public void addKeyframe(float time, float length)
    {
        animCurve.AddKey(new Keyframe(time, length));
    }

}


