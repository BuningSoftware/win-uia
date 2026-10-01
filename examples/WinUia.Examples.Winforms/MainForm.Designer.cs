namespace WinUia.Examples.Winforms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblResult = new Label();
            btnClick = new Button();
            txtInput = new TextBox();
            chkToggle = new CheckBox();
            lstItems = new ListBox();
            btnRecreate = new Button();
            btnVolatile = new Button();
            pnlItems = new FlowLayoutPanel();
            btnItemA = new Button();
            btnItemB = new Button();
            btnItemC = new Button();
            btnReverse = new Button();
            btnOpenDialog = new Button();
            tabMain = new TabControl();
            tabPage1 = new TabPage();
            lblTab1 = new Label();
            tabPage2 = new TabPage();
            lblTab2 = new Label();
            pnlItems.SuspendLayout();
            tabMain.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            //
            // lblResult
            //
            lblResult.AutoSize = true;
            lblResult.Location = new Point(12, 12);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(39, 15);
            lblResult.TabIndex = 0;
            lblResult.Text = "Ready";
            //
            // btnClick
            //
            btnClick.Location = new Point(12, 40);
            btnClick.Name = "btnClick";
            btnClick.Size = new Size(120, 23);
            btnClick.TabIndex = 1;
            btnClick.Text = "Click me";
            btnClick.UseVisualStyleBackColor = true;
            btnClick.Click += btnClick_Click;
            //
            // txtInput
            //
            txtInput.Location = new Point(12, 75);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(200, 23);
            txtInput.TabIndex = 2;
            //
            // chkToggle
            //
            chkToggle.AutoSize = true;
            chkToggle.Location = new Point(12, 105);
            chkToggle.Name = "chkToggle";
            chkToggle.Size = new Size(80, 19);
            chkToggle.TabIndex = 3;
            chkToggle.Text = "Toggle me";
            chkToggle.UseVisualStyleBackColor = true;
            //
            // lstItems
            //
            lstItems.FormattingEnabled = true;
            lstItems.ItemHeight = 15;
            lstItems.Items.AddRange(new object[] { "Alpha", "Beta", "Gamma" });
            lstItems.Location = new Point(230, 40);
            lstItems.Name = "lstItems";
            lstItems.SelectionMode = SelectionMode.MultiExtended;
            lstItems.Size = new Size(110, 79);
            lstItems.TabIndex = 4;
            //
            // btnRecreate
            //
            btnRecreate.Location = new Point(12, 140);
            btnRecreate.Name = "btnRecreate";
            btnRecreate.Size = new Size(120, 23);
            btnRecreate.TabIndex = 5;
            btnRecreate.Text = "Recreate";
            btnRecreate.UseVisualStyleBackColor = true;
            btnRecreate.Click += btnRecreate_Click;
            //
            // btnVolatile
            //
            btnVolatile.Location = new Point(140, 140);
            btnVolatile.Name = "btnVolatile";
            btnVolatile.Size = new Size(120, 23);
            btnVolatile.TabIndex = 6;
            btnVolatile.Text = "Volatile 1";
            btnVolatile.UseVisualStyleBackColor = true;
            //
            // pnlItems
            //
            pnlItems.Controls.Add(btnItemA);
            pnlItems.Controls.Add(btnItemB);
            pnlItems.Controls.Add(btnItemC);
            pnlItems.Location = new Point(12, 175);
            pnlItems.Name = "pnlItems";
            pnlItems.Size = new Size(330, 35);
            pnlItems.TabIndex = 7;
            //
            // btnItemA
            //
            btnItemA.Location = new Point(3, 3);
            btnItemA.Name = "btnItemA";
            btnItemA.Size = new Size(100, 23);
            btnItemA.TabIndex = 0;
            btnItemA.Text = "Item A";
            btnItemA.UseVisualStyleBackColor = true;
            //
            // btnItemB
            //
            btnItemB.Location = new Point(109, 3);
            btnItemB.Name = "btnItemB";
            btnItemB.Size = new Size(100, 23);
            btnItemB.TabIndex = 1;
            btnItemB.Text = "Item B";
            btnItemB.UseVisualStyleBackColor = true;
            //
            // btnItemC
            //
            btnItemC.Location = new Point(215, 3);
            btnItemC.Name = "btnItemC";
            btnItemC.Size = new Size(100, 23);
            btnItemC.TabIndex = 2;
            btnItemC.Text = "Item C";
            btnItemC.UseVisualStyleBackColor = true;
            //
            // btnReverse
            //
            btnReverse.Location = new Point(12, 215);
            btnReverse.Name = "btnReverse";
            btnReverse.Size = new Size(120, 23);
            btnReverse.TabIndex = 8;
            btnReverse.Text = "Reverse";
            btnReverse.UseVisualStyleBackColor = true;
            btnReverse.Click += btnReverse_Click;
            //
            // btnOpenDialog
            //
            btnOpenDialog.Location = new Point(140, 215);
            btnOpenDialog.Name = "btnOpenDialog";
            btnOpenDialog.Size = new Size(120, 23);
            btnOpenDialog.TabIndex = 9;
            btnOpenDialog.Text = "Open dialog";
            btnOpenDialog.UseVisualStyleBackColor = true;
            btnOpenDialog.Click += btnOpenDialog_Click;
            //
            // tabMain
            //
            tabMain.Controls.Add(tabPage1);
            tabMain.Controls.Add(tabPage2);
            tabMain.Location = new Point(12, 250);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(336, 80);
            tabMain.TabIndex = 10;
            //
            // tabPage1
            //
            tabPage1.Controls.Add(lblTab1);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(328, 52);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Tab 1";
            tabPage1.UseVisualStyleBackColor = true;
            //
            // lblTab1
            //
            lblTab1.AutoSize = true;
            lblTab1.Location = new Point(6, 6);
            lblTab1.Name = "lblTab1";
            lblTab1.Size = new Size(72, 15);
            lblTab1.TabIndex = 0;
            lblTab1.Text = "tab1 content";
            //
            // tabPage2
            //
            tabPage2.Controls.Add(lblTab2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(328, 52);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Tab 2";
            tabPage2.UseVisualStyleBackColor = true;
            //
            // lblTab2
            //
            lblTab2.AutoSize = true;
            lblTab2.Location = new Point(6, 6);
            lblTab2.Name = "lblTab2";
            lblTab2.Size = new Size(72, 15);
            lblTab2.TabIndex = 0;
            lblTab2.Text = "tab2 content";
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(360, 340);
            Controls.Add(lblResult);
            Controls.Add(btnClick);
            Controls.Add(txtInput);
            Controls.Add(chkToggle);
            Controls.Add(lstItems);
            Controls.Add(btnRecreate);
            Controls.Add(btnVolatile);
            Controls.Add(pnlItems);
            Controls.Add(btnReverse);
            Controls.Add(btnOpenDialog);
            Controls.Add(tabMain);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "WinUia Test App";
            TopMost = true;
            pnlItems.ResumeLayout(false);
            tabMain.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblResult;
        private Button btnClick;
        private TextBox txtInput;
        private CheckBox chkToggle;
        private ListBox lstItems;
        private Button btnRecreate;
        private Button btnVolatile;
        private FlowLayoutPanel pnlItems;
        private Button btnItemA;
        private Button btnItemB;
        private Button btnItemC;
        private Button btnReverse;
        private Button btnOpenDialog;
        private TabControl tabMain;
        private TabPage tabPage1;
        private Label lblTab1;
        private TabPage tabPage2;
        private Label lblTab2;
    }
}
