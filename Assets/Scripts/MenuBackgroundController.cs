using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBackgroundController : MonoBehaviour
{
    public float panSpeed = .25f;
    private Vector3 addition;
    private Vector3 rotation;

    void Start()
    {
        addition = new Vector3(-panSpeed * .25f, panSpeed, 0);
        rotation = new Vector3(0, 0, panSpeed/10f);
    }

    void Update()
    {
        this.transform.position += addition;
        this.transform.Rotate(rotation);
    }
}
