using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class MuscleAnimationEditor : Editor
{
    public GameObject ragdoll;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Write Current Pose to Animation Object"))
        {
            
        }
    }
}
