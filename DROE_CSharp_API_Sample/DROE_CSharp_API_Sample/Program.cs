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
        const bool TEST_MODE = false;

        //basic parameter
        static Robot robot = new Robot();
        const String myIP = "192.168.1.2", robotIP = "192.168.1.1";
        const int CRUISE_SPEED = 100;
        const int LOAD_SPEED = 75;
        const int LOAD_ACC_SPEED = 80;
        const int LOAD_DEC_SPEED = 50;
        const int CRUISE_ACC_SPEED = 100;
        const int CRUISE_DEC_SPEED = 100;
        const int SUCTION_INDEX = 2;
        const int CYLINDER_INDEX = 3;
        //const int PROJECT_INDEX = 9;

        //basic position
        static cPoint HOME_POS = new cPoint();
        static cPoint GET_NET_PLAT_POS = new cPoint();
        static cPoint GET_BASE_FRAME_POS = new cPoint();
        static cPoint GET_PICTURE_POS = new cPoint();
        static cPoint GET_TOP_FRAME_POS = new cPoint();
        static cPoint GET_SCREW_POS = new cPoint();
        static cPoint EXPORT_POS = new cPoint();
        static cPoint COMPOSE_POS = new cPoint();
        static cPoint LOCK_SCREW_POS = new cPoint();

        //object size
        static int FRAME_LENGTH = 63;
        static int FRAME_WIDTH = 53;

        //margin
        static int SCREW_MARGIN_X = 10;
        static int SCREW_MARGIN_Y = 10;
        static int COMPOSE_SCREW_MARGIN_X = 10;
        static int COMPOSE_SCREW_MARGIN_Y = 10;

        //height offset
        static int NET_PLAT_HEIGHT_OFFSET = 35;
        static int BOTTOM_FRAME_HEIGHT_OFFSET = 35;
        static int TOP_FRAME_HEIGHT_OFFSET = 35;
        static int PICTURE_HEIGHT_OFFSET = 45;
        static int SCREW_HEIGHT_OFFSET = 25;
        static int COMPOSE_HEIGHT_OFFSET = 30;

        static void Main()
        {
            initArduino();
            serialPort.Write("yellowLight*");
            initRobot();
            testRobot();
            Console.WriteLine("自動測試完成, 請確認來料完全補滿");
            Thread.Sleep(500);

            int pressTime = 0;

            while (true)
            {
                bool isFininsh = false;
                //testRobot();

                for (int i = 0; i < 1; i++)
                {
                    serialPort.Write("greenLight*");
                    Console.WriteLine("自動測試完成, 請確認來料完全補滿");
                    Console.WriteLine("按一下開始按鈕啟動執行, 或長按開始按鈕1秒結束程式");
                    pressTime = detectBtnPress();

                    while (true)
                    {
                        if (pressTime >= 800)
                        {
                            isFininsh = true;
                            break;
                        }
                        else if (pressTime <= 300) break;
                        else pressTime = detectBtnPress();
                    }

                    if (isFininsh == true) break;
                    serialPort.Write("redLight*");

                    //work flow=======================================================
                    getNetPlat();
                    getBaseFrame();
                    getPicture();
                    getTopFrame();
                    //getScrew();
                    //export();

                    //================================================================

                    Thread.Sleep(500);
                    Console.WriteLine("執行結束");
                    serialPort.Write("blink*");
                    Thread.Sleep(500);
                }

                if (isFininsh == true) break;

                serialPort.Write("redBlink*");
                Console.WriteLine("補料完成後, 長按開始按鈕1秒");
                pressTime = detectBtnPress();

                while (true)
                {
                    if (pressTime >= 800) break;
                    else pressTime = detectBtnPress();
                }

                //reset height

            }

            robotOff();
            Thread.Sleep(1000);
            serialPort.Close();
        }

        static void initArduino()
        {
            string portName = "COM10";
            int baudRate = 9600;
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            Thread.Sleep(2000);
            serialPort.Write("blink*");
            Thread.Sleep(800);
        }

        static void initRobot()
        {
            robot.ConnectRobot(robotIP, myIP, 11000);
            Thread.Sleep(500);
            Console.WriteLine("Connected to robot");

            robot.ResetAlarm();
            Thread.Sleep(100);
            robot.StartAPIMoveFunction();
            robot.ServoOn();
            Thread.Sleep(100);

            String errCode = robot.API_MoveFuction_Status();
            Console.WriteLine("API move function status: " + errCode);
            Thread.Sleep(100);

            robot.SetSpeed(CRUISE_SPEED);
            robot.SetOverrideSpeed(CRUISE_SPEED);
            Thread.Sleep(100);

            robot.FrameSelect(0, 0);
            robot.GoHome();
            Thread.Sleep(200);

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

            initPos();
            movePTP(HOME_POS);
            Console.WriteLine("robot init");
        }

        static void initPos()
        {
            HOME_POS = robot.GetGlobalPoint(0);
            Thread.Sleep(100);
            COMPOSE_POS = robot.GetGlobalPoint(1);
            Thread.Sleep(100);
            GET_NET_PLAT_POS = robot.GetGlobalPoint(2);
            Thread.Sleep(100);
            GET_BASE_FRAME_POS = robot.GetGlobalPoint(3);
            Thread.Sleep(100);
            GET_PICTURE_POS = robot.GetGlobalPoint(4);
            Thread.Sleep(100);
            GET_TOP_FRAME_POS = robot.GetGlobalPoint(5);
            Thread.Sleep(100);
            GET_SCREW_POS = robot.GetGlobalPoint(6);
            Thread.Sleep(100);
            LOCK_SCREW_POS = robot.GetGlobalPoint(7);
            Thread.Sleep(100);
            EXPORT_POS = robot.GetGlobalPoint(8);
            Thread.Sleep(100);
        }

        static void testRobot()
        {
            if(TEST_MODE == false) return;

            Console.WriteLine("moving Rel test");
            movePTPRel(50, 0, 0, 0);
            movePTPRel(-50, 0, 0, 0);
            movePTPRel(0, 100, 0, 0);
            movePTPRel(0, -100, 0, 0);
            moveLinRel(0, 0, -50, 0);
            moveLinRel(0, 0, 50, 0);
            movePTPRel(0, 0, 0, 90);
            movePTPRel(0, 0, 0, -90);
            Console.WriteLine("moving Rel test complete");
            Thread.Sleep(500);

            Console.WriteLine("Pneumatic test");
            robot.SetOutputState(SUCTION_INDEX, true);
            Thread.Sleep(1000);
            robot.SetOutputState(SUCTION_INDEX, false);
            Thread.Sleep(500);
            robot.SetOutputState(CYLINDER_INDEX, true);
            Thread.Sleep(1000);
            robot.SetOutputState(CYLINDER_INDEX, false);
            Console.WriteLine("Pneumatic test complete");
            Thread.Sleep(1000);

            Console.WriteLine("press button");
            int pressTime = detectBtnPress();
            Console.WriteLine("press time: " + pressTime);
            Console.WriteLine("button test complete");
            Thread.Sleep(500);

            // Move to every defined position
            movePTP(GET_NET_PLAT_POS);
            movePTP(GET_BASE_FRAME_POS);
            movePTP(GET_PICTURE_POS);
            movePTP(GET_TOP_FRAME_POS);
            movePTP(GET_SCREW_POS);
            movePTP(LOCK_SCREW_POS);
            movePTP(COMPOSE_POS);
            movePTP(EXPORT_POS);
        }

        static void robotOff()
        {
            movePTP(HOME_POS);
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
            pos[eAxisName.X] += offsetX * 1000;
            pos[eAxisName.Y] += offsetY * 1000;
            pos[eAxisName.Z] += offsetZ * 1000;
            pos[eAxisName.RZ] += offsetRz * 1000;
            robot.GotoMovP(pos);
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(100);
            }

            Thread.Sleep(500);
        }

        static void moveLin(cPoint pos)
        {
            robot.GotoMovL(pos);
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(100);
            }

            Thread.Sleep(500);
        }

        static void moveLin(cPoint pos, double offsetX, double offsetY, double offsetZ, double offsetRz)
        {
            pos[eAxisName.X] += offsetX * 1000;
            pos[eAxisName.Y] += offsetY * 1000;
            pos[eAxisName.Z] += offsetZ * 1000;
            pos[eAxisName.RZ] += offsetRz * 1000;
            robot.GotoMovL(pos);
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(100);
            }

            Thread.Sleep(500);
        }

        static void movePTPRel(double x, double y, double z, double Rz)
        {
            cPoint currrentPos = robot.GetPos();
            currrentPos[eAxisName.X] += x * 1000;
            currrentPos[eAxisName.Y] += y * 1000;
            currrentPos[eAxisName.Z] += z * 1000;
            currrentPos[eAxisName.RZ] += Rz * 1000;
            robot.GotoMovP(currrentPos);
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(100);
            }

            Thread.Sleep(500);
        }

        static void moveLinRel(double x, double y, double z, double Rz)
        {
            cPoint currrentPos = robot.GetPos();
            currrentPos[eAxisName.X] += x * 1000;
            currrentPos[eAxisName.Y] += y * 1000;
            currrentPos[eAxisName.Z] += z * 1000;
            currrentPos[eAxisName.RZ] += Rz * 1000;
            robot.GotoMovL(currrentPos);
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(100);
            }

            Thread.Sleep(500);
        }

        static int detectBtnPress()
        {
            Stopwatch sw = new Stopwatch();
            int pressTime = 0;
            bool isPressed = false;

            while (true)
            {
                bool pressState = robot.GetInputState(3);

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

            Console.WriteLine("press time: " + pressTime);
            return pressTime;
        }
        
        static void speedUp()
        {
            robot.SetSpeed(CRUISE_SPEED);
            robot.SetOverrideSpeed(CRUISE_SPEED);
            Thread.Sleep(100);
            robot.SetAccelEx(CRUISE_ACC_SPEED);
            robot.SetDecelEx(CRUISE_DEC_SPEED);
            Thread.Sleep(100);
        }

        static void speedDown()
        {
            robot.SetSpeed(LOAD_SPEED);
            robot.SetOverrideSpeed(LOAD_SPEED);
            Thread.Sleep(100);
            robot.SetAccelEx(LOAD_ACC_SPEED);
            robot.SetDecelEx(LOAD_DEC_SPEED);
            Thread.Sleep(100);
        }
        
        static void getNetPlat()
        {
            movePTP(GET_NET_PLAT_POS);

            //get
            speedDown();
            moveLinRel(0, 0, -NET_PLAT_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX, true);
            Thread.Sleep(300);
            moveLinRel(0, 0, NET_PLAT_HEIGHT_OFFSET, 0);
            movePTP(COMPOSE_POS);

            //put
            moveLinRel(0, 0, -COMPOSE_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX, false);
            speedUp();
            moveLinRel(0, 0, COMPOSE_HEIGHT_OFFSET, 0);

            GET_NET_PLAT_POS[eAxisName.Z] += 3;
            COMPOSE_POS[eAxisName.Z] += 3;
        }

        static void getBaseFrame()
        {
            movePTP(GET_BASE_FRAME_POS);
            //get
            speedDown();
            moveLinRel(0, 0, -BOTTOM_FRAME_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX, true);
            Thread.Sleep(300);
            moveLinRel(0, 0, BOTTOM_FRAME_HEIGHT_OFFSET, 0);
            movePTP(COMPOSE_POS);

            //put
            moveLinRel(0, 0, -COMPOSE_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX, false);
            speedUp();
            moveLinRel(0, 0, COMPOSE_HEIGHT_OFFSET, 0);
            GET_BASE_FRAME_POS[eAxisName.Z] += 3;
            COMPOSE_POS[eAxisName.Z] += 3;
        }

        static void getPicture()
        {
            movePTP(GET_PICTURE_POS);
            //get
            speedDown();
            moveLinRel(0, 0, -PICTURE_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX, true);
            Thread.Sleep(300);
            moveLinRel(0, 0, PICTURE_HEIGHT_OFFSET, 0);
            movePTP(COMPOSE_POS);

            //put
            moveLinRel(0, 0, -COMPOSE_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX, false);
            speedUp();
            moveLinRel(0, 0, COMPOSE_HEIGHT_OFFSET, 0);
        }

        static void getTopFrame()
        {
            movePTP(GET_TOP_FRAME_POS);
            //get
            speedDown();
            moveLinRel(0, 0, -TOP_FRAME_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX, true);
            Thread.Sleep(300);
            moveLinRel(0, 0, TOP_FRAME_HEIGHT_OFFSET, 0);
            movePTP(COMPOSE_POS);

            //put
            moveLinRel(0, 0, -COMPOSE_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX, false);
            speedUp();
            moveLinRel(0, 0, COMPOSE_HEIGHT_OFFSET, 0);

            GET_TOP_FRAME_POS[eAxisName.Z] += 6;
            COMPOSE_POS[eAxisName.Z] += 6;
        }

    }
}
