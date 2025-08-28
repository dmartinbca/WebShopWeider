using TentaloWebShop.Models;

namespace TentaloWebShop.Services
{
    public class EstadisticasService
    {
        private readonly RestDataService _rest;
        public EstadisticasService(RestDataService rest) => _rest = rest;
        // ⚠️ Rellena aquí tus familias y subfamilias reales
        private List<Estadisticas> _families = new List<Estadisticas>();

        public async Task<List<Estadisticas>> GetEstadisticas(string cliente)
        {
            var listEst = new List<Estadisticas>();
            HttpClient httpClient = new HttpClient();
            var eest = await _rest.GetEstadisticasVentas(cliente);
            if (eest != null)
            {
                listEst= eest;
                return listEst;
            }
            else
            {
                return listEst;
            }
        }
    }
}
