using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameDirector_CarGame : MonoBehaviour
{
    GameObject car;
    GameObject flag;
    GameObject distance;

    private void Start()
    {
        this.car = GameObject.Find("car");
        this.flag = GameObject.Find("flag");
        this.distance = GameObject.Find("Distance");
    }
    private void Update()
    {
        float length = this.flag.transform.position.x - this.car.transform.position.x;
        if (length >= 0)
        {
            this.distance.GetComponent<Text>().text = "목표 지점까지 " + length.ToString("F2") + " m";
        }
        else
        {
            this.distance.GetComponent<Text>().text = "게임 오버!";
        }
    }
}
