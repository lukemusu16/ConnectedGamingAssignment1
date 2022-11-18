using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class StoreManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (AssetData asset in FirebaseStorageController.Instance._assetData)
        {
            FirebaseStorageController.Instance.DownloadFileAsync(asset.ThumbnailUrl, FirebaseStorageController.DownloadType.Thumbnail);
        }
        
    }
}
