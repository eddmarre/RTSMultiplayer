using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health;
    public static event Action<int> ServerOnPlayerDie;
    public static event Action<UnitBase> ServerOnBaseSpawned;
    public static event Action<UnitBase> ServerOnBaseDespawned;
    
    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += Health_ServerOnDie;
        
        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        health.ServerOnDie -= Health_ServerOnDie;
        ServerOnBaseDespawned?.Invoke(this);
    }

    [Server]
    private void Health_ServerOnDie()
    {
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);
        
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    #endregion
}