using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AnimationMotionFields;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestSerializableClassRef : MonoBehaviour {

    public BoneMap testMap;

    public BoneMap assignedMap;

    [Space]
    public int targetLabelIndex;
    public BoneMap.BoneLabel targetLabel;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!assignedMap) { targetLabelIndex = 0; }
        else { targetLabelIndex = Mathf.Clamp(targetLabelIndex, 0, assignedMap.boneLabels.Count - 1); }
    }
#endif

}

#if UNITY_EDITOR
[CustomEditor(typeof(TestSerializableClassRef))]
public class TestSerializableClassRef_Editor : Editor
{
    TestSerializableClassRef selfScript;

    void OnEnable()
    {
        selfScript = (TestSerializableClassRef)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Draw_BoneLabelAssignmentButton();

    }

    private void Draw_BoneLabelAssignmentButton()
    {
        if (!selfScript) { return; }
        if (!selfScript.assignedMap) 
        {
            using(var missingBoneMap = new EditorGUILayout.VerticalScope("Box")) 
            {
                EditorGUILayout.HelpBox("Missing Bone Map", MessageType.Error);
            }
            return;
        }
        //if (selfScript.targetLabelIndex >= selfScript.assignedMap.boneLabels.Count)
        //{

        //}

        using(var boneLabelAssignmentDrawerScope = new EditorGUILayout.VerticalScope("Box")) 
        {
            EditorGUILayout.BeginHorizontal();

            selfScript.targetLabelIndex = EditorGUILayout.IntField("Index: ", selfScript.targetLabelIndex);

            selfScript.targetLabelIndex = Mathf.Clamp(selfScript.targetLabelIndex,
                                                      0,
                                                      selfScript.assignedMap.boneLabels.Count - 1);

            if (GUILayout.Button("Assign From Index")) 
            {
                BoneMap.BoneLabel assigningLabel = selfScript.assignedMap.boneLabels[selfScript.targetLabelIndex];
                if (assigningLabel != null) 
                {
                    selfScript.targetLabel = assigningLabel;
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

}
#endif