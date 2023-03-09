using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.code_inspect.core
{
    public class Voice2Text
    {

        private readonly string _token;

        public Voice2Text(string apiKey)
        {
            _token = apiKey;
        }

        public async Task<string> GetTextAsync(FileInfo audioFile)
        {
            return await RunCommandAsync(audioFile, string.Format("audio/{0}", 
                audioFile.Extension.Replace(".", "")));
        }

        private async Task<string> RunCommandAsync(FileInfo file, string mediaType)
        {
            var endpoint = "https://api.openai.com/v1/audio/transcriptions";
            var model = "whisper-1";

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(File.ReadAllBytes(file.FullName));
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
                    content.Add(fileContent, "file", Path.GetFileName(file.FullName));

                    content.Add(new StringContent(model), "model");

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                    var response = await client.PostAsync(endpoint, content);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
