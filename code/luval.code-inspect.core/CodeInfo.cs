using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.code_inspect.core
{
    public class CodeInfo
    {
        public CodeInfo()
        {
            Procedures = new List<string?>();
            SqlStatements = new List<string?>();
            Methods = new List<ProcedureInfo>();
        }

        public string? LanguageName { get; set; }
        public string? LanguageType { get; set; }
        public string? CodeDescription { get; set; }
        public List<string?> Procedures { get; set; }
        public List<ProcedureInfo> Methods { get; set; }
        public List<string?> SqlStatements { get; set; }
        public string? CSharpCode { get; set; }
        public string? OriginalCode { get; set; }
        public string? PseudoCode { get; set; }
    }
}
