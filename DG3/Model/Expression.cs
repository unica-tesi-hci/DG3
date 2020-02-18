using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace GestureStudio
{
	class Expression
	{
		public string title;
		public string expression;
		public bool generated = false;

		public override string ToString()
		{
			return (title);
		}

		public string GetExpression()
		{
			return (title + " = " + expression);
		}

		public Expression(string code)
		{
			if (code.Split('=').Length > 1)
			{
				this.expression = code.Split('=')[1].Trim();
				this.title = code.Split('=')[0].Trim();
			}
			else
			{
				this.expression = code.Trim();
				this.title = "Undefined";
			}
		}

		public Expression(string title, string expression)
		{
			this.title = title;
			this.expression = expression;
		}
	}
}
