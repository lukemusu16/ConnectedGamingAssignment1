using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Storage;
using Unity.VisualScripting;
using TMPro;
using System.Net.Http;
using System.ComponentModel;
using System.Security.Cryptography;

public class FirebaseStorageController : MonoBehaviour
{
    FirebaseFirestore db;
    private FirebaseStorage _firebaseStorageInstance;
    private GameObject DLCItemPrefab;
    private GameObject _thumbnailContainer;
    public List<GameObject> _DLCItemsList;
    public List<AssetData> _assetData;

    byte[] bruh;

    const long maxAllowedSize = 1 * 2048 * 2048;

    public enum DownloadType
    {
        Thumbnail, Manifest, Item
    }
    public enum DLCType
    {
        BACKGROUNDS, SKINPACKS, EFFECTS
    }


    //Singleton
    public static FirebaseStorageController Instance
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
        _firebaseStorageInstance = FirebaseStorage.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    private void Start()
    {
        _DLCItemsList = new List<GameObject>();
        _assetData = new List<AssetData>();
    }

    public async Task<byte[]> DownloadFile(string url)
    {
        StorageReference imageRef =
            _firebaseStorageInstance.GetReferenceFromUrl(url);

        // Download in memory with a maximum allowed size of 1MB (1 * 1024 * 1024 bytes)
        const long maxAllowedSize = 1 * 2048 * 2048;
        await imageRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Bytes not fonud");
            }
            else
            {
                bruh = task.Result;
            }
        });

        return bruh;
        
    }

    public void DownloadFileAsync(string url, DownloadType dType)
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
                //Debug.Log($"{imageRef.Name} finished downloading!");
                if (dType == DownloadType.Thumbnail)
                {
                    foreach (AssetData item in _assetData)
                    {
                        if (string.Equals(imageRef.ToString(), item.ThumbnailUrl))
                        {
                            LoadDLCItem(fileContents);
                        }

                    }


                }
                else if (dType == DownloadType.Manifest)
                {
                    //Load the manifest
                    StartCoroutine(LoadManifest(fileContents));
                }

            }
        });
    }

    IEnumerator LoadManifest(byte[] fileContents)
    {
        //Converting from byte array to String UTF8
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
            string contentUrlStr = elem.Element("ContentUrl")?.Value;
            int price = (priceStr != null) ? int.Parse(priceStr) : 0;
            AssetData.CURRENNCY currency = AssetData.CURRENNCY.Emojicoins;
            bool isPurchased = false;

            AssetData newAsset = new AssetData(id, nameStr, urlStr, price, currency, dlcTypeStr, contentUrlStr, isPurchased);
            _assetData.Add(newAsset);

            DownloadFileAsync(newAsset.ThumbnailUrl, DownloadType.Thumbnail);
        }

        yield return null;
    }

    public void LoadDLCItem(byte[] fileContents)
    {

        DLCItemPrefab = Resources.Load<GameObject>("DLCItem");
        _thumbnailContainer = GameObject.Find("Content");

        // Display the image inside _imagePlaceholder
        GameObject DLCItem = Instantiate(DLCItemPrefab, _thumbnailContainer.transform.position, Quaternion.identity, _thumbnailContainer.transform);

        DLCItem.name = _assetData[_DLCItemsList.Count].Id.ToString();
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(fileContents);
        DLCItem.GetComponentInChildren<RawImage>().texture = tex;
        DLCItem.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = _assetData[_DLCItemsList.Count].Price.ToString();
        DLCItem.transform.Find("DLCType").GetComponent<TextMeshProUGUI>().text = _assetData[_DLCItemsList.Count].Name.ToString();

        SetButtons(DLCItem);

        _DLCItemsList.Add(DLCItem);


    }


    public void DownloadContent(string url, DLCType type)
    {
        StorageReference storage = _firebaseStorageInstance.GetReferenceFromUrl(url);

        if (type == DLCType.EFFECTS)
        {
            //string localFile = "file://" + Application.streamingAssetsPath + "/bundle.bundle";
            string localFile = UnityEngine.Application.streamingAssetsPath + "/bundle.bundle";
            storage.GetFileAsync(localFile).ContinueWithOnMainThread(task =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    Debug.Log("File downloaded.");
                    var myLoadedAssetBundle = AssetBundle.LoadFromFile(localFile);
                    if (myLoadedAssetBundle == null)
                    {
                        Debug.Log("Failed to load AssetBundle!");
                        return;
                    }
                    string assetInBundle = "";
                    foreach (string asset in myLoadedAssetBundle.GetAllAssetNames())
                    {
                        assetInBundle = asset;
                    }

                    GameObject prefab = myLoadedAssetBundle.LoadAsset<GameObject>(assetInBundle);
                    prefab.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Legacy Shaders/Particles/Additive");
                    Instantiate(prefab, Vector3.zero, Quaternion.identity);

                }
            });
        }
        else if (type == DLCType.BACKGROUNDS)
        {
            const long maxAllowedSize = 1 * 2048 * 2048;
            float picWidth = Camera.main.pixelWidth * Camera.main.orthographicSize / (Camera.main.orthographicSize * 875);
            float picHeight = Camera.main.pixelHeight * Camera.main.orthographicSize / (Camera.main.orthographicSize * 875);

            storage.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogException(task.Exception);
                    // Uh-oh, an error occurred!
                }
                else
                {
                    byte[] fileContents = task.Result;

                    GameObject backgroundGameObject = new GameObject();
                    backgroundGameObject.transform.position = new Vector3(0, 0, 2);
                    
                    backgroundGameObject.transform.localScale = new Vector2(picWidth, picHeight);
                    backgroundGameObject.name = "PhoneBackground";
                    backgroundGameObject.AddComponent<SpriteRenderer>();
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(fileContents);
                    backgroundGameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(picHeight / 2, picHeight / 2));
                }
            });
        }
        else if (type == DLCType.SKINPACKS)
        {
            const long maxAllowedSize = 1 * 2048 * 2048;
            storage.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogException(task.Exception);
                    // Uh-oh, an error occurred!
                }
                else
                {
                    byte[] fileContents = task.Result;
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(fileContents);

                    GameObject spritesRoot = GameObject.Find("Main Camera");
                    GameObject n = new GameObject();
                    SpriteRenderer sr = n.AddComponent<SpriteRenderer>();
                    StartCoroutine(IterateEmojies(tex, spritesRoot, n, sr));

                }
            });
        }
    }

    public IEnumerator IterateEmojies(Texture2D tex, GameObject spritesRoot, GameObject n, SpriteRenderer sr)
    {
        while (true)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {

                    Sprite newSprite = Sprite.Create(tex, new Rect(i * 128, j * 128, 128, 128), new Vector2(0.5f, 0.5f));
                    sr.sprite = newSprite;
                    //n.transform.position = new Vector3(i * 2, j * 2, 0);
                    n.transform.parent = spritesRoot.transform;
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
        
        
    }

    public void PopulateGameScene()
    {
        Query colRef= db.Collection("playerData").Document(GlobalValues.PlayerID).Collection("Assets");
        print(colRef);
        colRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            
            QuerySnapshot allAssets = task.Result;

            foreach (DocumentSnapshot docSnapshot in allAssets)
            {
                Dictionary<string, object> assetData = docSnapshot.ToDictionary();


                if (assetData["isPurchase"].ConvertTo<bool>())
                {
                    print("AUGHHH");
                    string url;
                    string dlctypestr;
                    DLCType dlctype = DLCType.BACKGROUNDS;

                    url = assetData["AssetContentURL"].ConvertTo<string>();
                    dlctypestr = assetData["AssetDLCType"].ConvertTo<string>();

                    print(dlctypestr);
                    print(url);

                    if (dlctypestr.Equals("Backgrounds"))
                    {
                        dlctype = DLCType.BACKGROUNDS;
                    }
                    else if (dlctypestr.Equals("SkinPacks"))
                    {
                        dlctype = DLCType.SKINPACKS;
                    }
                    else if (dlctypestr.Equals("Effects"))
                    {
                        dlctype = DLCType.EFFECTS;
                    }
                    

                    print(dlctype);

                    DownloadContent(url, dlctype);
                }
            }
        });

    }

    public void SetButtons(GameObject DLCItem)
    {
        GameObject.Find("Balance").GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("Balance").ToString();
        DocumentReference docRef = db.Collection("playerData").Document(GlobalValues.PlayerID).Collection("Assets").Document("Asset "+DLCItem.name);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> asset = snapshot.ToDictionary();
                int price = int.Parse(asset["AssetPrice"].ToString());


                if (asset.ContainsValue(true))
                {
                    DLCItem.transform.Find("Button").GetComponent<Button>().interactable = false;

                    ColorBlock colors = DLCItem.transform.Find("Button").GetComponent<Button>().colors;
                    colors.normalColor = Color.gray;
                    DLCItem.transform.Find("Button").GetComponent<Button>().colors = colors;

                    DLCItem.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().text = "Owned";

                }
                else if (PlayerPrefs.GetInt("Balance") < price)
                {
                    DLCItem.transform.Find("Button").GetComponent<Button>().interactable = false;

                    ColorBlock colors = DLCItem.transform.Find("Button").GetComponent<Button>().colors;
                    colors.normalColor = new Color(0.5f,0,0,1);
                    DLCItem.transform.Find("Button").GetComponent<Button>().colors = colors;
                    DLCItem.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().fontSize = 20;
                    DLCItem.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().text = "Insufficient Funds";
                    DLCItem.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                }
                else
                {
                    DLCItem.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        int balance = PlayerPrefs.GetInt("Balance");

                        if (balance >= _assetData[int.Parse(DLCItem.name)].Price)
                        {

                            _assetData[int.Parse(DLCItem.name)].IsPurcahsed = true;
                            print("we here");
                            DocumentReference docRef = db.Collection("playerData").Document(GlobalValues.PlayerID).Collection("Assets").Document("Asset " + int.Parse(DLCItem.name));
                            Dictionary<string, object> updates = new Dictionary<string, object>
                            {
                                { "isPurchase", _assetData[int.Parse(DLCItem.name)].IsPurcahsed}
                            };

                            docRef.UpdateAsync(updates);

                            DLCItem.transform.Find("Button").GetComponent<Button>().interactable = false;


                            balance = balance - _assetData[int.Parse(DLCItem.name)].Price;
                            PlayerPrefs.SetInt("Balance", balance);
                            GameObject.Find("Balance").GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("Balance").ToString();

                            ColorBlock colors = DLCItem.transform.Find("Button").GetComponent<Button>().colors;
                            colors.normalColor = Color.gray;
                            DLCItem.transform.Find("Button").GetComponent<Button>().colors = colors;

                            DLCItem.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().text = "Owned";
                            DatabaseManager dm = new DatabaseManager();
                            dm.TrackClicks("purchase", DLCItem);
                            UpdateButtons();
                        }

                    });
                }
                
            }
            else
            {
                Debug.Log(String.Format("Document {0} does not exist!", snapshot.Id));
            }
        });
    }

    private void UpdateButtons()
    {
        int children = GameObject.Find("Content").transform.childCount;

        for (int i = 0; i < children; i++)
        {
            GameObject item = GameObject.Find(i.ToString());

            if (_assetData[i].IsPurcahsed)
            {
                item.transform.Find("Button").GetComponent<Button>().interactable = false;

                ColorBlock colors = item.transform.Find("Button").GetComponent<Button>().colors;
                colors.normalColor = Color.gray;
                item.transform.Find("Button").GetComponent<Button>().colors = colors;

                item.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().text = "Owned";
            }
            else if (PlayerPrefs.GetInt("Balance") < _assetData[i].Price)
            {
                item.transform.Find("Button").GetComponent<Button>().interactable = false;

                ColorBlock colors = item.transform.Find("Button").GetComponent<Button>().colors;
                colors.normalColor = new Color(0.5f, 0, 0, 1);
                item.transform.Find("Button").GetComponent<Button>().colors = colors;
                item.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().fontSize = 20;
                item.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().text = "Insufficient Funds";
                item.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
            }
        }
    }

    private void OnDestroy()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        StopAllCoroutines();
    }
}