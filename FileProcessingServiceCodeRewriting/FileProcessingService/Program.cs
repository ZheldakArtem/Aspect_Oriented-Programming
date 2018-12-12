using System;
using System.Diagnostics;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using Topshelf;

namespace FileProcessingService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			HostFactory.Run(
				conf => {
					conf.Service<FileService>(
					   s =>
					   {
						   s.ConstructUsing(() => new FileService());
						   s.WhenStarted(serv => serv.Start());
						   s.WhenStopped(serv => serv.Stop());
					   });
					conf.SetServiceName("FileProcessingService");
					conf.SetDisplayName("File Processing Service");
					conf.StartAutomaticallyDelayed();
					conf.RunAsLocalSystem();
				});
		}
	}
}
