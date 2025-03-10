using System;
using System.Diagnostics;
using RABD.Lib;
using RABD.DROE.SystemDefine;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace DROE_CSharp_API_Sample
{
    static class Program
    {
        const bool TEST_MODE = false;
        //static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        //basic parameter
        static Robot robot = new Robot();
        const String myIP = "192.168.1.2", robotIP = "192.168.1.1";
        const int CRUISE_SPEED = 45;
        const int LOAD_SPEED = 45;
        const int LOAD_ACC_SPEED = 100;
        const int LOAD_DEC_SPEED = 100;
        const int DOWN_SPEED = 10;
        const int DOWN_DEC_SPEED = 10;
        const int CRUISE_ACC_SPEED = 100;
        const int CRUISE_DEC_SPEED = 100;
        const int SUCTION_INDEX = 2;
        const int CYLINDER_INDEX = 3;

        //position
        static cPoint HOME_POS = new cPoint();
        static cPoint GET_BASE_FRAME_POS = new cPoint();
        static cPoint GET_PICTURE_POS = new cPoint();
        static cPoint GET_TOP_FRAME_POS = new cPoint();
        static cPoint GET_SCREW_POS = new cPoint(); //右下角
        static cPoint EXPORT_POS = new cPoint(); 
        static cPoint COMPOSE_POS = new cPoint();
        static cPoint LOCK_SCREW_POS = new cPoint();//左上角

        //object size
        static int FRAME_LENGTH = 63;
        static int FRAME_WIDTH = 53;

        //margin
        static int SCREW_MARGIN_X = 20;
        static int SCREW_MARGIN_Y = 35;
        static int COMPOSE_SCREW_MARGIN_X = 53;
        static int COMPOSE_SCREW_MARGIN_Y = 43;

        //height offset
        static double ORG_BOTTOM_FRAME_HEIGHT_OFFSET = 79;
        static double ORG_TOP_FRAME_HEIGHT_OFFSET = 44;
        static double PICTURE_HEIGHT_OFFSET = 45;
        static double SCREW_HEIGHT_OFFSET = 90.2;
        static double ORG_COMPOSE_HEIGHT_OFFSET = 103.5;
        static double EXPORT_HEIGHT_OFFSET = 80;
        static double LOCK_SCREW_HEIGHT_OFFSET = 17;

        static double bottomFrameHeightOffset = ORG_BOTTOM_FRAME_HEIGHT_OFFSET;
        static double topFrameHeightOffset = ORG_TOP_FRAME_HEIGHT_OFFSET;
        static double composeHeightOffset = ORG_COMPOSE_HEIGHT_OFFSET;

        static void Main()
        {
            initRobot();
            Thread.Sleep(500);

            int pressTime = 0;

            while (true)
            {
                bool isFininsh = false;

                if(TEST_MODE == true)
                {
                    Console.WriteLine("自動測試中...");
                    robot.ServoOn();
                    Thread.Sleep(500);
                    testRobot();
                    robot.ServoOff();
                    Thread.Sleep(500);
                    Console.WriteLine("自動測試完成, 請確認來料完全補滿");
                }

                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("按一下開始按鈕啟動執行, 或長按開始按鈕1秒結束程式");

                    pressTime = detectBtnPress(); //detect is finish or not
                    while (true) 
                    {
                        if (pressTime >= 800)
                        {
                            isFininsh = true;
                            break;
                        }
                        else if (pressTime <= 500) break;
                        else pressTime = detectBtnPress();
                    }

                    if (isFininsh == true) break;

                    Console.WriteLine("組裝中... (執行第" + (i+1) + "次)");
                    robotOn();
                    Thread.Sleep(100);

                    //work flow=======================================================
                    robot.ResetAlarm();
                    Thread.Sleep(100);
                    speedUp();
                    //getBaseFrame();
                    //getPicture();
                    //moveLin(HOME_POS);
                    //getTopFrame();
                    //moveLin(HOME_POS);
                    //getTopFrame();
                    moveLin(COMPOSE_POS);
                    getScrew(i, 0, 0, 0);
                    getScrew(i, 1, -53, 0);
                    getScrew(i, 2, 0, -43);
                    getScrew(i, 3, -53, -43);
                    //export();
                    moveLin(HOME_POS);
                    //================================================================

                    robot.ServoOff();
                    Thread.Sleep(300);
                    Console.WriteLine("組裝完成");
                    composeHeightOffset = ORG_COMPOSE_HEIGHT_OFFSET; //reset height
                }

                if (isFininsh == true) break;

                Console.WriteLine("來料不足, 補料完成後長按開始按鈕1秒");
                pressTime = detectBtnPress(); 
                while (true)
                {
                    if (pressTime >= 800) break;
                    else pressTime = detectBtnPress();
                }

                //reset height
                bottomFrameHeightOffset = ORG_BOTTOM_FRAME_HEIGHT_OFFSET;
                topFrameHeightOffset = ORG_TOP_FRAME_HEIGHT_OFFSET;
            }

            moveLin(HOME_POS);
            robotOff();
            Thread.Sleep(300);
            return;
        }

        //static void timerTick(object sender, EventArgs e)
        //{
        //    cPoint temp = robot.GetPos();
        //}

        static void initRobot()
        {
            robot.ConnectRobot(robotIP, myIP, 11000);
            Thread.Sleep(500);
            //myTimer.Tick += new EventHandler(timerTick);
            //myTimer.Interval = 1000;
            //myTimer.Start();
            Console.WriteLine("Connected to robot");
            robotOn();
            initPos();
            moveLin(HOME_POS);
            Console.WriteLine("robot init");
        }

        static void initPos()
        {
            HOME_POS = robot.GetGlobalPoint(0);
            Thread.Sleep(150);
            COMPOSE_POS = robot.GetGlobalPoint(1);
            Thread.Sleep(150);
            GET_BASE_FRAME_POS = robot.GetGlobalPoint(2);
            Thread.Sleep(150);
            GET_PICTURE_POS = robot.GetGlobalPoint(3);
            Thread.Sleep(150);
            GET_TOP_FRAME_POS = robot.GetGlobalPoint(4);
            Thread.Sleep(150);
            GET_SCREW_POS = robot.GetGlobalPoint(5);
            Thread.Sleep(150);
            LOCK_SCREW_POS = robot.GetGlobalPoint(6);
            Thread.Sleep(150);
            EXPORT_POS = robot.GetGlobalPoint(7);
            Thread.Sleep(150);
        }

        static void testRobot()
        {
            if(TEST_MODE == false) return;

            Console.WriteLine("moving Rel test");
            moveLinRel(50, 0, 0, 0);
            moveLinRel(-50, 0, 0, 0);
            moveLinRel(0, 50, 0, 0);
            moveLinRel(0, -50, 0, 0);
            moveLinRel(0, 0, -30, 0);
            moveLinRel(0, 0, 30, 0);
            moveLinRel(0, 0, 0, 45);
            moveLinRel(0, 0, 0, -45);
            Console.WriteLine("moving Rel test complete");
            Thread.Sleep(300);

            Console.WriteLine("Pneumatic test");
            robot.SetOutputState(SUCTION_INDEX, true);
            Thread.Sleep(800);
            robot.SetOutputState(SUCTION_INDEX, false);
            Thread.Sleep(1000);
            robot.SetOutputState(CYLINDER_INDEX, true);
            Thread.Sleep(800);
            robot.SetOutputState(CYLINDER_INDEX, false);
            Console.WriteLine("Pneumatic test complete");
            Thread.Sleep(300);

            Console.WriteLine("press start button");
            int pressTime = detectBtnPress();
            Console.WriteLine("press time: " + pressTime);
            Console.WriteLine("button test complete");
            Thread.Sleep(100);

            // Move to every defined position
            moveLin(GET_BASE_FRAME_POS);
            Thread.Sleep(100);
            moveLin(GET_PICTURE_POS);
            Thread.Sleep(100);
            moveLin(GET_TOP_FRAME_POS);
            Thread.Sleep(100);
            moveLin(GET_SCREW_POS);
            Thread.Sleep(100);
            moveLin(COMPOSE_POS);
            Thread.Sleep(100);
            moveLin(LOCK_SCREW_POS);
            Thread.Sleep(100);
            moveLin(EXPORT_POS);
        }

        static void robotOn()
        {
            if(robot.ServoState() == true) return;

            robot.ResetAlarm();
            Thread.Sleep(100);
            robot.StartAPIMoveFunction();
            robot.ServoOn();
            Thread.Sleep(100);

            String errCode = robot.API_MoveFuction_Status();
            Thread.Sleep(100);
            Console.WriteLine("API move function status: " + errCode);

            robot.SetSpeed(CRUISE_SPEED);
            Thread.Sleep(100);
            robot.SetOverrideSpeed(CRUISE_SPEED);
            Thread.Sleep(100);

            robot.FrameSelect(0 , 0);
            Thread.Sleep(100);
        }

        static void robotOff()
        {
            moveLin(HOME_POS);
            robot.ServoOff();
            Thread.Sleep(100);
            robot.CloseAPIMoveFunction();
            Console.WriteLine("robot off");
            Thread.Sleep(100);
            //myTimer.Stop();
            robot.DisConnectRobot();
            Thread.Sleep(100);
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
            Thread.Sleep(400);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(150);
            }

            Thread.Sleep(300);
        }

        static void moveLin(cPoint pos, double offsetX, double offsetY, double offsetZ, double offsetRz)
        {
            pos[eAxisName.X] += offsetX * 1000;
            pos[eAxisName.Y] += offsetY * 1000;
            pos[eAxisName.Z] += offsetZ * 1000;
            pos[eAxisName.RZ] += offsetRz * 1000;
            robot.GotoMovL(pos);
            Thread.Sleep(400);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(150);
            }

            pos[eAxisName.X] -= offsetX * 1000;
            pos[eAxisName.Y] -= offsetY * 1000;
            pos[eAxisName.Z] -= offsetZ * 1000;
            pos[eAxisName.RZ] -= offsetRz * 1000;
            Thread.Sleep(300);
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
            Thread.Sleep(500);
            cPoint currrentPos = robot.GetPos();
            Thread.Sleep(100);
            currrentPos[eAxisName.X] += x * 1000;
            currrentPos[eAxisName.Y] += y * 1000;
            currrentPos[eAxisName.Z] += z * 1000;
            currrentPos[eAxisName.RZ] += Rz * 1000;
            robot.GotoMovL(currrentPos);
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(150);
            }

            Thread.Sleep(300);
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
            Thread.Sleep(100);
            robot.SetAccelEx(CRUISE_ACC_SPEED);
            Thread.Sleep(100);
            robot.SetDecelEx(CRUISE_DEC_SPEED);
            Thread.Sleep(100);
        }

        static void speedDown()
        {
            robot.SetSpeed(LOAD_SPEED);
            Thread.Sleep(100);
            robot.SetAccelEx(LOAD_ACC_SPEED);
            Thread.Sleep(100);
            robot.SetDecelEx(LOAD_DEC_SPEED);
            Thread.Sleep(100);
        }
        
        static void setGetObjectSpeed()
        {
            robot.SetSpeed(DOWN_SPEED);
            Thread.Sleep(100);
            robot.SetAccelEx(LOAD_ACC_SPEED);
            Thread.Sleep(100);
            robot.SetDecelEx(DOWN_DEC_SPEED);
            Thread.Sleep(100);
        }

        static void getBaseFrame()
        {
            moveLin(GET_BASE_FRAME_POS, 0, 10, 0, 0);
            //get
            setGetObjectSpeed();
            moveLinRel(0, 0, -bottomFrameHeightOffset, 0);
            moveLinRel(0, -10, 0, 0);
            robot.SetOutputState(CYLINDER_INDEX, true);
            speedDown();
            Thread.Sleep(300);
            moveLinRel(43 , 0 , 0 , 0);
            moveLinRel(0, 0, bottomFrameHeightOffset, 0);

            moveLin(COMPOSE_POS);

            //put
            moveLinRel(0, 0, -composeHeightOffset, 0);
            robot.SetOutputState(CYLINDER_INDEX , false);
            speedUp();
            moveLinRel(0, 0, composeHeightOffset, 0);

            bottomFrameHeightOffset += 7;
            composeHeightOffset -= 3;
        }

        static void getPicture()
        {
            moveLin(GET_PICTURE_POS);
            //get
            setGetObjectSpeed();
            moveLinRel(0, 0, -PICTURE_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX, true);
            speedDown();
            Thread.Sleep(300);
            moveLinRel(0, 0, PICTURE_HEIGHT_OFFSET, 0);

            moveLin(COMPOSE_POS);

            //put
            moveLinRel(0, 0, -composeHeightOffset, 0);
            robot.SetOutputState(SUCTION_INDEX, false);
            speedUp();
            moveLinRel(0, 0, composeHeightOffset, 0);
        }

        static void getAcrylic()
        {
            moveLin(GET_TOP_FRAME_POS, 0, 10, 0, 0);
            //get
            setGetObjectSpeed();
            moveLinRel(0 , 0 , -topFrameHeightOffset , 0);
            moveLinRel(0 , -10 , 0 , 0);
            robot.SetOutputState(CYLINDER_INDEX , true);
            speedDown();
            Thread.Sleep(300);
            moveLinRel(43 , 0 , 0 , 0);
            moveLinRel(0 , 0 , topFrameHeightOffset , 0);
            moveLin(COMPOSE_POS);

            //put
            moveLinRel(0 , 0 , -composeHeightOffset , 0);
            robot.SetOutputState(CYLINDER_INDEX , false);
            speedUp();
            moveLinRel(0 , 0 , composeHeightOffset , 0);

            topFrameHeightOffset += 7;
            composeHeightOffset += 3;
        }

        static void getTopFrame()
        {
            moveLin(GET_TOP_FRAME_POS, 0, 10, 0, 0);
            //get
            setGetObjectSpeed();
            moveLinRel(0, 0, -topFrameHeightOffset, 0);
            moveLinRel(0, -10, 0, 0);
            robot.SetOutputState(CYLINDER_INDEX, true);
            speedDown();
            Thread.Sleep(300);
            moveLinRel(43, 0, 0, 0);
            moveLinRel(0, 0, topFrameHeightOffset, 0);
            moveLin(COMPOSE_POS);

            //put
            moveLinRel(0, 0, -composeHeightOffset, -1);
            robot.SetOutputState(CYLINDER_INDEX, false);
            speedUp();
            moveLinRel(0, 0, composeHeightOffset, 1);

            topFrameHeightOffset += 7;
            composeHeightOffset -= 3;
        }

        static void getScrew(int round, int screwNum, int putXOffset, int putYOffset)
        {
            speedDown();
            cPoint getScrewPos = GET_SCREW_POS;
            cPoint lockScrewPos = LOCK_SCREW_POS;
            int screwXOffset = screwNum / 2;
            int screwYOffset = screwNum % 2;

            int getXOffset = (round * 2 + screwXOffset) * SCREW_MARGIN_X * 1000;
            int getYOffset = screwYOffset * SCREW_MARGIN_Y * 1000;

            getScrewPos[eAxisName.X] += getXOffset;
            getScrewPos[eAxisName.Y] += getYOffset;
            lockScrewPos[eAxisName.X] += putXOffset * 1000;
            lockScrewPos[eAxisName.Y] += putYOffset * 1000;

            moveLin(getScrewPos , 200 , 0 , 0 , 0);
            moveLin(getScrewPos, 0, 5, 0, 0);

            //get
            robot.SetSpeed(4);
            Thread.Sleep(100);
            moveLinRel(0, 0, -SCREW_HEIGHT_OFFSET, 0);
            moveLinRel(0, -3, 0, 0);
            robot.SetOutputState(CYLINDER_INDEX, true);
            speedDown();
            Thread.Sleep(300);
            moveLinRel(0, 0, SCREW_HEIGHT_OFFSET , 0);

            moveLin(COMPOSE_POS, -40 ,150, 0, 0);
            moveLin(COMPOSE_POS);
            moveLin(lockScrewPos);

            //put
            moveLinRel(0, 0, -LOCK_SCREW_HEIGHT_OFFSET, 0);
            robot.SetOutputState(CYLINDER_INDEX, false);
            speedUp();
            moveLinRel(0, 0, LOCK_SCREW_HEIGHT_OFFSET, 0);

            getScrewPos[eAxisName.X] -= getXOffset; //reset
            getScrewPos[eAxisName.Y] -= getYOffset;
            lockScrewPos[eAxisName.X] -= putXOffset * 1000;
            lockScrewPos[eAxisName.Y] -= putYOffset * 1000;
        }

        static void export()
        {
            moveLin(COMPOSE_POS);
            //get
            setGetObjectSpeed();
            moveLinRel(0, 0, -composeHeightOffset, 0);
            robot.SetOutputState(SUCTION_INDEX, true);
            speedDown();
            Thread.Sleep(300);
            moveLinRel(0, 0, composeHeightOffset, 0);
            moveLin(EXPORT_POS);

            //put
            moveLinRel(0, 0, -EXPORT_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX, false);
            speedUp();
            moveLinRel(0, 0, EXPORT_HEIGHT_OFFSET, 0);
        }
    }
}
