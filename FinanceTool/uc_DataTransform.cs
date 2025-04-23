using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Concurrent;
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
    public partial class uc_DataTransform : UserControl
    {

        DataTable originDataTable;
        DataTable transformDataTable;
        DataTable viewTransformDataTable;
        DataTable modifiedDataTable;

        private bool isProcessingSelection = false;
        private decimal decimalDivider = 1;
        private string decimalDividerName = "원";
        private int keywordColumnsCount = 0;

        private bool isFinishSession = false;
        public uc_DataTransform()
        {
            InitializeComponent();
        }

        /*
        public void initUI(DataTable dataTable)
        {
            originDataTable = dataTable;
            transformDataTable = dataTable.Copy();
            dataGridView_2nd.DataSource = originDataTable;
            dataGridView_transform.DataSource = transformDataTable;

            create_merge_keyword_list();
            set_keyword_combo_list();

            DataHandler.SyncDataGridViewSelections(dataGridView_2nd, dataGridView_transform);
        }
        */
        public async Task initUI()
        {
            try
            {
                Debug.WriteLine("data Transform initUI->select Query Start");

                // 데이터 로딩은 이미 백그라운드 스레드에서 실행 중이므로 직접 실행
                string query = "SELECT * FROM process_view_data";
                DataTable viewData = DBManager.Instance.ExecuteQuery(query);
                Debug.WriteLine("data Transform initUI->select Query End");

                // DataTable 설정
                originDataTable = viewData;
                transformDataTable = viewData.Copy();
                
                Debug.WriteLine("data Transform initUI->transformDataTable Setting complete");

                // 메인 UI 스레드로 돌아가서 UI 컨트롤 업데이트
                await Task.Run(() =>
                {
                    if (Application.OpenForms.Count > 0)
                    {
                        Application.OpenForms[0].Invoke((MethodInvoker)delegate
                        {
                            dataGridView_2nd.DataSource = originDataTable;
                            if (dataGridView_2nd.Columns["raw_data_id"] != null)
                            {
                                dataGridView_2nd.Columns["raw_data_id"].Visible = false;
                            }

                            if (dataGridView_2nd.Columns["id"] != null)
                            {
                                dataGridView_2nd.Columns["id"].Visible = false;
                            }

                            if (dataGridView_2nd.Columns["import_date"] != null)
                            {
                                dataGridView_2nd.Columns["import_date"].Visible = false;
                            }



                            //sorting 기준 변환
                            //dataGridView_modified.SortCompare -= DataHandler.DataGridView1_SortCompare;
                            //match_keyword_table.SortCompare -= DataHandler.DataGridView1_SortCompare;

                            sum_keyword_table.SortCompare += DataHandler.money_SortCompare;
                            match_keyword_table.SortCompare += DataHandler.money_SortCompare;
                        });
                    }
                });

                // 데이터 변환 (이미 비동기 메서드)
                viewTransformDataTable = await EnrichTransformDataWithRawData(transformDataTable);

                Debug.WriteLine("data Transform initUI->DataGridView Bind Setting complete");

                // 나머지 초기화 로직
                await Task.Run(() => create_merge_keyword_list());
                Debug.WriteLine("data Transform initUI->create_merge_keyword_list complete");

                await Task.Run(() => set_keyword_combo_list());
                Debug.WriteLine("data Transform initUI->set_keyword_combo_list Setting complete");


                // 메인 UI 스레드로 돌아가서 DataHandler 등록
                await Task.Run(() =>
                {
                    if (Application.OpenForms.Count > 0)
                    {
                        Application.OpenForms[0].Invoke((MethodInvoker)delegate
                        {
                            Debug.WriteLine("RegisterDataGridView->match_keyword_table ");
                            DataHandler.RegisterDataGridView(match_keyword_table);
                            // 이벤트 핸들러 중복 등록 방지
                            decimal_combo.SelectedIndexChanged -= decimal_combo_SelectedIndexChanged; // 기존 핸들러 제거

                            decimal_combo.SelectedIndex = 0;

                            decimal_combo.SelectedIndexChanged += decimal_combo_SelectedIndexChanged;
                        });
                    }
                });
                
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"initUI 오류: {ex.Message}");
                await Task.Run(() =>
                {
                    MessageBox.Show($"데이터 로드 중 오류가 발생했습니다: {ex.Message}",
                                  "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }


        private void SyncDataGridViewSelections(DataGridView dataGridView1, DataGridView dataGridView2)
        {
            // 첫 번째 DataGridView의 SelectionChanged 이벤트 핸들러
            dataGridView1.SelectionChanged += (sender, e) =>
            {
                if (isProcessingSelection) return;  // 재귀적 호출 방지

                try
                {
                    isProcessingSelection = true;

                    if (dataGridView1.CurrentRow != null)
                    {
                        int selectedIndex = dataGridView1.CurrentRow.Index;

                        // 두 번째 DataGridView에 같은 행 인덱스가 있는지 확인
                        if (selectedIndex < dataGridView2.Rows.Count)
                        {
                            dataGridView2.ClearSelection();
                            dataGridView2.Rows[selectedIndex].Selected = true;
                            dataGridView2.CurrentCell = dataGridView2.Rows[selectedIndex].Cells[0];
                        }
                    }
                }
                finally
                {
                    isProcessingSelection = false;
                }
            };

            // 두 번째 DataGridView의 SelectionChanged 이벤트 핸들러
            dataGridView2.SelectionChanged += (sender, e) =>
            {
                if (isProcessingSelection) return;  // 재귀적 호출 방지

                try
                {
                    isProcessingSelection = true;

                    if (dataGridView2.CurrentRow != null)
                    {
                        int selectedIndex = dataGridView2.CurrentRow.Index;

                        // 첫 번째 DataGridView에 같은 행 인덱스가 있는지 확인
                        if (selectedIndex < dataGridView1.Rows.Count)
                        {
                            dataGridView1.ClearSelection();
                            dataGridView1.Rows[selectedIndex].Selected = true;
                            dataGridView1.CurrentCell = dataGridView1.Rows[selectedIndex].Cells[0];
                        }
                    }
                }
                finally
                {
                    isProcessingSelection = false;
                }
            };
        }


        private bool searchYN = false;
        private async Task create_merge_keyword_list(bool progressYN = false)
        {
            try
            {
                searchYN = true;
                if (progressYN)
                {
                    using (var progressForm = new ProcessProgressForm())
                    {
                        Debug.WriteLine("create_merge_keyword_list start ");
                        progressForm.Show();
                        await progressForm.UpdateProgressHandler(10, "키워드 요약 테이블 생성 중...");
                        await Task.Delay(10);

                        Debug.WriteLine("progressForm start");

                        // 1~4 단계는 동일 (테이블 생성 및 정보 가져오기)
                        //컬럼종류가 다르다면 테이블 다시 생성
                        if (transformDataTable.Columns.Count != keywordColumnsCount)
                        {
                            DBManager.Instance.ExecuteNonQuery("DROP TABLE IF EXISTS temp_transform_data");
                        }

                        Debug.WriteLine("DROP TABLE IF EXISTS temp_transform_data complete");

                        CreateTempTableFromDataTable("temp_transform_data", transformDataTable);
                        CreateTempTableFromDataTable("temp_money_data", DataHandler.moneyDataTable, true);

                        List<string> columns = transformDataTable.Columns.Cast<DataColumn>()
                        .Select(col => $"{col.ColumnName} {GetSQLiteType(col.DataType)}")
                        .ToList();

                        
                        Debug.WriteLine($"  transformDataTable.Columns : {string.Join(",", columns)}");

                        // 데이터베이스 최적화 설정 추가
                        DBManager.Instance.ExecuteNonQuery("PRAGMA journal_mode = MEMORY");
                        DBManager.Instance.ExecuteNonQuery("PRAGMA synchronous = OFF");
                        DBManager.Instance.ExecuteNonQuery("PRAGMA cache_size = 10000");
                        DBManager.Instance.ExecuteNonQuery("PRAGMA temp_store = MEMORY");

                        // 임시 테이블에 인덱스 생성
                        DBManager.Instance.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_temp_transform_value ON temp_transform_data(raw_data_id)");

                        // 컬럼 정보 가져오기 (기존 코드와 동일)
                        string tableInfoQuery = "PRAGMA table_info(temp_transform_data)";
                        DataTable columnInfo = DBManager.Instance.ExecuteQuery(tableInfoQuery);

                        await progressForm.UpdateProgressHandler(40, "키워드 요약 테이블 생성 중...");
                        await Task.Delay(10);
                        Debug.WriteLine("PRAGMA TABLE complete");

                        List<string> columnNames = new List<string>();
                        foreach (DataRow row in columnInfo.Rows)
                        {
                            string colName = row["name"].ToString();
                            if (colName.ToLower() != "rowid" && colName.ToLower() != "id"
                                && colName.ToLower() != "raw_data_id" && colName.ToLower() != "import_date")
                            {
                                columnNames.Add(colName);
                            }
                        }

                        // json_array 생성 (기존 코드와 동일)
                        StringBuilder jsonArrayBuilder = new StringBuilder();
                        for (int i = 0; i < columnNames.Count; i++)
                        {
                            if (i > 0) jsonArrayBuilder.Append(", ");
                            jsonArrayBuilder.Append($"CAST(t.{columnNames[i]} AS TEXT)");
                        }

                        string moneyColumnName = DataHandler.moneyDataTable.Columns[0].ColumnName;

                        // 5. 키워드 분할 전략 적용
                        // 5.1 먼저 키워드 추출 및 카운팅 임시 테이블 생성
                        string extractQuery = $@"
                    DROP TABLE IF EXISTS temp_keywords;
                    CREATE TABLE temp_keywords AS
                    WITH split_data AS (
                        SELECT t.raw_data_id as row_id, j.value
                        FROM temp_transform_data t, 
                             json_each(json_array(
                                {jsonArrayBuilder.ToString()}
                             )) j
                        WHERE j.value IS NOT NULL AND TRIM(j.value) != ''
                    )
                    SELECT 
                        TRIM(value) as keyword,
                        COUNT(*) as occurrence_count
                    FROM split_data
                    GROUP BY TRIM(value)
                    ORDER BY occurrence_count DESC, keyword ASC;";

                        DBManager.Instance.ExecuteNonQuery(extractQuery);
                        DBManager.Instance.ExecuteNonQuery("CREATE INDEX idx_temp_keywords ON temp_keywords(keyword)");

                        Debug.WriteLine("CREATE INDEX idx_temp_keywords ON temp_keywords(keyword) complete");

                        // 5.2 전체 키워드 수 확인
                        int totalKeywords = Convert.ToInt32(DBManager.Instance.ExecuteScalar("SELECT COUNT(*) FROM temp_keywords"));
                        Debug.WriteLine($"총 키워드 수: {totalKeywords}");

                        // 5.3 페이징 처리를 위한 설정
                        int pageSize = 1000; // 한 번에 처리할 키워드 수
                        int totalPages = (int)Math.Ceiling(totalKeywords / (double)pageSize);

                        // 결과를 저장할 DataTable 생성
                        DataTable sumMoneyTable = new DataTable();
                        sumMoneyTable.Columns.Add("keyword", typeof(string));
                        sumMoneyTable.Columns.Add("occurrence_count", typeof(int));
                        sumMoneyTable.Columns.Add("total_money", typeof(decimal));

                        


                        await progressForm.UpdateProgressHandler(70, "키워드 요약 정보 산출 중...");
                        await Task.Delay(10);


                        // 5.4 병렬 페이지 처리
                        List<Task<DataTable>> pageTasks = new List<Task<DataTable>>();

                        for (int page = 0; page < totalPages; page++)
                        {
                            int offset = page * pageSize;
                            int currentPage = page; // 클로저를 위해 복사

                            Task<DataTable> pageTask = Task.Run(() =>
                            {
                                string pageQuery = $@"
                    WITH page_keywords AS (
                        SELECT keyword, occurrence_count
                        FROM temp_keywords
                        LIMIT {pageSize} OFFSET {offset}
                    ),
                    split_data AS (
                        SELECT t.raw_data_id as row_id, TRIM(j.value) as value
                        FROM temp_transform_data t, 
                             json_each(json_array(
                                {jsonArrayBuilder.ToString()}
                             )) j
                        WHERE j.value IS NOT NULL AND TRIM(j.value) != ''
                    )
                    SELECT 
                        k.keyword,
                        k.occurrence_count,
                        COALESCE(SUM(CAST(m.'{moneyColumnName}' AS DECIMAL)), 0) as total_money
                    FROM page_keywords k
                    LEFT JOIN split_data s ON k.keyword = s.value
                    LEFT JOIN temp_money_data m ON s.row_id = m.raw_data_id
                    GROUP BY k.keyword, k.occurrence_count
                    ORDER BY k.occurrence_count DESC, k.keyword ASC;";

                                Debug.WriteLine($"페이지 {currentPage + 1}/{totalPages} 처리 시작");
                                DataTable pageResult = DBManager.Instance.ExecuteQuery(pageQuery);
                                Debug.WriteLine($"페이지 {currentPage + 1}/{totalPages} 처리 완료: {pageResult.Rows.Count}개 행");

                                return pageResult;
                            });

                            pageTasks.Add(pageTask);
                        }

                        // 모든 페이지 작업 대기
                        Debug.WriteLine($"총 {pageTasks.Count}개 페이지 작업 시작");
                        DataTable[] results = await Task.WhenAll(pageTasks);
                        Debug.WriteLine("모든 페이지 작업 완료");

                        // 결과 병합
                        foreach (DataTable pageResult in results)
                        {
                            foreach (DataRow row in pageResult.Rows)
                            {
                                sumMoneyTable.Rows.Add(
                                    row["keyword"],
                                    row["occurrence_count"],
                                    row["total_money"]
                                );
                            }
                        }

                        Debug.WriteLine($"병합된 결과: {sumMoneyTable.Rows.Count}개 행");

                        // 이하 기존 코드와 동일
                        modifiedDataTable = new DataTable();
                        modifiedDataTable.Columns.Add("Value", typeof(string));
                        modifiedDataTable.Columns.Add("Count", typeof(int));
                        modifiedDataTable.Columns.Add("합산금액", typeof(string));
                        foreach (DataRow row in sumMoneyTable.Rows)
                        {
                            //modifiedDataTable.Rows.Add(row["keyword"], row["occurrence_count"], row["total_money"]);
                            modifiedDataTable.Rows.Add(row["keyword"], row["occurrence_count"], FormatToKoreanUnit(Convert.ToDecimal(row["total_money"].ToString())));

                        }

                      
                        await progressForm.UpdateProgressHandler(90, "테이블 생성 마무리 중...");
                        await Task.Delay(10);


                        // UI 업데이트 부분만 UI 스레드에서 실행
                        await Task.Run(() =>
                        {
                            if (Application.OpenForms.Count > 0)
                            {
                                Application.OpenForms[0].Invoke((MethodInvoker)delegate
                                {

                                  

                                    if (sum_keyword_table.Rows.Count > 0)
                                    {
                                        sum_keyword_table.Rows.Clear();
                                        sum_keyword_table.Columns.Clear();
                                    }

                                    // 원본 DataTable의 컬럼들 추가
                                    foreach (DataColumn col in modifiedDataTable.Columns)
                                    {
                                        sum_keyword_table.Columns.Add(col.ColumnName, col.ColumnName);
                                    }


                                    // 데이터 필터링 및 추가
                                    foreach (DataRow row in modifiedDataTable.Rows)
                                    {
                                        int rowIndex = sum_keyword_table.Rows.Add();

                                        // 데이터 채우기
                                        for (int i = 0; i < modifiedDataTable.Columns.Count; i++)
                                        {
                                            sum_keyword_table.Rows[rowIndex].Cells[i].Value = row[i];  // +1은 체크박스 컬럼 때문
                                        }


                                    }

                                    // DataGridView 속성 설정
                                    sum_keyword_table.AllowUserToAddRows = false;
                                    sum_keyword_table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                                    sum_keyword_table.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                                    sum_keyword_table.Font = new System.Drawing.Font("맑은 고딕", 14.25F);
                                   

                                    // 나머지 컬럼들은 읽기 전용으로 설정
                                    for (int i = 1; i < sum_keyword_table.Columns.Count; i++)
                                    {
                                        sum_keyword_table.Columns[i].ReadOnly = true;
                                    }

                                });
                            }
                        });


                        // 임시 테이블 정리
                        //CleanupTempTables();
                        DBManager.Instance.ExecuteNonQuery("DROP TABLE IF EXISTS temp_keywords");

                       

                        await progressForm.UpdateProgressHandler(100);
                        await Task.Delay(10);
                        progressForm.Close();

                    }
                }
                else
                {
                    // 1~4 단계는 동일 (테이블 생성 및 정보 가져오기)
                    //컬럼종류가 다르다면 테이블 다시 생성
                    if (transformDataTable.Columns.Count != keywordColumnsCount)
                    {
                        DBManager.Instance.ExecuteNonQuery("DROP TABLE IF EXISTS temp_transform_data");
                    }


                    CreateTempTableFromDataTable("temp_transform_data", transformDataTable);
                    CreateTempTableFromDataTable("temp_money_data", DataHandler.moneyDataTable, true);

                    List<string> columns = transformDataTable.Columns.Cast<DataColumn>()
                    .Select(col => $"{col.ColumnName} {GetSQLiteType(col.DataType)}")
                    .ToList();

                    Debug.WriteLine(string.Join(",", columns));

                    // 데이터베이스 최적화 설정 추가
                    DBManager.Instance.ExecuteNonQuery("PRAGMA journal_mode = MEMORY");
                    DBManager.Instance.ExecuteNonQuery("PRAGMA synchronous = OFF");
                    DBManager.Instance.ExecuteNonQuery("PRAGMA cache_size = 10000");
                    DBManager.Instance.ExecuteNonQuery("PRAGMA temp_store = MEMORY");

                    // 임시 테이블에 인덱스 생성
                    DBManager.Instance.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_temp_transform_value ON temp_transform_data(raw_data_id)");

                    // 컬럼 정보 가져오기 (기존 코드와 동일)
                    string tableInfoQuery = "PRAGMA table_info(temp_transform_data)";
                    DataTable columnInfo = DBManager.Instance.ExecuteQuery(tableInfoQuery);

                    List<string> columnNames = new List<string>();
                    foreach (DataRow row in columnInfo.Rows)
                    {
                        string colName = row["name"].ToString();
                        if (colName.ToLower() != "rowid" && colName.ToLower() != "id"
                            && colName.ToLower() != "raw_data_id" && colName.ToLower() != "import_date")
                        {
                            columnNames.Add(colName);
                        }
                    }

                    // json_array 생성 (기존 코드와 동일)
                    StringBuilder jsonArrayBuilder = new StringBuilder();
                    for (int i = 0; i < columnNames.Count; i++)
                    {
                        if (i > 0) jsonArrayBuilder.Append(", ");
                        jsonArrayBuilder.Append($"CAST(t.{columnNames[i]} AS TEXT)");
                    }

                    string moneyColumnName = DataHandler.moneyDataTable.Columns[0].ColumnName;

                    // 5. 키워드 분할 전략 적용
                    // 5.1 먼저 키워드 추출 및 카운팅 임시 테이블 생성
                    string extractQuery = $@"
                    DROP TABLE IF EXISTS temp_keywords;
                    CREATE TABLE temp_keywords AS
                    WITH split_data AS (
                        SELECT t.raw_data_id as row_id, j.value
                        FROM temp_transform_data t, 
                             json_each(json_array(
                                {jsonArrayBuilder.ToString()}
                             )) j
                        WHERE j.value IS NOT NULL AND TRIM(j.value) != ''
                    )
                    SELECT 
                        TRIM(value) as keyword,
                        COUNT(*) as occurrence_count
                    FROM split_data
                    GROUP BY TRIM(value)
                    ORDER BY occurrence_count DESC, keyword ASC;";

                    DBManager.Instance.ExecuteNonQuery(extractQuery);
                    DBManager.Instance.ExecuteNonQuery("CREATE INDEX idx_temp_keywords ON temp_keywords(keyword)");

                    // 5.2 전체 키워드 수 확인
                    int totalKeywords = Convert.ToInt32(DBManager.Instance.ExecuteScalar("SELECT COUNT(*) FROM temp_keywords"));
                    Debug.WriteLine($"총 키워드 수: {totalKeywords}");

                    // 5.3 페이징 처리를 위한 설정
                    int pageSize = 1000; // 한 번에 처리할 키워드 수
                    int totalPages = (int)Math.Ceiling(totalKeywords / (double)pageSize);

                    // 결과를 저장할 DataTable 생성
                    DataTable sumMoneyTable = new DataTable();
                    sumMoneyTable.Columns.Add("keyword", typeof(string));
                    sumMoneyTable.Columns.Add("occurrence_count", typeof(int));
                    sumMoneyTable.Columns.Add("total_money", typeof(decimal));

                    // 5.4 병렬 페이지 처리
                    List<Task<DataTable>> pageTasks = new List<Task<DataTable>>();

                    for (int page = 0; page < totalPages; page++)
                    {
                        int offset = page * pageSize;
                        int currentPage = page; // 클로저를 위해 복사

                        Task<DataTable> pageTask = Task.Run(() =>
                        {
                            string pageQuery = $@"
                    WITH page_keywords AS (
                        SELECT keyword, occurrence_count
                        FROM temp_keywords
                        LIMIT {pageSize} OFFSET {offset}
                    ),
                    split_data AS (
                        SELECT t.raw_data_id as row_id, TRIM(j.value) as value
                        FROM temp_transform_data t, 
                             json_each(json_array(
                                {jsonArrayBuilder.ToString()}
                             )) j
                        WHERE j.value IS NOT NULL AND TRIM(j.value) != ''
                    )
                    SELECT 
                        k.keyword,
                        k.occurrence_count,
                        COALESCE(SUM(CAST(m.'{moneyColumnName}' AS DECIMAL)), 0) as total_money
                    FROM page_keywords k
                    LEFT JOIN split_data s ON k.keyword = s.value
                    LEFT JOIN temp_money_data m ON s.row_id = m.raw_data_id
                    GROUP BY k.keyword, k.occurrence_count
                    ORDER BY k.occurrence_count DESC, k.keyword ASC;";

                            Debug.WriteLine($"페이지 {currentPage + 1}/{totalPages} 처리 시작");
                            DataTable pageResult = DBManager.Instance.ExecuteQuery(pageQuery);
                            Debug.WriteLine($"페이지 {currentPage + 1}/{totalPages} 처리 완료: {pageResult.Rows.Count}개 행");

                            return pageResult;
                        });

                        pageTasks.Add(pageTask);
                    }

                    // 모든 페이지 작업 대기
                    Debug.WriteLine($"총 {pageTasks.Count}개 페이지 작업 시작");
                    DataTable[] results = await Task.WhenAll(pageTasks);
                    Debug.WriteLine("모든 페이지 작업 완료");

                    // 결과 병합
                    foreach (DataTable pageResult in results)
                    {
                        foreach (DataRow row in pageResult.Rows)
                        {
                            sumMoneyTable.Rows.Add(
                                row["keyword"],
                                row["occurrence_count"],
                                row["total_money"]
                            );
                        }
                    }

                    Debug.WriteLine($"병합된 결과: {sumMoneyTable.Rows.Count}개 행");

                    // 이하 기존 코드와 동일
                    modifiedDataTable = new DataTable();
                    modifiedDataTable.Columns.Add("Value", typeof(string));
                    modifiedDataTable.Columns.Add("Count", typeof(int));
                    modifiedDataTable.Columns.Add("합산금액", typeof(string));
                    foreach (DataRow row in sumMoneyTable.Rows)
                    {
                        //modifiedDataTable.Rows.Add(row["keyword"], row["occurrence_count"], row["total_money"]);
                        modifiedDataTable.Rows.Add(row["keyword"], row["occurrence_count"], FormatToKoreanUnit(Convert.ToDecimal(row["total_money"].ToString())));

                    }

                    // 금액 데이터만 추출
                    //DataTable onlyMoneyValue = DataHandler.ExtractColumnToNewTable(sumMoneyTable, 2);

                    // 결과 병합
                    //modifiedDataTable = DataHandler.AddColumnsFromBToA(modifiedDataTable, onlyMoneyValue);

                    // 단위 환산 컬럼 추가
                    //modifiedDataTable = AddKoreanUnitColumn(modifiedDataTable);

                    // UI 업데이트 부분만 UI 스레드에서 실행
                    await Task.Run(() =>
                    {
                        if (Application.OpenForms.Count > 0)
                        {
                            Application.OpenForms[0].Invoke((MethodInvoker)delegate
                            {

                                // UI 컨트롤 접근은 이 블록 내부에서만 수행
                                /*
                                sum_keyword_table.DataSource = modifiedDataTable;
                                sum_keyword_table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                                //dataGridView_modified.Columns[1].HeaderText = "Count";
                                sum_keyword_table.Columns[1].DefaultCellStyle.Format = "N0";
                                //dataGridView_modified.Columns[2].HeaderText = "합산금액";
                                //dataGridView_modified.Columns[2].DefaultCellStyle.Format = "N0";
                                */
                                //dataGridview 직접 생성

                                if (sum_keyword_table.Rows.Count > 0)
                                {
                                    sum_keyword_table.Rows.Clear();
                                    sum_keyword_table.Columns.Clear();
                                }

                                // 원본 DataTable의 컬럼들 추가
                                foreach (DataColumn col in modifiedDataTable.Columns)
                                {
                                    sum_keyword_table.Columns.Add(col.ColumnName, col.ColumnName);
                                }


                                // 데이터 필터링 및 추가
                                foreach (DataRow row in modifiedDataTable.Rows)
                                {
                                    int rowIndex = sum_keyword_table.Rows.Add();

                                    // 데이터 채우기
                                    for (int i = 0; i < modifiedDataTable.Columns.Count; i++)
                                    {
                                        sum_keyword_table.Rows[rowIndex].Cells[i].Value = row[i];  // +1은 체크박스 컬럼 때문
                                    }


                                }

                                // DataGridView 속성 설정
                                sum_keyword_table.AllowUserToAddRows = false;
                                sum_keyword_table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                                sum_keyword_table.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                                sum_keyword_table.Font = new System.Drawing.Font("맑은 고딕", 14.25F);
                                //dgv.Columns[2].DefaultCellStyle.Format = "N0";
                                //dgv.Columns[3].DefaultCellStyle.Format = "N0";

                                //Debug.WriteLine($"dgv.Columns[1] : {dgv.Columns[1].Name}");
                                //Debug.WriteLine($"dgv.Columns[2] : {dgv.Columns[2].Name}");

                                // 나머지 컬럼들은 읽기 전용으로 설정
                                for (int i = 1; i < sum_keyword_table.Columns.Count; i++)
                                {
                                    sum_keyword_table.Columns[i].ReadOnly = true;
                                }

                            });
                        }
                    });


                    // 임시 테이블 정리
                    //CleanupTempTables();
                    DBManager.Instance.ExecuteNonQuery("DROP TABLE IF EXISTS temp_keywords");
                }
               
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"키워드 리스트 생성 오류: {ex.Message}");
                throw;
            }
            finally
            {
                Debug.WriteLine($"create_merge_keyword_list complete");
                searchYN = false;
            }
        }


        public async Task<DataTable> EnrichTransformDataWithRawData(DataTable transformDataTable)
        {
            try
            {
                // 원본 데이터를 수정하지 않도록 복사본 생성
                //DataTable resultTable = transformDataTable.Copy();
                DataTable resultTable = new DataTable();

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
                /*
                foreach (string column in visibleColumns)
                {
                    if (!resultTable.Columns.Contains(column))
                    {
                        resultTable.Columns.Add(column, typeof(string));
                    }
                }
                */
               
                // 먼저 visibleColumns 추가
                foreach (string column in visibleColumns)
                {
                    resultTable.Columns.Add(column, typeof(string));
                }

                // 그 다음 원본 transformDataTable의 컬럼 추가 (중복 제외)
                foreach (DataColumn column in transformDataTable.Columns)
                {
                    if (!resultTable.Columns.Contains(column.ColumnName))
                    {
                        resultTable.Columns.Add(column.ColumnName, column.DataType);
                    }
                }

                // 원본 데이터의 모든 행 복사
                foreach (DataRow originalRow in transformDataTable.Rows)
                {
                    DataRow newRow = resultTable.NewRow();

                    // 원본 테이블의 모든 컬럼 값을 새 행에 복사
                    foreach (DataColumn column in transformDataTable.Columns)
                    {
                        newRow[column.ColumnName] = originalRow[column.ColumnName];
                    }

                    resultTable.Rows.Add(newRow);
                }

                // 3. raw_data_id 컬럼이 있는지 확인
                if (!resultTable.Columns.Contains("raw_data_id"))
                {
                    Debug.WriteLine("transformDataTable에 raw_data_id 컬럼이 없습니다.");
                    return resultTable;
                }

                // 4. 모든 행의 raw_data_id 목록 수집
                HashSet<int> rawDataIds = new HashSet<int>();
                Dictionary<int, List<DataRow>> idToRowsMap = new Dictionary<int, List<DataRow>>();

                foreach (DataRow row in resultTable.Rows)
                {
                    if (row["raw_data_id"] != DBNull.Value &&
                        int.TryParse(row["raw_data_id"].ToString(), out int rawDataId))
                    {
                        rawDataIds.Add(rawDataId);

                        if (!idToRowsMap.ContainsKey(rawDataId))
                        {
                            idToRowsMap[rawDataId] = new List<DataRow>();
                        }
                        idToRowsMap[rawDataId].Add(row);
                    }
                }

                if (rawDataIds.Count == 0)
                {
                    Debug.WriteLine("유효한 raw_data_id가 없습니다.");
                    return resultTable;
                }

                // 5. 배치 처리로 원본 데이터 가져오기 (병렬 처리 수정)
                const int batchSize = 1000;
                List<int> idList = rawDataIds.ToList();

                // 안전한 배치 처리
                for (int i = 0; i < idList.Count; i += batchSize)
                {
                    int currentBatchSize = Math.Min(batchSize, idList.Count - i);
                    // 범위 체크 추가
                    if (i >= idList.Count || currentBatchSize <= 0)
                        continue;

                    List<int> batchIds = idList.GetRange(i, currentBatchSize);

                    string batchIdList = string.Join(",", batchIds);
                    string columnsToSelect = "id, " + string.Join(", ", visibleColumns);
                    string rawDataQuery = $@"
                                    SELECT {columnsToSelect}
                                    FROM raw_data
                                    WHERE id IN ({batchIdList})";

                    DataTable batchRawData = DBManager.Instance.ExecuteQuery(rawDataQuery);

                    // 조회된 데이터를 매핑
                    foreach (DataRow rawRow in batchRawData.Rows)
                    {
                        int id = Convert.ToInt32(rawRow["id"]);

                        if (idToRowsMap.ContainsKey(id))
                        {
                            foreach (DataRow resultRow in idToRowsMap[id])
                            {
                                foreach (string column in visibleColumns)
                                {
                                    if (rawRow[column] != DBNull.Value)
                                    {
                                        resultRow[column] = rawRow[column];
                                    }
                                }
                            }
                        }
                    }

                    Debug.WriteLine($"배치 처리 완료: {batchIds.Count}개 ID, 처리된 ID: {batchRawData.Rows.Count}");
                }

                // 6. id와 import_date 컬럼 삭제
                if (resultTable.Columns.Contains("id"))
                    resultTable.Columns.Remove("id");

                if (resultTable.Columns.Contains("import_date"))
                    resultTable.Columns.Remove("import_date");

                return resultTable;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"raw_data 데이터 추가 중 오류 발생: {ex.Message}");
                // 예외 발생 시 원본 데이터 테이블의 복사본 반환
                return transformDataTable.Copy();
            }
        }

        public DataTable FilterTransformDataByKeyword(DataTable viewTransformDataTable, DataTable originalTransformDataTable, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return viewTransformDataTable.Copy();

            DataTable resultTable = viewTransformDataTable.Clone();

            // 원본 transformDataTable의 컬럼명 목록 가져오기
            List<string> originalColumnNames = new List<string>();
            foreach (DataColumn col in originalTransformDataTable.Columns)
            {
                originalColumnNames.Add(col.ColumnName);
            }
            Debug.WriteLine($"originalColumnNames  : {string.Join(',', originalColumnNames)}");

            // viewTransformDataTable의 각 행에 대해 검색
            for (int rowIndex = 0; rowIndex < viewTransformDataTable.Rows.Count; rowIndex++)
            {
                DataRow row = viewTransformDataTable.Rows[rowIndex];
                bool containsKeyword = false;

                // 원본 컬럼명에 해당하는 컬럼만 검사
                foreach (string colName in originalColumnNames)
                {
                    if (viewTransformDataTable.Columns.Contains(colName) &&
                        row[colName] != null &&
                        row[colName] != DBNull.Value)
                    {
                        string cellValue = row[colName].ToString();

                        if (cellValue.Equals(keyword, StringComparison.Ordinal))
                        {
                            containsKeyword = true;
                            break;
                        }
                    }
                }

                if (containsKeyword)
                {
                    resultTable.Rows.Add(row.ItemArray);
                }
            }

            return resultTable;
        }


        private void CreateTempTableFromDataTable(string tableName, DataTable dt, bool preserveTable = false)
        {
            try
            {
                // 테이블 존재 여부 확인
                bool tableExists = false;
                string checkQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
                object result = DBManager.Instance.ExecuteScalar(checkQuery);
                tableExists = (result != null);


                // 테이블이 존재하지 않거나 보존하지 않을 경우에만 테이블 생성
                if (!tableExists || !preserveTable)
                {
                    //테이블이 존재하지 않는 경우 생성
                    if (!tableExists)
                    {
                        // 테이블 생성
                        StringBuilder createQuery = new StringBuilder();
                        createQuery.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");
                        List<string> columns = dt.Columns.Cast<DataColumn>()
                            .Select(col => $"{col.ColumnName} {GetSQLiteType(col.DataType)}")
                            .ToList();
                        createQuery.AppendLine(string.Join(",\n", columns));
                        createQuery.AppendLine(");");
                        DBManager.Instance.ExecuteNonQuery(createQuery.ToString());


                        //temp_transform_data의 경우 keywordColumns 값 저장
                        if (!preserveTable)
                        {
                            keywordColumnsCount = dt.Columns.Count;
                        }
                    }
                    //테이블이 존재하는 경우
                    else
                    {
                        //보존 여부 x 이면 이전 데이터 삭제
                        if (!preserveTable)
                        {
                            Debug.WriteLine($"DELETE FROM {tableName}");
                            DBManager.Instance.ExecuteNonQuery($"DELETE FROM {tableName}");
                        }
                    }
                }
                //테이블 존재 o & 보존 테이블인 경우 로직 종료
                else
                {
                    Debug.WriteLine($"Table Exist => return {tableName}");
                    return;
                }

                // 데이터 삽입
                using (var transaction = DBManager.Instance.BeginTransaction())
                {
                    var paramNames = dt.Columns.Cast<DataColumn>()
                        .Select(col => $"@{col.ColumnName}")
                        .ToList();
                    string insertQuery = $"INSERT INTO {tableName} VALUES ({string.Join(",", paramNames)})";
                    foreach (DataRow row in dt.Rows)
                    {
                        var parameters = new Dictionary<string, object>();
                        foreach (DataColumn col in dt.Columns)
                        {
                            parameters[col.ColumnName] = row[col] ?? DBNull.Value;
                        }
                        DBManager.Instance.ExecuteNonQuery(insertQuery, parameters);
                    }
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"임시 테이블 생성 오류: {ex.Message}");
                throw;
            }
        }



        private string GetSQLiteType(Type type)
        {
            if (type == typeof(int) || type == typeof(long))
                return "INTEGER";
            if (type == typeof(double) || type == typeof(decimal))
                return "REAL";
            if (type == typeof(byte[]))
                return "BLOB";
            return "TEXT";
        }

        private async void set_keyword_combo_list()
        {
            await Task.Run(() =>
            {
                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0].Invoke((MethodInvoker)delegate
                    {
                        // UI 컨트롤 접근은 이 블록 내부에서만 수행
                        keyword_search_combo.Items.Clear();
                        keyword_search_combo.Items.Add("키워드 선택");

                        foreach (DataRow row in modifiedDataTable.Rows)
                        {
                            keyword_search_combo.Items.Add(row[0].ToString());
                        }

                        if (keyword_search_combo.Items.Count > 0)
                            keyword_search_combo.SelectedIndex = 0; // 첫 번째 열 선택
                    });
                }
            });


        }

        public void CreateFilteredDataGridView(DataGridView dgv, DataTable dt, List<string> filterWords)
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
                if (filterWords.Count > 0)
                {
                    string firstColumnValue = row[0].ToString();

                    // list<string>의 항목과 비교
                    if (filterWords.Any(word => firstColumnValue.Contains(word)))
                    {
                        int rowIndex = dgv.Rows.Add();
                        dgv.Rows[rowIndex].Cells["CheckBox"].Value = false;  // 체크박스 초기값

                        // 데이터 채우기
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            dgv.Rows[rowIndex].Cells[i + 1].Value = row[i];  // +1은 체크박스 컬럼 때문

                        }

                    }
                }
                else
                {
                    int rowIndex = dgv.Rows.Add();
                    dgv.Rows[rowIndex].Cells["CheckBox"].Value = false;  // 체크박스 초기값

                    // 데이터 채우기
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dgv.Rows[rowIndex].Cells[i + 1].Value = row[i];  // +1은 체크박스 컬럼 때문
                    }
                }

            }

            // DataGridView 속성 설정
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = false;
            dgv.Columns["CheckBox"].ReadOnly = false;  // 체크박스 컬럼만 편집 가능
            dgv.Font = new System.Drawing.Font("맑은 고딕", 14.25F);
            //dgv.Columns[2].DefaultCellStyle.Format = "N0";
            //dgv.Columns[3].DefaultCellStyle.Format = "N0";

            //Debug.WriteLine($"dgv.Columns[1] : {dgv.Columns[1].Name}");
            //Debug.WriteLine($"dgv.Columns[2] : {dgv.Columns[2].Name}");

            // 나머지 컬럼들은 읽기 전용으로 설정
            for (int i = 1; i < dgv.Columns.Count; i++)
            {
                dgv.Columns[i].ReadOnly = true;
            }


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


        private void keyword_search_button_Click(object sender, EventArgs e)
        {
            _ = DoKeywordSearchAsync(sender, e);
            
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

            return checkedData;
        }

        //데이터 치환 함수
        public void ReplaceDataTableValues(List<string> targetList, DataTable dt, string replaceText, int startColumnIndex)
        {
            // 모든 행 순회
            foreach (DataRow row in dt.Rows)
            {
                // startColumnIndex부터 마지막 컬럼까지만 순회
                for (int colIndex = startColumnIndex; colIndex < dt.Columns.Count; colIndex++)
                {
                    string currentValue = row[colIndex]?.ToString() ?? "";

                    // targetList의 문자열과 일치하는지 확인
                    if (targetList.Any(target => currentValue.Equals(target, StringComparison.OrdinalIgnoreCase)))
                    {
                        row[colIndex] = replaceText;
                    }
                }
            }
        }

        private async void change_keyword_Click(object sender, EventArgs e)
        {
            string target_keyword = "";

            if ("".Equals(modified_keyword.Text.ToString()) || modified_keyword.Text == null)
            {
                MessageBox.Show("변환 키워드를 입력하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }
            else
            {
                target_keyword = modified_keyword.Text.ToString();
            }
            
            //1.선택된 테이블 내 키워드 목록 출력
            List<string> changeValuelList = GetCheckedRowsData(match_keyword_table);

            if (changeValuelList.Count == 0)
            {
                MessageBox.Show("키워드 변환 대상을 선택하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }

            using (var progressForm = new ProcessProgressForm())
            {
                progressForm.Show();
                await progressForm.UpdateProgressHandler(10, "키워드 변환 중...");
                await Task.Delay(10);

                //2.dataTransform dataTable 내 키워드 일괄 변환
                //2,2 -> dataTable 에서 일일히 찾아가면서 변환
                //0,1번 index는 부서,공급업체명 일 것이라 가정하므로 2번 index부터 치환(현재는 부서,공급업체명을 표기하지 않는다)
                ReplaceDataTableValues(changeValuelList, transformDataTable, target_keyword, 0);

                await progressForm.UpdateProgressHandler(30, "키워드 변환 내역 저장 중...");
                await Task.Delay(10);

                //viewTransformDataTable 도 변환 
                Debug.WriteLine("EnrichTransformDataWithRawData start");
                viewTransformDataTable = await EnrichTransformDataWithRawData(transformDataTable);
                Debug.WriteLine("EnrichTransformDataWithRawData end");


                await progressForm.UpdateProgressHandler(60, "변환 키워드 기반 요약 정보 재 산출 중...");
                await Task.Delay(10);

                //3.변경된 키워드 기반 리스트 재 생성
                create_merge_keyword_list();
                set_keyword_combo_list();


                await progressForm.UpdateProgressHandler(90, "화면 완료...");
                await Task.Delay(10);


                await progressForm.UpdateProgressHandler(100);
                await Task.Delay(10);
                progressForm.Close();

            }
           

           

           
            MessageBox.Show("키워드 변환이 완료되었습니다.", "Info",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Information);


            match_keyword_table.DataSource = null;
            match_keyword_table.Rows.Clear();
            match_keyword_table.Columns.Clear();
            if (DataHandler.dragSelections.ContainsKey(match_keyword_table))
            {
                DataHandler.dragSelections[match_keyword_table].Clear();
            }
            
            dataGridView_transform.DataSource = null;
            dataGridView_transform.Rows.Clear();
            dataGridView_transform.Columns.Clear();

            //search_keyword_detail_list();

            //변환된 행 값으로 자동 선택

            bool exactMatch = true;

            for (int i = 0; i < sum_keyword_table.Rows.Count; i++)
            {
                if (sum_keyword_table.Rows[i].Cells[0].Value != null)
                {
                    string cellValue = sum_keyword_table.Rows[i].Cells[0].Value.ToString();

                    bool match = exactMatch
                        ? cellValue.Equals(target_keyword, StringComparison.OrdinalIgnoreCase)
                        : cellValue.IndexOf(target_keyword, StringComparison.OrdinalIgnoreCase) >= 0;

                    if (match)
                    {
                        // 현재 선택 모두 해제
                        sum_keyword_table.ClearSelection();

                        // 행 선택
                        sum_keyword_table.Rows[i].Selected = true;

                        // 선택한 행이 보이도록 스크롤
                        sum_keyword_table.FirstDisplayedScrollingRowIndex = i;

                    }
                }
            }
            // 키워드를 사용하여 transformDataTable 필터링
            DataTable filteredTable = FilterTransformDataByKeyword(viewTransformDataTable, transformDataTable, target_keyword);

            // 필터링된 결과를 다른 DataGridView에 표시
            dataGridView_2nd.DataSource = null;
            dataGridView_2nd.Rows.Clear();
            dataGridView_2nd.Columns.Clear();
            dataGridView_2nd.DataSource = filteredTable;
            //dataGridView_2nd.Columns["import_date"].Visible = false;

            if (dataGridView_2nd.Columns["raw_data_id"] != null)
            {
                dataGridView_2nd.Columns["raw_data_id"].Visible = false;
            }
        }

        private void check_all_keyword_list_CheckedChanged(object sender, EventArgs e)
        {
            // 모든 행의 체크박스 상태 변경
            foreach (DataGridViewRow row in match_keyword_table.Rows)
            {
                row.Cells[0].Value = check_all_keyword_list.Checked;
            }
        }

        // User Control을 Form으로 감싸서 보여주는 방법
        public void ShowUserControlAsDialog(UserControl userControl)
        {
            Form form = new Form();

            // Form 설정
            form.Text = "Clustering 결과 확인";
            form.StartPosition = FormStartPosition.CenterParent;
            //form.FormBorderStyle = FormBorderStyle.FixedDialog;
            //form.MaximizeBox = false;
            //form.MinimizeBox = false;

            // User Control 추가
            userControl.Dock = DockStyle.None;  // User Control이 Form에 꽉 차게 설정
            form.Controls.Add(userControl);

            // Form 크기를 User Control 크기에 맞게 조정
            form.ClientSize = new Size(userControl.Width, userControl.Height);

            // 모달 다이얼로그로 표시
            form.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Data Clustering
            //데이터가 없을 경우 신규 생성
            if (DataHandler.firstClusteringData.Rows.Count == 0)
            {
                DataHandler.firstClusteringData = DataHandler.CreateSetGroupDataTable(originDataTable, DataHandler.moneyDataTable);
            }

            if (DataHandler.secondClusteringData.Rows.Count == 0)
            {
                DataHandler.secondClusteringData = DataHandler.CreateSetGroupDataTable(transformDataTable, DataHandler.moneyDataTable, true);
            }


            uc_clusteringPopup popup_control = new uc_clusteringPopup();

            popup_control.initUI();

            ShowUserControlAsDialog(popup_control);
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            if (isFinishSession)
            {
                DialogResult dupleCheckResult = MessageBox.Show(
                $"현재 페이지에서 수정된 정보를 기준으로 Clustering 페이지를 갱신하기 위해 "
                + "기존 Clustering 페이지의 수정 내역을 초기화합니다."
                + "현재 페이지 정보를 기준으로 Clustering 페이지로 이동하시겠습니까?"
                + "\n(원치 않으실 경우 상단 메뉴바 > Clustering 항목을 클릭하여 이동 가능합니다. )",
                "Clustering 페이지 초기화 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
                );

                if (dupleCheckResult != DialogResult.Yes)
                {
                    return;
                }
                else
                {
                    DataHandler.finalClusteringData = null;
                }
            }

            using (var progressForm = new ProcessProgressForm())
            {
                progressForm.Show();
                await progressForm.UpdateProgressHandler(10, "데이터 저장 준비 중...");
                await Task.Delay(10);
                DataHandler.secondClusteringData = DataHandler.CreateSetGroupDataTable(transformDataTable, DataHandler.moneyDataTable, true);

                Debug.WriteLine("CreateSetGroupDataTable 수행 완료");

                DataHandler.recomandKeywordTable = modifiedDataTable;

                await progressForm.UpdateProgressHandler(30, "데이터 저장 준비 중...");
                await Task.Delay(10);

                userControlHandler.uc_clustering.initUI();

                await progressForm.UpdateProgressHandler(40, "화면 구성 중...");
                await Task.Delay(10);


                if (this.ParentForm is Form1 form)
                {
                    form.LoadUserControl(userControlHandler.uc_clustering);
                }
                await progressForm.UpdateProgressHandler(90, "화면 완료...");
                await Task.Delay(10);

                isFinishSession = true;

                await progressForm.UpdateProgressHandler(100);
                await Task.Delay(10);
                progressForm.Close();

            }
            
            /*
            DataHandler.secondClusteringData = DataHandler.CreateSetGroupDataTable(transformDataTable, DataHandler.moneyDataTable, true);


            DataHandler.recomandKeywordTable = modifiedDataTable;

            userControlHandler.uc_clustering.initUI();

            if (this.ParentForm is Form1 form)
            {
                form.LoadUserControl(userControlHandler.uc_clustering);
            }

            isFinishSession = true;
            */
        }

        private void search_keyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyword_search_button_Click(sender, e);   // 호출하고 싶은 함수
                e.SuppressKeyPress = true;  // 비프음 방지
            }
        }

        private void keyword_search_radio2_CheckedChanged(object sender, EventArgs e)
        {
            search_keyword.Enabled = keyword_search_radio2.Checked;
        }

        private void keyword_search_radio1_CheckedChanged(object sender, EventArgs e)
        {
            search_keyword.Enabled = keyword_search_radio2.Checked;

        }

        private void dept_col_check_CheckedChanged(object sender, EventArgs e)
        {
            DataHandler.dept_col_yn = dept_col_check.Checked;

            //기존 clustering 결과는 초기화
            if (DataHandler.secondClusteringData.Rows.Count > 0)
            {
                DataHandler.secondClusteringData = DataHandler.CreateSetGroupDataTable(transformDataTable, DataHandler.moneyDataTable, true);
            }
            
        }

        private void prod_col_check_CheckedChanged(object sender, EventArgs e)
        {
            DataHandler.prod_col_yn = prod_col_check.Checked;

            //기존 clustering 결과는 초기화
            if (DataHandler.secondClusteringData.Rows.Count > 0)
            {
                DataHandler.secondClusteringData = DataHandler.CreateSetGroupDataTable(transformDataTable, DataHandler.moneyDataTable, true);
            }
            
        }

        private void dataGridView_modified_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            search_keyword_detail_list();
        }

        private void search_keyword_detail_list()
        {
            if (sum_keyword_table.SelectedCells.Count > 0)
            {
                // 선택된 셀의 행 인덱스 가져오기
                int rowIndex = sum_keyword_table.SelectedCells[0].RowIndex;

                // 선택된 행의 첫 번째 열(Value/Keyword) 값 가져오기
                string keyword = sum_keyword_table.Rows[rowIndex].Cells[0].Value.ToString();

                // 키워드를 사용하여 transformDataTable 필터링
                DataTable filteredTable = FilterTransformDataByKeyword(viewTransformDataTable, transformDataTable, keyword);

                // 필터링된 결과를 다른 DataGridView에 표시
                dataGridView_2nd.DataSource = null;
                dataGridView_2nd.Rows.Clear();
                dataGridView_2nd.Columns.Clear();
                dataGridView_2nd.DataSource = filteredTable;
                //dataGridView_2nd.Columns["import_date"].Visible = false;

                if (dataGridView_2nd.Columns["raw_data_id"] != null)
                {
                    dataGridView_2nd.Columns["raw_data_id"].Visible = false;
                }

                // 또는 상태 표시줄에 결과 개수 표시
                Debug.WriteLine($"키워드 '{keyword}'를 포함하는 행: {filteredTable.Rows.Count}개");
            }
        }

        private void match_keyword_table_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (match_keyword_table.SelectedCells.Count > 0)
            {
                // 선택된 셀의 행 인덱스 가져오기
                int rowIndex = match_keyword_table.SelectedCells[0].RowIndex;

                // 선택된 행의 첫 번째 열(Value/Keyword) 값 가져오기
                string keyword = match_keyword_table.Rows[rowIndex].Cells[1].Value.ToString();

                // 키워드를 사용하여 transformDataTable 필터링
                DataTable filteredTable = FilterTransformDataByKeyword(viewTransformDataTable, transformDataTable, keyword);

                // 필터링된 결과를 다른 DataGridView에 표시
                dataGridView_transform.DataSource = filteredTable;
                //dataGridView_transform.Columns["import_date"].Visible = false;
                if (dataGridView_transform.Columns["raw_data_id"] != null)
                {
                    dataGridView_transform.Columns["raw_data_id"].Visible = false;
                }

                // 또는 상태 표시줄에 결과 개수 표시
                Debug.WriteLine($"키워드 '{keyword}'를 포함하는 행: {filteredTable.Rows.Count}개");
            }
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
            //await Task.Run(() => create_merge_keyword_list(true));
            //create_merge_keyword_list(true);
            // Task.Run을 사용하여 create_merge_keyword_list를 실행하고 완료될 때까지 기다림
                    await Task.Run(() => {
                        // UI 스레드에서 실행해야 하는 부분이 있다면 Invoke 사용
                        this.Invoke((MethodInvoker)delegate {
                            create_merge_keyword_list(true);                           
                        });
                    });

            if (match_keyword_table.Rows.Count > 0)
            {
                Debug.WriteLine("keyword_search_button_Click 함수 호출");
                await DoKeywordSearchAsync(sender, e);
            }

        }

        // 비동기 작업을 수행하는 내부 함수
        private async Task DoKeywordSearchAsync(object sender, EventArgs e)
        {

            //searchYN =true 이면 대기
            while (searchYN)
            {
                await Task.Delay(10);
            }
            string target_keyword = "";

            //combobox로 검색할 경우
            if (keyword_search_radio1.Checked)
            {
                if (keyword_search_combo.SelectedIndex != 0)
                {
                    target_keyword = keyword_search_combo.SelectedItem.ToString();
                }
            }
            //직접 검색할 경우
            else if (keyword_search_radio2.Checked)
            {
                if (!"".Equals(search_keyword.Text.ToString()) && search_keyword.Text != null)
                {
                    target_keyword = search_keyword.Text.ToString();
                }
            }

            //List<string> lowlevelList = DataHandler.GetColumnValuesAsList(DataHandler.lowLevelData, 0);
            List<string> valuelList = DataHandler.GetColumnValuesAsList(modifiedDataTable, 0);

            List<string> MathcingPairs = new List<string>();

            if (!"".Equals(target_keyword))
            {
                MathcingPairs = DataHandler.FindMachKeyword(valuelList, target_keyword);
                Debug.WriteLine($"MathcingPairs.Count : {MathcingPairs.Count}");
                if (MathcingPairs.Count == 0)
                {
                    match_keyword_table.DataSource = null;
                    match_keyword_table.Rows.Clear();
                    match_keyword_table.Columns.Clear();
                    if (DataHandler.dragSelections.ContainsKey(match_keyword_table))
                    {
                        DataHandler.dragSelections[match_keyword_table].Clear();
                    }
                    return;
                }
            }


            CreateFilteredDataGridView(match_keyword_table, modifiedDataTable, MathcingPairs);

            check_all_keyword_list.Checked = false;

            modified_keyword.Text = target_keyword;
        }

    }

}
