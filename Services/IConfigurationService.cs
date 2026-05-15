namespace EXE201_Backend.Services
{
    public interface IConfigurationService
    {
        string ALLOWED_ORIGINS { get; set; }
        string DATABASE_CONNECTION { get; set; }
        string JWT_AUDIENCE { get; set; }
        string JWT_ISSUER { get; set; }
        string JWT_KEY { get; set; }
        string SE_MERCHANT { get; set; }
        string SE_SECRET { get; set; }
        string VNP_RETURN { get; set; }
        string VNP_SECRET { get; set; }
        string VNP_TMN { get; set; }
    }
}