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



}
