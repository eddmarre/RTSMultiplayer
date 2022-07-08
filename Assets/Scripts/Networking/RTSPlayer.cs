using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Building[] buildings;
    [SerializeField] private LayerMask buildingBlockLayer;
    [SerializeField] private float buildingRangeLimit = 5f;


    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;
    [SyncVar(hook=nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    [SyncVar(hook = nameof(ClientHandleDispalyNameUpdated))] private string displayName;

    public event Action<int> ClientOnResourcesUpdated;
    public static event Action ClientOnInfoUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated; 

    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    private Color teamColor = new Color();

    public string GetDisplayName()
    {
        return displayName;
    }
    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }
    
    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    public Color GetTeamColor()
    {
        return teamColor;
    }

    [Server]
    public void SetTeamColor(Color color)
    {
        teamColor = color;
    }

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

    [Server]
    public void SetIsPartyOwner(bool value)
    {
        isPartyOwner = value;
    }

    [Server]
    public void SetDisplayName(string newName)
    {
        displayName = newName;
    }

    public bool CanPlaceBuilding(BoxCollider buildingColllider, Vector3 point)
    {
        if (Physics.CheckBox(point + buildingColllider.center,
                buildingColllider.size / 2,
                Quaternion.identity,
                buildingBlockLayer))
        {
            return false;
        }

        foreach (Building building in myBuildings)
        {
            if ((point - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }

        return false;
    }

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += Unit_ServerOnUnitSpawned;
        Unit.ServerOnUnitDespawned += Unit_ServerOnUnitDespawned;

        Building.ServerOnBuildingSpawned += Building_ServerOnBuildingSpawned;
        Building.ServerOnBuildingDespawned += Building_ServerOnBuildingDespawned;
        
        DontDestroyOnLoad(gameObject);
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
    public void CmdStartGame()
    {
        if(!isPartyOwner) return;
        
        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
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

        if (resources < buildingToPlace.GetPrice()) return;

        BoxCollider buildingColllider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(buildingColllider, position)) return;


        GameObject newBuilding = Instantiate(buildingToPlace.gameObject, position, buildingToPlace.transform.rotation);

        NetworkServer.Spawn(newBuilding, connectionToClient);

        SetResource(resources - buildingToPlace.GetPrice());
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        //skip host
        if (NetworkServer.active) return;

        DontDestroyOnLoad(gameObject);
        
        ((RTSNetworkManager) NetworkManager.singleton).players.Add(this);
    }

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
        ClientOnInfoUpdated?.Invoke();
        
        if (!isClientOnly) return;

        ((RTSNetworkManager) NetworkManager.singleton).players.Remove(this);

        if (!hasAuthority) return;

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

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if(!hasAuthority) return;

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void ClientHandleDispalyNameUpdated(string oldName, string newName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    #endregion
}