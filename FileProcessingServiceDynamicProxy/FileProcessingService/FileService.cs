using Castle.DynamicProxy;
using DynamicProxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using FileProcessingService.Util;

namespace FileProcessingService
{
	public class FileService : IDisposable
	{
		private readonly FileSystemWatcher watcher;
		private readonly Thread workThread;
		private readonly ManualResetEvent stopWork;
		private readonly AutoResetEvent newFileEvent;
		private readonly PdfCreator pdfCreator;
		private string inDir;
		private string outDir;
		private string outWrongFileNamingDir;
		private string invalidFileSequenceDir;
		private bool disposed = false;

		public FileService()
		{
			this.InitiateDirectories();

			this.watcher = new FileSystemWatcher(this.inDir);
			this.watcher.Created += this.Watcher_Created;
			this.workThread = new Thread(this.WorkProcedure);
			this.stopWork = new ManualResetEvent(false);
			this.newFileEvent = new AutoResetEvent(false);

			IProxyGenerator generator = Ioc.GetInstance<IProxyGenerator>();

			this.pdfCreator = generator.CreateClassProxyWithTarget(Ioc.GetInstance<PdfCreator>(), Ioc.GetInstance<IInterceptor>());

			this.pdfCreator.CallbackWhenReadyToSave += this.SavePdfDocument;
			this.pdfCreator.CallbackWhenSecuenceHasWrongFileExtention += this.MoveAllFileSequenceToOtherDir;
		}

		public virtual void Start()
		{
			this.workThread.Start();
			this.watcher.EnableRaisingEvents = true;
		}

		public virtual void Stop()
		{
			this.watcher.EnableRaisingEvents = false;
			this.stopWork.Set();
			this.workThread.Join();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (this.disposed)
				return;

			if (disposing)
			{
				this.watcher.Dispose();
				this.stopWork.Dispose();
				this.newFileEvent.Dispose();
				this.pdfCreator.CallbackWhenReadyToSave -= this.SavePdfDocument;
				this.pdfCreator.CallbackWhenSecuenceHasWrongFileExtention -= this.MoveAllFileSequenceToOtherDir;
			}
			this.disposed = true;
		}

		public virtual bool TryOpen(string fileNmae, int numberOfAttempt)
		{
			for (int i = 0; i < numberOfAttempt; i++)
			{
				try
				{
					FileStream file = File.Open(fileNmae, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

					file.Close();

					return true;
				}
				catch (IOException)
				{
					Thread.Sleep(2000);
				}
			}

			return false;
		}

		private void MoveAllFileSequenceToOtherDir(object sender, EventArgs e)
		{
			foreach (var fullFilePath in this.pdfCreator.GetAllImageFilePath)
			{
				if (this.TryOpen(fullFilePath, 5))
				{
					File.Move(fullFilePath, Path.Combine(this.invalidFileSequenceDir, Path.GetFileName(fullFilePath)));
				}
			}

			if (this.TryOpen(this.pdfCreator.CurrentBarcodeFilePath, 5))
			{
				File.Delete(this.pdfCreator.CurrentBarcodeFilePath);
			}

			this.pdfCreator.Reset();
		}

		private void SavePdfDocument(object sender, EventArgs e)
		{
			this.pdfCreator.Save(this.outDir);
			foreach (var fullFilePath in this.pdfCreator.GetAllImageFilePath)
			{
				if (this.TryOpen(fullFilePath, 5))
				{
					File.Delete(fullFilePath);
				}
			}

			if (this.TryOpen(this.pdfCreator.CurrentBarcodeFilePath, 5))
			{
				File.Delete(this.pdfCreator.CurrentBarcodeFilePath);
			}

			this.pdfCreator.Reset();
		}

		private void WorkProcedure(object obj)
		{
			List<string> fileNameValidList;
			List<string> sortedFileList;
			do
			{
				fileNameValidList = this.CatchWrongFiles(this.inDir, this.outWrongFileNamingDir);

				sortedFileList = fileNameValidList.OrderBy(s => int.Parse(Path.GetFileNameWithoutExtension(s).Split('_')[1])).ToList();

				foreach (var file in sortedFileList)
				{
					if (this.stopWork.WaitOne(TimeSpan.Zero))
					{
						return;
					}

					var outFile = Path.Combine(this.outDir, Path.GetFileName(file));

					if (this.TryOpen(file, 5))
					{
						this.pdfCreator.PushFile(file);
					}
				}

			} while (WaitHandle.WaitAny(new WaitHandle[] { this.stopWork, this.newFileEvent }, 5000) != 0);
		}

		private List<string> CatchWrongFiles(string targetDir, string outDir)
		{
			Regex regex = new Regex(@".*_\d+\.");

			List<string> result = new List<string>();

			foreach (var file in Directory.EnumerateFiles(targetDir))
			{
				if (!regex.Match(Path.GetFileName(file)).Success)
				{
					this.MoveToWrongFileDirectory(outDir, file);
				}
				else if (Path.GetFileName(file).Split('_').Length > 2)
				{
					this.MoveToWrongFileDirectory(outDir, file);
				}
				else
				{
					result.Add(file);
				}
			}

			return result;
		}

		private void MoveToWrongFileDirectory(string outWrongDir, string file)
		{
			if (this.TryOpen(file, 10))
			{
				File.Move(file, Path.Combine(outWrongDir, Path.GetFileName(file)));
			}
		}

		private void Watcher_Created(object sender, FileSystemEventArgs e)
		{
			this.newFileEvent.Set();
		}

		private void InitiateDirectories()
		{
			var currentDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

			this.inDir = Path.Combine(currentDir, "in");
			this.outDir = Path.Combine(currentDir, "out");
			this.outWrongFileNamingDir = Path.Combine(currentDir, "wrongFileNamingOut");
			this.invalidFileSequenceDir = Path.Combine(currentDir, "invalidSequence");

			if (!Directory.Exists(this.inDir))
			{
				Directory.CreateDirectory(this.inDir);
			}

			if (!Directory.Exists(this.outDir))
			{
				Directory.CreateDirectory(this.outDir);
			}

			if (!Directory.Exists(this.outWrongFileNamingDir))
			{
				Directory.CreateDirectory(this.outWrongFileNamingDir);
			}

			if (!Directory.Exists(this.invalidFileSequenceDir))
			{
				Directory.CreateDirectory(this.invalidFileSequenceDir);
			}
		}
	}
}
