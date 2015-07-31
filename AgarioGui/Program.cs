using System;
using Gtk;
using System.IO;

using Cairo;
using System.Threading;
using GLib;

using AgarioLib;

namespace AgarioGui
{
	class MainClass
	{


		public static void Main (string[] args)
		{

			Application.Init ();

			client = new AgarioClient ();


			w = new Gtk.Window ("Agario gtk client");
			w.Decorated = false;
			w.Maximize ();
			w.SetSizeRequest (1024, 768);

			darea = new CairoGraphic (client);
			darea.DoubleBuffered = true;
			darea.SetSizeRequest (800, 800);

			Box box = new HBox (false, 0);
			box.Add (darea);
			w.Add (box);
			w.Resize (500, 500);
			w.DeleteEvent += close_window;
			w.ShowAll ();

			w.KeyPressEvent += OnKeyPressEvent;

			//Timer loop for ui display
			new Timer (TimerCallback, null, 0, 20);
			//Timer loop for mouse events
			new Timer (UIControl, null, 0, 100);

			Application.Run ();
		}

		static CairoGraphic darea;
		static Gtk.Window w;
		public static AgarioClient client;

		private static void UIControl (object o)
		{
			Gtk.Application.Invoke (delegate {
				mouseMsg ();
			});

		}

		//Attribute may be unnecessary I copy pasted it
		[GLib.ConnectBefore]
		protected static void OnKeyPressEvent (object o, KeyPressEventArgs evnt)
		{
			//Console.WriteLine (evnt);
			if (evnt.Event.Key == Gdk.Key.s)
				client.Spawn ("Awsome");
			if (evnt.Event.Key == Gdk.Key.space)
				client.Split ();
			if (evnt.Event.Key == Gdk.Key.w)
				client.Eject ();
			if (evnt.Event.Key == Gdk.Key.f)
				client.Spectate ();
		}

		public static void mouseMsg ()
		{
			int x;
			int y;
			darea.GetPointer (out x, out y);

			var w = darea.Allocation.Width;
			var h = darea.Allocation.Height;

			var st = client.state;
			var mx = 0.0;
			var my = 0.0;
			if (st.myBalls.Count > 0) {
				var myb = st.myBalls [0];
				mx = myb.X;
				my = myb.Y;
			} else
				return;
			var scale = 5.0;
			var dx = mx + scale * (x - 0.5 * w);
			var dy = my + scale * (y - 0.5 * h);

			client.MoveTo (dx, dy);
		}

		private static void TimerCallback (object o)
		{
			Gtk.Application.Invoke (delegate {
				darea.QueueDraw ();
			});
		}

		static bool OnTimer ()
		{
			darea.QueueDraw ();
			return true;
		}

		static void close_window (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
		}

	}



	public class CairoGraphic : DrawingArea
	{
		static void OvalPath (Context cr, double xc, double yc, double xr, double yr)
		{
			Matrix m = cr.Matrix;

			cr.Translate (xc, yc);
			cr.Scale (1.0, yr / xr);
			cr.MoveTo (xr, 0.0);
			cr.Arc (0, 0, xr, 0, 2 * Math.PI);
			cr.ClosePath ();

			cr.Matrix = m;
		}

		static void DrawCircle (Context cr, double xc, double yc, double radius, Color color)
		{
			cr.SetSourceRGBA (color.R, color.G, color.B, color.A);
			OvalPath (cr, xc, yc, radius, radius);
			cr.Fill ();
		}

		public CairoGraphic (AgarioClient client)
		{
			Client = client;
		}

		public AgarioClient Client;

		protected override bool OnExposeEvent (Gdk.EventExpose args)
		{
			//Console.WriteLine ("ManagedThreadId {0}",System.Threading.Thread.CurrentThread.ManagedThreadId);
			var midx = Allocation.Width / 2.0;
			var midy = Allocation.Height / 2.0;
			var scale = 0.2;
			using (Context g = Gdk.CairoHelper.Create (args.Window)) {

				g.SetSourceColor (new Color (0, 0, 0));
				g.Rectangle (0.0, 0.0, Allocation.Width, Allocation.Height);

				g.Fill ();


				var st = Client.state;
				var x = 0.0;
				var y = 0.0;
				if (st.myBalls.Count > 0) {
					var myb = st.myBalls [0];
					x = myb.X;
					y = myb.Y;
				} else {
					x = st.x;
					y = st.y;
				}
				foreach (var b in Client.state.Balls) {
					var bal = b.Value;
					DrawCircle (g, midx + scale * (bal.X - x), midy + scale * (bal.Y - y), Math.Max (5.0, scale * bal.size), new Color ((double)bal.R / 255.0, (double)bal.G / 255.0, (double)bal.B / 255.0, 1.0));
				}

			}
			return true;
		}
	}

}
