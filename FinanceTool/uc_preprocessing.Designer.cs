namespace FinanceTool
{
    partial class uc_preprocessing
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
            dataGridView_target = new DataGridView();
            gb_separator = new GroupBox();
            seper_list_allcheck = new CheckBox();
            dataGridView_seperator = new DataGridView();
            seper_apply_btn = new Button();
            new_seper_word = new TextBox();
            seper_del_btn = new Button();
            seper_add_btn = new Button();
            groupBox2 = new GroupBox();
            remove_list_allcheck = new CheckBox();
            dataGridView_remove = new DataGridView();
            remove_apply_btn = new Button();
            new_remove_word = new TextBox();
            remove_del_btn = new Button();
            remove_add_btn = new Button();
            dataGridView_applied = new DataGridView();
            nlp_groupBox = new GroupBox();
            label1 = new Label();
            label8 = new Label();
            ai_limit_count = new NumericUpDown();
            keyword_model_split = new Button();
            label6 = new Label();
            label9 = new Label();
            label10 = new Label();
            button5 = new Button();
            groupBox1 = new GroupBox();
            remove_1key = new Button();
            keyword_seper_split = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView_target).BeginInit();
            gb_separator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_seperator).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_remove).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_applied).BeginInit();
            nlp_groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ai_limit_count).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView_target
            // 
            dataGridView_target.AllowUserToAddRows = false;
            dataGridView_target.AllowUserToDeleteRows = false;
            dataGridView_target.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_target.Location = new Point(24, 95);
            dataGridView_target.Name = "dataGridView_target";
            dataGridView_target.ReadOnly = true;
            dataGridView_target.Size = new Size(437, 900);
            dataGridView_target.TabIndex = 21;
            // 
            // gb_separator
            // 
            gb_separator.Controls.Add(seper_list_allcheck);
            gb_separator.Controls.Add(dataGridView_seperator);
            gb_separator.Controls.Add(seper_apply_btn);
            gb_separator.Controls.Add(new_seper_word);
            gb_separator.Controls.Add(seper_del_btn);
            gb_separator.Controls.Add(seper_add_btn);
            gb_separator.Font = new Font("맑은 고딕", 16F);
            gb_separator.Location = new Point(1466, 78);
            gb_separator.Name = "gb_separator";
            gb_separator.Size = new Size(410, 269);
            gb_separator.TabIndex = 22;
            gb_separator.TabStop = false;
            gb_separator.Text = "구분자 변환";
            // 
            // seper_list_allcheck
            // 
            seper_list_allcheck.AutoSize = true;
            seper_list_allcheck.Font = new Font("맑은 고딕", 14.25F);
            seper_list_allcheck.Location = new Point(288, 81);
            seper_list_allcheck.Name = "seper_list_allcheck";
            seper_list_allcheck.Size = new Size(114, 29);
            seper_list_allcheck.TabIndex = 44;
            seper_list_allcheck.Text = "전체 선택";
            seper_list_allcheck.UseVisualStyleBackColor = true;
            seper_list_allcheck.CheckedChanged += seper_list_allcheck_CheckedChanged;
            // 
            // dataGridView_seperator
            // 
            dataGridView_seperator.AllowUserToAddRows = false;
            dataGridView_seperator.AllowUserToDeleteRows = false;
            dataGridView_seperator.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_seperator.Location = new Point(16, 77);
            dataGridView_seperator.Name = "dataGridView_seperator";
            dataGridView_seperator.Size = new Size(261, 176);
            dataGridView_seperator.TabIndex = 43;
            // 
            // seper_apply_btn
            // 
            seper_apply_btn.AutoSize = true;
            seper_apply_btn.Font = new Font("맑은 고딕", 14.25F);
            seper_apply_btn.Location = new Point(285, 155);
            seper_apply_btn.Name = "seper_apply_btn";
            seper_apply_btn.Size = new Size(124, 40);
            seper_apply_btn.TabIndex = 29;
            seper_apply_btn.Text = "구분자 변환";
            seper_apply_btn.UseVisualStyleBackColor = true;
            seper_apply_btn.Visible = false;
            seper_apply_btn.Click += btn_apply_Click;
            // 
            // new_seper_word
            // 
            new_seper_word.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            new_seper_word.Location = new Point(14, 35);
            new_seper_word.Name = "new_seper_word";
            new_seper_word.PlaceholderText = "신규 변환 대상 입력";
            new_seper_word.Size = new Size(263, 33);
            new_seper_word.TabIndex = 27;
            new_seper_word.KeyDown += new_seper_word_KeyDown;
            // 
            // seper_del_btn
            // 
            seper_del_btn.AutoSize = true;
            seper_del_btn.Font = new Font("맑은 고딕", 14.25F);
            seper_del_btn.Location = new Point(283, 213);
            seper_del_btn.Name = "seper_del_btn";
            seper_del_btn.Size = new Size(124, 40);
            seper_del_btn.TabIndex = 24;
            seper_del_btn.Text = "항목 제거";
            seper_del_btn.UseVisualStyleBackColor = true;
            seper_del_btn.Click += seper_del_btn_Click;
            // 
            // seper_add_btn
            // 
            seper_add_btn.AutoSize = true;
            seper_add_btn.Font = new Font("맑은 고딕", 14.25F);
            seper_add_btn.Location = new Point(283, 35);
            seper_add_btn.Name = "seper_add_btn";
            seper_add_btn.Size = new Size(124, 40);
            seper_add_btn.TabIndex = 23;
            seper_add_btn.Text = "대상 추가";
            seper_add_btn.UseVisualStyleBackColor = true;
            seper_add_btn.Click += seper_add_btn_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(remove_list_allcheck);
            groupBox2.Controls.Add(dataGridView_remove);
            groupBox2.Controls.Add(remove_apply_btn);
            groupBox2.Controls.Add(new_remove_word);
            groupBox2.Controls.Add(remove_del_btn);
            groupBox2.Controls.Add(remove_add_btn);
            groupBox2.Font = new Font("맑은 고딕", 16F);
            groupBox2.Location = new Point(1466, 367);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(410, 269);
            groupBox2.TabIndex = 27;
            groupBox2.TabStop = false;
            groupBox2.Text = "불용어 제거";
            // 
            // remove_list_allcheck
            // 
            remove_list_allcheck.AutoSize = true;
            remove_list_allcheck.Font = new Font("맑은 고딕", 14.25F);
            remove_list_allcheck.Location = new Point(285, 77);
            remove_list_allcheck.Name = "remove_list_allcheck";
            remove_list_allcheck.Size = new Size(114, 29);
            remove_list_allcheck.TabIndex = 48;
            remove_list_allcheck.Text = "전체 선택";
            remove_list_allcheck.UseVisualStyleBackColor = true;
            remove_list_allcheck.CheckedChanged += remove_list_allcheck_CheckedChanged;
            // 
            // dataGridView_remove
            // 
            dataGridView_remove.AllowUserToAddRows = false;
            dataGridView_remove.AllowUserToDeleteRows = false;
            dataGridView_remove.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_remove.Location = new Point(16, 77);
            dataGridView_remove.Name = "dataGridView_remove";
            dataGridView_remove.Size = new Size(261, 176);
            dataGridView_remove.TabIndex = 45;
            // 
            // remove_apply_btn
            // 
            remove_apply_btn.AutoSize = true;
            remove_apply_btn.Font = new Font("맑은 고딕", 14.25F);
            remove_apply_btn.Location = new Point(283, 112);
            remove_apply_btn.Name = "remove_apply_btn";
            remove_apply_btn.Size = new Size(124, 40);
            remove_apply_btn.TabIndex = 47;
            remove_apply_btn.Text = "불용어 제거";
            remove_apply_btn.UseVisualStyleBackColor = true;
            remove_apply_btn.Visible = false;
            remove_apply_btn.Click += remove_apply_btn_Click;
            // 
            // new_remove_word
            // 
            new_remove_word.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            new_remove_word.Location = new Point(16, 35);
            new_remove_word.Name = "new_remove_word";
            new_remove_word.PlaceholderText = "신규 불용어 대상 입력";
            new_remove_word.Size = new Size(261, 33);
            new_remove_word.TabIndex = 25;
            new_remove_word.KeyDown += tb_remove_KeyDown;
            // 
            // remove_del_btn
            // 
            remove_del_btn.AutoSize = true;
            remove_del_btn.Font = new Font("맑은 고딕", 14.25F);
            remove_del_btn.Location = new Point(283, 213);
            remove_del_btn.Name = "remove_del_btn";
            remove_del_btn.Size = new Size(124, 40);
            remove_del_btn.TabIndex = 46;
            remove_del_btn.Text = "항목 제거";
            remove_del_btn.UseVisualStyleBackColor = true;
            remove_del_btn.Click += remove_del_btn_Click;
            // 
            // remove_add_btn
            // 
            remove_add_btn.AutoSize = true;
            remove_add_btn.Font = new Font("맑은 고딕", 14.25F);
            remove_add_btn.Location = new Point(283, 30);
            remove_add_btn.Name = "remove_add_btn";
            remove_add_btn.Size = new Size(124, 40);
            remove_add_btn.TabIndex = 45;
            remove_add_btn.Text = "대상 추가";
            remove_add_btn.UseVisualStyleBackColor = true;
            remove_add_btn.Click += remove_add_btn_Click;
            // 
            // dataGridView_applied
            // 
            dataGridView_applied.AllowUserToAddRows = false;
            dataGridView_applied.AllowUserToDeleteRows = false;
            dataGridView_applied.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_applied.Location = new Point(499, 95);
            dataGridView_applied.Name = "dataGridView_applied";
            dataGridView_applied.ReadOnly = true;
            dataGridView_applied.Size = new Size(924, 900);
            dataGridView_applied.TabIndex = 28;
            // 
            // nlp_groupBox
            // 
            nlp_groupBox.Controls.Add(label1);
            nlp_groupBox.Controls.Add(label8);
            nlp_groupBox.Controls.Add(ai_limit_count);
            nlp_groupBox.Controls.Add(keyword_model_split);
            nlp_groupBox.Controls.Add(label6);
            nlp_groupBox.Font = new Font("맑은 고딕", 16F);
            nlp_groupBox.Location = new Point(1466, 756);
            nlp_groupBox.Name = "nlp_groupBox";
            nlp_groupBox.Size = new Size(410, 189);
            nlp_groupBox.TabIndex = 29;
            nlp_groupBox.TabStop = false;
            nlp_groupBox.Text = "NLP 기반 키워드 추출";
            nlp_groupBox.Visible = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = Color.IndianRed;
            label1.Location = new Point(32, 60);
            label1.Name = "label1";
            label1.Size = new Size(234, 17);
            label1.TabIndex = 37;
            label1.Text = "AI가 추가적으로 키워드를 분할합니다.";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("맑은 고딕", 14.25F);
            label8.Location = new Point(99, 90);
            label8.Name = "label8";
            label8.Size = new Size(249, 25);
            label8.TabIndex = 36;
            label8.Text = "글자 이상 키워드 자동 분할";
            // 
            // ai_limit_count
            // 
            ai_limit_count.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            ai_limit_count.Location = new Point(16, 88);
            ai_limit_count.Name = "ai_limit_count";
            ai_limit_count.Size = new Size(77, 33);
            ai_limit_count.TabIndex = 35;
            ai_limit_count.TextAlign = HorizontalAlignment.Center;
            ai_limit_count.Value = new decimal(new int[] { 4, 0, 0, 0 });
            ai_limit_count.ValueChanged += ai_limit_count_ValueChanged;
            // 
            // keyword_model_split
            // 
            keyword_model_split.AutoSize = true;
            keyword_model_split.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            keyword_model_split.Location = new Point(136, 130);
            keyword_model_split.Name = "keyword_model_split";
            keyword_model_split.Size = new Size(141, 40);
            keyword_model_split.TabIndex = 34;
            keyword_model_split.Text = "키워드 추출";
            keyword_model_split.UseVisualStyleBackColor = true;
            keyword_model_split.Click += keyword_model_split_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label6.ForeColor = Color.IndianRed;
            label6.Location = new Point(14, 40);
            label6.Name = "label6";
            label6.Size = new Size(212, 17);
            label6.TabIndex = 31;
            label6.Text = "※ 구분자 기반으로 키워드 추출 후";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("맑은 고딕", 26.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label9.Location = new Point(825, 45);
            label9.Name = "label9";
            label9.Size = new Size(289, 47);
            label9.TabIndex = 48;
            label9.Text = "키워드 추출 결과";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("맑은 고딕", 26.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label10.Location = new Point(84, 45);
            label10.Name = "label10";
            label10.Size = new Size(289, 47);
            label10.TabIndex = 47;
            label10.Text = "키워드 추출 대상";
            // 
            // button5
            // 
            button5.AutoSize = true;
            button5.Font = new Font("맑은 고딕", 14.25F);
            button5.Location = new Point(1749, 960);
            button5.Name = "button5";
            button5.Size = new Size(122, 35);
            button5.TabIndex = 38;
            button5.Text = "완료";
            button5.UseVisualStyleBackColor = true;
            button5.Click += btn_complete_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(remove_1key);
            groupBox1.Controls.Add(keyword_seper_split);
            groupBox1.Font = new Font("맑은 고딕", 16F);
            groupBox1.Location = new Point(1466, 650);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(410, 100);
            groupBox1.TabIndex = 49;
            groupBox1.TabStop = false;
            groupBox1.Text = "구분자 기반 키워드 추출";
            // 
            // remove_1key
            // 
            remove_1key.AutoSize = true;
            remove_1key.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            remove_1key.Location = new Point(220, 43);
            remove_1key.Name = "remove_1key";
            remove_1key.Size = new Size(179, 40);
            remove_1key.TabIndex = 39;
            remove_1key.Text = "1글자 키워드 제거";
            remove_1key.UseVisualStyleBackColor = true;
            remove_1key.Click += remove_1key_Click;
            // 
            // keyword_seper_split
            // 
            keyword_seper_split.AutoSize = true;
            keyword_seper_split.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            keyword_seper_split.Location = new Point(14, 43);
            keyword_seper_split.Name = "keyword_seper_split";
            keyword_seper_split.Size = new Size(141, 40);
            keyword_seper_split.TabIndex = 38;
            keyword_seper_split.Text = "키워드 추출";
            keyword_seper_split.UseVisualStyleBackColor = true;
            keyword_seper_split.Click += keyword_seper_split_Click;
            // 
            // uc_preprocessing
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox1);
            Controls.Add(label9);
            Controls.Add(button5);
            Controls.Add(label10);
            Controls.Add(nlp_groupBox);
            Controls.Add(dataGridView_applied);
            Controls.Add(groupBox2);
            Controls.Add(gb_separator);
            Controls.Add(dataGridView_target);
            Name = "uc_preprocessing";
            Size = new Size(1904, 1017);
            ((System.ComponentModel.ISupportInitialize)dataGridView_target).EndInit();
            gb_separator.ResumeLayout(false);
            gb_separator.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_seperator).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_remove).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_applied).EndInit();
            nlp_groupBox.ResumeLayout(false);
            nlp_groupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ai_limit_count).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private DataGridView dataGridView_target;
        private GroupBox gb_separator;
        private Button seper_del_btn;
        private Button seper_add_btn;
        private GroupBox groupBox2;
        private TextBox new_remove_word;
        private DataGridView dataGridView_applied;
        private TextBox new_seper_word;
        private Button seper_apply_btn;
        private GroupBox nlp_groupBox;
        private Label label6;
        private Button keyword_model_split;
        private Label label8;
        private NumericUpDown ai_limit_count;
        private Label label9;
        private Label label10;
        private Button button5;
        private DataGridView dataGridView_seperator;
        private CheckBox seper_list_allcheck;
        private CheckBox remove_list_allcheck;
        private DataGridView dataGridView_remove;
        private Button remove_apply_btn;
        private Button remove_del_btn;
        private Button remove_add_btn;
        private Label label1;
        private GroupBox groupBox1;
        private Button remove_1key;
        private Button keyword_seper_split;
    }
}
