using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetData
{
    public enum CURRENNCY
    {
        Default,
        Emojicoins
    }

    private int _id;
    private string _name;
    private string _DLCType;
    private string _thumbnailUrl;
    private string _contentUrl;
    private float _price;
    private CURRENNCY _currency;

    public virtual CURRENNCY Currency
    { get { return _currency; } set { _currency = value; } }

    public virtual float Price
    { get { return _price; } set { _price = value; } }

    public virtual string ThumbnailUrl
    { get { return _thumbnailUrl; } set { _thumbnailUrl = value; } }

    public virtual string ContentUrl
    { get { return _contentUrl; } set { _contentUrl = value; } }

    public virtual string Name
    { get { return _name; } set { _name = value; } }

    public virtual string DLCType
    { get { return _DLCType; } set { _DLCType = value; } }

    public virtual int Id
    { get { return _id; } set { _id = value; } }

    public override string ToString()
    {
        return ($"The asset has the following values: ID:{Id}, NAME:{Name}, URL:{ThumbnailUrl}, PRICE:{Price}, CURRENCY:{Currency.ToString()}");
    }

    public AssetData(int id, string name, string url, float price, CURRENNCY currency, string dlcType, string contentUrl)
    {
        this.Id = id;
        this.Name = name;
        this.ThumbnailUrl = url;
        this.ContentUrl = contentUrl;
        this.Price = price;
        this.Currency = currency;
        this.DLCType = dlcType;
    }
}
