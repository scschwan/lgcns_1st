namespace FinanceTool
{
    partial class uc_Classification
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridView_origin = new DataGridView();
            dataGridView_keyword = new DataGridView();
            dataGridView_classify = new DataGridView();
            label1 = new Label();
            label2 = new Label();
            groupBox3 = new GroupBox();
            label7 = new Label();
            del_col_list_allcheck = new CheckBox();
            dataGridView_delete_col2 = new DataGridView();
            restore_col_btn = new Button();
            button5 = new Button();
            btn_nextPage = new Button();
            btn_prevPage = new Button();
            cmb_pageSize = new ComboBox();
            lbl_pageSizeText = new Label();
            lbl_pagination = new Label();
            num_pageNumber = new NumericUpDown();
            lbl_pagination2 = new Label();
            groupBox1 = new GroupBox();
            label3 = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView_origin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_keyword).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_classify).BeginInit();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_delete_col2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)num_pageNumber).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView_origin
            // 
            dataGridView_origin.AllowUserToAddRows = false;
            dataGridView_origin.AllowUserToDeleteRows = false;
            dataGridView_origin.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_origin.Location = new Point(16, 77);
            dataGridView_origin.Name = "dataGridView_origin";
            dataGridView_origin.ReadOnly = true;
            dataGridView_origin.Size = new Size(627, 869);
            dataGridView_origin.TabIndex = 0;
            // 
            // dataGridView_keyword
            // 
            dataGridView_keyword.AllowUserToAddRows = false;
            dataGridView_keyword.AllowUserToDeleteRows = false;
            dataGridView_keyword.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_keyword.Location = new Point(664, 77);
            dataGridView_keyword.Name = "dataGridView_keyword";
            dataGridView_keyword.ReadOnly = true;
            dataGridView_keyword.Size = new Size(725, 869);
            dataGridView_keyword.TabIndex = 1;
            // 
            // dataGridView_classify
            // 
            dataGridView_classify.AllowUserToAddRows = false;
            dataGridView_classify.AllowUserToDeleteRows = false;
            dataGridView_classify.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_classify.Location = new Point(10, 57);
            dataGridView_classify.Name = "dataGridView_classify";
            dataGridView_classify.Size = new Size(454, 516);
            dataGridView_classify.TabIndex = 2;
            dataGridView_classify.CellClick += dataGridView_classify_CellClick;
            dataGridView_classify.CellValueChanged += dataGridView_classify_CellValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 26.25F);
            label1.Location = new Point(222, 27);
            label1.Name = "label1";
            label1.Size = new Size(207, 47);
            label1.TabIndex = 3;
            label1.Text = "원본 테이블";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 26.25F);
            label2.Location = new Point(931, 27);
            label2.Name = "label2";
            label2.Size = new Size(202, 47);
            label2.TabIndex = 4;
            label2.Text = "Export 결과";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label7);
            groupBox3.Controls.Add(del_col_list_allcheck);
            groupBox3.Controls.Add(dataGridView_delete_col2);
            groupBox3.Controls.Add(restore_col_btn);
            groupBox3.Font = new Font("맑은 고딕", 16F);
            groupBox3.Location = new Point(1409, 646);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(477, 300);
            groupBox3.TabIndex = 44;
            groupBox3.TabStop = false;
            groupBox3.Text = "제거 열 설정";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label7.ForeColor = Color.IndianRed;
            label7.Location = new Point(6, 46);
            label7.Name = "label7";
            label7.Size = new Size(267, 17);
            label7.TabIndex = 45;
            label7.Text = "※ 선택한 열 정보만 출력하도록 지원합니다.";
            // 
            // del_col_list_allcheck
            // 
            del_col_list_allcheck.AutoSize = true;
            del_col_list_allcheck.Font = new Font("맑은 고딕", 14.25F);
            del_col_list_allcheck.Location = new Point(337, 88);
            del_col_list_allcheck.Name = "del_col_list_allcheck";
            del_col_list_allcheck.Size = new Size(114, 29);
            del_col_list_allcheck.TabIndex = 43;
            del_col_list_allcheck.Text = "전체 선택";
            del_col_list_allcheck.UseVisualStyleBackColor = true;
            del_col_list_allcheck.CheckedChanged += del_col_list_allcheck_CheckedChanged;
            // 
            // dataGridView_delete_col2
            // 
            dataGridView_delete_col2.AllowUserToAddRows = false;
            dataGridView_delete_col2.AllowUserToDeleteRows = false;
            dataGridView_delete_col2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_delete_col2.Location = new Point(6, 77);
            dataGridView_delete_col2.Name = "dataGridView_delete_col2";
            dataGridView_delete_col2.Size = new Size(316, 217);
            dataGridView_delete_col2.TabIndex = 42;
            // 
            // restore_col_btn
            // 
            restore_col_btn.AutoSize = true;
            restore_col_btn.Font = new Font("맑은 고딕", 14F);
            restore_col_btn.Location = new Point(333, 259);
            restore_col_btn.Name = "restore_col_btn";
            restore_col_btn.Size = new Size(131, 35);
            restore_col_btn.TabIndex = 14;
            restore_col_btn.Text = "선택 열 적용";
            restore_col_btn.UseVisualStyleBackColor = true;
            restore_col_btn.Click += restore_col_btn_Click;
            // 
            // button5
            // 
            button5.AutoSize = true;
            button5.Font = new Font("맑은 고딕", 14.25F);
            button5.Location = new Point(1764, 947);
            button5.Name = "button5";
            button5.Size = new Size(122, 35);
            button5.TabIndex = 46;
            button5.Text = "Excel 저장";
            button5.UseVisualStyleBackColor = true;
            button5.Click += btn_save_excel_Click;
            // 
            // btn_nextPage
            // 
            btn_nextPage.AutoSize = true;
            btn_nextPage.Font = new Font("맑은 고딕", 14.25F);
            btn_nextPage.Location = new Point(958, 953);
            btn_nextPage.Name = "btn_nextPage";
            btn_nextPage.Size = new Size(86, 35);
            btn_nextPage.TabIndex = 60;
            btn_nextPage.Text = "다음 ▶";
            btn_nextPage.UseVisualStyleBackColor = true;
            btn_nextPage.Click += btn_nextPage_Click;
            // 
            // btn_prevPage
            // 
            btn_prevPage.AutoSize = true;
            btn_prevPage.Font = new Font("맑은 고딕", 14.25F);
            btn_prevPage.Location = new Point(528, 952);
            btn_prevPage.Name = "btn_prevPage";
            btn_prevPage.Size = new Size(86, 35);
            btn_prevPage.TabIndex = 59;
            btn_prevPage.Text = "◀ 이전";
            btn_prevPage.UseVisualStyleBackColor = true;
            btn_prevPage.Click += btn_prevPage_Click;
            // 
            // cmb_pageSize
            // 
            cmb_pageSize.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            cmb_pageSize.FormattingEnabled = true;
            cmb_pageSize.Location = new Point(147, 954);
            cmb_pageSize.Name = "cmb_pageSize";
            cmb_pageSize.Size = new Size(121, 33);
            cmb_pageSize.TabIndex = 58;
            // 
            // lbl_pageSizeText
            // 
            lbl_pageSizeText.AutoSize = true;
            lbl_pageSizeText.Font = new Font("맑은 고딕", 14F);
            lbl_pageSizeText.Location = new Point(16, 957);
            lbl_pageSizeText.Name = "lbl_pageSizeText";
            lbl_pageSizeText.Size = new Size(125, 25);
            lbl_pageSizeText.TabIndex = 57;
            lbl_pageSizeText.Text = "페이지 크기 :";
            // 
            // lbl_pagination
            // 
            lbl_pagination.AutoSize = true;
            lbl_pagination.Font = new Font("맑은 고딕", 14F);
            lbl_pagination.Location = new Point(620, 957);
            lbl_pagination.Name = "lbl_pagination";
            lbl_pagination.Size = new Size(80, 25);
            lbl_pagination.TabIndex = 54;
            lbl_pagination.Text = "페이지 :";
            // 
            // num_pageNumber
            // 
            num_pageNumber.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            num_pageNumber.Location = new Point(713, 955);
            num_pageNumber.Name = "num_pageNumber";
            num_pageNumber.Size = new Size(52, 33);
            num_pageNumber.TabIndex = 56;
            // 
            // lbl_pagination2
            // 
            lbl_pagination2.AutoSize = true;
            lbl_pagination2.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lbl_pagination2.Location = new Point(771, 958);
            lbl_pagination2.Name = "lbl_pagination2";
            lbl_pagination2.Size = new Size(118, 25);
            lbl_pagination2.TabIndex = 55;
            lbl_pagination2.Text = "/ 0 (총 0 행)";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(dataGridView_classify);
            groupBox1.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            groupBox1.Location = new Point(1409, 43);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(477, 591);
            groupBox1.TabIndex = 61;
            groupBox1.TabStop = false;
            groupBox1.Text = "Clustering 결과";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label3.ForeColor = Color.IndianRed;
            label3.Location = new Point(6, 34);
            label3.Name = "label3";
            label3.Size = new Size(249, 17);
            label3.TabIndex = 48;
            label3.Text = "※ 클러스터명은 직접 수정이 가능합니다.";
            // 
            // uc_Classification
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox1);
            Controls.Add(btn_nextPage);
            Controls.Add(btn_prevPage);
            Controls.Add(cmb_pageSize);
            Controls.Add(lbl_pageSizeText);
            Controls.Add(lbl_pagination);
            Controls.Add(num_pageNumber);
            Controls.Add(lbl_pagination2);
            Controls.Add(button5);
            Controls.Add(groupBox3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(dataGridView_keyword);
            Controls.Add(dataGridView_origin);
            Name = "uc_Classification";
            Size = new Size(1904, 1017);
            ((System.ComponentModel.ISupportInitialize)dataGridView_origin).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_keyword).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_classify).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_delete_col2).EndInit();
            ((System.ComponentModel.ISupportInitialize)num_pageNumber).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView_origin;
        private DataGridView dataGridView_keyword;
        private DataGridView dataGridView_classify;
        private Label label1;
        private Label label2;
        private GroupBox groupBox3;
        private CheckBox del_col_list_allcheck;
        private DataGridView dataGridView_delete_col2;
        private Button restore_col_btn;
        private Button button5;
        private Button btn_nextPage;
        private Button btn_prevPage;
        private ComboBox cmb_pageSize;
        private Label lbl_pageSizeText;
        private Label lbl_pagination;
        private NumericUpDown num_pageNumber;
        private Label lbl_pagination2;
        private GroupBox groupBox1;
        private Label label7;
        private Label label3;
    }
}
