using System;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HP33120A_Controller
{
    public class Form1 : Form
    {
        private ComboBox comboBoxCOM, comboBoxWaveform, comboBoxFreqUnit, comboBoxVoltUnit;
        private TextBox txtFrequency, txtAmplitude, txtDuty, txtArbWave;
        private Button btnApply, btnQuery, btnLoadARB;
        private Label lblStatus;
        private SerialPort serialPort1;
        private string arbWaveName = "VOLATILE";

        public Form1()
        {
            this.Text = "HP 33120A Signal Generator Controller";
            this.ClientSize = new Size(480, 520);
            this.Font = new Font("Segoe UI", 10);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            CreateControls();
            PopulateCOMPorts();
            PopulateWaveforms();
        }

        private void CreateControls()
        {
            serialPort1 = new SerialPort
            {
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500
            };

            // COM Group
            var grpCom = new GroupBox { Text = "Connection", Location = new Point(10, 10), Size = new Size(450, 60) };
            this.Controls.Add(grpCom);
            grpCom.Controls.Add(new Label { Text = "COM Port:", Location = new Point(15, 25), AutoSize = true });
            comboBoxCOM = new ComboBox { Location = new Point(100, 20), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            grpCom.Controls.Add(comboBoxCOM);

            // Output Settings
            var grpOutput = new GroupBox { Text = "Output Settings", Location = new Point(10, 80), Size = new Size(450, 300) };
            this.Controls.Add(grpOutput);

            // Waveform
            grpOutput.Controls.Add(new Label { Text = "Waveform:", Location = new Point(15, 30), AutoSize = true });
            comboBoxWaveform = new ComboBox { Location = new Point(100, 25), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            comboBoxWaveform.SelectedIndexChanged += comboBoxWaveform_SelectedIndexChanged;
            grpOutput.Controls.Add(comboBoxWaveform);

            // Frequency
            grpOutput.Controls.Add(new Label { Text = "Frequency:", Location = new Point(15, 70), AutoSize = true });
            txtFrequency = new TextBox { Location = new Point(100, 65), Width = 120, Text = "1000" };
            grpOutput.Controls.Add(txtFrequency);
            comboBoxFreqUnit = new ComboBox { Location = new Point(230, 65), Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
            comboBoxFreqUnit.Items.AddRange(new string[] { "μHz", "Hz", "kHz", "MHz" });
            comboBoxFreqUnit.SelectedIndex = 1;
            grpOutput.Controls.Add(comboBoxFreqUnit);

            // Amplitude
            grpOutput.Controls.Add(new Label { Text = "Amplitude:", Location = new Point(15, 105), AutoSize = true });
            txtAmplitude = new TextBox { Location = new Point(100, 100), Width = 80, Text = "2" };
            grpOutput.Controls.Add(txtAmplitude);
            comboBoxVoltUnit = new ComboBox { Location = new Point(190, 100), Width = 60, DropDownStyle = ComboBoxStyle.DropDownList };
            comboBoxVoltUnit.Items.AddRange(new string[] { "mV", "V" });
            comboBoxVoltUnit.SelectedIndex = 1;
            grpOutput.Controls.Add(comboBoxVoltUnit);

            // Duty
            grpOutput.Controls.Add(new Label { Text = "Duty %:", Location = new Point(270, 100), AutoSize = true });
            txtDuty = new TextBox { Location = new Point(330, 100), Width = 60, Text = "50", Enabled = false };
            grpOutput.Controls.Add(txtDuty);

            // ARB waveform input
            grpOutput.Controls.Add(new Label { Text = "ARB Waveform (comma-separated):", Location = new Point(15, 145), AutoSize = true });
            txtArbWave = new TextBox
            {
                Location = new Point(15, 170),
                Width = 420,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            grpOutput.Controls.Add(txtArbWave);

            // Buttons
            btnApply = new Button { Location = new Point(100, 240), Text = "Apply", Width = 120, Height = 45 };
            btnApply.Click += btnApply_Click;
            grpOutput.Controls.Add(btnApply);

            btnQuery = new Button { Location = new Point(230, 240), Text = "Query Status", Width = 120, Height = 45 };
            btnQuery.Click += btnQuery_Click;
            grpOutput.Controls.Add(btnQuery);

            btnLoadARB = new Button { Location = new Point(100, 290), Text = "Upload ARB", Width = 120, Height = 45, Enabled = false };
            btnLoadARB.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtArbWave.Text))
                    UploadArbWaveformFromText(txtArbWave.Text, arbWaveName);
            };
            grpOutput.Controls.Add(btnLoadARB);

            // Device Status
            var grpStatus = new GroupBox { Text = "Device Status", Location = new Point(10, 390), Size = new Size(450, 120) };
            this.Controls.Add(grpStatus);
            lblStatus = new Label
            {
                Location = new Point(10, 25),
                Width = 430,
                Height = 85,
                Text = "Status will appear here...",
                AutoSize = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            grpStatus.Controls.Add(lblStatus);
        }

        private void PopulateCOMPorts()
        {
            comboBoxCOM.Items.Clear();
            comboBoxCOM.Items.AddRange(SerialPort.GetPortNames());
            if (comboBoxCOM.Items.Count > 0)
                comboBoxCOM.SelectedIndex = 0;
        }

        private void PopulateWaveforms()
        {
            comboBoxWaveform.Items.AddRange(new string[] { "SIN", "SQU", "TRI", "RAMP", "NOIS", "DC", "ARB" });
            comboBoxWaveform.SelectedIndex = 0;
        }

        private void comboBoxWaveform_SelectedIndexChanged(object sender, EventArgs e)
        {
            string wf = comboBoxWaveform.SelectedItem.ToString();
            txtDuty.Enabled = wf == "SQU";
            btnLoadARB.Enabled = wf == "ARB";
        }

        private bool OpenSerialPort()
        {
            try
            {
                if (!serialPort1.IsOpen)
                {
                    serialPort1.PortName = comboBoxCOM.SelectedItem.ToString();
                    serialPort1.DtrEnable = true;
                    serialPort1.RtsEnable = true;
                    serialPort1.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open serial port: " + ex.Message);
                return false;
            }
        }

        private void SendCommand(string cmd)
        {
            if (serialPort1.IsOpen)
                serialPort1.WriteLine(cmd);
        }

        private string Query(string cmd)
        {
            if (!serialPort1.IsOpen)
                throw new InvalidOperationException("Serial port is not open.");

            serialPort1.DiscardInBuffer();
            serialPort1.WriteLine(cmd);
            System.Threading.Thread.Sleep(100);
            return serialPort1.ReadExisting().Trim();
        }

        private double ScaleFrequency(double value, string unit)
        {
            switch (unit)
            {
                case "μHz": return value * 1e-6;
                case "Hz": return value;
                case "kHz": return value * 1e3;
                case "MHz": return value * 1e6;
                default: return value;
            }
        }

        private double ScaleVoltage(double value, string unit)
        {
            switch (unit)
            {
                case "mV": return value * 1e-3;
                case "V": return value;
                default: return value;
            }
        }

        private async void btnApply_Click(object sender, EventArgs e)
        {
            btnApply.Enabled = false;
            if (!OpenSerialPort()) { btnApply.Enabled = true; return; }

            string waveform = comboBoxWaveform.SelectedItem.ToString();

            if (!double.TryParse(txtFrequency.Text, out double freq)) freq = 1000;
            freq = ScaleFrequency(freq, comboBoxFreqUnit.SelectedItem.ToString());
            freq = Math.Max(0.1, Math.Min(freq, 15e6));

            if (!double.TryParse(txtAmplitude.Text, out double volt)) volt = 2;
            volt = ScaleVoltage(volt, comboBoxVoltUnit.SelectedItem.ToString());
            volt = Math.Max(0.0, Math.Min(volt, 10.0));

            double duty = 50;
            if (waveform == "SQU")
            {
                if (!double.TryParse(txtDuty.Text, out duty)) duty = 50;
                duty = Math.Max(0, Math.Min(duty, 100));
            }

            await Task.Run(() =>
            {
                try
                {
                    if (waveform == "ARB")
                    {
                        SendCommand("FUNC ARB");
                        SendCommand($"FUNC:ARB \"{arbWaveName}\"");
                    }
                    else
                    {
                        SendCommand("FUNC " + waveform);
                        SendCommand("FREQ " + freq);
                        SendCommand("VOLT " + volt);
                        if (waveform == "SQU") SendCommand("PULSe:DCYCle " + duty);
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Invoke(new Action(() => lblStatus.Text = "Error: " + ex.Message));
                }
            });

            lblStatus.Text = $"Applied: {waveform}, {freq} Hz, {volt} Vpp" + (waveform == "SQU" ? $", Duty {duty}%" : "");
            btnApply.Enabled = true;
        }

        private async void btnQuery_Click(object sender, EventArgs e)
        {
            btnQuery.Enabled = false;
            if (!OpenSerialPort()) { btnQuery.Enabled = true; return; }

            await Task.Run(() =>
            {
                try
                {
                    string func = Query("FUNC?");
                    string freq = Query("FREQ?");
                    string volt = Query("VOLT?");
                    string duty = func == "SQU" ? Query("PULSe:DCYCle?") : "";

                    string status = $"Function: {func}\nFrequency (Hz): {freq}\nAmplitude (Vpp): {volt}";
                    if (func == "SQU") status += $"\nDuty (%): {duty}";

                    lblStatus.Invoke(new Action(() => lblStatus.Text = status));
                }
                catch (Exception ex)
                {
                    lblStatus.Invoke(new Action(() => lblStatus.Text = "Error: " + ex.Message));
                }
            });

            btnQuery.Enabled = true;
        }

        private void UploadArbWaveformFromText(string text, string arbName)
        {
            if (!OpenSerialPort()) return;

            try
            {
                // Split by comma, newline, or space
                double[] points = text.Split(new[] { ',', '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => double.Parse(s.Trim()))
                                      .ToArray();

                // Normalize to -1..1
                double maxAbs = points.Max(p => Math.Abs(p));
                if (maxAbs > 1.0)
                    for (int i = 0; i < points.Length; i++)
                        points[i] /= maxAbs;

                if (points.Length > 8192)
                {
                    int factor = (int)Math.Ceiling(points.Length / 8192.0);
                    points = points.Where((x, i) => i % factor == 0).ToArray();
                }

                SendCommand("FORM REAL,32");
                SendCommand("FORM:BORD NORM");   // set normal byte order
                SendCommand("FUNC ARB");
                SendCommand($"FUNC:ARB \"{arbName}\"");

                int chunkSize = 200;
                for (int i = 0; i < points.Length; i += chunkSize)
                {
                    int len = Math.Min(chunkSize, points.Length - i);
                    string chunk = string.Join(",", points.Skip(i).Take(len).Select(p => p.ToString("F6")));
                    SendCommand($"DATA:ARB {arbName},{chunk}");
                    System.Threading.Thread.Sleep(50);
                }

                lblStatus.Text = $"ARB waveform uploaded ({points.Length} points) as {arbName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error uploading ARB waveform: " + ex.Message);
            }
        }
    }
}
