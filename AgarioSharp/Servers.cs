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

    public static class Servers
    {
        public const string initKey = "154669603";

        public static ServerCredentials MakePostRequest(string postData)
        {
            //Maybe we could use webclient but I didn't succeed making a correctly formated application/x-www-form-urlencoded post request
            WebRequest request = WebRequest.Create("http://m.agar.io/");
            request.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string result = reader.ReadToEnd();
            Console.WriteLine(result);
            var lines = result.Split('\n');
            var output = new ServerCredentials{ Server = lines[0], Key = lines[1] };
            reader.Close();
            dataStream.Close();
            response.Close();
            return output;
        }

        public static ServerCredentials GetFFAServer(string region = "EU-London")
        {
            return MakePostRequest(region + "\n" + initKey);
        }

        public static ServerCredentials GetExperimentalServer(string region = "EU-London")
        {
            return MakePostRequest(region + ":experimental\n" + initKey);
        }

        public static ServerCredentials GetTeamsServer(string region = "EU-London")
        {
            return MakePostRequest(region + ":teams\n" + initKey);
        }
    }

    //We use the following class for the deserialization
    //Ideally we would have just use a standard like protocol-buffer and thrift
    //And just reverse-engineered the correct structure
    //But, either for performance or for an other reason,it seems the serialization format is a custom-one sometimes array are length prefixed, and sometimes they are 0-terminated
    //Maybe with more work it can be possible to find the correct serialization
		
}
