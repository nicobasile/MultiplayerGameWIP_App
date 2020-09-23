using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpecialStick : Joystick
{
    private Vector2 defaultPosition;
    public bool IsPressed = false;

    protected override void Start()
    {
        defaultPosition = this.gameObject.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
        base.Start();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {        
        IsPressed = false;
        background.anchoredPosition = defaultPosition;
        base.OnPointerUp(eventData);
    }
}
