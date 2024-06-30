using UnityEngine;

public class BackgroundSizeHandler : MonoBehaviour
{
	private const float default_aspect = 1.777f;

	private void Awake()
	{
		float aspect = (float)Screen.height/(float)Screen.width  ;
		transform.localScale=Vector3.one*(aspect)/default_aspect;
	}
}
