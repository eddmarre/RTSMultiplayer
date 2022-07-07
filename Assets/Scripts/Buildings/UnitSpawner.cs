using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform spawnPoint;

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie +=  Health_ServerOnDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -=  Health_ServerOnDie;
    }

    [Command]
    private void CmdSpawnUnit()
    {
        GameObject unitInstance = Instantiate(unitPrefab, spawnPoint.position, spawnPoint.rotation);

        //connection to client gives user client control
        NetworkServer.Spawn(unitInstance, connectionToClient);
    }
    
    [Server]
    private void Health_ServerOnDie()
    {
        //NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (!hasAuthority) return;
        
        CmdSpawnUnit();
    }

    #endregion
}