using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public int _userID;
    public string _userName;

    private PositionSynchronization _positionSynchronization;

    public void Initialize(int userID, string userName, bool isPlayer)
    {
        this._userID = userID;
        this._userName = userName;

        _positionSynchronization = GetComponentInChildren<PositionSynchronization>();
        _positionSynchronization._playerID = userID;
        _positionSynchronization._isPlayer = isPlayer;
    }
}
