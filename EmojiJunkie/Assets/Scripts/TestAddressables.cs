using Firebase.Extensions;
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class TestAddressables : MonoBehaviour
{

    private FirebaseStorage _firebaseStorageInstance;

    //private AsyncOperationHandle<GameObject> handle;

    public enum DLCType
    {
        BACKGROUNDS, SKINPACKS, EFFECTS
    }

    private void Awake()
    {
        _firebaseStorageInstance = FirebaseStorage.DefaultInstance;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DownloadContent("gs://emojijunkie-c258a.appspot.com/Thumbnails/EmojiSkinPack.png", DLCType.SKINPACKS));
    }


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
                    backgroundGameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(picHeight/2, picHeight/2));
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
}
