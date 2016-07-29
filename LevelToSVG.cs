using UnityEngine;
using System.Collections;

public class LevelToSVG : MonoBehaviour
{
    bool done = false;

    public void Update()
    {
        Debug.Log("lvl2");
        if (!done)
        {
            GameObject[] shapes = GameObject.FindGameObjectsWithTag("Shape");
            foreach (GameObject shape in shapes)
            {
                Debug.Log("Printing Shape");
                ShapeToSVG.Append(shape);
            }
            Debug.Log(shapes.Length);
            done = true;
        }
    }
}
