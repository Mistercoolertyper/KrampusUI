using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using WebSocketSharp;

class KrampusWebSocket
{
	private WebSocket ws;

	public KrampusWebSocket(string loginToken)
	{
		ws = new WebSocket($"wss://loader.live/?login_token=\"{loginToken}\"");
		ws.Connect();
		Task.Run(StartWebsocketLoop);
	}

	private void StartWebsocketLoop()
	{
		while (true)
		{
			if (!ws.IsAlive)
			{
				ws.Connect();
				if (ws.IsAlive)
				{
					MessageBox.Show("Reconnected to WebSocket", "Krampus", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			else
			{
				ws.Send(JsonConvert.SerializeObject(new
				{
					type = 2
				}));
			}
			Thread.Sleep(1000);
		}
	}

	public void Execute(string script)
	{
		ws.Send($"<SCRIPT>{script}");
	}
}
