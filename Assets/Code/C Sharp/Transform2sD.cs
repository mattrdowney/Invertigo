using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Transform))]
public class Transform2sD : MonoBehaviour
{
    Transform self;

	// Use this for initialization
	void Start ()
    {
        self = this.gameObject.GetComponent<Transform>();
	}
	
    Vector2 position
    {
        get
        {
            return SphereUtility.CartesianToSphere(self.position);
        }
        set
        {
            self.position = SphereUtility.SphereToCartesian(value);
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
