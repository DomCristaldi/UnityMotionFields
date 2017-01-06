using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

using System.IO;
#endif

[System.Serializable]
public class TaskSettings
{

    public ATask task;

    //[Range(0.0f, 1.0f)]
    public float contributionScale;


}

[CreateAssetMenu]
public class TaskArrayInfo : ScriptableObject {

	public string storageFolderLocation;

	public List<ATask> TaskArray;

    public List<TaskSettings> tasks;

    public void ComputeReward(MotionPose pose, ref candidatePose newPose, Transform targetLocation)
    {
        newPose.reward = 0.0f;

        foreach (ATask task in TaskArray)
        {
            float taskReward = task.CheckReward(pose, newPose.pose, targetLocation);

            //Debug.LogFormat("Task: {0} - Value: {1}", task.name, taskReward);

            newPose.reward += taskReward;
        }
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(TaskArrayInfo))]
public class TaskArrayInfo_Editor : Editor {

    float heightPadding = 20.0f;

	private TaskArrayInfo selfScript;

	private ReorderableList reorderATaskList;

	void OnEnable() {
		selfScript = (TaskArrayInfo)target;

		reorderATaskList = new ReorderableList (serializedObject,
			serializedObject.FindProperty ("tasks"),
			true, true, true, true);

        reorderATaskList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {

            SerializedProperty indexProp = reorderATaskList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(rect,
                                    reorderATaskList.serializedProperty.GetArrayElementAtIndex(index),
                                    new GUIContent(),
                                    true);

            rect.y += EditorGUI.GetPropertyHeight(reorderATaskList.serializedProperty.GetArrayElementAtIndex(index));

            //EditorGUI.Slider(rect, 0.5f, 1.0f, 0.0f);
            //EditorGUI.FloatField(rect,
            //                     selfScript.TaskArray[index].contributionScale);

            /*
            selfScript.TaskArray[index].contributionScale = 
                EditorGUI.Slider(rect,
                                 selfScript.TaskArray[index].contributionScale,
                                 0.0f,
                                 1.0f);
            */
        };

        reorderATaskList.elementHeightCallback = (int index) => {
            return (EditorGUI.GetPropertyHeight(reorderATaskList.serializedProperty.GetArrayElementAtIndex(index)))
                   + heightPadding;
        };

        //reorderATaskList.onAddDropdownCallback = (Rect buttonRect, ReorderableList selfList) => {

        //	GenericMenu menu = new GenericMenu();
        //	string[] guids = AssetDatabase.FindAssets("", new[] {"Assets/" + selfScript.storageFolderLocation});

        //	foreach (string guid in guids) {
        //		if (AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(ATask)) as ATask == null) {continue;}

        //		string path = AssetDatabase.GUIDToAssetPath(guid);
        //		menu.AddItem(new GUIContent(Path.GetFileNameWithoutExtension(path)),
        //			false,
        //			null);
        //	}

        //	menu.ShowAsContext();
        //};

    }

	public override void OnInspectorGUI()
	{
		//base.OnInspectorGUI ();
        //return;

        serializedObject.Update();
        reorderATaskList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
