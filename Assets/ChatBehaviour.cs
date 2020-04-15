using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEditor;

public class ChatBehaviour : MonoBehaviour
{
    private BDDTools _bddTools;

    private TMP_InputField _inputField;
    private GameObject _chatBubbles;
    private ChatBubblesManager _chatBubblesManager;
    public TMP_InputField _chatBubblePrefab;


    private Transform _clientTransform;
    private RectTransform _inputFieldTransform;

    public Vector2 _IFTypeOffsetFromPlayer = new Vector2(40, 40);
    public Vector2 _IFReadOffsetFromPlayer = new Vector2(60, 100); //chelou ils sont pas calculés pareil 

    public List<ChatMessage> m_chatLog = new List<ChatMessage>();

    public float m_messagesTimeOnScreen = 3;

    private RoomBailer _roomBailer;

    [SerializeField]
    private bool _typing;
    public bool Typing
    {
        get { return _typing; }
        set
        {

            if (value)
            {
                _inputField.transform.gameObject.SetActive(true);
            }
            else
            {
                _inputField.transform.gameObject.SetActive(false);
            }

            _typing = value;
        }
    }

    private void Awake()
    {
        _inputField = GetComponentInChildren<TMP_InputField>();
        _inputFieldTransform = _inputField.GetComponent<RectTransform>();
        Typing = false;

        _bddTools = GameObject.FindGameObjectWithTag(Tags.ConnectionManager).GetComponent<BDDTools>(); //timing ok?

        _chatBubbles = GameObject.FindGameObjectWithTag(Tags.ChatBubbles);
        _chatBubblesManager = _chatBubbles.GetComponent<ChatBubblesManager>();

        _roomBailer = GameObject.FindGameObjectWithTag(Tags.RoomManager).GetComponent<RoomBailer>();

    }

    private void Start()
    {
        GameEvents.current.onPlayerPositionChanged += OnPositionChange;
        GameEvents.current.onBubbleDestroy += OnBubbleDestroy;
    }

    public void InitializeClientTransform(Transform transform)
    {
        _clientTransform = transform;
    }

    private void Update()
    {
        #region Sending messages
        Vector2 myPositionOnScreen = (Vector2)Camera.main.WorldToScreenPoint(_clientTransform.position) + _IFTypeOffsetFromPlayer;
        _inputFieldTransform.anchoredPosition = myPositionOnScreen;

        if (Typing)
        {


            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Typing = false;
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                _bddTools.SendMessageToBDD(_inputField.text, DateTime.Now); //TODO vérif sur taille de message /BDD et pas de drop table
                _inputField.text = "";
                Typing = false;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Typing = true;
                _inputField.Select();
            }
        }
        #endregion

        #region Showing messages
        //TODO : Coroutine qui vide de _chatLog les messages d'il y a plus de 5min toutes les 5minutes



        //Envoyer le messageid le plus récent
        int mostRecentMsgId = 0;
        
        for (int i = 0; i < m_chatLog.Count; i++)
        {
            if (m_chatLog[i].msgID > mostRecentMsgId)
            {
                mostRecentMsgId = m_chatLog[i].msgID;
            }

        }//Linq trop lourd pour une méthode qui vient souvent, voir à terme pour avoir une variable qui s'update sans qu'on ait à piocher dans la liste

        List<ChatMessage> recentMessages = _bddTools.GetNewMessages(mostRecentMsgId);

        //On remplit m_chatLog avec ce qui y était pas déjà  
        m_chatLog.AddRange(recentMessages);

        //On update la position des messages existants à la nouvelle place
        //Trouver une façon de faire avec la PositionSynchronisation genre si un message est attaché on envoie la nouvelle position ici //Events?
        //foreach (var item in _chatBubbles.transform.chil)
        //{

        //}


        //On montre les nouveaux messages
        ShowNewMessages(recentMessages);




        //Combien de temps on décide de garder les messages à l'écran?

        //Combien de temps on laisse le log? 10min?

        #endregion


    }

    
    private void OnPositionChange(int _playerID, Vector3 _currentPosition)
    {
        Debug.Log($"joueur {_playerID} bouge");

        //TODO désabonnement des events

        foreach (GameObject item in _chatBubblesManager.m_chatBubblesList)
        {
            MessageData data = item.GetComponent<MessageData>();
            if (data.chatData.userID == _playerID)
            {
                //mettre à la bonne position TODO mettre ça en commun avec ShowNewMessage dans une nouvelle méthode
                item.GetComponent<RectTransform>().anchoredPosition =
                     (Vector2)Camera.main.WorldToScreenPoint(_currentPosition) + _IFReadOffsetFromPlayer;
            }

        }
    }

    private void OnBubbleDestroy(GameObject bubble)
    {
        Debug.Log("DestroyBubble");
        _chatBubblesManager.m_chatBubblesList.Remove(bubble);
    }


    private void ShowNewMessages(List<ChatMessage> recentMessages)
    {
        foreach (ChatMessage message in recentMessages)
        {
            //TODO check le temps
            GameObject messageGO = Instantiate(_chatBubblePrefab.gameObject, _chatBubbles.transform);
            MessageData data = messageGO.GetComponent<MessageData>();
            data.chatData = message;
            TMP_InputField messageIF = messageGO.GetComponent<TMP_InputField>();

            //mettre à la bonne position
            Vector2 correspondingPlayerPosition = _roomBailer.GetPlayerPosition(message.userID);
            messageIF.GetComponent<RectTransform>().anchoredPosition =
                 (Vector2)Camera.main.WorldToScreenPoint(correspondingPlayerPosition) + _IFReadOffsetFromPlayer;

            //initialiser le message
            messageIF.text = message.text;

            _chatBubblesManager.m_chatBubblesList.Add(messageGO);
        }
    }


}
[System.Serializable]
public struct ChatMessage
{
    public int msgID;
    public int userID;
    public UDateTime date;
    public string text;

    public ChatMessage(int msgID, int userID, DateTime date, string message)
    {
        this.msgID = msgID;
        this.userID = userID;
        this.date = date;
        this.text = message;
    }
}
