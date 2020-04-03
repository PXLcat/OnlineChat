using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionSynchronization : MonoBehaviour
{
    private BDDTools _bddTools;
    public int _playerID;
    public bool _isPlayer;

    [SerializeField]
    private Transform _transform;

    private Vector3 _lastPosition;
    private Vector3 _currentPosition;

    private Camera _mainCamera;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        //A ce moment on ne sait pas si la co est opérationelle ou non comme elle est aussi dans l'Awake
        _bddTools = GameObject.FindGameObjectWithTag(Tags.ConnectionManager).GetComponent<BDDTools>();

        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_isPlayer) //Si c'est le joueur, on écrit ses déplacements dans la BDD
        {
            _currentPosition = _transform.position;

            if (_currentPosition != _lastPosition)
            {
                _bddTools.UpdatePosition((Vector2)_currentPosition);
            }

            _lastPosition = _currentPosition;
        }
        else //Sinon on récupère la position
        {
            GoToBDDPosition();
        }

    }

    public void GoToBDDPosition()
    {
        _transform.position = _bddTools.GetPosition(_playerID);
    }
}
//public struct PointPos
//{
//    public float posX;
//    public float posY;

//    public PointPos(float posX, float posY)
//    {
//        this.posX = posX;
//        this.posY = posY;
//    }

//    public static explicit operator PointPos(Vector3 v)
//    {
//        return new PointPos(v.x, v.y);
//    }
//    public static explicit operator Vector3(PointPos p)
//    {
//        return new Vector3(p.posX, p.posY, 0);
//    }
//}
