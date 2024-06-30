using UnityEngine;

public class CameraSizeAdjuster : MonoBehaviour
{
	private float _defaultCameraSize = 4.4f;
	private float defaultAspectRatio = 1.77777f;
	private Camera _camera;
	// Start is called before the first frame update
	void Start()
	{
		_camera = GetComponent<Camera>();
		float aspect = (float)Screen.height/(float)Screen.width  ;
		_camera.orthographicSize = _defaultCameraSize*(aspect/defaultAspectRatio);
	}

}
