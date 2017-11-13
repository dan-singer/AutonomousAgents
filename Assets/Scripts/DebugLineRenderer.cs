using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component with methods to draw debug lines
/// </summary>
public class DebugLineRenderer : MonoBehaviour {

    public Material[] materials;
    public GameObject debugShape;

    private Renderer debugShapeRend;

    private static event Action<bool> DrawChanged;

    private static bool draw = false;
    public static bool Draw
    {
        get
        {
            return draw;
        }
        set
        {
            draw = value;
            if (DrawChanged != null)
                DrawChanged(draw);
        }
    }

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
        lines = new List<LineInfo>();
        debugShapeRend = debugShape.GetComponent<Renderer>();
        DrawChanged += (draw) => {
            if (debugShapeRend != null)
                debugShapeRend.enabled = draw;
        };
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DrawLine(int matIndex, Vector3 start, Vector3 end)
    {
        lines.Add(new LineInfo(materials[matIndex], start, end));
    }

    public void SetShapeLocation(Vector3 loc)
    {
        if (debugShape)
            debugShape.transform.position = loc;
    }

    private void OnRenderObject()
    {
        if (!Draw)
            return;
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
