namespace FinanceTool
{
    partial class uc_FileLoad
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
            lbl_filename = new Label();
            btn_selectFile = new Button();
            label2 = new Label();
            dataGridView_target = new DataGridView();
            cmb_target = new ComboBox();
            label1 = new Label();
            cmb_money = new ComboBox();
            label3 = new Label();
            dataGridView_process = new DataGridView();
            groupBox2 = new GroupBox();
            delete_search_keyword = new TextBox();
            delete_search_button = new Button();
            restore_del_data_btn = new Button();
            del_data_list_allcheck = new CheckBox();
            dataGridView_delete_data = new DataGridView();
            delete_data_btn = new Button();
            stand_col_combo = new ComboBox();
            label4 = new Label();
            sub_acc_col_combo = new ComboBox();
            label5 = new Label();
            dept_col_combo = new ComboBox();
            label6 = new Label();
            btn_complete = new Button();
            dataGridView2 = new DataGridView();
            groupBox3 = new GroupBox();
            label7 = new Label();
            del_col_list_allcheck = new CheckBox();
            dataGridView_delete_col = new DataGridView();
            restore_col_btn = new Button();
            groupBox1 = new GroupBox();
            label8 = new Label();
            label9 = new Label();
            lbl_pagination2 = new Label();
            num_pageNumber = new NumericUpDown();
            lbl_pagination = new Label();
            lbl_pageSizeText = new Label();
            cmb_pageSize = new ComboBox();
            btn_prevPage = new Button();
            btn_nextPage = new Button();
            label10 = new Label();
            prod_col_combo = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView_target).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_process).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_delete_data).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_delete_col).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)num_pageNumber).BeginInit();
            SuspendLayout();
            // 
            // lbl_filename
            // 
            lbl_filename.AutoSize = true;
            lbl_filename.Font = new Font("맑은 고딕", 20F);
            lbl_filename.Location = new Point(146, 40);
            lbl_filename.Name = "lbl_filename";
            lbl_filename.Size = new Size(511, 37);
            lbl_filename.TabIndex = 0;
            lbl_filename.Text = "Excel 파일을 Upload 해주세요(.xls,xlsx) : ";
            // 
            // btn_selectFile
            // 
            btn_selectFile.AutoSize = true;
            btn_selectFile.Font = new Font("맑은 고딕", 18F);
            btn_selectFile.Location = new Point(12, 38);
            btn_selectFile.Name = "btn_selectFile";
            btn_selectFile.Size = new Size(128, 47);
            btn_selectFile.TabIndex = 1;
            btn_selectFile.Text = "파일 선택";
            btn_selectFile.UseVisualStyleBackColor = true;
            btn_selectFile.Click += btn_selectFile_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 14F);
            label2.Location = new Point(6, 41);
            label2.Name = "label2";
            label2.Size = new Size(132, 25);
            label2.TabIndex = 3;
            label2.Text = "제거 열 선택 :";
            // 
            // dataGridView_target
            // 
            dataGridView_target.AllowUserToAddRows = false;
            dataGridView_target.AllowUserToDeleteRows = false;
            dataGridView_target.AllowUserToResizeRows = false;
            dataGridView_target.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_target.Location = new Point(12, 174);
            dataGridView_target.Name = "dataGridView_target";
            dataGridView_target.ReadOnly = true;
            dataGridView_target.Size = new Size(668, 776);
            dataGridView_target.TabIndex = 13;
            dataGridView_target.CellClick += dataGridView_target_CellClick;
            dataGridView_target.SelectionChanged += dataGridView_target_SelectionChanged;
            // 
            // cmb_target
            // 
            cmb_target.Font = new Font("맑은 고딕", 14F);
            cmb_target.FormattingEnabled = true;
            cmb_target.Location = new Point(157, 252);
            cmb_target.Name = "cmb_target";
            cmb_target.Size = new Size(307, 33);
            cmb_target.TabIndex = 14;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 14F);
            label1.Location = new Point(26, 255);
            label1.Name = "label1";
            label1.Size = new Size(87, 25);
            label1.TabIndex = 15;
            label1.Text = "타겟 열 :";
            // 
            // cmb_money
            // 
            cmb_money.Font = new Font("맑은 고딕", 14F);
            cmb_money.FormattingEnabled = true;
            cmb_money.Location = new Point(157, 202);
            cmb_money.Name = "cmb_money";
            cmb_money.Size = new Size(307, 33);
            cmb_money.TabIndex = 18;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("맑은 고딕", 14F);
            label3.Location = new Point(26, 205);
            label3.Name = "label3";
            label3.Size = new Size(87, 25);
            label3.TabIndex = 19;
            label3.Text = "금액 열 :";
            // 
            // dataGridView_process
            // 
            dataGridView_process.AllowUserToAddRows = false;
            dataGridView_process.AllowUserToDeleteRows = false;
            dataGridView_process.AllowUserToResizeRows = false;
            dataGridView_process.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_process.Location = new Point(701, 174);
            dataGridView_process.Name = "dataGridView_process";
            dataGridView_process.ReadOnly = true;
            dataGridView_process.Size = new Size(668, 776);
            dataGridView_process.TabIndex = 20;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(delete_search_keyword);
            groupBox2.Controls.Add(delete_search_button);
            groupBox2.Controls.Add(restore_del_data_btn);
            groupBox2.Controls.Add(del_data_list_allcheck);
            groupBox2.Controls.Add(dataGridView_delete_data);
            groupBox2.Controls.Add(delete_data_btn);
            groupBox2.Controls.Add(stand_col_combo);
            groupBox2.Controls.Add(label4);
            groupBox2.Font = new Font("맑은 고딕", 16F);
            groupBox2.Location = new Point(1402, 372);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(477, 272);
            groupBox2.TabIndex = 16;
            groupBox2.TabStop = false;
            groupBox2.Text = "데이터 삭제";
            // 
            // delete_search_keyword
            // 
            delete_search_keyword.Font = new Font("맑은 고딕", 14.25F);
            delete_search_keyword.Location = new Point(153, 77);
            delete_search_keyword.Name = "delete_search_keyword";
            delete_search_keyword.PlaceholderText = "검색 키워드 입력";
            delete_search_keyword.Size = new Size(169, 33);
            delete_search_keyword.TabIndex = 46;
            delete_search_keyword.KeyDown += delete_search_keyword_KeyDown;
            // 
            // delete_search_button
            // 
            delete_search_button.AutoSize = true;
            delete_search_button.Font = new Font("맑은 고딕", 14.25F);
            delete_search_button.Location = new Point(343, 77);
            delete_search_button.Name = "delete_search_button";
            delete_search_button.Size = new Size(122, 35);
            delete_search_button.TabIndex = 45;
            delete_search_button.Text = "검색";
            delete_search_button.UseVisualStyleBackColor = true;
            delete_search_button.Click += delete_search_button_Click;
            // 
            // restore_del_data_btn
            // 
            restore_del_data_btn.AutoSize = true;
            restore_del_data_btn.Font = new Font("맑은 고딕", 14F);
            restore_del_data_btn.Location = new Point(334, 181);
            restore_del_data_btn.Name = "restore_del_data_btn";
            restore_del_data_btn.Size = new Size(131, 35);
            restore_del_data_btn.TabIndex = 44;
            restore_del_data_btn.Text = "데이터 원복";
            restore_del_data_btn.UseVisualStyleBackColor = true;
            restore_del_data_btn.Click += restore_del_data_btn_Click;
            // 
            // del_data_list_allcheck
            // 
            del_data_list_allcheck.AutoSize = true;
            del_data_list_allcheck.Font = new Font("맑은 고딕", 14.25F);
            del_data_list_allcheck.Location = new Point(337, 119);
            del_data_list_allcheck.Name = "del_data_list_allcheck";
            del_data_list_allcheck.Size = new Size(114, 29);
            del_data_list_allcheck.TabIndex = 44;
            del_data_list_allcheck.Text = "전체 선택";
            del_data_list_allcheck.UseVisualStyleBackColor = true;
            del_data_list_allcheck.CheckedChanged += del_data_list_allcheck_CheckedChanged;
            // 
            // dataGridView_delete_data
            // 
            dataGridView_delete_data.AllowUserToAddRows = false;
            dataGridView_delete_data.AllowUserToDeleteRows = false;
            dataGridView_delete_data.AllowUserToResizeRows = false;
            dataGridView_delete_data.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_delete_data.Location = new Point(6, 119);
            dataGridView_delete_data.Name = "dataGridView_delete_data";
            dataGridView_delete_data.ReadOnly = true;
            dataGridView_delete_data.Size = new Size(316, 138);
            dataGridView_delete_data.TabIndex = 42;
            // 
            // delete_data_btn
            // 
            delete_data_btn.AutoSize = true;
            delete_data_btn.Font = new Font("맑은 고딕", 14F);
            delete_data_btn.Location = new Point(334, 222);
            delete_data_btn.Name = "delete_data_btn";
            delete_data_btn.Size = new Size(131, 35);
            delete_data_btn.TabIndex = 14;
            delete_data_btn.Text = "데이터 삭제";
            delete_data_btn.UseVisualStyleBackColor = true;
            delete_data_btn.Click += delete_data_btn_Click;
            // 
            // stand_col_combo
            // 
            stand_col_combo.Font = new Font("맑은 고딕", 14F);
            stand_col_combo.FormattingEnabled = true;
            stand_col_combo.Location = new Point(144, 38);
            stand_col_combo.Name = "stand_col_combo";
            stand_col_combo.Size = new Size(320, 33);
            stand_col_combo.TabIndex = 2;
            stand_col_combo.SelectedIndexChanged += stand_col_combo_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("맑은 고딕", 14F);
            label4.Location = new Point(6, 41);
            label4.Name = "label4";
            label4.Size = new Size(132, 25);
            label4.TabIndex = 3;
            label4.Text = "기준 열 선택 :";
            // 
            // sub_acc_col_combo
            // 
            sub_acc_col_combo.Font = new Font("맑은 고딕", 14F);
            sub_acc_col_combo.FormattingEnabled = true;
            sub_acc_col_combo.Location = new Point(157, 54);
            sub_acc_col_combo.Name = "sub_acc_col_combo";
            sub_acc_col_combo.Size = new Size(307, 33);
            sub_acc_col_combo.TabIndex = 23;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("맑은 고딕", 14F);
            label5.Location = new Point(26, 57);
            label5.Name = "label5";
            label5.Size = new Size(87, 25);
            label5.TabIndex = 24;
            label5.Text = "세목 열 :";
            // 
            // dept_col_combo
            // 
            dept_col_combo.Font = new Font("맑은 고딕", 14F);
            dept_col_combo.FormattingEnabled = true;
            dept_col_combo.Location = new Point(157, 104);
            dept_col_combo.Name = "dept_col_combo";
            dept_col_combo.Size = new Size(308, 33);
            dept_col_combo.TabIndex = 21;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("맑은 고딕", 14F);
            label6.Location = new Point(26, 107);
            label6.Name = "label6";
            label6.Size = new Size(87, 25);
            label6.TabIndex = 22;
            label6.Text = "부서 열 :";
            // 
            // btn_complete
            // 
            btn_complete.AutoSize = true;
            btn_complete.Font = new Font("맑은 고딕", 14.25F);
            btn_complete.Location = new Point(343, 300);
            btn_complete.Name = "btn_complete";
            btn_complete.Size = new Size(122, 35);
            btn_complete.TabIndex = 38;
            btn_complete.Text = "완료";
            btn_complete.UseVisualStyleBackColor = true;
            btn_complete.Click += btn_complete_Click;
            // 
            // dataGridView2
            // 
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new Point(6, 77);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.Size = new Size(458, 180);
            dataGridView2.TabIndex = 41;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label7);
            groupBox3.Controls.Add(del_col_list_allcheck);
            groupBox3.Controls.Add(dataGridView_delete_col);
            groupBox3.Controls.Add(restore_col_btn);
            groupBox3.Font = new Font("맑은 고딕", 16F);
            groupBox3.Location = new Point(1402, 93);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(477, 268);
            groupBox3.TabIndex = 43;
            groupBox3.TabStop = false;
            groupBox3.Text = "제거 열 설정";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label7.ForeColor = Color.IndianRed;
            label7.Location = new Point(6, 32);
            label7.Name = "label7";
            label7.Size = new Size(267, 17);
            label7.TabIndex = 44;
            label7.Text = "※ 선택한 열 정보만 출력하도록 지원합니다.";
            // 
            // del_col_list_allcheck
            // 
            del_col_list_allcheck.AutoSize = true;
            del_col_list_allcheck.Font = new Font("맑은 고딕", 14.25F);
            del_col_list_allcheck.Location = new Point(337, 54);
            del_col_list_allcheck.Name = "del_col_list_allcheck";
            del_col_list_allcheck.Size = new Size(114, 29);
            del_col_list_allcheck.TabIndex = 43;
            del_col_list_allcheck.Text = "전체 선택";
            del_col_list_allcheck.UseVisualStyleBackColor = true;
            del_col_list_allcheck.CheckedChanged += del_col_list_allcheck_CheckedChanged;
            // 
            // dataGridView_delete_col
            // 
            dataGridView_delete_col.AllowUserToAddRows = false;
            dataGridView_delete_col.AllowUserToDeleteRows = false;
            dataGridView_delete_col.AllowUserToResizeRows = false;
            dataGridView_delete_col.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_delete_col.Location = new Point(6, 54);
            dataGridView_delete_col.Name = "dataGridView_delete_col";
            dataGridView_delete_col.Size = new Size(316, 203);
            dataGridView_delete_col.TabIndex = 42;
            // 
            // restore_col_btn
            // 
            restore_col_btn.AutoSize = true;
            restore_col_btn.Font = new Font("맑은 고딕", 14F);
            restore_col_btn.Location = new Point(333, 222);
            restore_col_btn.Name = "restore_col_btn";
            restore_col_btn.Size = new Size(131, 35);
            restore_col_btn.TabIndex = 14;
            restore_col_btn.Text = "선택 열 적용";
            restore_col_btn.UseVisualStyleBackColor = true;
            restore_col_btn.Click += restore_col_btn_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(prod_col_combo);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(cmb_target);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(cmb_money);
            groupBox1.Controls.Add(sub_acc_col_combo);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(dept_col_combo);
            groupBox1.Controls.Add(btn_complete);
            groupBox1.Font = new Font("맑은 고딕", 16F);
            groupBox1.Location = new Point(1402, 650);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(477, 347);
            groupBox1.TabIndex = 44;
            groupBox1.TabStop = false;
            groupBox1.Text = "필수 항목 설정";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("맑은 고딕", 26.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label8.Location = new Point(217, 120);
            label8.Name = "label8";
            label8.Size = new Size(207, 47);
            label8.TabIndex = 45;
            label8.Text = "원본 데이터";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("맑은 고딕", 26.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label9.Location = new Point(919, 120);
            label9.Name = "label9";
            label9.Size = new Size(207, 47);
            label9.TabIndex = 46;
            label9.Text = "가공 데이터";
            // 
            // lbl_pagination2
            // 
            lbl_pagination2.AutoSize = true;
            lbl_pagination2.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lbl_pagination2.Location = new Point(704, 961);
            lbl_pagination2.Name = "lbl_pagination2";
            lbl_pagination2.Size = new Size(118, 25);
            lbl_pagination2.TabIndex = 48;
            lbl_pagination2.Text = "/ 0 (총 0 행)";
            // 
            // num_pageNumber
            // 
            num_pageNumber.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            num_pageNumber.Location = new Point(646, 958);
            num_pageNumber.Name = "num_pageNumber";
            num_pageNumber.Size = new Size(52, 33);
            num_pageNumber.TabIndex = 49;
            // 
            // lbl_pagination
            // 
            lbl_pagination.AutoSize = true;
            lbl_pagination.Font = new Font("맑은 고딕", 14F);
            lbl_pagination.Location = new Point(553, 960);
            lbl_pagination.Name = "lbl_pagination";
            lbl_pagination.Size = new Size(80, 25);
            lbl_pagination.TabIndex = 25;
            lbl_pagination.Text = "페이지 :";
            // 
            // lbl_pageSizeText
            // 
            lbl_pageSizeText.AutoSize = true;
            lbl_pageSizeText.Font = new Font("맑은 고딕", 14F);
            lbl_pageSizeText.Location = new Point(15, 961);
            lbl_pageSizeText.Name = "lbl_pageSizeText";
            lbl_pageSizeText.Size = new Size(125, 25);
            lbl_pageSizeText.TabIndex = 50;
            lbl_pageSizeText.Text = "페이지 크기 :";
            // 
            // cmb_pageSize
            // 
            cmb_pageSize.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            cmb_pageSize.FormattingEnabled = true;
            cmb_pageSize.Location = new Point(146, 958);
            cmb_pageSize.Name = "cmb_pageSize";
            cmb_pageSize.Size = new Size(121, 33);
            cmb_pageSize.TabIndex = 51;
            // 
            // btn_prevPage
            // 
            btn_prevPage.AutoSize = true;
            btn_prevPage.Font = new Font("맑은 고딕", 14.25F);
            btn_prevPage.Location = new Point(461, 955);
            btn_prevPage.Name = "btn_prevPage";
            btn_prevPage.Size = new Size(86, 35);
            btn_prevPage.TabIndex = 52;
            btn_prevPage.Text = "◀ 이전";
            btn_prevPage.UseVisualStyleBackColor = true;
            btn_prevPage.Click += btn_prevPage_Click;
            // 
            // btn_nextPage
            // 
            btn_nextPage.AutoSize = true;
            btn_nextPage.Font = new Font("맑은 고딕", 14.25F);
            btn_nextPage.Location = new Point(891, 956);
            btn_nextPage.Name = "btn_nextPage";
            btn_nextPage.Size = new Size(86, 35);
            btn_nextPage.TabIndex = 53;
            btn_nextPage.Text = "다음 ▶";
            btn_nextPage.UseVisualStyleBackColor = true;
            btn_nextPage.Click += btn_nextPage_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("맑은 고딕", 14F);
            label10.Location = new Point(26, 158);
            label10.Name = "label10";
            label10.Size = new Size(125, 25);
            label10.TabIndex = 40;
            label10.Text = "공급업체 열 :";
            // 
            // prod_col_combo
            // 
            prod_col_combo.Font = new Font("맑은 고딕", 14F);
            prod_col_combo.FormattingEnabled = true;
            prod_col_combo.Location = new Point(157, 155);
            prod_col_combo.Name = "prod_col_combo";
            prod_col_combo.Size = new Size(308, 33);
            prod_col_combo.TabIndex = 39;
            // 
            // uc_FileLoad
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btn_nextPage);
            Controls.Add(btn_prevPage);
            Controls.Add(cmb_pageSize);
            Controls.Add(lbl_pageSizeText);
            Controls.Add(lbl_pagination);
            Controls.Add(num_pageNumber);
            Controls.Add(lbl_pagination2);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(groupBox1);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(dataGridView_process);
            Controls.Add(dataGridView_target);
            Controls.Add(btn_selectFile);
            Controls.Add(lbl_filename);
            Name = "uc_FileLoad";
            Size = new Size(1904, 1017);
            Load += uc_FileLoad_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView_target).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_process).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_delete_data).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_delete_col).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)num_pageNumber).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_filename;
        private Button btn_selectFile;
        
        private Label label2;
        private GroupBox groupBox1;
        
        private DataGridView dataGridView_target;
        
        private ComboBox cmb_target;
        private Label label1;
        private ComboBox cmb_money;
        private Label label3;
        private DataGridView dataGridView_process;
        private GroupBox groupBox2;
        
        private Button delete_data_btn;
        private ComboBox stand_col_combo;
        private Label label4;
        private ComboBox sub_acc_col_combo;
        private Label label5;
        private ComboBox dept_col_combo;
        private Label label6;
        private Button button7;
        private Button button6;
        private Button btn_complete;
        private DataGridView dataGridView2;
        private DataGridView dataGridView_delete_data;
        private GroupBox groupBox3;
        private DataGridView dataGridView_delete_col;
        private Button restore_col_btn;
        private CheckBox del_col_list_allcheck;
        private CheckBox del_data_list_allcheck;
        private Button restore_del_data_btn;
        private Label label8;
        private Label label9;
        private Label lbl_pagination2;
        private NumericUpDown num_pageNumber;
        private Label lbl_pagination;
        private Label lbl_pageSizeText;
        private ComboBox cmb_pageSize;
        private Button btn_prevPage;
        private Button btn_nextPage;
        private TextBox delete_search_keyword;
        private Button delete_search_button;
        private Label label7;
        private Label label10;
        private ComboBox prod_col_combo;
    }
}
