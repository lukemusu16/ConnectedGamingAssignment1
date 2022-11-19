using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
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

    }


    public void TrackClicks(string action, GameObject obj = null)
    {
        db = FirebaseFirestore.DefaultInstance;
        if (obj == null)
        {
            if (action.Equals("store"))
            {
                int clicks = PlayerPrefs.GetInt("storeClicks");
                clicks++;
                print(clicks);
                DocumentReference docRef = db.Collection("player_DataMining").Document(GlobalValues.PlayerID).Collection(action).Document("Click " +clicks.ToString());
                Dictionary<string, object> buttonClicks = new Dictionary<string, object>
                {
                    { "action", action},
                    { "timestamp", DateTime.Now.ToString("dd-MM-yyyy [HH:mm:ss]")},
                };
                docRef.SetAsync(buttonClicks);
                PlayerPrefs.SetInt("storeClicks", clicks);
                print("store click saved");
            }
            else if (action.Equals("game"))
            {
                int clicks = PlayerPrefs.GetInt("gameClicks");
                clicks++;

                DocumentReference docRef = db.Collection("player_DataMining").Document(GlobalValues.PlayerID).Collection(action).Document("Click " + clicks.ToString());
                Dictionary<string, object> buttonClicks = new Dictionary<string, object>
                {
                    { "action", action},
                    { "timestamp", DateTime.Now.ToString("dd-MM-yyyy [HH:mm:ss]")},
                };

                docRef.SetAsync(buttonClicks);
                PlayerPrefs.SetInt("gameClicks", clicks);
                print("game click saved");
            }
        }
        else
        {
            int clicks = PlayerPrefs.GetInt("purchaseClicks");
            clicks++;

            DocumentReference docRef = db.Collection("player_DataMining").Document(GlobalValues.PlayerID).Collection(action).Document("Click " + clicks.ToString());
            Dictionary<string, object> buttonClicks = new Dictionary<string, object>
            {
                { "action", action},
                { "item", obj.transform.Find("DLCType").GetComponent<TextMeshProUGUI>().text},
                { "timestamp", DateTime.Now.ToString("dd-MM-yyyy [HH:mm:ss]")},
            };

            docRef.SetAsync(buttonClicks);
            PlayerPrefs.SetInt("purchaseClicks", clicks);
            print("purchase click saved");
        }

        

    }
}