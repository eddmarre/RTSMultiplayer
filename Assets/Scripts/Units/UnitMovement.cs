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
    [SerializeField] private Targeter targeter;
    [SerializeField] private float chaseRange;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += GameOverHandler_ServerOnGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= GameOverHandler_ServerOnGameOver;
    }

    [Server]
    private void GameOverHandler_ServerOnGameOver()
    {
        playerAgent.ResetPath();
    }


    [ServerCallback]
    private void Update()
    {
        Targetable targetable = targeter.GetTarget();
        if (targetable != null)
        {
            //more effiecient than Vector3.distance becasue that uses square root which is more expensive than squaring
            if ((targetable.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                playerAgent.SetDestination(targetable.transform.position);
            }
            else if (playerAgent.hasPath)
            {
                playerAgent.ResetPath();
            }

            return;
        }

        if (!playerAgent.hasPath) return;

        if (playerAgent.remainingDistance > playerAgent.stoppingDistance) return;

        playerAgent.ResetPath();
    }


    [Command]
    public void CmdMove(Vector3 position)
    {
        targeter.ClearTarget();

        //if not valid nav mesh position
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;

        playerAgent.SetDestination(hit.position);
    }

    #endregion

    #region Client

    #endregion
}