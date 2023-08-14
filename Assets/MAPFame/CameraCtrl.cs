using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public Transform Player;
    private float mouseX, mouseY;       //鼠标移动值
    public float mouseSensitivity;      //鼠标灵敏度
    public float yRotation;

    private void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //对player进行旋转
        Player.Rotate(Vector3.up * mouseX);

        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -70f, 70f);
        //对相机进行旋转
        transform.localRotation = Quaternion.Euler(yRotation, 0, 0);

    }
}