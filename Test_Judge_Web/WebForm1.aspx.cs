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
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write(e.ToString());
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                string source = codeBox.Text;


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
                    using (MemoryStream stream = new MemoryStream())
                    {
                        using (BinaryWriter Writer = new BinaryWriter(stream))
                        {
                            //Password Here!!
                            Writer.Write(34659308463532339);
                        }
                        sock.Send(stream.ToArray());
                    }
                    sock.Send(new Message()
                    {
                        Type = Message.MessageType.Compile,
                        Content = new CompileIn()
                        {
                            Code = source,
                            Command = "g++ {Source}{S.cppS} -o {Execute}{E.exeE}",
                            Memory = 1024 * 1024 * 60,
                            Time = 5 * 1000
                        }
                    }.ToBytes());
                    
                    Out ret = new Out(sock);
                    
                    Log.Text =(ret.ToString()+"\r\n");
                    if (ret.ErrorCode == 0)
                    {
                        for (int i = 1; i <= 9; i++)
                        {
                            sock.Send(new Message()
                            {
                                Type = Message.MessageType.Test,
                                Content = new TestIn()
                                {
                                    CmpPath = @"\\vmware-host\Shared Folders\VariantF\Documents\Programs & Websites\Visual_Studio\My Projects\Online_Judge\Judge\x64\Release\Special_Judge.exe",
                                    ExecPath =  ret.Message,
                                    Memory = 1024 * 1024 * 128,
                                    Time = 1000,
                                    Input = Encoding.Default.GetBytes("1 2"),
                                    Output = Encoding.Default.GetBytes("3")
                                }
                            }.ToBytes());

                            Log.Text += (new Out(sock).ToString() + "\r\n");
                        }
                        File.Delete(ret.Message);
                    }
                     
                }
            }
            catch (Exception exp)
            {
                Response.Write(exp.Message);
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

            public override string ToString()
            {
                return "[Out ErrorCode=" + ErrorCode + " Time=" + Time + " Memory=" + Memory + " Messsage=" + Message + "]";
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