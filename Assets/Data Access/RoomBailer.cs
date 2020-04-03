using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBailer : MonoBehaviour
{
    //Se charge d'initialiser les positions
    //Check le nombre max et actuel de players connectés et l'envoie à la BDD

    public List<GameObject> m_Players;
    private BDDTools _bddTools;

    public GameObject _playerPrefab; //TODO pas opti de rester sur un GO dont on a besoin d'extraire les composants pour les donénes

    public void Initialize()
    {
        //Récupérer la connexion
        _bddTools = GameObject.FindGameObjectWithTag(Tags.ConnectionManager).GetComponent<BDDTools>();

        //Récupérer un user
        //est-ce qu'il y a un User dispo
        KeyValuePair<int, string> userIdentity = _bddTools.GetAvailablePlayer();

        CreateAndAddPlayer(userIdentity.Key, userIdentity.Value, true);

        //TODO Vérif qu'il y ait pas déjà une entrée à cet user_id là dans la BDD
        //Initialiser la position dans la BDD
        _bddTools.InitializePosition();


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
            newPlayer.GetComponent<PositionSynchronization>().GoToBDDPosition();
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

}
