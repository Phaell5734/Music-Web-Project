using Microsoft.AspNetCore.Http;
using Uyumsoft_Projem.Helpers;
using Uyumsoft_Projem.Models;
using Uyumsoft_Projem.Services;  

namespace Uyumsoft_Projem.Services;

public class SessionCartService : ICartService
{
    const string Key = "cart";
    readonly ISession _ses;

    public SessionCartService(IHttpContextAccessor acc)
    {
        _ses = acc.HttpContext!.Session;
    }

    List<CartItem> List
    {
        get => _ses.GetObject<List<CartItem>>(Key) ?? new();
        set => _ses.SetObject(Key, value);
    }

    public IEnumerable<CartItem> GetCart()
    {
        return List;
    }

    public void Add(CartItem i)
    {
        var l = List;
        l.Add(i);
        List = l;
    }

    public void Remove(int id)
    {
        var l = List;
        l.RemoveAll(x => x.SongId == id);
        List = l;
    }

    public void Clear()
    {
        _ses.Remove(Key);
    }

    public decimal Total()
    {
        return List.Sum(x => x.Price);
    }
}
