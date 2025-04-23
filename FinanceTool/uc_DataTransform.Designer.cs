namespace FinanceTool
{
    partial class uc_DataTransform
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            dataGridView_2nd = new DataGridView();
            label1 = new Label();
            label2 = new Label();
            dataGridView_transform = new DataGridView();
            groupBox1 = new GroupBox();
            sum_keyword_table = new DataGridView();
            label5 = new Label();
            decimal_combo = new ComboBox();
            prod_col_check = new CheckBox();
            dept_col_check = new CheckBox();
            label3 = new Label();
            button2 = new Button();
            groupbox2 = new GroupBox();
            modified_keyword = new TextBox();
            label4 = new Label();
            keyword_search_radio2 = new RadioButton();
            keyword_search_radio1 = new RadioButton();
            keyword_search_combo = new ComboBox();
            search_keyword = new TextBox();
            check_all_keyword_list = new CheckBox();
            change_keyword = new Button();
            keyword_search_button = new Button();
            match_keyword_table = new DataGridView();
            button5 = new Button();
            groupBox3 = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView_2nd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_transform).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sum_keyword_table).BeginInit();
            groupbox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)match_keyword_table).BeginInit();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView_2nd
            // 
            dataGridView_2nd.AllowUserToAddRows = false;
            dataGridView_2nd.AllowUserToDeleteRows = false;
            dataGridView_2nd.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_2nd.Location = new Point(24, 104);
            dataGridView_2nd.Name = "dataGridView_2nd";
            dataGridView_2nd.ReadOnly = true;
            dataGridView_2nd.Size = new Size(1171, 378);
            dataGridView_2nd.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 26.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label1.Location = new Point(451, 41);
            label1.Name = "label1";
            label1.Size = new Size(324, 47);
            label1.TabIndex = 1;
            label1.Text = "원본 키워드 데이터";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 26.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label2.Location = new Point(451, 513);
            label2.Name = "label2";
            label2.Size = new Size(324, 47);
            label2.TabIndex = 3;
            label2.Text = "변환 키워드 데이터";
            // 
            // dataGridView_transform
            // 
            dataGridView_transform.AllowUserToAddRows = false;
            dataGridView_transform.AllowUserToDeleteRows = false;
            dataGridView_transform.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_transform.Location = new Point(24, 577);
            dataGridView_transform.Name = "dataGridView_transform";
            dataGridView_transform.ReadOnly = true;
            dataGridView_transform.Size = new Size(1171, 378);
            dataGridView_transform.TabIndex = 2;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(sum_keyword_table);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(decimal_combo);
            groupBox1.Font = new Font("맑은 고딕", 15.75F);
            groupBox1.Location = new Point(1231, 53);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(622, 317);
            groupBox1.TabIndex = 25;
            groupBox1.TabStop = false;
            groupBox1.Text = "키워드 요약";
            // 
            // sum_keyword_table
            // 
            sum_keyword_table.AllowUserToAddRows = false;
            sum_keyword_table.AllowUserToDeleteRows = false;
            sum_keyword_table.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            sum_keyword_table.DefaultCellStyle = dataGridViewCellStyle1;
            sum_keyword_table.EnableHeadersVisualStyles = false;
            sum_keyword_table.Location = new Point(20, 73);
            sum_keyword_table.Name = "sum_keyword_table";
            sum_keyword_table.ReadOnly = true;
            sum_keyword_table.Size = new Size(583, 235);
            sum_keyword_table.TabIndex = 36;
            sum_keyword_table.CellClick += dataGridView_modified_CellClick;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label5.Location = new Point(18, 37);
            label5.Name = "label5";
            label5.Size = new Size(50, 25);
            label5.TabIndex = 35;
            label5.Text = "단위";
            // 
            // decimal_combo
            // 
            decimal_combo.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            decimal_combo.FormattingEnabled = true;
            decimal_combo.Items.AddRange(new object[] { "원", "천원", "백만원", "억원" });
            decimal_combo.Location = new Point(75, 34);
            decimal_combo.Name = "decimal_combo";
            decimal_combo.Size = new Size(90, 33);
            decimal_combo.TabIndex = 24;
            // 
            // prod_col_check
            // 
            prod_col_check.AutoSize = true;
            prod_col_check.Checked = true;
            prod_col_check.CheckState = CheckState.Checked;
            prod_col_check.Font = new Font("맑은 고딕", 14.25F);
            prod_col_check.Location = new Point(227, 50);
            prod_col_check.Name = "prod_col_check";
            prod_col_check.Size = new Size(126, 29);
            prod_col_check.TabIndex = 29;
            prod_col_check.Text = "공급업체명";
            prod_col_check.UseVisualStyleBackColor = true;
            prod_col_check.CheckedChanged += prod_col_check_CheckedChanged;
            // 
            // dept_col_check
            // 
            dept_col_check.AutoSize = true;
            dept_col_check.Checked = true;
            dept_col_check.CheckState = CheckState.Checked;
            dept_col_check.Font = new Font("맑은 고딕", 14.25F);
            dept_col_check.Location = new Point(152, 50);
            dept_col_check.Name = "dept_col_check";
            dept_col_check.Size = new Size(69, 29);
            dept_col_check.TabIndex = 28;
            dept_col_check.Text = "부서";
            dept_col_check.UseVisualStyleBackColor = true;
            dept_col_check.CheckedChanged += dept_col_check_CheckedChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("맑은 고딕", 14.25F);
            label3.Location = new Point(6, 50);
            label3.Name = "label3";
            label3.Size = new Size(140, 25);
            label3.TabIndex = 26;
            label3.Text = "필수 포함 항목";
            // 
            // button2
            // 
            button2.AutoSize = true;
            button2.Font = new Font("맑은 고딕", 14.25F);
            button2.Location = new Point(359, 46);
            button2.Name = "button2";
            button2.Size = new Size(127, 35);
            button2.TabIndex = 26;
            button2.Text = "Cluster 확인";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // groupbox2
            // 
            groupbox2.Controls.Add(modified_keyword);
            groupbox2.Controls.Add(label4);
            groupbox2.Controls.Add(keyword_search_radio2);
            groupbox2.Controls.Add(keyword_search_radio1);
            groupbox2.Controls.Add(keyword_search_combo);
            groupbox2.Controls.Add(search_keyword);
            groupbox2.Controls.Add(check_all_keyword_list);
            groupbox2.Controls.Add(change_keyword);
            groupbox2.Controls.Add(keyword_search_button);
            groupbox2.Controls.Add(match_keyword_table);
            groupbox2.Font = new Font("맑은 고딕", 15.75F);
            groupbox2.Location = new Point(1231, 376);
            groupbox2.Name = "groupbox2";
            groupbox2.Size = new Size(622, 467);
            groupbox2.TabIndex = 30;
            groupbox2.TabStop = false;
            groupbox2.Text = "키워드 변환";
            // 
            // modified_keyword
            // 
            modified_keyword.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            modified_keyword.Location = new Point(354, 118);
            modified_keyword.Name = "modified_keyword";
            modified_keyword.PlaceholderText = "키워드 입력 가능";
            modified_keyword.Size = new Size(169, 33);
            modified_keyword.TabIndex = 34;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label4.Location = new Point(153, 124);
            label4.Name = "label4";
            label4.Size = new Size(178, 25);
            label4.TabIndex = 30;
            label4.Text = "다음 키워드로 변경";
            // 
            // keyword_search_radio2
            // 
            keyword_search_radio2.AutoSize = true;
            keyword_search_radio2.Font = new Font("맑은 고딕", 14.25F);
            keyword_search_radio2.Location = new Point(208, 77);
            keyword_search_radio2.Name = "keyword_search_radio2";
            keyword_search_radio2.Size = new Size(132, 29);
            keyword_search_radio2.TabIndex = 33;
            keyword_search_radio2.Text = "키워드 입력";
            keyword_search_radio2.UseVisualStyleBackColor = true;
            keyword_search_radio2.CheckedChanged += keyword_search_radio2_CheckedChanged;
            // 
            // keyword_search_radio1
            // 
            keyword_search_radio1.AutoSize = true;
            keyword_search_radio1.Checked = true;
            keyword_search_radio1.Font = new Font("맑은 고딕", 14.25F);
            keyword_search_radio1.Location = new Point(208, 34);
            keyword_search_radio1.Name = "keyword_search_radio1";
            keyword_search_radio1.Size = new Size(132, 29);
            keyword_search_radio1.TabIndex = 32;
            keyword_search_radio1.TabStop = true;
            keyword_search_radio1.Text = "키워드 선택";
            keyword_search_radio1.UseVisualStyleBackColor = true;
            keyword_search_radio1.CheckedChanged += keyword_search_radio1_CheckedChanged;
            // 
            // keyword_search_combo
            // 
            keyword_search_combo.Font = new Font("맑은 고딕", 14.25F);
            keyword_search_combo.FormattingEnabled = true;
            keyword_search_combo.Location = new Point(340, 33);
            keyword_search_combo.Name = "keyword_search_combo";
            keyword_search_combo.Size = new Size(260, 33);
            keyword_search_combo.TabIndex = 31;
            keyword_search_combo.Text = "키워드 선택";
            // 
            // search_keyword
            // 
            search_keyword.Enabled = false;
            search_keyword.Font = new Font("맑은 고딕", 14.25F);
            search_keyword.Location = new Point(340, 73);
            search_keyword.Name = "search_keyword";
            search_keyword.PlaceholderText = "키워드 입력";
            search_keyword.Size = new Size(114, 33);
            search_keyword.TabIndex = 30;
            search_keyword.KeyDown += search_keyword_KeyDown;
            // 
            // check_all_keyword_list
            // 
            check_all_keyword_list.AutoSize = true;
            check_all_keyword_list.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            check_all_keyword_list.Location = new Point(18, 120);
            check_all_keyword_list.Name = "check_all_keyword_list";
            check_all_keyword_list.Size = new Size(114, 29);
            check_all_keyword_list.TabIndex = 29;
            check_all_keyword_list.Text = "전체 선택";
            check_all_keyword_list.UseVisualStyleBackColor = true;
            check_all_keyword_list.CheckedChanged += check_all_keyword_list_CheckedChanged;
            // 
            // change_keyword
            // 
            change_keyword.AutoSize = true;
            change_keyword.Font = new Font("맑은 고딕", 14.25F);
            change_keyword.Location = new Point(532, 116);
            change_keyword.Name = "change_keyword";
            change_keyword.Size = new Size(69, 35);
            change_keyword.TabIndex = 26;
            change_keyword.Text = "변환";
            change_keyword.UseVisualStyleBackColor = true;
            change_keyword.Click += change_keyword_Click;
            // 
            // keyword_search_button
            // 
            keyword_search_button.AutoSize = true;
            keyword_search_button.Font = new Font("맑은 고딕", 14.25F);
            keyword_search_button.Location = new Point(531, 74);
            keyword_search_button.Name = "keyword_search_button";
            keyword_search_button.Size = new Size(69, 35);
            keyword_search_button.TabIndex = 24;
            keyword_search_button.Text = "검색";
            keyword_search_button.UseVisualStyleBackColor = true;
            keyword_search_button.Click += keyword_search_button_Click;
            // 
            // match_keyword_table
            // 
            match_keyword_table.AllowUserToAddRows = false;
            match_keyword_table.AllowUserToDeleteRows = false;
            match_keyword_table.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            match_keyword_table.DefaultCellStyle = dataGridViewCellStyle2;
            match_keyword_table.EnableHeadersVisualStyles = false;
            match_keyword_table.Location = new Point(18, 157);
            match_keyword_table.Name = "match_keyword_table";
            match_keyword_table.ReadOnly = true;
            match_keyword_table.Size = new Size(583, 299);
            match_keyword_table.TabIndex = 23;
            match_keyword_table.CellClick += match_keyword_table_CellClick;
            // 
            // button5
            // 
            button5.AutoSize = true;
            button5.Font = new Font("맑은 고딕", 14.25F);
            button5.Location = new Point(492, 45);
            button5.Name = "button5";
            button5.Size = new Size(122, 35);
            button5.TabIndex = 35;
            button5.Text = "완료";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(prod_col_check);
            groupBox3.Controls.Add(button5);
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(button2);
            groupBox3.Controls.Add(dept_col_check);
            groupBox3.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            groupBox3.Location = new Point(1231, 855);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(622, 100);
            groupBox3.TabIndex = 36;
            groupBox3.TabStop = false;
            groupBox3.Text = "Cluster 설정";
            // 
            // uc_DataTransform
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox3);
            Controls.Add(groupbox2);
            Controls.Add(groupBox1);
            Controls.Add(label2);
            Controls.Add(dataGridView_transform);
            Controls.Add(label1);
            Controls.Add(dataGridView_2nd);
            Name = "uc_DataTransform";
            Size = new Size(1904, 1017);
            ((System.ComponentModel.ISupportInitialize)dataGridView_2nd).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_transform).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)sum_keyword_table).EndInit();
            groupbox2.ResumeLayout(false);
            groupbox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)match_keyword_table).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView_2nd;
        private Label label1;
        private Label label2;
        private DataGridView dataGridView_transform;
        private GroupBox groupBox1;
        private CheckBox prod_col_check;
        private CheckBox dept_col_check;
        private Label label3;
        private Button button2;
        private GroupBox groupbox2;
        private CheckBox check_all_keyword_list;
        private Button change_keyword;
        private Button keyword_search_button;
        private DataGridView match_keyword_table;
        private TextBox modified_keyword;
        private Label label4;
        private RadioButton keyword_search_radio2;
        private RadioButton keyword_search_radio1;
        private ComboBox keyword_search_combo;
        private TextBox search_keyword;
        private Button button5;
        private Label label5;
        private ComboBox decimal_combo;
        private GroupBox groupBox3;
        private DataGridView sum_keyword_table;
    }
}
