using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToClimbCloud : MonoBehaviour
{
	public string climbCloudSceneName = "ClimbCloud";

	public void LoadClimbCloud()
	{
		SceneManager.LoadScene(climbCloudSceneName);
	}
}
