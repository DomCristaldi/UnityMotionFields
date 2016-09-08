using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.Director;

// Generate random animation graph over time, with no actual content.
public class GenerateAndGrowGraph : MonoBehaviour
{
    AnimationMixerPlayable m_Root;

    // Keeping a list of instanciated mixers so we can pick one randomly
    // without having to navigate the graph
    List<AnimationMixerPlayable> m_Mixers = new List<AnimationMixerPlayable>();

    const float k_ProportionOfLeaves = 0.5f;

    private void Start()
    {
        InitializeGraph();
        InvokeRepeating("AddNodeRandomly", 1, 1f);
    }

    private void OnEnable()
    {
        InitializeGraph();
    }

    private void InitializeGraph()
    {
        m_Root = AnimationMixerPlayable.Create();
        m_Mixers.Clear();
        m_Mixers.Add(m_Root);
    }

    private void AddNodeRandomly()
    {
        // Pick one mixer randomly, and create either another mixer or a leaf
        int parentIndex = Random.Range(0, m_Mixers.Count);
        AnimationMixerPlayable parent = m_Mixers[parentIndex];
        AnimationPlayable newNode;

        if (Random.value > k_ProportionOfLeaves)
        {
            var temp = AnimationMixerPlayable.Create();
            newNode = temp;
            m_Mixers.Add(temp);
        }
        else
        {
            newNode = AnimationClipPlayable.Create(null);
        }

        parent.AddInput(newNode);
        parent.SetInputWeight(parent.inputCount - 1, Random.value);

        // Call this to visualize the graph in the graph visualizer. Will only be effective if window is open.
        GraphVisualizerClient.Show(m_Root, gameObject.name);
    }
}
