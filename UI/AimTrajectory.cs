using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimTrajectory : MonoBehaviour
{
	public Transform PointA;
	public Transform PointB;


	LineRenderer lineRenderer;
	public int NumSegments = 10;
	public float Amplitude = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
		lineRenderer = GetComponent<LineRenderer>();
	}

	List<Vector3> GeneratePoints(int numSegments, float a)
	{
		List<Vector3> points = new List<Vector3>();

		for (float x = -0.5f; x <= 0.5f; x += 1.0f/numSegments)
		{
			float y = a * (1 - 4.0f * x * x);
			points.Add(new Vector3(x, y, 0.0f));
		}

		return points;
	}

    // Update is called once per frame
    void Update()
    {
		lineRenderer.SetPositions(GeneratePoints(NumSegments, Amplitude).ToArray());
	}
}
