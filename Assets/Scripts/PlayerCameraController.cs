﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    // Offset from the viewport center to fix damping
    public float m_DampTime = 10f;
    public Transform m_Target;
    public float m_XOffset = 0;
    public float m_YOffset = 0;

    private float margin = 0.1f; // Prevents shaking

    private bool wasRight = true;

    void Start()
    {
        if (m_Target == null)
        {
            m_Target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (m_Target)
        {
            float targetX = m_Target.position.x + m_XOffset;
            float targetY = m_Target.position.y + m_YOffset;


            /*if (wasRight && m_XOffset < 0) // Just Changed
            {   
                m_DampTime /= 2;
                wasRight = false;
            }
            else if (!wasRight && m_XOffset > 0)
            {
                m_DampTime /= 2;
                wasRight = true;
            }*/

            // Need mechanism to change back damptime * 2

            if (Mathf.Abs(transform.position.x - targetX) > margin)
            {
                targetX = Mathf.Lerp(transform.position.x, targetX, m_DampTime * Time.deltaTime);
            }

            if (Mathf.Abs(transform.position.y - targetY) > margin)
                targetY = Mathf.Lerp(transform.position.y, targetY, m_DampTime * Time.deltaTime);

            transform.position = new Vector3(targetX, targetY, transform.position.z);
        }
    }
}
