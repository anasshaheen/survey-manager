namespace SurveyManager.API.Options
{
    public class JwtConfig
    {
        public string SigningKey { get; set; }
        public string Issuer { get; set; }
        public string Site { get; set; }
    }
}