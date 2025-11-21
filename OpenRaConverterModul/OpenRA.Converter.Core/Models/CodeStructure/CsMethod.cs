using System.Collections.Generic;
using System.Text;

namespace OpenRA.Converter.Core.Models.CodeStructure
{
    /// <summary>
    /// Represents a method within a C# class.
    /// </summary>
    public class CsMethod
    {
        public string Name { get; set; } = string.Empty;
        public string ReturnType { get; set; } = "void";
        public string AccessModifier { get; set; } = "public";
        public List<CsParameter> Parameters { get; set; } = new();

        /// <summary>
        /// The lines of code inside the method body.
        /// </summary>
        public List<string> BodyLines { get; set; } = new();

        /// <summary>
        /// OpenRA Interface methods often require explicit implementation (e.g., "void ITick.Tick(...)").
        /// </summary>
        public string? ExplicitInterfaceImplementation { get; set; }

        public string GetSignature()
        {
            var sb = new StringBuilder();
            sb.Append(AccessModifier);
            sb.Append(" ");
            if (!string.IsNullOrEmpty(ExplicitInterfaceImplementation))
            {
                // Explicit interface implementations don't usually have access modifiers in C#, 
                // but we keep the logic flexible here.
                sb.Append(ReturnType);
                sb.Append(" ");
                sb.Append(ExplicitInterfaceImplementation);
                sb.Append(".");
                sb.Append(Name);
            }
            else
            {
                sb.Append(ReturnType);
                sb.Append(" ");
                sb.Append(Name);
            }

            sb.Append("(");
            sb.Append(string.Join(", ", Parameters.ConvertAll(p => $"{p.Type} {p.Name}")));
            sb.Append(")");

            return sb.ToString();
        }
    }
}