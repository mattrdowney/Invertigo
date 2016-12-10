using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (first)
        {
            writer.Write("M" + curve.begin_UV.x * 1000 + "," + curve.begin_UV.y * 1000);
            first = false;
        }
        writer.Write(" Q" + curve.control_point.x * 1000 + "," + curve.control_point.y * 1000 + " " + curve.end_UV.x * 1000 + "," + curve.end_UV.y * 1000);
    }

    public static void EndShape()
    {
        writer.Write("\"/>\n\n\n");
    }

    static void WriteHeader()
    {
        writer = new StreamWriter("C:/Users/Matt Downey/Documents/Invertigo/Assets/Art/Scalable Vector Graphics/" + SceneManager.GetActiveScene().buildIndex + ".svg");
        writer.Write("<svg width=\"1000\" height=\"1000\">\n\n");
    }

    static void WriteFooter()
    {
        writer.Write("\n</svg>");
        writer.Flush();
        writer.Close();
    }
}