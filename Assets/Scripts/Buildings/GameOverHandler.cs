using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;
    public static event Action<string> ClientOnGameOver;

    private List<UnitBase> bases = new List<UnitBase>();

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += UnitBase_ServerOnBaseSpawned;
        UnitBase.ServerOnBaseDespawned += UnitBase_ServerOnBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawned -= UnitBase_ServerOnBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= UnitBase_ServerOnBaseDespawned;
    }

    [Server]
    private void UnitBase_ServerOnBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    private void UnitBase_ServerOnBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if (bases.Count != 1) return;

        int playerID = bases[0].connectionToClient.connectionId;

        RpcGameOver($"player {playerID}");
        
        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}