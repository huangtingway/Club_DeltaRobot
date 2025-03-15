using System;
using System.Diagnostics;
using RABD.Lib;
using RABD.DROE.SystemDefine;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
public class MyRobot
{
    private bool isFullSpeed = false;

    private Robot robot;
    private String myIP = "192.168.1.100", robotIP = "192.168.1.1";
    int CRUISE_SPEED = 10;
    int CRUISE_ACC_SPEED = 20;
    int CRUISE_DEC_SPEED = 20;

    int LOAD_SPEED = 10;
    int LOAD_ACC_SPEED = 20;
    int LOAD_DEC_SPEED = 20;

    public MyRobot()
    {
        robot = new Robot();
    }

    public MyRobot(string robotIP, string myIP, bool openFullSpeed)
    {
        robot = new Robot();
        this.robotIP = robotIP;
        this.myIP = myIP;
        this.isFullSpeed = openFullSpeed;
    }

    public Robot getRobotInstance()
    {
        return robot;
    }

    public void initRobot()
    {
        robot.ConnectRobot(robotIP, myIP, 10000);
        Thread.Sleep(500);
        robot.ResetAlarm();
        Thread.Sleep(100);
        robot.StopProgram();
        Thread.Sleep(1000);

        if (isFullSpeed)
        {
            robot.StartCmd();
            Thread.Sleep(3000);
        }
        
        robot.FrameSelect(0, 0);
        Thread.Sleep(100);
        robot.StartAPIMoveFunction();
        Thread.Sleep(100);
        Console.WriteLine("Connected to robot");

        string robotInfo = "robotInfo:\n";
        robotInfo += "firmware version: " + robot.GetFirmwareVersion() + "\n";
        robotInfo += "API version: " + robot.APIVersion() + "\n";
        robotInfo += "full speed: " + isFullSpeed + "\n";
        Console.WriteLine(robotInfo);

        Console.WriteLine("robot init");
    }

    public void robotOn()
    {
        if (robot.ServoState() == true) return;
        robot.ResetAlarm();
        Thread.Sleep(100);
        robot.ServoOn();
        Thread.Sleep(100);
    }

    public void robotOff()
    {
        robot.ServoOff();
        Thread.Sleep(100);
        robot.CloseAPIMoveFunction();
        Console.WriteLine("robot off");
        Thread.Sleep(100);

        if (isFullSpeed)
        {
            robot.EndCmd();
            Thread.Sleep(100);
        }

        robot.DisConnectRobot();
        Thread.Sleep(100);
    }

    public void movePTP(cPoint pos)
    {
        robot.MovP(pos);
        Thread.Sleep(300);

        while (true)
        {
            if (robot.RobotMovingStatus() == false)
            {
                cPoint tempPos = robot.GetPos();
                Thread.Sleep(100);

                if (tempPos[eAxisName.X] == pos[eAxisName.X] &&
                    tempPos[eAxisName.Y] == pos[eAxisName.Y] &&
                    tempPos[eAxisName.Z] == pos[eAxisName.Z] &&
                    tempPos[eAxisName.RZ] == pos[eAxisName.RZ])
                {
                    break;
                }
                else
                {
                    robot.MovP(pos);
                    Thread.Sleep(500);
                    continue;
                }
            }

            Thread.Sleep(100);
        }

        Thread.Sleep(100);
    }

    public void movePTP(cPoint pos, double offsetX, double offsetY, double offsetZ, double offsetRz)
    {
        pos[eAxisName.X] += offsetX * 1000;
        pos[eAxisName.Y] += offsetY * 1000;
        pos[eAxisName.Z] += offsetZ * 1000;
        pos[eAxisName.RZ] += offsetRz * 1000;

        robot.MovP(pos);
        Thread.Sleep(500);

        while (true)
        {
            if (robot.RobotMovingStatus() == false)
            {
                cPoint tempPos = robot.GetPos();
                Thread.Sleep(100);

                if (tempPos[eAxisName.X] == pos[eAxisName.X] &&
                    tempPos[eAxisName.Y] == pos[eAxisName.Y] &&
                    tempPos[eAxisName.Z] == pos[eAxisName.Z] &&
                    tempPos[eAxisName.RZ] == pos[eAxisName.RZ])
                {
                    break;
                }
                else
                {
                    robot.MovP(pos);
                    Thread.Sleep(500);
                    continue;
                }
            }

            Thread.Sleep(100);
        }

        pos[eAxisName.X] -= offsetX * 1000;
        pos[eAxisName.Y] -= offsetY * 1000;
        pos[eAxisName.Z] -= offsetZ * 1000;
        pos[eAxisName.RZ] -= offsetRz * 1000;
        Thread.Sleep(100);
    }

