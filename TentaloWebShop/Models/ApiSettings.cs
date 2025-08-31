namespace TentaloWebShop.Models
{
    public class ApiSettings
    {
        public string Empresa { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string UsuarioCloud { get; set; } = string.Empty;
        public string PassCloud { get; set; } = string.Empty;
        public string Entorno { get; set; } = string.Empty;
        public string APIPublisher { get; set; } = string.Empty;
        public string APIGroup { get; set; } = string.Empty;
        public string APIVersion { get; set; } = string.Empty;
    }

}