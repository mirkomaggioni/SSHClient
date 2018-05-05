using System;
using System.ComponentModel;
using Renci.SshNet;
using SSHClient.Helpers;

namespace SSHClient
{
	public class Connector
	{
		private string _server;
		private string _port;
		private string _uid;
		private string _password;
		private string _database;

		private string _sshhost;
		private string _sshport;
		private string _sshdbport;
		private string _sshusername;
		private string _sshpassword;
		private string ConnectionString => $"server={_server};port={_port};uid={_uid};database={_database};pwd={_password};Pooling=false; CommandTimeout=200;Timeout=200;";

		private PasswordConnectionInfo _sshConnectionInfo;
		private PasswordConnectionInfo sshConnectionInfo
		{
			get
			{
				if (_sshConnectionInfo == null)
				{
					_sshConnectionInfo = new PasswordConnectionInfo(_sshhost, int.Parse(_sshport), _sshusername, _sshpassword);
					_sshConnectionInfo.Timeout = TimeSpan.FromSeconds(5000);
				}

				return _sshConnectionInfo;
			}
		}

		private SshClient _client;
		private ForwardedPortLocal _forwardedPortLocal;

		public Connector(string parameters)
		{
			ParametersInitialization(parameters);
		}

		public void Dispose()
		{
			Disconnect();
			GC.SuppressFinalize(false);
		}

		public void Connect()
		{
			try
			{
				if (_client == null)
				{
					_client = new SshClient(sshConnectionInfo);
					_client.KeepAliveInterval = new TimeSpan(0, 0, 20);
				}

				if (_forwardedPortLocal == null)
				{
					ForwardedPortInitialization();
				}

				if (!_client.IsConnected)
				{
					_client.Connect();
					_client.AddForwardedPort(_forwardedPortLocal);
					_forwardedPortLocal.Start();
				}
			}
			catch (Exception ex)
			{
				Disconnect();

				var win32ex = ex as Win32Exception;

				if (win32ex != null && win32ex.ErrorCode == 10048)
				{
					ConnectionsHelper.KillProcessByPortNumber(int.Parse(_sshport));
					Connect();
				}
				else
				{
					throw;
				}
			}
		}

		~Connector()
		{
			Dispose();
		}

		private void Disconnect()
		{
			if (_client != null)
			{
				if (_forwardedPortLocal.IsStarted)
				{
					_forwardedPortLocal.Stop();
				}

				if (_client.IsConnected)
				{
					_client.Disconnect();
				}

				_forwardedPortLocal.Dispose();
				_client.Dispose();

				_forwardedPortLocal = null;
				_client = null;
			}
		}

		private void ParametersInitialization(string parameters)
		{
			if (string.IsNullOrEmpty(parameters))
				throw new ArgumentNullException("Missing parameters");
			var list = parameters.Split(';');
			foreach (var parameter in list)
			{
				var values = parameter.Split('=');
				if (values.Length != 2) continue;
				switch (values[0].ToLower())
				{
					case "server":
						_server = values[1];
						break;
					case "port":
						_port = values[1];
						break;
					case "uid":
						_uid = values[1];
						break;
					case "password":
						_password = values[1];
						break;
					case "database":
						_database = values[1];
						break;
					case "sshhost":
						_sshhost = values[1];
						break;
					case "sshport":
						_sshport = values[1];
						break;
					case "sshdbport":
						_sshdbport = values[1];
						break;
					case "sshusername":
						_sshusername = values[1];
						break;
					case "sshpassword":
						_sshpassword = values[1];
						break;
				}
			}
			if (string.IsNullOrEmpty(_server))
				throw new ArgumentNullException("Missing 'server' parameter.");
			if (string.IsNullOrEmpty(_port))
				throw new ArgumentNullException("Missing 'port' parameter.");
			if (string.IsNullOrEmpty(_uid))
				throw new ArgumentNullException("Missing 'uid' parameter.");
			if (string.IsNullOrEmpty(_password))
				throw new ArgumentNullException("Missing 'password' parameter.");
			if (string.IsNullOrEmpty(_database))
				throw new ArgumentNullException("Missing 'database' parameter.");
			if (string.IsNullOrEmpty(_sshhost))
				throw new ArgumentNullException("Missing 'sshhost' parameter.");
			if (string.IsNullOrEmpty(_sshdbport))
				throw new ArgumentNullException("Missing 'sshdbport' parameter.");
			if (string.IsNullOrEmpty(_sshport))
				throw new ArgumentNullException("Missing 'sshport' parameter.");
			if (string.IsNullOrEmpty(_sshusername))
				throw new ArgumentNullException("Missing 'sshusername' parameter.");
			if (string.IsNullOrEmpty(_sshpassword))
				throw new ArgumentNullException("Missing 'sshpassword' parameter.");

			ForwardedPortInitialization();
		}

		private void ForwardedPortInitialization()
		{
			_forwardedPortLocal = new ForwardedPortLocal(_server, uint.Parse(_port), _server, uint.Parse(_sshdbport));
		}
	}
}
