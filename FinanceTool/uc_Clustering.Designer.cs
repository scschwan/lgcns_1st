namespace FinanceTool
{
    partial class uc_Clustering
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
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            complete_btn = new Button();
            groupbox2 = new GroupBox();
            union_cluster_btn = new Button();
            label7 = new Label();
            check_search_radio2 = new RadioButton();
            check_search_radio1 = new RadioButton();
            check_search_combo = new ComboBox();
            check_search_keyword = new TextBox();
            merge_cancel_button = new Button();
            check_search_button = new Button();
            merge_check_table = new DataGridView();
            groupBox1 = new GroupBox();
            groupBox5 = new GroupBox();
            keyword_radio1 = new RadioButton();
            keyword_radio2 = new RadioButton();
            uncluster_count_money = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            excep_search_checkbox = new CheckBox();
            equal_search_checkbox = new CheckBox();
            uncluster_count = new Label();
            except_keyword = new TextBox();
            cluster_count = new Label();
            merge_addon_btn = new Button();
            merge_search_radio2 = new RadioButton();
            label5 = new Label();
            decimal_combo = new ComboBox();
            merge_search_radio1 = new RadioButton();
            merge_keyword_combo = new ComboBox();
            merge_search_keyword = new TextBox();
            merge_all_check = new CheckBox();
            button1 = new Button();
            merge_search_button = new Button();
            merge_cluster_table = new DataGridView();
            groupBox3 = new GroupBox();
            label1 = new Label();
            groupBox4 = new GroupBox();
            dataGridView_recoman_keyword = new DataGridView();
            new_reco_word = new TextBox();
            reco_del_btn = new Button();
            reco_add_btn = new Button();
            gb_separator = new GroupBox();
            dataGridView_lv1 = new DataGridView();
            new_lv1_word = new TextBox();
            lv1_del_btn = new Button();
            lv1_add_btn = new Button();
            dataGridView_modified = new DataGridView();
            button2 = new Button();
            groupbox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)merge_check_table).BeginInit();
            groupBox1.SuspendLayout();
            groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)merge_cluster_table).BeginInit();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_recoman_keyword).BeginInit();
            gb_separator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_lv1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_modified).BeginInit();
            SuspendLayout();
            // 
            // complete_btn
            // 
            complete_btn.AutoSize = true;
            complete_btn.Font = new Font("맑은 고딕", 14.25F);
            complete_btn.Location = new Point(507, 421);
            complete_btn.Name = "complete_btn";
            complete_btn.Size = new Size(122, 35);
            complete_btn.TabIndex = 45;
            complete_btn.Text = "완료";
            complete_btn.UseVisualStyleBackColor = true;
            complete_btn.Click += complete_btn_Click;
            // 
            // groupbox2
            // 
            groupbox2.Controls.Add(button2);
            groupbox2.Controls.Add(union_cluster_btn);
            groupbox2.Controls.Add(label7);
            groupbox2.Controls.Add(complete_btn);
            groupbox2.Controls.Add(check_search_radio2);
            groupbox2.Controls.Add(check_search_radio1);
            groupbox2.Controls.Add(check_search_combo);
            groupbox2.Controls.Add(check_search_keyword);
            groupbox2.Controls.Add(merge_cancel_button);
            groupbox2.Controls.Add(check_search_button);
            groupbox2.Controls.Add(merge_check_table);
            groupbox2.Font = new Font("맑은 고딕", 15.75F);
            groupbox2.Location = new Point(1232, 526);
            groupbox2.Name = "groupbox2";
            groupbox2.Size = new Size(635, 462);
            groupbox2.TabIndex = 44;
            groupbox2.TabStop = false;
            groupbox2.Text = "Clustering 병합 결과 확인";
            // 
            // union_cluster_btn
            // 
            union_cluster_btn.AutoSize = true;
            union_cluster_btn.Font = new Font("맑은 고딕", 14.25F);
            union_cluster_btn.Location = new Point(6, 91);
            union_cluster_btn.Name = "union_cluster_btn";
            union_cluster_btn.Size = new Size(195, 35);
            union_cluster_btn.TabIndex = 48;
            union_cluster_btn.Text = "선택 항목 간 병합";
            union_cluster_btn.UseVisualStyleBackColor = true;
            union_cluster_btn.Click += union_cluster_btn_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label7.ForeColor = Color.IndianRed;
            label7.Location = new Point(6, 433);
            label7.Name = "label7";
            label7.Size = new Size(249, 17);
            label7.TabIndex = 47;
            label7.Text = "※ 클러스터명은 직접 수정이 가능합니다.";
            // 
            // check_search_radio2
            // 
            check_search_radio2.AutoSize = true;
            check_search_radio2.Font = new Font("맑은 고딕", 14.25F);
            check_search_radio2.Location = new Point(224, 97);
            check_search_radio2.Name = "check_search_radio2";
            check_search_radio2.Size = new Size(132, 29);
            check_search_radio2.TabIndex = 33;
            check_search_radio2.Text = "키워드 입력";
            check_search_radio2.UseVisualStyleBackColor = true;
            check_search_radio2.CheckedChanged += check_search_radio2_CheckedChanged;
            // 
            // check_search_radio1
            // 
            check_search_radio1.AutoSize = true;
            check_search_radio1.Checked = true;
            check_search_radio1.Font = new Font("맑은 고딕", 14.25F);
            check_search_radio1.Location = new Point(224, 50);
            check_search_radio1.Name = "check_search_radio1";
            check_search_radio1.Size = new Size(132, 29);
            check_search_radio1.TabIndex = 32;
            check_search_radio1.TabStop = true;
            check_search_radio1.Text = "키워드 선택";
            check_search_radio1.UseVisualStyleBackColor = true;
            check_search_radio1.CheckedChanged += check_search_radio1_CheckedChanged;
            // 
            // check_search_combo
            // 
            check_search_combo.Font = new Font("맑은 고딕", 14.25F);
            check_search_combo.FormattingEnabled = true;
            check_search_combo.Location = new Point(362, 49);
            check_search_combo.Name = "check_search_combo";
            check_search_combo.Size = new Size(260, 33);
            check_search_combo.TabIndex = 31;
            check_search_combo.Text = "키워드 선택";
            // 
            // check_search_keyword
            // 
            check_search_keyword.Enabled = false;
            check_search_keyword.Font = new Font("맑은 고딕", 14.25F);
            check_search_keyword.Location = new Point(362, 94);
            check_search_keyword.Name = "check_search_keyword";
            check_search_keyword.PlaceholderText = "키워드 입력";
            check_search_keyword.Size = new Size(133, 33);
            check_search_keyword.TabIndex = 30;
            check_search_keyword.KeyDown += check_search_keyword_KeyDown;
            // 
            // merge_cancel_button
            // 
            merge_cancel_button.AutoSize = true;
            merge_cancel_button.Font = new Font("맑은 고딕", 14.25F);
            merge_cancel_button.Location = new Point(261, 421);
            merge_cancel_button.Name = "merge_cancel_button";
            merge_cancel_button.Size = new Size(195, 35);
            merge_cancel_button.TabIndex = 26;
            merge_cancel_button.Text = "선택 항목 병합 해제";
            merge_cancel_button.UseVisualStyleBackColor = true;
            merge_cancel_button.Click += merge_cancel_button_Click;
            // 
            // check_search_button
            // 
            check_search_button.AutoSize = true;
            check_search_button.Font = new Font("맑은 고딕", 14.25F);
            check_search_button.Location = new Point(559, 94);
            check_search_button.Name = "check_search_button";
            check_search_button.Size = new Size(63, 35);
            check_search_button.TabIndex = 24;
            check_search_button.Text = "검색";
            check_search_button.UseVisualStyleBackColor = true;
            check_search_button.Click += check_search_button_Click;
            // 
            // merge_check_table
            // 
            merge_check_table.AllowUserToAddRows = false;
            merge_check_table.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            merge_check_table.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            merge_check_table.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            merge_check_table.DefaultCellStyle = dataGridViewCellStyle2;
            merge_check_table.EnableHeadersVisualStyles = false;
            merge_check_table.Location = new Point(6, 140);
            merge_check_table.Name = "merge_check_table";
            merge_check_table.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("돋움체", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            merge_check_table.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            merge_check_table.Size = new Size(621, 275);
            merge_check_table.TabIndex = 23;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(groupBox5);
            groupBox1.Controls.Add(uncluster_count_money);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(excep_search_checkbox);
            groupBox1.Controls.Add(equal_search_checkbox);
            groupBox1.Controls.Add(uncluster_count);
            groupBox1.Controls.Add(except_keyword);
            groupBox1.Controls.Add(cluster_count);
            groupBox1.Controls.Add(merge_addon_btn);
            groupBox1.Controls.Add(merge_search_radio2);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(decimal_combo);
            groupBox1.Controls.Add(merge_search_radio1);
            groupBox1.Controls.Add(merge_keyword_combo);
            groupBox1.Controls.Add(merge_search_keyword);
            groupBox1.Controls.Add(merge_all_check);
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(merge_search_button);
            groupBox1.Controls.Add(merge_cluster_table);
            groupBox1.Font = new Font("맑은 고딕", 15.75F);
            groupBox1.Location = new Point(3, 15);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1201, 973);
            groupBox1.TabIndex = 42;
            groupBox1.TabStop = false;
            groupBox1.Text = "Clustering 병합";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(keyword_radio1);
            groupBox5.Controls.Add(keyword_radio2);
            groupBox5.Location = new Point(880, 23);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(300, 33);
            groupBox5.TabIndex = 54;
            groupBox5.TabStop = false;
            // 
            // keyword_radio1
            // 
            keyword_radio1.AutoSize = true;
            keyword_radio1.Checked = true;
            keyword_radio1.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            keyword_radio1.Location = new Point(6, 4);
            keyword_radio1.Name = "keyword_radio1";
            keyword_radio1.Size = new Size(132, 29);
            keyword_radio1.TabIndex = 52;
            keyword_radio1.TabStop = true;
            keyword_radio1.Text = "키워드 검색";
            keyword_radio1.UseVisualStyleBackColor = true;
            keyword_radio1.CheckedChanged += keyword_radio1_CheckedChanged;
            // 
            // keyword_radio2
            // 
            keyword_radio2.AutoSize = true;
            keyword_radio2.Font = new Font("맑은 고딕", 14.25F);
            keyword_radio2.Location = new Point(144, 4);
            keyword_radio2.Name = "keyword_radio2";
            keyword_radio2.Size = new Size(151, 29);
            keyword_radio2.TabIndex = 53;
            keyword_radio2.Text = "공급업체 검색";
            keyword_radio2.UseVisualStyleBackColor = true;
            // 
            // uncluster_count_money
            // 
            uncluster_count_money.AutoSize = true;
            uncluster_count_money.Location = new Point(33, 116);
            uncluster_count_money.Name = "uncluster_count_money";
            uncluster_count_money.Size = new Size(179, 30);
            uncluster_count_money.TabIndex = 51;
            uncluster_count_money.Text = "미병합 합산금액 :";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label4.ForeColor = Color.IndianRed;
            label4.Location = new Point(390, 97);
            label4.Name = "label4";
            label4.Size = new Size(140, 17);
            label4.TabIndex = 50;
            label4.Text = "   추가할 수 있습니다.";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label3.ForeColor = Color.IndianRed;
            label3.Location = new Point(390, 72);
            label3.Name = "label3";
            label3.Size = new Size(331, 17);
            label3.TabIndex = 49;
            label3.Text = "※ 제외 항목 입력 시 , 를 활용하여 제외 키워드 항목을";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = Color.IndianRed;
            label2.Location = new Point(390, 48);
            label2.Name = "label2";
            label2.Size = new Size(345, 17);
            label2.TabIndex = 48;
            label2.Text = "※ 검색어 입력 시 , 를 활용하여 AND 검색이 가능합니다.";
            // 
            // excep_search_checkbox
            // 
            excep_search_checkbox.AutoSize = true;
            excep_search_checkbox.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            excep_search_checkbox.Location = new Point(742, 153);
            excep_search_checkbox.Name = "excep_search_checkbox";
            excep_search_checkbox.Size = new Size(170, 29);
            excep_search_checkbox.TabIndex = 48;
            excep_search_checkbox.Text = "검색 제외 항목 :";
            excep_search_checkbox.UseVisualStyleBackColor = true;
            excep_search_checkbox.CheckedChanged += excep_search_checkbox_CheckedChanged;
            // 
            // equal_search_checkbox
            // 
            equal_search_checkbox.AutoSize = true;
            equal_search_checkbox.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            equal_search_checkbox.Location = new Point(532, 152);
            equal_search_checkbox.Name = "equal_search_checkbox";
            equal_search_checkbox.Size = new Size(204, 29);
            equal_search_checkbox.TabIndex = 47;
            equal_search_checkbox.Text = "검색 조건 완전 일치";
            equal_search_checkbox.UseVisualStyleBackColor = true;
            equal_search_checkbox.CheckedChanged += equal_search_checkbox_CheckedChanged;
            // 
            // uncluster_count
            // 
            uncluster_count.AutoSize = true;
            uncluster_count.Location = new Point(33, 79);
            uncluster_count.Name = "uncluster_count";
            uncluster_count.Size = new Size(159, 30);
            uncluster_count.TabIndex = 46;
            uncluster_count.Text = "미병합 Cluster :";
            // 
            // except_keyword
            // 
            except_keyword.Enabled = false;
            except_keyword.Font = new Font("맑은 고딕", 14.25F);
            except_keyword.Location = new Point(937, 150);
            except_keyword.Name = "except_keyword";
            except_keyword.PlaceholderText = "제외 항목 입력";
            except_keyword.Size = new Size(172, 33);
            except_keyword.TabIndex = 45;
            except_keyword.KeyDown += except_keyword_KeyDown;
            // 
            // cluster_count
            // 
            cluster_count.AutoSize = true;
            cluster_count.Location = new Point(33, 38);
            cluster_count.Name = "cluster_count";
            cluster_count.Size = new Size(74, 30);
            cluster_count.TabIndex = 43;
            cluster_count.Text = "행 수 :";
            // 
            // merge_addon_btn
            // 
            merge_addon_btn.AutoSize = true;
            merge_addon_btn.Font = new Font("맑은 고딕", 14.25F);
            merge_addon_btn.Location = new Point(222, 149);
            merge_addon_btn.Name = "merge_addon_btn";
            merge_addon_btn.Size = new Size(122, 35);
            merge_addon_btn.TabIndex = 42;
            merge_addon_btn.Text = "추가 병합";
            merge_addon_btn.UseVisualStyleBackColor = true;
            merge_addon_btn.Click += merge_addon_btn_Click;
            // 
            // merge_search_radio2
            // 
            merge_search_radio2.AutoSize = true;
            merge_search_radio2.Font = new Font("맑은 고딕", 14.25F);
            merge_search_radio2.Location = new Point(742, 101);
            merge_search_radio2.Name = "merge_search_radio2";
            merge_search_radio2.Size = new Size(132, 29);
            merge_search_radio2.TabIndex = 41;
            merge_search_radio2.Text = "검색어 입력";
            merge_search_radio2.UseVisualStyleBackColor = true;
            merge_search_radio2.CheckedChanged += merge_search_radio2_CheckedChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label5.Location = new Point(350, 154);
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
            decimal_combo.Location = new Point(406, 150);
            decimal_combo.Name = "decimal_combo";
            decimal_combo.Size = new Size(69, 33);
            decimal_combo.TabIndex = 24;
            decimal_combo.SelectedIndexChanged += decimal_combo_SelectedIndexChanged;
            // 
            // merge_search_radio1
            // 
            merge_search_radio1.AutoSize = true;
            merge_search_radio1.Checked = true;
            merge_search_radio1.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            merge_search_radio1.Location = new Point(742, 63);
            merge_search_radio1.Name = "merge_search_radio1";
            merge_search_radio1.Size = new Size(132, 29);
            merge_search_radio1.TabIndex = 40;
            merge_search_radio1.TabStop = true;
            merge_search_radio1.Text = "검색어 선택";
            merge_search_radio1.UseVisualStyleBackColor = true;
            merge_search_radio1.CheckedChanged += merge_search_radio1_CheckedChanged;
            // 
            // merge_keyword_combo
            // 
            merge_keyword_combo.Font = new Font("맑은 고딕", 14.25F);
            merge_keyword_combo.FormattingEnabled = true;
            merge_keyword_combo.Location = new Point(880, 62);
            merge_keyword_combo.Name = "merge_keyword_combo";
            merge_keyword_combo.Size = new Size(297, 33);
            merge_keyword_combo.TabIndex = 39;
            merge_keyword_combo.Text = "검색어 선택";
            // 
            // merge_search_keyword
            // 
            merge_search_keyword.Enabled = false;
            merge_search_keyword.Font = new Font("맑은 고딕", 14.25F);
            merge_search_keyword.Location = new Point(880, 100);
            merge_search_keyword.Name = "merge_search_keyword";
            merge_search_keyword.PlaceholderText = "검색 키워드 입력";
            merge_search_keyword.Size = new Size(229, 33);
            merge_search_keyword.TabIndex = 38;
            merge_search_keyword.KeyDown += merge_search_keyword_KeyDown;
            // 
            // merge_all_check
            // 
            merge_all_check.AutoSize = true;
            merge_all_check.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            merge_all_check.Location = new Point(33, 155);
            merge_all_check.Name = "merge_all_check";
            merge_all_check.Size = new Size(114, 29);
            merge_all_check.TabIndex = 37;
            merge_all_check.Text = "전체 선택";
            merge_all_check.UseVisualStyleBackColor = true;
            merge_all_check.CheckedChanged += merge_all_check_CheckedChanged;
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.Font = new Font("맑은 고딕", 14.25F);
            button1.Location = new Point(153, 149);
            button1.Name = "button1";
            button1.Size = new Size(63, 35);
            button1.TabIndex = 36;
            button1.Text = "병합";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // merge_search_button
            // 
            merge_search_button.AutoSize = true;
            merge_search_button.Font = new Font("맑은 고딕", 14.25F);
            merge_search_button.Location = new Point(1115, 102);
            merge_search_button.Name = "merge_search_button";
            merge_search_button.Size = new Size(63, 35);
            merge_search_button.TabIndex = 35;
            merge_search_button.Text = "검색";
            merge_search_button.UseVisualStyleBackColor = true;
            merge_search_button.Click += merge_search_button_Click;
            // 
            // merge_cluster_table
            // 
            merge_cluster_table.AllowUserToAddRows = false;
            merge_cluster_table.AllowUserToDeleteRows = false;
            merge_cluster_table.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            merge_cluster_table.EnableHeadersVisualStyles = false;
            merge_cluster_table.Location = new Point(33, 199);
            merge_cluster_table.Name = "merge_cluster_table";
            merge_cluster_table.ReadOnly = true;
            merge_cluster_table.Size = new Size(1143, 727);
            merge_cluster_table.TabIndex = 34;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label1);
            groupBox3.Controls.Add(groupBox4);
            groupBox3.Controls.Add(gb_separator);
            groupBox3.Controls.Add(dataGridView_modified);
            groupBox3.Font = new Font("맑은 고딕", 15.75F);
            groupBox3.Location = new Point(1232, 47);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(635, 472);
            groupBox3.TabIndex = 43;
            groupBox3.TabStop = false;
            groupBox3.Text = "검색 키워드 추천";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.Location = new Point(18, 41);
            label1.Name = "label1";
            label1.Size = new Size(159, 25);
            label1.TabIndex = 46;
            label1.Text = "상위 키워드 목록";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(dataGridView_recoman_keyword);
            groupBox4.Controls.Add(new_reco_word);
            groupBox4.Controls.Add(reco_del_btn);
            groupBox4.Controls.Add(reco_add_btn);
            groupBox4.Font = new Font("맑은 고딕", 16F);
            groupBox4.Location = new Point(314, 236);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(290, 226);
            groupBox4.TabIndex = 45;
            groupBox4.TabStop = false;
            groupBox4.Text = "추천 키워드 선택";
            // 
            // dataGridView_recoman_keyword
            // 
            dataGridView_recoman_keyword.AllowUserToAddRows = false;
            dataGridView_recoman_keyword.AllowUserToDeleteRows = false;
            dataGridView_recoman_keyword.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Window;
            dataGridViewCellStyle4.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            dataGridView_recoman_keyword.DefaultCellStyle = dataGridViewCellStyle4;
            dataGridView_recoman_keyword.Location = new Point(16, 76);
            dataGridView_recoman_keyword.Name = "dataGridView_recoman_keyword";
            dataGridView_recoman_keyword.Size = new Size(261, 144);
            dataGridView_recoman_keyword.TabIndex = 43;
            // 
            // new_reco_word
            // 
            new_reco_word.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            new_reco_word.Location = new Point(14, 35);
            new_reco_word.Name = "new_reco_word";
            new_reco_word.PlaceholderText = "신규 항목 입력";
            new_reco_word.Size = new Size(136, 33);
            new_reco_word.TabIndex = 27;
            new_reco_word.KeyDown += new_reco_word_KeyDown;
            // 
            // reco_del_btn
            // 
            reco_del_btn.AutoSize = true;
            reco_del_btn.Font = new Font("맑은 고딕", 14.25F);
            reco_del_btn.Location = new Point(217, 35);
            reco_del_btn.Name = "reco_del_btn";
            reco_del_btn.Size = new Size(60, 35);
            reco_del_btn.TabIndex = 24;
            reco_del_btn.Text = "제거";
            reco_del_btn.UseVisualStyleBackColor = true;
            reco_del_btn.Click += reco_del_btn_Click;
            // 
            // reco_add_btn
            // 
            reco_add_btn.AutoSize = true;
            reco_add_btn.Font = new Font("맑은 고딕", 14.25F);
            reco_add_btn.Location = new Point(156, 35);
            reco_add_btn.Name = "reco_add_btn";
            reco_add_btn.Size = new Size(60, 35);
            reco_add_btn.TabIndex = 23;
            reco_add_btn.Text = "추가";
            reco_add_btn.UseVisualStyleBackColor = true;
            reco_add_btn.Click += reco_add_btn_Click;
            // 
            // gb_separator
            // 
            gb_separator.Controls.Add(dataGridView_lv1);
            gb_separator.Controls.Add(new_lv1_word);
            gb_separator.Controls.Add(lv1_del_btn);
            gb_separator.Controls.Add(lv1_add_btn);
            gb_separator.Font = new Font("맑은 고딕", 16F);
            gb_separator.Location = new Point(18, 236);
            gb_separator.Name = "gb_separator";
            gb_separator.Size = new Size(290, 226);
            gb_separator.TabIndex = 36;
            gb_separator.TabStop = false;
            gb_separator.Text = "Lv1 선택";
            // 
            // dataGridView_lv1
            // 
            dataGridView_lv1.AllowUserToAddRows = false;
            dataGridView_lv1.AllowUserToDeleteRows = false;
            dataGridView_lv1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = SystemColors.Window;
            dataGridViewCellStyle5.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle5.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.False;
            dataGridView_lv1.DefaultCellStyle = dataGridViewCellStyle5;
            dataGridView_lv1.Location = new Point(16, 76);
            dataGridView_lv1.Name = "dataGridView_lv1";
            dataGridView_lv1.Size = new Size(261, 144);
            dataGridView_lv1.TabIndex = 43;
            // 
            // new_lv1_word
            // 
            new_lv1_word.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            new_lv1_word.Location = new Point(14, 35);
            new_lv1_word.Name = "new_lv1_word";
            new_lv1_word.PlaceholderText = "신규 항목 입력";
            new_lv1_word.Size = new Size(136, 33);
            new_lv1_word.TabIndex = 27;
            new_lv1_word.KeyDown += new_lv1_word_KeyDown;
            // 
            // lv1_del_btn
            // 
            lv1_del_btn.AutoSize = true;
            lv1_del_btn.Font = new Font("맑은 고딕", 14.25F);
            lv1_del_btn.Location = new Point(217, 35);
            lv1_del_btn.Name = "lv1_del_btn";
            lv1_del_btn.Size = new Size(60, 35);
            lv1_del_btn.TabIndex = 24;
            lv1_del_btn.Text = "제거";
            lv1_del_btn.UseVisualStyleBackColor = true;
            lv1_del_btn.Click += lv1_del_btn_Click;
            // 
            // lv1_add_btn
            // 
            lv1_add_btn.AutoSize = true;
            lv1_add_btn.Font = new Font("맑은 고딕", 14.25F);
            lv1_add_btn.Location = new Point(156, 35);
            lv1_add_btn.Name = "lv1_add_btn";
            lv1_add_btn.Size = new Size(60, 35);
            lv1_add_btn.TabIndex = 23;
            lv1_add_btn.Text = "추가";
            lv1_add_btn.UseVisualStyleBackColor = true;
            lv1_add_btn.Click += lv1_add_btn_Click;
            // 
            // dataGridView_modified
            // 
            dataGridView_modified.AllowUserToAddRows = false;
            dataGridView_modified.AllowUserToDeleteRows = false;
            dataGridView_modified.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_modified.Location = new Point(18, 69);
            dataGridView_modified.Name = "dataGridView_modified";
            dataGridView_modified.ReadOnly = true;
            dataGridView_modified.Size = new Size(583, 161);
            dataGridView_modified.TabIndex = 23;
            // 
            // button2
            // 
            button2.AutoSize = true;
            button2.Font = new Font("맑은 고딕", 14.25F);
            button2.Location = new Point(6, 47);
            button2.Name = "button2";
            button2.Size = new Size(195, 35);
            button2.TabIndex = 49;
            button2.Text = "선택 항목 상세 보기";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // uc_Clustering
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox3);
            Controls.Add(groupbox2);
            Controls.Add(groupBox1);
            Name = "uc_Clustering";
            Size = new Size(1904, 1017);
            groupbox2.ResumeLayout(false);
            groupbox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)merge_check_table).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)merge_cluster_table).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_recoman_keyword).EndInit();
            gb_separator.ResumeLayout(false);
            gb_separator.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_lv1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_modified).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Button complete_btn;
        private GroupBox groupbox2;
        private RadioButton check_search_radio2;
        private RadioButton check_search_radio1;
        private ComboBox check_search_combo;
        private TextBox check_search_keyword;
        private Button merge_cancel_button;
        private Button check_search_button;
        private DataGridView merge_check_table;
        private GroupBox groupBox1;
        private RadioButton merge_search_radio2;
        private RadioButton merge_search_radio1;
        private ComboBox merge_keyword_combo;
        private TextBox merge_search_keyword;
        private CheckBox merge_all_check;
        private Button button1;
        private Button merge_search_button;
        private DataGridView merge_cluster_table;
        private Button merge_addon_btn;
        private GroupBox groupBox3;
        private Label label5;
        private ComboBox decimal_combo;
        private DataGridView dataGridView_modified;
        private GroupBox groupBox4;
        private DataGridView dataGridView_recoman_keyword;
        private TextBox new_reco_word;
        private Button reco_del_btn;
        private Button reco_add_btn;
        private GroupBox gb_separator;
        private DataGridView dataGridView_lv1;
        private TextBox new_lv1_word;
        private Button lv1_del_btn;
        private Button lv1_add_btn;
        private Label label1;
        private Label label7;
        private Label cluster_count;
        private TextBox except_keyword;
        private Label uncluster_count;
        private CheckBox equal_search_checkbox;
        private CheckBox excep_search_checkbox;
        private Label label3;
        private Label label2;
        private Label label4;
        private Label uncluster_count_money;
        private RadioButton keyword_radio2;
        private RadioButton keyword_radio1;
        private GroupBox groupBox5;
        private Button union_cluster_btn;
        private Button button2;
    }
}
