using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;

namespace SharkMXEmail
{
    internal class Modem
    {
        private string device;
        private int speed;
        private SerialPort port;

        private byte[] dialToneWav;
        
        private bool sendDialTone;
        private DateTime timeSinceLastDialTone;
        private int dialToneCounter;

        public Modem(string deviceName)
        {
            device = deviceName;
            speed = 576000;

            var dialToneAudioBytes = File.ReadAllBytes("assets/dial-tone.wav");
            dialToneWav = new byte[dialToneAudioBytes.Length - 44]; 
            Buffer.BlockCopy(dialToneAudioBytes, 44, dialToneWav, 0, dialToneAudioBytes.Length - 44); // strip the header

            sendDialTone = false;

            port = new SerialPort(device, speed);
            //port.DtrEnable = true;
            port.Open();
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedCallback);
        }

        private void DataReceivedCallback(object sender, SerialDataReceivedEventArgs e)
        {
            if (!port.IsOpen) return;

            int bytes = port.BytesToRead;
            byte[] buffer = new byte[bytes];
            port.Read(buffer, 0, bytes);

            HandleSerialDataCallback(buffer);
        }

        private void HandleSerialDataCallback(byte[] buffer)
        {
            Console.WriteLine("Received bytes: {0}\n", ByteArrayToHexString(buffer));
        }

        private void Connect()
        {
            Console.WriteLine("Starting dial tone.. waiting for a call...");
            var mode = "LISTENING";
            StartDialTone();


            while (true)
            {
                var now = DateTime.Now;

                if (mode == "LISTENING")
                {
                    Update();
                    //if ()
                }
            }
        }

        private void StartDialTone()
        {
            SendCommand("AT+FCLASS=8");  // Enter voice mode
            SendCommand("AT+VLS=1"); // Go off-hook
            SendCommand("AT+VSM=1,8000"); // 8 bit unsigned PCM
            SendCommand("AT+VTX"); // Voice transmission mode

            sendDialTone = true;
            timeSinceLastDialTone = DateTime.Now.AddSeconds(-100);
        }

        private void SendCommand(string command)
        {
            var c = String.Format("{0}\r\n", command);
            port.Write(c);
        }

        private void Update()
        {
            if (sendDialTone)
            {
                var now = DateTime.Now;

                var BUFFER_LENGTH = 1000;
                var TIME_BETWEEN_UPLOAD_MS = (1000.0 / 8000.0) * BUFFER_LENGTH;

                var ms = (now - timeSinceLastDialTone).TotalMilliseconds;
                if (ms >= TIME_BETWEEN_UPLOAD_MS)
                {
                    var dat = new byte[BUFFER_LENGTH];
                    Buffer.BlockCopy(dialToneWav, dialToneCounter, dat, 0, BUFFER_LENGTH);

                    dialToneCounter += BUFFER_LENGTH;
                    if (dialToneCounter >= 37000) // last divisible section of dialtone before out of bounds
                    {
                        dialToneCounter = 0;
                    }

                    port.Write(dat, 0, dat.Length);
                    timeSinceLastDialTone = DateTime.Now;
                }
            }
        }

        private static string ByteArrayToHexString(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder(Bytes.Length * 2);
            string HexAlphabet = "0123456789ABCDEF";

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
            }

            return Result.ToString();
        }

        public void Start()
        {
            Connect();

            Console.WriteLine("Waiting to receive data...");

            while (true)
            {
                
            }
        }
    }
}
