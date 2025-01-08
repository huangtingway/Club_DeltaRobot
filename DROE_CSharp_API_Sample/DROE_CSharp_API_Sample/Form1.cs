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

namespace DROE_CSharp_API_Sample
{
    public partial class Form1 : Form
    {
        Robot d = new Robot();
        Robot d1 = new Robot();
        bool scara = false;
        cPoint pos, pos1;
        int focus_RowIndex = 0;
        List<int> points = new List<int>();
        bool running = true;
        string modbusValue = "";
        bool connect = true;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//�ޥ�stopwatch����
        RobotMechanism robotMechanism;
        public Form1()
        {

            InitializeComponent();
            label45.Text = "V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            cbb_baudrate.Items.Add(Baudrate.Baudrate_115200);
            cbb_baudrate.Items.Add(Baudrate.Baudrate_19200);
            cbb_baudrate.Items.Add(Baudrate.Baudrate_38400);
            cbb_baudrate.Items.Add(Baudrate.Baudrate_4800);
            cbb_baudrate.Items.Add(Baudrate.Baudrate_57600);
            cbb_baudrate.Items.Add(Baudrate.Baudrate_9600);

            cbb_ModbusMode.Items.Add(ModbusMode.RS232_Master);
            cbb_ModbusMode.Items.Add(ModbusMode.RS232_Slave);
            cbb_ModbusMode.Items.Add(ModbusMode.RS485_Master);
            cbb_ModbusMode.Items.Add(ModbusMode.RS485_Slave);

            cbb_Protocol.Items.Add(Protocol.MODBUS_ASCII_7_E_1);
            cbb_Protocol.Items.Add(Protocol.MODBUS_ASCII_7_N_2);
            cbb_Protocol.Items.Add(Protocol.MODBUS_ASCII_7_O_1);
            cbb_Protocol.Items.Add(Protocol.MODBUS_ASCII_8_E_1);
            cbb_Protocol.Items.Add(Protocol.MODBUS_ASCII_8_N_2);
            cbb_Protocol.Items.Add(Protocol.MODBUS_ASCII_8_O_1);
            cbb_Protocol.Items.Add(Protocol.MODBUS_RTU_8_E_1);
            cbb_Protocol.Items.Add(Protocol.MODBUS_RTU_8_N_2);
            cbb_Protocol.Items.Add(Protocol.MODBUS_RTU_8_O_1);

           
        }
         
        private void button1_Click(object sender, EventArgs e)
        {
            string SIp = textBox1.Text;
            //  d.ConnectRobot(SIp);  //Ms IP 
            d.ConnectRobot(SIp, "192.168.1.30", 11000);  //Ms IP ,Computer IP , Computer Port
            label43.Text = d.RobotType().ToString();
            RobotTypeCheck();
            dataupdate += delegate ()
            {
                if (d.ServoState())
                {
                    button_All_ServoOnOff.BackColor = Color.GreenYellow;
                }
                else
                {
                    button_All_ServoOnOff.BackColor = Color.Orange;
                }
                 
            };
 
                dataupdate += delegate ()
             { 

                 if (d.HasAlarm())
                 {
                     button_Monitor_Alarm.Enabled = true;
                     button_Monitor_Reset.Enabled = true;
                     timer_Monitor_Alarm.Enabled = true;
                     timer_Monitor_Warn.Enabled = false;
                     button_Monitor_Alarm.BackColor = Color.Red;
                     groupBox4.Visible = true;
                 }
                 else
                 {

                     if (d.HasWarning())
                     {
                         button_Monitor_Alarm.Enabled = true;
                         button_Monitor_Reset.Enabled = false;
                         timer_Monitor_Alarm.Enabled = false;
                         timer_Monitor_Warn.Enabled = true;
                         button_Monitor_Alarm.BackColor = Color.Yellow;
                         groupBox4.Visible = true;
                     }
                     else
                     {
                         button_Monitor_Alarm.Enabled = false;
                         button_Monitor_Reset.Enabled = false;
                         timer_Monitor_Alarm.Enabled = false;
                         timer_Monitor_Warn.Enabled = false;
                         button_Monitor_Alarm.BackColor = Color.Transparent;
                         groupBox4.Visible = false;
                     }
                 }
             };

            dataupdate += delegate ()
            {
                if (d.GetInputState(0))
                    btn_DI1.BackColor = Color.Green;
                else
                    btn_DI1.BackColor = Color.Transparent;
            };

            dataupdate += delegate ()
            {
                if (d.GetOutputState(0))
                    btn_DO1.BackColor = Color.Green;
                else
                    btn_DO1.BackColor = Color.Transparent;

            };



            dataupdate += delegate ()
            {
                if (d.IsConnected() == false && connect == true)
                {
                    connect = false;
                    MessageBox.Show("�_�u");
                }
                pos = d.GetPos();
                if (pos[eAxisName.X] == 0)
                {
                    //    MessageBox.Show("error");
                }
                textBox2.Text = pos[eAxisName.J1].ToString();
                textBox3.Text = pos[eAxisName.J2].ToString();
                textBox4.Text = pos[eAxisName.J3].ToString();
                textBox5.Text = pos[eAxisName.J4].ToString();
                textBox27.Text = pos[eAxisName.J5].ToString();
                textBox28.Text = pos[eAxisName.J6].ToString();
                textBox6.Text = (pos[eAxisName.X] / 1000).ToString("0.000");
                textBox7.Text = (pos[eAxisName.Y] / 1000).ToString("0.000");
                textBox8.Text = (pos[eAxisName.Z] / 1000).ToString("0.000");
                textBox9.Text = (pos[eAxisName.RX] / 1000).ToString("0.000");
                textBox30.Text = (pos[eAxisName.RZ] / 1000).ToString("0.000");
                textBox29.Text = (pos[eAxisName.RY] / 1000).ToString("0.000");
                textBox26.Text = pos.Info.Hand.ToString();
                textBox31.Text = pos.Info.Hand.ToString();
                textBox32.Text = pos.Info.Shoulder.ToString();
                textBox33.Text = pos.Info.Flip.ToString();
                ExternaPuuBox.Text = d.ExternalPUU(1).ToString();
     
                if (!running)
                {
                    listBox5.Items.Add(modbusValue);
                    running = true;
                }
            };


            this.timer_dataupdate.Enabled = true;
            d.SetSpeed(20);
            d.SetJointDistance(1000);
            d.SetCartesianDistance(1000);

            LoadSystemPointFile();

        }
        delegate void update();
        update dataupdate;

