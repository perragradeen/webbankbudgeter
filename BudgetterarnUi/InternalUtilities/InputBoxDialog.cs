using System.ComponentModel;

// ReSharper disable IdentifierTypo

namespace Budgetterarn.InternalUtilities
{
    public partial class InputBoxDialog : Form
    {
        public static string InputBox(string prompt, string title, string defaultValue)
        {
            var ib = new InputBoxDialog
            {
                FormPrompt = prompt,
                FormCaption = title,
                DefaultValue = defaultValue
            };

            ib.ShowDialog();
            var s = ib.InputResponse;
            ib.Close();
            return s;
        }

        private void BtnOKClick(object sender, EventArgs e)
        {
            InputResponse = txtInput.Text;
            Close();
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            Close();
        }

        #region Windows Contols and Constructor

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        private Button btnOK;
        private Label lblPrompt;
        private TextBox txtInput;

        private InputBoxDialog()
        {
            // Required for Windows Form Designer support
            InitializeComponent();
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code

        private Button btnCancel;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblPrompt = new System.Windows.Forms.Label();
            txtInput = new System.Windows.Forms.TextBox();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            SuspendLayout();

            // lblPrompt
            lblPrompt.Anchor =

                System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
                   | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblPrompt.BackColor = System.Drawing.SystemColors.Control;
            lblPrompt.Font = new System.Drawing.Font(
                "Microsoft Sans Serif",
                9.75F,
                System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point,
                0);
            lblPrompt.Location = new System.Drawing.Point(12, 9);
            lblPrompt.Name = "lblPrompt";
            lblPrompt.Size = new System.Drawing.Size(302, 82);
            lblPrompt.TabIndex = 3;

            // txtInput
            txtInput.Location = new System.Drawing.Point(8, 100);
            txtInput.Name = "txtInput";
            txtInput.Size = new System.Drawing.Size(379, 20);
            txtInput.TabIndex = 0;

            // btnOK
            btnOK.Location = new System.Drawing.Point(320, 10);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 25);
            btnOK.TabIndex = 4;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += new System.EventHandler(this.BtnOKClick);

            // btnCancel
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(320, 41);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 25);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += new System.EventHandler(this.BtnCancelClick);

            // InputBoxDialog
            AcceptButton = this.btnOK;
            AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            BackColor = System.Drawing.SystemColors.Control;
            CancelButton = this.btnCancel;
            ClientSize = new System.Drawing.Size(398, 128);
            Controls.Add(this.btnCancel);
            Controls.Add(this.btnOK);
            Controls.Add(this.txtInput);
            Controls.Add(this.lblPrompt);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InputBoxDialog";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "InputBox";
            Load += new System.EventHandler(this.InputBox_Load);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        #region Private Variables

        #endregion

        #region Public Properties

        private string FormCaption { get; set; } = string.Empty;
        private string FormPrompt { get; set; } = string.Empty;
        private string InputResponse { get; set; } = string.Empty;
        private string DefaultValue { get; set; } = string.Empty;

        #endregion

        #region Form and Control Events

        private void InputBox_Load(object sender, EventArgs e)
        {
            txtInput.Text = DefaultValue;
            lblPrompt.Text = FormPrompt;
            Text = FormCaption;
            txtInput.SelectionStart = 0;
            txtInput.SelectionLength = txtInput.Text.Length;
            txtInput.Focus();
        }

        #endregion
    }
}