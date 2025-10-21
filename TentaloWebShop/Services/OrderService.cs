using Syncfusion.Blazor.Chart3D.Internal;
using TentaloWebShop.Models;

namespace TentaloWebShop.Services
{
    public class OrderService
    {
        private readonly RestDataService _rest;
        private List<OrderNAVCabecera>? _cache;
        private readonly Dictionary<string, OrderNAVCabecera> _orderCache = new();

        public OrderService(RestDataService rest) => _rest = rest;

        public async Task<List<OrderNAVCabecera>> GetOrdersAsync(string cliente)
        {
            if (_cache is not null) return _cache;
            
            try 
            {
                var orders = await _rest.ListaPedidosCabeceraVenta(cliente);
                _cache = orders ?? new List<OrderNAVCabecera>();
                
                // Cachear cada pedido individualmente para acceso rápido
                foreach (var order in _cache)
                {
                    if (!string.IsNullOrEmpty(order.No))
                        _orderCache[order.No] = order;
                }
                
                return _cache;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR GetOrdersAsync] {ex.Message}");
                return new List<OrderNAVCabecera>();
            }
        }

        public async Task<OrderNAVCabecera?> GetOrderByIdAsync(string orderId, string cliente)
        {
            // Intentar obtener del cache primero
            if (_orderCache.TryGetValue(orderId, out var cachedOrder))
                return cachedOrder;

            // Si no está en cache, cargar todos los pedidos
            await GetOrdersAsync(cliente);
            
            // Intentar de nuevo después de cargar
            return _orderCache.TryGetValue(orderId, out var order) ? order : null;
        }

        public void ClearCache()
        {
            _cache = null;
            _orderCache.Clear();
        }

        public decimal GetOrderTotal(OrderNAVCabecera order)
        {
            return (decimal)order.Amount_Including_VAT;
        }

        public decimal GetOrderVAT(OrderNAVCabecera order)
        {
            return (decimal)order.VAT_Amount;
        }

        public int GetOrderLineCount(OrderNAVCabecera order)
        {
            return order.Lines?.Count ?? order.LineasVenta?.Count ?? 0;
        }

        public async Task<Status> BorrarPedido(string id) 
        {
            var res = await _rest.BorrarPedido(id);
            return res;
        }
    }
}