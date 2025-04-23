using System;
using System.Drawing;
using System.Windows.Forms;

namespace FinanceTool
{
    public class ProcessProgressForm : Form
    {
        private ProgressBar progressBar;
        private Label statusLabel;
        private Form parentForm; // 부모 폼 참조를 저장

        public delegate Task UpdateProgressDelegate(int value, string status=null);
        public UpdateProgressDelegate UpdateProgressHandler;

        public ProcessProgressForm(Form parent = null)
        {
            InitializeComponent();
            UpdateProgressHandler = UpdateProgressValue;
            parentForm = parent; // 부모 폼 참조 저장

           
        }

        private void InitializeComponent()
        {
            // AutoScaling 설정 추가
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // 최소 크기 설정
            this.MinimumSize = new Size(400, 150);

            // 기본 크기 설정
            this.ClientSize = new Size(400, 120);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ControlBox = false;
            this.Text = "처리 중...";

            // Anchor 설정으로 컨트롤 위치 유지
            progressBar = new ProgressBar();
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Location = new Point(20, 20);
            progressBar.Size = new System.Drawing.Size(360, 30);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            statusLabel = new Label();
            statusLabel.Location = new Point(20, 60);
            statusLabel.Size = new System.Drawing.Size(360, 30);
            statusLabel.Text = "데이터 처리 중... (0%)";
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            statusLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            // 마진 여유 확보
            this.Padding = new Padding(10);

            this.Controls.Add(progressBar);
            this.Controls.Add(statusLabel);

            // 폼 크기를 컨트롤에 맞게 자동 조정
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

       

        public async Task UpdateProgressValue(int percentage, string status = null)
        {
            if (InvokeRequired)
            {
                await Invoke(async () => await UpdateProgressValue(percentage, status));
                return;
            }

            progressBar.Value = percentage;
            statusLabel.Text = status ?? $"데이터 처리 중입니다... ({percentage}%)";

            if (percentage >= 100)
            {
                statusLabel.Text = "처리가 완료되었습니다. (100%)";
                await Task.Delay(100); // 0.5초 대기

                this.Close();
            }
        }

        // 프로그레스 폼 표시 및 부모 폼 비활성화를 위한 정적 메서드
       


    }
}