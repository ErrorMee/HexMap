using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teamer : MonoBehaviour
{
	private UnitAnimation unitAnimation;

	const float rotationSpeed = 720f;

	float orientation;
	public float Orientation
	{
		get
		{
			return orientation;
		}
		set
		{
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	public IEnumerator LookAt(Vector3 point)
	{
		if (HexMetrics.Wrapping)
		{
			float xDistance = point.x - transform.localPosition.x;
			if (xDistance < -HexMetrics.innerRadius * HexMetrics.wrapSize)
			{
				point.x += HexMetrics.innerDiameter * HexMetrics.wrapSize;
			}
			else if (xDistance > HexMetrics.innerRadius * HexMetrics.wrapSize)
			{
				point.x -= HexMetrics.innerDiameter * HexMetrics.wrapSize;
			}
		}

		point.y = transform.localPosition.y;
		Quaternion fromRotation = transform.localRotation;
		Quaternion toRotation =
			Quaternion.LookRotation(point - transform.localPosition);
		float angle = Quaternion.Angle(fromRotation, toRotation);

		if (angle > 0f)
		{
			float speed = rotationSpeed / angle;
			for (
				float t = Time.deltaTime * speed;
				t < 1f;
				t += Time.deltaTime * speed
			)
			{
				transform.localRotation =
					Quaternion.Slerp(fromRotation, toRotation, t);
				yield return null;
			}
		}
		if (unitAnimation)
		{
			unitAnimation.Move(true);
		}
		transform.LookAt(point);
		orientation = transform.localRotation.eulerAngles.y;
	}

	void OnEnable()
	{
		unitAnimation = GetComponent<UnitAnimation>();
	}

	public void Idle()
	{
		unitAnimation.Move(false);
	}
		 
}
