using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector_AAG : MonoBehaviour
{
    GameObject hpGauge;

	[Header("Screen Orientation Settings")]
	public ScreenOrientation targetOrientation = ScreenOrientation.LandscapeLeft;

	void Start()
    {
		Debug.Log($"Applying screen settings for scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
		Screen.orientation = targetOrientation;
		this.hpGauge = GameObject.Find("hpGauge");
    }

    public void DecreaseHp()
    {
        this.hpGauge.GetComponent<Image>().fillAmount -= 0.1f;
    }
}
