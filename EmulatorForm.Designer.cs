namespace ModbusRtuEmulator
{
    partial class EmulatorForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
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
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tvTree = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiClear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLoad = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSave = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDeleteSplitter = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.pgProps = new System.Windows.Forms.PropertyGrid();
            this.lbMessages = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbMute = new System.Windows.Forms.CheckBox();
            this.nudPort = new System.Windows.Forms.NumericUpDown();
            this.dudBaudRate = new System.Windows.Forms.DomainUpDown();
            this.cbPort = new System.Windows.Forms.ComboBox();
            this.lbPortTuned = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.tvTree, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pgProps, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbMessages, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(591, 557);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // tvTree
            // 
            this.tvTree.ContextMenuStrip = this.contextMenuStrip1;
            this.tvTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTree.HideSelection = false;
            this.tvTree.Location = new System.Drawing.Point(3, 3);
            this.tvTree.Name = "tvTree";
            this.tvTree.Size = new System.Drawing.Size(289, 324);
            this.tvTree.TabIndex = 2;
            this.tvTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvTree_AfterSelect);
            this.tvTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvTree_MouseDown);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiClear,
            this.tsmiLoad,
            this.tsmiSave,
            this.tsmiDeleteSplitter,
            this.tsmiDelete});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(138, 98);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // tsmiClear
            // 
            this.tsmiClear.Name = "tsmiClear";
            this.tsmiClear.Size = new System.Drawing.Size(137, 22);
            this.tsmiClear.Text = "Очистить";
            this.tsmiClear.Click += new System.EventHandler(this.tsmiClear_Click);
            // 
            // tsmiLoad
            // 
            this.tsmiLoad.Name = "tsmiLoad";
            this.tsmiLoad.Size = new System.Drawing.Size(137, 22);
            this.tsmiLoad.Text = "Загрузить...";
            this.tsmiLoad.Click += new System.EventHandler(this.tsmiLoad_Click);
            // 
            // tsmiSave
            // 
            this.tsmiSave.Name = "tsmiSave";
            this.tsmiSave.Size = new System.Drawing.Size(137, 22);
            this.tsmiSave.Text = "Сохранить";
            this.tsmiSave.Click += new System.EventHandler(this.tsmiSave_Click);
            // 
            // tsmiDeleteSplitter
            // 
            this.tsmiDeleteSplitter.Name = "tsmiDeleteSplitter";
            this.tsmiDeleteSplitter.Size = new System.Drawing.Size(134, 6);
            // 
            // tsmiDelete
            // 
            this.tsmiDelete.Name = "tsmiDelete";
            this.tsmiDelete.Size = new System.Drawing.Size(137, 22);
            this.tsmiDelete.Text = "Удалить";
            this.tsmiDelete.Click += new System.EventHandler(this.tsmiDelete_Click);
            // 
            // pgProps
            // 
            this.pgProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgProps.Location = new System.Drawing.Point(298, 3);
            this.pgProps.Name = "pgProps";
            this.pgProps.Size = new System.Drawing.Size(290, 324);
            this.pgProps.TabIndex = 3;
            this.pgProps.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgProps_PropertyValueChanged);
            // 
            // lbMessages
            // 
            this.lbMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.lbMessages, 2);
            this.lbMessages.FormattingEnabled = true;
            this.lbMessages.ItemHeight = 17;
            this.lbMessages.Location = new System.Drawing.Point(3, 363);
            this.lbMessages.Name = "lbMessages";
            this.lbMessages.Size = new System.Drawing.Size(585, 191);
            this.lbMessages.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
            this.panel1.Controls.Add(this.cbMute);
            this.panel1.Controls.Add(this.nudPort);
            this.panel1.Controls.Add(this.dudBaudRate);
            this.panel1.Controls.Add(this.cbPort);
            this.panel1.Controls.Add(this.lbPortTuned);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(2, 332);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(587, 26);
            this.panel1.TabIndex = 4;
            // 
            // cbMute
            // 
            this.cbMute.AutoSize = true;
            this.cbMute.Dock = System.Windows.Forms.DockStyle.Left;
            this.cbMute.Location = new System.Drawing.Point(202, 0);
            this.cbMute.Name = "cbMute";
            this.cbMute.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.cbMute.Size = new System.Drawing.Size(103, 26);
            this.cbMute.TabIndex = 6;
            this.cbMute.Text = "не отвечать";
            this.cbMute.UseVisualStyleBackColor = true;
            this.cbMute.CheckedChanged += new System.EventHandler(this.cbMute_CheckedChanged);
            // 
            // nudPort
            // 
            this.nudPort.Dock = System.Windows.Forms.DockStyle.Left;
            this.nudPort.Location = new System.Drawing.Point(147, 0);
            this.nudPort.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudPort.Name = "nudPort";
            this.nudPort.Size = new System.Drawing.Size(55, 25);
            this.nudPort.TabIndex = 5;
            this.nudPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudPort.Value = new decimal(new int[] {
            502,
            0,
            0,
            0});
            this.nudPort.ValueChanged += new System.EventHandler(this.nudPort_ValueChanged);
            // 
            // dudBaudRate
            // 
            this.dudBaudRate.Dock = System.Windows.Forms.DockStyle.Left;
            this.dudBaudRate.Items.Add("115200");
            this.dudBaudRate.Items.Add("57600");
            this.dudBaudRate.Items.Add("38400");
            this.dudBaudRate.Items.Add("19200");
            this.dudBaudRate.Items.Add("9600");
            this.dudBaudRate.Items.Add("4800");
            this.dudBaudRate.Items.Add("2400");
            this.dudBaudRate.Items.Add("1200");
            this.dudBaudRate.Items.Add("600");
            this.dudBaudRate.Items.Add("300");
            this.dudBaudRate.Location = new System.Drawing.Point(79, 0);
            this.dudBaudRate.Name = "dudBaudRate";
            this.dudBaudRate.ReadOnly = true;
            this.dudBaudRate.Size = new System.Drawing.Size(68, 25);
            this.dudBaudRate.TabIndex = 4;
            this.dudBaudRate.Text = "115200";
            this.dudBaudRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.dudBaudRate.SelectedItemChanged += new System.EventHandler(this.dudBaudRate_SelectedItemChanged);
            // 
            // cbPort
            // 
            this.cbPort.Dock = System.Windows.Forms.DockStyle.Left;
            this.cbPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPort.FormattingEnabled = true;
            this.cbPort.Location = new System.Drawing.Point(0, 0);
            this.cbPort.Name = "cbPort";
            this.cbPort.Size = new System.Drawing.Size(79, 25);
            this.cbPort.TabIndex = 3;
            this.cbPort.SelectionChangeCommitted += new System.EventHandler(this.cbPort_SelectionChangeCommitted);
            // 
            // lbPortTuned
            // 
            this.lbPortTuned.Dock = System.Windows.Forms.DockStyle.Right;
            this.lbPortTuned.Location = new System.Drawing.Point(480, 0);
            this.lbPortTuned.Name = "lbPortTuned";
            this.lbPortTuned.Size = new System.Drawing.Size(107, 26);
            this.lbPortTuned.TabIndex = 2;
            this.lbPortTuned.Text = "COM3, 115200";
            this.lbPortTuned.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "tree";
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "*.tree|*.tree";
            this.openFileDialog1.InitialDirectory = ".\\";
            // 
            // EmulatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 557);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "EmulatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Эмулятор протокола MODBUS RTU";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EmulatorForm_FormClosing);
            this.Load += new System.EventHandler(this.EmulatorForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TreeView tvTree;
        private System.Windows.Forms.PropertyGrid pgProps;
        private System.Windows.Forms.ListBox lbMessages;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbMute;
        private System.Windows.Forms.NumericUpDown nudPort;
        private System.Windows.Forms.DomainUpDown dudBaudRate;
        private System.Windows.Forms.ComboBox cbPort;
        private System.Windows.Forms.Label lbPortTuned;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiClear;
        private System.Windows.Forms.ToolStripMenuItem tsmiLoad;
        private System.Windows.Forms.ToolStripMenuItem tsmiSave;
        private System.Windows.Forms.ToolStripSeparator tsmiDeleteSplitter;
        private System.Windows.Forms.ToolStripMenuItem tsmiDelete;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}

