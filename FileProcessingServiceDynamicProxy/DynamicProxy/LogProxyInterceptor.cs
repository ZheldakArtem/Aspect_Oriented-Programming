using Castle.DynamicProxy;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DynamicProxy
{
	public class LogProxyInterceptor : IInterceptor
	{
		private static Logger infoLog = LogManager.GetLogger("Info");

		public void Intercept(IInvocation invocation)
		{
			StringBuilder infoStr = new StringBuilder();

			object[] args = invocation.Arguments;

			infoStr.AppendLine($"Date: {DateTime.Now}, Called method: \"{invocation.Method.Name}\"; Argument(s): {args.Length}");

			for (int i = 0; i < args.Length; i++)
			{
				XmlSerializer s = new XmlSerializer(args[i].GetType());
				try
				{
					using (var format = new MemoryStream())
					{
						s.Serialize(format, args[i]);

						infoStr.Append($"<{i}>");
						infoStr.AppendLine(Encoding.UTF8.GetString(format.ToArray()).Replace("<?xml version=\"1.0\"?>", ""));
						infoStr.AppendLine($"</{i}>");
					}
				}
				catch
				{
					infoStr.AppendLine("Not serializable");
				}

				invocation.Proceed();

				infoStr.AppendLine($"Return value: {invocation.ReturnValue}");

				infoLog.Info(infoStr);
				infoStr.Clear();
			}
		}
	}
}
