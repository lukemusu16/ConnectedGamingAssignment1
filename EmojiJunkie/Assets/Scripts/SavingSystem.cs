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
            //DontDestroyOnLoad(this);
            _firebaseStorageInstance = FirebaseStorage.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
        }
        Instance = this;

        if (PlayerPrefs.HasKey("firstLaunch"))
        {
            PlayerPrefs.SetInt("firstLaunch", 1);
        }
        else
        {
            PlayerPrefs.SetInt("firstLaunch", 0);
        }
    }

    private void DeletePrefs()
    {
        PlayerPrefs.DeleteAll();
        print("Prefs deleted");
    }


    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("firstLaunch") == 0)
        {
            setPlayerID();
            setPrivacyPolicy();
            setBalance();
            setClicks();

            PlayerPrefs.Save();
        }
        else
        {
            Destroy(GameObject.Find("Canvas").transform.Find("Privacy").gameObject);
        }

        GlobalValues.PlayerID = PlayerPrefs.GetString("playerID");
        LoadManifestItems("gs://emojijunkie-c258a.appspot.com/manifest.xml");

        ///
        /// Testing Functions
        ///
        //setPlayerIDManual("WEJI1835fcxv");
        //DeletePrefs();

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
            print(PlayerPrefs.GetString("playerID"));
        }
        else
        {
            PlayerPrefs.SetString("playerID", InterpretRegex("[A-Z]{4}[0-9]{4}[a-z]{4}"));
        }
    }
    void setPlayerIDManual(string playerID)
    {
        if (PlayerPrefs.HasKey("playerID"))
        {
            PlayerPrefs.SetString("playerID", playerID);
        }
        else
        {
            PlayerPrefs.SetString("playerID", playerID);
        }
    }

    void setBalance()
    {
        if (PlayerPrefs.HasKey("Balance"))
        {
            print(PlayerPrefs.GetInt("Balance"));
        }
        else
        {
            PlayerPrefs.SetInt("Balance", 1000);
        }
    }

    void setPrivacyPolicy()
    {
        GameObject.Find("Canvas").transform.Find("Privacy").gameObject.SetActive(true);
    }

    void setClicks()
    {
        if (PlayerPrefs.HasKey("storeClicks"))
        {
            print(PlayerPrefs.GetInt("storeClicks"));
        }
        else
        {
            PlayerPrefs.SetInt("storeClicks", 0);
        }

        if (PlayerPrefs.HasKey("gameClicks"))
        {
            print(PlayerPrefs.GetInt("gameClicks"));
        }
        else
        {
            PlayerPrefs.SetInt("gameClicks", 0);
        }

        if (PlayerPrefs.HasKey("purchaseClicks"))
        {
            print(PlayerPrefs.GetInt("purchaseClicks"));
        }
        else
        {
            PlayerPrefs.SetInt("purchaseClicks", 0);
        }
    }

    public void LoadManifestItems(string url)
    {
        if (PlayerPrefs.GetInt("firstLaunch") == 0)
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

                    XDocument manifest = XDocument.Parse(System.Text.Encoding.UTF8.GetString(fileContents));
                    foreach (XElement elem in manifest.Root.Elements())
                    {
                        //Debug.Log(elem.Element("id")?.Value);
                        string idStr = elem.Element("id")?.Value;
                        int id = (idStr != null) ? int.Parse(idStr) : 0;
                        string nameStr = elem.Element("name")?.Value;
                        string urlStr = elem.Element("thumbnail")?.Element("url")?.Value;
                        string priceStr = elem.Element("price")?.Value;
                        string dlcTypeStr = elem.Element("DLCType")?.Value;
                        string contentUrl = elem.Element("contentUrl")?.Value;
                        int price = (priceStr != null) ? int.Parse(priceStr) : 0;
                        AssetData.CURRENNCY currency = AssetData.CURRENNCY.Emojicoins;
                        bool isPurchased = false;


                        AssetData newAsset = new AssetData(id, nameStr, urlStr, price, currency, dlcTypeStr, contentUrl, isPurchased);
                        FirebaseStorageController.Instance._assetData.Add(newAsset);

                        DocumentReference docRef = db.Collection("playerData").Document(GlobalValues.PlayerID).Collection("Assets").Document("Asset " + FirebaseStorageController.Instance._assetData[id].Id);
                        Dictionary<string, object> PurchaseData = new Dictionary<string, object>
                    {
                        { "AssetID", FirebaseStorageController.Instance._assetData[id].Id},
                        { "AssetName", FirebaseStorageController.Instance._assetData[id].Name},
                        { "AssetThumbnailURL", FirebaseStorageController.Instance._assetData[id].ThumbnailUrl},
                        { "AssetPrice", FirebaseStorageController.Instance._assetData[id].Price},
                        { "AssetCurrency", FirebaseStorageController.Instance._assetData[id].Currency},
                        { "AssetDLCType", FirebaseStorageController.Instance._assetData[id].DLCType},
                        { "AssetContentURL", FirebaseStorageController.Instance._assetData[id].ContentUrl},
                        { "isPurchase", FirebaseStorageController.Instance._assetData[id].IsPurcahsed},
                        { "timestamp", DateTime.Now.ToString("dd-MM-yyyy [HH:mm:ss]")}
                    };

                        docRef.SetAsync(PurchaseData);

                    }
                    print("Data Saved");
                }
            });
        }
        else
        {
            Query colRef = db.Collection("playerData").Document(GlobalValues.PlayerID).Collection("Assets");
            print(colRef);
            colRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {

                QuerySnapshot allAssets = task.Result;

                print(allAssets);

                foreach (DocumentSnapshot docSnapshot in allAssets)
                {
                    DocumentReference docRef = docSnapshot.Reference;
                   
                    int id = 0;
                    string name = "Default";
                    string thumbnailUrl = "Default";
                    int price = 0;
                    AssetData.CURRENNCY currency = AssetData.CURRENNCY.Emojicoins;
                    string dlcType = "Default";
                    string contentUrl = "Default";
                    bool isPurcahsed = false;

                    docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                    {
                        Dictionary<string, object> assetData = task.Result.ToDictionary();

                        print(assetData);

                        foreach (KeyValuePair<string, object> assetItem in assetData)
                        {
                            switch (assetItem.Key)
                            {
                                case "AssetID":
                                    id = int.Parse(assetItem.Value.ToString());
                                    break;

                                case "AssetName":
                                    name = assetItem.Value.ToString();
                                    break;

                                case "AssetThumbnailURL":
                                    thumbnailUrl = assetItem.Value.ToString();
                                    break;

                                case "AssetPrice":
                                    price = int.Parse(assetItem.Value.ToString());
                                    break;

                                case "AssetCurrency":
                                    Enum.TryParse<AssetData.CURRENNCY>(assetItem.Value.ToString(), out currency);
                                    break;

                                case "AssetDLCType":
                                    dlcType = assetItem.Value.ToString();
                                    break;

                                case "AssetContentURL":
                                    contentUrl = assetItem.Value.ToString();
                                    break;

                                case "isPurchase":
                                    isPurcahsed = Convert.ToBoolean(assetItem.Value.ToString());
                                    break;

                            }
                        }

                        AssetData newAsset = new AssetData(id, name, thumbnailUrl, price, currency, dlcType, contentUrl, isPurcahsed);
                        FirebaseStorageController.Instance._assetData.Add(newAsset);
                        print("Data Loaded");
                    });

                }
            });
        }
        
    }
}
