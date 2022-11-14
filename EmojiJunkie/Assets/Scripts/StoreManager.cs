using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class StoreManager : MonoBehaviour
{
    private FirebaseStorage _firebaseStorageInstance;
    private FirebaseStorageController fsc;
    [SerializeField] private GameObject DLCItemPrefab;
    private GameObject _thumbnailContainer;
    private List<GameObject> _DLCItemsList;
    private List<AssetData> _assetData;


    // Start is called before the first frame update
    void Start()
    {

        fsc = transform.Find("DLC Manager").GetComponent<FirebaseStorageController>();

        _thumbnailContainer = GameObject.Find("Content");
    }

    public void DownloadFileAsync(string url)
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

            }
        });
    }
}
