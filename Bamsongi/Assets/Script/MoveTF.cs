using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTF : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 1f;

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        float move = zInput * moveSpeed * Time.deltaTime;
        float rotate = xInput * rotationSpeed * Time.deltaTime;
        transform.Translate(0, 0, move);
        transform.Rotate(0, rotate, 0);
    }
}
