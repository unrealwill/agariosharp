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

    public class Ball
    {
        public bool isVirus;
        public byte R;
        public byte G;
        public byte B;
        public int X;
        public int Y;
        public short size;
        public string Nick;

        public void SetCoords(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Mine;
    }

    //We use the following class for the deserialization
    //Ideally we would have just use a standard like protocol-buffer and thrift
    //And just reverse-engineered the correct structure
    //But, either for performance or for an other reason,it seems the serialization format is a custom-one sometimes array are length prefixed, and sometimes they are 0-terminated
    //Maybe with more work it can be possible to find the correct serialization
		
}
