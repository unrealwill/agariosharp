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

	//We use the following class for the deserialization
	//Ideally we would have just use a standard like protocol-buffer and thrift
	//And just reverse-engineered the correct structure
	//But, either for performance or for an other reason,it seems the serialization format is a custom-one sometimes array are length prefixed, and sometimes they are 0-terminated
	//Maybe with more work it can be possible to find the correct serialization
	static class Packet
	{
		public static Int16 ReadSInt16LE(byte[] buffer, ref int index)
		{
			if (index > buffer.Length - 2)
				return 0;
			var b = new byte[2];
			Array.Copy(buffer, index, b, 0, 2);
			if (BitConverter.IsLittleEndian == false)
				Array.Reverse(b);
			index += 2;
			return BitConverter.ToInt16(b, 0);

		}

		public static Int32 ReadSInt32LE(byte[] buffer, ref int index)
		{
			if (index > buffer.Length - 4)
				return 0;
			var b = new byte[4];
			Array.Copy(buffer, index, b, 0, 4);
			if (BitConverter.IsLittleEndian == false)
				Array.Reverse(b);
			index += 4;
			return BitConverter.ToInt32(b, 0);
		}

		public static byte ReadUInt8(byte[] buffer, ref int index)
		{
			if (index > buffer.Length - 1)
				return 0;
			var b = buffer[index];
			index++;
			return b;
		}

		public static UInt16 ReadUInt16LE(byte[] buffer, ref int index)
		{
			if (index > buffer.Length - 2)
				return 0;
			var b = new byte[2];
			Array.Copy(buffer, index, b, 0, 2);

			if (BitConverter.IsLittleEndian == false)
				Array.Reverse(b);
			index += 2;
			return BitConverter.ToUInt16(b, 0);

		}

		public static UInt32 ReadUInt32LE(byte[] buffer, ref int index)
		{
			if (index > buffer.Length - 4)
				return 0;
			var b = new byte[4];
			Array.Copy(buffer, index, b, 0, 4);
			if (BitConverter.IsLittleEndian == false)
				Array.Reverse(b);
			index += 4;
			return BitConverter.ToUInt32(b, 0);
		}

		public static float ReadFloatLE(byte[] buffer, ref int index)
		{
			if (index > buffer.Length - 4)
				return 0.0f;
			var b = new byte[4];
			Array.Copy(buffer, index, b, 0, 4);
			if (BitConverter.IsLittleEndian == false)
				Array.Reverse(b);
			index += 4;
			return BitConverter.ToSingle(b, 0);
		}

		public static double ReadDoubleLE(byte[] buffer, ref int index)
		{
			if (index > buffer.Length - 8)
				return 0.0;
			var b = new byte[8];
			Array.Copy(buffer, index, b, 0, 8);
			if (BitConverter.IsLittleEndian == false)
				Array.Reverse(b);
			index += 8;
			return BitConverter.ToDouble(b, 0);
		}

	}
		
}
