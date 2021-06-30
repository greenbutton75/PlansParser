namespace PlansParser
{
    partial class PlansDownloaderForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.Logic = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.button7 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.AutoStartRunMappingSql = new System.Windows.Forms.CheckBox();
            this.RunMappingSqlButton = new System.Windows.Forms.Button();
            this.LoadBigAssDataButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.BaseSourceFolderDirTextBox = new System.Windows.Forms.TextBox();
            this.AutoStartLoadNewPlans = new System.Windows.Forms.CheckBox();
            this.AutoStartDawnloadPdfPlans = new System.Windows.Forms.CheckBox();
            this.DownloadNewPlansButton = new System.Windows.Forms.Button();
            this.AutoStartConvertToCsv = new System.Windows.Forms.CheckBox();
            this.txtFunds = new System.Windows.Forms.TextBox();
            this.ConvertXlsToCsvButton = new System.Windows.Forms.Button();
            this.ConvertPdfToXls = new System.Windows.Forms.Button();
            this.DownloadGowPlansPdfButton = new System.Windows.Forms.Button();
            this.DownloadGowPlansListButton = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.prgLine = new System.Windows.Forms.ToolStripProgressBar();
            this.prgLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.DetectEmptyXlsxButton = new System.Windows.Forms.Button();
            this.ParseSubAccButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ToolsBaseFolderTextBox = new System.Windows.Forms.TextBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.DownloadLoansButton = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Logic.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Logic
            // 
            this.Logic.BackColor = System.Drawing.SystemColors.Control;
            this.Logic.Controls.Add(this.txtLog);
            this.Logic.Controls.Add(this.MainPanel);
            this.Logic.Location = new System.Drawing.Point(4, 29);
            this.Logic.Name = "Logic";
            this.Logic.Size = new System.Drawing.Size(1993, 971);
            this.Logic.TabIndex = 0;
            this.Logic.Text = "Logic";
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtLog.Location = new System.Drawing.Point(0, 834);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(1993, 137);
            this.txtLog.TabIndex = 2;
            this.txtLog.WordWrap = false;
            // 
            // MainPanel
            // 
            this.MainPanel.BackColor = System.Drawing.SystemColors.Control;
            this.MainPanel.Controls.Add(this.label3);
            this.MainPanel.Controls.Add(this.label2);
            this.MainPanel.Controls.Add(this.textBox3);
            this.MainPanel.Controls.Add(this.button7);
            this.MainPanel.Controls.Add(this.textBox1);
            this.MainPanel.Controls.Add(this.button6);
            this.MainPanel.Controls.Add(this.button5);
            this.MainPanel.Controls.Add(this.button4);
            this.MainPanel.Controls.Add(this.button3);
            this.MainPanel.Controls.Add(this.button2);
            this.MainPanel.Controls.Add(this.button1);
            this.MainPanel.Controls.Add(this.AutoStartRunMappingSql);
            this.MainPanel.Controls.Add(this.RunMappingSqlButton);
            this.MainPanel.Controls.Add(this.LoadBigAssDataButton);
            this.MainPanel.Controls.Add(this.label4);
            this.MainPanel.Controls.Add(this.BaseSourceFolderDirTextBox);
            this.MainPanel.Controls.Add(this.AutoStartLoadNewPlans);
            this.MainPanel.Controls.Add(this.AutoStartDawnloadPdfPlans);
            this.MainPanel.Controls.Add(this.DownloadNewPlansButton);
            this.MainPanel.Controls.Add(this.AutoStartConvertToCsv);
            this.MainPanel.Controls.Add(this.txtFunds);
            this.MainPanel.Controls.Add(this.ConvertXlsToCsvButton);
            this.MainPanel.Controls.Add(this.ConvertPdfToXls);
            this.MainPanel.Controls.Add(this.DownloadGowPlansPdfButton);
            this.MainPanel.Controls.Add(this.DownloadGowPlansListButton);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(1993, 834);
            this.MainPanel.TabIndex = 1;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(621, 424);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(220, 61);
            this.button7.TabIndex = 81;
            this.button7.Text = "Clear Empty file (after patterns)";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(138, 190);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 26);
            this.textBox1.TabIndex = 80;
            this.textBox1.Text = "1000";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(617, 671);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(224, 81);
            this.button6.TabIndex = 79;
            this.button6.Text = "Find CSV without providers";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(624, 491);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(220, 61);
            this.button5.TabIndex = 78;
            this.button5.Text = "Create SQL (after ML && patterns and clearing Empty file)";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(617, 327);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(226, 53);
            this.button4.TabIndex = 77;
            this.button4.Text = "Unload txt for ML (after Mapping.sql)";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(612, 232);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(232, 59);
            this.button3.TabIndex = 76;
            this.button3.Text = "Get ACCOP files (after PDF_with_positions.txt)";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(612, 72);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(232, 59);
            this.button2.TabIndex = 75;
            this.button2.Text = "Prepare PDFs (after PDF downloading)";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(612, 149);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(232, 59);
            this.button1.TabIndex = 74;
            this.button1.Text = "Merge CSV (after both CSV folders ready)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AutoStartRunMappingSql
            // 
            this.AutoStartRunMappingSql.AutoSize = true;
            this.AutoStartRunMappingSql.Location = new System.Drawing.Point(138, 472);
            this.AutoStartRunMappingSql.Name = "AutoStartRunMappingSql";
            this.AutoStartRunMappingSql.Size = new System.Drawing.Size(174, 24);
            this.AutoStartRunMappingSql.TabIndex = 70;
            this.AutoStartRunMappingSql.Text = "Auto start next step";
            this.AutoStartRunMappingSql.UseVisualStyleBackColor = true;
            // 
            // RunMappingSqlButton
            // 
            this.RunMappingSqlButton.Location = new System.Drawing.Point(12, 502);
            this.RunMappingSqlButton.Name = "RunMappingSqlButton";
            this.RunMappingSqlButton.Size = new System.Drawing.Size(120, 63);
            this.RunMappingSqlButton.TabIndex = 66;
            this.RunMappingSqlButton.Text = "Run Mapping.sql";
            this.RunMappingSqlButton.UseVisualStyleBackColor = true;
            this.RunMappingSqlButton.Click += new System.EventHandler(this.RunMappingSqlButton_Click);
            // 
            // LoadBigAssDataButton
            // 
            this.LoadBigAssDataButton.Location = new System.Drawing.Point(12, 414);
            this.LoadBigAssDataButton.Name = "LoadBigAssDataButton";
            this.LoadBigAssDataButton.Size = new System.Drawing.Size(120, 63);
            this.LoadBigAssDataButton.TabIndex = 64;
            this.LoadBigAssDataButton.Text = "Load BigAss Data";
            this.LoadBigAssDataButton.UseVisualStyleBackColor = true;
            this.LoadBigAssDataButton.Click += new System.EventHandler(this.LoadBigAssDataButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(156, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 20);
            this.label4.TabIndex = 41;
            this.label4.Text = "Base Folder";
            // 
            // BaseSourceFolderDirTextBox
            // 
            this.BaseSourceFolderDirTextBox.Location = new System.Drawing.Point(159, 28);
            this.BaseSourceFolderDirTextBox.Name = "BaseSourceFolderDirTextBox";
            this.BaseSourceFolderDirTextBox.Size = new System.Drawing.Size(716, 26);
            this.BaseSourceFolderDirTextBox.TabIndex = 40;
            this.BaseSourceFolderDirTextBox.Text = "F:\\Rixtema\\Plans_01022017";
            // 
            // AutoStartLoadNewPlans
            // 
            this.AutoStartLoadNewPlans.AutoSize = true;
            this.AutoStartLoadNewPlans.Location = new System.Drawing.Point(138, 65);
            this.AutoStartLoadNewPlans.Name = "AutoStartLoadNewPlans";
            this.AutoStartLoadNewPlans.Size = new System.Drawing.Size(174, 24);
            this.AutoStartLoadNewPlans.TabIndex = 39;
            this.AutoStartLoadNewPlans.Text = "Auto start next step";
            this.AutoStartLoadNewPlans.UseVisualStyleBackColor = true;
            // 
            // AutoStartDawnloadPdfPlans
            // 
            this.AutoStartDawnloadPdfPlans.AutoSize = true;
            this.AutoStartDawnloadPdfPlans.Location = new System.Drawing.Point(138, 151);
            this.AutoStartDawnloadPdfPlans.Name = "AutoStartDawnloadPdfPlans";
            this.AutoStartDawnloadPdfPlans.Size = new System.Drawing.Size(174, 24);
            this.AutoStartDawnloadPdfPlans.TabIndex = 38;
            this.AutoStartDawnloadPdfPlans.Text = "Auto start next step";
            this.AutoStartDawnloadPdfPlans.UseVisualStyleBackColor = true;
            // 
            // DownloadNewPlansButton
            // 
            this.DownloadNewPlansButton.Location = new System.Drawing.Point(12, 94);
            this.DownloadNewPlansButton.Name = "DownloadNewPlansButton";
            this.DownloadNewPlansButton.Size = new System.Drawing.Size(120, 58);
            this.DownloadNewPlansButton.TabIndex = 37;
            this.DownloadNewPlansButton.Text = "Load New Plans";
            this.DownloadNewPlansButton.UseVisualStyleBackColor = true;
            this.DownloadNewPlansButton.Click += new System.EventHandler(this.AddPlanLoad_Click);
            // 
            // AutoStartConvertToCsv
            // 
            this.AutoStartConvertToCsv.AutoSize = true;
            this.AutoStartConvertToCsv.Location = new System.Drawing.Point(138, 314);
            this.AutoStartConvertToCsv.Name = "AutoStartConvertToCsv";
            this.AutoStartConvertToCsv.Size = new System.Drawing.Size(174, 24);
            this.AutoStartConvertToCsv.TabIndex = 35;
            this.AutoStartConvertToCsv.Text = "Auto start next step";
            this.AutoStartConvertToCsv.UseVisualStyleBackColor = true;
            // 
            // txtFunds
            // 
            this.txtFunds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFunds.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtFunds.Location = new System.Drawing.Point(878, 3);
            this.txtFunds.Multiline = true;
            this.txtFunds.Name = "txtFunds";
            this.txtFunds.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtFunds.Size = new System.Drawing.Size(1105, 827);
            this.txtFunds.TabIndex = 34;
            this.txtFunds.WordWrap = false;
            // 
            // ConvertXlsToCsvButton
            // 
            this.ConvertXlsToCsvButton.Location = new System.Drawing.Point(12, 337);
            this.ConvertXlsToCsvButton.Name = "ConvertXlsToCsvButton";
            this.ConvertXlsToCsvButton.Size = new System.Drawing.Size(120, 62);
            this.ConvertXlsToCsvButton.TabIndex = 22;
            this.ConvertXlsToCsvButton.Text = "Convert xls to csv";
            this.ConvertXlsToCsvButton.UseVisualStyleBackColor = true;
            this.ConvertXlsToCsvButton.Click += new System.EventHandler(this.ConvertXlsToCsvButton_Click);
            // 
            // ConvertPdfToXls
            // 
            this.ConvertPdfToXls.Location = new System.Drawing.Point(12, 257);
            this.ConvertPdfToXls.Name = "ConvertPdfToXls";
            this.ConvertPdfToXls.Size = new System.Drawing.Size(120, 62);
            this.ConvertPdfToXls.TabIndex = 18;
            this.ConvertPdfToXls.Text = "Convert Pdf To Xlsx";
            this.ConvertPdfToXls.UseVisualStyleBackColor = true;
            this.ConvertPdfToXls.Click += new System.EventHandler(this.ConvertPdfToXml_Click);
            // 
            // DownloadGowPlansPdfButton
            // 
            this.DownloadGowPlansPdfButton.Location = new System.Drawing.Point(12, 172);
            this.DownloadGowPlansPdfButton.Name = "DownloadGowPlansPdfButton";
            this.DownloadGowPlansPdfButton.Size = new System.Drawing.Size(120, 62);
            this.DownloadGowPlansPdfButton.TabIndex = 14;
            this.DownloadGowPlansPdfButton.Text = "Download pdf plans";
            this.DownloadGowPlansPdfButton.UseVisualStyleBackColor = true;
            this.DownloadGowPlansPdfButton.Click += new System.EventHandler(this.DownloadGovPlansPdfButton_Click);
            // 
            // DownloadGowPlansListButton
            // 
            this.DownloadGowPlansListButton.Location = new System.Drawing.Point(12, 9);
            this.DownloadGowPlansListButton.Name = "DownloadGowPlansListButton";
            this.DownloadGowPlansListButton.Size = new System.Drawing.Size(120, 62);
            this.DownloadGowPlansListButton.TabIndex = 13;
            this.DownloadGowPlansListButton.Text = "StartNew Iteration";
            this.DownloadGowPlansListButton.UseVisualStyleBackColor = true;
            this.DownloadGowPlansListButton.Click += new System.EventHandler(this.DownloadGowPlansListButton_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.prgLine,
            this.prgLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 972);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 15, 0);
            this.statusStrip1.Size = new System.Drawing.Size(2001, 32);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.statusStrip1_ItemClicked);
            // 
            // prgLine
            // 
            this.prgLine.Name = "prgLine";
            this.prgLine.Size = new System.Drawing.Size(338, 24);
            // 
            // prgLabel
            // 
            this.prgLabel.Name = "prgLabel";
            this.prgLabel.Size = new System.Drawing.Size(60, 25);
            this.prgLabel.Text = "Ready";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.Logic);
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(2001, 1004);
            this.tabControl.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1993, 971);
            this.tabPage1.TabIndex = 1;
            this.tabPage1.Text = "Tools";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.DetectEmptyXlsxButton);
            this.panel1.Controls.Add(this.ParseSubAccButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.ToolsBaseFolderTextBox);
            this.panel1.Controls.Add(this.checkBox6);
            this.panel1.Controls.Add(this.textBox2);
            this.panel1.Controls.Add(this.DownloadLoansButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1987, 831);
            this.panel1.TabIndex = 2;
            // 
            // DetectEmptyXlsxButton
            // 
            this.DetectEmptyXlsxButton.Location = new System.Drawing.Point(12, 185);
            this.DetectEmptyXlsxButton.Name = "DetectEmptyXlsxButton";
            this.DetectEmptyXlsxButton.Size = new System.Drawing.Size(120, 62);
            this.DetectEmptyXlsxButton.TabIndex = 43;
            this.DetectEmptyXlsxButton.Text = "Detect Empty Xlsx";
            this.DetectEmptyXlsxButton.UseVisualStyleBackColor = true;
            this.DetectEmptyXlsxButton.Click += new System.EventHandler(this.DetectEmptyXlsxButton_Click);
            // 
            // ParseSubAccButton
            // 
            this.ParseSubAccButton.Location = new System.Drawing.Point(12, 100);
            this.ParseSubAccButton.Name = "ParseSubAccButton";
            this.ParseSubAccButton.Size = new System.Drawing.Size(120, 62);
            this.ParseSubAccButton.TabIndex = 42;
            this.ParseSubAccButton.Text = "Parse SubAcc";
            this.ParseSubAccButton.UseVisualStyleBackColor = true;
            this.ParseSubAccButton.Click += new System.EventHandler(this.ParseSubAccButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(156, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 20);
            this.label1.TabIndex = 41;
            this.label1.Text = "Base Folder";
            // 
            // ToolsBaseFolderTextBox
            // 
            this.ToolsBaseFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolsBaseFolderTextBox.Location = new System.Drawing.Point(159, 28);
            this.ToolsBaseFolderTextBox.Name = "ToolsBaseFolderTextBox";
            this.ToolsBaseFolderTextBox.Size = new System.Drawing.Size(1145, 26);
            this.ToolsBaseFolderTextBox.TabIndex = 40;
            this.ToolsBaseFolderTextBox.Text = "F:\\Rixtema\\SubAcc\\done\\Funds\\Edgar";
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Checked = true;
            this.checkBox6.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox6.Location = new System.Drawing.Point(668, 29);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(174, 24);
            this.checkBox6.TabIndex = 39;
            this.checkBox6.Text = "Auto start next step";
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox2.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox2.Location = new System.Drawing.Point(878, 3);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox2.Size = new System.Drawing.Size(439, 822);
            this.textBox2.TabIndex = 34;
            this.textBox2.WordWrap = false;
            // 
            // DownloadLoansButton
            // 
            this.DownloadLoansButton.Location = new System.Drawing.Point(12, 9);
            this.DownloadLoansButton.Name = "DownloadLoansButton";
            this.DownloadLoansButton.Size = new System.Drawing.Size(120, 62);
            this.DownloadLoansButton.TabIndex = 13;
            this.DownloadLoansButton.Text = "Download Loans List";
            this.DownloadLoansButton.UseVisualStyleBackColor = true;
            this.DownloadLoansButton.Click += new System.EventHandler(this.DownloadLoansButton_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(138, 222);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 26);
            this.textBox3.TabIndex = 82;
            this.textBox3.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(244, 196);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(188, 20);
            this.label2.TabIndex = 83;
            this.label2.Text = "Time between downloads";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(244, 228);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 20);
            this.label3.TabIndex = 84;
            this.label3.Text = "Processes";
            // 
            // PlansDownloaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2001, 1004);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl);
            this.Name = "PlansDownloaderForm";
            this.Text = "PlansDownloader";
            this.Logic.ResumeLayout(false);
            this.Logic.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TabPage Logic;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.Button DownloadGowPlansPdfButton;
        private System.Windows.Forms.Button DownloadGowPlansListButton;
        private System.Windows.Forms.Button ConvertPdfToXls;
        private System.Windows.Forms.Button ConvertXlsToCsvButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar prgLine;
        private System.Windows.Forms.ToolStripStatusLabel prgLabel;
        private System.Windows.Forms.TextBox txtFunds;
        private System.Windows.Forms.CheckBox AutoStartConvertToCsv;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox BaseSourceFolderDirTextBox;
        private System.Windows.Forms.CheckBox AutoStartLoadNewPlans;
        private System.Windows.Forms.CheckBox AutoStartDawnloadPdfPlans;
        private System.Windows.Forms.Button DownloadNewPlansButton;
        private System.Windows.Forms.Button LoadBigAssDataButton;
        private System.Windows.Forms.Button RunMappingSqlButton;
        private System.Windows.Forms.CheckBox AutoStartRunMappingSql;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ToolsBaseFolderTextBox;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button DownloadLoansButton;
        private System.Windows.Forms.Button ParseSubAccButton;
        private System.Windows.Forms.Button DetectEmptyXlsxButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox3;
    }
}

