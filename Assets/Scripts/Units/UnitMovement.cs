using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent playerAgent;

    #region Server

    [Command]
    public void CmdMove(Vector3 position)
    {
        //if not valid nav mesh position
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;

        playerAgent.SetDestination(hit.position);
    }

    #endregion

    #region Client



    #endregion
}