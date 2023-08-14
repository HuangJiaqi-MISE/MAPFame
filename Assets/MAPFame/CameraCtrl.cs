using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public Transform Player;
    private float mouseX, mouseY;       //����ƶ�ֵ
    public float mouseSensitivity;      //���������
    public float yRotation;

    private void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //��player������ת
        Player.Rotate(Vector3.up * mouseX);

        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -70f, 70f);
        //�����������ת
        transform.localRotation = Quaternion.Euler(yRotation, 0, 0);

    }
}