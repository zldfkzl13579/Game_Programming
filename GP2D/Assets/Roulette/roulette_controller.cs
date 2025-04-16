using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class roulette_controller :MonoBehaviour
{
    float rotSpeed = 0; //기본 회전 속도

    void Start()
    {
        
    }

    void Update()
    {
        //클릭하면 룰렛의 회전속도를 설정한다.
        if (Input.GetMouseButtonDown(0))
        {
            this.rotSpeed = 10;
        }

        transform.Rotate(0, 0, this.rotSpeed); //회전 속도만큼 룰렛을 회전시킨다
        this.rotSpeed *= 0.96f; //룰렛을 감속시킨다.
    }
}
