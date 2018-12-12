using NLog;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PostSharpAspectLog
{
	[Serializable]
	public class LoggingAspect : OnMethodBoundaryAspect
	{
		private static Logger infoLog = LogManager.GetLogger("Info");

		public override void OnEntry(MethodExecutionArgs args)
		{
			StringBuilder infoStr = new StringBuilder();
			infoStr.AppendLine($"Date: {DateTime.Now}, Called method: \"{args.Method.Name}\"; Argument(s): {args.Arguments.Count}");

			for (int i = 0; i < args.Arguments.Count; i++)
			{
				XmlSerializer s = new XmlSerializer(args.Arguments[i].GetType());
				try
				{
					using (var format = new MemoryStream())
					{
						s.Serialize(format, args.Arguments[i]);

						infoStr.Append($"<{i}>");
						infoStr.AppendLine(Encoding.UTF8.GetString(format.ToArray()).Replace("<?xml version=\"1.0\"?>", string.Empty));
						infoStr.AppendLine($"</{i}>");
					}
				}
				catch
				{
					infoStr.AppendLine("Not serializable");
				}

				infoLog.Info(infoStr);
				infoStr.Clear();
			}
		}

		public override void OnSuccess(MethodExecutionArgs args)
		{
			string retVal = args.ReturnValue == null ? "void" : args.ReturnValue.ToString();
			infoLog.Info($"Method name: {args.Method.Name} called success; Return value = {retVal}");
		}
	}
}
