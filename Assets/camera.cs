using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class camera : MonoBehaviour
{
    public GameObject mainCamera;
    public TextMeshPro rotationText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float cameraRotationX = mainCamera.transform.rotation.eulerAngles.x;

        if (cameraRotationX < 180)
        {
            cameraRotationX = 0;
        }
        else
        {
            cameraRotationX = 360 - cameraRotationX;
        }

        float cameraRotationY = 175 + 360 - mainCamera.transform.rotation.eulerAngles.y;

        if (cameraRotationY > 360)
        {
            cameraRotationY = cameraRotationY - 360;
        }

        if (cameraRotationY < 5)
        {
            cameraRotationY = 5;
        }
        else if (cameraRotationY > 355)
        {
            cameraRotationY = 355;
        }

        cameraRotationX = cameraRotationX * 10;
        cameraRotationY = cameraRotationY * 10;
        rotationText.text = "X : " + cameraRotationX + "\nY : " + cameraRotationY;

        //Y >> 초기값이 0~ 3500 // 시작위치를 1700으로 해줘야 되고
            

    }
}
