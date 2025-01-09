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
        static Robot robot = new Robot();
        static SerialPort serialPort;
        const String myIP = "192.168.1.2", robotIP = "192.168.1.1";

        static void Main()
        {
            //arduinoCommTest();
            initRobot();
            //testRobot();
            robot.DisConnectRobot();
        }

        static void arduinoCommTest()
        {
            string portName = "COM10";
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

        static void initRobot()
        {
            robot.ConnectRobot(robotIP, myIP, 11000);
            Thread.Sleep(1000);
            robot.ResetAlarm();
            robot.StartAPIMoveFunction();
            robot.ServoOn();
            robot.SetSpeed(40);
            Thread.Sleep(1000);
            robot.GoHome();

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
            }

            Thread.Sleep(5000);
            Console.WriteLine("robot go home");
            Console.WriteLine("Connected to robot");
        }

        static void testRobot()
        {
            cPoint pos1 = new cPoint();
            pos1[eAxisName.X] = 360;
            pos1[eAxisName.Y] = 90;
            pos1[eAxisName.Z] = -40;
            pos1[eAxisName.RZ] = 360;
            robot.GotoMovP(pos1);
            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
            }

            robot.GoHome();

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
            }

            robot.ServoOff();
        }

    }
}
