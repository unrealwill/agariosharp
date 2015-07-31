using System;

using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

//At time of writing the WebSocketSharp nuget package has issues with ws:// on port 443 which agar.io sometimes use
//It is corrected on their github master branch so you should pull this branch build from source and reference the built library
using WebSocketSharp;
using System.Linq;
using Gtk;
using System.IO;

using Cairo;
using System.Threading;
using System.Net;
using System.Text;

namespace AgarioLib
{
	public class AgarioClient
	{
		public AgarioClient()
		{
			Console.WriteLine(BitConverter.IsLittleEndian);
			state = new WorldState();
			credentials = Servers.GetFFAServer();

			Console.WriteLine("Server {0}", credentials.Server);
			Console.WriteLine("Key {0}", credentials.Key);

			var uri = "ws://" + credentials.Server;
			Console.WriteLine(uri);

			ws = new WebSocket(uri);

			ws.Origin = "http://agar.io";
			ws.OnOpen += OnOpen;
			ws.OnError += OnError;
			ws.OnMessage += OnMessageReceived;
			ws.OnClose += OnClose;
			ws.Connect();

		}

		public void Spawn(string name)
		{
			var buf = new byte[1 + 2 * name.Length];
			buf[0] = 0;

			for (var i = 0; i < name.Length; i++)
			{
				buf[2 * i + 1] = (byte)(name[i]);
				buf[2 * i + 2] = 0;
			}
			ws.Send(buf);
		}

		public void MoveTo(double x, double y)
		{
			var buf = new byte[21];
			buf[0] = 16;
			var b = BitConverter.GetBytes(x);
			Array.Copy(b, 0, buf, 1, 8);
			b = BitConverter.GetBytes(y);
			Array.Copy(b, 0, buf, 9, 8);
			b = BitConverter.GetBytes((uint)0);
			Array.Copy(b, 0, buf, 17, 4);
			ws.Send(buf);
		}

		public void Spectate()
		{
			var buf = new byte[1 ];
			buf[0] = 1;

			ws.Send(buf);
		}

		public  void Split()
		{
			var buf = new byte[1 ];
			buf[0] = 17;

			ws.Send(buf);
		}

		public  void Eject()
		{
			var buf = new byte[1 ];
			buf[0] = 21;

			ws.Send(buf);
		}

		public void OnClose(object sender, EventArgs e)
		{
			Console.WriteLine("OnClose");
		}

		public void OnError(object sender, EventArgs e)
		{
			Console.WriteLine("OnError");
			//var err = (SuperSocket.ClientEngine.ErrorEventArgs)e;

			//Console.WriteLine(err.Exception.Message);
		}

		public void OnOpen(object sender, EventArgs e)
		{
			Console.WriteLine("OnOpen");
			var buf = new byte[5];
			buf[0] = 254;
			//buf.writeUInt8(254, 0);
			buf[1] = 5;
			ws.Send(buf);

			buf = new byte[5];
			buf[0] = 255;
			UInt32 v = 154669603;
			var vBytes = BitConverter.GetBytes(v);
			if (BitConverter.IsLittleEndian == false)
				Array.Reverse(vBytes);

			for (var i = 0; i < 4; i++)
				buf[i + 1] = vBytes[i];

			ws.Send(buf);

			buf = new byte[1 + credentials.Key.Length];
			buf[0] = 80;

			for (var i = 1; i <= credentials.Key.Length; ++i)
			{
				buf[i] = (byte)(credentials.Key[i - 1]);
			}

			ws.Send(buf);
		}

		public void OnMessageReceived(object sender, EventArgs e)
		{
			//Console.WriteLine ("ManagedThreadId {0}",System.Threading.Thread.CurrentThread.ManagedThreadId);
			var evt = (WebSocketSharp.MessageEventArgs)e;

			//We push the message to the ui thread to process all message in a single thread to avoid multithreading issues
			Gtk.Application.Invoke(delegate
				{
					state.ProcessMessage(evt.RawData);
				});

		}

		WebSocket ws = null;
		ServerCredentials credentials = null;
		public WorldState state = null;


	}
}

