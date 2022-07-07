using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Building[] buildings;
    
    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;

    public event Action<int> ClientOnResourcesUpdated;
    
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }

    public int GetResources()
    {
        return resources;
    }

    [Server]
    public void SetResource(int newResources)
    {
        resources = newResources;
    }

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += Unit_ServerOnUnitSpawned;
        Unit.ServerOnUnitDespawned += Unit_ServerOnUnitDespawned;

        Building.ServerOnBuildingSpawned += Building_ServerOnBuildingSpawned;
        Building.ServerOnBuildingDespawned += Building_ServerOnBuildingDespawned;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= Unit_ServerOnUnitSpawned;
        Unit.ServerOnUnitDespawned -= Unit_ServerOnUnitDespawned;

        Building.ServerOnBuildingSpawned -= Building_ServerOnBuildingSpawned;
        Building.ServerOnBuildingDespawned -= Building_ServerOnBuildingDespawned;
    }

    private void Building_ServerOnBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myBuildings.Add(building);
    }

    private void Building_ServerOnBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myBuildings.Remove(building);
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

    [Command]
    public void CmdTryPlaceBuilding(int buildingID, Vector3 position)
    {
        Building buildingToPlace = null;
        
        foreach (Building building in buildings)
        {
            if (building.GetID() == buildingID)
            {
                buildingToPlace = building;
                break;
            }
        }

        if (buildingToPlace == null) return;

        GameObject newBuilding = Instantiate(buildingToPlace.gameObject, position, buildingToPlace.transform.rotation);

        NetworkServer.Spawn(newBuilding, connectionToClient);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if (NetworkServer.active) return;

        Unit.AuthorityOnUnitSpawned += Unit_AuthorityOnUnitSpawned;
        Unit.AuthorityOnUnitDespawned += Unit_AuthorityOnUnitDespawned;

        Building.AuthorityOnBuildingSpawned += Building_AuthorityOnBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += Building_AuthorityOnBuildingDespawned;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) return;

        Unit.AuthorityOnUnitSpawned -= Unit_AuthorityOnUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= Unit_AuthorityOnUnitDespawned;

        Building.AuthorityOnBuildingSpawned -= Building_AuthorityOnBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= Building_AuthorityOnBuildingDespawned;
    }

    private void Unit_AuthorityOnUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void Unit_AuthorityOnUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void Building_AuthorityOnBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }

    private void Building_AuthorityOnBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    private void ClientHandleResourcesUpdated(int oldResource, int newResource)
    {
        ClientOnResourcesUpdated?.Invoke(newResource);
    }

    #endregion
}