using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace tracertlab
{
    class Program
    {
        static void Main(string[] args)
        {
            short number = 1;
            short id = 1;
            const string teststring = "1234";//строка, которая будет отправляться
            const int mesSize = 1024;                               
            string ipAddr = args[0];
            bool getDomen = false;
            bool isRIgthRecv = false;
       if ((args.Length==2)&&(args[1]=="getDomainName"))
            {
                getDomen = true;
            }
            //Console.WriteLine("enter ip-adress");
           // string ipAddr = Console.ReadLine();
            IPAddress iP;
            IPEndPoint iep;
            bool isIP = IPAddress.TryParse(ipAddr,out iP);
            if (isIP)
            {
                iep = new IPEndPoint(IPAddress.Parse(ipAddr), 0);
            }
            else
            {
                    try
                {
                    IPHostEntry iphe = Dns.GetHostEntry(ipAddr);
                    iep = new IPEndPoint(iphe.AddressList[0], 0);
                }
                catch 
                {
                    Console.WriteLine("error! Please check entered data and try again");
                    return 1;
                    throw;           
                }
            }

            byte[] message = new byte[mesSize];
            message = Encoding.ASCII.GetBytes(teststring);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            EndPoint ep = (EndPoint)iep;
            ICMP packet = new ICMP();
            int ICMPsize;
            int recv;
            ICMPsize = packet.CreateICMPrequest(message, number);//создаем ICMP-пакет для запроса
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);
            Console.WriteLine("Tracing route to {0}",iep.ToString() );
            short ttl = 1;
            int start, end;
            bool epIsReached = false;
            do 
            {
                Console.Write("{0}", ttl);
                for ( int i=0; i<3; i++) 
                {
                    packet.changeIdNumber(number);
                    socket.Ttl = ttl;
                    start = Environment.TickCount;
                    socket.SendTo(packet.getBytes(), ICMPsize, SocketFlags.None, iep);                   
                    try
                    {
                        message = new byte[mesSize];
                        recv = socket.ReceiveFrom(message, ref ep);
                        ICMP reply = new ICMP(recv, message);
                       // Console.Write(" {0} ", reply.getNum().ToString());
                        end = Environment.TickCount;                       
                        if (reply.type == 11)
                        {
                            Console.Write(" {0}ms ", end - start);
                            if (i == 2) {
                                Console.Write(" {0}  ", ep.ToString());

                                if (getDomen)
                                {
                                    IPHostEntry domen;
                                    try
                                    {
                                        IPEndPoint ip = (IPEndPoint)ep;
                                        IPAddress ip1 = ip.Address;
                                        domen = Dns.GetHostEntry(ip1.ToString());
                                        Console.Write("[ {0} ]", domen.HostName);
                                    }
                                    catch
                                    {
                                        Console.Write("-");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (reply.type ==0)
                            {
                                epIsReached = true;
                                Console.Write(" {0}ms ", end - start);
                                if (i == 2) {
                                    Console.Write(" {0} ", ep.ToString());

                                    if (getDomen)
                                    {
                                        IPHostEntry domen;
                                        try
                                        {
                                            IPEndPoint ip = (IPEndPoint)ep;
                                            IPAddress ip1 = ip.Address;
                                            domen = Dns.GetHostEntry(ip1.ToString());
                                            Console.Write("[ {0} ]", domen.HostName);
                                        }
                                        catch
                                        {
                                            Console.Write("-");
                                        }
                                    }

                                }
                                                               
                            }
                        }

                    }
                    catch (SocketException)
                    {
                        Console.Write("* ");
                    }
                    number++;
                }
                Console.WriteLine();
                ttl++;
                id++;
            }while ((ttl<30)&&(!epIsReached));

            if (epIsReached)
            {
                Console.WriteLine("Trace complete");
            }

            socket.Close();

        }
    }
}
