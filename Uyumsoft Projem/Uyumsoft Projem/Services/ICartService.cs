using System.Collections.Generic;
using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.Services;

public interface ICartService
{
    IEnumerable<CartItem> GetCart();
    void Add(CartItem item);
    void Remove(int songId);
    void Clear();
    decimal Total();
}
