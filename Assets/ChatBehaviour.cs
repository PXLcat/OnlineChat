using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ChatBehaviour : MonoBehaviour
{
    private BDDTools _bddTools;

    private TMP_InputField _inputField;

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
        Typing = false;

        _bddTools = GameObject.FindGameObjectWithTag(Tags.ConnectionManager).GetComponent<BDDTools>(); //timing ok?
    }



    private void Update()
    {
        if (Typing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Typing = false;
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                _bddTools.SendMessageToBDD(_inputField.text, DateTime.Now);
                _inputField.text = "";
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Typing = true;
            }
        }



    }

}
