using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private LayerMask floorMask;

    private RTSPlayer _player;
    private GameObject _buildingPreviewInstance;
    private Renderer _buildingRendererInstance;

    private void Start()
    {
        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();
    }

    private void Update()
    {
        if (_player == null)
        {
            _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        if (_buildingPreviewInstance == null) return;

        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        _buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        _buildingRendererInstance = _buildingPreviewInstance.GetComponentInChildren<Renderer>();

        _buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_buildingPreviewInstance == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            //place building
            _player.CmdTryPlaceBuilding(building.GetID(), hit.point);
        }

        Destroy(_buildingPreviewInstance);
    }

    private void UpdateBuildingPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) return;

        _buildingPreviewInstance.transform.position = hit.point;

        if (!_buildingPreviewInstance.activeSelf)
        {
            _buildingPreviewInstance.SetActive(true);
        }
    }
}