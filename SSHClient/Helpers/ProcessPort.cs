using System;

namespace SSHClient.Helpers
{
	public class ProcessPort
	{
		private string _ProcessName = String.Empty;
		private int _ProcessId = 0;
		private string _Protocol = String.Empty;
		private int _PortNumber = 0;

		internal ProcessPort(string ProcessName, int ProcessId, string Protocol, int PortNumber)
		{
			_ProcessName = ProcessName;
			_ProcessId = ProcessId;
			_Protocol = Protocol;
			_PortNumber = PortNumber;
		}

		public string ProcessPortDescription
		{
			get
			{
				return String.Format("{0} ({1} port {2} pid {3})", _ProcessName, _Protocol, _PortNumber, _ProcessId);
			}
		}
		public string ProcessName
		{
			get { return _ProcessName; }
		}
		public int ProcessId
		{
			get { return _ProcessId; }
		}
		public string Protocol
		{
			get { return _Protocol; }
		}
		public int PortNumber
		{
			get { return _PortNumber; }
		}
	}
}
