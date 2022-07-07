using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private List<Unit> myUnits = new List<Unit>();

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += Unit_ServerOnUnitSpawned;
        Unit.ServerOnUnitDespawned += Unit_ServerOnUnitDespawned;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= Unit_ServerOnUnitSpawned;
        Unit.ServerOnUnitDespawned -= Unit_ServerOnUnitDespawned;
    }

    private void Unit_ServerOnUnitSpawned(Unit unit)
    {
        //if this is the correct player controller
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myUnits.Add(unit);
    }

    private void Unit_ServerOnUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myUnits.Remove(unit);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if (NetworkServer.active) return;

        Unit.AuthorityOnUnitSpawned += Unit_AuthorityOnUnitSpawned;
        Unit.AuthorityOnUnitDespawned += Unit_AuthorityOnUnitDespawned;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) return;

        Unit.AuthorityOnUnitSpawned -= Unit_AuthorityOnUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= Unit_AuthorityOnUnitDespawned;
    }

    private void Unit_AuthorityOnUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void Unit_AuthorityOnUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    #endregion
}