using UnityEngine;
using System.Collections;

public class LevelToSVG : MonoBehaviour
{
    bool done = false;

    public void Update()
    {
        if (!done)
        {
            GameObject[] shapes = GameObject.FindGameObjectsWithTag("Shape");
            foreach (GameObject shape in shapes)
            {
                ShapeToSVG.Append(shape);
            }
            done = true;
        }
    }
}
