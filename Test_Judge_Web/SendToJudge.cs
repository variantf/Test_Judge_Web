using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace Test_Judge_Web
{
    public class SendToJudge
    {
       string Source, FilePath, Language;
        string[] InFile, OutFile;      //各个测试文件名称
        int TestNum;                   //测试点数量
        int[] TimeLimit,ScoreList;     //时间限制、各个测试点得分
        HttpResponse Response;
        protected void ReadFileText(string path)
        {
            string mbPath = path;
            Encoding code = Encoding.GetEncoding("GB2312");
            StreamReader sr = null;
            string str = null;
            //读取 
            try
            {
                sr = new StreamReader(mbPath, code);
                str = sr.ReadLine();
                TestNum = Convert.ToInt32(str);
                InFile = new string[TestNum];
                OutFile = new string[TestNum];
                TimeLimit = new int[TestNum];
                ScoreList = new int[TestNum];
                for (int i = 0; i < TestNum; ++i)
                {
                    str = sr.ReadLine();
                    string[] s = str.Split('|');
                    InFile[i] = s[0];
                    OutFile[i] = s[1];
                    TimeLimit[i] = Convert.ToInt32(s[2]);
                    ScoreList[i] = Convert.ToInt32(s[3]);
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {
                sr.Close();
            }
        }
        public SendToJudge(string _Source, string _FilePath,string _Language, HttpResponse _Response)
        {
            Source = _Source;
            FilePath = _FilePath;
            ReadFileText(FilePath+"\\Config.ini");
            Response = _Response;
            Language = _Language;
        }
        public void Judge()
        {
            try
            {
                //string CodePath = "E:\\tmp\\tmp"+Language;
                //string BinaryPath = "E:\\tmp\\tmp.exe";
                //File.WriteAllText(CodePath, Source);

                using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    bool con = false;
                    while(!con)
                        try
                        {
                            sock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6000));
                            con = true;
                        }
                        catch
                        {
                            con = false;
                        }

                    string Compile = "";
                    if (Language == ".cpp")
                        Compile = "g++ -o {Execute}{.cpp} {Source}";
                    if (Language==".c")
                        Compile = "gcc -o {Execute}{.c} {Source}";
                    if (Language==".pas")
                        Compile = "fpc -o{Execute}{.pas} {Source}";

                    sock.Send(new Message()
                    {
                        Type = Message.MessageType.Compile,
                        Content = new CompileIn()
                        {
                            Code = Source,
                            Command = Compile,
                            Memory = 1024 * 1024 * 60,
                            Time = 5 * 1000
                        }
                    }.ToBytes());

                    Out ret = new Out(sock);

                    Response.Write("<div class=\"style2\" style=\"text-align:left; padding-top:20px; padding-left:20px;\">编译信息：</div>");
                    Response.Write("<div style=\"color:green; text-align:left; padding-left:30px;\"" + ret.CompileMessage() + "</div>");

                    if (ret.ErrorCode == 0)
                    {
                        Response.Write("<div class=\"style2\" style=\"text-align:left; padding-top:20px; padding-left:20px;\">测试点信息：</div>");
                        for (int i = 0; i < TestNum; ++i)
                        {
                            //Response.Write("??:"+FilePath+"\\input\\"+InFile[i]+" "+FilePath+"\\output\\"+OutFile[i]);
                            sock.Send(new Message()
                            {
                                Type = Message.MessageType.Test,
                                Content = new TestIn()
                                {
                                    CmpPath = "E:\\tmp\\Special_Judge.exe",
                                    ExecPath = ret.Message,
                                    Memory = 1024 * 1024 * 30,
                                    Time = 1000,
                                    Input = File.ReadAllBytes(FilePath + "\\input\\" + InFile[i]),
                                    Output = File.ReadAllBytes(FilePath + "\\output\\" + OutFile[i])
                                }
                            }.ToBytes());

                            ret = new Out(sock);
                            Response.Write("<div style=\"color:green; text-align:left; padding-left:30px;\">");
                            Response.Write("#"+(i+1).ToString()+": "+ret.RunMessage()+"</div>");
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                
            }
        }
        public class Out
        {
            public uint ErrorCode { get; set; }
            public long Time { get; set; }
            public long Memory { get; set; }
            public string Message { get; set; }

            public Out(Socket sock)
            {
                byte[] buf = new byte[sizeof(uint) + sizeof(uint) + sizeof(long) + sizeof(long)];
                int read = 0;
                while (read < buf.Length)
                {
                    int rev = sock.Receive(buf, read, buf.Length - read, SocketFlags.None);
                    if (rev == 0) break;
                    read += rev;
                }

                uint messageLength;
                using (MemoryStream stream = new MemoryStream(buf))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        messageLength = reader.ReadUInt32();
                        ErrorCode = reader.ReadUInt32();
                        Time = reader.ReadInt64();
                        Memory = reader.ReadInt64();
                    }
                }

                buf = new byte[messageLength];
                read = 0;
                while (read < buf.Length)
                {
                    int rev = sock.Receive(buf, read, buf.Length - read, SocketFlags.None);
                    if (rev == 0) break;
                    read += rev;
                }
                Message = Encoding.Default.GetString(buf);
            }

            public string CompileMessage()
            {
				string s="<br/>";
                if (ErrorCode == 0) s = s + "<span style=\"color:red\">Compile Successful</span>";
                else s = s + "<span style=\"color:blue\">Compile Error</span>";
				return s + "<br/>Compile Time = " + Time + " ms<br/>Compile Memory = " + Memory/1024 + " KB<br/>Compile Messsage = " + Message + "<br/>";
			}

			public string RunMessage()
            {
				string s;
                if (ErrorCode == 0) s = "<span style=\"color:red\">Accepted</span>";
                else if (ErrorCode == 1) s = "<span style=\"color:blue\">Wrong Answer</span>";
                else if (ErrorCode == 2) s = "<span style=\"color:blue\">Time Limit Exceeded</span>";
                else if (ErrorCode == 3) s = "<span style=\"color:blue\">Runtime Error</span>";
                else if (ErrorCode == 4) s = "<span style=\"color:blue\">Memory Limit Exceeded</span>";
                else s = "<span style=\"color:blue\">System Error</span>";
                return s + " Time = " + Time + " ms , Memory = " + Memory / 1024 + " KB";
            }
        }

        public class Message
        {
            public enum MessageType : uint
            {
                Compile = 1, Test = 2
            }

            public MessageType Type { get; set; }
            public IMessageContent Content { get; set; }

            public byte[] ToBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write((uint)Type);
                        byte[] contentBytes = Content.ToBytes();
                        writer.Write((uint)contentBytes.LongLength);
                        writer.Write(contentBytes);
                    }
                    return stream.ToArray();
                }
            }
        }

        public interface IMessageContent
        {
            byte[] ToBytes();
        }

        public class CompileIn : IMessageContent
        {
            public long Time { get; set; }
            public long Memory { get; set; }
            public string Code { get; set; }
            public string Command { get; set; }

            public byte[] ToBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(Time);
                        writer.Write(Memory);
                        byte[] code = Encoding.UTF8.GetBytes(Code);
                        writer.Write((uint)code.Length);
                        writer.Write(code);
                        writer.Write(Encoding.UTF8.GetBytes(Command));
                        writer.Write((byte)0);
                    }
                    return stream.ToArray();
                }
            }
        }

        public class TestIn : IMessageContent
        {
            public long Time { get; set; }
            public long Memory { get; set; }
            public string ExecPath { get; set; }
            public string CmpPath { get; set; }
            public byte[] Input { get; set; }
            public byte[] Output { get; set; }

            public byte[] ToBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        byte[] execPath = Encoding.UTF8.GetBytes(ExecPath);
                        byte[] cmpPath = Encoding.UTF8.GetBytes(CmpPath);
                        writer.Write((uint)0);
                        writer.Write((uint)(execPath.Length + 1));
                        writer.Write((uint)(execPath.Length + 1 + Input.Length));
                        writer.Write((uint)(execPath.Length + 1 + Input.Length + Output.Length));
                        writer.Write(Time);
                        writer.Write(Memory);
                        writer.Write(execPath);
                        writer.Write((byte)0);
                        writer.Write(Input);
                        writer.Write(Output);
                        writer.Write(cmpPath);
                        writer.Write((byte)0);
                    }
                    return stream.ToArray();
                }
            }
        }
    }
    }
}