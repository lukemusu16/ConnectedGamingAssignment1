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
    private int _price;
    private CURRENNCY _currency;
    private bool _isPurcahsed;

    public virtual CURRENNCY Currency
    { get { return _currency; } set { _currency = value; } }

    public virtual int Price
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

    public virtual bool IsPurcahsed
    { get { return _isPurcahsed; } set { _isPurcahsed = value; } }

    public override string ToString()
    {
        return ($"The asset has the following values: ID:{Id}, NAME:{Name}, URL:{ThumbnailUrl}, PRICE:{Price}, CURRENCY:{Currency.ToString()}, isPurchased:{IsPurcahsed}");
    }

    public AssetData(int id, string name, string url, int price, CURRENNCY currency, string dlcType, string contentUrl, bool isPurchased)
    {
        this.Id = id;
        this.Name = name;
        this.ThumbnailUrl = url;
        this.Price = price;
        this.Currency = currency;
        this.DLCType = dlcType;
        this.ContentUrl = contentUrl;
        this.IsPurcahsed = isPurchased;
    }
}
