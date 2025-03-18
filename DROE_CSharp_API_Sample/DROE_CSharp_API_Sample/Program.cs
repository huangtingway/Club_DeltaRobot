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

        //basic parameter
        static Robot robot = new Robot();
        const String myIP = "192.168.1.100", robotIP = "192.168.1.1";

        const int CRUISE_SPEED = 10;
        const int CRUISE_ACC_SPEED = 5;
        const int CRUISE_DEC_SPEED = 5;

        const int LOAD_SPEED = 3;
        const int LOAD_ACC_SPEED = 3;
        const int LOAD_DEC_SPEED = 3;

        const int DOWN_SPEED = 2;
        const int DOWN_DEC_SPEED = 1;

        const int CYLINDER_INDEX = 2;
        const int SUCTION_INDEX1 = 3; //fornt
        const int SUCTION_INDEX2 = 5; //back

        //position
        static cPoint HOME_POS = new cPoint();
        static cPoint GET_BASE_FRAME_POS = new cPoint();
        static cPoint GET_PICTURE_POS = new cPoint();
        static cPoint GET_TOP_FRAME_POS = new cPoint();
        static cPoint GET_SCREW_POS = new cPoint(); //右下角
        static cPoint EXPORT_POS = new cPoint();
        static cPoint COMPOSE_POS = new cPoint();
        static cPoint LOCK_SCREW_POS = new cPoint();//左上角
        static cPoint GET_EXPORT_POS = new cPoint();
        static cPoint PRE_GET_SCREW_POS = new cPoint(); 

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
        static double ORG_TOP_FRAME_HEIGHT_OFFSET = 44.3;
        static double GET_PICTURE_HEIGHT_OFFSET = 23;
        static double PLACE_PICTURE_HEIGHT_OFFSET = 20.2;

        static double SCREW_HEIGHT_OFFSET = 90;
        static double COMPOSE_HEIGHT_OFFSET = 96;
        static double LOCK_SCREW_HEIGHT_OFFSET = 14.7;

        static double bottomFrameHeightOffset = ORG_BOTTOM_FRAME_HEIGHT_OFFSET;
        static double topFrameHeightOffset = ORG_TOP_FRAME_HEIGHT_OFFSET;

        static void Main()
        {
            initRobot();
            Thread.Sleep(500);

            int pressTime = 0;

            while (true)
            {
                if (TEST_MODE == true)
                {
                    Console.WriteLine("自動測試中...");
                    robot.ServoOn();
                    Thread.Sleep(500);
                    testRobot();
                    robot.ServoOff();
                    Thread.Sleep(500);
                    Console.WriteLine("自動測試完成, 請確認來料完全補滿");
                }

                bool isFininsh = false;

                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("按一下開始按鈕啟動執行, 或長按開始按鈕1秒結束程式");

                    pressTime = detectBtnPress(); //detect press time
                    while (true) //handle button event
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

                    Console.WriteLine("組裝中... (執行第" + (i + 1) + "次)");
                    robotOn();
                    Thread.Sleep(100);

                    if(robot.HasAlarm() == true || robot.HasWarning() == true)
                    {
                        robot.ResetAlarm();
                        Thread.Sleep(100);
                        robot.EndCmd();
                        Thread.Sleep(500);
                        robot.StartCmd();
                        Thread.Sleep(1000);
                    }

                    //work flow=======================================================
                    speedUp();
                    getBaseFrame();
                    moveLin(COMPOSE_POS);
                    getPicture();
                    moveLin(HOME_POS);
                    getTopFrame();
                    moveLin(HOME_POS);
                    getTopFrame();
                    moveLin(COMPOSE_POS);
                    getScrew(i , 0 , 0 , 0);
                    getScrew(i , 1 , -53.0 , 0.2);
                    getScrew(i , 2 , -0.3 , -43.4);
                    getScrew(i , 3 , -53.8 , -43.2);
                    export();
                    moveLin(HOME_POS);
                    //================================================================

                    robot.ServoOff();
                    Thread.Sleep(100);
                    Console.WriteLine("組裝完成");
                }

                if (isFininsh == true) break;

                Console.WriteLine("來料不足, 補料完成後長按開始按鈕1秒"); //material not enough 
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

        static void initRobot()
        {
            robot.ConnectRobot(robotIP, myIP, 10000);
            Thread.Sleep(500);
            initPos();
            robot.ResetAlarm();
            Thread.Sleep(100);
            robot.EndCmd();
            Thread.Sleep(500);
            robot.StartCmd();
            Thread.Sleep(1000);
            robot.FrameSelect(0, 0);
            Thread.Sleep(100);
            robot.StartAPIMoveFunction();
            Thread.Sleep(100);
            Console.WriteLine("Connected to robot");

            string robotInfo = "robotInfo:\n";
            robotInfo += "firmware version: " + robot.GetFirmwareVersion() + "\n";
            robotInfo += "API version: " + robot.APIVersion() + "\n";
            robotInfo += "test mode: " + TEST_MODE + "\n";
            Console.WriteLine(robotInfo);

            robotOn();
            moveLin(HOME_POS);
            Console.WriteLine("robot init");
        }

        static void initPos()
        {
            HOME_POS = robot.GetGlobalPoint(0);
            Thread.Sleep(50);
            COMPOSE_POS = robot.GetGlobalPoint(1);
            Thread.Sleep(50);
            GET_BASE_FRAME_POS = robot.GetGlobalPoint(2);
            Thread.Sleep(50);
            GET_PICTURE_POS = robot.GetGlobalPoint(3);
            Thread.Sleep(50);
            GET_TOP_FRAME_POS = robot.GetGlobalPoint(4);
            Thread.Sleep(50);
            GET_SCREW_POS = robot.GetGlobalPoint(5);
            Thread.Sleep(50);
            LOCK_SCREW_POS = robot.GetGlobalPoint(6);
            Thread.Sleep(50);
            EXPORT_POS = robot.GetGlobalPoint(7);
            Thread.Sleep(50);
            GET_EXPORT_POS = robot.GetGlobalPoint(8);
            Thread.Sleep(50);
            PRE_GET_SCREW_POS = robot.GetGlobalPoint(9);
            Thread.Sleep(50);
        }

        static void testRobot()
        {
            if (TEST_MODE == false) return;

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
            robot.SetOutputState(SUCTION_INDEX1, true);
            Thread.Sleep(800);
            robot.SetOutputState(SUCTION_INDEX1, false);
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
            if (robot.ServoState() == true) return;
            robot.ResetAlarm();
            Thread.Sleep(100);
            robot.EndCmd();
            Thread.Sleep(500);
            robot.StartCmd();
            Thread.Sleep(1000);
            robot.ServoOn();
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
            robot.EndCmd();
            Thread.Sleep(100);
            robot.StopProgram();
            robot.DisConnectRobot();
            Thread.Sleep(100);
        }

        static void movePTP(cPoint pos)
        {
            robot.MovP(pos);
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
            robot.MovP(pos);
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(100);
            }

            pos[eAxisName.X] -= offsetX * 1000;
            pos[eAxisName.Y] -= offsetY * 1000;
            pos[eAxisName.Z] -= offsetZ * 1000;
            pos[eAxisName.RZ] -= offsetRz * 1000;
            Thread.Sleep(500);
        }
        
        static void movePTPRel(double x, double y, double z, double Rz)
        {
            cPoint currrentPos = robot.GetPos();
            currrentPos[eAxisName.X] += x * 1000;
            currrentPos[eAxisName.Y] += y * 1000;
            currrentPos[eAxisName.Z] += z * 1000;
            currrentPos[eAxisName.RZ] += Rz * 1000;
            robot.MovP(currrentPos);
            Thread.Sleep(500);

            while (true)
            {
                if (robot.RobotMovingStatus() == false) break;
                Thread.Sleep(100);
            }

            currrentPos[eAxisName.X] -= x * 1000;
            currrentPos[eAxisName.Y] -= y * 1000;
            currrentPos[eAxisName.Z] -= z * 1000;
            currrentPos[eAxisName.RZ] -= Rz * 1000;
            Thread.Sleep(500);
        }

        static void moveLin(cPoint pos)
        {
            robot.MovL(pos);
            Thread.Sleep(100);

            while (true)
            {
                if (robot.RobotMovingStatus() == false)
                {
                    cPoint tempPos = robot.GetPos();
                    Thread.Sleep(100);

                    if (isArrivedPos(tempPos, pos) == false)
                    {
                        robot.MovL(pos);
                        Thread.Sleep(100);
                        continue;
                    }

                    break;
                }

                Thread.Sleep(100);
            }

            Thread.Sleep(100);
        }

        static void moveLin(cPoint pos, double offsetX, double offsetY, double offsetZ, double offsetRz)
        {
            pos[eAxisName.X] += offsetX * 1000;
            pos[eAxisName.Y] += offsetY * 1000;
            pos[eAxisName.Z] += offsetZ * 1000;
            pos[eAxisName.RZ] += offsetRz * 1000;

            robot.MovL(pos);
            Thread.Sleep(100);

            while (true)
            {
                if(robot.RobotMovingStatus() == false)
                {
                    cPoint tempPos = robot.GetPos();
                    Thread.Sleep(100);

                    if(isArrivedPos(tempPos , pos) == false)
                    {
                        robot.MovL(pos);
                        Thread.Sleep(100);
                        continue;
                    }

                    break;
                }

                Thread.Sleep(100);
            }

            pos[eAxisName.X] -= offsetX * 1000;
            pos[eAxisName.Y] -= offsetY * 1000;
            pos[eAxisName.Z] -= offsetZ * 1000;
            pos[eAxisName.RZ] -= offsetRz * 1000;
            Thread.Sleep(100);
        }

        static void moveLinRel(double x, double y, double z, double Rz)
        {
            cPoint currrentPos = robot.GetPos();
            Thread.Sleep(100);
            currrentPos[eAxisName.X] += x * 1000;
            currrentPos[eAxisName.Y] += y * 1000;
            currrentPos[eAxisName.Z] += z * 1000;
            currrentPos[eAxisName.RZ] += Rz * 1000;
            robot.MovL(currrentPos);
            Thread.Sleep(100);

            while (true)
            {
                if(robot.RobotMovingStatus() == false)
                {
                    cPoint tempPos = robot.GetPos();
                    Thread.Sleep(100);

                    if(isArrivedPos(tempPos , currrentPos) == false)
                    {
                        robot.MovL(currrentPos);
                        Thread.Sleep(100);
                        continue;
                    }

                    break;
                }

                Thread.Sleep(100);
            }

            Thread.Sleep(100);
        }

        static bool isArrivedPos(cPoint pos1, cPoint pos2)
        {
            double pos1X = pos1[eAxisName.X];
            double pos1Y = pos1[eAxisName.Y];
            double pos1Z = pos1[eAxisName.Z];
            double pos1RZ = pos1[eAxisName.RZ];
            double pos2X = pos2[eAxisName.X];
            double pos2Y  = pos2[eAxisName.Y];
            double pos2Z  = pos2[eAxisName.Z];
            double pos2RZ = pos2[eAxisName.RZ];

            if(Math.Abs(pos1X - pos2X) <= 1000 &&
            Math.Abs(pos1Y - pos2Y) <= 1000 &&
            Math.Abs(pos1Z - pos2Z) <= 1000 &&
            Math.Abs(pos1RZ - pos2RZ) <= 1000)
            {
                return true;
            }

            return false;
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
            Thread.Sleep(50);
            robot.SetSpeedEx(CRUISE_SPEED);
            Thread.Sleep(50);

            robot.SetAccelEx(CRUISE_ACC_SPEED);
            Thread.Sleep(50);
            robot.SetDecelEx(CRUISE_DEC_SPEED);
            Thread.Sleep(50);
            robot.SetAccurEx(eAccur.HIGH);
            Thread.Sleep(50);
        }

        static void speedDown()
        {
            robot.SetSpeed(LOAD_SPEED);
            Thread.Sleep(50);
            robot.SetSpeedEx(LOAD_SPEED);
            Thread.Sleep(50);
            robot.SetAccelEx(LOAD_ACC_SPEED);
            Thread.Sleep(50);
            robot.SetDecelEx(LOAD_DEC_SPEED);
            Thread.Sleep(50); 
            robot.SetAccurEx(eAccur.HIGH);
            Thread.Sleep(50);
        }

        static void setGetObjectSpeed()
        {
            robot.SetSpeed(DOWN_SPEED);
            Thread.Sleep(50);
            robot.SetSpeedEx(DOWN_SPEED);
            Thread.Sleep(50);
            robot.SetAccelEx(DOWN_DEC_SPEED);
            Thread.Sleep(50);
            robot.SetDecelEx(DOWN_DEC_SPEED);
            Thread.Sleep(50); 
            robot.SetAccurEx(eAccur.HIGH);
            Thread.Sleep(50);
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
            moveLinRel(43, 0, 0, 0);
            moveLinRel(0, 0, bottomFrameHeightOffset, 0);

            moveLin(COMPOSE_POS);

            //put
            moveLinRel(0, 0, -COMPOSE_HEIGHT_OFFSET , 0);
            robot.SetOutputState(CYLINDER_INDEX, false);
            speedUp();
            moveLinRel(0, 0, COMPOSE_HEIGHT_OFFSET , 0);

            bottomFrameHeightOffset += 7;
        }

        static void getPicture()
        {
            moveLin(GET_PICTURE_POS);

            //get
            setGetObjectSpeed();
            moveLinRel(0, 0, -GET_PICTURE_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX2, true);
            speedDown();
            Thread.Sleep(300);
            moveLinRel(0, 0, GET_PICTURE_HEIGHT_OFFSET , 0);

            //put
            moveLin(GET_EXPORT_POS);
            setGetObjectSpeed();
            moveLinRel(0, 0, -PLACE_PICTURE_HEIGHT_OFFSET, 0);
            robot.SetOutputState(SUCTION_INDEX2, false);
            Thread.Sleep(200);
            robot.SetAccelEx(1);
            moveLinRel(0, 0, PLACE_PICTURE_HEIGHT_OFFSET , 0);
            speedUp();
        }

        static void getTopFrame()
        {
            //cPoint currentPos = robot.GetPos();
            //robot.StartContinuousMovL(currentPos);
            //robot.PathL(HOME_POS);
            //robot.EndContinuousMovL(GET_TOP_FRAME_POS);
            //Thread.Sleep(100);

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
            moveLinRel(0, 0, -COMPOSE_HEIGHT_OFFSET , -1);
            robot.SetOutputState(CYLINDER_INDEX, false);
            speedUp();
            moveLinRel(0, 0, COMPOSE_HEIGHT_OFFSET , 1);

            topFrameHeightOffset += 7;
        }

        static void getScrew(int round, int screwNum, double putXOffset, double putYOffset)
        {
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

            moveLin(PRE_GET_SCREW_POS);
            moveLin(getScrewPos, 0, 3, 0, 0);

            //get
            setGetObjectSpeed();
            Thread.Sleep(100);
            moveLinRel(0, 0, -SCREW_HEIGHT_OFFSET, 0);
            moveLinRel(0, -3, 0, 0);
            robot.SetOutputState(CYLINDER_INDEX, true);
            speedDown();
            Thread.Sleep(300);
            moveLinRel(0, 0, SCREW_HEIGHT_OFFSET, 0);

            moveLin(COMPOSE_POS, -60, 150, 0, 0);
            moveLin(lockScrewPos);

            //put
            setGetObjectSpeed();
            moveLinRel(0, 0, -(LOCK_SCREW_HEIGHT_OFFSET * (3.0f / 4)), 0); //clamp down
            robot.SetOutputState(CYLINDER_INDEX, false);
            Thread.Sleep(250);
            moveLin(lockScrewPos, 0, 3, 0, 0);
            moveLinRel(0, -20, 0, 0);

            if (screwNum == 1 || screwNum == 3) //press down
            {
                moveLinRel(0, 0, -(LOCK_SCREW_HEIGHT_OFFSET + 0.4), 0);
                moveLinRel(0, 0, LOCK_SCREW_HEIGHT_OFFSET + 0.4, 0);
            }
            else
            {
                moveLinRel(0, 0, -LOCK_SCREW_HEIGHT_OFFSET, 0);
                moveLinRel(0, 0, LOCK_SCREW_HEIGHT_OFFSET, 0);
            }

            speedUp();

            getScrewPos[eAxisName.X] -= getXOffset; //reset
            getScrewPos[eAxisName.Y] -= getYOffset;
            lockScrewPos[eAxisName.X] -= putXOffset * 1000;
            lockScrewPos[eAxisName.Y] -= putYOffset * 1000;
        }

        static void export()
        {
            moveLin(GET_EXPORT_POS);

            //get
            setGetObjectSpeed();
            moveLinRel(0, 0, -18.5, 0);
            robot.SetOutputState(SUCTION_INDEX2, true);
            speedDown();
            Thread.Sleep(500);
            moveLinRel(0, 0, 18.5, 0);

            moveLin(EXPORT_POS);

            //put
            robot.SetOutputState(SUCTION_INDEX2, false);
            Thread.Sleep(100);
            speedUp();
        }
    }
}
