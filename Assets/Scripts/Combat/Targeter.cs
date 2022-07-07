using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
     private Targetable _targetable;

     public Targetable GetTarget()
     {
         return _targetable;
     }
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
        ClearTarget();
    }

    [Command]
    public void CmdSetTarget(GameObject targetGO)
    {
        if (!targetGO.TryGetComponent(out Targetable targetable)) return;

        _targetable = targetable;
    }

    [Server]
    public void ClearTarget()
    {
        _targetable = null;
    }

    #endregion

    #region Client

    #endregion
}