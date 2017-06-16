namespace RequestReply.Sender
{
    partial class frmSender
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSend = new System.Windows.Forms.Button();
            this.txtMessageText = new System.Windows.Forms.TextBox();
            this.drpTotalMessagesToSend = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.drpBatchSize = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(295, 38);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "Send!";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMessageText
            // 
            this.txtMessageText.Location = new System.Drawing.Point(97, 40);
            this.txtMessageText.Name = "txtMessageText";
            this.txtMessageText.Size = new System.Drawing.Size(193, 20);
            this.txtMessageText.TabIndex = 1;
            // 
            // drpTotalMessagesToSend
            // 
            this.drpTotalMessagesToSend.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.drpTotalMessagesToSend.FormattingEnabled = true;
            this.drpTotalMessagesToSend.Location = new System.Drawing.Point(97, 13);
            this.drpTotalMessagesToSend.Name = "drpTotalMessagesToSend";
            this.drpTotalMessagesToSend.Size = new System.Drawing.Size(55, 21);
            this.drpTotalMessagesToSend.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Send a total of ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(158, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "messages, with batches of";
            // 
            // drpBatchSize
            // 
            this.drpBatchSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.drpBatchSize.FormattingEnabled = true;
            this.drpBatchSize.Location = new System.Drawing.Point(296, 13);
            this.drpBatchSize.Name = "drpBatchSize";
            this.drpBatchSize.Size = new System.Drawing.Size(55, 21);
            this.drpBatchSize.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(357, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "in each message.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 47);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "with this text:";
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 67);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(600, 285);
            this.txtLog.TabIndex = 8;
            // 
            // frmSender
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 364);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.drpBatchSize);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.drpTotalMessagesToSend);
            this.Controls.Add(this.txtMessageText);
            this.Controls.Add(this.btnSend);
            this.Name = "frmSender";
            this.Text = "MassTransit Message Sender";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtMessageText;
        private System.Windows.Forms.ComboBox drpTotalMessagesToSend;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox drpBatchSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtLog;
    }
}