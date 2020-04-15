using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public UnityEvent callMessagesManager;//il est appelé lui? 

    private void Awake()
    {
        _transform = transform.parent.GetComponent<Transform>();
        //A ce moment on ne sait pas si la co est opérationelle ou non comme elle est aussi dans l'Awake
        _bddTools = GameObject.FindGameObjectWithTag(Tags.ConnectionManager).GetComponent<BDDTools>();

        _mainCamera = Camera.main;
    }

    private void Start()
    {

    }


    private void FixedUpdate()
    {
        _currentPosition = _transform.position;

        if (_isPlayer) //Si c'est le joueur, on écrit ses déplacements dans la BDD
        {

            if (_currentPosition != _lastPosition)
            {
                _bddTools.UpdatePosition((Vector2)_currentPosition);
                //change message position event (id, position)
                GameEvents.current.PlayerPositionChanged(_playerID, _currentPosition);
            }           
        }
        else //Sinon on récupère la position
        {
            _currentPosition = GetBDDPosition();
            //change message position event (id, position)
            if (_currentPosition != _lastPosition)
            {
                GameEvents.current.PlayerPositionChanged(_playerID, _currentPosition);//attention ça le fait à l'initialisation je crois
            }
            _transform.position = _currentPosition;
        }

        _lastPosition = _currentPosition;
    }

    public Vector2 GetBDDPosition()
    {
        return _bddTools.GetPosition(_playerID);
    }
}

