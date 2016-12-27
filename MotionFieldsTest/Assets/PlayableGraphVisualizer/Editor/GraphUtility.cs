using UnityEngine;
using UnityEngine.Experimental.Director;

// Interface for a generic layout algorithm that takes the root of a Playable graph and places its nodes and
// edges in a graphically pleasing and efficient way. Algorithm will provide a graph with a minimum spacing of
// 1 unit between nodes and between levels.
public interface ILayoutAlgorithm
{
    Vertex[] vertices
    {
        get;
    }

    Edge[] edges
    {
        get;
    }

    void Layout(Playable root);
};

// Interface for rendering a tree layout to screen.
public interface ITreeRenderer
{
    void Draw(ILayoutAlgorithm layout, Rect drawingArea);
    void Draw(ILayoutAlgorithm layout, Rect drawingArea, NodeConstraints nodeConstraints);
};

// One vertex is associated to each node in the graph
public class Vertex
{
    // center of the node in the graph layout
    public Vector2 position;

    // weight of this vertex relative to its siblings
    public float weight;

    // effective weight of the vertex in the graph. Takes the weight of its ancestors into account
    public float propagatedWeight;

    // Content represented by the node, to allow custom information to be drawn
    public object payload;
};

// One edge is associated to each connection in the graph
public class Edge
{
    // s, d: indices in the vertex array of the layout algorithm
    public Edge(int s, int d)
    {
        source = s;
        destination = d;
    }

    public int source;
    public int destination;
};

// Customization of how the graph nodes should be rendered (size, distances and proportions)
public struct NodeConstraints
{
    // In layout units. If 1, node will be drawn as large as possible to avoid overlapping, and to respect aspect ratio
    public float maximumNormalizedNodeSize;

    // when the graph is very simple, the nodes can seem disproportionate relative to the graph. This limits their size
    public float maximumNodeSizeInPixels;

    // width / height; 1 represents a square node
    public float aspectRatio;
}
