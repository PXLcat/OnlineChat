using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;

    private void Awake()
    {
        current = this;
    }

    public void PlayerPositionChanged(int _playerID, Vector3 _currentPosition)
    {
        if (onPlayerPositionChanged != null)
        {
            onPlayerPositionChanged(_playerID, _currentPosition);
        }
    }

    public void BubbleDestroy(GameObject bubble)
    {
        if (onBubbleDestroy != null)
        {
            onBubbleDestroy(bubble);
        }
    }

    public event Action<int, Vector3> onPlayerPositionChanged;
    public event Action<GameObject> onBubbleDestroy;
}
