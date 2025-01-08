using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using RABD.Lib;
using RABD.DROE.SystemDefine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace DROE_CSharp_API_Sample
{
    static class Program
    {
        static SerialPort serialPort;
        const String myIP = "192.168.1.1", robotIP = "192.168.1.0";

        static void Main()
        {
            arduinoCommTest();

            Robot robot = new Robot();
            robot.ConnectRobot(myIP, robotIP, 11000);

            if(robot.IsConnected()) //connect test
            {
                Console.WriteLine("Connected to robot");
                robot.StartAPIMoveFunction();
            }
            else
            {
                Console.WriteLine("Failed to connect to robot");
            }
            
        }

        static void arduinoCommTest()
        {
            string portName = "COM11";
            int baudRate = 9600;
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            Thread.Sleep(2000);
            string command = "1*";

            for (int i = 0; i < 5; i++)
            {
                command = "1*";
                serialPort.Write(command);
                Thread.Sleep(600);
                command = "0*";
                serialPort.Write(command);
                Thread.Sleep(600);
            }

            serialPort.Close();
        }
    }
}
