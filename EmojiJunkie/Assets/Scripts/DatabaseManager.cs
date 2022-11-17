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

    public static DatabaseManager Instance
    {
        get;
        private set;
    }

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        Instance = this;

        db = FirebaseFirestore.DefaultInstance;
    }


    public void TrackClicks(string action)
    {
        DocumentReference docRef = db.Collection("player_DataMining").Document(GlobalValues.PlayerID);
        Dictionary<string, object> buttonClicks = new Dictionary<string, object>
        {
            { "action", action},
            { "timestamp", DateTime.Now.ToString("dd-MM-yyyy [HH:mm:ss]")},
        };

        print(docRef);
        docRef.SetAsync(buttonClicks);

    }
}