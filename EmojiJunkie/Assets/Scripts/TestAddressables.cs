using Firebase.Extensions;
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TestAddressables : MonoBehaviour
{

    private FirebaseStorage _firebaseStorageInstance;
    IProgress<DownloadState> progress;

    //private AsyncOperationHandle<GameObject> handle;


    private void Awake()
    {
        _firebaseStorageInstance = FirebaseStorage.DefaultInstance;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DownloadBundle());
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
        storeRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(async task =>
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

    public IEnumerator DownloadBundle()
    {
        string localFile = "file://" + Application.streamingAssetsPath + "/bundle.bundle";
        StorageReference storage = _firebaseStorageInstance.GetReferenceFromUrl("gs://emojijunkie-c258a.appspot.com/DLC/defaultlocalgroup_assets_all_ecbd5d1c653825621ea8d9e3dfb39264.bundle");
        storage.GetFileAsync(localFile).ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("File downloaded.");
                var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "bundle.bundle"));
                if (myLoadedAssetBundle == null)
                {
                    Debug.Log("Failed to load AssetBundle!");
                    return;
                }
                var prefab = myLoadedAssetBundle.LoadAsset<GameObject>("DLC/SwirlParticle.prefab");
                Instantiate(prefab, Vector3.zero, Quaternion.identity);
            }
        });
        yield return null;
    }

    private IProgress<T> IProgess<T>()
    {
        throw new NotImplementedException();
    }
}
