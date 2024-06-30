using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSizeAdjuster : MonoBehaviour
{
	private float defaultSize = 4.4f;
	private float defaultAspect = 1.77777f;
	private Camera _camera;
	// Start is called before the first frame update
	void Start()
	{
		_camera = GetComponent<Camera>();
		float aspect = (float)Screen.height/(float)Screen.width  ;
		_camera.orthographicSize = defaultSize*(aspect/defaultAspect);
	}

}
