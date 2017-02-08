/** The primative spherical geometry component that is used to traverse a block or terrain
 *
 * TODO: detailed description
 * 
 * @file
 */

using UnityEngine;
using System.Diagnostics;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public abstract class ArcOfSphere /* : Component*/ : MonoBehaviour //TODO: get rid of this in production builds
{
	/*CONSIDER: make const*/
	[SerializeField] public ArcOfSphere						next;
	[SerializeField] public ArcOfSphere						prev;

	public abstract float AngularRadius(float radius);
	public float AngularRadius() { return AngularRadius(0); }

	public abstract float Begin(float radius);
	public float Begin() { return Begin(0); }

	protected abstract Vector3 Center(float radius);
	protected Vector3 Center() { return Center(0); }

	/** Determine if the character (represented by a point) is inside of a arc (extruded by the radius of the character)
	 *  
	 */
	public abstract bool Contains(Vector3 pos, float radius);

	/** Counts the number of booleans that are true in a comma separated list of booleans
	 * 
	 *  credit: http://stackoverflow.com/questions/377990/elegantly-determine-if-more-than-one-boolean-is-true
	 * 
	 *  @example "CountTrueBooleans(true, false, true, true);" will return 3
	 */
	protected static int CountTrueBooleans(params bool[] boolean_list) //allow for comma separated booleans using "params"
	{
		return boolean_list.Count(bIsTrue => bIsTrue); //count booleans that are true using Linq's .Count function
	}

	public abstract optional<float> Distance(Vector3 to, Vector3 from, float radius);

	public abstract float End(float radius);
	public float End() { return End(0); }

	/** return the position of the player based on the circular path
	 *  
	 */
	public abstract Vector3 Evaluate(float angle, float radius);
	public Vector3 Evaluate(float angle) { return Evaluate(angle, 0); }

	/** return the position of the player based on the circular path
	 * 
	 *  return the position of the player based on the circular path
	 *  If the player would go outside of [Begin(radius), End(radius)],
	 *  the arc should transfer control of the player to (prev, next) respectively
	 */
	public static Vector3 Evaluate(GroundInfo ground, float radius)
	{
		if(ground.angle >= ground.end)
		{
			ground.angle	-= ground.end; //FIXME: verbose, redundant, inelegant
			ground.angle	*= ground.height;
			ground.arc		 = ground.arc.next;
			ground.height	 = ground.arc.LengthRadius(radius);
			ground.angle	/= ground.height;
			ground.begin	 = ground.arc.Begin(radius);
			ground.end		 = ground.arc.End(radius);
			ground.angle	+= ground.begin;
			return Evaluate(ground, radius);
		}
		if(ground.angle < ground.begin)
		{
			ground.angle	-= ground.begin;
			ground.angle	*= ground.height;
			ground.arc		 = ground.arc.prev;
			ground.height	 = ground.arc.LengthRadius(radius);
			ground.begin	 = ground.arc.Begin(radius);
			ground.end		 = ground.arc.End(radius);
			ground.angle	/= ground.height;
			ground.angle	+= ground.end;
			return Evaluate(ground, radius);
		}
		
		return ground.arc.Evaluate(ground.angle, radius);
	}

    public abstract optional<float> CartesianToRadial(Vector3 position);

    public virtual Vector3 EvaluateLeft(float angle, float radius) { return -EvaluateRight(angle, radius); }
    public Vector3 EvaluateLeft(float angle) { return EvaluateLeft(angle, 0); }

    public abstract Vector3 EvaluateNormal(float angle, float radius);
	public Vector3 EvaluateNormal(float angle) { return EvaluateNormal(angle, 0); }

	public abstract Vector3 EvaluateRight(float angle, float radius);
	public Vector3 EvaluateRight(float angle) { return EvaluateRight(angle, 0); }

    /** Find the point of collision with an arc.
     */
    public optional<Vector3> Intersect(Vector3 to, Vector3 from, float radius)
    {
        optional<Vector3> result = SphereUtility.Intersection(from, to, Pole(), AngularRadius(radius));
        return result;
    }

    #if UNITY_EDITOR
    public void LinkBlock(Transform block_transform) { Undo.SetTransformParent(this.transform, block_transform, "Link arc to block"); }
    public void LinkBlock(ArcOfSphere other) { LinkBlock(other.gameObject.transform.parent); }
    #endif

    //CONSIDER: Can you "inversion of control" ClosestPoint and MaxGradient?
    public Vector3 ClosestPoint(Vector3 point) //TODO: eliminate code duplication with MaxGradient //HACK: may not work in all or even most cases
	{
		Vector3 closest_point = Vector3.zero;
		float min_distance = Mathf.Infinity;

		/** if we don't calculate per quadrant, calculations for an arc with angle 2*PI become ambiguous because left == right
		 */ 
		float quadrants = Mathf.Ceil(this.End() / (Mathf.PI / 2f)); //maximum of 4, always integral, float for casting "left" and "right"
		for(float quadrant = 0; quadrant < quadrants; ++quadrant)
		{
			float left  = this.End()*( quadrant      / quadrants); //get beginning of quadrant i.e. 0.00,0.25,0.50,0.75
			float right = this.End()*((quadrant + 1) / quadrants); //get    end    of quadrant i.e. 0.25,0.50,0.75,1.00
			
			float left_distance  = (this.Evaluate(left) - point).sqrMagnitude; //find the correlation factor between left and the desired direction
			float right_distance = (this.Evaluate(right) - point).sqrMagnitude;

			/** this is basically a binary search
			 * 
			 *  1) take the left and right vectors and compute the (squared) distance between the desired point.
			 *  2) take the larger distance and ignore that half of the remaining arc
			 */
			for(int iteration = 0; iteration < 8*sizeof(float); ++iteration) //because we are dealing with floats, more precision could help (or hurt?)
			{
				float midpoint = (left + right) / 2;
				if(right_distance > left_distance) //is the right vector further from the desired point?
				{
					right = midpoint; //throw out the right half if the left vector is closer
                    right_distance = (this.Evaluate(right) - point).sqrMagnitude;
                }
				else
				{
					left = midpoint; //throw out the left half if the right vector is closer
                    left_distance  = (this.Evaluate(left) - point).sqrMagnitude;
                }
			}
			
			/** figure out if this quadrant contains a closer point
			 */
			if(min_distance > right_distance)
			{
				closest_point = this.Evaluate(right);
				min_distance = right_distance;
			}
			if(min_distance > left_distance)
			{
                closest_point = this.Evaluate(left);
                min_distance = left_distance;
			}
		}
		return closest_point;
	}

    public abstract float LengthRadius(float radius);
	public float LengthRadius() { return LengthRadius(0); }

    protected static Vector3 MaxGradient(ArcOfSphere arc, Vector3 desired)
    {
        Vector3 max_gradient = Vector3.zero;
        float max_product = Mathf.NegativeInfinity;

        /** if we don't calculate per quadrant, calculations for an arc with angle 2*PI become ambiguous because left == right
		 */
        float quadrants = Mathf.Ceil(arc.End() / (Mathf.PI / 2f)); //maximum of 4, always integral, float for casting "left" and "right"
        for (float quadrant = 0; quadrant < quadrants; ++quadrant)
        {
            float left = arc.End() * (quadrant / quadrants); //get beginning of quadrant i.e. 0.00,0.25,0.50,0.75
            float right = arc.End() * ((quadrant + 1) / quadrants); //get    end    of quadrant i.e. 0.25,0.50,0.75,1.00

            float left_product = Vector3.Dot(arc.Evaluate(left), desired); //find the correlation factor between left and the desired direction
            float right_product = Vector3.Dot(arc.Evaluate(right), desired);

            /** this is basically a binary search
			 * 
			 *  1) take the left and right vectors and compute their dot products with the desired direction.
			 *  2) take the lesser dot product and ignore that half of the remaining arc
			 */
            for (int iteration = 0; iteration < 8 * sizeof(float); ++iteration) //because we are dealing with floats, more precision could help (or hurt?)
            {
                float midpoint = (left + right) / 2;
                if (left_product < right_product) //is the right vector closer to the desired direction?
                {
                    left = midpoint; //throw out the left half if the right vector is closer
                    left_product = Vector3.Dot(arc.Evaluate(left), desired);
                }
                else
                {
                    right = midpoint; //throw out the right half if the left vector is closer
                    right_product = Vector3.Dot(arc.Evaluate(right), desired);
                }
            }

            /** figure out if this quadrant contains a larger gradient
			 */
            if (max_product < right_product)
            {
                max_gradient = arc.Evaluate(right);
                max_product = right_product;
            }
            if (max_product < left_product)
            {
                max_gradient = arc.Evaluate(left);
                max_product = left_product;
            }
        }
        return max_gradient;
    }

    protected abstract Vector3 Pole();

    /** Create a AABB that perfectly contains a circular arc
	 * 
	 *  TODO: detailed description and math link
	 * 
	 *  TODO: Ex. 
	 * 
	 *  @param collider the box collider that will be altered to contain the ArcOfSphere
	 */
    protected static void RecalculateAABB(ArcOfSphere arc)
	{
		float x_min = MaxGradient(arc, Vector3.left   ).x;
		float x_max = MaxGradient(arc, Vector3.right  ).x;
		float y_min = MaxGradient(arc, Vector3.down   ).y;
		float y_max = MaxGradient(arc, Vector3.up     ).y;
		float z_min = MaxGradient(arc, Vector3.back   ).z;
		float z_max = MaxGradient(arc, Vector3.forward).z;

		arc.transform.position = new Vector3((x_max + x_min) / 2,
		                    			 	 (y_max + y_min) / 2,
		                            	 	 (z_max + z_min) / 2);

		BoxCollider	collider = arc.GetComponent<BoxCollider>();
		collider.size   = new Vector3(x_max - x_min,
		                              y_max - y_min,
		                              z_max - z_min);
	}

	public ArcOfSphere Relink(ArcOfSphere left, ArcOfSphere right)
	{
        #if UNITY_EDITOR
        this.Save();
		left.Save();
		right.Save();
        #endif

        this.next  = right;
		this.prev  = left;

		left.next  = this;
		right.prev = this;

		return this;
	}


    #if UNITY_EDITOR
    public virtual void Save()
	{
		Undo.RecordObject(this, "Save arc of sphere");
		Undo.RecordObject(this.transform, "Save transform");
		Undo.RecordObject(this.GetComponent<BoxCollider>(), "Save box collider");
	}
    #endif
}