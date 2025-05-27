using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
struct Circle
{
	public Vector2 Center;
	public float Radius;
};

struct Box
{
	public Vector2 Center;
	public Vector2 Size;
}

public class Master : MonoBehaviour
{
	Vector2 rayStart;
	Vector2 rayEnd;

	List<Circle> circles;
	List<Box> boxes;
	
	void Start()
	{
		rayStart = Vector2.zero;
		rayEnd = Vector2.zero;

		circles = new List<Circle>();
		boxes = new List<Box>();

		GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		foreach (GameObject go in allObjects)
		{
			if (go.name.StartsWith("Circle"))
			{
				Circle circle = new Circle();
				circle.Center = go.transform.position;
				circle.Radius = go.transform.localScale.x * 0.5f;
				circles.Add(circle);
			}
			else if (go.name.StartsWith("Box"))
			{
				Box box = new Box();
				box.Center = go.transform.position;
				box.Size = go.transform.localScale;
				boxes.Add(box);
			}
		}
	}

	void Update()
	{
		Vector3 mouseScreenPos = Input.mousePosition;
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

		rayEnd = mouseWorldPos;
		if (Input.GetMouseButtonDown(0))
		{
			rayStart = mouseWorldPos;
		}
	}

	void OnDrawGizmos()
	{
		if (!Application.isPlaying)
			return;

		Gizmos.color = Color.white;

		Vector3 startPos = rayStart;
		Vector3 direction = (rayEnd - rayStart).normalized;
		float totalDist = 0;
		for (int i = 0; i < 1000; ++i)
		{
			float signedDist = calcSignedDistToScene(startPos);
			drawGizmosCircle(startPos, signedDist);

			startPos = startPos + direction * signedDist;
			totalDist += signedDist;

			Vector3 viewportPos = Camera.main.WorldToViewportPoint(startPos);
			if (!(viewportPos.z > 0 &&
				viewportPos.x >= 0 && viewportPos.x <= 1 &&
				viewportPos.y >= 0 && viewportPos.y <= 1))
				break;
		}

		Gizmos.DrawSphere(rayStart, 0.1f);
		Gizmos.DrawRay(rayStart, direction * totalDist);
	}

	float calcSignedDistToScene(Vector2 p)
	{
		float distToScene = float.MaxValue;

		foreach (Circle circle in circles)
		{
			float distToCircle = calcSignedDistToCircle(p, circle);
			distToScene = Mathf.Min(distToScene, distToCircle);
		}

		foreach (Box box in boxes)
		{
			float distToBox = calcSignedDistToBox(p, box);
			distToScene = Mathf.Min(distToScene, distToBox);
		}

		return distToScene;
	}

	float calcSignedDistToCircle(Vector2 p, Circle circle)
	{
		return (circle.Center - p).magnitude - circle.Radius;
	}

	float calcSignedDistToBox(Vector2 p, Box box)
	{
		Vector2 halfSize = box.Size * 0.5f;
		Vector2 offset = (p - box.Center).Abs() - halfSize;

		Vector2 maxOffset = Vector2.Max(offset, Vector2.zero);
		float distOutside = maxOffset.magnitude;

		float distInside = Mathf.Min(Mathf.Max(offset.x, offset.y), 0.0f);

		return distOutside + distInside;
	}

	void drawGizmosCircle(Vector3 center, float radius)
	{
		const int NUM_SEGMENTS = 36;
		const float ANGLE_STEP = 360f / NUM_SEGMENTS;

		Vector3 prevPoint = center + new Vector3(radius, 0, 0);
		for (int i = 1; i <= NUM_SEGMENTS; i++)
		{
			float angle = ANGLE_STEP * i * Mathf.Deg2Rad;
			Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

			Gizmos.DrawLine(prevPoint, newPoint);
			prevPoint = newPoint;
		}
	}
}
