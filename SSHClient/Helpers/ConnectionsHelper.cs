using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SSHClient.Helpers
{
	public static class ConnectionsHelper
	{
		public static void KillProcessByPortNumber(int port)
		{
			DropConnection(port);
		}

		private static void DropConnection(int port)
		{
			try
			{
				using (var process = new Process())
				{
					var processStartInfo = new ProcessStartInfo();
					processStartInfo.FileName = "netstat.exe";
					processStartInfo.Arguments = " -ano";
					processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					processStartInfo.UseShellExecute = false;
					processStartInfo.RedirectStandardInput = true;
					processStartInfo.RedirectStandardOutput = true;
					processStartInfo.RedirectStandardError = true;

					process.StartInfo = processStartInfo;
					process.Start();

					var netStatContent = process.StandardOutput.ReadToEnd();

					foreach (string netStatRow in Regex.Split(netStatContent, "\r\n"))
					{
						var tokens = Regex.Split(netStatRow, "\\s+");
						if (tokens.Length > 4
							&& tokens[1].Equals("TCP")
							&& Int32.TryParse(tokens[5], out int _)
							&& Int32.TryParse(tokens[3].Split(':')[1], out int _)
							&& Convert.ToInt32(tokens[3].Split(':')[1]) == port)
						{
							try
							{
								KillProcessById(tokens[5]);
							}
							catch (Exception)
							{
								throw;
							}
						}
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		private static void KillProcessById(string processId)
		{
			try
			{
				using (var process = new Process())
				{
					ProcessStartInfo StartInfo = new ProcessStartInfo();
					StartInfo.FileName = "taskkill.exe";

					StartInfo.Arguments = $" /f /pid {processId}";
					StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					StartInfo.UseShellExecute = false;
					StartInfo.RedirectStandardInput = true;
					StartInfo.RedirectStandardOutput = true;
					StartInfo.RedirectStandardError = true;

					process.StartInfo = StartInfo;
					process.Start();
				}
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
