using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class roulette_controller :MonoBehaviour
{
    float rotSpeed = 0; //�⺻ ȸ�� �ӵ�

    void Start()
    {
        
    }

    void Update()
    {
        //Ŭ���ϸ� �귿�� ȸ���ӵ��� �����Ѵ�.
        if (Input.GetMouseButtonDown(0))
        {
            this.rotSpeed = 10;
        }

        transform.Rotate(0, 0, this.rotSpeed); //ȸ�� �ӵ���ŭ �귿�� ȸ����Ų��
        this.rotSpeed *= 0.96f; //�귿�� ���ӽ�Ų��.
    }
}
