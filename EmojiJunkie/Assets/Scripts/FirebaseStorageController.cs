using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Firebase.Extensions;
using Firebase.Storage;

using UnityEngine;
using UnityEngine.UI;

public class FirebaseStorageController : MonoBehaviour
{

    private FirebaseStorage _firebaseStorageInstance;
    [SerializeField] private GameObject DLCItemPrefab;
    private GameObject _thumbnailContainer;
    public List<GameObject> _DLCItemsList;
    private List<AssetData> _assetData;
    public enum DownloadType
    {
        Thumbnail, Manifest, Item
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
    }

    private void Start()
    {
        _thumbnailContainer = GameObject.Find("Content");
        _DLCItemsList = new List<GameObject>();
        //Download Manifest
        DownloadFileAsync("gs://emojijunkie-c258a.appspot.com/manifest.xml", DownloadType.Manifest);
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
                Debug.Log($"{imageRef.Name} finished downloading!");
                if (dType == DownloadType.Thumbnail)
                {
                    //Load Image
                    StartCoroutine(LoadDLCItem(fileContents));
                }
                else if (dType == DownloadType.Manifest)
                {
                    //Load the manifest
                    StartCoroutine(LoadManifest(fileContents));
                }
                else if (dType == DownloadType.Item)
                { 
                    
                }

            }
        });
    }

    IEnumerator LoadManifest(byte[] fileContents)
    {
        //Converting from byte array to String UTF8
        XDocument manifest = XDocument.Parse(System.Text.Encoding.UTF8.GetString(fileContents));
        _assetData = new List<AssetData>();

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

            AssetData newAsset = new AssetData(id, nameStr, urlStr, price, currency, dlcTypeStr, contentUrlStr);
            _assetData.Add(newAsset);

            DownloadFileAsync(newAsset.ThumbnailUrl, DownloadType.Thumbnail);
        }

        yield return null;
    }

    IEnumerator LoadDLCItem(byte[] fileContents)
    {
        // Display the image inside _imagePlaceholder
        GameObject DLCItem = Instantiate(DLCItemPrefab, _thumbnailContainer.transform.position, Quaternion.identity, _thumbnailContainer.transform);
        DLCItem.name = "DownloadedItem_" + _assetData[_DLCItemsList.Count].Id;
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(fileContents);
        DLCItem.GetComponentInChildren<RawImage>().texture = tex;
        DLCItem.transform.Find("Price").GetComponent<TMPro.TextMeshProUGUI>().text = _assetData[_DLCItemsList.Count].Price.ToString();
        DLCItem.transform.Find("DLCType").GetComponent<TMPro.TextMeshProUGUI>().text = _assetData[_DLCItemsList.Count].DLCType.ToString();

        /*DLCItem.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => {
            DownloadFileAsync()
        });*/

        _DLCItemsList.Add(DLCItem);
        yield return DLCItem;
    }
}