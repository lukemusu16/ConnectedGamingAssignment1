using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;

public class SavingSystem : MonoBehaviour
{

    FirebaseFirestore db;
    FirebaseStorage _firebaseStorageInstance;
    string playerId;


    public static SavingSystem Instance
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
        else
        {
            DontDestroyOnLoad(this);
            _firebaseStorageInstance = FirebaseStorage.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        if (GlobalValues.LaunchCheck)
        {
            setPlayerID();
            setPrivacyPolicy();

            PlayerPrefs.Save();

            if (PlayerPrefs.HasKey("playerID"))
            {
                print(PlayerPrefs.GetString("playerID"));
            }
            GlobalValues.LaunchCheck = false;

            playerId = PlayerPrefs.GetString("playerID");

            LoadManifestItems("gs://emojijunkie-c258a.appspot.com/manifest.xml");
        }
        

    }

    string InterpretRegex(string regex)
    {
        const string upperCase = "ABCDEFGHIJKLMNOPQERSTUVWXYZ";
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string nums = "0123456789";

        List<string> needs = new List<string>();
        List<string> reps = new List<string>();

        foreach (char c in regex)
        {
            Console.WriteLine(regex);
            if (c == '[')
            {

                string need = regex.Substring(regex.IndexOf(c), regex.IndexOf(']') + 1);
                Console.WriteLine(need);
                string newRegex = regex.Remove(regex.IndexOf(c), regex.IndexOf(']') + 1);
                regex = newRegex;

                needs.Add(need);
            }
            else if (c == '{')
            {

                string rep = regex.Substring(regex.IndexOf(c), regex.IndexOf('}') + 1);
                Console.WriteLine(rep);
                string newRegex = regex.Remove(regex.IndexOf(c), regex.IndexOf('}') + 1);
                regex = newRegex;

                reps.Add(rep);
            }
        }

        System.Random rnd = new System.Random();
        string interpretedRegex = "";

        for (int x = 0; x < needs.Count; x++)
        {
            if (needs[x].Contains("A-Z"))
            {
                string num = reps[x].Substring(1, reps[x].Length - 2);
                int repeater = Convert.ToInt32(num);

                for (int z = 0; z < repeater; z++)
                {
                    interpretedRegex += upperCase[rnd.Next(0, upperCase.Length - 1)];
                }
            }
            else if (needs[x].Contains("a-z"))
            {
                string num = reps[x].Substring(1, reps[x].Length - 2);
                int repeater = Convert.ToInt32(num);

                for (int z = 0; z < repeater; z++)
                {
                    interpretedRegex += lowerCase[rnd.Next(0, lowerCase.Length - 1)];
                }
            }
            else if (needs[x].Contains("0-9"))
            {
                string num = reps[x].Substring(1, reps[x].Length - 2);
                int repeater = Convert.ToInt32(num);

                for (int z = 0; z < repeater; z++)
                {
                    interpretedRegex += nums[rnd.Next(0, nums.Length - 1)];
                }
            }
        }

        return interpretedRegex;
    }

    void setPlayerID()
    {
        if (PlayerPrefs.HasKey("playerID"))
        {
            PlayerPrefs.SetString("playerID", InterpretRegex("[A-Z]{4}[0-9]{4}[a-z]{4}"));
        }
        else
        {
            PlayerPrefs.SetString("playerID", InterpretRegex("[A-Z]{4}[0-9]{4}[a-z]{4}"));
        }
    }

    void setPrivacyPolicy()
    {
        if (PlayerPrefs.HasKey("PrivacyPolicy"))
        {
            PlayerPrefs.SetInt("PrivacyPolicy", 0);
        }
        else
        {
            PlayerPrefs.SetInt("PrivacyPolicy", 0);
        }
    }

    public void LoadManifestItems(string url)
    {
        // Create a storage reference from our storage service
        StorageReference imageRef =
            _firebaseStorageInstance.GetReferenceFromUrl(url);

        // Download in memory with a maximum allowed size of 1MB (1 * 1024 * 1024 bytes)
        const long maxAllowedSize = 1 * 2048 * 2048;
        imageRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogException(task.Exception);
                // Uh-oh, an error occurred!
            }
            else
            {
            
                byte[] fileContents = task.Result;
 
                DocumentReference docRef = db.Collection("playerData").Document(playerId);

                XDocument manifest = XDocument.Parse(System.Text.Encoding.UTF8.GetString(fileContents));
                foreach (XElement elem in manifest.Root.Elements())
                {
                    print("entered");
                    //Debug.Log(elem.Element("id")?.Value);
                    string idStr = elem.Element("id")?.Value;
                    int id = (idStr != null) ? int.Parse(idStr) : 0;
                    string nameStr = elem.Element("name")?.Value;
                    string urlStr = elem.Element("thumbnail")?.Element("url")?.Value;
                    string priceStr = elem.Element("price")?.Value;
                    string dlcTypeStr = elem.Element("DLCType")?.Value;
                    string contentUrlStr = elem.Element("ContentUrl")?.Value;
                    float price = (priceStr != null) ? float.Parse(priceStr) : 0f;
                    AssetData.CURRENNCY currency = AssetData.CURRENNCY.Emojicoins;
                    bool isPurchased = false;

                    AssetData newAsset = new AssetData(id, nameStr, urlStr, price, currency, dlcTypeStr, contentUrlStr, isPurchased);
                    FirebaseStorageController.Instance._assetData.Add(newAsset);

                    Dictionary<string, object> PurchaseData = new Dictionary<string, object>
                    {
                        { "Asset "+FirebaseStorageController.Instance._assetData[id].Id, new Dictionary<string, object>
                            {
                                { "AssetID", FirebaseStorageController.Instance._assetData[id].Id},
                                { "AssetName", FirebaseStorageController.Instance._assetData[id].Name},
                                { "isPurchase", FirebaseStorageController.Instance._assetData[id].IsPurcahsed},
                                { "timestamp", DateTime.Now.ToString("dd-MM-yyyy [HH:mm:ss]")}
                            } 
                        },
                    };

                    if (FirebaseStorageController.Instance._assetData[id].Id == 0)
                    {
                        docRef.SetAsync(PurchaseData);
                    }
                    else
                    {
                        docRef.UpdateAsync(PurchaseData);
                    }
                    
                }
            }
        });
    }
}
