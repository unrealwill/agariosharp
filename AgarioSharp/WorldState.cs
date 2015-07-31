using System;

using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

//As time of writing the WebSocketSharp nuget package has issues with ws:// on port 443 which agario sometimes use
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

    public class WorldState
    {
        public WorldState()
        {
            Balls = new Dictionary<uint,Ball>();
            myBalls = new List<Ball>();
        }
        //I didn't understand what tick_counter is really used for
        //In https://github.com/pulviscriptor/agario-client it is used to remove detect and remove 'inactive' balls
        //Yet I is seems the server sends a disapear message for inactive balls because I haven't implemented inactive ball removal and it seems ok
        public int tick_counter;

        public Dictionary<uint,Ball> Balls;
        public List<Ball> myBalls;

        public double minx;
        public double miny;
        public double maxx;
        public double maxy;

        public double x;
        public double y;
        public double zoom;

        public void ProcessWorldSize(byte[] buffer)
        {
            int offset = 1;

            minx = Packet.ReadDoubleLE(buffer, ref offset);
            miny = Packet.ReadDoubleLE(buffer, ref offset);
            maxx = Packet.ReadDoubleLE(buffer, ref offset);
            maxy = Packet.ReadDoubleLE(buffer, ref offset);
            Console.WriteLine("world size : {0} {1} {2} {3}", minx, miny, maxx, maxy);
        }

        public void ProcessTick(byte[] buffer)
        {
            int offset = 1;
            var eaters_count = Packet.ReadUInt16LE(buffer, ref offset);

            tick_counter++;

            //reading eat events
            for (var i = 0; i < eaters_count; i++)
            {
                var eater_id = Packet.ReadUInt32LE(buffer, ref offset);
                var eaten_id = Packet.ReadUInt32LE(buffer, ref offset);

                //if(client.debug >= 4)
                //	client.log(eater_id + ' ate ' + eaten_id + ' (' + client.balls[eater_id] + '>' + client.balls[eaten_id] + ')');

                if (Balls.ContainsKey(eater_id) == false)
                    Balls.Add(eater_id, new Ball());
                if (Balls.ContainsKey(eaten_id) == false)
                    Balls.Remove(eaten_id);
                //Console.WriteLine ("{0} ate {1}", eater_id, eaten_id);

                //client.balls[eater_id].update();
                //if(client.balls[eaten_id]) client.balls[eaten_id].destroy({'reason':'eaten', 'by':eater_id});

                //client.emit('somebodyAteSomething', eater_id, eaten_id);
            }

            //reading actions of balls
            while (true)
            {
                var is_virus = false;
                UInt32 ball_id;
                Int32 coordinate_x;
                Int32 coordinate_y;
                Int16 size;
                string nick = null;

                ball_id = Packet.ReadUInt32LE(buffer, ref offset);
                if (ball_id == 0)
                    break;
                coordinate_x = Packet.ReadSInt32LE(buffer, ref offset);
                coordinate_y = Packet.ReadSInt32LE(buffer, ref offset);
                size = Packet.ReadSInt16LE(buffer, ref offset);

                var color_R = Packet.ReadUInt8(buffer, ref offset);
                var color_G = Packet.ReadUInt8(buffer, ref offset);
                var color_B = Packet.ReadUInt8(buffer, ref offset);

                //color = (color_R << 16 | color_G << 8 | color_B).toString(16);
                //color = '#' + ('000000' + color).substr(-6);

                var opt = Packet.ReadUInt8(buffer, ref offset);

                is_virus = (opt & 1) != 0;

                //reserved for future use?
                if ((opt & 2) != 0)
                {
                    offset += 4;
                }
                if ((opt & 4) != 0)
                {
                    offset += 8;
                }
                if ((opt & 8) != 0)
                {
                    offset += 16;
                }

                nick = "";
                while (true)
                {
                    var ch = Packet.ReadUInt16LE(buffer, ref offset);
                    if (ch == 0)
                        break;
                    nick += (Char)ch;
                }

                if (Balls.ContainsKey(ball_id) == false)
                    Balls.Add(ball_id, new Ball());

                var ball = Balls[ball_id];
                ball.R = color_R;
                ball.G = color_G;
                ball.B = color_B;
                ball.isVirus = is_virus;
                ball.SetCoords(coordinate_x, coordinate_y);
                ball.size = size;
                if (nick != "")
                    ball.Nick = nick;

                if (nick != "")
                {
                    //	Console.WriteLine (nick);
                }
                //ball.update_tick = tick_counter;
                //ball.appear();
                //ball.update();
                //if( Balls[ball_id].Nick!="")
                //	Console.WriteLine("action: ball_id= {0} coordinate_x= {1} coordinate_y= {2}' size= {3} is_virus={4} nick={5}",ball_id,coordinate_x,coordinate_y,size,is_virus,Balls[ball_id].Nick);

                //if(client.debug >= 4)
                //	client.log('action: ball_id=' + ball_id + ' coordinate_x=' + coordinate_x + ' coordinate_y=' + coordinate_y + ' size=' + size + ' is_virus=' + is_virus + ' nick=' + nick);

                //client.emit('ballAction', ball_id, coordinate_x, coordinate_y, size, is_virus, nick);

            }

            var balls_on_screen_count = Packet.ReadUInt32LE(buffer, ref offset);
            if (balls_on_screen_count == 0)
                return;
            //Console.WriteLine ("balls_on_screen_count {0}", balls_on_screen_count);
            //disappear events
            //Console.WriteLine(Balls.Count());
            for (int i = 0; i < balls_on_screen_count; i++)
            {
                var ball_id = Packet.ReadUInt32LE(buffer, ref offset);
                //Console.WriteLine ("ballid to remove {0}", ball_id);
                //Console.WriteLine (Balls.ContainsKey (ball_id));
                var b = Balls.ContainsKey(ball_id) ? Balls[ball_id] : new Ball();
                if (b.Mine)
                    myBalls.Remove(b);

                Balls.Remove(ball_id);

                /*
				ball = client.balls[ball_id] || new Ball(client, ball_id);
				ball.update_tick = client.tick_counter;
				ball.update();
				if(ball.mine) {
					ball.destroy({reason: 'merge'});
					client.emit('merge', ball.id);
				}else{
					ball.disappear();
				}*/

            }
            //Console.WriteLine(Balls.Count());
        }

        public void ProcessSpectate(byte[] buffer)
        {
            int offset = 1;
            x = Packet.ReadFloatLE(buffer, ref offset);
            y = Packet.ReadFloatLE(buffer, ref offset);
            zoom = Packet.ReadFloatLE(buffer, ref offset);
        }

        public void ProcessNewId(byte[] buffer)
        {
            int offset = 1;
            uint myBallId = Packet.ReadUInt32LE(buffer, ref offset);
            var b = new Ball();
            b.Mine = true;
            Balls.Add(myBallId, b);
            myBalls.Add(b);
        }

        public void ProcessMessage(byte[] buffer)
        {
            //Console.WriteLine ("ManagedThreadId {0}",System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (buffer.Length == 0)
            {
                Console.WriteLine("buffer of length 0");
                return;
            }
            switch (buffer[0])
            {
                case 16:
                    {
                        //Console.WriteLine ("ProcessTick");
                        ProcessTick(buffer);
                        break;
                    }
                case 17:
                    {
                        //Console.WriteLine ("Spectate");
                        ProcessSpectate(buffer);
                        break;
                    }
                case 20:
                    {
                        //Not done see https://github.com/pulviscriptor/agario-client
                        break;
                    }
                case 32:
                    {
                        //Console.WriteLine ("NewId");
                        ProcessNewId(buffer);
                        break;
                    }
                case 49:
                    {
                        //leaderboard update in FFA mode
                        //Console.WriteLine ("Leader board {0}", buffer.Length);
                        //TODO:implement see https://github.com/pulviscriptor/agario-client
                        break;
                    }

                case 50:
                    {
                        //teams scored update in teams mode
                        //TODO:implement see https://github.com/pulviscriptor/agario-client
                        break;
                    }
                case 64:
                    {
                        ProcessWorldSize(buffer);
                        break;
                    }
                case 72:
                    {
                        //packet is sent by server but not used in original code
                        break;
                    }
                case 81:
                    {
                        //client.emit('experienceUpdate', level, curernt_exp, need_exp);
                        //I don't know what this should do
                        break;
                    }
                case 240:
                    {

                        break;
                    }
                case 254:
                    {
                        //somebody won, end of the game (server restart)
                        break;
                    }

                default:
                    {
                        Console.WriteLine("Unknown packet id {0}", buffer[0]);
                        break;
                    }
            }
        }

    }
		
}
