using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MiniMap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform miniMapRect;
    [SerializeField] private float mapScale = 20f;
    [SerializeField] private float offset = -6f;
    private Transform playerCameraTransform;


    private void Update()
    {
        if (playerCameraTransform != null) return;

        if (NetworkClient.connection.identity == null) return;


        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
    }

    private void MoveCamera()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapRect,
                mousePos,
                null,
                out Vector2 localPoint))
        {
            return;
        }

        Vector2 lerp = new Vector2((localPoint.x - miniMapRect.rect.x) / miniMapRect.rect.width,
            (localPoint.y - miniMapRect.rect.y) / miniMapRect.rect.height);

        Vector3 newCameraPos = new Vector3(Mathf.Lerp(-mapScale, mapScale, lerp.x),
            playerCameraTransform.position.y,
            Mathf.Lerp(-mapScale, mapScale, lerp.y));

        playerCameraTransform.position = newCameraPos + new Vector3(0f, 0f, offset);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }
}