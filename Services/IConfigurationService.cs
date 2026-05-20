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
        string IMAGE_DIR { get; set; }
        int IMAGE_EXPIRE_SEC { get; set; }
        string SMTP_SERVER { get; set; }
        int SMTP_PORT { get; set; }
        string SMTP_EMAIL { get; set; }
        string SMTP_PASSWORD { get; set; }
        string SMTP_FROM { get; set; }
        string SELF_SCHEME { get; set; }
        string SELF_HOST { get; set; }
        int SERVICE_COST_PERCENTAGE { get; set; }
        int PAYMENT_EXPIRE_SEC { get; set; }
        string SE_RETURN { get; set; }
        string SE_ERROR { get; set; }
        string SE_CANCEL { get; set; }
    }
}