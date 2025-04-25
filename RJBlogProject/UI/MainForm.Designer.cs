namespace RJBlogProject.UI
{
    partial class MainForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtNaverPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNaverId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtDefaultComment = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtWaitTime = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDefaultBlogId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtSpecificPostUrl = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.chkUseSpecificPost = new System.Windows.Forms.CheckBox();
            this.rbSpecificPost = new System.Windows.Forms.RadioButton();
            this.rbLatestPost = new System.Windows.Forms.RadioButton();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblLogDir = new System.Windows.Forms.Label();
            this.btnOpenLogDir = new System.Windows.Forms.Button();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.chkHeadless = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtNaverPassword);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtNaverId);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(327, 100);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "계정 정보";
            // 
            // txtNaverPassword
            // 
            this.txtNaverPassword.Location = new System.Drawing.Point(106, 56);
            this.txtNaverPassword.Name = "txtNaverPassword";
            this.txtNaverPassword.PasswordChar = '*';
            this.txtNaverPassword.Size = new System.Drawing.Size(200, 20);
            this.txtNaverPassword.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "네이버 비밀번호:";
            // 
            // txtNaverId
            // 
            this.txtNaverId.Location = new System.Drawing.Point(106, 25);
            this.txtNaverId.Name = "txtNaverId";
            this.txtNaverId.Size = new System.Drawing.Size(200, 20);
            this.txtNaverId.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "네이버 아이디:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtDefaultComment);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.txtWaitTime);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.txtDefaultBlogId);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(12, 118);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(327, 152);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "기본 설정";
            // 
            // txtDefaultComment
            // 
            this.txtDefaultComment.Location = new System.Drawing.Point(106, 88);
            this.txtDefaultComment.Multiline = true;
            this.txtDefaultComment.Name = "txtDefaultComment";
            this.txtDefaultComment.Size = new System.Drawing.Size(200, 47);
            this.txtDefaultComment.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "기본 댓글 내용:";
            // 
            // txtWaitTime
            // 
            this.txtWaitTime.Location = new System.Drawing.Point(106, 57);
            this.txtWaitTime.Name = "txtWaitTime";
            this.txtWaitTime.Size = new System.Drawing.Size(200, 20);
            this.txtWaitTime.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "대기 시간(초):";
            // 
            // txtDefaultBlogId
            // 
            this.txtDefaultBlogId.Location = new System.Drawing.Point(106, 25);
            this.txtDefaultBlogId.Name = "txtDefaultBlogId";
            this.txtDefaultBlogId.Size = new System.Drawing.Size(200, 20);
            this.txtDefaultBlogId.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "기본 블로그:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtSpecificPostUrl);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.chkUseSpecificPost);
            this.groupBox3.Controls.Add(this.rbSpecificPost);
            this.groupBox3.Controls.Add(this.rbLatestPost);
            this.groupBox3.Location = new System.Drawing.Point(12, 276);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(327, 131);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "작업 설정";
            // 
            // txtSpecificPostUrl
            // 
            this.txtSpecificPostUrl.Location = new System.Drawing.Point(106, 91);
            this.txtSpecificPostUrl.Name = "txtSpecificPostUrl";
            this.txtSpecificPostUrl.Size = new System.Drawing.Size(200, 20);
            this.txtSpecificPostUrl.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 94);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "특정 글 URL:";
            // 
            // chkUseSpecificPost
            // 
            this.chkUseSpecificPost.AutoSize = true;
            this.chkUseSpecificPost.Location = new System.Drawing.Point(21, 68);
            this.chkUseSpecificPost.Name = "chkUseSpecificPost";
            this.chkUseSpecificPost.Size = new System.Drawing.Size(137, 17);
            this.chkUseSpecificPost.TabIndex = 2;
            this.chkUseSpecificPost.Text = "항상 특정 글 URL 사용";
            this.chkUseSpecificPost.UseVisualStyleBackColor = true;
            // 
            // rbSpecificPost
            // 
            this.rbSpecificPost.AutoSize = true;
            this.rbSpecificPost.Location = new System.Drawing.Point(21, 42);
            this.rbSpecificPost.Name = "rbSpecificPost";
            this.rbSpecificPost.Size = new System.Drawing.Size(185, 17);
            this.rbSpecificPost.TabIndex = 1;
            this.rbSpecificPost.Text = "특정 글의 댓글 작성자 처리 (URL)";
            this.rbSpecificPost.UseVisualStyleBackColor = true;
            this.rbSpecificPost.CheckedChanged += new System.EventHandler(this.rbSpecificPost_CheckedChanged);
            // 
            // rbLatestPost
            // 
            this.rbLatestPost.AutoSize = true;
            this.rbLatestPost.Checked = true;
            this.rbLatestPost.Location = new System.Drawing.Point(21, 19);
            this.rbLatestPost.Name = "rbLatestPost";
            this.rbLatestPost.Size = new System.Drawing.Size(178, 17);
            this.rbLatestPost.TabIndex = 0;
            this.rbLatestPost.TabStop = true;
            this.rbLatestPost.Text = "최신 글의 댓글 작성자 처리";
            this.rbLatestPost.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Location = new System.Drawing.Point(12, 457);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(159, 38);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "시작";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Location = new System.Drawing.Point(177, 457);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(162, 38);
            this.btnSaveSettings.TabIndex = 4;
            this.btnSaveSettings.Text = "설정 저장";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 501);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(327, 23);
            this.progressBar.TabIndex = 5;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.Black;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.ForeColor = System.Drawing.Color.Lime;
            this.txtLog.Location = new System.Drawing.Point(345, 12);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(443, 483);
            this.txtLog.TabIndex = 6;
            // 
            // lblLogDir
            // 
            this.lblLogDir.AutoSize = true;
            this.lblLogDir.Location = new System.Drawing.Point(345, 506);
            this.lblLogDir.Name = "lblLogDir";
            this.lblLogDir.Size = new System.Drawing.Size(97, 13);
            this.lblLogDir.TabIndex = 7;
            this.lblLogDir.Text = "로그 저장 위치: ";
            // 
            // btnOpenLogDir
            // 
            this.btnOpenLogDir.Location = new System.Drawing.Point(624, 501);
            this.btnOpenLogDir.Name = "btnOpenLogDir";
            this.btnOpenLogDir.Size = new System.Drawing.Size(75, 23);
            this.btnOpenLogDir.TabIndex = 8;
            this.btnOpenLogDir.Text = "로그 폴더";
            this.btnOpenLogDir.UseVisualStyleBackColor = true;
            this.btnOpenLogDir.Click += new System.EventHandler(this.btnOpenLogDir_Click);
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(705, 501);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(83, 23);
            this.btnClearLog.TabIndex = 9;
            this.btnClearLog.Text = "로그 지우기";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // chkHeadless
            // 
            this.chkHeadless.AutoSize = true;
            this.chkHeadless.Location = new System.Drawing.Point(12, 413);
            this.chkHeadless.Name = "chkHeadless";
            this.chkHeadless.Size = new System.Drawing.Size(159, 17);
            this.chkHeadless.TabIndex = 10;
            this.chkHeadless.Text = "헤드리스 모드 (브라우저 숨김)";
            this.chkHeadless.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 536);
            this.Controls.Add(this.chkHeadless);
            this.Controls.Add(this.btnClearLog);
            this.Controls.Add(this.btnOpenLogDir);
            this.Controls.Add(this.lblLogDir);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnSaveSettings);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "네이버 블로그 댓글 자동화";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtNaverPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNaverId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtDefaultComment;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtWaitTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDefaultBlogId;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtSpecificPostUrl;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkUseSpecificPost;
        private System.Windows.Forms.RadioButton rbSpecificPost;
        private System.Windows.Forms.RadioButton rbLatestPost;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblLogDir;
        private System.Windows.Forms.Button btnOpenLogDir;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.CheckBox chkHeadless;
    }
}