using System;
using System.Collections.Generic;
using System.Text;

namespace SSHClient
{
	public class Service
	{
		private readonly Connector _connector;

		public Service()
		{
			_connector = new Connector("Server=127.0.0.1;Port=5431;Database=database name;Uid=username;Password=password;sshHost=ip address;sshPort=2022;sshDbPort=5432;sshUsername=sshUsername;sshPassword=sshPassword");
		}
	}
}
