using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpecialStick : Joystick
{
    private Vector2 defaultPosition;
    
    protected override void Start()
    {
        defaultPosition = new Vector2(64.0f, 64.0f);
        base.Start();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {        
        background.anchoredPosition = defaultPosition;
        base.OnPointerUp(eventData);
    }
}
