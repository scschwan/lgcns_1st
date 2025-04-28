using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinanceTool
{
    public partial class uc_FileLoad : UserControl
    {
        // 기존 필드
        private List<string> process_col_list = new List<string>();
        private string selectedStandColumn = "";
        //private HashSet<int> hiddenRows = new HashSet<int>();

        // SQLite 관련 추가 필드
        private DataConverter dataConverter;
        private int currentPage = 1;
        private int pageSize = 1000;
        private int totalPages = 1;
        private int totalRows = 0;

       

        // uc_FileLoad 클래스에서 멤버 변수 추가
        private bool _fileLoaded = false;

        public uc_FileLoad()
        {
            InitializeComponent();
            InitializePagingControls(false);

            dataConverter = new DataConverter();
        }

        private void InitializePagingControls(bool attachEvents)
        {
            // 콤보박스 초기화
            cmb_pageSize.Items.Clear();
            cmb_pageSize.Items.AddRange(new object[] { 500, 1000, 2000, 5000 });
            cmb_pageSize.SelectedIndex = 1; // 기본값 1000
            cmb_pageSize.DropDownStyle = ComboBoxStyle.DropDownList;

            // NumericUpDown 설정
            num_pageNumber.Minimum = 1;
            num_pageNumber.Maximum = 1;
            num_pageNumber.Value = 1;


            // 컨트롤 비활성화 (파일 로드 전)
            EnablePagingControls(false);

            // 이벤트 등록은 옵션에 따라 결정
            if (attachEvents)
            {
                AttachPagingEvents();
            }

            // 초기 페이징 상태
            UpdatePaginationInfo();

            DataHandler.RegisterDataGridView(dataGridView_delete_col);
            DataHandler.RegisterDataGridView(dataGridView_delete_data);
        }

        // 페이징 이벤트 등록 메서드 (별도로 분리)
        private void AttachPagingEvents()
        {
            // 이벤트 등록
            cmb_pageSize.SelectedIndexChanged += cmb_pageSize_SelectedIndexChanged;
            num_pageNumber.ValueChanged += num_pageNumber_ValueChanged;
            //btn_prevPage.Click += btn_prevPage_Click;
            //btn_nextPage.Click += btn_nextPage_Click;
        }

        // 페이징 컨트롤 활성화/비활성화 메서드
        private void EnablePagingControls(bool enabled)
        {
            btn_prevPage.Enabled = enabled;
            btn_nextPage.Enabled = enabled;
            num_pageNumber.Enabled = enabled;
            cmb_pageSize.Enabled = enabled;
        }

        // NumericUpDown 값 변경 이벤트 핸들러
        private async void num_pageNumber_ValueChanged(object sender, EventArgs e)
        {
            // 값이 범위를 벗어나면 조정
            if (num_pageNumber.Value < 1)
            {
                num_pageNumber.Value = 1;
                return;
            }

            if (num_pageNumber.Value > totalPages)
            {
                num_pageNumber.Value = totalPages;
                return;
            }

            // 이벤트 재귀 방지
            if (currentPage == (int)num_pageNumber.Value)
                return;

            // 페이지 이동
            currentPage = (int)num_pageNumber.Value;
            await LoadPagedDataAsync();
        }

        private void uc_FileLoad_Load(object sender, EventArgs e)
        {
            // 초기화 시 아무 작업도 하지 않음
        }

        private async void btn_selectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "엑셀 파일 선택"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 기존 데이터 존재 여부 확인
                bool dataExists = DBManager.Instance.IsInitialized && DBManager.Instance.TableExists("raw_data");

                // 기존 데이터가 있으면 확인 메시지 표시
                if (dataExists)
                {
                    DialogResult result = MessageBox.Show(
                        "파일을 새로 업로드 할 경우 \n기존 업로드 내역 및 작업 내용이 모두 초기화 됩니다.\n" +
                        "파일을 계속 업로드 하시겠습니까?",
                        "경고",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Cancel)
                    {
                        return; // 사용자가 취소함
                    }

                    // 사용자가 확인했으므로 데이터베이스 초기화 진행
                    if (!DBManager.Instance.ResetDatabase())
                    {
                        MessageBox.Show("데이터베이스 초기화에 실패했습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    // 초기 데이터베이스 설정 (첫 실행 시)
                    if (!DBManager.Instance.EnsureInitialized())
                    {
                        MessageBox.Show("데이터베이스 초기화에 실패했습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                string filePath = openFileDialog.FileName;
                await LoadExcelDataAsync(filePath);
                lbl_filename.Text = filePath;
            }
        }

        private async Task LoadExcelDataAsync(string filePath)
        {
            try
            {
                using (var progress = new ProcessProgressForm())
                {
                    progress.Show();
                    await Task.Delay(10);

                    await progress.UpdateProgressHandler(5, "파일 업로드 중...");

                    // Convert Excel to SQLite using the data converter
                    await dataConverter.ConvertExcelToSQLiteAsync(filePath, progress.UpdateProgressHandler);

                    await progress.UpdateProgressHandler(80, "페이징 정보 생성중...");
                    await Task.Delay(10);

                    // File load completed - register events if first load
                    if (!_fileLoaded)
                    {
                        AttachPagingEvents();
                        _fileLoaded = true;
                    }

                    // Enable paging controls
                    EnablePagingControls(true);

                    // Initialize paging and load first page
                    currentPage = 1;
                    pageSize = 1000;
                    await LoadPagedDataAsync(true);
                    await Task.Delay(10);

                    await AddSelectedColumnToGrid(dataGridView_delete_col, dataGridView_process);

                    await progress.UpdateProgressHandler(90, "컬럼 정보 생성 중...");
                    await Task.Delay(10);

                    Debug.WriteLine("GetColumnList call");
                    GetColumnList();

                    Debug.WriteLine("setup_col_list call");
                    setup_col_list();

                    await progress.UpdateProgressHandler(100);
                    await Task.Delay(10);
                    progress.Close();

                    Debug.WriteLine("File loading completed successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"엑셀 파일 로드 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 페이지 데이터 로드
        private async Task LoadPagedDataAsync(bool progressYN = false)
        {

            // 파일이 로드되지 않았으면 아무 작업도 수행하지 않음
            if (!_fileLoaded)
            {
                Debug.WriteLine("파일이 로드되지 않아 페이징 작업을 건너뜁니다.");
                return;
            }

            try
            {
                // dataConverter가 null인지 확인하고 필요한 경우 초기화
                if (dataConverter == null)
                {
                    dataConverter = new DataConverter();
                }

                if (progressYN)
                {
                    // 페이징된 데이터 가져오기
                    DataTable pageData = null;

                    await Task.Run(() =>
                    {
                        pageData = dataConverter.GetPagedRawData(currentPage, pageSize, DataHandler.hiddenData);
                        
                    });

                    // 페이징 메타데이터 추출
                    if (pageData != null && pageData.ExtendedProperties.Contains("Paging"))
                    {
                        DataTable metaData = pageData.ExtendedProperties["Paging"] as DataTable;
                        if (metaData != null && metaData.Rows.Count > 0)
                        {
                            totalRows = Convert.ToInt32(metaData.Rows[0]["TotalRows"]);
                            totalPages = Convert.ToInt32(metaData.Rows[0]["TotalPages"]);
                            currentPage = Convert.ToInt32(metaData.Rows[0]["CurrentPage"]);
                        }
                    }


                    // UI 업데이트는 메인 스레드에서 수행
                    this.BeginInvoke(new Action(() =>
                    {
                       
                        ConfigureDataGridView(pageData, dataGridView_target);
                        ConfigureDataGridView(pageData, dataGridView_process);
                        UpdatePaginationInfo();
                        ApplyGridFormatting();

                        
                    }));
                }
                else
                {
                    using (var loadingForm = new ProcessProgressForm())
                    {
                        loadingForm.Show();
                        loadingForm.UpdateProgressHandler(10);

                        // 페이징된 데이터 가져오기
                        DataTable pageData = null;

                        await Task.Run(() =>
                        {
                            pageData = dataConverter.GetPagedRawData(currentPage, pageSize, DataHandler.hiddenData);
                            loadingForm.UpdateProgressHandler(70);
                        });

                        // 페이징 메타데이터 추출
                        if (pageData != null && pageData.ExtendedProperties.Contains("Paging"))
                        {
                            DataTable metaData = pageData.ExtendedProperties["Paging"] as DataTable;
                            if (metaData != null && metaData.Rows.Count > 0)
                            {
                                totalRows = Convert.ToInt32(metaData.Rows[0]["TotalRows"]);
                                totalPages = Convert.ToInt32(metaData.Rows[0]["TotalPages"]);
                                currentPage = Convert.ToInt32(metaData.Rows[0]["CurrentPage"]);
                            }
                        }

                        loadingForm.UpdateProgressHandler(80);

                        // UI 업데이트는 메인 스레드에서 수행
                        this.BeginInvoke(new Action(() =>
                        {
                            //dataGridView_target.DataSource = pageData;
                            //dataGridView_process.DataSource = pageData;
                            ConfigureDataGridView(pageData, dataGridView_target);
                            ConfigureDataGridView(pageData, dataGridView_process);
                            UpdatePaginationInfo();
                            ApplyGridFormatting();

                            // 숨겨진 행 상태 적용
                            /*
                            if (hiddenRows.Count > 0)
                            {
                                ApplyHiddenRowsToGrids();
                            }
                            */
                        }));

                        loadingForm.UpdateProgressHandler(100);
                        await Task.Delay(300);
                        loadingForm.Close();
                    }
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"페이지 데이터 로드 중 오류: {ex.Message}");
                MessageBox.Show($"데이터 로드 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ConfigureDataGridView(DataTable dataTable, DataGridView dataGridView)
        {
            // DataGridView의 DataSource를 DataTable로 설정
            dataGridView.DataSource = dataTable;

            // hiddenYN 컬럼이 있는지 확인
            if (dataTable.Columns.Contains("hiddenYN"))
            {
                // hiddenYN 컬럼을 숨김
                dataGridView.Columns["hiddenYN"].Visible = false;

                // 각 행을 순회하며 hiddenYN 값이 0인 경우 스타일 적용
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    // hiddenYN 컬럼의 값이 0인지 확인
                    if (row.Cells["hiddenYN"].Value != null && row.Cells["hiddenYN"].Value.ToString() == "0")
                    {
                        //Debug.WriteLine($"hidden row Process row id : {row.Cells["id"].Value.ToString()} , row hiddenyn  : {row.Cells["hiddenYN"].Value.ToString()}");
                        // 배경색과 글자색 변경
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.DarkGray;
                    }
                    else
                    {
                        //Debug.WriteLine($"hidden row Process row id : {row.Cells["id"].Value.ToString()} , row hiddenyn  : {row.Cells["hiddenYN"].Value.ToString()}");
                    }
                }
            }
            else
            {
                // hiddenYN 컬럼이 없는 경우 경고 메시지 출력 (옵션)
                MessageBox.Show("'hiddenYN' 컬럼이 존재하지 않습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // 페이징 정보 업데이트
        private void UpdatePaginationInfo()
        {
            // NumericUpDown 범위 설정
            num_pageNumber.Maximum = Math.Max(1, totalPages);

            // 현재 페이지 값 설정 (이벤트 발생 방지를 위해 조건 체크)
            if (num_pageNumber.Value != currentPage)
                num_pageNumber.Value = currentPage;

            // 라벨 텍스트 업데이트
            lbl_pagination2.Text = $"/ {totalPages} (총 {totalRows:N0}행)";

            // 버튼 활성화/비활성화
            btn_prevPage.Enabled = currentPage > 1;
            btn_nextPage.Enabled = currentPage < totalPages;
        }

        // 그리드 형식 적용
        private void ApplyGridFormatting()
        {
            foreach (DataGridView dgv in new[] { dataGridView_target, dataGridView_process })
            {
                // AutoSizeColumnsMode 설정 제거
                dgv.AllowUserToAddRows = false;
                dgv.ReadOnly = true;
                dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                // 헤더 스타일 설정
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.LightSteelBlue;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.Black;
                dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("맑은 고딕", 9.0f, FontStyle.Bold);

                // 셀 폰트 설정
                dgv.DefaultCellStyle.Font = new System.Drawing.Font("맑은 고딕", 9.0f);
            }
        }

        // 이전 페이지 이동
        private async void btn_prevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                num_pageNumber.Value--;
                //await LoadPagedDataAsync();
            }
        }

        // 다음 페이지 이동
        private async void btn_nextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                num_pageNumber.Value++;
                //await LoadPagedDataAsync();
            }
        }

        // 페이지 크기 변경
        private async void cmb_pageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_pageSize.SelectedItem != null)
            {
                pageSize = Convert.ToInt32(cmb_pageSize.SelectedItem);
                currentPage = 1; // 페이지 크기 변경 시 첫 페이지로
                await LoadPagedDataAsync();
            }
        }

        public void GetColumnList()
        {
            process_col_list = new List<string>();

            foreach (DataGridViewColumn column in dataGridView_process.Columns)
            {
                if (column.Visible && column.HeaderText != "id" && column.HeaderText != "import_date")
                {
                    process_col_list.Add(column.HeaderText);
                }
            }

            Debug.WriteLine($"process_col_list count : {process_col_list.Count}");
        }

        private void setup_col_list()
        {
            try
            {
              
                // ComboBox에 열 이름 추가
                stand_col_combo.Items.Clear();
                stand_col_combo.Items.Add("데이터 삭제 기준 열 선택");
                foreach (string column in process_col_list)
                {
                    stand_col_combo.Items.Add(column);
                }
                stand_col_combo.SelectedIndex = 0; // 첫 번째 열 선택

                // ComboBox에 열 이름 추가
                sub_acc_col_combo.Items.Clear();
                sub_acc_col_combo.Items.Add("세목 열 선택");
                foreach (string column in process_col_list)
                {
                    sub_acc_col_combo.Items.Add(column);
                }
                sub_acc_col_combo.SelectedIndex = 0; // 첫 번째 열 선택

                // ComboBox에 열 이름 추가
                dept_col_combo.Items.Clear();
                dept_col_combo.Items.Add("부서 열 선택");
                foreach (string column in process_col_list)
                {
                    dept_col_combo.Items.Add(column);
                }
                dept_col_combo.SelectedIndex = 0; // 첫 번째 열 선택

                prod_col_combo.Items.Clear();
                prod_col_combo.Items.Add("공급업체 열 선택");
                foreach (string column in process_col_list)
                {
                    prod_col_combo.Items.Add(column);
                }
                prod_col_combo.SelectedIndex = 0; // 첫 번째 열 선택

                // ComboBox에 열 이름 추가
                cmb_target.Items.Clear();
                cmb_target.Items.Add("키워드 대상 열 선택");
                foreach (string column in process_col_list)
                {
                    cmb_target.Items.Add(column);
                }

                if (cmb_target.Items.Count > 0)
                    cmb_target.SelectedIndex = 0; // 첫 번째 열 선택

                // ComboBox에 열 이름 추가
                cmb_money.Items.Clear();
                cmb_money.Items.Add("금액 열 선택");
                foreach (string column in process_col_list)
                {
                    cmb_money.Items.Add(column);
                }

                if (cmb_money.Items.Count > 0)
                    cmb_money.SelectedIndex = 0; // 첫 번째 열 선택

            }
            catch (Exception ex)
            {
                MessageBox.Show($"키워드 생성 중 오류 발생: {ex.Message}");
            }
        }

        public void initCursor()
        {
            dataGridView_target.ClearSelection();
            dataGridView_process.ClearSelection();

            dataGridView_target.CurrentCell = null;
            dataGridView_process.CurrentCell = null;
        }

        public async Task AddSelectedColumnToGrid(DataGridView targetDgv, DataGridView sourceDgv)
        {
            
            // 대상 DataGridView가 비어있는 경우에만 컬럼 초기 설정
            if (targetDgv.Columns.Count == 0)
            {
                // 체크박스 컬럼 추가
                DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn
                {
                    Name = "CheckBox",
                    HeaderText = "",
                    Width = 50,
                    ThreeState = false,
                    FillWeight = 20
                };
                targetDgv.Columns.Add(checkColumn);

                // 데이터 컬럼 추가
                DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Data",  // 고정된 컬럼명 사용
                    HeaderText = "컬럼명"
                };
                targetDgv.Columns.Add(textColumn);

                // GridView 설정
                targetDgv.AllowUserToAddRows = false;
                targetDgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                targetDgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                targetDgv.Columns["Data"].ReadOnly = true;  // 체크박스 컬럼만 편집 가능
                targetDgv.Columns["CheckBox"].ReadOnly = false;  // 체크박스 컬럼만 편집 가능
                targetDgv.Font = new System.Drawing.Font("맑은 고딕", 14.25F);
            } 
            //데이터가 있을 경우 초기화
            else
            {
                // dataGridView_delete_col 초기화
                targetDgv.DataSource = null;
                targetDgv.Rows.Clear();
                targetDgv.Columns.Clear();
                if (DataHandler.dragSelections.ContainsKey(targetDgv))
                {
                    DataHandler.dragSelections[targetDgv].Clear();
                }

                // 체크박스 컬럼 추가
                DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn
                {
                    Name = "CheckBox",
                    HeaderText = "",
                    Width = 50,
                    ThreeState = false,
                    FillWeight = 20
                };
                targetDgv.Columns.Add(checkColumn);

                // 데이터 컬럼 추가
                DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Data",  // 고정된 컬럼명 사용
                    HeaderText = "컬럼명"
                };
                targetDgv.Columns.Add(textColumn);

                // GridView 설정
                targetDgv.AllowUserToAddRows = false;
                targetDgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                targetDgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                targetDgv.Columns["Data"].ReadOnly = true;  // 체크박스 컬럼만 편집 가능
                targetDgv.Columns["CheckBox"].ReadOnly = false;  // 체크박스 컬럼만 편집 가능
                targetDgv.Font = new System.Drawing.Font("맑은 고딕", 14.25F);
            }

            Debug.WriteLine($"AddSelectedColumnToGrid  sourceDgv.Columns.Count: {sourceDgv.Columns.Count} ");
            for (int i = 0; i < sourceDgv.Columns.Count; i++)
            {

                if (!"id".Equals(sourceDgv.Columns[i].Name) && !"import_date".Equals(sourceDgv.Columns[i].Name) && !"hiddenYN".Equals(sourceDgv.Columns[i].Name))
                {
                    // 새 행 추가
                    int rowIndex = targetDgv.Rows.Add();
                    targetDgv.Rows[rowIndex].Cells["CheckBox"].Value = true;
                    targetDgv.Rows[rowIndex].Cells["Data"].Value = sourceDgv.Columns[i].Name;  // 고정된 컬럼명 사용
                }
                
            }

            Debug.WriteLine($"AddSelectedColumnToGrid  complete targetDgv.Rows.Count: {targetDgv.Rows.Count} ");
        }

        

        //체크 항목 데이터 수집
        public List<string> GetCheckedRowsData(DataGridView dgv)
        {
            List<string> checkedData = new List<string>();

            foreach (DataGridViewRow row in dgv.Rows)
            {
                // CheckBox 컬럼(0번째)이 체크되었는지 확인
                if (row.Cells[0].Value != null &&
                    Convert.ToBoolean(row.Cells[0].Value) == true)
                {
                    // 1번째 열의 데이터를 리스트에 추가
                    string value = row.Cells[1].Value?.ToString() ?? "";
                    checkedData.Add(value);
                }
            }

            Debug.WriteLine(String.Join(", ", checkedData));

            return checkedData;
        }

        public class NonNumericData
        {
            public int RowIndex { get; set; }
            public string Value { get; set; }
        }

        //금액 컬럼 검증 로직
        //숫자 데이터가 아닌 데이터가 있다면 false return
        private (bool isAllNumeric, List<NonNumericData> nonNumericList) CheckNumericColumn(DataGridView dgv, string columnName)
        {
            var nonNumericList = new List<NonNumericData>();

            try
            {
                if (!dgv.Columns.Contains(columnName))
                {
                    throw new ArgumentException($"컬럼 '{columnName}'이 존재하지 않습니다.");
                }

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;

                    var cellValue = row.Cells[columnName].Value;

                    if (cellValue == null || cellValue == DBNull.Value || string.IsNullOrWhiteSpace(cellValue.ToString()))
                        continue;

                    string strValue = cellValue.ToString().Trim();

                    if (!decimal.TryParse(strValue, out _))
                    {
                        nonNumericList.Add(new NonNumericData
                        {
                            RowIndex = row.Index,
                            Value = strValue
                        });
                    }
                }

                return (nonNumericList.Count == 0, nonNumericList);
            }
            catch (Exception ex)
            {
                throw new Exception($"컬럼 검사 중 오류 발생: {ex.Message}");
            }
        }

        private void btn_complete_Click(object sender, EventArgs e)
        {
            //data Validation 
            if (sub_acc_col_combo.SelectedIndex < 1)
            {
                MessageBox.Show("세목 열을 선택하셔야 합니다.", "알림",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                return;
            }

            if (dept_col_combo.SelectedIndex < 1)
            {
                MessageBox.Show("부서 열을 선택하셔야 합니다.", "알림",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                return;
            }

            if (prod_col_combo.SelectedIndex < 1)
            {
                MessageBox.Show("공급업체 열을 선택하셔야 합니다.", "알림",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                return;
            }

            if (cmb_money.SelectedIndex < 1)
            {
                MessageBox.Show("금액 열을 선택하셔야 합니다.", "알림",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                return;
            }
            else
            {
                // 금액 컬럼 데이터 유효성 검증
                var (isAllNumeric, nonNumericData) = CheckNumericColumn(dataGridView_process, cmb_money.SelectedItem.ToString());
                if (!isAllNumeric)
                {
                    var firstError = nonNumericData[0];
                    MessageBox.Show(
                        $"금액 열은 숫자값만 있어야 합니다.\n행 번호 : {firstError.RowIndex + 1}, 행 값 : {firstError.Value}",
                        "알림",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }
            }

            if (cmb_target.SelectedIndex < 1)
            {
                MessageBox.Show("타겟 열을 선택하셔야 합니다.", "알림",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var progressForm = new ProcessProgressForm())
                {
                    progressForm.Show();
                    progressForm.UpdateProgressHandler(10);

                    // 선택된 컬럼 목록 생성
                    List<string> selectedColumns = new List<string>();
                    selectedColumns.Add(sub_acc_col_combo.SelectedItem.ToString());
                    selectedColumns.Add(dept_col_combo.SelectedItem.ToString());
                    selectedColumns.Add(prod_col_combo.SelectedItem.ToString());
                    selectedColumns.Add(cmb_money.SelectedItem.ToString());
                    selectedColumns.Add(cmb_target.SelectedItem.ToString());

                    // 필요한 전역 변수 설정
                    DataHandler.sub_acc_col_name = sub_acc_col_combo.SelectedItem.ToString();
                    DataHandler.dept_col_name = dept_col_combo.SelectedItem.ToString();
                    DataHandler.prod_col_name = prod_col_combo.SelectedItem.ToString();
                    DataHandler.levelList.Clear();
                    DataHandler.levelName.Clear();

                    // 금액 컬럼 인덱스 설정 (프로세스 테이블에서는 0)
                    DataHandler.levelList.Add(5);
                    DataHandler.moneyIndex = 5;
                    DataHandler.levelName.Add(cmb_money.SelectedItem.ToString());

                    // 타겟 컬럼 인덱스 설정 (프로세스 테이블에서는 1)
                    DataHandler.levelList.Add(6);
                    DataHandler.levelName.Add(cmb_target.SelectedItem.ToString());
                    

                    progressForm.UpdateProgressHandler(30);

                    // process_data 테이블 준비
                    dataConverter.PrepareProcessTable(selectedColumns);

                    progressForm.UpdateProgressHandler(70);

                    // 레거시 코드와의 호환을 위해 DataHandler.processTable 설정
                    DataHandler.processTable = DBManager.Instance.ExecuteQuery("SELECT * FROM process_data");

                    progressForm.UpdateProgressHandler(90);

                    // 숨겨진 행 데이터 처리
                    /*
                    if (hiddenRows.Count > 0)
                    {
                        foreach (int rowId in hiddenRows)
                        {
                            dataConverter.HideRow(rowId, "User hidden");
                        }
                    }
                    */
                    progressForm.UpdateProgressHandler(100);
                }

                // 다음 단계로 이동
                userControlHandler.uc_Preprocessing.initUI();
                if (this.ParentForm is Form1 form)
                {
                    form.LoadUserControl(userControlHandler.uc_Preprocessing);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"처리 중 오류가 발생했습니다: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"btn_complete_Click 오류: {ex.Message}\n{ex.StackTrace}");
            }
        }

       

        private void del_col_list_allcheck_CheckedChanged(object sender, EventArgs e)
        {
            // 모든 행의 체크박스 상태 변경
            foreach (DataGridViewRow row in dataGridView_delete_col.Rows)
            {
                row.Cells[0].Value = del_col_list_allcheck.Checked;
            }
        }

        private void del_data_list_allcheck_CheckedChanged(object sender, EventArgs e)
        {
            // 모든 행의 체크박스 상태 변경
            foreach (DataGridViewRow row in dataGridView_delete_data.Rows)
            {
                row.Cells[0].Value = del_data_list_allcheck.Checked;
            }
        }

        private void restore_col_btn_Click(object sender, EventArgs e)
        {
            List<string> restore_list = GetCheckedRowsData(dataGridView_delete_col);

            // 선택된 열이 없는 경우
            //if (restore_list.Count == 0) return;

            for (int i = 0; i < dataGridView_process.Columns.Count; i++)
            {
                if (restore_list.Contains(dataGridView_process.Columns[i].Name))
                {
                    dataGridView_process.Columns[i].Visible = true;

                    // SQLite에서 컬럼 가시성 업데이트
                    dataConverter.UpdateColumnVisibility(dataGridView_process.Columns[i].Name, true);
                }
                else
                {
                    dataGridView_process.Columns[i].Visible = false;

                    // SQLite에서 컬럼 가시성 업데이트
                    dataConverter.UpdateColumnVisibility(dataGridView_process.Columns[i].Name, false);
                }
            }

            GetColumnList();
            setup_col_list();
        }

       

        private void stand_col_combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (stand_col_combo.SelectedIndex == 0)
            {
                return;
            }

            selectedStandColumn = stand_col_combo.SelectedItem.ToString();
            PopulateDeleteDataGrid(selectedStandColumn);
        }

        // 삭제 데이터 그리드 채우기
        private void PopulateDeleteDataGrid(string columnName)
        {
            try
            {
                // SQLite에서 고유 값 가져오기
                string query = $"SELECT DISTINCT {columnName} FROM raw_data WHERE {columnName} IS NOT NULL ORDER BY {columnName}";
                DataTable distinctValues = DBManager.Instance.ExecuteQuery(query);

                // DataGridView 초기화
                dataGridView_delete_data.DataSource = null;
                dataGridView_delete_data.Rows.Clear();
                dataGridView_delete_data.Columns.Clear();
                if (DataHandler.dragSelections.ContainsKey(dataGridView_delete_data))
                {
                    DataHandler.dragSelections[dataGridView_delete_data].Clear();
                }

                // 체크박스 컬럼 추가
                DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn
                {
                    Name = "CheckBox",
                    HeaderText = "",
                    Width = 50,
                    ThreeState = false,
                    FillWeight = 20
                };
                dataGridView_delete_data.Columns.Add(checkColumn);

                // 데이터 컬럼 추가
                DataGridViewTextBoxColumn dataColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Data",
                    HeaderText = "데이터"
                };
                dataGridView_delete_data.Columns.Add(dataColumn);

                // 데이터 리스트의 각 항목을 행으로 추가
                foreach (DataRow row in distinctValues.Rows)
                {
                    if (row[0] != DBNull.Value && !string.IsNullOrEmpty(row[0].ToString()))
                    {
                        int rowIndex = dataGridView_delete_data.Rows.Add();
                        dataGridView_delete_data.Rows[rowIndex].Cells["CheckBox"].Value = false;
                        dataGridView_delete_data.Rows[rowIndex].Cells["Data"].Value = row[0].ToString();
                    }
                }

                dataGridView_delete_data.AllowUserToAddRows = false;
                dataGridView_delete_data.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView_delete_data.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView_delete_data.ReadOnly = false;
                dataGridView_delete_data.Columns["CheckBox"].ReadOnly = false;  // 체크박스 컬럼만 편집 가능
                dataGridView_delete_data.Font = new System.Drawing.Font("맑은 고딕", 14.25F);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"데이터 로드 중 오류: {ex.Message}");
                MessageBox.Show($"데이터 로드 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void restore_del_data_btn_Click(object sender, EventArgs e)
        {
            try
            {
                // 모든 그리드 선택 초기화 - 이 부분이 중요!
                InitializeCursors();


                DataHandler.hiddenData = false;

                using (var progressForm = new ProcessProgressForm())
                {
                    progressForm.Show();
                    progressForm.UpdateProgressHandler(20);

                    // SQLite에서 모든 숨겨진 행 복원
                    dataConverter.UnhideAllRows();

                    progressForm.UpdateProgressHandler(50);

                    // UI 업데이트
                    this.BeginInvoke(new Action(() =>
                    {
                        // 숨겨진 행 컬렉션 초기화
                        //hiddenRows.Clear();

                        // 모든 행 표시 설정 & 스타일 초기화
                        RestoreAllRowsVisibility();
                    }));

                    progressForm.UpdateProgressHandler(70);

                    // 데이터 다시 로드
                    if (!string.IsNullOrEmpty(selectedStandColumn))
                    {
                        PopulateDeleteDataGrid(selectedStandColumn);
                    }

                    progressForm.UpdateProgressHandler(90);

                    // 페이지 데이터 리로드
                    /*
                    Task.Run(async () =>
                    {
                        await LoadPagedDataAsync();
                    }).Wait();
                    */
                    await LoadPagedDataAsync();
                    progressForm.UpdateProgressHandler(100);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 복원 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 모든 행 표시 상태 및 스타일 복원
        private void RestoreAllRowsVisibility()
        {
            // Process 그리드
            foreach (DataGridViewRow row in dataGridView_process.Rows)
            {
                row.Visible = true;
            }

            // Target 그리드 - 스타일 초기화
            foreach (DataGridViewRow row in dataGridView_target.Rows)
            {
                row.DefaultCellStyle.BackColor = dataGridView_target.DefaultCellStyle.BackColor;
                row.DefaultCellStyle.ForeColor = dataGridView_target.DefaultCellStyle.ForeColor;
            }
        }

        private async void delete_data_btn_Click(object sender, EventArgs e)
        {
            List<string> delList = GetCheckedRowsData(dataGridView_delete_data);

            if (delList.Count == 0)
            {
                MessageBox.Show("데이터 삭제 대상을 선택하셔야 합니다.", "알림",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                return;
            }

            DataHandler.hiddenData = true;

            try
            {
                using (var progressForm = new ProcessProgressForm())
                {
                    progressForm.Show();
                    progressForm.UpdateProgressHandler(10);

                    // SQLite에서 숨기기 처리
                    int hiddenCount = 0;
                    await Task.Run(() =>
                    {
                        // 각 값에 대해 해당하는 행 숨기기
                        foreach (string value in delList)
                        {
                            hiddenCount += dataConverter.HideRowsByColumnValue(selectedStandColumn, value);
                        }
                    });

                    progressForm.UpdateProgressHandler(30);

                    // UI에 숨겨진 행 표시 - 기존 코드와 유사한 방식 적용
                    /*
                    await Task.Run(() =>
                    {
                        // 각 행을 순회하며 값이 일치하는 행 찾기
                        for (int i = 0; i < dataGridView_process.Rows.Count; i++)
                        {
                            try
                            {
                                if (i >= dataGridView_process.Rows.Count) continue;

                                object cellObj = dataGridView_process.Rows[i].Cells[selectedStandColumn].Value;
                                if (cellObj == null) continue;

                                string cellValue = cellObj.ToString();
                                if (delList.Contains(cellValue))
                                {
                                    // 숨길 행 인덱스 저장
                                    hiddenRows.Add(i);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"행 {i} 처리 중 오류: {ex.Message}");
                            }
                        }
                    });
                    */

                    //progressForm.UpdateProgressHandler(60);

                    // UI 업데이트는 메인 스레드에서 수행
                    this.BeginInvoke(new Action(() =>
                    {
                        // 모든 그리드 선택 초기화 - 이 부분이 중요!
                        InitializeCursors();

                        // 숨겨진 행 적용
                        //ApplyHiddenRowsToGrids();
                    }));

                    progressForm.UpdateProgressHandler(80);

                    // 삭제 데이터 목록에서 처리된 항목 제거
                    RemoveProcessedRows(delList);

                    progressForm.UpdateProgressHandler(90);

                    // 페이지 데이터 리로드
                    await LoadPagedDataAsync();

                    progressForm.UpdateProgressHandler(100);
                }

                MessageBox.Show($"{delList.Count}개 항목이 숨겨졌습니다.", "정보",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 숨기기 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 그리드에 숨김 처리 적용하는 새 메서드
        private void ApplyHiddenRowsToGrids()
        {

            // 먼저 그리드 선택 및 현재 셀 초기화
            InitializeCursors();

            // 각 숨겨진 행에 대해 처리
            /*
            foreach (int rowIndex in hiddenRows)
            {
                if (rowIndex < dataGridView_process.Rows.Count)
                {
                    //dataGridView_process.Rows[rowIndex].Visible = false;
                    //2025.02.21
                    //페이징 처리 기준으로 data를 hidden 처리 하는 것이 더 확인이 어려울 것으로 판단 됨.
                    dataGridView_process.Rows[rowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                    dataGridView_process.Rows[rowIndex].DefaultCellStyle.ForeColor = System.Drawing.Color.DarkGray;
                }

                if (rowIndex < dataGridView_target.Rows.Count)
                {
                    // 숨겨진 행 스타일링
                    dataGridView_target.Rows[rowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                    dataGridView_target.Rows[rowIndex].DefaultCellStyle.ForeColor = System.Drawing.Color.DarkGray;
                }
            }
            */
        }

        // 그리드 커서 초기화 메서드 분리
        private void InitializeCursors()
        {
            // 모든 그리드 선택 해제
            foreach (DataGridView dgv in new[] { dataGridView_target, dataGridView_process })
            {
                try
                {
                    // 현재 선택을 모두 제거
                    dgv.ClearSelection();

                    // 현재 셀을 null로 설정
                    dgv.CurrentCell = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"그리드 커서 초기화 실패: {ex.Message}");
                }
            }

            // 이전에 처리했던 방식대로 추가 조치
            Application.DoEvents(); // UI 업데이트 허용
        }

        // 처리된 행을 삭제 데이터 그리드에서 제거
        private void RemoveProcessedRows(List<string> values)
        {
            for (int i = dataGridView_delete_data.Rows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = dataGridView_delete_data.Rows[i];
                string value = row.Cells["Data"].Value?.ToString();
                if (values.Contains(value))
                {
                    dataGridView_delete_data.Rows.RemoveAt(i);
                }
            }
        }



        // 그리드뷰 셀 선택 이벤트 핸들러
        private void dataGridView_target_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView_target.SelectedRows.Count > 0)
            {
                /*
                foreach (DataGridViewRow row in dataGridView_target.SelectedRows)
                {
                    if (hiddenRows.Contains(row.Index))
                    {
                        
                        // 선택을 제거
                        row.Selected = false;
                    }
                }
                */
            }
            /*
            if (dataGridView_target.CurrentCell != null &&
                hiddenRows.Contains(dataGridView_target.CurrentCell.RowIndex))
            {
                int selectRow = dataGridView_target.CurrentCell.RowIndex;
                initCursor();
                dataGridView_process.Rows[selectRow].Visible = false;
            }
            */
        }

        private void dataGridView_target_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            /*
            if (e.RowIndex >= 0 && hiddenRows.Contains(e.RowIndex))
            {
                // 선택을 캔슬
                initCursor();
            }
            */
        }

        private void delete_search_button_Click(object sender, EventArgs e)
        {
            // 현재 선택된 컬럼이 없으면 메시지 표시 후 종료
            if (string.IsNullOrEmpty(selectedStandColumn) || stand_col_combo.SelectedIndex == 0)
            {
                MessageBox.Show("검색하기 전에 기준 열을 선택해주세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 검색 키워드 가져오기
            string searchText = delete_search_keyword.Text.Trim();

            // 키워드가 비어있으면 모든 항목 표시
            if (string.IsNullOrEmpty(searchText))
            {
                PopulateDeleteDataGrid(selectedStandColumn);
                return;
            }

            try
            {
                // 키워드를 쉼표(,)로 분리
                string[] keywords = searchText.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(k => k.Trim())
                                              .Where(k => !string.IsNullOrEmpty(k))
                                              .ToArray();

                if (keywords.Length == 0)
                {
                    PopulateDeleteDataGrid(selectedStandColumn);
                    return;
                }

                // SQL LIKE 조건을 위한 쿼리 구성
                List<string> conditions = new List<string>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                for (int i = 0; i < keywords.Length; i++)
                {
                    string paramName = $"keyword{i}";
                    conditions.Add($"{selectedStandColumn} LIKE @{paramName}");
                    parameters[paramName] = $"%{keywords[i]}%";
                }

                // OR 조건으로 각 키워드에 대한 LIKE 절 결합
                string whereClause = string.Join(" OR ", conditions);

                // SQLite에서 검색 조건에 맞는 값 가져오기
                string query = $"SELECT DISTINCT {selectedStandColumn} FROM raw_data " +
                               $"WHERE {selectedStandColumn} IS NOT NULL AND ({whereClause}) " +
                               $"ORDER BY {selectedStandColumn}";

                DataTable filteredValues = DBManager.Instance.ExecuteQuery(query, parameters);

                // DataGridView 초기화 및 데이터 표시
                PopulateDeleteDataGridWithResults(filteredValues);

                // 결과 메시지 표시 (선택사항)
                if (filteredValues.Rows.Count == 0)
                {
                    MessageBox.Show($"검색 결과가 없습니다.", "검색 결과",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"검색 중 오류 발생: {ex.Message}");
                MessageBox.Show($"데이터 검색 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 필터링된 결과로 DataGridView 채우기
        private void PopulateDeleteDataGridWithResults(DataTable filteredValues)
        {
            // DataGridView 초기화
            dataGridView_delete_data.DataSource = null;
            dataGridView_delete_data.Rows.Clear();
            dataGridView_delete_data.Columns.Clear();
            if (DataHandler.dragSelections.ContainsKey(dataGridView_delete_data))
            {
                DataHandler.dragSelections[dataGridView_delete_data].Clear();
            }

            // 체크박스 컬럼 추가
            DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn
            {
                Name = "CheckBox",
                HeaderText = "",
                Width = 50,
                ThreeState = false,
                FillWeight = 20
            };
            dataGridView_delete_data.Columns.Add(checkColumn);

            // 데이터 컬럼 추가
            DataGridViewTextBoxColumn dataColumn = new DataGridViewTextBoxColumn
            {
                Name = "Data",
                HeaderText = "데이터"
            };
            dataGridView_delete_data.Columns.Add(dataColumn);

            // 필터링된 데이터 추가
            foreach (DataRow row in filteredValues.Rows)
            {
                if (row[0] != DBNull.Value && !string.IsNullOrEmpty(row[0].ToString()))
                {
                    int rowIndex = dataGridView_delete_data.Rows.Add();
                    dataGridView_delete_data.Rows[rowIndex].Cells["CheckBox"].Value = false;
                    dataGridView_delete_data.Rows[rowIndex].Cells["Data"].Value = row[0].ToString();
                }
            }

            // DataGridView 설정
            dataGridView_delete_data.AllowUserToAddRows = false;
            dataGridView_delete_data.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView_delete_data.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView_delete_data.ReadOnly = false;
            dataGridView_delete_data.Columns["CheckBox"].ReadOnly = false;
            dataGridView_delete_data.Font = new System.Drawing.Font("맑은 고딕", 14.25F);
        }

        private void delete_search_keyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                delete_search_button_Click(sender, e);
                e.SuppressKeyPress = true;  // 비프음 방지
            }
        }

       
    }

    

    // 간단한 로딩 폼
    public class LoadingForm : Form
    {
        private Label messageLabel;

        public LoadingForm(string message)
        {
            InitializeComponent(message);
        }

        private void InitializeComponent(string message)
        {
            this.Width = 300;
            this.Height = 100;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.White;

            messageLabel = new Label();
            messageLabel.Text = message;
            messageLabel.AutoSize = false;
            messageLabel.Size = new Size(280, 60);
            messageLabel.Location = new Point(10, 20);
            messageLabel.TextAlign = ContentAlignment.MiddleCenter;
            messageLabel.Font = new System.Drawing.Font("맑은 고딕", 12F, FontStyle.Regular);

            this.Controls.Add(messageLabel);
        }
    }
}