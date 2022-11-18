using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Storage;

using UnityEngine;
using UnityEngine.UI;

public class FirebaseStorageController : MonoBehaviour
{
    FirebaseFirestore db;
    private FirebaseStorage _firebaseStorageInstance;
    private GameObject DLCItemPrefab;
    private GameObject _thumbnailContainer;
    public List<GameObject> _DLCItemsList;
    public List<AssetData> _assetData;

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
            float price = (priceStr != null) ? float.Parse(priceStr) : 0f;
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
        DLCItem.transform.Find("Price").GetComponent<TMPro.TextMeshProUGUI>().text = _assetData[_DLCItemsList.Count].Price.ToString();
        DLCItem.transform.Find("DLCType").GetComponent<TMPro.TextMeshProUGUI>().text = _assetData[_DLCItemsList.Count].DLCType.ToString();

        DLCItem.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            _assetData[int.Parse(DLCItem.name)].IsPurcahsed = true;
            print("we here");
            DocumentReference docRef = db.Collection("playerData").Document(GlobalValues.PlayerID).Collection("Assets").Document("Asset " + int.Parse(DLCItem.name));
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "isPurchase", _assetData[int.Parse(DLCItem.name)].IsPurcahsed}
            };

            docRef.UpdateAsync(updates);


        });

        _DLCItemsList.Add(DLCItem);
        
        
    }


    public IEnumerator DownloadContent(string url, DLCType type)
    {
        StorageReference storage = _firebaseStorageInstance.GetReferenceFromUrl(url);

        if (type == DLCType.EFFECTS)
        {
            string localFile = "file://" + Application.streamingAssetsPath + "/bundle.bundle";
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
                    backgroundGameObject.transform.position = Vector2.zero;
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

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            Sprite newSprite = Sprite.Create(tex, new Rect(i * 128, j * 128, 128, 128), new Vector2(0.5f, 0.5f));
                            GameObject n = new GameObject();
                            SpriteRenderer sr = n.AddComponent<SpriteRenderer>();
                            sr.sprite = newSprite;
                            n.transform.position = new Vector3(i * 2, j * 2, 0);
                            n.transform.parent = spritesRoot.transform;
                        }
                    }
                }
            });
        }

        yield return null;
    }

    public void PopulateGameScene()
    {
        Query colRef= db.Collection("playerData").Document(GlobalValues.PlayerID).Collection("Assets");
        print(colRef);
        colRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            print(task);
            QuerySnapshot allAssets = task.Result;

            foreach (DocumentSnapshot docSnapshot in allAssets)
            {
                DocumentReference docRef = docSnapshot.Reference;

                docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    Dictionary<string, object> assetData = task.Result.ToDictionary();
                    object purchased;

                    if (assetData.TryGetValue("AssetID", out purchased) && purchased.Equals(true))
                    {
                        string url;
                        object content;

                        assetData.TryGetValue("AssetContentUrl", out content);

                        url = content.ToString();
                        print(url);

                        //DownloadContent
                    }
                    

                    
                });
            }
        });

    }
}