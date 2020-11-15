using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
	public static float Distance(Bounds bounds, Vector3 p)
	{
		return (p - bounds.ClosestPoint(p)).magnitude;

		//var dx = Mathf.Max(rect.min.x - p.x, 0, p.x - rect.max.x);
		//var dy = Mathf.Max(rect.min.y - p.y, 0, p.y - rect.max.y);
		//return Mathf.Sqrt(dx * dx + dy * dy);
	}
}
