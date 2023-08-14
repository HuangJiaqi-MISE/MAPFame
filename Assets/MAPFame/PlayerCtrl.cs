using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    private CharacterController cc;

    public float MoveSpeed;

    public float JumpSpeed;

    private float horizontalMove, verticalMove;

    private Vector3 dir;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        verticalMove =  Input.GetAxis("Vertical") * MoveSpeed;
        horizontalMove = Input.GetAxis("Horizontal") * MoveSpeed;

        dir = transform.forward * verticalMove + transform.right * horizontalMove;
        cc.Move(dir * Time.deltaTime);

    }

}
