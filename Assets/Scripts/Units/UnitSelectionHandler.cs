using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UnitSelectionHandler : MonoBehaviour
{
    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    [SerializeField] private LayerMask unitLayer;

    [SerializeField] private RectTransform unitSelectionArea;

    private Vector2 _startPosition;

    private RTSPlayer _player;

    private void Update()
    {
        if(_player==null)
            _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
                
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            //start selection area
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        float areaWidth = mousePos.x - _startPosition.x;
        float areaHiegth = mousePos.y - _startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHiegth));
        unitSelectionArea.anchoredPosition = _startPosition + new Vector2(areaWidth / 2, areaHiegth / 2);
    }

    private void StartSelectionArea()
    {
        foreach (Unit myUnit in SelectedUnits)
        {
            myUnit.Deselect();
        }

        SelectedUnits.Clear();

        unitSelectionArea.gameObject.SetActive(true);

        _startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);
        //if just clicked
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayer)) return;

            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) return;

            if (!unit.hasAuthority) return;

            SelectedUnits.Add(unit);

            foreach (Unit myUnit in SelectedUnits)
            {
                myUnit.Select();
            }

            return;
        }

        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (Unit unit in _player.GetMyUnits())
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (screenPos.x > min.x && screenPos.x < max.x && 
                screenPos.y > min.y && screenPos.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }
}