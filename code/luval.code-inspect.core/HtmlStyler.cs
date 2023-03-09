using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace luval.code_inspect.core
{
    public class HtmlStyler
    {
        public static string Style(CodeInfo codeInfo)
        {
            var html = new HtmlDocument();
            html.Load(File.OpenRead("SampleReport.html"));
            var body = html.DocumentNode.SelectSingleNode("//div");
            body.AppendChild(HtmlNode.CreateNode("<h2>Code Description</h2>"));
            body.AppendChild(HtmlNode.CreateNode(string.Format("<p>{0}</p>", codeInfo.CodeDescription)));

            if (codeInfo.Procedures.Any())
                PrintProcedures(codeInfo, body);
            if (codeInfo.SqlStatements.Any())
                PrintSqlStatements(codeInfo, body);
            if (codeInfo.Methods.Any())
                PrintMethods(codeInfo, body);

            body.AppendChild(HtmlNode.CreateNode("<h2>Code Analyzed</h2>"));
            body.AppendChild(CreateCodeTab(codeInfo));
            var fileName = string.Format("{0}-{1}.html", codeInfo.LanguageName, DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff")).ToLower();
            html.Save(fileName);
            return fileName;
        }

        private static void PrintMethods(CodeInfo codeInfo, HtmlNode body)
        {
            body.AppendChild(HtmlNode.CreateNode("<h2>Methods</h2>"));
            var table = HtmlNode.CreateNode("<table class=\"table table-striped\"></table>");
            var tableBody = new StringWriter();
            tableBody.WriteLine(@"<thead><tr><th scope=""col"">Method</th><th scope=""col"">Description</th></tr></thead>");
            tableBody.WriteLine("<tbody>");
            foreach (var method in codeInfo.Methods)
            {
                tableBody.WriteLine("<tr><th scope=\"row\">{0}</th><td>{1}</td></tr>",
                    HttpUtility.HtmlEncode(method.Name),
                    HttpUtility.HtmlEncode(method.Description));
            }
            tableBody.WriteLine("</tbody>");
            table.InnerHtml = tableBody.ToString();
            body.AppendChild(table);
        }

        private static void PrintSqlStatements(CodeInfo codeInfo, HtmlNode body)
        {
            body.AppendChild(HtmlNode.CreateNode("<h2>Sql Statements</h2>"));
            var sqlStatements = HtmlNode.CreateNode("<ul></ul>");
            foreach (var statement in codeInfo.SqlStatements)
            {
                sqlStatements.AppendChild(HtmlNode.CreateNode(string.Format("<li><pre><code class=\"lang-sql\">{0}</code></pre></li>", statement)));
            }
            body.AppendChild(sqlStatements);
        }

        private static void PrintProcedures(CodeInfo codeInfo, HtmlNode body)
        {
            body.AppendChild(HtmlNode.CreateNode("<h2>Methods</h2>"));
            var methods = HtmlNode.CreateNode("<ul></ul>");
            foreach (var procedure in codeInfo.Procedures)
            {
                methods.AppendChild(HtmlNode.CreateNode(string.Format("<li>{0}</li>", procedure)));
            }
            body.AppendChild(methods);
        }

        private static HtmlNode CreateCodeTab(CodeInfo codeInfo)
        {
            var codeClass = codeInfo.LanguageName.ToLower().Equals("cobol") ? "cobol" : "cpp";
            var html = @"
<nav>
  <div class=""nav nav-tabs"" id=""nav-tab"" role=""tablist"">
    <button class=""nav-link active"" id=""nav-home-tab"" data-bs-toggle=""tab"" data-bs-target=""#nav-home"" type=""button"" role=""tab"" aria-controls=""nav-home"" aria-selected=""true"">{0}</button>
    <button class=""nav-link"" id=""nav-profile-tab"" data-bs-toggle=""tab"" data-bs-target=""#nav-profile"" type=""button"" role=""tab"" aria-controls=""nav-profile"" aria-selected=""false"">C#</button>
  </div>
</nav>
<div class=""tab-content"" id=""nav-tabContent"">
  <div class=""tab-pane fade show active"" id=""nav-home"" role=""tabpanel"" aria-labelledby=""nav-home-tab""><pre><code class=""lang-{1}"">{2}</code></pre></div>
  <div class=""tab-pane fade"" id=""nav-profile"" role=""tabpanel"" aria-labelledby=""nav-profile-tab""><pre><code class=""lang-{3}"">{4}</code></pre></div>
</div>
";
            var node = HtmlNode.CreateNode("<div></div>");
            node.InnerHtml = string.Format(html, codeInfo.LanguageName,
                codeInfo.LanguageName, HttpUtility.HtmlEncode(codeInfo.OriginalCode), "none",
                HttpUtility.HtmlEncode(codeInfo.PseudoCode));
            return node;
        }
    }
}
