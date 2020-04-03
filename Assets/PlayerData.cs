﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public int _userID;
    public string _userName;

    private PositionSynchronization _positionSynchronization;

    public void Initialize(int userID, string userName)
    {
        this._userID = userID;
        this._userName = userName;

        _positionSynchronization = GetComponent<PositionSynchronization>();
        _positionSynchronization._playerID = userID;
    }
}