        void LoadSystemPointFile()
        {
            int JRCMode = 0, J4JRC = 0, J5JRC = 0, J6JRC = 0;
            List<cPoint> PointList = d.GetGlobalPointData();
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    this.datagridview_SystemPointForm["Name1", i].Value = PointList[i].Name;
                    this.datagridview_SystemPointForm["X", i].Value = PointList[i][eAxisName.X] / 1000;
                    this.datagridview_SystemPointForm["Y", i].Value = PointList[i][eAxisName.Y] / 1000;
                    this.datagridview_SystemPointForm["Z", i].Value = PointList[i][eAxisName.Z] / 1000;
                    this.datagridview_SystemPointForm["RX", i].Value = PointList[i][eAxisName.RX] / 1000;
                    this.datagridview_SystemPointForm["RY", i].Value = PointList[i][eAxisName.RY] / 1000;
                    this.datagridview_SystemPointForm["RZ", i].Value = PointList[i][eAxisName.RZ] / 1000;
                    this.datagridview_SystemPointForm["Hand", i].Value = PointList[i].Info.Hand;
                    this.datagridview_SystemPointForm["Shoulder", i].Value = PointList[i].Info.Shoulder;
                    this.datagridview_SystemPointForm["Flip", i].Value = PointList[i].Info.Flip;
                    this.datagridview_SystemPointForm["UserID", i].Value = PointList[i].Info.UserFrame;
                    this.datagridview_SystemPointForm["ToolID", i].Value = PointList[i].Info.ToolFrame;
                    this.datagridview_SystemPointForm["Coordinate", i].Value = PointList[i].Info.Coordinate;
                    d.GetJointdexValue(PointList[i].Info.JointIndex, ref JRCMode, ref J4JRC, ref J5JRC, ref J6JRC);
                    this.datagridview_SystemPointForm["JRC", i].Value = JRCMode;
                    this.datagridview_SystemPointForm["J4JRC", i].Value = J4JRC;
                    this.datagridview_SystemPointForm["J5JRC", i].Value = J5JRC;
                    this.datagridview_SystemPointForm["J6JRC", i].Value = J6JRC;
                    this.datagridview_SystemPointForm.Rows[i].HeaderCell.Value = String.Format("{0}", i + 1);

                }
                catch { };
            }
        }

        void GetSystemPoint()
        {
            cPoint PointData = d.GetGlobalPoint(focus_RowIndex);
            int JRCMode = 0, J4JRC = 0, J5JRC = 0, J6JRC = 0;
            try
            {
                this.datagridview_SystemPointForm["Name1", focus_RowIndex].Value = PointData.Name;
                this.datagridview_SystemPointForm["X", focus_RowIndex].Value = PointData[eAxisName.X] / 1000;
                this.datagridview_SystemPointForm["Y", focus_RowIndex].Value = PointData[eAxisName.Y] / 1000;
                this.datagridview_SystemPointForm["Z", focus_RowIndex].Value = PointData[eAxisName.Z] / 1000;
                this.datagridview_SystemPointForm["RX", focus_RowIndex].Value = PointData[eAxisName.RX] / 1000;
                this.datagridview_SystemPointForm["RY", focus_RowIndex].Value = PointData[eAxisName.RY] / 1000;
                this.datagridview_SystemPointForm["RZ", focus_RowIndex].Value = PointData[eAxisName.RZ] / 1000;
                this.datagridview_SystemPointForm["Hand", focus_RowIndex].Value = PointData.Info.Hand;
                this.datagridview_SystemPointForm["Shoulder", focus_RowIndex].Value = PointData.Info.Shoulder;
                this.datagridview_SystemPointForm["Flip", focus_RowIndex].Value = PointData.Info.Flip;
                this.datagridview_SystemPointForm["UserID", focus_RowIndex].Value = PointData.Info.UserFrame;
                this.datagridview_SystemPointForm["ToolID", focus_RowIndex].Value = PointData.Info.ToolFrame;
                this.datagridview_SystemPointForm["Coordinate", focus_RowIndex].Value = PointData.Info.Coordinate;
                d.GetJointdexValue(PointData.Info.JointIndex, ref JRCMode, ref J4JRC, ref J5JRC, ref J6JRC);
                this.datagridview_SystemPointForm["JRC", focus_RowIndex].Value = JRCMode;
                this.datagridview_SystemPointForm["J4JRC", focus_RowIndex].Value = J4JRC;
                this.datagridview_SystemPointForm["J5JRC", focus_RowIndex].Value = J5JRC;
                this.datagridview_SystemPointForm["J6JRC", focus_RowIndex].Value = J6JRC;

            }
            catch { };
        }

        void ToolFrame_Get(int Index)
        {
            string[] data = d.GetToolFrame(Index);
            if (data != null)
            {
                this.datagridview_ToolFrame_DirectInput[0, 0].Value = Convert.ToString(Convert.ToDouble(data[0]) / 1000.0);
                this.datagridview_ToolFrame_DirectInput[1, 0].Value = Convert.ToString(Convert.ToDouble(data[1]) / 1000.0);
                this.datagridview_ToolFrame_DirectInput[2, 0].Value = Convert.ToString(Convert.ToDouble(data[2]) / 1000.0);
            }
        }


        void UserFrame_Get(int Index)
        {
            string[] data = d.GetUserFrame(Index);
            if (data != null)
            {
                this.datagridview_UserFrame_ThreePoint[0, 0].Value = Convert.ToString(Convert.ToDouble(data[0]) / 1000.0);
                this.datagridview_UserFrame_ThreePoint[1, 0].Value = Convert.ToString(Convert.ToDouble(data[1]) / 1000.0);
                this.datagridview_UserFrame_ThreePoint[2, 0].Value = Convert.ToString(Convert.ToDouble(data[2]) / 1000.0);

                this.datagridview_UserFrame_ThreePoint[0, 1].Value = Convert.ToString(Convert.ToDouble(data[3]) / 1000.0);
                this.datagridview_UserFrame_ThreePoint[1, 1].Value = Convert.ToString(Convert.ToDouble(data[4]) / 1000.0);
                this.datagridview_UserFrame_ThreePoint[2, 1].Value = Convert.ToString(Convert.ToDouble(data[5]) / 1000.0);

                this.datagridview_UserFrame_ThreePoint[0, 2].Value = Convert.ToString(Convert.ToDouble(data[6]) / 1000.0);
                this.datagridview_UserFrame_ThreePoint[1, 2].Value = Convert.ToString(Convert.ToDouble(data[7]) / 1000.0);
                this.datagridview_UserFrame_ThreePoint[2, 2].Value = Convert.ToString(Convert.ToDouble(data[8]) / 1000.0);

                if (data[9] == "Orthogonal")
                    comboBox_UserFrame_OrthogonalType.SelectedIndex = 0;
                else
                    comboBox_UserFrame_OrthogonalType.SelectedIndex = 1;

                if (data[10] == "Inclined")
                    comboBox_UserFrame_StandardType.SelectedIndex = 0;
                else
                    comboBox_UserFrame_StandardType.SelectedIndex = 1;
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            this.timer_dataupdate.Enabled = false;
            timer_Monitor_Alarm.Enabled = false;
            d.DisConnectRobot();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            d.ServoOn();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            d.ServoOff();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region UserFrame Initial
            this.datagridview_UserFrame_ThreePoint.Rows.Clear();
            this.datagridview_UserFrame_ThreePoint.Rows.Add("0", "0", "0");
            this.datagridview_UserFrame_ThreePoint.Rows[0].HeaderCell.Value = "Original";
            this.datagridview_UserFrame_ThreePoint.Rows.Add("0", "0", "0");
            this.datagridview_UserFrame_ThreePoint.Rows[1].HeaderCell.Value = "+ Xaxis";
            this.datagridview_UserFrame_ThreePoint.Rows.Add("0", "0", "0");
            this.datagridview_UserFrame_ThreePoint.Rows[2].HeaderCell.Value = "+ Yaxis";
            #endregion

            #region ToolFrame Initial
            this.datagridview_ToolFrame_DirectInput.Rows.Clear();
            this.datagridview_ToolFrame_DirectInput.Rows.Add("0", "0", "0");
            this.datagridview_ToolFrame_DirectInput.ForeColor = Color.Black;
            #endregion

            #region GlobalPoint Initial
            this.datagridview_SystemPointForm.Rows.Clear();
            for (int i = 0; i < 1000; i++)
            {
                this.datagridview_SystemPointForm.Rows.Add("GL", (float)0 / 1000, (float)0 / 1000, (float)0 / 1000, (float)0 / 1000, (float)0 / 1000, (float)0 / 1000, 0, 0, 0, 0, 0);
            }
            this.datagridview_SystemPointForm.ForeColor = Color.Black;
            #endregion

            #region --Initial WorkSpaceUI--
            this.dataGridView_Cylinder.Rows.Clear();
            this.dataGridView_Cylinder.Rows.Add("0", "0", "0", "0", "0", "0");
            this.dataGridView_Cylinder.Rows[0].HeaderCell.Value = "Center";
            this.dataGridView_Cylinder.ForeColor = Color.Black;

            this.dataGridView_Rectangle.Rows.Clear();
            this.dataGridView_Rectangle.Rows.Add("0", "0", "0", "0");
            this.dataGridView_Rectangle.Rows[0].HeaderCell.Value = "PO";
            this.dataGridView_Rectangle.Rows.Add("0", "0", "0", "0");
            this.dataGridView_Rectangle.Rows[1].HeaderCell.Value = "PX";
            this.dataGridView_Rectangle.Rows.Add("0", "0", "0", "0");
            this.dataGridView_Rectangle.Rows[2].HeaderCell.Value = "PY";
            this.dataGridView_Rectangle.ForeColor = Color.Black;

            #endregion
        }

        private void button_ToolFrame_DownLoadToController_Click(object sender, EventArgs e)
        {
            DialogResult myResult = MessageBox.Show("Set Tool Frame?", "[Tool Frame]", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (myResult != DialogResult.Yes) return;

            int index = Convert.ToInt32(this.combobox_ToolFrame_Number.Text);

            if (this.datagridview_ToolFrame_DirectInput[0, 0].Value.ToString() == "") this.datagridview_ToolFrame_DirectInput[0, 0].Value = "0";
            if (this.datagridview_ToolFrame_DirectInput[1, 0].Value.ToString() == "") this.datagridview_ToolFrame_DirectInput[1, 0].Value = "0";
            if (this.datagridview_ToolFrame_DirectInput[2, 0].Value.ToString() == "") this.datagridview_ToolFrame_DirectInput[2, 0].Value = "0";

            try
            {
                d.SetToolFrame(index, Convert.ToDouble(this.datagridview_ToolFrame_DirectInput[1, 0].Value) * 1000, Convert.ToDouble(this.datagridview_ToolFrame_DirectInput[0, 0].Value) * 1000, Convert.ToDouble(this.datagridview_ToolFrame_DirectInput[2, 0].Value) * 1000);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Input string was not in a correct format."))
                    MessageBox.Show("Input value is not correct!", "[Tool Frame] Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show(ex.Message, "[Tool Frame] Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }
            MessageBox.Show("Set Finished!", "Tool Frame", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button_ToolFrame_UpLoadFromController_Click(object sender, EventArgs e)
        {
            int index = Convert.ToInt32(this.combobox_ToolFrame_Number.Text);
            ToolFrame_Get(index);
        }

        private void button_UserFrame_UpLoadFromController_Click(object sender, EventArgs e)
        {
            int index = Convert.ToInt32(this.combobox_UserFrame_Number.Text);
            this.UserFrame_Get(index);
        }

        List<cFileStruct> Projects = new List<cFileStruct>();

        private void button9_Click(object sender, EventArgs e)
        {
            string[] _ProjectsExecuteMS;

            _ProjectsExecuteMS = d.ProjectList();
            string _strProjectName = string.Empty;

            if (InputBox_Combobox1("Execute project from controller.", "Project Name? ", _ProjectsExecuteMS, ref _strProjectName, validation) == DialogResult.OK)
            {
                int _ProjectIndex = Convert.ToInt32(_strProjectName.Substring(0, 4));
                d.SelectProgram(_ProjectIndex);
                this.label_MSExecute_ProjectName.Text = _strProjectName;
            }
        }

        public DialogResult InputBox_Combobox1(string title, string promptText, string[] source, ref string value, InputBoxValidation validation)
        {
            Form form = new Form();
            Label label = new Label();
            ComboBox comboBox = new ComboBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            for (int i = 0; i < source.Length; i++)
            {
                comboBox.Items.Add(source[i]);
            }
            if (comboBox.Items.Count != 0)
                comboBox.Text = source[0];

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 380, 15);
            comboBox.SetBounds(12, 50, 372, 20);
            buttonOk.SetBounds(210, 90, 75, 35);
            buttonCancel.SetBounds(310, 90, 75, 35);

            Font font = new Font("Calibri", 10, FontStyle.Regular);
            form.Font = font;
            label.Font = font;
            comboBox.Font = font;
            buttonOk.Font = font;
            buttonCancel.Font = font;

            label.AutoSize = true;
            comboBox.Anchor = comboBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(400, 140);
            form.Controls.AddRange(new System.Windows.Forms.Control[] { label, comboBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(400, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.TopMost = true; //���������n�̤W�h
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            if (validation != null)
            {
                form.FormClosing += delegate (object sender, FormClosingEventArgs e)
                {
                    if (form.DialogResult == DialogResult.OK)
                    {
                        string errorText = validation(comboBox.Text, 3);
                        if (e.Cancel = (errorText != ""))
                        {
                            MessageBox.Show(errorText, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            comboBox.Focus();
                        }
                    }
                };
            }
            DialogResult dialogResult = form.ShowDialog();
            value = comboBox.Text;
            return dialogResult;
        }
        public DialogResult InputBox_Textbox(ref int value, InputBoxValidation validation)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox text = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = "Controller Project ID";
            label.Text = "New Number (1~999):";

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 380, 15);
            text.SetBounds(12, 50, 372, 20);
            buttonOk.SetBounds(210, 90, 75, 35);
            buttonCancel.SetBounds(310, 90, 75, 35);

            Font font = new Font("Calibri", 10, FontStyle.Regular);
            form.Font = font;
            label.Font = font;
            text.Font = font;
            buttonOk.Font = font;
            buttonCancel.Font = font;

            label.AutoSize = true;
            text.Anchor = text.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(400, 140);
            form.Controls.AddRange(new System.Windows.Forms.Control[] { label, text, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(400, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.TopMost = true; //���������n�̤W�h
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            if (validation != null)
            {
                form.FormClosing += delegate (object sender, FormClosingEventArgs e)
                {
                    if (form.DialogResult == DialogResult.OK)
                    {
                        string errorText = validation(text.Text, 3);
                        if (e.Cancel = (errorText != ""))
                        {
                            MessageBox.Show(errorText, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            text.Focus();
                        }
                    }
                };
            }
            DialogResult dialogResult = form.ShowDialog();

            if (dialogResult == DialogResult.Cancel)
                value = 1;
            else
                value = Convert.ToInt32(text.Text);
            return dialogResult;
        }

        public delegate string InputBoxValidation(string errorMessage, int Type);

        InputBoxValidation validation = delegate (string val, int Type)
        {
            if (val == "")
                return "Value cannot be empty.";

            switch (Type)
            {
                case 1:
                    char[] tmp = val.ToArray();
                    foreach (char element in tmp)
                    {
                        if (!(new Regex(@"[^\W_]+")).IsMatch(Convert.ToString(element)))
                            return "Value is not valid.";
                    }
                    break;
                case 2:
                    if (!(new Regex(@"^[\w\-. _]+$")).IsMatch(val))
                        return "Value is not valid.";
                    break;

                case 4:
                    //�u��t�Ʀr [�i�H��0�A�Ĥ@�ӼƦr���ର0�A�Ʀr���i�H��0]
                    if (!(new Regex(@"^(0|[0-9])*$")).IsMatch(val))
                    {
                        return "Value is not valid.";
                    }
                    break;
            }
            return "";
        };

        private void button_Monitor_Reset_Click(object sender, EventArgs e)
        {
            if (d != null)
            {
                d.ResetAlarm();
                listBox2.Items.Clear();
            }
        }

        private void timer_Monitor_Alarm_Tick(object sender, EventArgs e)
        {
            string[,] AL = { { "" } };
            if (radioButton9.Checked == true)
                AL = d.GetAlarmCodes(eLanguage.EN, eCertification.CE);
            else
                AL = d.GetAlarmCodes(eLanguage.TW, eCertification.CE);

            listBox2.Items.Clear();
            for (int i = 0; i < (AL.Length) / 3; i++)
            {
                listBox2.Items.Add(AL[i, 0]);
                listBox2.Items.Add(AL[i, 1]);
                listBox2.Items.Add(AL[i, 2]);
            }
        }

        private void timer_Monitor_Warn_Tick(object sender, EventArgs e)
        {
            string[,] WA = { { "" } };
            if (radioButton9.Checked == true)
                WA = d.GetWarnCodes(eLanguage.EN);
            else
                WA = d.GetWarnCodes(eLanguage.TW);

            listBox2.Items.Clear();
            for (int i = 0; i < (WA.Length) / 3; i++)
            {
                listBox2.Items.Add(WA[i, 0]);
                listBox2.Items.Add(WA[i, 1]);
                listBox2.Items.Add(WA[i, 2]);
            }
        }

        private void timer_dataupdate_Tick(object sender, EventArgs e)
        {
            dataupdate();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            d.RunProgram();
        }
        private void button24_Click(object sender, EventArgs e)
        {
            d.PauseProgram();
        }
        private void button23_Click(object sender, EventArgs e)
        {
            d.StopProgram();
        }
        bool StepRun = true;

        private void btn_DI1_Click(object sender, EventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J1N);
            else
                d.Jog(eDirection.J1N);
        }

        private void btn_DO1_Click(object sender, EventArgs e)
        {
            bool outio = d.GetOutputState(0);
            outio = !outio;

            d.SetOutputState(1, outio);
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            StepRun = true;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            StepRun = false;
        }

        private void button6_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J1P);
            else
                d.Jog(eDirection.J1P);

        }

        private void button6_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.J1);
        }

        private void button7_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J1N);
            else
                d.Jog(eDirection.J1N);




        }

        private void button8_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J2P);
            else
                d.Jog(eDirection.J2P);
        }

        private void button8_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.J2);
        }

        private void button10_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J2N);
            else
                d.Jog(eDirection.J2N);
        }

        private void button11_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J3P);
            else
                d.Jog(eDirection.J3P);
        }

        private void button11_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.J3);
        }

        private void button12_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J3N);
            else
                d.Jog(eDirection.J3N);
        }

        private void button21_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J4P);
            else
                d.Jog(eDirection.J4P);
        }

        private void button21_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.J4);
        }

        private void button22_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J4N);
            else
                d.Jog(eDirection.J4N);
        }

        private void button13_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.XP);
            else
                d.Jog(eDirection.XP);
        }

        private void button13_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.X);
        }

        private void button14_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.XN);
            else
                d.Jog(eDirection.XN);
        }

        private void button15_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.YP);
            else
                d.Jog(eDirection.YP);
        }

        private void button15_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.Y);
        }

        private void button16_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.YN);
            else
                d.Jog(eDirection.YN);
        }

        private void button17_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.ZP);
            else
                d.Jog(eDirection.ZP);
        }

        private void button17_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.Z);
        }

        private void button18_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.ZN);
            else
                d.Jog(eDirection.ZN);
        }

        private void button19_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.RZP);
            else
                d.Jog(eDirection.RZP);
        }

        private void button19_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.RZ);
        }

        private void button20_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.RZN);
            else
                d.Jog(eDirection.RZN);

        }

        private void button25_MouseDown(object sender, MouseEventArgs e)
        {
            cPoint p = new cPoint();
            p = d.GetGlobalPoint(focus_RowIndex);
            d.GotoMovP(p);
        }

        private void button25_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop();
        }

        private void button26_Click(object sender, EventArgs e)
        {
            int IJRCMode = Convert.ToInt32(comboBox4.Text);
            d.TechGlobalPoint(focus_RowIndex, IJRCMode);
            GetSystemPoint();
        }

        private void datagridview_SystemPointForm_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            focus_RowIndex = this.datagridview_SystemPointForm.CurrentRow.Index;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            d.DisConnectRobot();
        }

        private void button27_Click(object sender, EventArgs e)
        {
            eModbusWordType WordType = eModbusWordType.DoubleWord;
            if (radioButton5.Checked == true)
                WordType = eModbusWordType.DoubleWord;
            else
                WordType = eModbusWordType.Word;

            UInt16 address = (UInt16)Convert.ToInt32(textBox10.Text, 16);
            string ret1 = d.WriteModbus(address, WordType, Convert.ToInt32(textBox11.Text));
        }

        private void button28_Click(object sender, EventArgs e)
        {
            eModbusWordType WordType = eModbusWordType.DoubleWord;
            if (radioButton5.Checked == true)
                WordType = eModbusWordType.DoubleWord;
            else
                WordType = eModbusWordType.Word;

            UInt16 address = (UInt16)Convert.ToInt32(textBox10.Text, 16);
            int returnvalue = 0;
            d.ReadModbus(address, WordType, ref returnvalue);
            textBox11.Text = returnvalue.ToString();

        }

        private void button29_Click(object sender, EventArgs e)
        {
            d.StartAPIMoveFunction();
            //d.StartCmd(); 
        }

        private void button34_Click(object sender, EventArgs e)
        {
            d.CloseAPIMoveFunction();
            //  d.EndCmd();
        }



        private async void button32_Click(object sender, EventArgs e)
        { 
            cPoint p = new cPoint();
            listBox1.SelectedIndex = 0;
            int count = points.Count;

            if (count >= 2)
            {
                for (int i = 1; i <= count; i++)
                {
                    listBox1.SelectedIndex = (i - 1);
                    p = d.GetGlobalPoint(points[i - 1]);

                    await Task.Run(() =>
                    {                        
                        if (i == 1)
                            d.StartContinuousMovP(p);
                        else if (i == count)
                            d.EndContinuousMovP(p);
                        else
                            d.PathP(p);
                    });
                }
            } 
        }

        private async void button33_Click(object sender, EventArgs e)
        { 
            cPoint p = new cPoint();
            listBox1.SelectedIndex = 0;
            int count = points.Count;
            if (count >= 2)
            {
                for (int i = 1; i <= count; i++)
                {
                    listBox1.SelectedIndex = (i - 1);
                    p = d.GetGlobalPoint(points[i - 1]);

                await Task.Run(() =>
                {
                    if (i == 1)
                            d.StartContinuousMovL(p);
                        else if (i == count)
                            d.EndContinuousMovL(p);
                        else
                            d.PathL(p);

                    });
                }
            }
        }

        private async void button30_MouseDown(object sender, MouseEventArgs e)
        {
            await Task.Run(() =>
            { 
                cPoint p = new cPoint();
                p = d.GetGlobalPoint(focus_RowIndex);
                string ErrorCode = d.MovP(p);   //  return 0: No Connect  1:Finish 2:No data 3:TimeOut   ExCeption: "Error Message"
               // while (!d.Ready()) ;
                System.Threading.Thread.Sleep(10);
            });
        }

        private void button30_MouseUp(object sender, MouseEventArgs e)
        {
            d.MotionStop();
        }

        private async void button31_MouseDown(object sender, MouseEventArgs e)
        {
            await Task.Run(() =>
            {
                cPoint p = new cPoint();
                p = d.GetGlobalPoint(focus_RowIndex);
                d.MovL(p);
                //while (!d.IsReady())
                System.Threading.Thread.Sleep(10);
            });
            
        }

        private void button31_MouseUp(object sender, MouseEventArgs e)
        {
            d.MotionStop();
        }

        private void button36_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            points.Clear();
        }

        private void button35_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(focus_RowIndex);
            points.Add(focus_RowIndex);
        }

        private void button37_Click(object sender, EventArgs e)
        {
            listBox1.TopIndex = 2;
        }

        private async void button37_Click_1(object sender, EventArgs e)
        {
            
                cPoint p = new cPoint();
                p = d.GetGlobalPoint(focus_RowIndex);
                int H1 = Convert.ToInt32(this.textBox13.Text);
                int H2 = Convert.ToInt32(this.textBox14.Text);
                int H3 = Convert.ToInt32(this.textBox15.Text);
            await Task.Run(() =>
            {
                d.MArchL(p, H1, H2, H3);
                //while (!d.IsReady()) ;
            });
        }

        private async void button43_Click(object sender, EventArgs e)
        {
            
                cPoint p = new cPoint();
                p = d.GetGlobalPoint(focus_RowIndex);
                int H1 = Convert.ToInt32(this.textBox13.Text);
                int H2 = Convert.ToInt32(this.textBox14.Text);
                int H3 = Convert.ToInt32(this.textBox15.Text);
                int a = 0;
            await Task.Run(() =>
            {
                d.MArchP(p, H1, H2, H3);
                //while (!d.IsReady()) ;
            });
        }

        private void button38_Click(object sender, EventArgs e)
        {
            int userFrameID = Convert.ToInt32(this.textBox18.Text);
            int toolFrameID = Convert.ToInt32(this.textBox19.Text);
            int Coordinate = Convert.ToInt32(this.textBox34.Text);
            bool[] FrameSelct = new bool[3];

            CoordinateType coordinateType = CoordinateType.User;
            switch (Coordinate)
            {
                case 0:
                    coordinateType = CoordinateType.Joint;
                    break;
                case 1:
                    coordinateType = CoordinateType.World;
                    break;
                case 2:
                    coordinateType = CoordinateType.User;
                    break;
                case 3:
                    coordinateType = CoordinateType.Tool;
                    break;
                default:
                    break;
            }
            FrameSelct = d.FrameSelect(toolFrameID, userFrameID, coordinateType);
        }

        private void button_UserFrame_DownLoadToController_Click(object sender, EventArgs e)
        {

            DialogResult myResult = MessageBox.Show("Set User Frame?", "[User Frame]", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (myResult != DialogResult.Yes) return;

            //----- �g�J���Шt�о��I���ƨöi��p�� -----//

            int index = Convert.ToInt32(this.combobox_UserFrame_Number.Text);

            //�ˬd�����

            if (this.datagridview_UserFrame_ThreePoint[0, 0].Value.ToString() == "") this.datagridview_UserFrame_ThreePoint[0, 0].Value = "0";
            if (this.datagridview_UserFrame_ThreePoint[1, 0].Value.ToString() == "") this.datagridview_UserFrame_ThreePoint[1, 0].Value = "0";
            if (this.datagridview_UserFrame_ThreePoint[2, 0].Value.ToString() == "") this.datagridview_UserFrame_ThreePoint[2, 0].Value = "0";

            if (this.datagridview_UserFrame_ThreePoint[0, 1].Value.ToString() == "") this.datagridview_UserFrame_ThreePoint[0, 1].Value = "0";
            if (this.datagridview_UserFrame_ThreePoint[1, 1].Value.ToString() == "") this.datagridview_UserFrame_ThreePoint[1, 1].Value = "0";
            if (this.datagridview_UserFrame_ThreePoint[2, 1].Value.ToString() == "") this.datagridview_UserFrame_ThreePoint[2, 1].Value = "0";

            if (this.datagridview_UserFrame_ThreePoint[0, 2].Value.ToString() == "") this.datagridview_UserFrame_ThreePoint[0, 2].Value = "0";
            if (this.datagridview_UserFrame_ThreePoint[1, 2].Value.ToString() == "") this.datagridview_UserFrame_ThreePoint[1, 2].Value = "0";
            if (this.datagridview_UserFrame_ThreePoint[2, 2].Value.ToString() == "") this.datagridview_UserFrame_ThreePoint[2, 2].Value = "0";


            //===========================================================================================================================
            bool Orthogonal = true;
            if (comboBox_UserFrame_OrthogonalType.Text == "NonOrthogonal")
                Orthogonal = false;

            eParallelSurfaces StandardType = eParallelSurfaces.Z;
            switch (comboBox_UserFrame_StandardType.Text)
            {
                case "Inclined":
                    StandardType = eParallelSurfaces.none;
                    break;
                case "NonInclined.Z":
                    StandardType = eParallelSurfaces.Z;
                    break;
            }
            cPoint p0 = new cPoint();
            cPoint px = new cPoint();
            cPoint py = new cPoint();
            try
            {
                p0[eAxisName.X] = Convert.ToDouble(this.datagridview_UserFrame_ThreePoint[0, 0].Value) * 1000;
                p0[eAxisName.Y] = Convert.ToDouble(this.datagridview_UserFrame_ThreePoint[1, 0].Value) * 1000;
                p0[eAxisName.Z] = Convert.ToDouble(this.datagridview_UserFrame_ThreePoint[2, 0].Value) * 1000;

                px[eAxisName.X] = Convert.ToDouble(this.datagridview_UserFrame_ThreePoint[0, 1].Value) * 1000;
                px[eAxisName.Y] = Convert.ToDouble(this.datagridview_UserFrame_ThreePoint[1, 1].Value) * 1000;
                px[eAxisName.Z] = Convert.ToDouble(this.datagridview_UserFrame_ThreePoint[2, 1].Value) * 1000;

                py[eAxisName.X] = Convert.ToDouble(this.datagridview_UserFrame_ThreePoint[0, 2].Value) * 1000;
                py[eAxisName.Y] = Convert.ToDouble(this.datagridview_UserFrame_ThreePoint[1, 2].Value) * 1000;
                py[eAxisName.Z] = Convert.ToDouble(this.datagridview_UserFrame_ThreePoint[2, 2].Value) * 1000;

                d.SetUserFrame(index, Orthogonal, StandardType, p0, px, py);


            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Input string was not in a correct format."))
                    MessageBox.Show("Input value is not correct!", "[User Frame] Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show(ex.Message, "[User Frame] Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }


            MessageBox.Show("Set Finished!", "User Frame", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button39_Click(object sender, EventArgs e)
        {
            d.SetSpeedEx((int)(Convert.ToDouble(textBox12.Text)));
            while (!d.IsReady())
                System.Threading.Thread.Sleep(10);
            d.SetAccelEx((int)(Convert.ToDouble(textBox16.Text)));
            while (!d.IsReady())
                System.Threading.Thread.Sleep(10);
            d.SetDecelEx((int)(Convert.ToDouble(textBox17.Text)));
            while (!d.IsReady())
                System.Threading.Thread.Sleep(10);

            switch (comboBox1.Text)
            {
                case "MAXROUGH":
                    d.SetAccurEx(eAccur.MAXROUGH);
                    break;
                case "ROUGH":
                    d.SetAccurEx(eAccur.ROUGH);
                    break;
                case "STANDARD":
                    d.SetAccurEx(eAccur.STANDARD);
                    break;
                case "MEDIUM":
                    d.SetAccurEx(eAccur.MEDIUM);
                    break;
                case "HIGH":
                    d.SetAccurEx(eAccur.HIGH);
                    break;
            }
            while (!d.IsReady())
                System.Threading.Thread.Sleep(10);

        }

        private void button45_Click(object sender, EventArgs e)
        {
            textBox12.Text = d.GetSpeedEx().ToString();
            textBox16.Text = d.GetAccelEx().ToString();
            textBox17.Text = d.GetDecelEx().ToString();

            switch (d.GetAccurEx())
            {
                case eAccur.MAXROUGH:
                    comboBox1.SelectedIndex = 0;
                    break;
                case eAccur.ROUGH:
                    comboBox1.SelectedIndex = 1;
                    break;
                case eAccur.STANDARD:
                    comboBox1.SelectedIndex = 2;
                    break;
                case eAccur.MEDIUM:
                    comboBox1.SelectedIndex = 3;
                    break;
                case eAccur.HIGH:
                    comboBox1.SelectedIndex = 4;
                    break;
            }
        }

        private void button40_Click(object sender, EventArgs e)
        {
            UInt16 address = (UInt16)Convert.ToInt32(textBox21.Text, 16);

            short[] value = new short[32];
            for (int i = 0; i < listBox6.Items.Count; i++)
                value[i] = Convert.ToInt16(listBox6.Items[i].ToString());

            d.WriteMulitModbus(address, value);
        }
        Stopwatch stopwatch = new Stopwatch();
        private void button41_Click(object sender, EventArgs e)
        {
            listBox5.Items.Clear();
            UInt16 address = (UInt16)Convert.ToInt32(textBox21.Text, 16);
            short count = Convert.ToInt16(textBox22.Text);
            short[] modbus_value = new short[count];

            stopwatch.Restart();
            modbus_value = d.ReadMulitModbus(address, count);
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);


            for (int i = 0; i < modbus_value.Length; i++)
                listBox5.Items.Add(modbus_value[i]); ;
        }


        private void button_WS_Rectangle_Set_Click(object sender, EventArgs e)
        {
            DialogResult myResult = MessageBox.Show("Set WorkSpace?", "[WorkSpace]", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (myResult != DialogResult.Yes) return;

            int index = Convert.ToInt32(this.comboBox_WorkSpace_ID.Text);
            cPoint p0 = new cPoint();
            cPoint px = new cPoint();
            cPoint py = new cPoint();
            try
            {
                p0[eAxisName.X] = Convert.ToInt32(this.dataGridView_Rectangle["Rectangle_X", 0].Value) * 1000;
                p0[eAxisName.Y] = Convert.ToInt32(this.dataGridView_Rectangle["Rectangle_Y", 0].Value) * 1000;
                p0[eAxisName.Z] = Convert.ToInt32(this.dataGridView_Rectangle["Rectangle_Z", 0].Value) * 1000;

                px[eAxisName.X] = Convert.ToInt32(this.dataGridView_Rectangle["Rectangle_X", 1].Value) * 1000;
                px[eAxisName.Y] = Convert.ToInt32(this.dataGridView_Rectangle["Rectangle_Y", 1].Value) * 1000;
                px[eAxisName.Z] = Convert.ToInt32(this.dataGridView_Rectangle["Rectangle_Z", 1].Value) * 1000;

                py[eAxisName.X] = Convert.ToInt32(this.dataGridView_Rectangle["Rectangle_X", 2].Value) * 1000;
                py[eAxisName.Y] = Convert.ToInt32(this.dataGridView_Rectangle["Rectangle_Y", 2].Value) * 1000;
                py[eAxisName.Z] = Convert.ToInt32(this.dataGridView_Rectangle["Rectangle_Z", 2].Value) * 1000;
            }
            catch
           {
                return;
            }

            eWorkSpaceAreaType Area = eWorkSpaceAreaType.Working;
            if (comboBoxWorkSpace_Area.Text == "Restricted Area")
                Area = eWorkSpaceAreaType.Restricted;
            else
                Area = eWorkSpaceAreaType.Working;

            eWorkSpaceSetType Enable_Setting = eWorkSpaceSetType.Disable;
            if (comboBox_WS_Rectangle.Text == "Enable")
                Enable_Setting = eWorkSpaceSetType.Enable;
            else
                Enable_Setting = eWorkSpaceSetType.Disable;

            d.Set_Rectangle_WS(index, Area, Enable_Setting, p0, px, py);
        }

        private void button_WS_Rectangle_Get_Click(object sender, EventArgs e)
        {
            int index = Convert.ToInt32(this.comboBox_WorkSpace_ID.Text);

            WorkSpaceRectangle_Get(index);
        }

        void WorkSpaceRectangle_Get(int index)
        {
            string[] Rectangle_WSdata;
            Rectangle_WSdata = d.Get_Rectangle_WS(index);
            if (Rectangle_WSdata != null)
            {
                this.dataGridView_Rectangle["Rectangle_X", 0].Value = Convert.ToString(Convert.ToDouble(Rectangle_WSdata[0]) / 1000.0);
                this.dataGridView_Rectangle["Rectangle_Y", 0].Value = Convert.ToString(Convert.ToDouble(Rectangle_WSdata[1]) / 1000.0);
                this.dataGridView_Rectangle["Rectangle_Z", 0].Value = Convert.ToString(Convert.ToDouble(Rectangle_WSdata[2]) / 1000.0);

                this.dataGridView_Rectangle["Rectangle_X", 1].Value = Convert.ToString(Convert.ToDouble(Rectangle_WSdata[3]) / 1000.0);
                this.dataGridView_Rectangle["Rectangle_Y", 1].Value = Convert.ToString(Convert.ToDouble(Rectangle_WSdata[4]) / 1000.0);
                this.dataGridView_Rectangle["Rectangle_Z", 1].Value = Convert.ToString(Convert.ToDouble(Rectangle_WSdata[5]) / 1000.0);

                this.dataGridView_Rectangle["Rectangle_X", 2].Value = Convert.ToString(Convert.ToDouble(Rectangle_WSdata[6]) / 1000.0);
                this.dataGridView_Rectangle["Rectangle_Y", 2].Value = Convert.ToString(Convert.ToDouble(Rectangle_WSdata[7]) / 1000.0);
                this.dataGridView_Rectangle["Rectangle_Z", 2].Value = Convert.ToString(Convert.ToDouble(Rectangle_WSdata[8]) / 1000.0);

                if (Rectangle_WSdata[9] == "Restricted")
                    comboBoxWorkSpace_Area.SelectedIndex = 0;
                else
                    comboBoxWorkSpace_Area.SelectedIndex = 1;

                if (Rectangle_WSdata[10] == "Disable")
                    comboBox_WS_Cylinder.SelectedIndex = 0;
                else
                    comboBox_WS_Cylinder.SelectedIndex = 1;
            }
        }

        private void button_WorkSpace_Switch_Click(object sender, EventArgs e)
        {
            if (button_WorkSpace_Switch.Text == "Close Work Space")
            {
                d.WorkSpace_Switch(true);
                button_WorkSpace_Switch.Text = "Open Work Space";
                button_WorkSpace_Switch.BackColor = Color.GreenYellow;
            }
            else if (button_WorkSpace_Switch.Text == "Open Work Space")
            {
                d.WorkSpace_Switch(false);
                button_WorkSpace_Switch.Text = "Close Work Space";
                button_WorkSpace_Switch.BackColor = Color.Orange;
            }
        }

        private void button_WS_Cylinder_Set_Click(object sender, EventArgs e)
        {
            DialogResult myResult = MessageBox.Show("Set WorkSpace?", "[WorkSpace]", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (myResult != DialogResult.Yes) return;

            int index = Convert.ToInt32(this.comboBox_WorkSpace_ID.Text);
            cPoint p0 = new cPoint();
            double radius = 0, height = 0;
            try
            {
                p0[eAxisName.X] = Convert.ToInt32(this.dataGridView_Cylinder[0, 0].Value) * 1000;
                p0[eAxisName.Y] = Convert.ToInt32(this.dataGridView_Cylinder[1, 0].Value) * 1000;
                p0[eAxisName.Z] = Convert.ToInt32(this.dataGridView_Cylinder[2, 0].Value) * 1000;

                radius = (double)Convert.ToDouble(this.dataGridView_Cylinder[3, 0].Value) * 1000;

                height = (double)Convert.ToDouble(this.dataGridView_Cylinder[4, 0].Value) * 1000;
            }
            catch
            {
                return;
            }

            eWorkSpaceAreaType Area = eWorkSpaceAreaType.Working;
            if (comboBoxWorkSpace_Area.Text == "Restricted Area")
                Area = eWorkSpaceAreaType.Restricted;
            else
                Area = eWorkSpaceAreaType.Working;

            eWorkSpaceSetType Enable_Setting = eWorkSpaceSetType.Disable;
            if (comboBox_WS_Cylinder.Text == "Enable")
                Enable_Setting = eWorkSpaceSetType.Enable;
            else
                Enable_Setting = eWorkSpaceSetType.Disable;

            d.Set_Cylinder_WS(index, Area, Enable_Setting, p0, radius, height);
        }

        private void button_WS_Cylinder_Get_Click(object sender, EventArgs e)
        {
            int index = Convert.ToInt32(this.comboBox_WorkSpace_ID.Text);
            WorkSpaceCylinder_Get(index);
        }

        void WorkSpaceCylinder_Get(int index)
        {
            string[] Cylinder_WSdata;
            Cylinder_WSdata = d.Get_Cylinder_WS(index);
            if (Cylinder_WSdata != null)
            {
                this.dataGridView_Cylinder[0, 0].Value = Convert.ToString(Convert.ToDouble(Cylinder_WSdata[0]) / 1000.0);
                this.dataGridView_Cylinder[1, 0].Value = Convert.ToString(Convert.ToDouble(Cylinder_WSdata[1]) / 1000.0);
                this.dataGridView_Cylinder[2, 0].Value = Convert.ToString(Convert.ToDouble(Cylinder_WSdata[2]) / 1000.0);
                this.dataGridView_Cylinder[3, 0].Value = Convert.ToString(Convert.ToDouble(Cylinder_WSdata[3]) / 1000.0);
                this.dataGridView_Cylinder[4, 0].Value = Convert.ToString(Convert.ToDouble(Cylinder_WSdata[4]) / 1000.0);

                if (Cylinder_WSdata[5] == "Restricted")
                    comboBoxWorkSpace_Area.SelectedIndex = 0;
                else
                    comboBoxWorkSpace_Area.SelectedIndex = 1;

                if (Cylinder_WSdata[6] == "Disable")
                    comboBox_WS_Cylinder.SelectedIndex = 0;
                else
                    comboBox_WS_Cylinder.SelectedIndex = 1;
            }
        }

        private void comboBoxWorkSpace_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = Convert.ToInt32(this.comboBox_WorkSpace_ID.Text) - 1;

            if (comboBoxWorkSpace_Type.Text == "Cylinder")
            {
                groupBox_Cylinder.Visible = true;
                groupBox_Rectangle.Visible = false;
                WorkSpaceCylinder_Get(index);
            }
            else
            {
                groupBox_Cylinder.Visible = false;
                groupBox_Rectangle.Visible = true;
                WorkSpaceRectangle_Get(index + 10);
            }
        }

        private void textBox20_TextChanged(object sender, EventArgs e)
        {
            try
            {
                d.SetSpeed(Convert.ToInt32(textBox20.Text));
                System.Threading.Thread.Sleep(100);
                uint ret = d.GetSpeed();
            }
            catch { }
        }

        private void textBox23_TextChanged(object sender, EventArgs e)
        {
            d.SetJointDistance(Convert.ToInt32(textBox23.Text));
            System.Threading.Thread.Sleep(100);
        }

        private void textBox24_TextChanged(object sender, EventArgs e)
        {
            d.SetCartesianDistance(Convert.ToInt32(textBox24.Text));
            System.Threading.Thread.Sleep(100);
        }

        private void button42_Click_1(object sender, EventArgs e)
        {
            switch (comboBox2.Text)
            {
                case "RLState":
                    label44.Text = "RLState:" + d.ExecutorState().ToString();
                    break;
                case "FunctionalPauseStatus":
                    label44.Text = "FunctionalPauseStatus:" + d.FunctionalPauseStatus().ToString();
                    break;
                case "TPStatus":
                    label44.Text = "TPStatus:" + d.TPStatus().ToString();
                    break;
                case "OperatingStatus":
                    label44.Text = "OperatingStatus:" + d.OperatingStatus().ToString();
                    break;
                case "TemperatureStatus":
                    label44.Text = "TemperatureStatus:" + d.TemperatureStatus().ToString();
                    break;
                case "RobotMovingStatus":
                    label44.Text = "RobotMovingStatus:" + d.RobotMovingStatus().ToString();
                    break;
                case "APIVersion":
                    label44.Text = d.APIVersion().ToString();
                    break;
                case "API_Motion_Status":
                    label44.Text = "Now Status: " + d.API_MoveFuction_Status(); 
                    break;
                case "GetFirmwareVersion":
                    label44.Text = "Firmware Version: " + d.GetFirmwareVersion();
                    break;
                case "GetMACAddress":
                    label44.Text = "GetMACAddress: " + d.GetMACAddress();
                    break;
                case "GetRobotName":
                    label44.Text = "GetRobotName: " + d.GetRobotName();
                    break;
                case "GetSerialPortParameter":
                    short address = 0;
                    Baudrate baudrate = Baudrate.Baudrate_9600;
                    Protocol protocol = Protocol.MODBUS_ASCII_7_E_1; 
                    ModbusMode mode = ModbusMode.RS232_Master;

                    d.GetSerialPortParameter(ref address, ref baudrate, ref protocol, ref mode);
                    MessageBox.Show("GetSerialPortParameter: address = " + address + ", baudrate : " + baudrate +
                        ", protocol : " + protocol + ", mode : " + mode);
                    label44.Text = "";
                    break;
                case "MonitorAlarmCode":
                    label44.Text = "Alarm Code : " + d.MonitorAlarmCode().ToString();
                    break;
                case "APIRLProjectVersion":
                    label44.Text = "APIRLProjectVersion : " + d.APIRLProjectVersion().ToString();
                    break;
                    // label44.Text = d.RobotMovingStatus().ToString();
            }
        }

        private async void button44_MouseDown(object sender, MouseEventArgs e)
        {
            double inputPuu = Convert.ToDouble(textBox25.Text);

            await Task.Run(() =>
            {
                if (radioButton3.Checked == true)
                    d.MovJ(eAxisName.J1, inputPuu);
                else if (radioButton4.Checked == true)
                    d.MovJ(eAxisName.J2, inputPuu);
                else if (radioButton7.Checked == true)
                    d.MovJ(eAxisName.J3, inputPuu);
                else if (radioButton8.Checked == true)
                    d.MovJ(eAxisName.J4, inputPuu);
                else if (radioButton11.Checked == true)
                    d.MovJ(eAxisName.J5, inputPuu);
                else if (radioButton12.Checked == true)
                    d.MovJ(eAxisName.J6, inputPuu); 
            });
        }

        private void button44_MouseUp(object sender, MouseEventArgs e)
        {
            d.MotionStop();
        }
        private void button46_Click(object sender, EventArgs e)
        {
            //string path=System.Windows.Forms.Application.StartupPath+"\\"+"ProjectName";

            //string path = "C:\\xxx\\xxx\\xxx\\ProjectName";

            FolderBrowserDialog FolderBrowserDialog1 = new FolderBrowserDialog();
            FolderBrowserDialog1.ShowDialog();
            string path = FolderBrowserDialog1.SelectedPath;
            int Project_ID = 1;

            if (InputBox_Textbox(ref Project_ID, validation) == DialogResult.OK)
                d.ProjectUploadToController(Project_ID, path);
        }

        private void button47_Click(object sender, EventArgs e)
        {
            string[] _ProjectsExecuteMS;

            _ProjectsExecuteMS = d.ProjectList();
            string _strProjectName = string.Empty;

            if (InputBox_Combobox1("Download project from controller.", "Project Name? ", _ProjectsExecuteMS, ref _strProjectName, validation) == DialogResult.OK)
            {
                FolderBrowserDialog FolderBrowserDialog1 = new FolderBrowserDialog();
                FolderBrowserDialog1.ShowDialog();
                string path = FolderBrowserDialog1.SelectedPath;

                d.ProjectDownloadToLocal(_strProjectName, path);
            }
        }

        private void button48_Click(object sender, EventArgs e)
        {
            string[] _ProjectsExecuteMS;

            _ProjectsExecuteMS = d.ProjectList();
            string _strProjectName = string.Empty;

            if (InputBox_Combobox1("Delete controller project.", "Project Name? ", _ProjectsExecuteMS, ref _strProjectName, validation) == DialogResult.OK)
            {
                d.DeleteControllerProjec(_strProjectName);
            }
        }

        private void btn_RDO1_Click(object sender, EventArgs e)
        {
            bool outio = d.GetExtOutputState(1, 1);
            outio = !outio;

            d.SetExtOutputState(1, 1, outio);

        }

        private void button54_Click(object sender, EventArgs e)
        {
            string ProjectPath = System.Windows.Forms.Application.StartupPath;
            string ProjectName = "";
            System.IO.StreamWriter ProjectLog;

            string _ProjectName = "main.lua";
            ProjectName = ProjectPath + "\\" + _ProjectName;
            ProjectLog = new System.IO.StreamWriter(ProjectName);
            for (int i = 1; i <= 5000; i++)
                ProjectLog.WriteLine("local a" + i + " = " + i);
            ProjectLog.Close();

        }

        private void button55_Click(object sender, EventArgs e)
        {
            cPoint p = new cPoint();

            p.Name = "123";
            p[eAxisName.X] = 527000;
            p[eAxisName.Y] = -112000;
            p[eAxisName.Z] = 775000;
            p[eAxisName.RX] = -180000;
            p[eAxisName.RY] = -76000;
            p[eAxisName.RZ] = 6600;
            p.Info.Hand = 1;
            p.Info.Shoulder = 0;
            p.Info.Flip = 0;
            p.Info.UserFrame = 1;
            p.Info.ToolFrame = 2;
            p.Info.Coordinate = 1; // 1=user    0=�j�a�y��
            int JRC_Mode = 2;
            int J4_JRC = 4;
            int J5_JRC = 3;
            int J6_JRC = 2;
            p.Info.JointIndex = d.SetJointIndexValue(JRC_Mode, J4_JRC, J5_JRC, J6_JRC);  //JRC Mode ,J4 JRC, J6 JRC  Or Set (4,0,0) 
            d.SetGlobalPoint(focus_RowIndex, p);
            GetSystemPoint();
        }

        private void btn_RDI1_Click(object sender, EventArgs e)
        {
            if (d.GetExtInputState(4, 1))
                btn_RDI1.BackColor = Color.Green;
            else
                btn_RDI1.BackColor = Color.Transparent;
        }

        private void button56_Click(object sender, EventArgs e)
        {
            d.ShutdownRobot();
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void button49_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J5P);
            else
                d.Jog(eDirection.J5P);
        }

        private void button49_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.J5);
        }

        private void button50_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J5N);
            else
                d.Jog(eDirection.J5N);
        }

        private void button50_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.J5);
        }

        private void button51_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J6P);
            else
                d.Jog(eDirection.J6P);
        }

        private void button51_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.J6);
        }

        private void button52_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.J6N);
            else
                d.Jog(eDirection.J6N);
        }

        private void button52_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.J6);
        }

        private void button53_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.RXP);
            else
                d.Jog(eDirection.RXP);
        }

        private void button53_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.RX);
        }

        private void button54_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.RXN);
            else
                d.Jog(eDirection.RXN);
        }

        private void button54_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.RX);
        }

        private void button57_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.RYP);
            else
                d.Jog(eDirection.RYP);
        }

        private void button57_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.RY);
        }
        System.Threading.Thread Forlistening = null;
        int aaa = 0;
        Stopwatch stopWatch = new Stopwatch();
        private void button59_Click(object sender, EventArgs e)
        {

            
        }

        private void RobotTypeCheck()
        {
            if (d.RobotType().ToString() == "Scara")
            {

                this.textBox27.Visible = false;
                this.textBox28.Visible = false;
                this.textBox31.Visible = false;
                this.textBox32.Visible = false;
                this.textBox33.Visible = false;
                this.button49.Visible = false;
                this.button50.Visible = false;
                this.button51.Visible = false;
                this.button52.Visible = false;
                this.button53.Visible = false;
                this.button54.Visible = false;
                this.button57.Visible = false;
                this.button58.Visible = false;
                this.radioButton11.Visible = false;
                this.radioButton12.Visible = false;
                this.label36.Visible = false;
                this.label41.Visible = false;
                this.label42.Visible = false;
                this.label37.Visible = false;
                this.label38.Visible = false;
                this.textBox30.Location = new System.Drawing.Point(1019, 269);
                this.label8.Visible = false;
                this.label40.Visible = false;
                this.textBox9.Visible = false;
                this.textBox29.Visible = false;
                this.textBox30.Location = new System.Drawing.Point(1019, 269);
                this.label39.Location = new System.Drawing.Point(987, 275);
                this.button19.Location = new System.Drawing.Point(666, 504);
                this.button20.Location = new System.Drawing.Point(741, 504);
                this.textBox26.Location = new System.Drawing.Point(882, 299);
                this.label31.Location = new System.Drawing.Point(846, 305);
            }
            else if (d.RobotType().ToString() == "ScaraFive")
            {
                this.textBox30.Location = new System.Drawing.Point(1019, 328);
                this.textBox27.Visible = true;
                this.textBox28.Visible = true;
                this.textBox31.Visible = true;
                this.textBox32.Visible = true;
                this.textBox33.Visible = true;
                this.button49.Visible = true;
                this.button50.Visible = true;
                this.button51.Visible = false;
                this.button52.Visible = false;
                this.button53.Visible = false;
                this.button54.Visible = false;
                this.button57.Visible = true;
                this.button58.Visible = true;
                this.radioButton11.Visible = true;
                this.radioButton12.Visible = true;
                this.label36.Visible = true;
                this.label41.Visible = true;
                this.label42.Visible = true;
                this.label37.Visible = true;
                this.label38.Visible = true;
                this.label8.Visible = true;
                this.label40.Visible = true;
                this.textBox9.Visible = true;
                this.textBox29.Visible = true;

            }

        }

        private void button60_Click(object sender, EventArgs e)
        {
            d.GoHome();

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox3.Text)
            {
                case "BUFFER":
                    d.SetBufferModeEx(eBuffermode.BUFFER);
                    break;
                case "ABORT":
                    d.SetBufferModeEx(eBuffermode.ABORT);
                    break;

            }
        }

        private void button58_MouseDown(object sender, MouseEventArgs e)
        {
            if (StepRun)
                d.Step(eDirection.RYN);
            else
                d.Jog(eDirection.RYN);
        }

        private void button58_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop(eAxisName.RY);
        }

        private void button30_Click(object sender, EventArgs e)
        {

        }

        private void button31_Click(object sender, EventArgs e)
        {

        }

        private void button61_Click(object sender, EventArgs e)
        {

        }
        public static string data = null;
        public static bool Isready;

        private void label31_Click(object sender, EventArgs e)
        {

        }

        private void button25_Click(object sender, EventArgs e)
        {

        }


        private void textBox34_TextChanged(object sender, EventArgs e)
        {

        }

        private void ExternalServoOn_Click(object sender, EventArgs e)
        {
            d.ExternalServoOn(1);
        }

        private void ExternalServoOff_Click(object sender, EventArgs e)
        {
            d.ExternalServoOff(1);
        }

        private void ExternalFoward_MouseDown(object sender, MouseEventArgs e)
        {
            d.ExternalJogForward(1);
        }

        private void ExternalFoward_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop();
        }

        private void ExternalReverse_MouseDown(object sender, MouseEventArgs e)
        {
            d.ExternalJogReverse(1);
        }

        private void ExternalReverse_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop();
        }

        private void btn_DynamicBrakeOn_Click_(object sender, EventArgs e)
        {
            d.DynamicbrakeOff(true);
        }

        private void btn_DynamicBrakeOff_Click_(object sender, EventArgs e)
        {
            d.DynamicbrakeOff(false);
        }

        private async void button63_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                cPoint p = new cPoint();
                cPoint p2 = new cPoint();
                p = d.GetGlobalPoint(focus_RowIndex);
                p2 = d.GetGlobalPoint(focus_RowIndex + 1);
                while (true)
                {
                    d.MCircle(p, p2);
                    //while (!d.IsReady()) ;
                }
            });
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button64_Click(object sender, EventArgs e)
        {
            String ret = d.MutipleMovJ(Convert.ToDouble(J1Degree.Text), Convert.ToDouble(J2Degree.Text), Convert.ToDouble(J3Degree.Text), Convert.ToDouble(J4Degree.Text), Convert.ToDouble(J5Degree.Text), Convert.ToDouble(J6Degree.Text));
            Console.WriteLine(ret);
           
        }
        int extID = 1;
        private async void btn_ExtMovJ_MouseDown(object sender, MouseEventArgs e)
        {
            int.TryParse(ExternalIdComboBox.Text, out extID);
            int spd = 1;
            int acc = 1;
            int dec = 1;
            int.TryParse(textBox12.Text, out spd);
            int.TryParse(textBox16.Text, out acc);
            int.TryParse(textBox17.Text, out dec);
            int puuTmp = 0;

            
            int puu = 0;
            int.TryParse(txtExtPuu.Text, out puu);  
            string ErrorCode = "";
            await Task.Run(() =>
            {
                d.ExtMovJ(extID - 1, puuTmp, spd , acc, dec);  
            });
        }

        private void btn_ExtMovJ_MouseUp(object sender, MouseEventArgs e)
        {
            d.ExtMotionStop();
        }

        private void btn_ExtSethome_Click(object sender, EventArgs e)
        { 
            bool[] states = d.ExtServoState(); 
            int.TryParse(ExternalIdComboBox.Text, out extID);
            bool ret = d.ExtSetHome(extID, HomeMode.HomeMode35);
        }

        private async void btn_MovP_MouseDown(object sender, MouseEventArgs e)
        {
            PassMode passmode = PassMode.None;
            if(rad_OverlapTime.Checked == true)
            {
                passmode = PassMode.OverlapTime;
            }
            else if (rad_OverlapDistance.Checked == true)
            {
                passmode = PassMode.OverlapDistance;
            }

            int Overlapinput = 0;
            if (rad_OverlapTime.Checked == true || rad_OverlapDistance.Checked == true)
            {
                int.TryParse(txt_Overlapinput.Text, out Overlapinput);
            }
            int spd = 1;
            int acc = 1;
            int dec = 1;
            int.TryParse(textBox12.Text, out spd);
            int.TryParse(textBox16.Text, out acc);
            int.TryParse(textBox17.Text, out dec);

            await Task.Run(() =>
            {
                cPoint p = new cPoint();
                p = d.GetGlobalPoint(focus_RowIndex);
                string ErrorCode = d.MovP(p, passmode , Overlapinput, spd, acc, dec);   //  return 0: No Connect  1:Finish 2:No data 3:TimeOut   ExCeption: "Error Message"
                                                // while (!d.Ready()) ;
                System.Threading.Thread.Sleep(10);
            });
        }

        private void btn_MovP_MouseUp(object sender, MouseEventArgs e)
        {
            d.MotionStop();
        }

        private async void btn_CalculateToolFrameToolSize_Click(object sender, EventArgs e)
        {
            cPoints WorkPoint = new cPoints();

            double hight = 0;
            double width = 0;
            double degree = 0;

            await Task.Run(() =>
            {
                cPoint p = new cPoint();
                p = d.GetGlobalPoint(focus_RowIndex);
                cPoint ret = d.GetPos();
                d.MovP(p);   //  return 0: No Connect  1:Finish 2:No data 3:TimeOut   ExCeption: "Error Message"
                while (!(ret[eAxisName.X] == p[eAxisName.X] && ret[eAxisName.Y] == p[eAxisName.Y] && ret[eAxisName.Z] == p[eAxisName.Z] && ret[eAxisName.RZ] == p[eAxisName.RZ])) {

                    ret = d.GetPos();
                };
            
                int IJRCMode = 1;
                d.TechGlobalPoint(focus_RowIndex, IJRCMode);
                p = d.GetGlobalPoint(focus_RowIndex);

                cPoint p2 = new cPoint();
                p2 = d.GetGlobalPoint(focus_RowIndex + 1);
                d.MovP(p2);   //  return 0: No Connect  1:Finish 2:No data 3:TimeOut   ExCeption: "Error Message"
                while (!(ret[eAxisName.X] == p2[eAxisName.X] && ret[eAxisName.Y] == p2[eAxisName.Y] && ret[eAxisName.Z] == p2[eAxisName.Z] && ret[eAxisName.RZ] == p2[eAxisName.RZ]))
                {
                    ret = d.GetPos();
                };
                d.TechGlobalPoint(focus_RowIndex + 1, IJRCMode);
                p2 = d.GetGlobalPoint(focus_RowIndex + 1);


                cPoint p3 = new cPoint();
                p3 = d.GetGlobalPoint(focus_RowIndex + 2);
                d.MovP(p3);   //  return 0: No Connect  1:Finish 2:No data 3:TimeOut   ExCeption: "Error Message"
                while (!(ret[eAxisName.X] == p3[eAxisName.X] && ret[eAxisName.Y] == p3[eAxisName.Y] && ret[eAxisName.Z] == p3[eAxisName.Z] && ret[eAxisName.RZ] == p3[eAxisName.RZ]))
                {
                    ret = d.GetPos();
                };

                d.TechGlobalPoint(focus_RowIndex + 2, IJRCMode);
                p3 = d.GetGlobalPoint(focus_RowIndex + 2);

                 
                WorkPoint[0] = p; 
                WorkPoint[1] = p2; 
                WorkPoint[2] = p3;


                ArrayList Error = new ArrayList();
                d.CalculateToolFrameToolSize(WorkPoint, ref hight, ref width, ref degree, ref Error); 
            });

            txt_height.Text = hight.ToString();
            txt_width.Text = width.ToString();
            txt_degree.Text = degree.ToString();
        }

        private void btn_CSV2LPT_Click(object sender, EventArgs e)
        {
            d.CSV2LPT(txt_sourcepath.Text, txt_dirpath.Text, "LPTFile", 4400);
        }

        private void btn_MotionStop_Click(object sender, EventArgs e)
        {
            d.MotionStop();
        }

        private void btn_getInfo_Click(object sender, EventArgs e)
        {
            int ret = d.GetRobotMechanism(ref robotMechanism);
            ccb_Joint.Items.Clear();
            for ( int idx = 0; idx < robotMechanism.gear_ratio.Length; idx ++)
            {
                ccb_Joint.Items.Add("J" + (idx + 1));
            }
            ccb_Joint.Text = ccb_Joint.Items[0].ToString();
            lbl_arm_length.Text = robotMechanism.arm_length.ToString();
            lbl_gearratio.Text = robotMechanism.gear_ratio[0].ToString();
            lbl_pitch.Text = robotMechanism.pitch[0].ToString();
            lbl_reduceDen.Text = robotMechanism.reduce_ratio_den[0].ToString();
            lbl_reduceNum.Text = robotMechanism.reduce_ratio_num[0].ToString();
            lbl_softlimitN.Text = robotMechanism.soft_limit_joint_n[0].ToString() + "PUU";
            lbl_softlimitP.Text = robotMechanism.soft_limit_joint_p[0].ToString() + "PUU";
            lbl_softlimitN_degree.Text = (robotMechanism.soft_limit_degree_n[0] / 1000).ToString() + "deg/mm";
            lbl_softlimitP_degree.Text = (robotMechanism.soft_limit_degree_p[0] / 1000).ToString() + "deg/mm";
        }

        private void ccb_Joint_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ccb_Joint.Text = ccb_Joint.Items[ccb_Joint.SelectedIndex].ToString();
            lbl_arm_length.Text = robotMechanism.arm_length.ToString();
            lbl_gearratio.Text = robotMechanism.gear_ratio[ccb_Joint.SelectedIndex].ToString();
            lbl_pitch.Text = robotMechanism.pitch[ccb_Joint.SelectedIndex].ToString();
            lbl_reduceDen.Text = robotMechanism.reduce_ratio_den[ccb_Joint.SelectedIndex].ToString();
            lbl_reduceNum.Text = robotMechanism.reduce_ratio_num[ccb_Joint.SelectedIndex].ToString();
            lbl_softlimitN.Text = robotMechanism.soft_limit_joint_n[ccb_Joint.SelectedIndex].ToString() + "PUU";
            lbl_softlimitP.Text = robotMechanism.soft_limit_joint_p[ccb_Joint.SelectedIndex].ToString() + "PUU";
            lbl_softlimitN_degree.Text = (robotMechanism.soft_limit_degree_n[ccb_Joint.SelectedIndex] / 1000).ToString() + "deg/mm";
            lbl_softlimitP_degree.Text = (robotMechanism.soft_limit_degree_p[ccb_Joint.SelectedIndex] / 1000).ToString() + "deg/mm";
        }

        private void btn_GoHomeSingle_MouseUp(object sender, MouseEventArgs e)
        {
            d.MovStop();
        }

        private void btn_GoHomeSingle_MouseDown(object sender, MouseEventArgs e)
        {
            eAxisName axisName = eAxisName.J1;
            switch (cbb_GoHomeJoint.Text)
            {
                case "J1":
                    axisName = eAxisName.J1;
                    break;
                case "J2":
                    axisName = eAxisName.J2;
                    break;
                case "J3":
                    axisName = eAxisName.J3;
                    break;
                case "J4":
                    axisName = eAxisName.J4;
                    break;
                case "J5":
                    axisName = eAxisName.J5;
                    break;
                case "J6":
                    axisName = eAxisName.J6;
                    break;
            }
            d.GoHomeSingle(axisName);
        }

        private void btn_GotoMovL_MouseDown(object sender, MouseEventArgs e)
        {
            cPoint p = new cPoint();
            p = d.GetGlobalPoint(focus_RowIndex);
            d.GotoMovL(p);
        }

        private void btn_RebootRobot_Click(object sender, EventArgs e)
        {
            d.RebootRobot();
        }

        private void btn_GetOverrideSpeed_Click(object sender, EventArgs e)
        {
            double speed = 0;
            d.GetOverrideSpeed(ref speed);
            txt_SetOverrideSpeed.Text = speed.ToString();
        }

        private void btn_SetSerialPortParameter_Click(object sender, EventArgs e)
        {
            Baudrate baudrate = (Baudrate)cbb_baudrate.SelectedItem;
            Protocol protocol = (Protocol)cbb_Protocol.SelectedItem;
            ModbusMode modbusMode = (ModbusMode)cbb_ModbusMode.SelectedItem;
            short address = 0;
            short.TryParse(txt_address.Text, out address);
            string ret = d.SetSerialPortParameter(address, baudrate, protocol, modbusMode);

        }

        private async void btn_SetToolFrameRotation_Click(object sender, EventArgs e)
        {

            int idx = 1;
            int.TryParse(combobox_ToolFrame_Number.Text, out idx);
            await Task.Run(() =>
            {
                cPoint ret = new cPoint();
                cPoint pori = new cPoint(); 

                pori = d.GetGlobalPoint(focus_RowIndex);
                d.GotoMovP(pori);
                while (!(ret[eAxisName.X] == pori[eAxisName.X] && ret[eAxisName.Y] == pori[eAxisName.Y] && ret[eAxisName.Z] == pori[eAxisName.Z] && ret[eAxisName.RZ] == pori[eAxisName.RZ]))
                {
                    ret = d.GetPos();
                };
                pori = d.GetPos();
                cPoint px = new cPoint();
                px = d.GetGlobalPoint(focus_RowIndex + 1);
                d.GotoMovP(px);
                while (!(ret[eAxisName.X] == px[eAxisName.X] && ret[eAxisName.Y] == px[eAxisName.Y] && ret[eAxisName.Z] == px[eAxisName.Z] && ret[eAxisName.RZ] == px[eAxisName.RZ]))
                {
                    ret = d.GetPos();
                };

                px = d.GetPos();
                cPoint pxy = new cPoint();
                pxy = d.GetGlobalPoint(focus_RowIndex + 2);
                d.GotoMovP(pxy);
                while (!(ret[eAxisName.X] == pxy[eAxisName.X] && ret[eAxisName.Y] == pxy[eAxisName.Y] && ret[eAxisName.Z] == pxy[eAxisName.Z] && ret[eAxisName.RZ] == pxy[eAxisName.RZ]))
                {
                    ret = d.GetPos();
                };
                pxy = d.GetPos(); 

                cPoints valuePoints = new cPoints();
                valuePoints[0] = pori;
                valuePoints[1] = px;
                valuePoints[2] = pxy;
                d.SetToolFrameRotation(idx, valuePoints);
            });
        }

        private void btn_GetToolFrameRotationValue_Click(object sender, EventArgs e)
        {
            int idx = 1;
            int roll = 0;
            int pitch = 0;
            int yow = 0;
            d.GetToolFrameRotationValue(idx,ref roll, ref pitch, ref yow);
            double Roll = roll;
            double Pitch = pitch;
            double Yow = yow;

            Roll = Roll / 1000;
            Pitch = Pitch / 1000;
            Roll = Yow / 1000;
            txt_Roll.Text = Roll.ToString();
            txt_Pitch.Text = Pitch.ToString();
            txt_Yow.Text = Yow.ToString();
        }

        private void btn_LPT_Build_Funcion_Click(object sender, EventArgs e)
        {
            cPoints WorkPoint = new cPoints();
            cPoint p = new cPoint();
            for (int idx = 0; idx < 10; idx++)
            {
                p = new cPoint();
                p = d.GetGlobalPoint(idx);
                p.Name = "Local" + idx;
                WorkPoint[idx] = p;
            }
            d.LPT_Build_Funcion(WorkPoint, txt_dirpath.Text);
        }

        private void btn_UpdateBinFile_Click(object sender, EventArgs e)
        {
            d.UpdateBinFile(txt_dirpath.Text+ "LPT.bin","LPT.bin"); //D:\LPT.bin
        }

        private async void btn_RelToolMovP_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                cPoint p = new cPoint();
                p = d.GetGlobalPoint(focus_RowIndex);
                
                string ErrorCode = d.RelToolMovP(p,eAxisName.X.ToString(),10);
            });
        }

        private async void btn_RelToolMovL_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                cPoint p = new cPoint();
                p = d.GetGlobalPoint(focus_RowIndex);
                string ErrorCode = d.RelToolMovL(p, eAxisName.X.ToString(), 10);

            });
        }

        private async void btn_ContinuousRelToolP_Click(object sender, EventArgs e)
        {
            cPoint p = new cPoint();
            listBox1.SelectedIndex = 0;
            int count = points.Count;

            if (count >= 2)
            {
                for (int i = 1; i <= count; i++)
                {
                    listBox1.SelectedIndex = (i - 1);
                    p = d.GetGlobalPoint(points[i - 1]);

                    await Task.Run(() =>
                    {
                        if (i == 1)
                            d.StartContinuousRelToolP(p, eAxisName.X.ToString(), 10);
                        else if (i == count)
                            d.EndContinuousRelToolP(p, eAxisName.X.ToString(), 10);
                        else
                            d.PathRelToolP(p, eAxisName.X.ToString(), 10);
                    });
                }
            }
        }

        private async void btn_ContinuousRelToolL_Click(object sender, EventArgs e)
        {
            cPoint p = new cPoint();
            listBox1.SelectedIndex = 0;
            int count = points.Count;

            if (count >= 2)
            {
                for (int i = 1; i <= count; i++)
                {
                    listBox1.SelectedIndex = (i - 1);
                    p = d.GetGlobalPoint(points[i - 1]);

                    await Task.Run(() =>
                    {
                        if (i == 1)
                            d.StartContinuousRelToolL(p,eAxisName.X.ToString(),10);
                        else if (i == count)
                            d.EndContinuousRelToolL(p, eAxisName.X.ToString(), 10);
                        else
                            d.PathRelToolL(p, eAxisName.X.ToString(), 10);
                    });
                }
            }
        }

        private void btn_SetOverrideSpeed_Click(object sender, EventArgs e)
        {
            double speed = 0;
            double.TryParse(txt_SetOverrideSpeed.Text, out speed);
            d.SetOverrideSpeed(speed);
        }
         
    }
}
