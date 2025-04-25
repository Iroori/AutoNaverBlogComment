using RJBlogProject.Config;
using RJBlogProject.Common;
using RJBlogProject.Service;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace RJBlogProject.UI
{
    public partial class MainForm : Form
    {
        private AppSettings _settings;
        private CancellationTokenSource _cts;
        private bool _isRunning = false;

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                _settings = AppSettings.Load();
                
                // 설정값을 UI에 적용
                txtNaverId.Text = _settings.NaverId;
                txtNaverPassword.Text = _settings.NaverPassword;
                txtDefaultBlogId.Text = _settings.DefaultBlogId;
                txtWaitTime.Text = _settings.DefaultWaitTimeSeconds.ToString();
                txtDefaultComment.Text = _settings.DefaultCommentText;
                txtSpecificPostUrl.Text = _settings.SpecificPostUrl;
                chkUseSpecificPost.Checked = _settings.UseSpecificPost;
                
                // 라디오 버튼 설정
                if (_settings.UseSpecificPost)
                {
                    rbSpecificPost.Checked = true;
                }
                else
                {
                    rbLatestPost.Checked = true;
                }
                
                // 로그 디렉토리 표시
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                lblLogDir.Text = $"로그 저장 위치: {logDir}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정을 불러오는 중 오류가 발생했습니다: {ex.Message}", "설정 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                // UI 값을 설정에 저장
                _settings.NaverId = txtNaverId.Text.Trim();
                _settings.NaverPassword = txtNaverPassword.Text;
                _settings.DefaultBlogId = txtDefaultBlogId.Text.Trim();
                
                if (int.TryParse(txtWaitTime.Text, out int waitTime))
                {
                    _settings.DefaultWaitTimeSeconds = waitTime;
                }
                
                _settings.DefaultCommentText = txtDefaultComment.Text;
                _settings.SpecificPostUrl = txtSpecificPostUrl.Text.Trim();
                _settings.UseSpecificPost = chkUseSpecificPost.Checked;
                
                // 설정 저장
                _settings.Save();
                LogToUI("설정이 저장되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정을 저장하는 중 오류가 발생했습니다: {ex.Message}", "설정 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                // 실행 중인 작업 중지
                _cts?.Cancel();
                btnStart.Enabled = false;
                LogToUI("작업을 중지 중입니다. 잠시만 기다려주세요...");
                return;
            }
            
            // 입력값 검증
            if (string.IsNullOrEmpty(txtNaverId.Text) || string.IsNullOrEmpty(txtNaverPassword.Text))
            {
                MessageBox.Show("네이버 아이디와 비밀번호를 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (rbSpecificPost.Checked && string.IsNullOrEmpty(txtSpecificPostUrl.Text))
            {
                MessageBox.Show("특정 글 URL을 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // 설정 저장
            SaveSettings();
            
            // UI 상태 변경
            _isRunning = true;
            btnStart.Text = "중지";
            progressBar.Style = ProgressBarStyle.Marquee;
            
            // 작업 시작
            _cts = new CancellationTokenSource();
            try
            {
                await Task.Run(() => RunBlogAutomation(_cts.Token), _cts.Token);
            }
            catch (OperationCanceledException)
            {
                LogToUI("작업이 취소되었습니다.");
            }
            catch (Exception ex)
            {
                LogToUI($"오류 발생: {ex.Message}");
                MessageBox.Show($"실행 중 오류가 발생했습니다: {ex.Message}", "실행 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // UI 상태 원복
                _isRunning = false;
                btnStart.Text = "시작";
                btnStart.Enabled = true;
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
            }
        }

        private void RunBlogAutomation(CancellationToken token)
        {
            LogToUI("블로그 자동화를 시작합니다...");
            
            using (var naverService = new NaverLoginService(_settings))
            {
                try
                {
                    // 웹 드라이버 초기화 및 로그인
                    LogToUI("웹 드라이버를 초기화하고 로그인합니다...");
                    naverService.OpenWebMode(chkHeadless.Checked);
                    
                    if (token.IsCancellationRequested) return;
                    
                    if (!naverService.Login())
                    {
                        LogToUI("로그인에 실패했습니다.");
                        return;
                    }
                    
                    LogToUI("로그인 성공!");
                    
                    // 블로그 댓글 서비스 생성
                    var blogService = new NaverBlogCommentService(naverService.Driver, _settings);
                    
                    if (token.IsCancellationRequested) return;
                    
                    if (rbSpecificPost.Checked)
                    {
                        // 특정 글 처리
                        string postUrl = txtSpecificPostUrl.Text;
                        LogToUI($"특정 글을 처리합니다: {postUrl}");
                        
                        if (blogService.ProcessSpecificPost(postUrl))
                        {
                            LogToUI("특정 글 처리가 완료되었습니다.");
                        }
                        else
                        {
                            LogToUI("특정 글 처리에 실패했습니다.");
                        }
                    }
                    else
                    {
                        // 최신 글 처리
                        LogToUI("블로그로 이동합니다...");
                        naverService.GoToBlog();
                        
                        if (token.IsCancellationRequested) return;
                        
                        LogToUI("최신 글로 이동합니다...");
                        if (blogService.GoToLatestPost())
                        {
                            LogToUI("댓글을 수집 중입니다...");
                            blogService.ReplyToLatestComment();
                            
                            if (token.IsCancellationRequested) return;
                            
                            LogToUI("작업이 완료되었습니다.");
                        }
                        else
                        {
                            LogToUI("최신 글로 이동하지 못했습니다.");
                        }
                    }
                    
                    // 상태 출력 및 실패한 URL 내보내기
                    if (!token.IsCancellationRequested)
                    {
                        blogService.PrintStatus();
                        blogService.ExportFailedUrlsToCsv();
                        LogToUI("실패한 URL을 CSV 파일로 내보냈습니다.");
                    }
                }
                catch (Exception ex)
                {
                    LogToUI($"자동화 실행 중 오류 발생: {ex.Message}");
                    Logger.Error("자동화 실행 중 오류", ex);
                }
                finally
                {
                    Thread.Sleep(1000); // 마지막 작업 완료 대기
                }
            }
            
            LogToUI("블로그 자동화가 종료되었습니다.");
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void rbSpecificPost_CheckedChanged(object sender, EventArgs e)
        {
            txtSpecificPostUrl.Enabled = rbSpecificPost.Checked;
            chkUseSpecificPost.Checked = rbSpecificPost.Checked;
        }

        private void btnOpenLogDir_Click(object sender, EventArgs e)
        {
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            
            System.Diagnostics.Process.Start("explorer.exe", logDir);
        }

        private void LogToUI(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(LogToUI), message);
                return;
            }
            
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
            txtLog.ScrollToCaret();
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }
    }
}
