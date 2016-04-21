using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Transform))]
public class Transform2sD : MonoBehaviour
{
    public Transform self { get; private set; }

	// Use this for initialization
	void Start ()
    {
        self = this.gameObject.GetComponent<Transform>();
	}
	
    Vector2 position
    {
        get
        {
            return SphereUtility.SphericalPosition(self.position);
        }
        set
        {
            self.position = SphereUtility.Position(value);
            self.LookAt(self.position);
        }
    }

    float rotation
    {
        get
        {
            return 0;
        }
        set
        {

        }
    }
}
