using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBackgroundController : MonoBehaviour
{
    public float panSpeed = .25f;
    private Vector3 addition;

    void Start()
    {
        addition = new Vector3(panSpeed * .25f, panSpeed, 0);
    }

    void Update()
    {
        this.transform.position += addition;
    }
}
