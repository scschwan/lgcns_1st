using DocumentFormat.OpenXml.Wordprocessing;
using System.Data;

namespace FinanceTool
{
    public partial class Form1 : Form
    {
        public static DataTable excelData = new DataTable();
        private TrialManager trialManager;
        private bool trialYN = false;
        //private bool trialYN = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void SetFormLayout()
        {
            // DPI 변경 이벤트 처리 - 해상도가 변경될 때 호출됨
            this.DpiChanged += (sender, e) => {
                // DPI 변경 시 컨트롤 크기 및 위치 재조정
                this.SuspendLayout();
                ResizeControls();
                this.ResumeLayout();
            };

            // 폼 리사이즈 이벤트 처리
            this.ResizeEnd += (sender, e) => {
                ResizeControls();
            };
        }

        /// <summary>
        /// 컨트롤 크기 및 위치를 재조정하는 메소드
        /// </summary>
        private void ResizeControls()
        {
            // 컨트롤 크기 및 위치 재조정 로직 구현
            // 각 UserControl에 대한 레이아웃 업데이트가 필요한 경우 처리

            // 현재 mainPanel에 추가된 컨트롤들의 레이아웃을 업데이트
            foreach (System.Windows.Forms.Control control in mainPanel.Controls)
            {
                if (control is UserControl)
                {
                    control.Dock = DockStyle.Fill;
                }
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            // 메인 화면 초기화 작업
            mainPanel.Controls.Add(userControlHandler.uc_fileLoad);

            // 폼을 일시적으로 비활성화 (ProcessProgressForm은 별도 폼이므로 영향 없음)
            this.Enabled = false;

            try
            {
                if (trialYN)
                {
                    using (var progress = new ProcessProgressForm())
                    {
                        // 모달리스 창으로 표시
                        progress.Show(this);

                        // 진행 상태 업데이트
                        await progress.UpdateProgressHandler(30, "프로그램 초기화...");
                        await Task.Delay(10);

                        // 평가판 일정확인
                        TrialManager trialManager = new TrialManager();
                        await trialManager.CheckTrial();

                        // 진행 완료 및 폼 닫기
                        await progress.UpdateProgressHandler(100);
                        await Task.Delay(10);
                        progress.Close();
                    }
                }

                // 폼 다시 활성화
                this.Enabled = true;
            }
            catch (Exception ex)
            {
                // 오류 발생 시 처리
                MessageBox.Show($"초기화 중 오류가 발생했습니다: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
               

                // 레이아웃 최종 조정
                ResizeControls();
            }


        }

        // 모든 컨트롤의 Enabled 속성을 설정하는 재귀 메소드


        public void LoadUserControl(UserControl control)
        {
            // 기존 컨트롤 제거
            mainPanel.Controls.Clear();

            // 새 컨트롤 추가 및 레이아웃 설정
            control.Dock = DockStyle.Fill;
            control.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainPanel.Controls.Add(control);

            // 레이아웃 업데이트
            control.Invalidate();
            mainPanel.Invalidate();
        }

        private void fileLoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadUserControl(userControlHandler.uc_fileLoad);
        }

        private void dataPreprocessingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadUserControl(userControlHandler.uc_Preprocessing);
        }

        private void dataAnalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadUserControl(userControlHandler.uc_dataTransform);
        }

        private void classificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadUserControl(userControlHandler.uc_clustering);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 모든 처리가 끝났을 때만 데이터베이스 정리
            DBManager.Instance.ManualDispose();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadUserControl(userControlHandler.uc_classification);
        }
    }
   
}
