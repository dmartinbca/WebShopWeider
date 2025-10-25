
using TentaloWebShop.Models;

namespace TentaloWebShop.Services
{
    public class OrderService
    {
        private readonly RestDataService _rest;
        private readonly AuthService _auth;
        private List<OrderNAVCabecera>? _cache;
        private readonly Dictionary<string, OrderNAVCabecera> _orderCache = new();
        private string? _lastCustomerNo;

        public OrderService(RestDataService rest, AuthService auth)
        {
            _rest = rest;
            _auth = auth;
            // Suscribirse a cambios de cliente
            _auth.OnCustomerChanged += OnCustomerChanged;
        }

        // ✅ CAMBIO: de void a async Task
        private async Task OnCustomerChanged()
        {
            Console.WriteLine("[OrderService.OnCustomerChanged] Limpiando caché");
            ClearCache();
            await Task.CompletedTask;
        }

        public async Task<List<OrderNAVCabecera>> GetOrdersAsync(string? cliente = null)
        {
            // Si no se proporciona cliente, usar el actual
            string efectiveCliente = cliente ?? GetEffectiveCustomerNo();

            // Si cambió el cliente, invalidar caché
            if (_lastCustomerNo != efectiveCliente)
            {
                ClearCache();
                _lastCustomerNo = efectiveCliente;
            }

            if (_cache is not null) return _cache;

            try
            {
                var orders = await _rest.ListaPedidosCabeceraVenta(efectiveCliente);
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

        public async Task<OrderNAVCabecera?> GetOrderByIdAsync(string orderId, string? cliente = null)
        {
            // Si no se proporciona cliente, usar el actual
            string efectiveCliente = cliente ?? GetEffectiveCustomerNo();

            // Intentar obtener del cache primero
            if (_orderCache.TryGetValue(orderId, out var cachedOrder))
                return cachedOrder;

            // Si no está en cache, cargar todos los pedidos
            await GetOrdersAsync(efectiveCliente);

            // Intentar de nuevo después de cargar
            return _orderCache.TryGetValue(orderId, out var order) ? order : null;
        }

        private string GetEffectiveCustomerNo()
        {
            // Usar el CurrentCustomer que puede ser el cliente seleccionado por Sales Team
            return _auth.CurrentCustomer?.CustNo ?? _auth.CurrentUser?.CustomerNo ?? "";
        }

        public void ClearCache()
        {
            _cache = null;
            _orderCache.Clear();
            _lastCustomerNo = null;
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
            // Limpiar caché después de borrar
            ClearCache();
            return res;
        }
    }
}
