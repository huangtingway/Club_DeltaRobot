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
        static SerialPort serialPort; //arduino communication

        static Robot robot = new Robot();
        const String myIP = "192.168.1.2", robotIP = "192.168.1.1";
        const int SPEED = 80;
        static cPoint HOMEPOS = new cPoint();

        static void Main()
        {
            //initArduino();
            //serialPort.Write("yellowLight*");

            initRobot();
            testRobot();
            //Console.WriteLine("自動測試完成, 請確認來料完全補滿");
            //Thread.Sleep(500);
            //
            //int pressTime = 0;
            //
            //while (true)
            //{
            //    bool isFininsh = false;
            //
            //    for (int i = 0; i < 10; i++)
            //    {
            //        //serialPort.Write("greenLight*");
            //        //testRobot();
            //        //Console.WriteLine("自動測試完成, 請確認來料完全補滿");
            //        Console.WriteLine("按一下開始按鈕啟動執行, 或長按開始按鈕1秒結束程式");
            //        pressTime = detectBtnPress();
            //
            //        while (true)
            //        {
            //            if (pressTime >= 800)
            //            {
            //                isFininsh = true;
            //                break;
            //            }
            //            else if (pressTime <= 300) break;
            //            else pressTime = detectBtnPress();
            //        }
            //
            //        if (isFininsh == true) break;
            //        //serialPort.Write("yellowLight*");
            //
            //        //work flow=======================================================
            //
            //
            //
            //        //================================================================
            //
            //        Thread.Sleep(500);
            //        Console.WriteLine("執行結束");
            //        //serialPort.Write("greenLight*");
            //        //serialPort.Write("blink*");
            //    }
            //
            //    if (isFininsh == true) break;
            //    //serialPort.Write("redLight*");
            //    Console.WriteLine("補料完成後, 長按開始按鈕1秒");
            //    pressTime = detectBtnPress();
            //
            //    while (true)
            //    {
            //        if (pressTime >= 800) break;
            //        else pressTime = detectBtnPress();
            //    }
            //}
            
            robotOff();
            Thread.Sleep(1000);
            //serialPort.Close();
        }

        static void initArduino()
        {
            string portName = "COM10";
            int baudRate = 9600;
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            Thread.Sleep(2000);
            serialPort.Write("blink*");
        }

        static void initRobot()
        {
            robot.ConnectRobot(robotIP, myIP, 11000);
            Thread.Sleep(1000);
            Console.WriteLine("Connected to robot");

            robot.ResetAlarm();
            Thread.Sleep(100);
            robot.StartAPIMoveFunction();
            robot.ServoOn();
            Thread.Sleep(100);

            robot.SetSpeed(SPEED);
            robot.SetOverrideSpeed(SPEED);
            Thread.Sleep(100);

            robot.FrameSelect(0, 0);
            robot.GoHome();
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus())
                {
                    Console.WriteLine("Moveing to origin home");
                }
                else break;

                Thread.Sleep(100);
            }

            Thread.Sleep(200);

            HOMEPOS[eAxisName.X] = 340000;
            HOMEPOS[eAxisName.Y] = 0;
            HOMEPOS[eAxisName.Z] = -30000;
            HOMEPOS[eAxisName.RZ] = 90000;
            movePTP(HOMEPOS); 
            Thread.Sleep(200);

            robot.GoHome();
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus())
                {
                    Console.WriteLine("Moveing to origin home");
                }
                else break;

                Thread.Sleep(100);
            }

            Thread.Sleep(500);
            Console.WriteLine("robot init");
        }

        static void testRobot()
        {
            Console.WriteLine("moving Rel test");
            movePTPRel(50, 0, 0, 0);
            movePTPRel(-50, 0, 0, 0);
            movePTPRel(0, 50, 0, 0);
            movePTPRel(0, -50, 0, 0);
            movePTPRel(0, 0, 50, 0);
            movePTPRel(0, 0, -50, 0);
            movePTPRel(0, 0, 0, 90);
            movePTPRel(0, 0, 0, -90);
            Console.WriteLine("moving Rel test complete");
            Thread.Sleep(1000);

            Console.WriteLine("press button");
            int pressTime = detectBtnPress();
            Console.WriteLine("press time: " + pressTime);
            Console.WriteLine("button test complete");
            Thread.Sleep(1000);

            Console.WriteLine("Pneumatic test");
            robot.SetOutputState(0, true);
            Thread.Sleep(1000);
            robot.SetOutputState(0, false);
            Thread.Sleep(1000);
            robot.SetOutputState(1, true);
            Thread.Sleep(1000);
            robot.SetOutputState(1, false);
            Console.WriteLine("Pneumatic test complete");
            Thread.Sleep(1000);

            //TODO: move source obj pos test
        }

        static void robotOff()
        {
            movePTP(HOMEPOS);
            robot.ServoOff();
            robot.CloseAPIMoveFunction();
            Console.WriteLine("robot off");
            Thread.Sleep(1000);
            robot.DisConnectRobot();
        }

        static void movePTP(cPoint pos)
        {
            robot.GotoMovP(pos);
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(100);
            }

            Thread.Sleep(500);
        }

        static void movePTP(cPoint pos, double offsetX, double offsetY, double offsetZ, double offsetRz)
        {
            pos[eAxisName.X] += offsetX;
            pos[eAxisName.Y] += offsetY;
            pos[eAxisName.Z] += offsetZ;
            pos[eAxisName.RZ] += offsetRz;
            robot.GotoMovP(pos);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(50);
            }

            Thread.Sleep(100);
        }

        static void moveLin(cPoint pos)
        {
            robot.GotoMovL(pos);
            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(50);
            }

            Thread.Sleep(100);
        }

        static void moveLin(cPoint pos, double offsetX, double offsetY, double offsetZ, double offsetRz)
        {
            pos[eAxisName.X] += offsetX;
            pos[eAxisName.Y] += offsetY;
            pos[eAxisName.Z] += offsetZ;
            pos[eAxisName.RZ] += offsetRz;
            robot.GotoMovL(pos);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(50);
            }

            Thread.Sleep(100);
        }

        static void movePTPRel(double x, double y, double z, double Rz)
        {
            cPoint currrentPos = robot.GetPos();
            currrentPos[eAxisName.X] += x;
            currrentPos[eAxisName.Y] += y;
            currrentPos[eAxisName.Z] += z;
            currrentPos[eAxisName.RZ] += Rz;
            robot.GotoMovP(currrentPos);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(50);
            }

            Thread.Sleep(100);
        }

        static void moveLinRel(double x, double y, double z, double Rz)
        {
            cPoint currrentPos = robot.GetPos();
            currrentPos[eAxisName.X] += x;
            currrentPos[eAxisName.Y] += y;
            currrentPos[eAxisName.Z] += z;
            currrentPos[eAxisName.RZ] += Rz;
            robot.GotoMovL(currrentPos);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(50);
            }

            Thread.Sleep(100);
        }

        static int detectBtnPress()
        {
            Stopwatch sw = new Stopwatch();
            int pressTime = 0;
            bool isPressed = false;

            while (true)
            {
                bool pressState = robot.GetInputState(0);

                if (pressState == true && isPressed == false)
                {
                    sw.Start();
                    isPressed = true;
                }
                else if (pressState == false && isPressed == true) break;
                else if (pressState == true && isPressed == true)
                {
                    pressTime = (int)sw.ElapsedMilliseconds;
                }
                
                Thread.Sleep(50);
            }

            return pressTime;
        }
    }
}
