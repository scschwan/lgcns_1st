using DocumentFormat.OpenXml.Presentation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinanceTool
{
    public partial class uc_preprocessing : UserControl
    {
        bool iskeywordExtractor = false;
        private bool isProcessingSelection = false;
        DataTable modifiedDataTable;
        DataTable originKeywordDataTable;

        
        private bool _dataLoaded = false;

        private bool isExtractRunning = false;

        public uc_preprocessing()
        {
            InitializeComponent();
        }

        public void initUI()
        {
            Debug.WriteLine($"[preprocessing]processTable.Columns.Count : {DataHandler.processTable.Columns.Count}");
            Debug.WriteLine($"[preprocessing]processTable.Rows.Count : {DataHandler.processTable.Rows.Count}");

            // 데이터베이스에 전처리 뷰 생성
            //CreatePreprocessingView();

            originKeywordDataTable = DataHandler.CreateDataTableFromColumns(DataHandler.processTable, DataHandler.levelList);

            dataGridView_target.DataSource = originKeywordDataTable;
            _dataLoaded = true;

            // 금액 컬럼 visible false
            dataGridView_target.Columns[0].Visible = false;
            // raw_data_id visible false
            dataGridView_target.Columns["raw_data_id"].Visible = false;
            dataGridView_target.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            modifiedDataTable = DataHandler.CreateDataTableFromColumns(DataHandler.processTable, DataHandler.levelList);
            //DataHandler.moneyDataTable = DataHandler.ExtractColumnToNewTable(modifiedDataTable, 0);
            DataHandler.moneyDataTable = DataHandler.ExtractColumnToNewTable(DataHandler.processTable, DataHandler.levelList[0]);


            Debug.WriteLine($"modifiedDataTable.Columns.Count : {modifiedDataTable.Columns.Count}");
            Debug.WriteLine($"modifiedDataTable.Columns.Count : {modifiedDataTable.Columns[2].ColumnName}");

            //DataHandler.separator = _separatorManager.Separators;
            //DataHandler.remover = _separatorManager.Removers;

            // 구분자 및 불용어 목록 추가
            LoadSeparatorsAndRemovers();

            // 데이터그리드뷰 간 선택 동기화
            DataHandler.SyncDataGridViewSelections(dataGridView_target, dataGridView_applied);

            DataHandler.RegisterDataGridView(dataGridView_seperator);
            DataHandler.RegisterDataGridView(dataGridView_remove);
        }

        

        private void LoadSeparatorsAndRemovers()
        {
            // 프로그램 시작 시 로드
            DataHandler.spManager = new SeparatorManager();

            // 데이터 가져오기 및 중복 제거
            List<string> seperate_list = DataHandler.spManager.Separators
                .Distinct()  // 중복 제거
                .ToList();   // List로 변환

            List<string> remove_list = DataHandler.spManager.Removers
                .Distinct()  // 중복 제거
                .ToList();   // List로 변환

            //구분자 리스트 추가
            create_seperate_table(dataGridView_seperator, seperate_list);

            //불용어 리스트 추가
            create_seperate_table(dataGridView_remove, remove_list);
        }

        private void create_seperate_table(DataGridView dgv, List<string> data_list)
        {
            // DataGridView 초기화
            dgv.DataSource = null;
            dgv.Rows.Clear();
            dgv.Columns.Clear();

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

            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Columns["Data"].ReadOnly = true;  // 체크박스 컬럼만 편집 가능
            dgv.Columns["CheckBox"].ReadOnly = false;  // 체크박스 컬럼만 편집 가능
            dgv.Font = new System.Drawing.Font("맑은 고딕", 14.25F);
        }


        private async void btn_apply_Click(object sender, EventArgs e)
        {
            //await ApplyToAllData(dt => DataHandler.ReplaceSeparatorInColumn(dt, dt.Columns.Count - 1, "_", "separate"));
            // 키워드 추출이 이미 수행되었다면 데이터 초기화 후 재수행
            if (iskeywordExtractor)
            {
                modifiedDataTable = DataHandler.CreateDataTableFromColumns(DataHandler.processTable, DataHandler.levelList);
            }

            // raw_data_id 컬럼 정보 임시 저장
            DataColumn rawDataIdColumn = null;
            Dictionary<string, object> rawDataIdValues = new Dictionary<string, object>();



            // 컬럼 객체 저장
            rawDataIdColumn = modifiedDataTable.Columns["raw_data_id"];

            // 각 행의 raw_data_id 값을 저장
            foreach (DataRow row in modifiedDataTable.Rows)
            {
                // 행 식별을 위한 고유 키 생성 (여기서는 행 인덱스 사용)
                string rowKey = modifiedDataTable.Rows.IndexOf(row).ToString();
                rawDataIdValues[rowKey] = row["raw_data_id"];
            }

            // 컬럼 제거
            modifiedDataTable.Columns.Remove("raw_data_id");

            modifiedDataTable = DataHandler.ReplaceSeparatorInColumn(modifiedDataTable, modifiedDataTable.Columns.Count -1, "_", "separate");

            // 새 raw_data_id 컬럼 추가
            DataColumn newRawDataIdColumn = new DataColumn("raw_data_id", rawDataIdColumn.DataType);
            modifiedDataTable.Columns.Add(newRawDataIdColumn);


            // 각 행에 저장해둔 값 복원
            for (int i = 0; i < modifiedDataTable.Rows.Count && i < rawDataIdValues.Count; i++)
            {
                // 같은 위치의 행에 값 복원
                modifiedDataTable.Rows[i]["raw_data_id"] = rawDataIdValues[i.ToString()];
            }

            //dataGridView_applied.DataSource = modifiedDataTable;
            dataGridView_applied.DataSource = DataHandler.CombineDataTables(modifiedDataTable);
            dataGridView_applied.Columns["raw_data_id"].Visible = false;
            iskeywordExtractor = true;
            isProcessingSelection = false;
        }

        private async void remove_apply_btn_Click(object sender, EventArgs e)
        {
            if (iskeywordExtractor)
            {
                modifiedDataTable = DataHandler.CreateDataTableFromColumns(DataHandler.processTable, DataHandler.levelList);
            }


            // raw_data_id 컬럼 정보 임시 저장
            DataColumn rawDataIdColumn = null;
            Dictionary<string, object> rawDataIdValues = new Dictionary<string, object>();



            // 컬럼 객체 저장
            rawDataIdColumn = modifiedDataTable.Columns["raw_data_id"];

            // 각 행의 raw_data_id 값을 저장
            foreach (DataRow row in modifiedDataTable.Rows)
            {
                // 행 식별을 위한 고유 키 생성 (여기서는 행 인덱스 사용)
                string rowKey = modifiedDataTable.Rows.IndexOf(row).ToString();
                rawDataIdValues[rowKey] = row["raw_data_id"];
            }

            // 컬럼 제거
            modifiedDataTable.Columns.Remove("raw_data_id");

            //await ApplyToAllData(dt => DataHandler.ReplaceSeparatorInColumn(dt, dt.Columns.Count - 1, "", "remove"));
            modifiedDataTable = DataHandler.ReplaceSeparatorInColumn(modifiedDataTable, modifiedDataTable.Columns.Count - 1, "", "remove");

            // 새 raw_data_id 컬럼 추가
            DataColumn newRawDataIdColumn = new DataColumn("raw_data_id", rawDataIdColumn.DataType);
            modifiedDataTable.Columns.Add(newRawDataIdColumn);


            // 각 행에 저장해둔 값 복원
            for (int i = 0; i < modifiedDataTable.Rows.Count && i < rawDataIdValues.Count; i++)
            {
                // 같은 위치의 행에 값 복원
                modifiedDataTable.Rows[i]["raw_data_id"] = rawDataIdValues[i.ToString()];
            }

            //dataGridView_applied.DataSource = modifiedDataTable;
            dataGridView_applied.DataSource = DataHandler.CombineDataTables(modifiedDataTable);
            dataGridView_applied.Columns["raw_data_id"].Visible = false;
            iskeywordExtractor = true;
            isProcessingSelection = false;
        }

       
        private async void keyword_seper_split_Click(object sender, EventArgs e)
        {
            try
            {
                isExtractRunning = true;
                using (var progressForm = new ProcessProgressForm())
                {
                   
                    progressForm.Show();
                    progressForm.UpdateProgressHandler(10);

                    // 키워드 추출이 이미 수행되었다면 데이터 초기화 후 재수행
                    if (iskeywordExtractor)
                    {
                        modifiedDataTable = DataHandler.CreateDataTableFromColumns(DataHandler.processTable, DataHandler.levelList);
                        progressForm.UpdateProgressHandler(20);
                    }

                    // raw_data_id 컬럼 정보 임시 저장
                    DataColumn rawDataIdColumn = null;
                    Dictionary<string, object> rawDataIdValues = new Dictionary<string, object>();

                    Debug.WriteLine($"modifiedDataTable.Columns.Count : {modifiedDataTable.Columns.Count}");
                    
                    // 컬럼 객체 저장
                    rawDataIdColumn = modifiedDataTable.Columns["raw_data_id"];

                    // 각 행의 raw_data_id 값을 저장
                    foreach (DataRow row in modifiedDataTable.Rows)
                    {
                        // 행 식별을 위한 고유 키 생성 (여기서는 행 인덱스 사용)
                        string rowKey = modifiedDataTable.Rows.IndexOf(row).ToString();
                        rawDataIdValues[rowKey] = row["raw_data_id"];
                    }

                    // 컬럼 제거
                    modifiedDataTable.Columns.Remove("raw_data_id");

                    // 1.구분자 변환
                    modifiedDataTable = await Task.Run(() =>
                        //DataHandler.ReplaceSeparatorInColumn(modifiedDataTable, modifiedDataTable.Columns.Count - 1, "_", "separate"));
                        DataHandler.ReplaceSeparatorInColumn(modifiedDataTable, modifiedDataTable.Columns.Count - 1, "_", "separate"));
                    await progressForm.UpdateProgressHandler(30);
                    await Task.Delay(10);

                    // 2.불용어 제거
                    modifiedDataTable = await Task.Run(() =>
                        //DataHandler.ReplaceSeparatorInColumn(modifiedDataTable, modifiedDataTable.Columns.Count - 1, "", "remove"));
                        DataHandler.ReplaceSeparatorInColumn(modifiedDataTable, modifiedDataTable.Columns.Count - 1, "", "remove"));
                    await progressForm.UpdateProgressHandler(50);
                    await Task.Delay(10);

                    // 3.구분자 기반 추출 전 전처리
                    modifiedDataTable = await Task.Run(() => DataHandler.ProcessShortStringsToNull(modifiedDataTable));
                    modifiedDataTable = await Task.Run(() => DataHandler.ProcessUnderscoresInAllColumn(modifiedDataTable));
                    await progressForm.UpdateProgressHandler(70);
                    await Task.Delay(10);

                    // 4.구분자 기반 추출
                    modifiedDataTable = await Task.Run(() => DataHandler.SplitColumnBySeparator(modifiedDataTable, "_"));
                    await progressForm.UpdateProgressHandler(80);
                    await Task.Delay(10);

                    // 새 raw_data_id 컬럼 추가
                    DataColumn newRawDataIdColumn = new DataColumn("raw_data_id", rawDataIdColumn.DataType);
                    modifiedDataTable.Columns.Add(newRawDataIdColumn);

                    
                    // 각 행에 저장해둔 값 복원
                    for (int i = 0; i < modifiedDataTable.Rows.Count && i < rawDataIdValues.Count; i++)
                    {
                        // 같은 위치의 행에 값 복원
                        modifiedDataTable.Rows[i]["raw_data_id"] = rawDataIdValues[i.ToString()];
                    }

                    await progressForm.UpdateProgressHandler(90);
                    await Task.Delay(10);


                    dataGridView_applied.DataSource = DataHandler.CombineDataTables(modifiedDataTable);
                    dataGridView_applied.Columns["raw_data_id"].Visible = false;
                    iskeywordExtractor = true;
                    isProcessingSelection = true;

                    if (!nlp_groupBox.Visible)
                    {
                        nlp_groupBox.Visible = true;
                    }

                    progressForm.UpdateProgressHandler(100);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"키워드 추출 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"키워드 추출 오류: {ex.Message}");
            }
            finally
            {
                isExtractRunning = false;
            }
        }

        private async void keyword_model_split_Click(object sender, EventArgs e)
        {
            try
            {
                isExtractRunning = true;
                Console.WriteLine("Model_split start");
                Stopwatch sw = Stopwatch.StartNew();

                // raw_data_id 컬럼 정보 임시 저장
                DataColumn rawDataIdColumn = null;
                Dictionary<string, object> rawDataIdValues = new Dictionary<string, object>();


                // NLP 기반 키워드 추출 (모든 데이터에 적용)
                //modifiedDataTable = await DataHandler.SplitColumnByModel(modifiedDataTable, ai_limit_cnt);

                
                // 컬럼 객체 저장
                rawDataIdColumn = modifiedDataTable.Columns["raw_data_id"];

                // 각 행의 raw_data_id 값을 저장
                foreach (DataRow row in modifiedDataTable.Rows)
                {
                    // 행 식별을 위한 고유 키 생성 (여기서는 행 인덱스 사용)
                    string rowKey = modifiedDataTable.Rows.IndexOf(row).ToString();
                    rawDataIdValues[rowKey] = row["raw_data_id"];
                }

                // 컬럼 제거
                modifiedDataTable.Columns.Remove("raw_data_id");
                

                // NLP 함수 호출
                modifiedDataTable = await DataHandler.SplitColumnByModel(modifiedDataTable, ai_limit_cnt);


                
                // 새 raw_data_id 컬럼 추가
                DataColumn newRawDataIdColumn = new DataColumn("raw_data_id", rawDataIdColumn.DataType);
                modifiedDataTable.Columns.Add(newRawDataIdColumn);

                // 각 행에 저장해둔 값 복원
                for (int i = 0; i < modifiedDataTable.Rows.Count && i < rawDataIdValues.Count; i++)
                {
                    // 같은 위치의 행에 값 복원
                    modifiedDataTable.Rows[i]["raw_data_id"] = rawDataIdValues[i.ToString()];
                }
                

                dataGridView_applied.DataSource = DataHandler.CombineDataTables(modifiedDataTable);
                dataGridView_applied.Columns["raw_data_id"].Visible = false;

                sw.Stop();
                Debug.WriteLine($"Excel → SQLite 변환 완료. 소요 시간: {sw.ElapsedMilliseconds}ms, 행 수: {dataGridView_applied.Rows.Count}");

                Console.WriteLine("Model_split end");

                iskeywordExtractor = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"NLP 키워드 추출 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"NLP 키워드 추출 오류: {ex.Message}");
            }
            finally
            {
                isExtractRunning = false;
            }
        }

        private async void remove_1key_Click(object sender, EventArgs e)
        {
            try
            {
                isExtractRunning = true; 
                // raw_data_id 컬럼 정보 임시 저장
                DataColumn rawDataIdColumn = null;
                Dictionary<string, object> rawDataIdValues = new Dictionary<string, object>();

                Debug.WriteLine($"modifiedDataTable.Columns.Count : {modifiedDataTable.Columns.Count}");

                // 컬럼 객체 저장
                rawDataIdColumn = modifiedDataTable.Columns["raw_data_id"];

                // 각 행의 raw_data_id 값을 저장
                foreach (DataRow row in modifiedDataTable.Rows)
                {
                    // 행 식별을 위한 고유 키 생성 (여기서는 행 인덱스 사용)
                    string rowKey = modifiedDataTable.Rows.IndexOf(row).ToString();
                    rawDataIdValues[rowKey] = row["raw_data_id"];
                }

                // 컬럼 제거
                modifiedDataTable.Columns.Remove("raw_data_id");

                // 각 행에서 최대 몇 개의 유효한 항목(1글자 아닌)이 있는지 계산
                int maxValidItemsPerRow = 0;
                foreach (DataRow row in modifiedDataTable.Rows)
                {
                    int validCount = 0;
                    foreach (var item in row.ItemArray)
                    {
                        string value = item?.ToString() ?? "";
                        if (value.Length != 1)
                        {
                            validCount++;
                        }
                    }
                    maxValidItemsPerRow = Math.Max(maxValidItemsPerRow, validCount);
                }

                // 새로운 DataTable 생성 - 유효한 항목 수만큼 컬럼 생성
                DataTable result = new DataTable();
                for (int i = 0; i < maxValidItemsPerRow; i++)
                {
                    result.Columns.Add($"Column{i}", typeof(string));
                }

                // 각 행을 처리
                foreach (DataRow originalRow in modifiedDataTable.Rows)
                {
                    DataRow newRow = result.NewRow();
                    int newIndex = 0;

                    // 각 셀 처리 - 길이가 1인 항목만 제거하고 나머지는 순서대로 채움
                    foreach (var item in originalRow.ItemArray)
                    {
                        string value = item?.ToString() ?? "";
                        if (value.Length != 1)
                        {
                            if (newIndex < result.Columns.Count)
                            {
                                newRow[newIndex] = item;
                                newIndex++;
                            }
                        }
                    }

                    result.Rows.Add(newRow);
                }

                Debug.WriteLine($"modifiedDataTable.Rows.Count : {modifiedDataTable.Rows.Count}");
                Debug.WriteLine($"result.Rows.Count : {result.Rows.Count}");

                modifiedDataTable = result;

                // 새 raw_data_id 컬럼 추가
                DataColumn newRawDataIdColumn = new DataColumn("raw_data_id", rawDataIdColumn.DataType);
                modifiedDataTable.Columns.Add(newRawDataIdColumn);

                // 각 행에 저장해둔 값 복원
                for (int i = 0; i < modifiedDataTable.Rows.Count && i < rawDataIdValues.Count; i++)
                {
                    // 같은 위치의 행에 값 복원
                    modifiedDataTable.Rows[i]["raw_data_id"] = rawDataIdValues[i.ToString()];
                }

                dataGridView_applied.DataSource = DataHandler.CombineDataTables(modifiedDataTable);
                dataGridView_applied.Columns["raw_data_id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"1글자 키워드 제거 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"1글자 키워드 제거 오류: {ex.Message}");
            }finally
            {
                isExtractRunning = false;
            }

        }

        private async void btn_complete_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("btn_complete_Click start");
            Debug.WriteLine($"modifiedDataTable.Columns.Count : {modifiedDataTable.Columns.Count} modifiedDataTable.Rows.Count :  {modifiedDataTable.Rows.Count}");
            if (modifiedDataTable.Columns.Count < 4 && !isProcessingSelection)
            {
                MessageBox.Show("키워드 추출이 완료되지 않았습니다.", "알림",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (var progressForm = new ProcessProgressForm())
                {
                    progressForm.Show();

                    if (isExtractRunning)
                    {
                        while (isExtractRunning)
                        {
                            await progressForm.UpdateProgressHandler(5, "전처리 데이터 저장 준비 중...");
                            await Task.Delay(10);
                        }
                        
                    }

                    await progressForm.UpdateProgressHandler(10, "전처리 데이터 저장 준비 중...");
                    await SaveProcessData(modifiedDataTable, progressForm.UpdateProgressHandler);
                    Debug.WriteLine("SaveProcessData 수행 완료");

                    await progressForm.UpdateProgressHandler(50, "데이터 전송 중...");

                    Debug.WriteLine("UI 초기화 시작");
                    // initUI가 이미 Task를 반환하므로 추가 Task.Run 없이 직접 await
                    
                    await userControlHandler.uc_dataTransform.initUI();

                    await progressForm.UpdateProgressHandler(80, "데이터 전송 완료");

                    if (this.ParentForm is Form1 form)
                    {
                        Debug.WriteLine("btn_complete_Click -> LoadUserControl start");
                        form.LoadUserControl(userControlHandler.uc_dataTransform);
                        Debug.WriteLine("btn_complete_Click -> LoadUserControl complete");
                        await progressForm.UpdateProgressHandler(100, "데이터 저장 완료");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 처리 완료 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"데이터 처리 완료 오류: {ex.Message}");
            }
        }

        private async Task SaveProcessData(DataTable dataTable, ProcessProgressForm.UpdateProgressDelegate progress)
        {
            try
            {
                await progress(20, "테이블 생성 중...");
                await Task.Run(() =>
                {
                    DBManager.Instance.ExecuteNonQuery("DROP TABLE IF EXISTS process_view_data");

                    StringBuilder createTableQuery = new StringBuilder();
                    createTableQuery.AppendLine("CREATE TABLE process_view_data (");

                    // id와 원본 ID 컬럼만 추가
                    createTableQuery.AppendLine("id INTEGER PRIMARY KEY AUTOINCREMENT,");
                    createTableQuery.AppendLine("raw_data_id INTEGER,");

                    List<string> columns = new List<string>();
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        string sqliteType = "TEXT";
                        if (col.DataType == typeof(int) || col.DataType == typeof(long))
                            sqliteType = "INTEGER";
                        else if (col.DataType == typeof(double) || col.DataType == typeof(decimal))
                            sqliteType = "REAL";
                        else if (col.DataType == typeof(byte[]))
                            sqliteType = "BLOB";

                        // 이미 추가한 ID 컬럼은 건너뛰기
                        if (col.ColumnName.ToLower() != "id" &&
                            col.ColumnName.ToLower() != "raw_data_id")
                        {
                            columns.Add($"{col.ColumnName} {sqliteType}");
                        }
                    }

                    createTableQuery.AppendLine(string.Join(",\n", columns));
                    createTableQuery.AppendLine(");");

                    DBManager.Instance.ExecuteNonQuery(createTableQuery.ToString());
                    DBManager.Instance.ExecuteNonQuery("PRAGMA journal_mode = MEMORY");
                    DBManager.Instance.ExecuteNonQuery("PRAGMA synchronous = OFF");

                    // 인덱스 생성
                    DBManager.Instance.ExecuteNonQuery("CREATE INDEX idx_process_view_data_raw_id ON process_view_data(raw_data_id)");
                });

                int totalRows = dataTable.Rows.Count;
                int processedRows = 0;
                var processorCount = Environment.ProcessorCount;
                int optimalThreads = Math.Max(2, Math.Min(processorCount - 1, 10));
                int rowsPerThread = (int)Math.Ceiling(totalRows / (double)optimalThreads);

                Debug.WriteLine($"병렬 처리 시작: {optimalThreads}개 스레드, 스레드당 {rowsPerThread}행");
                await progress(30, "데이터 삽입 준비 중...");

                // 데이터 테이블에 raw_data_id 컬럼이 없다면 추가
                /*
               if (!dataTable.Columns.Contains("raw_data_id"))
                   dataTable.Columns.Add("raw_data_id", typeof(int));

               // 각 행에 ID 값 설정

               for (int i = 0; i < dataTable.Rows.Count; i++)
               {
                   DataRow row = dataTable.Rows[i];
                   if (row["raw_data_id"] == DBNull.Value)
                   {
                       // 임의로 행 인덱스 + 1을 설정
                       row["raw_data_id"] = i + 1;
                   }
               }
               */

                // 모든 컬럼 파라미터 생성 (ID 컬럼 제외)
                var columnNames = dataTable.Columns.Cast<DataColumn>()
                    .Where(c => c.ColumnName.ToLower() != "id")
                    .Select(c => c.ColumnName);

                var paramNames = columnNames.Select(name => $"@{name}");

                // INSERT 쿼리에 명시적으로 컬럼명 포함
                string insertQuery = $@"
            INSERT INTO process_view_data (
                {string.Join(", ", columnNames)}
            ) 
            VALUES (
                {string.Join(", ", paramNames)}
            )";

                var completedThreads = 0;
                var tasks = new List<Task>();
                var lockObj = new object();

                using (var transaction = DBManager.Instance.BeginTransaction())
                {
                    try
                    {
                        for (int t = 0; t < optimalThreads; t++)
                        {
                            int startRow = t * rowsPerThread;
                            int endRow = Math.Min(startRow + rowsPerThread, totalRows);
                            int threadId = t;

                            var task = Task.Run(async () =>
                            {
                                var rowsProcessed = 0;
                                for (int rowIdx = startRow; rowIdx < endRow; rowIdx++)
                                {
                                    if (rowIdx >= dataTable.Rows.Count) break;

                                    var rowData = new Dictionary<string, object>();
                                    foreach (DataColumn column in dataTable.Columns)
                                    {
                                        // id 컬럼 제외
                                        if (column.ColumnName.ToLower() != "id")
                                        {
                                            rowData[column.ColumnName] = dataTable.Rows[rowIdx][column] ?? DBNull.Value;
                                        }
                                    }

                                    DBManager.Instance.ExecuteNonQuery(insertQuery, rowData);
                                    rowsProcessed++;

                                    if (rowsProcessed % 1000 == 0)
                                    {
                                        lock (lockObj)
                                        {
                                            processedRows += 1000;
                                            var percentage = (int)(30 + (processedRows * 20.0 / totalRows));
                                            progress(percentage, $"데이터 처리 중... ({processedRows}/{totalRows} 행)");
                                        }
                                    }
                                }

                                lock (lockObj)
                                {
                                    completedThreads++;
                                    Debug.WriteLine($"스레드 {threadId} 완료. 완료된 스레드: {completedThreads}/{optimalThreads}");
                                }
                            });

                            tasks.Add(task);
                        }

                        await Task.WhenAll(tasks);
                        await progress(50, "트랜잭션 커밋 중...");
                        transaction.Commit();

                        await progress(50, "처리 완료");
                        Debug.WriteLine("process_view_data 테이블 생성 완료");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.WriteLine($"데이터 저장 중 오류 발생: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"process_view_data 테이블 생성 오류: {ex.Message}");
                throw;
            }
        }






        // AI 관련 설정
        int ai_limit_cnt = 4;

        private void ai_limit_count_ValueChanged(object sender, EventArgs e)
        {
            ai_limit_cnt = (int)ai_limit_count.Value;
            Debug.WriteLine($"ai_limit_cnt : {ai_limit_cnt}");
        }

        // 구분자 및 불용어 관리 관련 메서드
        private void new_seper_word_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                add_seperate_keyword();
                // Enter 키가 다른 동작을 막도록 처리
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void seper_add_btn_Click(object sender, EventArgs e)
        {
            add_seperate_keyword();
        }

        private void add_seperate_keyword()
        {
            // TextBox에 입력된 텍스트를 가져옴
            string inputText = new_seper_word.Text.Trim();

            // 텍스트가 비어있지 않은 경우 ListBox에 추가
            if (!string.IsNullOrEmpty(inputText))
            {
                //DataHandler.separator.Add(inputText);
                DataHandler.spManager.AddSeparator(inputText);
                new_seper_word.Clear(); // TextBox 초기화

                Debug.WriteLine($"_separatorManager.getSeparators() : {DataHandler.spManager.getSeparators()}");
                Debug.WriteLine($"_separatorManager : {string.Join(",", DataHandler.spManager.Separators)}");
            }

            List<string> seper_list = DataHandler.spManager.Separators
           .Distinct()  // 중복 제거
           .ToList();   // List로 변환

            //불용어 리스트 추가
            create_seperate_table(dataGridView_seperator, seper_list);
        }

        private void tb_remove_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                add_remove_keyword();

                // Enter 키가 다른 동작을 막도록 처리
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void remove_add_btn_Click(object sender, EventArgs e)
        {
            add_remove_keyword();
        }

        private void add_remove_keyword()
        {
            // TextBox에 입력된 텍스트를 가져옴
            string inputText = new_remove_word.Text.Trim();

            // 텍스트가 비어있지 않은 경우 ListBox에 추가
            if (!string.IsNullOrEmpty(inputText))
            {
                //DataHandler.remover.Add(inputText);
                DataHandler.spManager.AddRemover(inputText);
                new_remove_word.Clear(); // TextBox 초기화
            }

            Debug.WriteLine($"_separatorManager.getRemover() : {DataHandler.spManager.getRemover()}");

            List<string> remove_list = DataHandler.spManager.Removers
           .Distinct()  // 중복 제거
           .ToList();   // List로 변환

            //불용어 리스트 추가
            create_seperate_table(dataGridView_remove, remove_list);
        }

        private void seper_list_allcheck_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView_seperator.Rows)
            {
                row.Cells[0].Value = seper_list_allcheck.Checked;
            }
        }

        private void remove_list_allcheck_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView_remove.Rows)
            {
                row.Cells[0].Value = remove_list_allcheck.Checked;
            }
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

        private void seper_del_btn_Click(object sender, EventArgs e)
        {
            List<string> seper_del_list = GetCheckedRowsData(dataGridView_seperator);

            if (seper_del_list.Count == 0)
            {
                MessageBox.Show("구분자 변환 제거 대상을 선택하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }

            foreach (string seperate in seper_del_list)
            {
                //_separatorManager.Separators.Remove(seperate);
                DataHandler.spManager.RemoveSeparator(seperate);
            }

            for (int i = dataGridView_seperator.Rows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = dataGridView_seperator.Rows[i];

                // columnListDgv의 두 번째 컬럼(체크박스 다음)의 값 확인
                string seperData = row.Cells[1].Value?.ToString();
                if (seper_del_list.Contains(seperData))
                {
                    dataGridView_seperator.Rows.RemoveAt(i);
                }
            }
        }

        private void remove_del_btn_Click(object sender, EventArgs e)
        {
            List<string> remove_del_list = GetCheckedRowsData(dataGridView_remove);

            if (remove_del_list.Count == 0)
            {
                MessageBox.Show("불용어 항목 제거 대상을 선택하셔야 합니다.", "알림",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);

                return;
            }

            foreach (string remove in remove_del_list)
            {
                //DataHandler.remover.Remove(remove);
                DataHandler.spManager.RemoveRemover(remove);
            }

            for (int i = dataGridView_remove.Rows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = dataGridView_remove.Rows[i];

                // columnListDgv의 두 번째 컬럼(체크박스 다음)의 값 확인
                string removeData = row.Cells[1].Value?.ToString();
                if (remove_del_list.Contains(removeData))
                {
                    dataGridView_remove.Rows.RemoveAt(i);
                }
            }
        }
    }
}