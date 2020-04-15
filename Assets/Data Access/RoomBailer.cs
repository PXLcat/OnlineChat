using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBailer : MonoBehaviour
{
    //Se charge d'initialiser les positions
    //Check le nombre max et actuel de players connectés et l'envoie à la BDD

    public List<GameObject> m_Players;
    private BDDTools _bddTools;
    private ChatBehaviour _chatManager;

    public float checkPlayerInterval = 1.5f;

    public GameObject _playerPrefab; //TODO pas opti de rester sur un GO dont on a besoin d'extraire les composants pour les donénes

    public void Initialize()
    {
        //Récupérer la connexion
        _bddTools = GameObject.FindGameObjectWithTag(Tags.ConnectionManager).GetComponent<BDDTools>();

        //Récupérer un user
        //est-ce qu'il y a un User dispo
        KeyValuePair<int, string> userIdentity = _bddTools.GetAvailablePlayer();

        GameObject clientPlayer = CreateAndAddPlayer(userIdentity.Key, userIdentity.Value, true);

        //TODO Vérif qu'il y ait pas déjà une entrée à cet user_id là dans la BDD
        //Initialiser la position dans la BDD
        _bddTools.InitializePosition();
        //Charger le chat manager
        _chatManager = GameObject.FindGameObjectWithTag(Tags.ChatManager).GetComponent<ChatBehaviour>();
        _chatManager.InitializeClientTransform(clientPlayer.transform);

        CheckNewPlayers();
    }

    //Pour l'instant on va check toutes les 3 secondes si il y a un nouveau arrivant dans la room / faut faire au début aussi
    //Plus tard essayer avec un trigger de la db https://www.codeproject.com/Questions/341680/Using-Triggers-in-Csharp-and-Sql-Server

    [ContextMenu("Check for new players")]
    public void CheckNewPlayers()
    {
        //récupérer les id de la liste m_Players
        int[] current_players = new int[m_Players.Count];
        for (int i = 0; i < current_players.Length; i++)
        {
            current_players[i] = m_Players[i].GetComponent<PlayerData>()._userID;
        }
        List<KeyValuePair<int, string>> newPlayers = _bddTools.GetNewPlayers(current_players);

        foreach (KeyValuePair<int, string> playerData in newPlayers)
        {
            Debug.Log("new player : " + playerData.Key + " " + playerData.Value);

            //Initialiser un joueur dans la liste 
            GameObject newPlayer = CreateAndAddPlayer(playerData.Key, playerData.Value);

            //Le mettre à sa bonne position
            newPlayer.GetComponentInChildren<PositionSynchronization>().GetBDDPosition();
        }
    }

    private void Start()
    {
        Debug.Log("start");
        StartCoroutine(CheckPlayersCoroutine());
    }

    IEnumerator CheckPlayersCoroutine()
    {
        while (true)
        {
            Debug.Log("check");
            CheckNewPlayers();
            yield return new WaitForSeconds(checkPlayerInterval);
        }
    }

    private GameObject CreateAndAddPlayer (int userId, string username, bool isPlayer = false)
    {
        GameObject newPlayer = Instantiate(_playerPrefab);
        PlayerData playerData = newPlayer.GetComponent<PlayerData>();
        playerData.Initialize(userId, username, isPlayer);

        m_Players.Add(newPlayer);

        return newPlayer;
    }

    public Vector2 GetPlayerPosition(int id)
    {
        Vector2 result = Vector2.zero;
        foreach (GameObject player in m_Players) //stocker ça sous une autre forme qu'une liste de GameObject? Si oui comment accéder facilement au Transform?
        {
            PlayerData playerData = player.GetComponent<PlayerData>();
            if (playerData._userID == id)
            {
                result = player.transform.position;
                break;//ça sort du if ou du foreach?
            }
        }
        return result;
    }

    #region Bouton Check for new players
    //private void OnGUI()
    //{
    //    if (GUI.Button(new Rect(100, 140, 160, 60), "Check for new players"))
    //    {
    //        CheckNewPlayers();
    //    }
    //}
    #endregion

}
