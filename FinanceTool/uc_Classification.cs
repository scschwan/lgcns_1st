using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinanceTool
{
    public partial class uc_Classification : UserControl
    {
        DataTable export_result = new DataTable();
        DataTable cluster_result = new DataTable();
        List<string> process_col_list = new List<string>();

        private DataConverter dataConverter;
        private int currentPage = 1;
        private int pageSize = 1000;
        private int totalPages = 1;
        private int totalRows = 0;

        private bool isProcessingSearch = false;

        public uc_Classification()
        {
            InitializeComponent();
            //lb_priority.Items.Add("총 금액");
        }

        public async void initUI()
        {


            //visible column list 조회
            GetColumnList();

            // DataHandler.finalClusteringData를 확장한 DataTable 생성
            DataTable enhancedClusteringData = await CreateEnhancedClusteringData();


            // 메인 UI 스레드로 돌아가서 DataHandler 등록
            await Task.Run(() =>
            {
                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0].Invoke((MethodInvoker)delegate
                    {

                        LoadPagedDataAsync();

                        Debug.WriteLine("");

                        //CreateCheckDataGridView(dataGridView_classify, DataHandler.finalClusteringData);


                        // 확장된 DataTable을 이용하여 dataGridView_classify 구성
                        CreateCheckDataGridView(dataGridView_classify, enhancedClusteringData);


                        //dataGridView_classify 이벤트 추가


                        AddSelectedColumnToGrid(dataGridView_delete_col2, dataGridView_keyword);

                        InitializePagingControls(true);

                        DataHandler.SyncDataGridViewSelections(dataGridView_origin, dataGridView_keyword);
                    });
                }
            });

        }

        private async Task<DataTable> CreateEnhancedClusteringData()
        {
            // 원본 데이터 복사
            DataTable enhancedTable = DataHandler.finalClusteringData.Copy();

            // 공급업체명과 부서명 컬럼 추가
            enhancedTable.Columns.Add(DataHandler.prod_col_name, typeof(string));
            enhancedTable.Columns.Add(DataHandler.dept_col_name, typeof(string));

            // ClusterID 기반으로 dataIndex 그룹화
            var clusterGroups = DataHandler.finalClusteringData.AsEnumerable()
                .GroupBy(row => row.Field<int>("ClusterID"))
                .ToDictionary(g => g.Key, g => g.ToList());

            // 각 ClusterID에 대한 dataIndex 수집
            Dictionary<int, List<int>> clusterToDataIndices = new Dictionary<int, List<int>>();

            foreach (var clusterGroup in clusterGroups)
            {
                int clusterId = clusterGroup.Key;
                List<int> dataIndices = new List<int>();

                foreach (var row in clusterGroup.Value)
                {
                    string dataIndexStr = row["dataIndex"].ToString();
                    if (!string.IsNullOrEmpty(dataIndexStr))
                    {
                        // 쉼표로 구분된 dataIndex 파싱
                        string[] indices = dataIndexStr.Split(',');
                        foreach (string indexStr in indices)
                        {
                            if (int.TryParse(indexStr.Trim(), out int index))
                            {
                                dataIndices.Add(index);
                            }
                        }
                    }
                }

                // 중복 제거
                clusterToDataIndices[clusterId] = dataIndices.Distinct().ToList();
            }

            // 각 ClusterID에 대한 공급업체명과 부서명 조회 및 설정
            foreach (var kvp in clusterToDataIndices)
            {
                int clusterId = kvp.Key;
                List<int> dataIndices = kvp.Value;

                if (dataIndices.Count == 0)
                    continue;

                // ID 목록 문자열 생성
                string idListStr = string.Join(",", dataIndices);

                // 공급업체명 및 부서명 조회
                string query = $@"
            SELECT DISTINCT {DataHandler.prod_col_name}, {DataHandler.dept_col_name}
            FROM raw_data
            WHERE id IN ({idListStr})";

                DataTable resultTable = DBManager.Instance.ExecuteQuery(query);

                // 결과가 없으면 다음으로
                if (resultTable.Rows.Count == 0)
                    continue;

                // 공급업체명 및 부서명 집계
                HashSet<string> uniqueProds = new HashSet<string>();
                HashSet<string> uniqueDepts = new HashSet<string>();

                foreach (DataRow resultRow in resultTable.Rows)
                {
                    string prod = resultRow[DataHandler.prod_col_name]?.ToString();
                    string dept = resultRow[DataHandler.dept_col_name]?.ToString();

                    if (!string.IsNullOrEmpty(prod))
                        uniqueProds.Add(prod);

                    if (!string.IsNullOrEmpty(dept))
                        uniqueDepts.Add(dept);
                }

                // 쉼표로 구분된 문자열로 변환
                string combinedProds = string.Join(",", uniqueProds);
                string combinedDepts = string.Join(",", uniqueDepts);

                // enhancedTable에 해당 ClusterID를 가진 모든 행에 값 설정
                foreach (DataRow enhancedRow in enhancedTable.Rows)
                {
                    if (!enhancedRow.IsNull("ClusterID") && enhancedRow.Field<int>("ClusterID") == clusterId)
                    {
                        enhancedRow[DataHandler.prod_col_name] = combinedProds;
                        enhancedRow[DataHandler.dept_col_name] = combinedDepts;
                    }
                }
            }

            return enhancedTable;
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


            // 컨트롤 활성화 
            //EnablePagingControls(false);

            // 이벤트 등록은 옵션에 따라 결정
            if (attachEvents)
            {
                AttachPagingEvents();
            }

            // 초기 페이징 상태
            UpdatePaginationInfo();

            DataHandler.RegisterDataGridView(dataGridView_delete_col2);
        }

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
            foreach (DataGridView dgv in new[] { dataGridView_origin, dataGridView_keyword })
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

        public DataTable GetPagedRawDataWithClusters(int pageNumber, int pageSize, List<string> columnList, bool hiddenTableYN = false)
        {
            Debug.WriteLine($"columnList : {String.Join(", ", columnList)}");


            // 기존 쿼리로 데이터 가져오기
            DataTable result = dataConverter.GetPagedProcessData(pageNumber, pageSize, columnList, hiddenTableYN);

            /*
            // 클러스터명 컬럼 추가
            if (!result.Columns.Contains("클러스터명"))
            {
                result.Columns.Add("클러스터명", typeof(string));
            }

            // finalClusteringData에서 각 행의 클러스터명 찾아 설정
            foreach (DataRow row in result.Rows)
            {
                int rawDataId = Convert.ToInt32(row["id"]);
                string clusterName = FindClusterNameForRawDataId(rawDataId);
                row["클러스터명"] = clusterName;
            }
            */
            return result;
        }

        private string FindClusterNameForRawDataId(int rawDataId)
        {
            Dictionary<int, string> clusterNameMap = new Dictionary<int, string>();
            foreach (DataRow row in DataHandler.finalClusteringData.Rows)
            {
                if (row["ID"] != DBNull.Value && row["ClusterID"] != DBNull.Value)
                {
                    int id = Convert.ToInt32(row["ID"]);
                    int clusterId = Convert.ToInt32(row["ClusterID"]);

                    if (id == clusterId) // ID와 ClusterID가 일치하는 경우만
                    {
                        string clusterName = row["클러스터명"]?.ToString();
                        if (!string.IsNullOrEmpty(clusterName))
                        {
                            clusterNameMap[id] = clusterName;
                        }
                    }
                }
            }

            foreach (DataRow clusterRow in DataHandler.finalClusteringData.Rows)
            {
                if (clusterRow["ClusterID"] == DBNull.Value) continue;

                int clusterId = Convert.ToInt32(clusterRow["ClusterID"]);
                string dataIndices = clusterRow["dataIndex"]?.ToString();

                if (clusterNameMap.ContainsKey(clusterId) && !string.IsNullOrEmpty(dataIndices))
                {
                    string[] indexStrings = dataIndices.Split(',');
                    foreach (string indexStr in indexStrings)
                    {
                        if (int.TryParse(indexStr.Trim(), out int index) && index == rawDataId)
                        {
                            return clusterNameMap[clusterId];
                        }
                    }
                }
            }

            return string.Empty; // 클러스터명을 찾지 못한 경우
        }

        public void PopulateDataGridViewWithClusterNames(DataGridView dataGridView_keyword,
                                              DataTable processTable,
                                              DataTable finalClusteringData)
        {
            try
            {
                // 1. "클러스터명" 컬럼이 processTable에 없으면 추가
                if (!processTable.Columns.Contains("클러스터명"))
                {
                    processTable.Columns.Add("클러스터명", typeof(string));
                }


                // hidden_rows에서 row_id 목록 가져오기
                List<int> hiddenRowIds = GetHiddenRowIds();

                // processTable에 hiddenYN 컬럼이 없으면 추가
                if (!processTable.Columns.Contains("hiddenYN"))
                {
                    processTable.Columns.Add("hiddenYN", typeof(int));
                }

                // 모든 행에 대해 raw_data_id와 hidden 상태 비교하여 hiddenYN 값 설정
                foreach (DataRow row in processTable.Rows)
                {
                    if (row["raw_data_id"] != DBNull.Value)
                    {
                        int rawDataId = Convert.ToInt32(row["raw_data_id"]);
                        // hidden_rows에 있으면 0(숨김), 없으면 1(표시)
                        row["hiddenYN"] = hiddenRowIds.Contains(rawDataId) ? 0 : 1;
                    }
                    else
                    {
                        // raw_data_id가 없는 경우 기본값으로 1(표시) 설정
                        row["hiddenYN"] = 1;
                    }
                }


                // 1.5 clusterID와 id가 일치하는 행의 클러스터명 매핑 만들기
                Dictionary<int, string> clusterNameMap = new Dictionary<int, string>();
                foreach (DataRow row in finalClusteringData.Rows)
                {
                    if (row["ID"] != DBNull.Value && row["ClusterID"] != DBNull.Value)
                    {
                        int id = Convert.ToInt32(row["ID"]);
                        int clusterId = Convert.ToInt32(row["ClusterID"]);

                        if (id == clusterId) // ID와 ClusterID가 일치하는 경우만
                        {
                            string clusterName = row["클러스터명"]?.ToString();
                            if (!string.IsNullOrEmpty(clusterName))
                            {
                                clusterNameMap[id] = clusterName;
                            }
                        }
                    }
                }

                // 2. finalClusteringData를 기반으로 processTable의 "클러스터명" 컬럼 채우기
                foreach (DataRow clusterRow in finalClusteringData.Rows)
                {
                    if (clusterRow["ClusterID"] == DBNull.Value) continue;

                    int clusterId = Convert.ToInt32(clusterRow["ClusterID"]);
                    string dataIndices = clusterRow["dataIndex"]?.ToString();

                    // 매핑된 클러스터명 찾기
                    if (clusterNameMap.ContainsKey(clusterId) && !string.IsNullOrEmpty(dataIndices))
                    {
                        string clusterName = clusterNameMap[clusterId];

                        // 인덱스 처리
                        string[] indexStrings = dataIndices.Split(',');
                        foreach (string indexStr in indexStrings)
                        {
                            if (int.TryParse(indexStr.Trim(), out int rawDataId))
                            {
                                // raw_data_id 값과 일치하는 행 찾기
                                foreach (DataRow row in processTable.Rows)
                                {

                                    if (row["raw_data_id"] != DBNull.Value &&
                                        Convert.ToInt32(row["raw_data_id"]) == rawDataId)
                                    {
                                        // 일치하는 raw_data_id를 가진 행에 클러스터명 설정
                                        row["클러스터명"] = clusterName;
                                        break; // 일치하는 첫 번째 행을 찾았으면 루프 종료
                                    }
                                }
                            }
                        }
                    }
                }

                // 3. DataGridView에 데이터 바인딩
                dataGridView_keyword.DataSource = processTable;

                // 4. 필요시 그리드뷰 설정 추가
                //dataGridView_keyword.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                //dataGridView_keyword.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView_keyword.Columns["raw_data_id"].Visible = false;
                dataGridView_keyword.Columns["hiddenYN"].Visible = false;
                dataGridView_keyword.Columns["import_date"].Visible = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PopulateDataGridViewWithClusterNames: {ex.Message}");
                throw;
            }
        }

        public List<int> GetHiddenRowIds()
        {
            DBManager dbManager = DBManager.Instance;
            DataTable hiddenRowsTable = dbManager.ExecuteQuery("SELECT row_id FROM hidden_rows WHERE original_table = 'raw_data'");

            List<int> hiddenRowIds = new List<int>();
            foreach (DataRow row in hiddenRowsTable.Rows)
            {
                if (row["row_id"] != DBNull.Value)
                {
                    hiddenRowIds.Add(Convert.ToInt32(row["row_id"]));
                }
            }

            return hiddenRowIds;
        }

        public void CreateCheckDataGridView(DataGridView dgv, DataTable dt)
        {
            // 조건에 맞는 데이터만 필터링
            var filteredData = dt.AsEnumerable()
                .Where(row =>
                    Convert.ToInt32(row["ClusterID"]) <= 0 ||
                    Convert.ToInt32(row["ClusterID"]) == Convert.ToInt32(row["ID"]))
                .CopyToDataTable();

            dgv.DataSource = filteredData;

            // ID 컬럼 숨기기
            dgv.Columns["ID"].Visible = false;
            // ClusterID 컬럼 숨기기
            dgv.Columns["ClusterID"].Visible = false;

            // dataIndex 컬럼 숨기기
            dgv.Columns["dataIndex"].Visible = false;

            if (dgv.Columns["Count"] != null)
            {
                dgv.Columns["Count"].DefaultCellStyle.Format = "N0"; // 천 단위 구분자
                dgv.Columns["Count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            }

            if (dgv.Columns["합산금액"] != null)
            {
                dgv.Columns["합산금액"].DefaultCellStyle.Format = "N0"; // 천 단위 구분자
                dgv.Columns["합산금액"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // DataGridView 속성 설정
            dgv.AllowUserToAddRows = false;
            //dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 나머지 컬럼들은 읽기 전용으로 설정
            for (int i = 1; i < dgv.Columns.Count; i++)
            {
                dgv.Columns[i].ReadOnly = true;
            }

            dgv.Columns["클러스터명"].ReadOnly = false;  // 클러스터명 편집 가능
            //dgv.CellEndEdit += DataGridView_CellEndEdit;
            //dgv.Font = new System.Drawing.Font("맑은 고딕", 14.25F);
            dgv.Font = new System.Drawing.Font("맑은 고딕", 9F);
            // "클러스터명" 컬럼의 배경색을 연노란색으로 설정
            dgv.Columns["클러스터명"].DefaultCellStyle.BackColor = System.Drawing.Color.LightYellow;
        }



        public DataTable ConvertDataGridViewToCustomDataTable(DataGridView dgv)
        {
            try
            {
                // 새 DataTable 생성
                DataTable result = new DataTable();

                // 열 정보 가져오기
                List<int> columnsToKeep = new List<int>();
                List<int> decimalColumns = new List<int>();

                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    // 1, 0, 1, 6번 컬럼 제외 (인덱스 기준)
                    if (i != 0 && i != 1 && i != 6)
                    {
                        columnsToKeep.Add(i);

                        // 4, 5번 컬럼은 decimal로 변환 (인덱스 기준)
                        if (i == 4 || i == 5)
                        {
                            decimalColumns.Add(i);
                        }
                    }
                }

                // 유지할 열 추가
                int newColIndex = 0;
                foreach (int originalIndex in columnsToKeep)
                {
                    DataGridViewColumn originalColumn = dgv.Columns[originalIndex];
                    Type dataType = typeof(string); // 기본 타입은 string

                    // decimal로 변환할 열 처리
                    if (decimalColumns.Contains(originalIndex))
                    {
                        dataType = typeof(decimal);
                    }

                    result.Columns.Add(originalColumn.HeaderText, dataType);
                    newColIndex++;
                }

                // 행 데이터 추가
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        DataRow newRow = result.NewRow();
                        newColIndex = 0;

                        foreach (int originalIndex in columnsToKeep)
                        {
                            object cellValue = row.Cells[originalIndex].Value;

                            // 2번, 3번 컬럼의 경우 100자로 제한
                            if (originalIndex == 2 || originalIndex == 3)
                            {
                                if (cellValue != null && cellValue != DBNull.Value)
                                {
                                    string strValue = cellValue.ToString();
                                    if (strValue.Length > 100)
                                    {
                                        cellValue = strValue.Substring(0, 97) + "...";
                                    }
                                }
                            }


                            // decimal 컬럼 처리
                            if (decimalColumns.Contains(originalIndex))
                            {
                                if (cellValue != null && cellValue != DBNull.Value)
                                {
                                    // 숫자로 변환 시도
                                    if (decimal.TryParse(cellValue.ToString(), out decimal decValue))
                                    {
                                        newRow[newColIndex] = decValue;
                                    }
                                    else
                                    {
                                        newRow[newColIndex] = 0m; // 변환 실패 시 0으로 설정
                                    }
                                }
                                else
                                {
                                    newRow[newColIndex] = 0m;
                                }
                            }
                            else
                            {
                                newRow[newColIndex] = cellValue ?? DBNull.Value;
                            }

                            newColIndex++;
                        }

                        result.Rows.Add(newRow);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DataGridView 변환 중 오류 발생: {ex.Message}");
                throw;
            }
        }

        public async Task ExportToExcelAsync(List<string> columnList, bool hiddenTableYN = false)
        {
            try
            {
                using (var progress = new ProcessProgressForm())
                {
                    progress.Show();
                    await Task.Delay(10);

                    // 1단계: export_result 데이터 테이블 생성 (80%)
                    await progress.UpdateProgressHandler(5, "데이터 조회 준비 중...");
                    DataTable export_result = await dataConverter.GetAllRawDataWithClustersAsync(
                        columnList,
                        hiddenTableYN,
                        maxThreads: 4,
                        initialProgress: 5,
                        maxProgress: 80,
                        progressHandler: progress.UpdateProgressHandler  // 프로그레스 핸들러 전달
                    );

                    // 2단계: cluster_result 데이터 테이블 생성 (10%)
                    await progress.UpdateProgressHandler(85, "클러스터 정보 가져오는 중...");
                    DataTable cluster_result = ConvertDataGridViewToCustomDataTable(dataGridView_classify);

                    // 3단계: Excel 저장 (10%)
                    await progress.UpdateProgressHandler(90, "Excel 파일 저장 중...");
                    DataHandler.SaveDataTableToExcel(cluster_result, export_result);

                    await progress.UpdateProgressHandler(100, "Excel 파일 저장 완료");
                    await Task.Delay(500); // 완료 메시지 표시
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excel 파일 저장 중 오류 발생: {ex.Message}");
                MessageBox.Show($"Excel 파일 저장 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btn_save_excel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
               $"Excel File을 생성하시겠습니까?",
               "Excel 생성",
               MessageBoxButtons.YesNo,
               MessageBoxIcon.Question
           );

            if (result == DialogResult.Yes)
            {
                /*
                //export DataTable 생성
                export_result = CreateFilteredDataTable(dataGridView_keyword);

                cluster_result = ConvertDataGridViewToCustomDataTable(dataGridView_classify);

                DataHandler.SaveDataTableToExcel(cluster_result, export_result);
                */
                // 기존 코드를 새 함수 호출로 대체
                //List<string> columnList = GetVisibleColumns(); // 보이는 컬럼 목록을 가져오는 함수 (필요시 구현)

                await ExportToExcelAsync(process_col_list, DataHandler.hiddenData);
            }
            else
            {
                return;
            }


        }

        public void GetColumnList()
        {
            List<string> columnList = new List<string>();

            // 1. is_visible=1인 컬럼 목록 가져오기
            string columnsQuery = @"
                                    SELECT original_name 
                                    FROM column_mapping 
                                    WHERE is_visible = 1 
                                    ORDER BY sequence";

            DataTable visibleColumnsTable = DBManager.Instance.ExecuteQuery(columnsQuery);
            List<string> visibleColumns = visibleColumnsTable.AsEnumerable()
                .Select(row => row["original_name"].ToString())
                .ToList();

            foreach (string columnName in visibleColumns)
            {
                columnList.Add(columnName);
            }
            process_col_list = columnList;

            //import_date 삭제
            process_col_list.Remove("import_date");
            //Debug.WriteLine($"process_col_list : {process_col_list.ToList()}");

            Debug.WriteLine($"process_col_list count : {process_col_list.Count}");

        }


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



        private async void dataGridView_classify_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Debug.WriteLine("call dataGridView_classify_CellValueChanged");
            // "클러스터명" 컬럼이 변경되었을 때만 처리

            if (e.ColumnIndex == dataGridView_classify.Columns["클러스터명"].Index && e.RowIndex >= 0)
            {
                //UpdateClusterName(dataGridView_keyword, DataHandler.processTable, DataHandler.finalClusteringData, e.RowIndex);

                // 수정된 값 가져오기
                string newValue = dataGridView_classify.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";

                if ("".Equals(newValue))
                {
                    //함수 재조회로 변경
                    CreateCheckDataGridView(dataGridView_classify, DataHandler.finalClusteringData);
                    return;
                }

                // DataHandler.finalClusteringData 업데이트
                int id = Convert.ToInt32(dataGridView_classify.Rows[e.RowIndex].Cells["ID"].Value);
                DataRow[] rows = DataHandler.finalClusteringData.Select($"ID = {id}");
                if (rows.Length > 0)
                {
                    rows[0]["클러스터명"] = newValue;
                }

                Debug.WriteLine($"변경된 클러스터명 {newValue}");

                // 변경 사항 저장
                DataHandler.finalClusteringData.AcceptChanges();

                //화면 갱신
                await LoadPagedDataAsync();

                //PopulateDataGridViewWithClusterNames(dataGridView_keyword, DataHandler.processTable, DataHandler.finalClusteringData);
            }

        }


        private void restore_col_btn_Click(object sender, EventArgs e)
        {
            List<string> restore_list = GetCheckedRowsData(dataGridView_delete_col2);

            foreach (string col_name in restore_list)
            {
                dataGridView_keyword.Columns[col_name].Visible = true;
            }



            for (int i = 0; i < dataGridView_keyword.Columns.Count; i++)
            {
                //부서,공급업체명,금액,타겟열,클러스터명은 제외
                if (dataGridView_keyword.Columns[i].Name.Equals(DataHandler.dept_col_name) || dataGridView_keyword.Columns[i].Name.Equals(DataHandler.prod_col_name) || dataGridView_keyword.Columns[i].Name.Equals(DataHandler.sub_acc_col_name))
                {
                    continue;
                }

                if (DataHandler.levelName.Contains(dataGridView_keyword.Columns[i].Name) || "클러스터명".Equals(dataGridView_keyword.Columns[i].Name))
                {
                    continue;
                }


                if (restore_list.Contains(dataGridView_keyword.Columns[i].Name))
                {
                    dataGridView_keyword.Columns[i].Visible = true;

                    // SQLite에서 컬럼 가시성 업데이트
                    dataConverter.UpdateColumnVisibility(dataGridView_keyword.Columns[i].Name, true);
                }
                else
                {
                    dataGridView_keyword.Columns[i].Visible = false;

                    // SQLite에서 컬럼 가시성 업데이트
                    dataConverter.UpdateColumnVisibility(dataGridView_keyword.Columns[i].Name, false);
                }
            }

            GetColumnList();

        }

        private void del_col_list_allcheck_CheckedChanged(object sender, EventArgs e)
        {
            // 모든 행의 체크박스 상태 변경
            foreach (DataGridViewRow row in dataGridView_delete_col2.Rows)
            {
                row.Cells[0].Value = del_col_list_allcheck.Checked;
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

        private async Task LoadPagedDataAsync()
        {

            if (isProcessingSearch) return;

            try
            {
                //재귀 호출 방지
                isProcessingSearch = true;

                // dataConverter가 null인지 확인하고 필요한 경우 초기화
                if (dataConverter == null)
                {
                    dataConverter = new DataConverter();
                }

                using (var loadingForm = new ProcessProgressForm())
                {
                    loadingForm.Show();
                    loadingForm.UpdateProgressHandler(10);

                    await Task.Delay(10);

                    // 페이징된 데이터 가져오기
                    DataTable pageData = null;
                    DataTable processPageData = null;

                    await Task.Run(() =>
                    {
                        pageData = dataConverter.GetPagedRawData(currentPage, pageSize, DataHandler.hiddenData);
                        Task.Delay(10);
                        Debug.WriteLine("origin page load complete");
                        loadingForm.UpdateProgressHandler(40);
                        processPageData = GetPagedRawDataWithClusters(currentPage, pageSize, process_col_list, DataHandler.hiddenData);

                        Debug.WriteLine("process page load complete");

                        Task.Delay(10);

                        loadingForm.UpdateProgressHandler(70);
                    });


                    await Task.Delay(10);

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

                    Debug.WriteLine("page infoamtion extract complete");

                    loadingForm.UpdateProgressHandler(80);

                    await Task.Delay(10);

                    // UI 업데이트는 메인 스레드에서 수행
                    this.BeginInvoke(new Action(() =>
                    {
                        //dataGridView_target.DataSource = pageData;
                        //dataGridView_process.DataSource = pageData;
                        DataHandler.ConfigureDataGridView(pageData, dataGridView_origin);
                        DataHandler.ConfigureDataGridView(processPageData, dataGridView_keyword);

                        Debug.WriteLine("ConfigureDataGridView complete");
                        UpdatePaginationInfo();

                        Debug.WriteLine("UpdatePaginationInfo complete");

                        //header style comple
                        ApplyGridFormatting();
                        Debug.WriteLine("ApplyGridFormattingt complete");

                        // 숨겨진 행 상태 적용
                        /*
                        if (hiddenRows.Count > 0)
                        {
                            ApplyHiddenRowsToGrids();
                        }
                        */
                    }));

                    Debug.WriteLine("page infoamtion extract complete");

                    loadingForm.UpdateProgressHandler(100);
                    await Task.Delay(100);
                    loadingForm.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"페이지 데이터 로드 중 오류: {ex.Message}");
                MessageBox.Show($"데이터 로드 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {

                isProcessingSearch = false;
            }
        }

        public void AddSelectedColumnToGrid(DataGridView targetDgv, DataGridView sourceDgv)
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

                foreach (string colName in process_col_list)
                {

                    //공급업체,부서명,금액,타겟열 제외
                    if (colName.Equals(DataHandler.dept_col_name) || colName.Equals(DataHandler.prod_col_name))
                    {
                        continue;
                    }

                    if (DataHandler.levelName.Contains(colName))
                    {
                        continue;
                    }

                    // 새 행 추가
                    int rowIndex = targetDgv.Rows.Add();
                    targetDgv.Rows[rowIndex].Cells["CheckBox"].Value = true;
                    targetDgv.Rows[rowIndex].Cells["Data"].Value = colName;  // 고정된 컬럼명 사용


                }
            }



        }


        //2025.04.28
        //상세 항목을 불러오는게 어려운 상황이라 우선 구현 skip
        private void dataGridView_classify_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
            // 헤더 행 클릭시 무시
            if (e.RowIndex < 0)
                return;

            // "클러스터명" 컬럼 클릭 시 이벤트 무시
            if (e.ColumnIndex >= 0 && dataGridView_classify.Columns[e.ColumnIndex].Name == "클러스터명")
                return;
            
            /*
            // 선택된 행에서 ClusterID 또는 ID 값 가져오기
            DataGridViewRow selectedRow = dataGridView_classify.Rows[e.RowIndex];

            // ClusterID 컬럼이 있으면 사용, 없으면 ID 컬럼 사용
            int selectedClusterId;
            string selectedClusterName;
            if (selectedRow.Cells["ClusterID"] != null && selectedRow.Cells["ClusterID"].Value != null)
            {
                selectedClusterId = Convert.ToInt32(selectedRow.Cells["ClusterID"].Value);
                selectedClusterName = selectedRow.Cells["클러스터명"].Value.ToString();
            }
            else if (selectedRow.Cells["ID"] != null && selectedRow.Cells["ID"].Value != null)
            {
                selectedClusterId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                selectedClusterName = selectedRow.Cells["클러스터명"].Value.ToString();
            }
            else
            {
                // ID, ClusterID 모두 없는 경우 예외 처리
                MessageBox.Show("클러스터 ID를 찾을 수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 팝업 폼 생성 및 표시
            ShowClusterDetailPopup(selectedClusterId , selectedClusterName);
            */
        }

        private void ShowClusterDetailPopup(int selectedClusterId ,string selectedClusterName)
        {
            // 팝업용 Form 생성
            Form popupForm = new Form
            {
                Text = $"클러스터 상세 내역 (클러스터명: {selectedClusterName})",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(1800, 1000),
                MinimizeBox = false,
                MaximizeBox = true,
                FormBorderStyle = FormBorderStyle.Sizable
            };

            // DataGridView 생성
            DataGridView detailGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                Font = new System.Drawing.Font("맑은 고딕", 9F)
            };

            // DataGridView 초기화
            detailGridView.Rows.Clear();
            detailGridView.Columns.Clear();

          

            // 원본 DataTable의 컬럼들 추가
            foreach (DataColumn col in DataHandler.finalClusteringData.Columns)
            {
                detailGridView.Columns.Add(col.ColumnName, col.ColumnName);
            }

            // 데이터 필터링 및 추가
            foreach (DataRow row in DataHandler.finalClusteringData.Rows)
            {
                if (!row.IsNull("ClusterID") && Convert.ToInt32(row["ClusterID"]) == selectedClusterId)
                {
                    int rowIndex = detailGridView.Rows.Add();
                    
                    for (int i = 0; i < DataHandler.finalClusteringData.Columns.Count; i++)
                    {
                        // 합산금액 컬럼은 포맷 적용
                        if ("합산금액".Equals(DataHandler.finalClusteringData.Columns[i].ColumnName))
                        {
                            //detailGridView.Rows[rowIndex].Cells[i].Value = FormatToKoreanUnit(Convert.ToDecimal(row[i]));
                            detailGridView.Rows[rowIndex].Cells[i].Value = Convert.ToDecimal(row[i]);
                        }
                        else
                        {
                            detailGridView.Rows[rowIndex].Cells[i].Value = row[i];
                        }
                    }
                }
            }

            // 필요한 컬럼 숨기기
            if (detailGridView.Columns["ID"] != null)
                detailGridView.Columns["ID"].Visible = false;

            if (detailGridView.Columns["ClusterID"] != null)
                detailGridView.Columns["ClusterID"].Visible = false;

            if (detailGridView.Columns["dataIndex"] != null)
                detailGridView.Columns["dataIndex"].Visible = false;

            if (detailGridView.Columns["import_date"] != null)
                detailGridView.Columns["import_date"].Visible = false;

            // Count 컬럼 포맷 설정
            if (detailGridView.Columns["Count"] != null)
            {
                detailGridView.Columns["Count"].DefaultCellStyle.Format = "N0";
                detailGridView.Columns["Count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // 기타 DataGridView 속성 설정
            detailGridView.AllowUserToAddRows = false;
            detailGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            detailGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            detailGridView.ReadOnly = false;

            // 나머지 컬럼들은 읽기 전용으로 설정
            for (int i = 1; i < detailGridView.Columns.Count; i++)
            {
                detailGridView.Columns[i].ReadOnly = true;
            }

            // 클러스터명 컬럼 설정
            if (detailGridView.Columns["클러스터명"] != null)
            {
                detailGridView.Columns["클러스터명"].ReadOnly = true;
                detailGridView.Columns["클러스터명"].Width = 400;
                detailGridView.Columns["클러스터명"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }

            // 타겟 컬럼 너비 고정
            if (DataHandler.levelName.Count > 0)
            {
                string lastLevelName = DataHandler.levelName[DataHandler.levelName.Count - 1];
                if (detailGridView.Columns[lastLevelName] != null)
                {
                    detailGridView.Columns[lastLevelName].Width = 400;
                    detailGridView.Columns[lastLevelName].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }

            // 나머지 컬럼은 자동 크기 조정
            for (int i = 1; i < detailGridView.Columns.Count; i++)
            {
                string colName = detailGridView.Columns[i].Name;
                if (colName != "클러스터명" &&
                    colName != DataHandler.prod_col_name &&
                    (DataHandler.levelName.Count == 0 || colName != DataHandler.levelName[DataHandler.levelName.Count - 1]))
                {
                    detailGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }

            // SortCompare 이벤트 핸들러 추가
            detailGridView.SortCompare -= DataHandler.money_SortCompare;
            detailGridView.SortCompare += DataHandler.money_SortCompare;

            // 컬럼 순서 재배치
            List<string> desiredOrder = new List<string>
                {
                    "Count",
                    DataHandler.sub_acc_col_name,
                    DataHandler.levelName[DataHandler.levelName.Count - 1],
                    DataHandler.prod_col_name,
                    DataHandler.dept_col_name,
                    "합산금액"
                };

            // 기존 컬럼 위치 저장
            Dictionary<string, int> originalIndices = new Dictionary<string, int>();
            for (int i = 0; i < detailGridView.Columns.Count; i++)
            {
                originalIndices[detailGridView.Columns[i].Name] = i;
            }

            // 새로운 컬럼 순서 설정
            for (int i = 0; i < desiredOrder.Count; i++)
            {
                string colName = desiredOrder[i];
                if (detailGridView.Columns.Contains(colName))
                {
                    detailGridView.Columns[colName].DisplayIndex = i;
                }
            }

            // 나머지 컬럼들은 안전하게 순서 설정
            var remainingColumns = detailGridView.Columns.Cast<DataGridViewColumn>()
                .Where(col => !desiredOrder.Contains(col.Name))
                .ToList();

            int nextIndex = desiredOrder.Count;
            foreach (var col in remainingColumns)
            {
                try
                {
                    col.DisplayIndex = nextIndex++;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // 오류 발생 시 최대 허용 인덱스로 설정
                    col.DisplayIndex = detailGridView.Columns.Count - 1;
                }
            }

            // 팝업 폼에 DataGridView 추가 및 표시
            popupForm.Controls.Add(detailGridView);
            popupForm.ShowDialog();
        }
    }
}