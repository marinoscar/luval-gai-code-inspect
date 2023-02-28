namespace luval.code_inspect.core
{
    public class GitCodeExtractor
    {
        public async static Task<string> GetCodeAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
            var result = string.Empty;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) { throw new ArgumentException("Invalid url", nameof(url)); }
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }
    }
}