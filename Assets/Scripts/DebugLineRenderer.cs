using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component with methods to draw debug lines
/// </summary>
public class DebugLineRenderer : MonoBehaviour {

    public Material[] materials;

    private class LineInfo
    {
        public Material Material { get; private set; }
        public Vector3 Start { get; private set; }
        public Vector3 End { get; private set; }

        public LineInfo(Material mat, Vector3 start, Vector3 end)
        {
            Material = mat;
            Start = start;
            End = end;
        }
    }

    private List<LineInfo> lines;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DrawLine(int matIndex, Vector3 start, Vector3 end)
    {
        lines.Add(new LineInfo(materials[matIndex], start, end));
    }

    private void OnRenderObject()
    {
        foreach (LineInfo line in lines)
        {
            line.Material.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(line.Start);
            GL.Vertex(line.End);
            GL.End();
        }
        lines.Clear();
    }
}
