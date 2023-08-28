namespace API.Entities
{
    public class Url
    {
        public Url(string redirectUrl)
        {
            RedirectUrl = redirectUrl;
        }
        public string RedirectUrl { get; set; }
        public List<string> VisitHistory { get; set; } = new List<string>();
        public int Ckicks { get; set; } = 0;

    }
}