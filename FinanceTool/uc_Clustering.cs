using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using FinanceTool;
using Microsoft.VisualBasic.Devices;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace FinanceTool
{
    public partial class uc_Clustering : UserControl
    {

        DataTable mergeClusterDataTable;
        DataTable checkClusterDataTable;

        private decimal decimalDivider = 1;
        private string decimalDividerName = "원";
        private string selectecLv1Name = "";
        private bool equalsSearchYN = false;
        private bool andSearchYN = false;

        private bool isFinishSession = false;

        List<string> merge_keyword_list;
        List<string> check_keyword_list;
        List<string> supplier_keyword_list;

        // 전역 인스턴스 생성
        private static RecomandKeywordManager _recomandKeywordManager;

        public uc_Clustering()
        {
            InitializeComponent();
        }

        public async void initUI()
        {
            // Deep Copy 수행
            DataHandler.finalClusteringData = DataHandler.secondClusteringData.Copy();

            mergeClusterDataTable = EnrichWithRawTableData(DataHandler.finalClusteringData);

            // 검색 모드 UI 초기화
            //InitializeSearchModeUI();

            set_keyword_combo_list();

            create_merge_keyword_list(true);


            //최초 수행시만 별도 수행
            supplier_keyword_list = ExtractUniqueSupplierKeywords(mergeClusterDataTable, 0);

            // 메인 UI 스레드로 돌아가서 DataHandler 등록
            await Task.Run(() =>
            {
                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0].Invoke((MethodInvoker)delegate
                    {
                        //merge_check_table 초기화
                        merge_check_table.DataSource = null;
                        merge_check_table.Rows.Clear();
                        merge_check_table.Columns.Clear();
                        if (DataHandler.dragSelections.ContainsKey(merge_check_table))
                        {
                            DataHandler.dragSelections[merge_check_table].Clear();
                        }

                        Debug.WriteLine("RegisterDataGridView->match_keyword_table ");
                        DataHandler.RegisterDataGridView(merge_cluster_table);
                        DataHandler.RegisterDataGridView(dataGridView_lv1);
                        DataHandler.RegisterDataGridView(dataGridView_recoman_keyword);

                        Debug.WriteLine("RegisterDataGridView->complete ");
                        // 이벤트 핸들러 중복 등록 방지
                        decimal_combo.SelectedIndexChanged -= decimal_combo_SelectedIndexChanged; // 기존 핸들러 제거

                        decimal_combo.SelectedIndex = 0;

                        decimal_combo.SelectedIndexChanged += decimal_combo_SelectedIndexChanged;

                        //sorting 기준 변환
                        merge_cluster_table.SortCompare -= DataHandler.money_SortCompare;
                        merge_check_table.SortCompare -= DataHandler.money_SortCompare;
                        dataGridView_modified.SortCompare -= DataHandler.money_SortCompare;

                        merge_cluster_table.SortCompare += DataHandler.money_SortCompare;
                        merge_check_table.SortCompare += DataHandler.money_SortCompare;
                        dataGridView_modified.SortCompare += DataHandler.money_SortCompare;

                        dataGridView_modified.CellClick -= dataGridView_keyword_CellClick;


                        if (dataGridView_modified.Rows.Count > 0)
                        {
                            dataGridView_modified.DataSource = null;  // 먼저 DataSource를 null로 설정
                            dataGridView_modified.Rows.Clear();
                            dataGridView_modified.Columns.Clear();
                        }

                        /*
                        // 원본 DataTable의 컬럼들 추가
                        foreach (DataColumn col in DataHandler.recomandKeywordTable.Columns)
                        {
                            dataGridView_modified.Columns.Add(col.ColumnName, col.ColumnName);
                        }


                        // 데이터 필터링 및 추가
                        foreach (DataRow row in DataHandler.recomandKeywordTable.Rows)
                        {
                            int rowIndex = dataGridView_modified.Rows.Add();

                            // 데이터 채우기
                            for (int i = 0; i < DataHandler.recomandKeywordTable.Columns.Count; i++)
                            {
                                dataGridView_modified.Rows[rowIndex].Cells[i].Value = row[i];
                            }


                        }
                        */

                        // 초기 데이터 로드 후 업데이트
                        UpdateModifiedDataGridView();

                        // DataGridView 속성 설정
                        dataGridView_modified.AllowUserToAddRows = false;
                        dataGridView_modified.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridView_modified.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        dataGridView_modified.Font = new System.Drawing.Font("맑은 고딕", 14.25F);


                        // 나머지 컬럼들은 읽기 전용으로 설정
                        for (int i = 1; i < dataGridView_modified.Columns.Count; i++)
                        {
                            dataGridView_modified.Columns[i].ReadOnly = true;
                        }


                        dataGridView_modified.CellClick += dataGridView_keyword_CellClick;

                        //dataGridView_modified.SortCompare -= DataHandler.DataGridView1_SortCompare;
                        dataGridView_modified.SortCompare += DataHandler.money_SortCompare;

                        Debug.WriteLine("LoadSeparatorsAndRemovers");

                        LoadSeparatorsAndRemovers();


                    });
                }
            });
        }

        private void LoadSeparatorsAndRemovers()
        {
            // 프로그램 시작 시 로드
            _recomandKeywordManager = new RecomandKeywordManager();

            Debug.WriteLine("_recomandKeywordManager init complete");

            // 데이터 가져오기 및 중복 제거
            List<string> lv1_list = _recomandKeywordManager.Lv1List
                .Distinct()  // 중복 제거
                .ToList();   // List로 변환



            //구분자 리스트 추가
            create_keyword_table(dataGridView_lv1, lv1_list);


        }

        private void create_keyword_table(DataGridView dgv, List<string> data_list, bool lv1yn = true)
        {
            Debug.WriteLine("lv1 table init start");
            // DataGridView 초기화
            dgv.DataSource = null;
            dgv.Rows.Clear();
            dgv.Columns.Clear();
            if (DataHandler.dragSelections.ContainsKey(dgv))
            {
                DataHandler.dragSelections[dgv].Clear();
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
            dgv.Columns.Add(checkColumn);

            // 데이터 컬럼 추가
            DataGridViewTextBoxColumn dataColumn = new DataGridViewTextBoxColumn
            {
                Name = "Data",
                HeaderText = "데이터"
            };
            dgv.Columns.Add(dataColumn);

            // 데이터 리스트의 각 항목을 행으로 추가
            foreach (string data in data_list)
            {
                int rowIndex = dgv.Rows.Add();
                dgv.Rows[rowIndex].Cells["CheckBox"].Value = false;
                dgv.Rows[rowIndex].Cells["Data"].Value = data;
            }

            Debug.WriteLine("lv1 table init presrsaa");

            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Columns["Data"].ReadOnly = true;  // 체크박스 컬럼만 편집 가능
            dgv.Columns["CheckBox"].ReadOnly = false;  // 체크박스 컬럼만 편집 가능
            dgv.Font = new System.Drawing.Font("맑은 고딕", 14.25F);

            Debug.WriteLine("lv1 table init complete");

            if (lv1yn)
            {
                dgv.CellClick += dataGridView_lv1_CellClick;
            }
            else
            {
                dgv.CellClick += dataGridView_keyword_CellClick;
            }

        }

        private void dataGridView_lv1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (dgv == null) return;

            List<string> keywords;

            //Debug.WriteLine($"DataGridView_CellContentClick start => dragSelections[dgv].Count : {dragSelections[dgv].Count}");
            // 체크박스 컬럼이 아닌 다른 컬럼 클릭 시
            //if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            if (e.ColumnIndex != 0 && e.RowIndex >= 0)
            {
                string lv1Name = dataGridView_lv1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                selectecLv1Name = lv1Name;
                Lv1Item selectedItem = _recomandKeywordManager.GetLv1Item(lv1Name);


                if (selectedItem != null)
                {
                    keywords = selectedItem.Keywords;
                    create_keyword_table(dataGridView_recoman_keyword, keywords, false);
                }
            }


        }


        private void dataGridView_keyword_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            DataGridView dgv = sender as DataGridView;
            int valueIndex = 0;
            if (dgv == null) return;

            //키워드 요약 테이블은 0번, 키워드 추천 테이블은 1번
            // 그리드뷰 이름에 따라 valueIndex 설정
            if (dgv.Name == "dataGridView_modified")
            {
                valueIndex = 0; // dataGridView_modified일 경우 0번 인덱스 사용
            }
            else if (dgv.Name == "dataGridView_recoman_keyword")
            {
                valueIndex = 1; // dataGridView_recoman_keyword일 경우 1번 인덱스 사용
            }
            else
            {
                Debug.WriteLine($"Unknown DataGridView: {dgv.Name}");
                return; // 알 수 없는 그리드뷰인 경우 처리 중단
            }

            Debug.WriteLine($"DataGridView_CellContentClick start => dgv.Name : {dgv.Name} , valueIndex : {valueIndex}");
            // 체크박스 컬럼이 아닌 다른 컬럼 클릭 시
            //if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            if (e.ColumnIndex == valueIndex && e.RowIndex >= 0)
            {
                string keyword = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                List<string> MathcingPairs = new List<string>();

                Debug.WriteLine($"selected keyword : {keyword}");

                //2025.03.07 -정확히 일치하는 항목만 검색
                //MathcingPairs = DataHandler.FindMachKeyword(merge_keyword_list, keyword);
                MathcingPairs = DataHandler.FindMachEqualsKeyword(merge_keyword_list, keyword);


                if (MathcingPairs.Count > 0)
                {
                    //CreateFilteredDataGridView(merge_cluster_table, DataHandler.finalClusteringData, MathcingPairs);
                    if (!"".Equals(except_keyword.Text))
                    {
                        CreateFilteredDataGridView(merge_cluster_table, mergeClusterDataTable, MathcingPairs, except_keyword.Text.ToString());
                    }
                    else
                    {
                        CreateFilteredDataGridView(merge_cluster_table, mergeClusterDataTable, MathcingPairs);
                    }


                    merge_all_check.Checked = false;

                    change_row_count();
                }
                else
                {
                    merge_cluster_table.DataSource = null;
                    merge_cluster_table.Rows.Clear();
                    merge_cluster_table.Columns.Clear();

                    if (DataHandler.dragSelections.ContainsKey(merge_cluster_table))
                    {
                        DataHandler.dragSelections[merge_cluster_table].Clear();
                    }

                    change_row_count();
                }

                //테이블 표기 순서 변경
                // 맨 오른쪽으로 보낼 컬럼들
                /*
                List<string> columnsToMoveRight = new List<string> { "클러스터명", "키워드목록" };

                foreach (string columnName in columnsToMoveRight)
                {
                    if (merge_cluster_table.Columns.Contains(columnName))
                    {
                        // 해당 컬럼을 맨 마지막 인덱스로 이동
                        merge_cluster_table.Columns[columnName].DisplayIndex = merge_cluster_table.Columns.Count - 1;
                    }
                }
                */


            }
        }


        public DataTable EnrichWithRawTableData(DataTable inputTable)
        {
            DataTable resultTable = inputTable.Copy();


            try
            {
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

                if (visibleColumns.Count == 0)
                {
                    Debug.WriteLine("표시할 컬럼이 없습니다. column_mapping 테이블의 is_visible 속성을 확인하세요.");
                    return resultTable;
                }

                // 2. 결과 테이블에 컬럼 추가
                foreach (string column in visibleColumns)
                {
                    if (!resultTable.Columns.Contains(column))
                    {
                        resultTable.Columns.Add(column, typeof(string));
                    }
                }

                // 3. 모든 행에서 조회할 ID 목록 수집
                HashSet<int> rawTableIds = new HashSet<int>();
                Dictionary<int, List<DataRow>> idToRowsMap = new Dictionary<int, List<DataRow>>();

                foreach (DataRow row in resultTable.Rows)
                {
                    string dataIndices = row["dataIndex"].ToString();
                    if (string.IsNullOrEmpty(dataIndices))
                        continue;

                    string firstIndexStr = dataIndices.Split(',')[0].Trim();
                    if (!int.TryParse(firstIndexStr, out int firstIndex))
                        continue;

                    // rowindex를 RAW_TABLE의 id로 변환 
                    int rawTableId = firstIndex;

                    rawTableIds.Add(rawTableId);

                    // ID를 키로, 해당 ID를 참조하는 행들을 값으로 저장
                    if (!idToRowsMap.ContainsKey(rawTableId))
                    {
                        idToRowsMap[rawTableId] = new List<DataRow>();
                    }
                    idToRowsMap[rawTableId].Add(row);
                }

                if (rawTableIds.Count == 0)
                    return resultTable;

                // 4. 임시 테이블 생성 및 ID 삽입
                DBManager.Instance.ExecuteNonQuery("DROP TABLE IF EXISTS temp_ids");
                DBManager.Instance.ExecuteNonQuery("CREATE TEMP TABLE temp_ids (id INTEGER PRIMARY KEY)");

                // 배치로 나누어 ID 삽입
                const int batchSize = 100; // 더 작은 배치 크기
                List<int> idList = rawTableIds.ToList();

                using (var transaction = DBManager.Instance.BeginTransaction())
                {
                    for (int i = 0; i < idList.Count; i += batchSize)
                    {
                        // 현재 배치의 ID 범위 가져오기
                        int currentBatchSize = Math.Min(batchSize, idList.Count - i);
                        StringBuilder insertBatch = new StringBuilder("INSERT INTO temp_ids (id) VALUES ");

                        for (int j = 0; j < currentBatchSize; j++)
                        {
                            if (j > 0) insertBatch.Append(",");
                            insertBatch.Append($"({idList[i + j]})");
                        }

                        DBManager.Instance.ExecuteNonQuery(insertBatch.ToString());
                    }

                    transaction.Commit();
                }

                // 5. 임시 테이블을 사용하여 JOIN 쿼리 실행
                string columnsToSelect = "r.id, " + string.Join(", ", visibleColumns.Select(c => "r." + c));
                string joinQuery = $@"
            SELECT {columnsToSelect}
            FROM raw_data r
            JOIN temp_ids t ON r.id = t.id";

                DataTable rawData = DBManager.Instance.ExecuteQuery(joinQuery);

                // 6. 조회된 데이터를 결과 테이블에 매핑
                foreach (DataRow rawRow in rawData.Rows)
                {
                    int id = Convert.ToInt32(rawRow["id"]);

                    if (idToRowsMap.ContainsKey(id))
                    {
                        foreach (DataRow resultRow in idToRowsMap[id])
                        {
                            foreach (string column in visibleColumns)
                            {
                                resultRow[column] = rawRow[column];
                            }
                        }
                    }
                }

                // 7. 임시 테이블 삭제
                DBManager.Instance.ExecuteNonQuery("DROP TABLE IF EXISTS temp_ids");

                return resultTable;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RAW_TABLE 데이터 추가 중 오류 발생: {ex.Message}");
                // 예외 발생 시에도 임시 테이블 정리
                try { DBManager.Instance.ExecuteNonQuery("DROP TABLE IF EXISTS temp_ids"); } catch { }
                return resultTable;
            }
        }


        private async void create_merge_keyword_list(bool isAlreadyProgress = false)
        {
            if (isAlreadyProgress)
            {
                string target_keyword = "";

                //combobox로 검색할 경우
                if (merge_search_radio1.Checked)
                {
                    if (merge_keyword_combo.SelectedIndex != 0)
                    {
                        target_keyword = merge_keyword_combo.SelectedItem.ToString();
                    }
                }
                //직접 검색할 경우
                else if (merge_search_radio2.Checked)
                {
                    if (!"".Equals(merge_search_keyword.Text.ToString()) && merge_search_keyword.Text != null)
                    {
                        target_keyword = merge_search_keyword.Text.ToString();
                    }
                }

                List<string> MathcingPairs = new List<string>();

                if (!"".Equals(target_keyword))
                {
                    //쉼표 포함 시 다중검색 기능 표기
                    if (target_keyword.Contains(","))
                    {
                        andSearchYN = true;
                    }

                    Debug.WriteLine($"andSearchYN : {andSearchYN} ,equalsSearchYN : {equalsSearchYN} ");

                    // 키워드 검색 또는 공급업체 검색 여부에 따라 다른 리스트 사용
                    List<string> searchList = keyword_radio1.Checked ? merge_keyword_list : supplier_keyword_list;

                    //완전 일치 검색
                    if (equalsSearchYN)
                    {
                        MathcingPairs = DataHandler.FindMachEqualsKeyword(searchList, target_keyword);
                    }
                    //포함 검색
                    else
                    {

                        MathcingPairs = DataHandler.FindMachKeyword(searchList, target_keyword);
                    }

                    if (MathcingPairs.Count == 0)
                    {
                        Debug.WriteLine($"such result == 0");
                        merge_cluster_table.DataSource = null;
                        merge_cluster_table.Rows.Clear();
                        merge_cluster_table.Columns.Clear();
                        if (DataHandler.dragSelections.ContainsKey(merge_cluster_table))
                        {
                            DataHandler.dragSelections[merge_cluster_table].Clear();
                        }

                        change_row_count();
                        return;
                    }

                    if (!"".Equals(except_keyword.Text))
                    {
                        CreateFilteredDataGridView(merge_cluster_table, mergeClusterDataTable, MathcingPairs, except_keyword.Text.ToString(), keyword_radio2.Checked);
                    }
                    else
                    {
                        CreateFilteredDataGridView(merge_cluster_table, mergeClusterDataTable, MathcingPairs, null, keyword_radio2.Checked);
                    }
                }
                //전체 검색
                else
                {
                    CreateFilteredDataGridView(merge_cluster_table, mergeClusterDataTable, MathcingPairs, null, keyword_radio2.Checked);
                }

                merge_all_check.Checked = false;
                andSearchYN = false;
                change_row_count();
            }
            else
            {
                using (var progressForm = new ProcessProgressForm())
                {
                    progressForm.Show();

                    string target_keyword = "";

                    //combobox로 검색할 경우
                    if (merge_search_radio1.Checked)
                    {
                        if (merge_keyword_combo.SelectedIndex != 0)
                        {
                            target_keyword = merge_keyword_combo.SelectedItem.ToString();
                        }
                    }
                    //직접 검색할 경우
                    else if (merge_search_radio2.Checked)
                    {
                        if (!"".Equals(merge_search_keyword.Text.ToString()) && merge_search_keyword.Text != null)
                        {
                            target_keyword = merge_search_keyword.Text.ToString();
                        }
                    }

                    await progressForm.UpdateProgressHandler(10, "데이터 검색 시작");
                    await Task.Delay(10);

                    List<string> MathcingPairs = new List<string>();

                    if (!"".Equals(target_keyword))
                    {
                        //쉼표 포함 시 다중검색 기능 표기
                        if (target_keyword.Contains(","))
                        {
                            andSearchYN = true;
                        }

                        Debug.WriteLine($"andSearchYN : {andSearchYN} ,equalsSearchYN : {equalsSearchYN} ");

                        // 키워드 검색 또는 공급업체 검색 여부에 따라 다른 리스트 사용
                        List<string> searchList = keyword_radio1.Checked ? merge_keyword_list : supplier_keyword_list;

                        //완전 일치 검색
                        if (equalsSearchYN)
                        {
                            MathcingPairs = DataHandler.FindMachEqualsKeyword(searchList, target_keyword);
                        }
                        //포함 검색
                        else
                        {
                            MathcingPairs = DataHandler.FindMachKeyword(searchList, target_keyword);
                        }

                        if (MathcingPairs.Count == 0)
                        {
                            Debug.WriteLine($"such result == 0");
                            merge_cluster_table.DataSource = null;
                            merge_cluster_table.Rows.Clear();
                            merge_cluster_table.Columns.Clear();
                            if (DataHandler.dragSelections.ContainsKey(merge_cluster_table))
                            {
                                DataHandler.dragSelections[merge_cluster_table].Clear();
                            }

                            change_row_count();

                            await progressForm.UpdateProgressHandler(100);
                            await Task.Delay(10);
                            progressForm.Close();

                            return;
                        }

                        await progressForm.UpdateProgressHandler(40, "데이터 검색 중...");
                        await Task.Delay(10);

                        if (!"".Equals(except_keyword.Text))
                        {
                            CreateFilteredDataGridView(merge_cluster_table, mergeClusterDataTable, MathcingPairs, except_keyword.Text.ToString(), keyword_radio2.Checked);
                        }
                        else
                        {
                            CreateFilteredDataGridView(merge_cluster_table, mergeClusterDataTable, MathcingPairs, null, keyword_radio2.Checked);
                        }
                    }
                    //전체 검색
                    else
                    {
                        await progressForm.UpdateProgressHandler(40, "데이터 검색 중...");
                        await Task.Delay(10);

                        CreateFilteredDataGridView(merge_cluster_table, mergeClusterDataTable, MathcingPairs, null, keyword_radio2.Checked);
                    }

                    await progressForm.UpdateProgressHandler(90, "데이터 검색 완료");
                    await Task.Delay(10);

                    merge_all_check.Checked = false;
                    andSearchYN = false;
                    change_row_count();

                    await progressForm.UpdateProgressHandler(100);
                    await Task.Delay(10);
                    progressForm.Close();
                }
            }
        }

        private void change_row_count()
        {
            int rowCount = merge_cluster_table.RowCount;

            cluster_count.Text = $"행 수  : {rowCount}";

            int unClusterCount = GetCountOfNegativeOneClusterIDs(DataHandler.finalClusteringData);
            string unClusterCountMoney = GetSumOfNegativeTotalMoney(DataHandler.finalClusteringData);
            uncluster_count.Text = $"미병합 Cluster  : {unClusterCount}";
            uncluster_count_money.Text = $"미병합 합산금액  : {unClusterCountMoney}";
        }

        // DataTable에서 ClusterID가 -1인 행 개수 구하기
        public int GetCountOfNegativeOneClusterIDs(DataTable dataTable)
        {
            // DataTable이 null인지 확인
            if (dataTable == null)
                return 0;

            // "ClusterID" 컬럼이 존재하는지 확인
            if (!dataTable.Columns.Contains("ClusterID"))
                return 0;

            // LINQ를 사용하여 ClusterID가 -1인 행 개수 계산
            int count = dataTable.AsEnumerable()
                                 .Count(row => row.Field<int>("ClusterID") == -1);

            return count;
        }

        public string GetSumOfNegativeTotalMoney(DataTable dataTable)
        {
            // DataTable이 null인지 확인
            if (dataTable == null)
                return FormatToKoreanUnit(0);

            // "ClusterID" 컬럼이 존재하는지 확인
            if (!dataTable.Columns.Contains("ClusterID"))
                return FormatToKoreanUnit(0);

            // "합산금액" 컬럼이 존재하는지 확인
            if (!dataTable.Columns.Contains("합산금액"))
                return FormatToKoreanUnit(0);

            // LINQ를 사용하여 ClusterID가 -1인 행들의 합산금액 총합 계산
            decimal sum = dataTable.AsEnumerable()
                                  .Where(row => row.Field<int>("ClusterID") == -1)
                                  .Sum(row => row.Field<decimal>("합산금액"));

            return FormatToKoreanUnit(sum);
        }

        private void create_check_keyword_list()
        {
            string target_keyword = "";

            //combobox로 검색할 경우
            if (check_search_radio1.Checked)
            {
                if (check_search_combo.SelectedIndex != 0)
                {
                    target_keyword = check_search_combo.SelectedItem.ToString();
                }
            }
            //직접 검색할 경우
            else if (check_search_radio2.Checked)
            {
                if (!"".Equals(check_search_keyword.Text.ToString()) && check_search_keyword.Text != null)
                {
                    target_keyword = check_search_keyword.Text.ToString();
                }
            }


            List<string> MathcingPairs = new List<string>();
            try
            {
                if (!"".Equals(target_keyword))
                {
                    MathcingPairs = DataHandler.FindMachKeyword(check_keyword_list, target_keyword);
                    if (MathcingPairs.Count == 0)
                    {
                        merge_check_table.DataSource = null;
                        merge_check_table.Rows.Clear();
                        merge_check_table.Columns.Clear();
                        if (DataHandler.dragSelections.ContainsKey(merge_check_table))
                        {
                            DataHandler.dragSelections[merge_check_table].Clear();
                        }

                        return;
                    }
                    else
                    {
                        CreateCheckDataGridView(merge_check_table, DataHandler.finalClusteringData, MathcingPairs);
                    }

                }
                //전체 검색
                else
                {
                    CreateCheckDataGridView(merge_check_table, DataHandler.finalClusteringData, MathcingPairs);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        //checkFlag = 0 : merge table combo box
        //checkFlag = 1 : check table combo box
        public List<string> ExtractUniqueKeywords(DataTable dataTable, int checkFlag)
        {
            HashSet<string> uniqueKeywords = new HashSet<string>();

            foreach (DataRow row in dataTable.Rows)
            {


                if (!row.IsNull("ClusterID"))
                {
                    int clusterId = Convert.ToInt32(row["ClusterID"]);
                    //병합된 cluster의 키워드는 merge table combo box에서는 skip
                    if (checkFlag == 0 && clusterId > 0)
                    {
                        continue;
                    }

                    //clusterID가 없는 데이터는 check table combo box에서 skip
                    if (checkFlag == 1 && clusterId < 0)
                    {
                        continue;
                    }

                }
                else
                {
                    //clusterID가 없는 데이터는 check table combo box에서 skip
                    if (checkFlag == 1)
                    {
                        continue;
                    }
                }


                // 키워드목록 컬럼의 데이터 가져오기 (null 체크 포함)
                string keywordList = row["키워드목록"]?.ToString();

                if (!string.IsNullOrEmpty(keywordList))
                {
                    // 쉼표로 구분된 키워드를 분리하고 각각 HashSet에 추가
                    string[] keywords = keywordList.Split(',');
                    foreach (string keyword in keywords)
                    {
                        // 앞뒤 공백 제거 후 추가
                        string trimmedKeyword = keyword.Trim();
                        if (!string.IsNullOrEmpty(trimmedKeyword))
                        {
                            uniqueKeywords.Add(trimmedKeyword);
                        }
                    }
                }
            }

            // HashSet을 List로 변환하여 반환 (정렬된 상태로)
            return uniqueKeywords.OrderBy(k => k).ToList();
        }

        // 공급업체 키워드 추출 함수
        public List<string> ExtractUniqueSupplierKeywords(DataTable dataTable, int checkFlag)
        {
            HashSet<string> uniqueKeywords = new HashSet<string>();

            // dataTable이 null이거나 공급업체 컬럼이 없으면 빈 리스트 반환
            if (dataTable == null || !dataTable.Columns.Contains(DataHandler.prod_col_name))
            {
                Debug.WriteLine($"ExtractUniqueSupplierKeywords: dataTable에 {DataHandler.prod_col_name} 컬럼이 없거나 테이블이 null입니다.");
                return new List<string>();
            }

            foreach (DataRow row in dataTable.Rows)
            {
                if (!row.IsNull("ClusterID"))
                {
                    int clusterId = Convert.ToInt32(row["ClusterID"]);
                    //병합된 cluster의 키워드는 merge table combo box에서는 skip
                    if (checkFlag == 0 && clusterId > 0)
                    {
                        continue;
                    }

                    //clusterID가 없는 데이터는 check table combo box에서 skip
                    if (checkFlag == 1 && clusterId < 0)
                    {
                        continue;
                    }
                }
                else
                {
                    //clusterID가 없는 데이터는 check table combo box에서 skip
                    if (checkFlag == 1)
                    {
                        continue;
                    }
                }

                // 공급업체 컬럼 데이터 가져오기 (null 체크 포함)
                string supplierValue = row[DataHandler.prod_col_name]?.ToString();

                if (!string.IsNullOrEmpty(supplierValue))
                {
                    // 공급업체 값을 그대로 추가 (쉼표로 나누지 않음)
                    string trimmedValue = supplierValue.Trim();
                    if (!string.IsNullOrEmpty(trimmedValue))
                    {
                        uniqueKeywords.Add(trimmedValue);
                    }
                }
            }

            // HashSet을 List로 변환하여 반환 (정렬된 상태로)
            return uniqueKeywords.OrderBy(k => k).ToList();
        }

        private void set_keyword_combo_list()
        {
            merge_keyword_combo.Items.Clear();
            merge_keyword_combo.Items.Add("키워드 선택");

            check_search_combo.Items.Clear();
            check_search_combo.Items.Add("키워드 선택");

            // 키워드 리스트 생성
            merge_keyword_list = ExtractUniqueKeywords(DataHandler.finalClusteringData, 0);
            check_keyword_list = ExtractUniqueKeywords(DataHandler.finalClusteringData, 1);

            // 공급업체 리스트는 mergeClusterDataTable에서 생성
            if (mergeClusterDataTable != null && mergeClusterDataTable.Columns.Contains(DataHandler.prod_col_name))
            {
                supplier_keyword_list = ExtractUniqueSupplierKeywords(mergeClusterDataTable, 0);
                Debug.WriteLine($"공급업체 리스트 생성 완료: {supplier_keyword_list.Count}개 항목");
            }
            else
            {
                supplier_keyword_list = new List<string>();
                Debug.WriteLine("공급업체 리스트 생성 실패: mergeClusterDataTable이 null이거나 컬럼이 없음");
            }

            // 현재 선택된 검색 모드에 따라 콤보박스 내용 업데이트
            UpdateSearchComboBox();

            // 검증 콤보박스는 항상 키워드 기준
            foreach (string keyword in check_keyword_list)
            {
                if ("\\".Equals(keyword))
                {
                    continue;
                }
                check_search_combo.Items.Add(keyword);
            }

            check_search_combo.SelectedIndex = 0; // 첫 번째 열 선택
        }

        public void CreateFilteredDataGridView(DataGridView dgv, DataTable dt, List<string> filterWords, string except_keyword = null, bool isSupplierSearch = false)
        {
            // DataGridView 초기화
            dgv.DataSource = null;
            dgv.Rows.Clear();
            dgv.Columns.Clear();
            if (DataHandler.dragSelections.ContainsKey(dgv))
            {
                DataHandler.dragSelections[dgv].Clear();
            }

            Debug.WriteLine($"filterWords : {string.Join(",", filterWords)}");

            // CheckBox 컬럼 추가
            DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn();
            checkColumn.Name = "CheckBox";
            checkColumn.HeaderText = "";
            checkColumn.Width = 50;
            checkColumn.ThreeState = false;
            checkColumn.FillWeight = 20;  // 다른 컬럼들보다 작은 값 설정

            dgv.Columns.Add(checkColumn);


            // 원본 DataTable의 컬럼들 추가
            foreach (DataColumn col in dt.Columns)
            {
                dgv.Columns.Add(col.ColumnName, col.ColumnName);
            }

            // 데이터 필터링 및 추가
            foreach (DataRow row in dt.Rows)
            {
                // ClusterID 체크
                bool skipRow = false;
                if (!row.IsNull("ClusterID"))  // ClusterID가 null이 아니고
                {
                    int clusterId = Convert.ToInt32(row["ClusterID"]);
                    if (clusterId > 0)  // -1 이 아니면 skip
                    {
                        skipRow = true;
                    }
                }

                if (!skipRow)  // ClusterID 조건을 통과한 경우만 처리
                {
                    if (filterWords.Count > 0)
                    {
                        // 검색 대상 컬럼 결정 (키워드 또는 공급업체)
                        string searchColumnName = isSupplierSearch ? DataHandler.prod_col_name : "키워드목록";
                        string searchColumnValue = row[searchColumnName].ToString();

                        string searchColumnReplace = searchColumnValue.Replace(" ", "");

                        //제외 키워드 포함 시 해당 행 skip
                        //다중 키워드 기능 추가
                        if (!string.IsNullOrEmpty(except_keyword))
                        {
                            // 제외 키워드가 콤마로 구분되어 있는지 확인하고 리스트로 변환
                            List<string> exceptKeywordList = except_keyword.Split(',')
                                                            .Select(k => k.Trim())
                                                            .Where(k => !string.IsNullOrEmpty(k))
                                                            .ToList();

                            // 제외 키워드 목록 중 하나라도 포함되어 있으면 skip
                            if (exceptKeywordList.Any(exWord => searchColumnReplace.Contains(exWord)))
                            {
                                continue;
                            }
                        }

                        // 검색 방식에 따라 리스트 생성 방법 다르게 적용
                        List<string> valueList;
                        if (isSupplierSearch)
                        {
                            // 공급업체는 그냥 문자열 자체 사용
                            valueList = new List<string> { searchColumnValue.Trim() };
                        }
                        else
                        {
                            // 키워드 목록은 쉼표로 구분
                            valueList = searchColumnValue.Split(',')
                                                   .Select(k => k.Trim())
                                                   .Where(k => !string.IsNullOrEmpty(k))
                                                   .ToList();
                        }

                        //쉼표 기반 and 조건 검색 && 완전 일치일 경우
                        if (andSearchYN && equalsSearchYN)
                        {
                            // filterWords의 모든 단어가 valueList에 포함되어 있는지 확인 (대소문자 무시)
                            if (filterWords.All(filterWord => valueList.Any(value =>
                                string.Equals(value, filterWord, StringComparison.OrdinalIgnoreCase))))
                            {
                                int rowIndex = dgv.Rows.Add();
                                dgv.Rows[rowIndex].Cells["CheckBox"].Value = false;
                                for (int i = 0; i < dt.Columns.Count; i++)
                                {
                                    //합산금액 컬럼은 수정
                                    if ("합산금액".Equals(dt.Columns[i].ColumnName))
                                    {
                                        dgv.Rows[rowIndex].Cells[i + 1].Value = FormatToKoreanUnit(Convert.ToDecimal(row[i]));
                                    }
                                    else
                                    {
                                        dgv.Rows[rowIndex].Cells[i + 1].Value = row[i];
                                    }
                                }
                            }
                        }
                        //그 외 검색
                        else
                        {
                            // 공급업체 검색은 포함 여부만 체크, 키워드 검색은 정확한 일치 체크
                            bool match = false;
                            if (isSupplierSearch)
                            {
                                // 공급업체 검색은 포함 여부 체크
                                match = valueList.Any(value => filterWords.Any(filterWord =>
                                    value.IndexOf(filterWord, StringComparison.OrdinalIgnoreCase) >= 0));
                            }
                            else
                            {
                                // 키워드 검색은 정확한 일치 체크
                                match = valueList.Any(value => filterWords.Any(filterWord =>
                                    string.Equals(value, filterWord, StringComparison.OrdinalIgnoreCase)));
                            }

                            if (match)
                            {
                                int rowIndex = dgv.Rows.Add();
                                dgv.Rows[rowIndex].Cells["CheckBox"].Value = false;
                                for (int i = 0; i < dt.Columns.Count; i++)
                                {
                                    //합산금액 컬럼은 수정
                                    if ("합산금액".Equals(dt.Columns[i].ColumnName))
                                    {
                                        dgv.Rows[rowIndex].Cells[i + 1].Value = FormatToKoreanUnit(Convert.ToDecimal(row[i]));
                                    }
                                    else
                                    {
                                        dgv.Rows[rowIndex].Cells[i + 1].Value = row[i];
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int rowIndex = dgv.Rows.Add();
                        dgv.Rows[rowIndex].Cells["CheckBox"].Value = false;

                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            //합산금액 컬럼은 수정
                            if ("합산금액".Equals(dt.Columns[i].ColumnName))
                            {
                                //Debug.WriteLine("합산 금액 컬럼 수정 로직 적용");
                                dgv.Rows[rowIndex].Cells[i + 1].Value = FormatToKoreanUnit(Convert.ToDecimal(row[i]));
                            }
                            else
                            {
                                dgv.Rows[rowIndex].Cells[i + 1].Value = row[i];
                            }
                        }
                    }
                }
            }

            // ID 컬럼 숨기기
            dgv.Columns["ID"].Visible = false;
            // ClusterID 컬럼 숨기기
            dgv.Columns["ClusterID"].Visible = false;

            // dataIndex 컬럼 숨기기
            dgv.Columns["dataIndex"].Visible = false;

            // import_date 컬럼 숨기기
            if (dgv.Columns["import_date"] != null)
            {
                dgv.Columns["import_date"].Visible = false;
            }

            if (dgv.Columns["Count"] != null)
            {
                dgv.Columns["Count"].DefaultCellStyle.Format = "N0"; // 천 단위 구분자
                dgv.Columns["Count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // DataGridView 속성 설정
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = false;

            dgv.Columns["CheckBox"].ReadOnly = false;  // 체크박스 컬럼만 편집 가능

            // 나머지 컬럼들은 읽기 전용으로 설정
            for (int i = 1; i < dgv.Columns.Count; i++)
            {
                dgv.Columns[i].ReadOnly = true;
            }

            dgv.Columns["클러스터명"].ReadOnly = true;

            // 컬럼 너비 고정 설정

            // 클러스터명 컬럼의 너비 고정 (200픽셀로 설정)
            if (dgv.Columns["클러스터명"] != null)
            {
                dgv.Columns["클러스터명"].Width = 400;
                dgv.Columns["클러스터명"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }

            // 타겟 컬럼 너비 고정
            if (DataHandler.levelName.Count > 0)
            {
                string lastLevelName = DataHandler.levelName[DataHandler.levelName.Count - 1];
                if (dgv.Columns[lastLevelName] != null)
                {
                    dgv.Columns[lastLevelName].Width = 400;
                    dgv.Columns[lastLevelName].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }

            // 공급업체 컬럼 너비 고정
            /*
            if (dgv.Columns[DataHandler.prod_col_name] != null)
            {
                dgv.Columns[DataHandler.prod_col_name].Width = 200;
                dgv.Columns[DataHandler.prod_col_name].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            */

            // 나머지 컬럼은 자동 크기 조정
            for (int i = 1; i < dgv.Columns.Count; i++)
            {
                string colName = dgv.Columns[i].Name;
                if (colName != "클러스터명" &&
                    colName != DataHandler.prod_col_name &&
                    (DataHandler.levelName.Count == 0 || colName != DataHandler.levelName[DataHandler.levelName.Count - 1]))
                {
                    dgv.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }
            dgv.Font = new System.Drawing.Font("맑은 고딕", 9F);

            dgv.SortCompare -= DataHandler.money_SortCompare;
            dgv.SortCompare += DataHandler.money_SortCompare;

            //2025.04.28
            // 컬럼 순서 재배치
            //선택박스, Count수, 세목열, 타겟열, 공급업체열, 부서열,금액열
            List<string> desiredOrder = new List<string>
                {
                    "CheckBox",
                    "Count",
                    DataHandler.sub_acc_col_name,
                    DataHandler.levelName[DataHandler.levelName.Count - 1],
                    DataHandler.prod_col_name,
                    DataHandler.dept_col_name,
                    "합산금액"
                };

            // 기존 컬럼 위치 저장
            Dictionary<string, int> originalIndices = new Dictionary<string, int>();
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                originalIndices[dgv.Columns[i].Name] = i;
            }

            // 새로운 컬럼 순서 설정
            for (int i = 0; i < desiredOrder.Count; i++)
            {
                string colName = desiredOrder[i];
                if (dgv.Columns.Contains(colName))
                {
                    dgv.Columns[colName].DisplayIndex = i;
                }
            }

            // 나머지 컬럼들은 기존 순서 유지
            // 나머지 컬럼들은 기존 순서 유지하되 우선 순위가 낮은 컬럼으로 배치
            var remainingColumns = dgv.Columns.Cast<DataGridViewColumn>()
                                     .Where(col => !desiredOrder.Contains(col.Name))
                                     .OrderBy(col => originalIndices[col.Name])
                                     .ToList();

            int nextIndex = desiredOrder.Count;
            foreach (var col in remainingColumns)
            {
                col.DisplayIndex = nextIndex++;
            }
        }

        public void CreateCheckDataGridView(DataGridView dgv, DataTable dt, List<string> filterWords)
        {
            // DataGridView 초기화
            dgv.DataSource = null;
            dgv.Rows.Clear();
            dgv.Columns.Clear();
            if (DataHandler.dragSelections.ContainsKey(dgv))
            {
                DataHandler.dragSelections[dgv].Clear();
            }


            // CheckBox 컬럼 추가
            DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn();
            checkColumn.Name = "CheckBox";
            checkColumn.HeaderText = "";
            checkColumn.Width = 50;
            checkColumn.ThreeState = false;
            checkColumn.FillWeight = 20;  // 다른 컬럼들보다 작은 값 설정

            dgv.Columns.Add(checkColumn);


            // 원본 DataTable의 컬럼들 추가
            foreach (DataColumn col in dt.Columns)
            {
                dgv.Columns.Add(col.ColumnName, col.ColumnName);

            }

            // 데이터 필터링 및 추가
            foreach (DataRow row in dt.Rows)
            {
                // ClusterID 체크
                bool skipRow = true;
                if (!row.IsNull("ClusterID"))  // ClusterID가 null이 아니고
                {
                    int clusterId = Convert.ToInt32(row["ClusterID"]);
                    int rowId = Convert.ToInt32(row["ID"]);
                    if (clusterId == rowId)  // 0이 아니면 스킵
                    {
                        skipRow = false;
                    }
                }

                if (!skipRow)  // ClusterID 조건을 통과한 경우만 처리
                {
                    if (filterWords.Count > 0)
                    {
                        string keywordColumnValue = row["키워드목록"].ToString();

                        if (filterWords.Any(word => keywordColumnValue.Contains(word)))
                        {
                            int rowIndex = dgv.Rows.Add();
                            dgv.Rows[rowIndex].Cells["CheckBox"].Value = false;

                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                //합산금액 컬럼은 수정
                                if ("합산금액".Equals(dt.Columns[i].ColumnName))
                                {
                                    //Debug.WriteLine("합산 금액 컬럼 수정 로직 적용");
                                    dgv.Rows[rowIndex].Cells[i + 1].Value = FormatToKoreanUnit(Convert.ToDecimal(row[i]));
                                }
                                else
                                {
                                    dgv.Rows[rowIndex].Cells[i + 1].Value = row[i];
                                }
                            }
                        }
                    }
                    else
                    {
                        int rowIndex = dgv.Rows.Add();
                        dgv.Rows[rowIndex].Cells["CheckBox"].Value = false;

                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            //합산금액 컬럼은 수정
                            if ("합산금액".Equals(dt.Columns[i].ColumnName))
                            {
                                //Debug.WriteLine("합산 금액 컬럼 수정 로직 적용");
                                dgv.Rows[rowIndex].Cells[i + 1].Value = FormatToKoreanUnit(Convert.ToDecimal(row[i]));
                            }
                            else
                            {
                                dgv.Rows[rowIndex].Cells[i + 1].Value = row[i];
                            }
                        }
                    }
                }

            }

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



            // DataGridView 속성 설정
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = false;

            dgv.Columns["CheckBox"].ReadOnly = false;  // 체크박스 컬럼만 편집 가능

            // 나머지 컬럼들은 읽기 전용으로 설정
            for (int i = 1; i < dgv.Columns.Count; i++)
            {
                dgv.Columns[i].ReadOnly = true;
            }

            dgv.Columns["클러스터명"].ReadOnly = false;  // 클러스터명 편집 가능
            dgv.CellEndEdit -= DataGridView_CellEndEdit;
            dgv.CellEndEdit += DataGridView_CellEndEdit;
            dgv.CellBeginEdit -= DataGridView_CellBeginEdit; // 중복 등록 방지
            dgv.CellBeginEdit += DataGridView_CellBeginEdit;
            //dgv.Font = new System.Drawing.Font("맑은 고딕", 14.25F);
            // "클러스터명" 컬럼의 배경색을 연노란색으로 설정
            dgv.Columns["클러스터명"].DefaultCellStyle.BackColor = System.Drawing.Color.LightYellow;


            // ID 컬럼을 기준으로 내림차순 정렬
            dgv.Sort(dgv.Columns["ID"], System.ComponentModel.ListSortDirection.Descending);

            // 정렬 후 0번째 행이 있다면 선택
            if (dgv.Rows.Count > 0)
            {
                dgv.ClearSelection();
                dgv.Rows[0].Selected = true;
                dgv.CurrentCell = dgv.Rows[0].Cells[0];
            }
        }


        public List<string> GetCheckedRowsStringData(DataGridView dgv)
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

        //체크 항목 데이터 수집
        public List<int> GetCheckedRowsData(DataGridView dgv)
        {
            List<int> checkedData = new List<int>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                // CheckBox 컬럼(0번째)이 체크되었는지 확인
                if (row.Cells[0].Value != null &&
                    Convert.ToBoolean(row.Cells[0].Value) == true)
                {
                    // ID 컬럼의 값을 직접 int로 가져오기
                    if (row.Cells["ID"].Value != null)
                    {
                        int id = Convert.ToInt32(row.Cells["ID"].Value);
                        checkedData.Add(id);
                    }
                }
            }
            return checkedData;
        }

        public int GetCheckedRowsIndex(DataGridView dgv)
        {
            int checkedData = 0;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                // CheckBox 컬럼(0번째)이 체크되었는지 확인
                if (row.Cells[0].Value != null &&
                    Convert.ToBoolean(row.Cells[0].Value) == true)
                {
                    checkedData = row.Index;
                    break;
                }
            }
            return checkedData;
        }

        public void MergeAndCreateNewCluster(DataTable dataTable, List<int> targetIds, string clusterName = null, string clusterID = null)
        {
            if (targetIds == null || targetIds.Count == 0) return;

            int newId;
            bool isNewCluster = true;

            // clusterID가 주어진 경우, 해당 ID를 사용
            if (clusterID != null)
            {
                newId = Convert.ToInt32(clusterID);
                isNewCluster = false;  // 새 클러스터를 생성하지 않음
            }
            else
            {
                // 1. 가장 큰 ID 값 찾기
                int maxId = DataHandler.finalClusteringData.AsEnumerable()
                    .Max(row => Convert.ToInt32(row["ID"]));
                newId = maxId + 1;
            }

            //Debug.WriteLine($"[MergeAndCreateNewCluster] newId: {newId}, isNewCluster: {isNewCluster}");


            // 2. 병합할 행들의 데이터 수집
            var targetRows = dataTable.AsEnumerable()
                .Where(row => targetIds.Contains(Convert.ToInt32(row["ID"])))
                .ToList();

            if (targetRows.Count == 0) return;

            // 3. 병합 데이터 준비
            string mergedClusterName = "";

            if (clusterName == null || "".Equals(clusterName))
            {
                // 3. 병합 데이터 준비
                mergedClusterName = string.Join("_",
                    targetRows.Select(row => row["클러스터명"].ToString()));

                // 20자 제한 처리
                if (mergedClusterName.Length > 20)
                {
                    mergedClusterName = mergedClusterName.Substring(0, 17) + "...";
                }
            }
            else
            {
                mergedClusterName = clusterName;
            }


            string mergedKeywords = string.Join(",",
                targetRows.Select(row => row["키워드목록"].ToString()));

            int totalCount = targetRows.Sum(row =>
                Convert.ToInt32(row["Count"]));

            decimal totalAmount = targetRows.Sum(row =>
                Convert.ToDecimal(row["합산금액"]));

            if (isNewCluster)
            {
                // 4. 새로운 행 추가 (새 클러스터 생성 시)
                DataRow newRow = dataTable.NewRow();
                newRow["ID"] = newId;
                newRow["ClusterID"] = newId;
                newRow["클러스터명"] = mergedClusterName;
                newRow["키워드목록"] = mergedKeywords;
                newRow["Count"] = totalCount;
                newRow["합산금액"] = totalAmount;
                dataTable.Rows.Add(newRow);
            }
            else
            {
                // 4. 기존 클러스터 정보 업데이트
                DataRow existingRow = dataTable.AsEnumerable()
                    .FirstOrDefault(row => Convert.ToInt32(row["ID"]) == newId);

                if (existingRow != null)
                {
                    // 기존 클러스터에 추가될 항목들의 정보를 통합
                    string existingKeywords = existingRow["키워드목록"].ToString();
                    int existingCount = Convert.ToInt32(existingRow["Count"]);
                    decimal existingAmount = Convert.ToDecimal(existingRow["합산금액"]);

                    // 클러스터명 업데이트 (사용자 지정 클러스터명이 있으면 그것 사용)
                    if (clusterName != null && !string.IsNullOrEmpty(clusterName))
                    {
                        existingRow["클러스터명"] = clusterName;
                    }

                    // 키워드 목록 병합 (중복 제거)
                    string combinedKeywords = existingKeywords;
                    if (!string.IsNullOrEmpty(mergedKeywords))
                    {
                        // 키워드 목록을 합치되, 중복된 키워드는 한 번만 포함
                        HashSet<string> keywordSet = new HashSet<string>(
                            existingKeywords.Split(',').Select(k => k.Trim()));

                        foreach (string keyword in mergedKeywords.Split(',').Select(k => k.Trim()))
                        {
                            keywordSet.Add(keyword);
                        }

                        combinedKeywords = string.Join(",", keywordSet);
                    }

                    // 값 업데이트
                    existingRow["키워드목록"] = combinedKeywords;
                    existingRow["Count"] = existingCount + totalCount;
                    existingRow["합산금액"] = existingAmount + totalAmount;
                }
            }
            // 5. 병합된 행들의 ClusterID 업데이트
            foreach (var row in targetRows)
            {
                row["ClusterID"] = newId;
            }

            // 6. 변경사항 적용
            dataTable.AcceptChanges();
            mergeClusterDataTable = EnrichWithRawTableData(dataTable);
        }

        public void deleteClusterId(DataTable dataTable, List<int> targetIds)
        {

            Debug.WriteLine($"targetIds : {targetIds[0]}");

            // 삭제할 행들을 찾아서 리스트에 담기
            var rowsToDelete = dataTable.AsEnumerable()
                .Where(row => targetIds.Contains(Convert.ToInt32(row["ID"])))
                .ToList();

            // 찾은 행들을 삭제
            foreach (var row in rowsToDelete)
            {
                dataTable.Rows.Remove(row);
            }

            // 대상 ID를 가진 행들에 새로운 ClusterID 할당
            foreach (DataRow row in dataTable.Rows)
            {
                // null 체크 후 변환
                if (row["ClusterID"] != DBNull.Value)
                {
                    int rowId = Convert.ToInt32(row["ClusterID"]);
                    if (targetIds.Contains(rowId))
                    {
                        //row["ClusterID"] = DBNull.Value;  // null 대신 DBNull.Value 사용
                        row["ClusterID"] = -1;
                    }
                }

            }

            // 변경사항 적용
            dataTable.AcceptChanges();
            mergeClusterDataTable = EnrichWithRawTableData(dataTable);
        }


        private void merge_search_button_Click(object sender, EventArgs e)
        {
            create_merge_keyword_list();
        }

        private void merge_all_check_CheckedChanged(object sender, EventArgs e)
        {
            // 모든 행의 체크박스 상태 변경
            foreach (DataGridViewRow row in merge_cluster_table.Rows)
            {
                row.Cells[0].Value = merge_all_check.Checked;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            List<int> mergeIDlList = GetCheckedRowsData(merge_cluster_table);

            if (mergeIDlList.Count == 0)
            {
                MessageBox.Show("병합 대상을 선택하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }

            using (var progressForm = new ProcessProgressForm())
            {
                progressForm.Show();


                await progressForm.UpdateProgressHandler(10, "클러스터 병합 시작");
                await Task.Delay(10);


                MergeAndCreateNewCluster(DataHandler.finalClusteringData, mergeIDlList);

                await progressForm.UpdateProgressHandler(50, "클러스터 병합 진행중...");
                await Task.Delay(10);



                set_keyword_combo_list();

                //검색조건 초기화
                merge_keyword_combo.SelectedIndex = 0;
                merge_search_keyword.Text = "";

                await progressForm.UpdateProgressHandler(70, "클러스터 병합 결과 출력 중...");
                await Task.Delay(10);


                create_merge_keyword_list(true);
                create_check_keyword_list();

                await progressForm.UpdateProgressHandler(100);
                await Task.Delay(10);
                progressForm.Close();


                MessageBox.Show("클러스터 병합이 완료되었습니다.", "Info",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Information);
            }

            // 병합 작업 후 업데이트
            UpdateModifiedDataGridView();

            /*
            List<int> mergeIDlList = GetCheckedRowsData(merge_cluster_table);

            if (mergeIDlList.Count == 0)
            {
                MessageBox.Show("병합 대상을 선택하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }


            MergeAndCreateNewCluster(DataHandler.finalClusteringData, mergeIDlList);

            set_keyword_combo_list();

            //검색조건 초기화
            merge_keyword_combo.SelectedIndex = 0;
            merge_search_keyword.Text = "";

            create_merge_keyword_list();
            create_check_keyword_list();

            //DataHandler.SetupDataGridView(dataGridView_3rd, DataHandler.finalClusteringData);

            MessageBox.Show("클러스터 병합이 완료되었습니다.", "Info",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Information);
            */
        }

        private void merge_cancel_button_Click(object sender, EventArgs e)
        {
            List<int> mergeIDlList = GetCheckedRowsData(merge_check_table);

            if (mergeIDlList.Count == 0)
            {
                MessageBox.Show("병합 해제 대상을 선택하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }

            deleteClusterId(DataHandler.finalClusteringData, mergeIDlList);

            set_keyword_combo_list();

            //검색조건 초기화
            check_search_combo.SelectedIndex = 0;
            check_search_keyword.Text = "";


            create_merge_keyword_list();
            create_check_keyword_list();
            //DataHandler.SetupDataGridView(dataGridView_3rd, DataHandler.finalClusteringData);

            // 병합 작업 후 업데이트
            UpdateModifiedDataGridView();

            MessageBox.Show("클러스터 병합 해제가 완료되었습니다.", "Info",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Information);
        }
        // 클러스터명 원래 값을 저장할 Dictionary 추가 (클래스의 멤버 변수로 선언)
        private Dictionary<int, string> originalClusterNames = new Dictionary<int, string>();

        private void DataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;

            // 클러스터명 컬럼만 처리
            if (e.ColumnIndex == dgv.Columns["클러스터명"].Index)
            {
                // 현재 값 저장
                int rowId = e.RowIndex;
                string currentValue = dgv.Rows[rowId].Cells[e.ColumnIndex].Value?.ToString() ?? "";

                // 같은 키가 이미 있을 경우 업데이트, 없으면 추가
                if (originalClusterNames.ContainsKey(rowId))
                    originalClusterNames[rowId] = currentValue;
                else
                    originalClusterNames.Add(rowId, currentValue);

                Debug.WriteLine($"셀 편집 시작: 행 {rowId}, 원래 값: {currentValue}");
            }
        }

        private async void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;

            // 클러스터명 컬럼이 아니면 종료
            if (e.ColumnIndex != dgv.Columns["클러스터명"].Index)
            {
                Debug.WriteLine("클러스터명 컬럼이 아닙니다. 편집 처리를 건너뜁니다.");
                return;
            }

            // 수정된 값 가져오기
            string newValue = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";

            // 원래 값 확인 (없으면 빈 문자열 사용)
            string originalValue = "";
            if (originalClusterNames.ContainsKey(e.RowIndex))
                originalValue = originalClusterNames[e.RowIndex];

            // 값이 변경되지 않았으면 종료
            if (newValue == originalValue)
            {
                Debug.WriteLine("값이 변경되지 않았습니다. 업데이트를 건너뜁니다.");
                return;
            }

            // 값이 비어있으면 종료
            if (string.IsNullOrEmpty(newValue))
            {
                Debug.WriteLine("새 값이 비어 있습니다. 업데이트를 건너뜁니다.");
                create_check_keyword_list();
                return;
            }

            using (var progressForm = new ProcessProgressForm())
            {
                progressForm.Show();
                await progressForm.UpdateProgressHandler(10, "클러스터명 변경 시작");
                await Task.Delay(10);

                // DataHandler.finalClusteringData 업데이트
                int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["ID"].Value);
                DataRow[] rows = DataHandler.finalClusteringData.Select($"ID = {id}");
                if (rows.Length > 0)
                {
                    rows[0]["클러스터명"] = newValue;
                }

                await progressForm.UpdateProgressHandler(50, "클러스터명 변경 진행중...");
                await Task.Delay(10);

                // 변경 사항 저장
                DataHandler.finalClusteringData.AcceptChanges();
                mergeClusterDataTable = EnrichWithRawTableData(DataHandler.finalClusteringData);

                await progressForm.UpdateProgressHandler(70, "클러스터명 변경 결과 출력 중...");
                await Task.Delay(10);

                create_check_keyword_list();

                await progressForm.UpdateProgressHandler(100);
                await Task.Delay(10);
                progressForm.Close();

                MessageBox.Show("클러스터명 변경이 완료되었습니다.", "Info",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Information);
            }

            // 사용한 원래 값 정보 삭제
            if (originalClusterNames.ContainsKey(e.RowIndex))
                originalClusterNames.Remove(e.RowIndex);
        }

        private void check_search_button_Click(object sender, EventArgs e)
        {
            create_check_keyword_list();
        }

        private void merge_search_radio1_CheckedChanged(object sender, EventArgs e)
        {
            merge_search_keyword.Enabled = merge_search_radio2.Checked;

        }

        private void merge_search_radio2_CheckedChanged(object sender, EventArgs e)
        {
            merge_search_keyword.Enabled = merge_search_radio2.Checked;
        }

        private void check_search_radio1_CheckedChanged(object sender, EventArgs e)
        {
            check_search_keyword.Enabled = check_search_radio2.Checked;
        }

        private void check_search_radio2_CheckedChanged(object sender, EventArgs e)
        {
            check_search_keyword.Enabled = check_search_radio2.Checked;
        }

        private void merge_search_keyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                create_merge_keyword_list();   // 호출하고 싶은 함수
                e.SuppressKeyPress = true;  // 비프음 방지
            }
        }

        private void check_search_keyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                create_check_keyword_list();   // 호출하고 싶은 함수
                e.SuppressKeyPress = true;  // 비프음 방지
            }
        }

        private void ShowMergeConfirmation(int clusterCount)
        {
            if (isFinishSession)
            {
                DialogResult dupleCheckResult = MessageBox.Show(
                $"현재 페이지에서 수정된 정보를 기준으로 Export 페이지를 갱신하기 위해 "
                + "기존 Export 페이지의 수정 내역을 초기화합니다."
                + "현재 페이지 정보를 기준으로 Export 페이지로 이동하시겠습니까?"
                + "\n(원치 않으실 경우 상단 메뉴바 > Export 항목을 클릭하여 이동 가능합니다. )",
                "Export 페이지 초기화 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
                );

                if (dupleCheckResult != DialogResult.Yes)
                {
                    return;
                }
            }



            DialogResult result = MessageBox.Show(
                $"Clustering 병합 테이블에 {clusterCount} 개의 Cluster가 남아있습니다.\n해당 Cluster를 'Undefined'로 일괄 통합하시겠습니까?\n(클러스터명은 다음 페이지에서 수정 가능합니다.)",
                "클러스터 통합 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {

                //남은 clustering 항목 일괄 병합
                /*
                List<int> checkedData = new List<int>();
                
                foreach (DataGridViewRow row in merge_cluster_table.Rows)
                {
                    // ID 컬럼의 값을 직접 int로 가져오기
                    if (row.Cells["ID"].Value != null)
                    {
                        int id = Convert.ToInt32(row.Cells["ID"].Value);
                        checkedData.Add(id);
                    }
                }
                */
                List<int> checkedData = DataHandler.finalClusteringData.AsEnumerable()
                                .Where(row => row.Field<int>("clusterID") < 0)
                                .Select(row => row.Field<int>("ID"))
                                .ToList();

                MergeAndCreateNewCluster(DataHandler.finalClusteringData, checkedData, "Undefined");

                set_keyword_combo_list();

                //검색조건 초기화
                merge_keyword_combo.SelectedIndex = 0;
                merge_search_keyword.Text = "";

                create_merge_keyword_list();
                create_check_keyword_list();

                //DataHandler.SetupDataGridView(dataGridView_3rd, DataHandler.finalClusteringData);



                //Export 페이지로 이동
                userControlHandler.uc_classification.initUI();

                if (this.ParentForm is Form1 form)
                {
                    form.LoadUserControl(userControlHandler.uc_classification);
                }
            }
            else
            {
                return;
            }
        }


        private void complete_btn_Click(object sender, EventArgs e)
        {
            int clusterCount = GetCountOfNegativeOneClusterIDs(DataHandler.finalClusteringData);
            if (clusterCount > 0)
            {
                ShowMergeConfirmation(clusterCount);
            }
            else
            {
                userControlHandler.uc_classification.initUI();

                if (this.ParentForm is Form1 form)
                {
                    form.LoadUserControl(userControlHandler.uc_classification);
                }
            }
            isFinishSession = true;
        }

        public string FormatToKoreanUnit(decimal number)
        {
            // 절대값으로 계산 후 나중에 부호 처리
            bool isNegative = number < 0;
            number = Math.Abs(number);


            string result;
            decimal divideNum = 0;


            divideNum = Math.Round(number / decimalDivider, 2);

            // 소수점 이하가 없는 경우 (정수인 경우)
            if (divideNum == Math.Truncate(divideNum))
            {
                result = string.Format("{0:N0}", divideNum) + " " + decimalDividerName;

            }
            // 소수점 둘째 자리가 0인 경우 (예: 10.5)
            else if (divideNum * 10 % 1 == 0)
            {
                result = string.Format("{0:N1}", divideNum) + " " + decimalDividerName;
            }
            //소수점 2째자리 표기
            else
            {
                result = string.Format("{0:N2}", divideNum) + " " + decimalDividerName;
            }




            // 음수 처리
            if (isNegative && divideNum != 0)
            {
                result = "-" + result;
            }

            return result;
        }

        private async void decimal_combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine($"decimal_combo.SelectedIndex : {decimal_combo.SelectedIndex}");
            //선택 값 기준 decimal 단위 변환
            double divider = Math.Pow(1000, decimal_combo.SelectedIndex);
            //억 원은 10 나누기
            if (decimal_combo.SelectedIndex == 3)
            {
                divider = divider / 10;
            }
            decimalDivider = (decimal)divider;
            decimalDividerName = decimal_combo.SelectedItem.ToString();

            //리스트 재 조회
            // 나머지 초기화 로직
            //await Task.Run(() => create_merge_keyword_list());
            await Task.Run(() =>
            {
                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0].Invoke((MethodInvoker)delegate
                    {
                        merge_cluster_table.DataSource = null;
                        create_merge_keyword_list();

                        merge_check_table.DataSource = null;

                        create_check_keyword_list();

                        //2025.04.23
                        //추천 키워드 리스트 재조회
                        UpdateModifiedDataGridView();
                    });
                }
            });
        }

        private async void merge_addon_btn_Click(object sender, EventArgs e)
        {
            List<int> mergeIDlList = GetCheckedRowsData(merge_cluster_table);
            if (mergeIDlList.Count < 1)
            {
                MessageBox.Show("병합 테이블에서 추가 병합을 진행할 대상을 선택하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }

            List<int> mergeClusterIDlList = GetCheckedRowsData(merge_check_table);
            if (mergeClusterIDlList.Count < 1)
            {
                MessageBox.Show("추가 병합 수행 시 병합 결과 확인 테이블에서 \n 병합 시킬 클러스터를 선택하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }

            if (mergeClusterIDlList.Count > 1)
            {
                MessageBox.Show("추가 병합 수행 시 병합 결과 확인 테이블에서 \n 병합 시킬 클러스터 1개만 선택해주세요.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }

            using (var progressForm = new ProcessProgressForm())
            {
                progressForm.Show();


                await progressForm.UpdateProgressHandler(10, "클러스터 병합 시작");
                await Task.Delay(10);



                int mergeAddClusterID = mergeClusterIDlList[0];
                int checkIndex = GetCheckedRowsIndex(merge_check_table);

                Debug.WriteLine($" checkIndex : {checkIndex}");

                MergeAndCreateNewCluster(DataHandler.finalClusteringData, mergeIDlList, null, mergeAddClusterID.ToString());

                await progressForm.UpdateProgressHandler(50, "클러스터 병합 진행중...");
                await Task.Delay(10);


                set_keyword_combo_list();

                //검색조건 초기화
                merge_keyword_combo.SelectedIndex = 0;
                merge_search_keyword.Text = "";

                await progressForm.UpdateProgressHandler(70, "클러스터 병합 결과 출력 중...");
                await Task.Delay(10);


                create_merge_keyword_list(true);
                create_check_keyword_list();



                //DataHandler.SetupDataGridView(dataGridView_3rd, DataHandler.finalClusteringData);

                //추가 병합의 경우만 포커스 셀 변경
                await Task.Run(() =>
                {
                    if (Application.OpenForms.Count > 0)
                    {
                        Application.OpenForms[0].Invoke((MethodInvoker)delegate
                        {
                            merge_check_table.ClearSelection();
                            merge_check_table.Rows[checkIndex].Selected = true;
                            merge_check_table.CurrentCell = merge_check_table.Rows[checkIndex].Cells[0];
                        });
                    }
                });

                await progressForm.UpdateProgressHandler(100);
                await Task.Delay(10);
                progressForm.Close();


                MessageBox.Show("클러스터 병합이 완료되었습니다.", "Info",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Information);
            }

            // 병합 작업 후 업데이트
            UpdateModifiedDataGridView();
            /*
            int mergeAddClusterID = mergeClusterIDlList[0];
            int checkIndex = GetCheckedRowsIndex(merge_check_table);

            Debug.WriteLine($" checkIndex : {checkIndex}");

            MergeAndCreateNewCluster(DataHandler.finalClusteringData, mergeIDlList, null, mergeAddClusterID.ToString());

            set_keyword_combo_list();

            //검색조건 초기화
            merge_keyword_combo.SelectedIndex = 0;
            merge_search_keyword.Text = "";

            create_merge_keyword_list();
            create_check_keyword_list();

            //DataHandler.SetupDataGridView(dataGridView_3rd, DataHandler.finalClusteringData);

            //추가 병합의 경우만 포커스 셀 변경
            await Task.Run(() =>
            {
                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0].Invoke((MethodInvoker)delegate
                    {
                        merge_check_table.ClearSelection();
                        merge_check_table.Rows[checkIndex].Selected = true;
                        merge_check_table.CurrentCell = merge_check_table.Rows[checkIndex].Cells[0];
                    });
                }
            });



            MessageBox.Show("클러스터 병합이 완료되었습니다.", "Info",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Information);
            */
        }



        private void lv1_add_btn_Click(object sender, EventArgs e)
        {
            add_lv1_keyword();
        }

        private void add_lv1_keyword()
        {
            // TextBox에 입력된 텍스트를 가져옴
            string inputText = new_lv1_word.Text.Trim();

            // 텍스트가 비어있지 않은 경우 ListBox에 추가
            if (!string.IsNullOrEmpty(inputText))
            {
                //DataHandler.separator.Add(inputText);
                _recomandKeywordManager.AddLv1Item(inputText);
                new_lv1_word.Clear(); // TextBox 초기화
            }

            List<string> lv1_list = _recomandKeywordManager.Lv1List
           .Distinct()  // 중복 제거
           .ToList();   // List로 변환

            //lv1 리스트 추가
            create_keyword_table(dataGridView_lv1, lv1_list);
        }

        private void new_lv1_word_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                add_lv1_keyword();
                // Enter 키가 다른 동작을 막도록 처리
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void lv1_del_btn_Click(object sender, EventArgs e)
        {
            List<string> lv1_del_list = GetCheckedRowsStringData(dataGridView_lv1);

            if (lv1_del_list.Count == 0)
            {
                MessageBox.Show("Lv1 제거 대상을 선택하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }

            foreach (string seperate in lv1_del_list)
            {
                //_separatorManager.Separators.Remove(seperate);
                _recomandKeywordManager.RemoveLv1Item(seperate);
            }

            for (int i = dataGridView_lv1.Rows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = dataGridView_lv1.Rows[i];

                // columnListDgv의 두 번째 컬럼(체크박스 다음)의 값 확인
                string seperData = row.Cells[1].Value?.ToString();
                if (lv1_del_list.Contains(seperData))
                {
                    dataGridView_lv1.Rows.RemoveAt(i);
                }
            }
        }

        private void reco_add_btn_Click(object sender, EventArgs e)
        {
            add_reco_keyword();
        }

        private void add_reco_keyword()
        {
            // TextBox에 입력된 텍스트를 가져옴
            string inputText = new_reco_word.Text.Trim();

            // 텍스트가 비어있지 않은 경우 ListBox에 추가
            if (!string.IsNullOrEmpty(inputText))
            {
                //DataHandler.separator.Add(inputText);
                _recomandKeywordManager.AddKeyword(selectecLv1Name, inputText);
                new_reco_word.Clear(); // TextBox 초기화
            }

            Lv1Item selectedItem = _recomandKeywordManager.GetLv1Item(selectecLv1Name);


            if (selectedItem != null)
            {
                List<string> keywords = selectedItem.Keywords;
                create_keyword_table(dataGridView_recoman_keyword, keywords, false);
            }
        }

        private void new_reco_word_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                add_reco_keyword();
                // Enter 키가 다른 동작을 막도록 처리
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void reco_del_btn_Click(object sender, EventArgs e)
        {
            List<string> reco_keyword_del_list = GetCheckedRowsStringData(dataGridView_recoman_keyword);

            if (reco_keyword_del_list.Count == 0)
            {
                MessageBox.Show("추천 키워드 제거 대상을 선택하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }

            foreach (string seperate in reco_keyword_del_list)
            {
                //_separatorManager.Separators.Remove(seperate);
                _recomandKeywordManager.RemoveKeyword(selectecLv1Name, seperate);
            }

            for (int i = dataGridView_recoman_keyword.Rows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = dataGridView_recoman_keyword.Rows[i];

                // columnListDgv의 두 번째 컬럼(체크박스 다음)의 값 확인
                string seperData = row.Cells[1].Value?.ToString();
                if (reco_keyword_del_list.Contains(seperData))
                {
                    dataGridView_recoman_keyword.Rows.RemoveAt(i);
                }
            }
        }

        private void excep_search_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            except_keyword.Text = "";
            except_keyword.Enabled = excep_search_checkbox.Checked;

        }

        private void equal_search_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            equalsSearchYN = equal_search_checkbox.Checked;
        }

        private void except_keyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                create_merge_keyword_list();   // 호출하고 싶은 함수
                e.SuppressKeyPress = true;  // 비프음 방지
            }
        }

        private void keyword_radio1_CheckedChanged(object sender, EventArgs e)
        {
            // 검색 모드가 변경되면 콤보박스와 검색어 초기화
            //merge_keyword_combo.SelectedIndex = 0;
            merge_search_keyword.Text = "";

            // 필요에 따라 콤보박스 내용 업데이트
            UpdateSearchComboBox();
        }

        private void UpdateSearchComboBox()
        {
            merge_keyword_combo.Items.Clear();

            if (keyword_radio1.Checked)
            {
                // 키워드 검색 모드
                merge_keyword_combo.Items.Add("키워드 선택");
                foreach (string keyword in merge_keyword_list)
                {
                    if ("\\".Equals(keyword))
                    {
                        continue;
                    }
                    merge_keyword_combo.Items.Add(keyword);
                }
            }
            else
            {
                // 공급업체 검색 모드
                merge_keyword_combo.Items.Add("공급업체 선택");
                if (supplier_keyword_list != null)
                {
                    foreach (string supplier in supplier_keyword_list)
                    {
                        if ("\\".Equals(supplier))
                        {
                            continue;
                        }
                        merge_keyword_combo.Items.Add(supplier);
                    }
                }

            }

            merge_keyword_combo.SelectedIndex = 0;
        }

        // 키워드별 데이터를 저장할 클래스
        class KeywordData
        {
            public int Count { get; set; }
            public decimal TotalAmount { get; set; }
        }

        //2025.04.25
        //추천 키워드 갱신 함수
        // uc_clustering.cs에 추가할 새 메서드
        private void UpdateModifiedDataGridView()
        {
            // UI 업데이트 시작 전에 SuspendLayout 호출
            dataGridView_modified.SuspendLayout();

            try
            {
                // DataGridView 초기화
                dataGridView_modified.DataSource = null;
                dataGridView_modified.Rows.Clear();
                dataGridView_modified.Columns.Clear();

                if (DataHandler.dragSelections.ContainsKey(dataGridView_modified))
                {
                    DataHandler.dragSelections[dataGridView_modified].Clear();
                }

                // 미병합 클러스터 필터링 (ClusterID == -1)
                var unboundClusters = mergeClusterDataTable.AsEnumerable()
                    .Where(row => row.Field<int>("ClusterID") == -1)
                    .ToList(); // CopyToDataTable 대신 ToList 사용

                if (unboundClusters.Count < 1)
                {
                    return; // 데이터가 없으면 종료
                }

                // 키워드를 그룹화하여 집계할 Dictionary 생성
                Dictionary<string, KeywordData> keywordDict = new Dictionary<string, KeywordData>();

       
       

                // 모든 키워드 추출 및 집계
                foreach (var row in unboundClusters)
                {
                    string keywordList = row["키워드목록"].ToString();
                string[] keywords = keywordList.Split(',');
                int rowCount = Convert.ToInt32(row["Count"]);
                decimal rowAmount = Convert.ToDecimal(row["합산금액"]);

                    foreach (string keyword in keywords)
                    {
                        string trimmedKeyword = keyword.Trim();
                        if (string.IsNullOrEmpty(trimmedKeyword))
                            continue;

                        if (keywordDict.ContainsKey(trimmedKeyword))
                        {
                            // 기존 키워드에 값 추가
                            keywordDict[trimmedKeyword].Count += rowCount;
                            keywordDict[trimmedKeyword].TotalAmount += rowAmount;
                        }
                        else
                        {
                            // 새 키워드 추가
                            keywordDict[trimmedKeyword] = new KeywordData
                            {
                                Count = rowCount,
                                TotalAmount = rowAmount
                                };
                        }
                    }
                }

                // 정렬을 위해 리스트로 변환 (Count 기준 내림차순)
                var sortedKeywords = keywordDict.OrderByDescending(kv => kv.Value.Count).ToList();

                // DataGridView 컬럼 설정
                dataGridView_modified.Columns.Add("키워드", "키워드");
                dataGridView_modified.Columns.Add("Count", "Count");
                dataGridView_modified.Columns.Add("합산금액", "합산금액");

                // 데이터 행 추가
                foreach (var keywordEntry in sortedKeywords)
                {
                    int rowIndex = dataGridView_modified.Rows.Add();
                    dataGridView_modified.Rows[rowIndex].Cells["키워드"].Value = keywordEntry.Key;
                    dataGridView_modified.Rows[rowIndex].Cells["Count"].Value = keywordEntry.Value.Count;
                    dataGridView_modified.Rows[rowIndex].Cells["합산금액"].Value =
                        FormatToKoreanUnit(keywordEntry.Value.TotalAmount);
                }

                // 열 형식 지정
                if (dataGridView_modified.Columns["Count"] != null)
                {
                    dataGridView_modified.Columns["Count"].DefaultCellStyle.Format = "N0";
                    dataGridView_modified.Columns["Count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                // DataGridView 속성 설정
                dataGridView_modified.AllowUserToAddRows = false;
                dataGridView_modified.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView_modified.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView_modified.ReadOnly = true;
                dataGridView_modified.Font = new System.Drawing.Font("맑은 고딕", 14.25F);

                // 정렬 이벤트 핸들러 설정
                dataGridView_modified.SortCompare -= DataHandler.money_SortCompare;
                dataGridView_modified.SortCompare += DataHandler.money_SortCompare;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateModifiedDataGridView 오류: {ex.Message}");

                // 데이터가 없는 경우 빈 그리드 생성
                dataGridView_modified.Columns.Clear();
                dataGridView_modified.Columns.Add("키워드", "키워드");
                dataGridView_modified.Columns.Add("Count", "Count");
                dataGridView_modified.Columns.Add("합산금액", "합산금액");
            }
            finally
            {
                // UI 업데이트 재개
                dataGridView_modified.ResumeLayout();
            }
        }

        private void union_cluster_btn_Click(object sender, EventArgs e)
        {
            // 1. 체크된 항목들 찾기
            List<int> checkedClusterIds = new List<int>();

            foreach (DataGridViewRow row in merge_check_table.Rows)
            {
                // 체크된 항목의 ClusterID 수집
                DataGridViewCheckBoxCell checkCell = row.Cells[0] as DataGridViewCheckBoxCell;
                if (checkCell != null && checkCell.Value != null && Convert.ToBoolean(checkCell.Value))
                {
                    int clusterId = Convert.ToInt32(row.Cells["ClusterID"].Value);
                    checkedClusterIds.Add(clusterId);
                }
            }

            // 2. 체크된 항목이 1개 이하인 경우 종료
            if (checkedClusterIds.Count < 2)
            {
                MessageBox.Show("클러스터 간 병합 수행 시\n2개 이상의 병합된 클러스터를 선택해주세요.",
                    "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 3. 병합할 정보 수집 및 새 클러스터 생성
                int maxId = DataHandler.finalClusteringData.AsEnumerable()
                    .Max(row => Convert.ToInt32(row["ID"]));
                int newClusterId = maxId + 1;

                string combinedKeywords = "";
                int totalCount = 0;
                decimal totalAmount = 0;
                string combinedClusterName = "";
                string combinedDataIndex = "";

                // 병합할 클러스터 정보 수집 및 하위 항목 ID 수집
                List<int> allSubItemIds = new List<int>(); // 모든 하위 항목의 ID

                foreach (int clusterId in checkedClusterIds)
                {
                    // 클러스터 대표 행 찾기
                    DataRow clusterRow = DataHandler.finalClusteringData.AsEnumerable()
                        .FirstOrDefault(row => Convert.ToInt32(row["ID"]) == clusterId);

                    if (clusterRow != null)
                    {
                        // 클러스터명 병합
                        if (!string.IsNullOrEmpty(combinedClusterName))
                            combinedClusterName += "_";
                        combinedClusterName += clusterRow["클러스터명"].ToString();

                        // 키워드 병합
                        if (!string.IsNullOrEmpty(combinedKeywords) && !string.IsNullOrEmpty(clusterRow["키워드목록"].ToString()))
                        {
                            // 중복 제거하여 키워드 병합
                            HashSet<string> keywordSet = new HashSet<string>(
                                combinedKeywords.Split(',').Select(k => k.Trim()));

                            foreach (string keyword in clusterRow["키워드목록"].ToString().Split(',').Select(k => k.Trim()))
                            {
                                keywordSet.Add(keyword);
                            }

                            combinedKeywords = string.Join(",", keywordSet);
                        }
                        else if (!string.IsNullOrEmpty(clusterRow["키워드목록"].ToString()))
                        {
                            combinedKeywords = clusterRow["키워드목록"].ToString();
                        }

                        // dataIndex 병합
                        if (!string.IsNullOrEmpty(combinedDataIndex) && !string.IsNullOrEmpty(clusterRow["dataIndex"].ToString()))
                        {
                            combinedDataIndex += "," + clusterRow["dataIndex"].ToString();
                        }
                        else if (!string.IsNullOrEmpty(clusterRow["dataIndex"].ToString()))
                        {
                            combinedDataIndex = clusterRow["dataIndex"].ToString();
                        }

                        // 개수와 금액 합산
                        totalCount += Convert.ToInt32(clusterRow["Count"]);
                        totalAmount += Convert.ToDecimal(clusterRow["합산금액"]);
                    }

                    // 해당 클러스터에 속한 모든 항목 수집
                    foreach (DataRow row in DataHandler.finalClusteringData.Rows)
                    {
                        if (row["ClusterID"] != DBNull.Value && Convert.ToInt32(row["ClusterID"]) == clusterId)
                        {
                            allSubItemIds.Add(Convert.ToInt32(row["ID"]));
                        }
                    }
                }

                // 클러스터명 길이 제한
                if (combinedClusterName.Length > 20)
                {
                    combinedClusterName = combinedClusterName.Substring(0, 17) + "...";
                }

                // 4. 새 클러스터 행 생성
                DataRow newRow = DataHandler.finalClusteringData.NewRow();
                newRow["ID"] = newClusterId;
                newRow["ClusterID"] = newClusterId;
                newRow["클러스터명"] = combinedClusterName;
                newRow["키워드목록"] = combinedKeywords;
                newRow["Count"] = totalCount;
                newRow["합산금액"] = totalAmount;
                newRow["dataIndex"] = combinedDataIndex;

                DataHandler.finalClusteringData.Rows.Add(newRow);

                // 5. 기존 클러스터 대표 행 삭제 (중요: 먼저 하위 항목의 ClusterID를 변경한 후 삭제해야 함)
                // 모든 하위 항목의 ClusterID 변경
                foreach (DataRow row in DataHandler.finalClusteringData.Rows)
                {
                    int rowId = Convert.ToInt32(row["ID"]);
                    if (row["ClusterID"] != DBNull.Value)
                    {
                        int rowClusterId = Convert.ToInt32(row["ClusterID"]);
                        if (checkedClusterIds.Contains(rowClusterId))
                        {
                            row["ClusterID"] = newClusterId;
                        }
                    }
                }

                // 기존 클러스터 대표 행 삭제
                List<DataRow> rowsToDelete = new List<DataRow>();
                foreach (int clusterId in checkedClusterIds)
                {
                    DataRow clusterRow = DataHandler.finalClusteringData.AsEnumerable()
                        .FirstOrDefault(row => Convert.ToInt32(row["ID"]) == clusterId);

                    if (clusterRow != null)
                    {
                        rowsToDelete.Add(clusterRow);
                    }
                }

                foreach (DataRow row in rowsToDelete)
                {
                    DataHandler.finalClusteringData.Rows.Remove(row);
                }

                // 6. 변경사항 적용
                DataHandler.finalClusteringData.AcceptChanges();
                mergeClusterDataTable = EnrichWithRawTableData(DataHandler.finalClusteringData);

                // 7. 데이터 리스트 갱신
                set_keyword_combo_list();
                create_check_keyword_list();
                UpdateModifiedDataGridView();

                MessageBox.Show("선택한 클러스터들이 성공적으로 병합되었습니다.",
                    "병합 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"클러스터 병합 중 오류가 발생했습니다: {ex.Message}",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
