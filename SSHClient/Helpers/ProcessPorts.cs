using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SSHClient.Helpers
{
	public static class ProcessPorts
	{
		public static List<ProcessPort> ProcessPortMap
		{
			get
			{
				return GetNetStatPorts();
			}
		}

		public static void CleanPort(string port)
		{
			if (Int32.TryParse(port, out Int32 _))
			{
				foreach (ProcessPort p in GetNetStatPorts().Where(p => p.PortNumber == Int32.Parse(port) && p.ProcessId != 0))
				{
					KillProcess(p.ProcessId.ToString());
				}
			}
		}

		private static List<ProcessPort> GetNetStatPorts()
		{
			List<ProcessPort> ProcessPorts = new List<ProcessPort>();

			try
			{
				using (Process Proc = new Process())
				{

					ProcessStartInfo StartInfo = new ProcessStartInfo();
					StartInfo.FileName = "netstat.exe";

					StartInfo.Arguments = " -ano";
					StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					StartInfo.UseShellExecute = false;
					StartInfo.RedirectStandardInput = true;
					StartInfo.RedirectStandardOutput = true;
					StartInfo.RedirectStandardError = true;

					Proc.StartInfo = StartInfo;
					Proc.Start();

					StreamReader StandardOutput = Proc.StandardOutput;
					StreamReader StandardError = Proc.StandardError;

					string NetStatContent = StandardOutput.ReadToEnd() + StandardError.ReadToEnd();
					string NetStatExitStatus = Proc.ExitCode.ToString();

					foreach (string NetStatRow in Regex.Split(NetStatContent, "\r\n"))
					{
						string[] Tokens = Regex.Split(NetStatRow, "\\s+");
						if (Tokens.Length > 4)
						{
							string IpAddress = Regex.Replace(Tokens[3], @"\[(.*?)\]", "1.1.1.1");

							if (((Tokens[1].Equals("UDP") && Int32.TryParse(Tokens[4], out int _))
									|| (Tokens[1].Equals("TCP") && Int32.TryParse(Tokens[5], out int _)))
								&& Int32.TryParse(IpAddress.Split(':')[1], out int _))
							{
								try
								{
									ProcessPorts.Add(new ProcessPort(
										Tokens[1] == "UDP" ? GetProcessName(Convert.ToInt16(Tokens[4])) : GetProcessName(Convert.ToInt16(Tokens[5])),
										Tokens[1] == "UDP" ? Convert.ToInt32(Tokens[4]) : Convert.ToInt32(Tokens[5]),
										IpAddress.Contains("1.1.1.1") ? String.Format("{0}v6", Tokens[1]) : String.Format("{0}v4", Tokens[1]),
										Convert.ToInt32(IpAddress.Split(':')[1])
									));
								}
								catch (Exception)
								{
									throw;
								}
							}
						}
						else
						{
							if (!NetStatRow.Trim().StartsWith("Proto") && !NetStatRow.Trim().StartsWith("Active") && !String.IsNullOrWhiteSpace(NetStatRow))
							{
								throw new Exception("Unrecognized NetStat row to a Process to Port mapping.");
							}
						}
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
			return ProcessPorts;
		}

		public static void KillProcess(string processId)
		{
			try
			{
				using (var Proc = new Process())
				{
					ProcessStartInfo StartInfo = new ProcessStartInfo();
					StartInfo.FileName = "taskkill.exe";

					StartInfo.Arguments = $" /f /pid {processId}";
					StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					StartInfo.UseShellExecute = false;
					StartInfo.RedirectStandardInput = true;
					StartInfo.RedirectStandardOutput = true;
					StartInfo.RedirectStandardError = true;

					Proc.StartInfo = StartInfo;
					Proc.Start();
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		private static string GetProcessName(int ProcessId)
		{
			string procName = "UNKNOWN";

			try
			{
				procName = Process.GetProcessById(ProcessId).ProcessName;
			}
			catch { }

			return procName;
		}
	}
}
