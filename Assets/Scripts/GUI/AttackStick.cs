using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttackStick : Joystick
{
    private Vector2 defaultPosition;
    private float MoveThreshold { get { return moveThreshold; } set { moveThreshold = Mathf.Abs(value); } }

    [SerializeField] private float moveThreshold = 1f;

    public bool IsPressed = false;

    protected override void Start()
    {
        defaultPosition = this.gameObject.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
        MoveThreshold = moveThreshold;
        base.Start();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
        Vector2 temp = ScreenPointToAnchoredPosition(eventData.position);
        temp.x -= baseRect.rect.width; // Accounts for right-handed anchor
        background.anchoredPosition = temp;
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {       
        IsPressed = false;
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

