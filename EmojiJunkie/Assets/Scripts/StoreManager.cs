using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Storage;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("Balance").GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("Balance").ToString();
        foreach (AssetData asset in FirebaseStorageController.Instance._assetData)
        {
            FirebaseStorageController.Instance.DownloadFileAsync(asset, FirebaseStorageController.DownloadType.Thumbnail);
        }
        
    }
}
