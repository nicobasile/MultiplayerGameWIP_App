using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveStick : Joystick
{
    private Vector2 defaultPosition;
    private float MoveThreshold { get { return moveThreshold; } set { moveThreshold = Mathf.Abs(value); } }

    private float moveThreshold = .6f;

    protected override void Start()
    {
        defaultPosition = this.gameObject.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
        MoveThreshold = moveThreshold;
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

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude > moveThreshold)
        {
            Vector2 difference = normalised * (magnitude - moveThreshold) * radius;
            background.anchoredPosition += difference;
        }
        base.HandleInput(magnitude, normalised, radius, cam);
    }
}