    public void movePTPRel(double x, double y, double z, double Rz)
    {
        cPoint currrentPos = robot.GetPos();
        Thread.Sleep(200);
        currrentPos[eAxisName.X] += x * 1000;
        currrentPos[eAxisName.Y] += y * 1000;
        currrentPos[eAxisName.Z] += z * 1000;
        currrentPos[eAxisName.RZ] += Rz * 1000;
        robot.MovP(currrentPos);
        Thread.Sleep(500);

        while (true)
        {
            if (robot.RobotMovingStatus() == false)
            {
                cPoint tempPos = robot.GetPos();
                Thread.Sleep(100);

                if (tempPos[eAxisName.X] == currrentPos[eAxisName.X] &&
                    tempPos[eAxisName.Y] == currrentPos[eAxisName.Y] &&
                    tempPos[eAxisName.Z] == currrentPos[eAxisName.Z] &&
                    tempPos[eAxisName.RZ] == currrentPos[eAxisName.RZ])
                {
                    break;
                }
                else
                {
                    robot.MovP(currrentPos);
                    Thread.Sleep(500);
                    continue;
                }
            }
            Thread.Sleep(100);
        }

        Thread.Sleep(100);
    }

    public void moveLin(cPoint pos)
    {
        robot.MovL(pos);
        Thread.Sleep(300);

        while (true)
        {
            if (robot.RobotMovingStatus() == false)
            {
                cPoint tempPos = robot.GetPos();
                Thread.Sleep(100);

                if (tempPos[eAxisName.X] == pos[eAxisName.X] &&
                    tempPos[eAxisName.Y] == pos[eAxisName.Y] &&
                    tempPos[eAxisName.Z] == pos[eAxisName.Z] &&
                    tempPos[eAxisName.RZ] == pos[eAxisName.RZ])
                {
                    break;
                }
                else
                {
                    robot.MovL(pos);
                    Thread.Sleep(500);
                    continue;
                }
            }

            Thread.Sleep(100);
        }

        Thread.Sleep(100);
    }

    public void moveLin(cPoint pos, double offsetX, double offsetY, double offsetZ, double offsetRz)
    {
        pos[eAxisName.X] += offsetX * 1000;
        pos[eAxisName.Y] += offsetY * 1000;
        pos[eAxisName.Z] += offsetZ * 1000;
        pos[eAxisName.RZ] += offsetRz * 1000;

        robot.MovL(pos);
        Thread.Sleep(500);

        while (true)
        {
            if (robot.RobotMovingStatus() == false)
            {
                cPoint tempPos = robot.GetPos();
                Thread.Sleep(100);

                if (tempPos[eAxisName.X] == pos[eAxisName.X] &&
                    tempPos[eAxisName.Y] == pos[eAxisName.Y] &&
                    tempPos[eAxisName.Z] == pos[eAxisName.Z] &&
                    tempPos[eAxisName.RZ] == pos[eAxisName.RZ])
                {
                    break;
                }
                else
                {
                    robot.MovL(pos);
                    Thread.Sleep(500);
                    continue;
                }
            }

            Thread.Sleep(100);
        }

        pos[eAxisName.X] -= offsetX * 1000;
        pos[eAxisName.Y] -= offsetY * 1000;
        pos[eAxisName.Z] -= offsetZ * 1000;
        pos[eAxisName.RZ] -= offsetRz * 1000;
        Thread.Sleep(100);
    }
    
    public void moveLinRel(double x, double y, double z, double Rz)
    {
        cPoint currrentPos = robot.GetPos();
        Thread.Sleep(200);
        currrentPos[eAxisName.X] += x * 1000;
        currrentPos[eAxisName.Y] += y * 1000;
        currrentPos[eAxisName.Z] += z * 1000;
        currrentPos[eAxisName.RZ] += Rz * 1000;
        robot.MovL(currrentPos);
        Thread.Sleep(500);

        while (true)
        {
            if (robot.RobotMovingStatus() == false)
            {
                cPoint tempPos = robot.GetPos();
                Thread.Sleep(100);

                if (tempPos[eAxisName.X] == currrentPos[eAxisName.X] &&
                    tempPos[eAxisName.Y] == currrentPos[eAxisName.Y] &&
                    tempPos[eAxisName.Z] == currrentPos[eAxisName.Z] &&
                    tempPos[eAxisName.RZ] == currrentPos[eAxisName.RZ])
                {
                    break;
                }
                else
                {
                    robot.MovL(currrentPos);
                    Thread.Sleep(500);
                    continue;
                }
            }
            Thread.Sleep(100);
        }

        Thread.Sleep(100);
    }
    
    public void setSpeedParams()
    {

    }

    public void speedUp()
    {

    }

    public void speedDown()
    {

    }

    public void setFullSpeed(bool openFullSpeed)
    {
        this.isFullSpeed = openFullSpeed;
    }

    public bool getFullSpeed()
    {
        return this.isFullSpeed;
    }

    public void setRobotIP(string robotIP)
    {
        this.robotIP = robotIP;
    }

    public string getRobotIP()
    {
        return this.robotIP;
    }

    public void setMyIP(string myIP)
    {
        this.myIP = myIP;
    }

    public string getMyIP()
    {
        return this.myIP;
    }
}
