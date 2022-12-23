using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class TestLogger
{
	static FileStream fs;
	static StreamReader sr;
	static FileSystemWatcher fswatcher;
	
	public static void Main(String[] args)
	{
		WatcherInitialize();
		LatestLogFileOpen();
		
		Console.WriteLine("exitで終了します");
		while(true)
		{
			String str = Console.ReadLine();
			if("exit".Equals(str))break;
		}
	}
	// FileSystemWatcherの初期化
	static void WatcherInitialize()
	{
		fswatcher = new FileSystemWatcher();
		fswatcher.Path = Regex.Replace(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\\[^\\]+$", @"\LocalLow\VRChat\VRChat"); // VRChatのログがあるフォルダを監視
		fswatcher.NotifyFilter = 
                NotifyFilters.FileName |
                NotifyFilters.Size;
		fswatcher.Filter = "output_log_*.txt";
		fswatcher.Created += new FileSystemEventHandler(LogFileCreated);
		fswatcher.Changed += new FileSystemEventHandler(LogFileUpdated);
		fswatcher.EnableRaisingEvents = true;
	}
	// 最新のログファイルを開く
	static void LatestLogFileOpen()
	{
		DirectoryInfo dir = new DirectoryInfo(fswatcher.Path);
		FileInfo fi = dir.GetFiles(fswatcher.Filter).OrderByDescending(p => p.LastWriteTime).ToArray()[0];
		
		OpenLogFile(fi.DirectoryName+"\\"+fi.Name);
	}
	
	// ログファイルをFileStreamで開く、読み取り用のStreamReaderも初期化する
	static void OpenLogFile(String fullpath)
	{
		Console.WriteLine("Open: "+fullpath);
		
		if(fs!=null)fs.Close();
		if(sr!=null)sr.Close();
		
		fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		sr = new StreamReader(fs, System.Text.Encoding.UTF8);
		
		fs.Seek(0, SeekOrigin.End); // すでに書き込まれている内容はいらないので、末尾を基準に0シークする
		
	}
	
	static void LogFileCreated(Object source, FileSystemEventArgs e)
	{
		Console.WriteLine("========== LogFileCreated Process ==========");
		OpenLogFile(e.FullPath);
		Console.WriteLine();
	}
	static void LogFileUpdated(Object source, FileSystemEventArgs e)
	{
		if(fs == null)
		{
			Console.WriteLine("fs null");
			return;
		}
		if(!e.FullPath.Equals(fs.Name))
		{
			Console.WriteLine(".");
			//Console.WriteLine("!e.FullPath.Equals(fs.Name)");
			//Console.WriteLine(e.FullPath);
			//Console.WriteLine(fs.Name);
			return;
		}
		
		Console.WriteLine("========== LogFileUpdated Process ==========");
		Console.WriteLine(e.FullPath);
		
		String line = null;
		do
		{
			line = sr.ReadLine();
			Console.WriteLine(line);
		}while(line != null);
	}
}