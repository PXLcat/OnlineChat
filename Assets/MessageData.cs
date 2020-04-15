using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MessageData : MonoBehaviour
{
    public UnityEvent callList;

    public float m_aliveTime = 3;
    [SerializeField]
    private float _currentTimer;

    public ChatMessage chatData;

    private void Update()
    {
        _currentTimer += Time.deltaTime;

        if (_currentTimer > m_aliveTime)
        {

            Destroy(gameObject);
            GameEvents.current.BubbleDestroy(transform.gameObject);//attention ça le fait à l'initialisation je crois
        }
    }
}
