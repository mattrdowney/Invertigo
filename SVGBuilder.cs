using System;
using System.IO;

using UnityEngine;
using System.Collections;

public class SVGBuilder : MonoBehaviour //DOESN'T need to be MonoBehaviour
{
    static StreamWriter writer;
    static bool first;

	public void Start ()
    {
        WriteHeader();
	}

    public void OnDestroy()
    {
        WriteFooter();
    }

    public static void BeginShape()
    {
        first = true;
        writer.Write("\t<path d=\"");
    }

    public static void SetEdge(QuadraticBezier curve)
    {
        curve.begin_UV *= 1000; // XXX: altering the data isn't a great idea
        curve.end_UV *= 1000;

        if (first)
        {
            writer.Write("M" + curve.begin_UV.x + "," + curve.begin_UV.y);
            first = false;
        }
        writer.Write(" Q" + curve.control_point.x + "," + curve.control_point.y + " " + curve.end_UV.x + "," + curve.end_UV.y);
    }

    public static void EndShape()
    {
        writer.Write("\"/>\n\n\n");
    }

    static void WriteHeader()
    {
        writer = new StreamWriter("C:/Users/Matt Downey/Documents/Invertigo/Assets/TestSVG.svg");
        writer.Write("<svg width=\"1000\" height=\"1000\">\n\n");
    }

    static void WriteFooter()
    {
        writer.Write("\n</svg>");
        writer.Flush();
    }
}