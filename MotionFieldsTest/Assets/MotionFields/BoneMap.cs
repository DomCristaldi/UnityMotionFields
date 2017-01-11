using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace AnimationMotionFields
{

[CreateAssetMenu(menuName = "Motion Fields/Bone Map")]
public class BoneMap : ScriptableObject
{
    public CosmeticSkeleton testSkel;

    [System.Serializable]
    public class BoneLabel
    {
        public string label;
             
    }

    public List<BoneLabel> boneLabels;


    public void AddBoneLabel(string labelName)
    {
        
    }
}

}