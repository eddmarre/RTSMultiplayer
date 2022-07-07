using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health;
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TextMeshProUGUI remainingUnitText;
    [SerializeField] private Image unitProgressImage;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7;
    [SerializeField] private float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int _queuedUnits;

    [SyncVar] private float _unitTimer;

    private float progressImageVelocity;

    private void Update()
    {
        if (isServer)
        {
            ProduceUnits();
        }

        if (isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += Health_ServerOnDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= Health_ServerOnDie;
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if (_queuedUnits == maxUnitQueue) return;

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if (player.GetResources() < unitPrefab.GetResourceCost()) return;

        _queuedUnits++;
        player.SetResource(player.GetResources() - unitPrefab.GetResourceCost());
    }

    [Server]
    private void ProduceUnits()
    {
        if (_queuedUnits == 0) return;

        _unitTimer += Time.deltaTime;

        if (_unitTimer < unitSpawnDuration) return;

        GameObject unitInstance = Instantiate(unitPrefab.gameObject, spawnPoint.position, spawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);


        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = spawnPoint.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(spawnPoint.position + spawnOffset);

        _queuedUnits--;
        _unitTimer = 0f;
    }


    [Server]
    private void Health_ServerOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    private void UpdateTimerDisplay()
    {
        float newProgress = _unitTimer / unitSpawnDuration;

        if (newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (!hasAuthority) return;

        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldnumber, int newNumber)
    {
        remainingUnitText.text = newNumber.ToString();
    }

    #endregion
}