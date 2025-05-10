using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuDirector : MonoBehaviour
{
	[Header("Screen Orientation Settings")]
	public ScreenOrientation targetOrientation = ScreenOrientation.Portrait;

	void Start()
	{
		Debug.Log($"Applying screen settings for scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
		Screen.orientation = targetOrientation;
	}
}