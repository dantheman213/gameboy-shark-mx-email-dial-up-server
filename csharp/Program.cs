using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using static System.Net.Mime.MediaTypeNames;

namespace SharkMXEmail
{
    static class Program
    {
        static void Main(string[] args)
        {
            var port = ListPortsAndSelect();
            Console.WriteLine("Port {0} selected...", port);

            var modem = new Modem(port);
            modem.Start();
        }

        static string ListPortsAndSelect()
        {
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();

            Console.WriteLine("The following serial ports were found:");

            if (ports.Length == 1)
            {
                return ports[0];
            }

            // Display each port name to the console.
            foreach (string port in ports)
            {
                Console.WriteLine(port);
            }

            var comPort = Console.ReadLine();
            return comPort;
        }
    }
}