using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

using System.IO;
#endif


[CreateAssetMenu]
public class TaskArrayInfo : ScriptableObject {

	public string storageFolderLocation;

	public List<ATask> TaskArray;

}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(TaskArrayInfo))]
public class TaskArrayInfo_Editor : Editor {

	private TaskArrayInfo selfScript;

	private ReorderableList reorderATaskList;

	void OnEnable() {
		selfScript = (TaskArrayInfo)target;

		reorderATaskList = new ReorderableList (serializedObject,
			serializedObject.FindProperty ("TaskArray"),
			true, true, true, true);

		reorderATaskList.onAddDropdownCallback = (Rect buttonRect, ReorderableList selfList) => {

			GenericMenu menu = new GenericMenu();
			string[] guids = AssetDatabase.FindAssets("", new[] {"Assets/" + selfScript.storageFolderLocation});

			foreach (string guid in guids) {
				if (AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(ATask)) as ATask == null) {continue;}

				string path = AssetDatabase.GUIDToAssetPath(guid);
				menu.AddItem(new GUIContent(Path.GetFileNameWithoutExtension(path)),
					false,
					null);
			}

			menu.ShowAsContext();
		};
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();
        return;

		serializedObject.Update();
		reorderATaskList.DoLayoutList();
		serializedObject.ApplyModifiedProperties();
	}

}
#endif
