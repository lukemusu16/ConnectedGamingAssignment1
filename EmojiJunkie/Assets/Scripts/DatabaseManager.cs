using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UI;

public class DatabaseManager : MonoBehaviour
{
    FirebaseFirestore db;
    string playerId;

    private void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
        DontDestroyOnLoad(this);
    }


    public void TrackClicks(string action)
    {
        playerId = PlayerPrefs.GetString("playerID");

        DocumentReference docRef = db.Collection("player_DataMining").Document(playerId);
        Dictionary<string, object> buttonClicks = new Dictionary<string, object>
        {
            { "action", action},
            { "timestamp", DateTime.Now.ToString("dd-MM-yyyy [HH:mm:ss]")},
        };

        docRef.SetAsync(buttonClicks);

    }
}