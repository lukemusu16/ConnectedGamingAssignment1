using Firebase.Extensions;
using Firebase.Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TestAddressables : MonoBehaviour
{

    private FirebaseStorage _firebaseStorageInstance;

    //private AsyncOperationHandle<GameObject> handle;


    private void Awake()
    {
        _firebaseStorageInstance = FirebaseStorage.DefaultInstance;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DownloadCatalog());
        //DownloadFileAsync("gs://emojijunkie-c258a.appspot.com/DLC/EmojiBG1.prefab");
/*        handle = Addressables.LoadAsset<GameObject>(address);
        handle.Completed += Handle_Completed;*/
    }

/*    private void Handle_Completed(AsyncOperationHandle<GameObject> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            Instantiate(operation.Result, transform);
        }
        else
        {
            //Debug.Log($"Asset for {address} failed to load");
        }
    }

    private void OnDestroy()
    {
        Addressables.Release(handle);
    }*/

    public void DownloadFileAsync(string url)
    {
        // Create a storage reference from our storage service
        StorageReference storeRef = _firebaseStorageInstance.GetReferenceFromUrl(url);

        // Download in memory with a maximum allowed size of 1MB (1 * 1024 * 1024 bytes)
        const long maxAllowedSize = 1 * 2048 * 2048;
        storeRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogException(task.Exception);
                // Uh-oh, an error occurred!
            }
            else
            {
                byte[] fileContents = task.Result;
                Debug.Log($"{storeRef.Name} finished downloading!");
                Debug.Log($"{task.Result}");
            }
        });
    }

    public IEnumerator DownloadCatalog()
    {

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("prefab");
        yield return handle;
    }
}
