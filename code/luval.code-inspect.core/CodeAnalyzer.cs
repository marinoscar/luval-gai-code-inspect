using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace luval.code_inspect.core
{
    public class CodeAnalyzer
    {
        private readonly string _codeContent;
        private readonly string _apiKey;
        private readonly StringWriter _context;
        private readonly JObject _config;

        public event EventHandler<StreamEventArgs> StreamEvent;

        protected virtual void OnStreamEvent(string? message)
        {
            EventHandler<StreamEventArgs> eventHandler = StreamEvent;
            if (eventHandler != null) eventHandler(this, new StreamEventArgs(message));

        }

        public CodeAnalyzer(string apiKey, string codeContent, string promptConfig)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentNullException(nameof(apiKey));
            if (string.IsNullOrWhiteSpace(codeContent)) throw new ArgumentNullException(nameof(codeContent));
            if (string.IsNullOrWhiteSpace(promptConfig)) throw new ArgumentNullException(nameof(promptConfig));


            _codeContent = GitCodeExtractor.GetCodeAsync(codeContent).Result;
            _apiKey = apiKey;
            _config = JObject.Parse(promptConfig);
            _context = new StringWriter();
            _context.WriteLine("I will ask you questions about the following code");
            _context.WriteLine();
            _context.WriteLine(_codeContent);
            _context.WriteLine();

        }

        public async Task<CodeInfo> GetCodeDetails()
        {
            var response = new CodeInfo();
            var info = await GetCodeInfo();
            response.LanguageName = info["languageName"].Value<string>();
            response.LanguageType = info["languageType"].Value<string>();
            response.CodeDescription = await GetCodeDescription();
            response.Procedures = await GetSubProcedures();
            response.SqlStatements = await GetSqlStatements();
            response.CSharpCode = await GetCSharpCode();
            response.OriginalCode = _codeContent;

            return response;
        }

        public async Task<string> RunPrompt(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));

            var request = new StringWriter();
            var response = new StringWriter();
            var api = new OpenAI_API.OpenAIAPI(_apiKey);
            request.Write(_context.ToString());
            request.WriteLine("{0}", prompt);

            var tokens = (int)(_context.ToString().Length / 2.9);
            if (tokens > 4097) throw new ArgumentOutOfRangeException(nameof(prompt), "The prompt exceeds the max number of tokens allowed");

            var maxTokens = (int)(4097 - tokens);
            // for example
            await foreach (var token in api.Completions.StreamCompletionEnumerableAsync(request.ToString(), max_tokens: maxTokens))
            {
                response.Write(token);
                OnStreamEvent(token.ToString());
            }

            return response.ToString();
        }

        private async Task<string?> GetCSharpCode()
        {
            var p = GetPrompt("csharpCode");
            return await RunPrompt(p);
        }


        private async Task<List<string?>> GetCSVData(string prompt)
        {
            var r = await RunPrompt(prompt);
            return r.Split(",").Select(s => (string?)string.Format("\"{0}\"", s.Trim())).ToList();
        }

        private async Task<List<string?>> GetSqlStatements()
        {
            var p = GetPrompt("sqlStatements");
            return await GetCSVData(p);
        }

        private async Task<List<string?>> GetSubProcedures()
        {
            var p = GetPrompt("procSubProcesses");
            return await GetCSVData(p);
        }

        private async Task<string?> GetCodeDescription()
        {
            var p = GetPrompt("codeDescription");
            var r = await RunPrompt(p);
            return r;
        }

        private async Task<JObject> GetCodeInfo()
        {
            var p = GetPrompt("languageInfo");
            var r = await RunPrompt(p);
            return JObject.Parse(r);
        }



        private string GetPrompt(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            var res = _config.Value<string>(key);
            return res;
        }
    }
}

