using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System;
using System.Text;

public class BDDTools : MonoBehaviour
{
    private SqlConnection _sqlCo;
    private int _userID;

    private RoomBailer _roomBailer;

    #region Ouverture de la connexion en Awake et fermeture en Quit
    private void Awake()
    {
        OpenConnection();

        _roomBailer = GameObject.FindGameObjectWithTag(Tags.RoomManager).GetComponent<RoomBailer>();
        _roomBailer.Initialize();
    }

    private void OpenConnection()
    {
        _sqlCo = new SqlConnection("Data Source=192.168.0.51;Initial Catalog=Worlds;Integrated Security=false;User ID=sa;Password=password;");
        try
        {
            UnityEngine.Debug.Log("Trying to connect database");
            _sqlCo.Open();
        }
        catch (SqlException e)
        {
            UnityEngine.Debug.Log("Database error: " + e.Message);
        }

    }

    private void OnApplicationQuit()
    {
        DeletePosition();
        FreeUser();
        CloseConnection();
    }


    private void CloseConnection()
    {
        if ((_sqlCo.State == ConnectionState.Open) || (_sqlCo.State == ConnectionState.Executing))
        //TODO: quoi faire si opération en cours? :/
        {
            _sqlCo.Close();
        }
    }


    #endregion

    #region Réserver l'identifiant et le libérer
    public KeyValuePair<int, string> GetAvailablePlayer()
    {
        string username = "";

        SqlCommand cmd = CreateRequest(@"
            UPDATE Room_User SET available = 0
            OUTPUT deleted.user_id, deleted.username
            WHERE user_id = (SELECT TOP 1 user_id FROM Room_User WHERE available = 1)
            ");

        using (SqlDataReader dr = cmd.ExecuteReader())
        { //TODO message si aucun dispo
            while (dr.Read())
            {
                _userID = dr.GetInt32(dr.GetOrdinal("user_id"));
                username = dr.GetString(dr.GetOrdinal("username")).Trim();
            }
        }

        return new KeyValuePair<int, string>(_userID, username);
    }

    private void FreeUser()
    {
        if (_userID != 0)
        {
            SqlCommand cmd = CreateRequest(@"
            UPDATE Room_User SET available = 1
            WHERE user_id = @user_id
            ");

            cmd.Parameters.AddWithValue("@user_id", _userID);

            cmd.ExecuteNonQuery();
        }

    }
    #endregion

    /// <summary>
    /// Met au millieu pour un joueur que le client vient de créer (et non récupérer)
    /// </summary>
    public void InitializePosition()
    {
        SqlCommand cmd = CreateRequest(@"
            INSERT INTO ROOM_POSITION
            VALUES (@user_id, 0.0, 0.0)
            ");

        cmd.Parameters.AddWithValue("@user_id", _userID);

        cmd.ExecuteNonQuery();
    }

    public Vector2 GetPosition(int id)
    {
        Vector2 result = Vector2.zero;

        SqlCommand cmd = CreateRequest(@"
            SELECT posX, posY FROM Room_Position
            WHERE user_id = @user_id
            ");
        cmd.Parameters.AddWithValue("@user_id", id);
        using (SqlDataReader dr = cmd.ExecuteReader())
        {
            while (dr.Read())
            {
                result.x = (float)dr.GetDouble(dr.GetOrdinal("posX"));
                result.y = (float)dr.GetDouble(dr.GetOrdinal("posY"));
            }
            //TODO Verif que c'est bien passé sinon on sera pas sûrs avec le Vector2.Zero
        }
        return result;
    }

    public void DeletePosition()
    {
        SqlCommand cmd = CreateRequest(@" 
            DELETE FROM ROOM_POSITION
            WHERE user_id = @user_id
            ");

        cmd.Parameters.AddWithValue("@user_id", _userID);

        cmd.ExecuteNonQuery();
    }

    public void UpdatePosition(Vector2 position)
    {
        SqlCommand cmd = CreateRequest(@"
            UPDATE ROOM_POSITION 
	        SET posX = @posX , posY = @posY
            WHERE user_id = @user_id
            ");

        cmd.Parameters.AddWithValue("@posX", position.x);
        cmd.Parameters.AddWithValue("@posY", position.y);
        cmd.Parameters.AddWithValue("@user_id", _userID);

        cmd.ExecuteNonQuery();
    }

    public List<KeyValuePair<int, string>> GetNewPlayers(int[] currentPlayers) //TODO garde fous si la liste est vide
    {
        List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();

        //Construction de la requête
        StringBuilder sb = new StringBuilder();
        sb.Append("SELECT Room_Position.user_id, Room_User.username,posX, posY ");
        sb.Append("FROM Room_User ");
        sb.Append("INNER JOIN Room_Position ON Room_Position.user_id = ROOM_USER.user_id ");
        sb.Append("WHERE Room_Position.user_id != @currentPlayer0 ");

        for (int i = 1; i < currentPlayers.Length; i++)
        //voir si y'a moyen de faire le append et le passage d'arguments dans la même boucle
        {
            sb.Append("AND Room_Position.user_id != @currentPlayer ");
            sb.Append(i);
        }

        SqlCommand cmd = CreateRequest(sb.ToString());

        //Passage des arguments
        cmd.Parameters.AddWithValue("@currentPlayer0", currentPlayers[0]);

        for (int i = 1; i < currentPlayers.Length; i++)
        {
            cmd.Parameters.AddWithValue("@currentPlayer" + i, currentPlayers[i]);
        }

        //Execution et lecture du résultat
        using (SqlDataReader dr = cmd.ExecuteReader())
        {
            if (false) //TODO c'est quoi le check null à utiliser?
            {
                UnityEngine.Debug.Log("No new players");
            }
            else
            {
                while (dr.Read())
                {
                    result.Add(new KeyValuePair<int, string>(
                        dr.GetInt32(dr.GetOrdinal("user_id")),
                        dr.GetString(dr.GetOrdinal("username")).Trim()));

                }
            }
        }

        return result;
    }
    public void SendMessageToBDD(string message, DateTime date)
    {
        SqlCommand cmd = CreateRequest(@"
            INSERT INTO Room_Chat
            VALUES (@user_id, '@time', '@message');
            ");

        cmd.Parameters.AddWithValue("@user_id", _userID);
        cmd.Parameters.AddWithValue("@time", "13:30;25"); //date.ToString("hh:mm:ss"));
        cmd.Parameters.AddWithValue("@message", message);

        cmd.ExecuteNonQuery();
    }

    public SqlCommand CreateRequest(string requete)
    {
        SqlCommand cmd = _sqlCo.CreateCommand();
        cmd.CommandText = requete;
        cmd.CommandType = CommandType.Text;
        return cmd;
    }

}
