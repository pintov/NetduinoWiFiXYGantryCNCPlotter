using System;
using Maple;
using System.Collections;
using System.IO;
using Json.NETMF;
using System.Net;

namespace NetduinoWiFiXYGantryCNCPlotter
{
    public class RequestHandler : RequestHandlerBase
    {
        public void getStatus()
        {
            Context.Response.ContentType = "application/json";
            Context.Response.StatusCode = 200;
            Hashtable result = new Hashtable { { "alive", DateTime.Now.ToString() } };
            Send(result);
        }

        public void postSettings()
        {
            try
            {
                string json = ReadInputString(Context.Request);
                var settings = (Hashtable)JsonSerializer.DeserializeString(json);
                foreach(DictionaryEntry setting in settings)
                {
                    if ((string)setting.Key == "OneStepZ_Angle_Positive")
                        Program.settings.OneStepZ_Angle_Positive = (int)(long)setting.Value;
                    if ((string)setting.Key == "OneStepZ_Angle_Negative")
                        Program.settings.OneStepZ_Angle_Negative = (int)(long)setting.Value;
                }

                
                Program.parser.ParseLines("G0 Z-1");
                Program.parser.ParseLines("G0 Z1");
                SendOK();
            }
            catch (Exception e)
            {
                SendError(e);
            }
        }

        public void postPrint()
        {
            try
            {
                byte[] line = new byte[500];
                var total = 0;
                int i = 0;
                while (true)
                {
                    line[i] = (byte)Context.Request.InputStream.ReadByte();

                    if (i > 0)
                        if (line[i - 1] == 13 && line[i] == 10)
                        {
                            var gcode = BytesToString(line);
                            Program.parser.ParseLine(gcode);
                            line = new byte[500];
                            i = -1;
                        }

                    i++;
                    total++;

                    if (total == Context.Request.ContentLength64)
                    {
                        var gcode = BytesToString(line);
                        Program.parser.ParseLines(gcode);
                        break;
                    }
                }


                
                SendOK();
            }
            catch (Exception e)
            {
                SendError(e);
            }
        }

        private string ReadInputString(HttpListenerRequest Request)
        {
            var bytes = ReadBytes(Request);
            var res = BytesToString(bytes);
            return res;
        }

        private void SendOK()
        {
            Context.Response.ContentType = "application/json";
            Context.Response.StatusCode = 200;
            Hashtable result = new Hashtable { { "result", "OK" } };
            Send(result);
        }

        private void SendError(Exception e)
        {
            Context.Response.ContentType = "application/json";
            Context.Response.StatusCode = 500;
            Hashtable result = new Hashtable
                    {
                        { "result", "Error" },
                        { "errorText", e.Message + " " + e.StackTrace }
                    };
            Send(result);
        }


        private byte[] ReadBytes(HttpListenerRequest Request)
        {
            byte[] bytes = new byte[Request.ContentLength64];
            
            var i = 0;
            while (true)
            {
                bytes[i] = (byte)Request.InputStream.ReadByte();
                i++;
                if (i == Request.ContentLength64)
                    break;
            }
            return bytes;
        }

        private string BytesToString(byte[] bytes)
        {
            string res = new string(System.Text.Encoding.UTF8.GetChars(bytes, 0, bytes.Length));
            return res;
        }

    }
}
