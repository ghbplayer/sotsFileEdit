using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.CodeDom;
using Microsoft.CSharp;
using System.Reflection;
using System.Globalization;



namespace sotsedit
{
    class Modifier
    {
        public Modifier(string expression)
		{
			provider = new CSharpCodeProvider();
			parameters = new CompilerParameters();

			string source = @"
class DynamicExpression
{
    public static <!Value!> f(<!Value!> v)
    {
        <!expression!>
    }
}
";

			string codeNR = source.Replace("<!expression!>", expression);
			string codeR = source.Replace("<!expression!>", "return v " + expression + ";");
			string codeD = codeNR.Replace("<!Value!>", "double");
			string codeDR = codeR.Replace("<!Value!>", "double");
			//string codeS = codeNR.Replace("<!Value!>", "string");
			string codeSR = source.Replace("<!expression!>", "return \"" + expression + "\";").Replace("<!Value!>", "string");
			StringBuilder errors = new StringBuilder();

			if (compile(codeDR, errors))
			{
				isDouble = true;
				return;
			}
			if (compile(codeD, errors))
			{
				isDouble = true;
				return;
			}
			if (compile(codeSR, errors))
				return;
			//if (compile(codeS, errors))
			//	return;
			throw new Exception(errors.ToString());
		}

		private bool compile(string source, StringBuilder sb)
		{
			compileUnit = new CodeSnippetCompileUnit(source);
			results = provider.CompileAssemblyFromDom(parameters, compileUnit);
			if (results.Errors.Count > 0)
			{
				sb.AppendLine("Your script didn't compile.");
				foreach (CompilerError error in results.Errors)
					sb.AppendLine(error.ErrorText);
				sb.AppendLine();
				sb.AppendLine(source);
				return false;
			}
			type = results.CompiledAssembly.GetType("DynamicExpression");
			method = type.GetMethod("f");
			return true;
		}

		public void apply(ref string value)
		{
			if (!isDouble)
			{
				value = (string)method.Invoke(null, new object[] { value });
				return;
			}
			double v = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
			v = (double)method.Invoke(null, new object[] { v });
			value = Convert.ToString(v);
		}

		CodeSnippetCompileUnit compileUnit;
        CodeDomProvider provider;
        CompilerParameters parameters;
        CompilerResults results;
        Type type;
        MethodInfo method;
		bool isDouble;
    }
}
