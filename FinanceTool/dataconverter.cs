using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Math;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Presentation;

namespace FinanceTool
{
    /// <summary>
    /// Excel 데이터와 SQLite 데이터 간의 변환을 담당하는 클래스
    /// </summary>
    public class DataConverter
    {
        private const string RAW_TABLE = "raw_data";
        private const string PROCESS_TABLE = "process_data";
        private const string IMPORT_DATE_COLUMN = "import_date";

        /*
        // Excel 파일을 SQLite 데이터베이스로 변환
        public async Task<DataTable> ConvertExcelToSQLiteAsync(string filePath, IProgress<int> progress = null)
        {
            try
            {
                // DBManager 초기화 확인
                if (!DBManager.Instance.EnsureInitialized())
                {
                    throw new InvalidOperationException("데이터베이스 초기화에 실패했습니다.");
                }

                Stopwatch sw = Stopwatch.StartNew();
                Debug.WriteLine($"Excel 파일 로드 시작: {filePath}");

                // 병렬 처리로 Excel 데이터 로드
                DataTable excelData = await ParallelExcelProcessing(filePath, progress);

                sw.Stop();
                Debug.WriteLine($"Excel → SQLite 변환 완료. 소요 시간: {sw.ElapsedMilliseconds}ms, 행 수: {excelData.Rows.Count}");

                
                // 완료 진행률 업데이트
                progress?.Report(100);

                return excelData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excel → SQLite 변환 오류: {ex.Message}");
                throw;
            }
        }
        */

        public async Task<DataTable> ConvertExcelToSQLiteAsync(string filePath, ProcessProgressForm.UpdateProgressDelegate progress)
        {
            try
            {
                // DBManager 초기화 확인
                if (!DBManager.Instance.EnsureInitialized())
                {
                    throw new InvalidOperationException("데이터베이스 초기화에 실패했습니다.");
                }

                Stopwatch sw = Stopwatch.StartNew();
                Debug.WriteLine($"Excel 파일 로드 시작: {filePath}");

                // 병렬 처리로 Excel 데이터 로드
                DataTable excelData = await ParallelExcelProcessing(filePath, progress);

                sw.Stop();
                Debug.WriteLine($"Excel → SQLite 변환 완료. 소요 시간: {sw.ElapsedMilliseconds}ms, 행 수: {excelData.Rows.Count}");


                
                //progress(100);

                return excelData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excel → SQLite 변환 오류: {ex.Message}");
                throw;
            }
        }

        /*
        private async Task<DataTable> ParallelExcelProcessing(string filePath, IProgress<int> progress = null)
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheets.First();
                var range = worksheet.RangeUsed();
                var totalRows = range.RowCount();

                progress?.Report(10); // 10% 진행

                await Task.Delay(100);

                // 원본 데이터 로드
                DataTable excelData = range.AsTable().AsNativeDataTable();
                NormalizeColumnNames(excelData);
                AddMetadataColumns(excelData);

                progress?.Report(20); // 20% 진행

                // raw_data 테이블 생성
                CreateRawDataTable(excelData);
                SaveColumnMappingInfo(excelData);

                progress?.Report(30); // 30% 진행

                // SQLite 최적화 설정 적용
                OptimizeSQLiteForBulkInsert();

                // 데이터 분할 처리
                int processorCount = Environment.ProcessorCount;
                //int optimalThreads = Math.Max(2, Math.Min(processorCount - 1, 6)); // 최소 2개, 최대 6개 스레드
                int optimalThreads = Math.Max(2, Math.Min(processorCount - 1, 10)); // 최소 2개, 최대 10개 스레드
                int rowsPerThread = (int)Math.Ceiling(totalRows / (double)optimalThreads);

                Debug.WriteLine($"병렬 처리 시작: {optimalThreads}개 스레드, 스레드당 {rowsPerThread}행");

                List<Task> tasks = new List<Task>();
                List<List<Dictionary<string, object>>> allBatches = new List<List<Dictionary<string, object>>>();

                // 각 스레드별 데이터 준비
                for (int t = 0; t < optimalThreads; t++)
                {
                    int startRow = t * rowsPerThread;
                    int endRow = Math.Min(startRow + rowsPerThread, totalRows);

                    var threadBatches = new List<Dictionary<string, object>>();
                    allBatches.Add(threadBatches);

                    for (int rowIdx = startRow; rowIdx < endRow; rowIdx++)
                    {
                        if (rowIdx >= excelData.Rows.Count) continue;

                        Dictionary<string, object> rowData = new Dictionary<string, object>();
                        foreach (DataColumn column in excelData.Columns)
                        {
                            rowData[column.ColumnName] = excelData.Rows[rowIdx][column];
                        }
                        threadBatches.Add(rowData);
                    }
                }

                progress?.Report(40); // 40% 진행

                // 트랜잭션 시작
                using (var transaction = DBManager.Instance.BeginTransaction())
                {
                    try
                    {
                        // 병렬로 각 배치 처리
                        for (int t = 0; t < allBatches.Count; t++)
                        {
                            int threadId = t;
                            var threadData = allBatches[t];

                            // 배치 처리 작업 생성
                            tasks.Add(Task.Run(() => {
                                ProcessThreadData(threadData, threadId, optimalThreads, progress);
                            }));
                        }

                        // 모든 작업 완료 대기
                        await Task.WhenAll(tasks);

                        // 진행 상황 업데이트
                        progress?.Report(80); // 80% 진행

                        // 트랜잭션 커밋
                        transaction.Commit();

                        // 인덱스 생성 (트랜잭션 외부에서)
                        CreateIndicesForRawData();

                        progress?.Report(90); // 90% 진행
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                // 결과 데이터 반환
                return GetRawDataFromSQLite();
            }
        }
        */

        private async Task<DataTable> ParallelExcelProcessing(string filePath, ProcessProgressForm.UpdateProgressDelegate progress)
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheets.First();
                var range = worksheet.RangeUsed();
                var totalRows = range.RowCount();
                
                await Task.Delay(10);

                await progress(10); // 10% 진행

                await Task.Delay(10);

                // 원본 데이터 로드
                DataTable excelData = range.AsTable().AsNativeDataTable();
                NormalizeColumnNames(excelData);
                AddMetadataColumns(excelData);

                await progress(20); // 20% 진행

                await Task.Delay(10);

                // raw_data 테이블 생성
                CreateRawDataTable(excelData);
                SaveColumnMappingInfo(excelData);

                await progress(30); // 30% 진행


                await Task.Delay(10);

                // SQLite 최적화 설정 적용
                OptimizeSQLiteForBulkInsert();

                // 데이터 분할 처리
                int processorCount = Environment.ProcessorCount;
                //int optimalThreads = Math.Max(2, Math.Min(processorCount - 1, 6)); // 최소 2개, 최대 6개 스레드
                int optimalThreads = Math.Max(2, Math.Min(processorCount - 1, 10)); // 최소 2개, 최대 10개 스레드
                int rowsPerThread = (int)Math.Ceiling(totalRows / (double)optimalThreads);

                Debug.WriteLine($"병렬 처리 시작: {optimalThreads}개 스레드, 스레드당 {rowsPerThread}행");

                List<Task> tasks = new List<Task>();
                List<List<Dictionary<string, object>>> allBatches = new List<List<Dictionary<string, object>>>();

                // 각 스레드별 데이터 준비
                for (int t = 0; t < optimalThreads; t++)
                {
                    int startRow = t * rowsPerThread;
                    int endRow = Math.Min(startRow + rowsPerThread, totalRows);

                    var threadBatches = new List<Dictionary<string, object>>();
                    allBatches.Add(threadBatches);

                    for (int rowIdx = startRow; rowIdx < endRow; rowIdx++)
                    {
                        if (rowIdx >= excelData.Rows.Count) continue;

                        Dictionary<string, object> rowData = new Dictionary<string, object>();
                        foreach (DataColumn column in excelData.Columns)
                        {
                            rowData[column.ColumnName] = excelData.Rows[rowIdx][column];
                        }
                        threadBatches.Add(rowData);
                    }
                }
                await Task.Delay(10);

                await progress(40); // 40% 진행

                await Task.Delay(10);

                // 트랜잭션 시작
                using (var transaction = DBManager.Instance.BeginTransaction())
                {
                    try
                    {
                        // 병렬로 각 배치 처리
                        for (int t = 0; t < allBatches.Count; t++)
                        {
                            int threadId = t;
                            var threadData = allBatches[t];

                            // 배치 처리 작업 생성
                            tasks.Add(Task.Run(() => {
                                ProcessThreadData(threadData, threadId, optimalThreads, progress);
                            }));
                        }

                        // 모든 작업 완료 대기
                        await Task.WhenAll(tasks);

                        await Task.Delay(10);

                        // 진행 상황 업데이트
                        await progress(80); // 80% 진행

                        await Task.Delay(10);

                        // 트랜잭션 커밋
                        transaction.Commit();

                        // 인덱스 생성 (트랜잭션 외부에서)
                        CreateIndicesForRawData();

                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                // 결과 데이터 반환
                return GetRawDataFromSQLite();
            }
        }

        // DataConverter 클래스에 추가할 InsertBatch 메서드
        private void InsertBatch(string tableName, List<Dictionary<string, object>> rows)
        {
            if (rows.Count == 0) return;

            DBManager dbManager = DBManager.Instance;

            try
            {
                // 첫 번째 행의 컬럼 목록 가져오기
                var firstRow = rows[0];
                var columns = firstRow.Keys.ToList();

                // INSERT 쿼리 생성
                StringBuilder queryBuilder = new StringBuilder();
                queryBuilder.Append($"INSERT INTO {tableName} (");
                queryBuilder.Append(string.Join(", ", columns));
                queryBuilder.Append(") VALUES ");

                List<string> valuesSets = new List<string>();
                Dictionary<string, object> allParameters = new Dictionary<string, object>();

                // 각 행에 대한 파라미터 생성
                for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    var row = rows[rowIndex];
                    List<string> parameterNames = new List<string>();

                    foreach (var column in columns)
                    {
                        string paramName = $"{column}_{rowIndex}";
                        parameterNames.Add($"@{paramName}");
                        allParameters[paramName] = row.ContainsKey(column) ? row[column] : DBNull.Value;
                    }

                    valuesSets.Add($"({string.Join(", ", parameterNames)})");
                }

                queryBuilder.Append(string.Join(", ", valuesSets));

                // 쿼리 실행
                dbManager.ExecuteNonQuery(queryBuilder.ToString(), allParameters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"배치 삽입 중 오류: {ex.Message}");
                throw;
            }
        }

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private async Task ProcessThreadData(List<Dictionary<string, object>> threadData, int threadId, int totalThreads, ProcessProgressForm.UpdateProgressDelegate progress)
        {
            try
            {
                int batchSize = 1000;
                int totalBatches = (int)Math.Ceiling(threadData.Count / (double)batchSize);
                int processedBatches = 0;
                double processStack = Math.Round((double)40 /totalBatches,2);

                Debug.WriteLine($"processStack : {processStack},  totalBatches : {totalBatches}, 40 /totalBatches : {(double)40 / totalBatches}, Math.Round((double)(40 /totalBatches),0) : {Math.Round((double)40 / totalBatches, 2)} ");

                for (int batchIdx = 0; batchIdx < totalBatches; batchIdx++)
                {
                    int startIdx = batchIdx * batchSize;
                    int count = Math.Min(batchSize, threadData.Count - startIdx);
                    if (count <= 0) continue;

                    var batch = threadData.GetRange(startIdx, count);
                    InsertBatch(RAW_TABLE, batch);

                    await _semaphore.WaitAsync();
                    try
                    {
                        processedBatches++;
                        //int progressValue = 40 + (int)((processedBatches / (double)(totalThreads * totalBatches)) * 40);
                        //int progressValue = 40 + (int)(batchIdx + 1 / (totalBatches * totalThreads)) * 40;
                        int progressValue = 40 + (int)((batchIdx + 1 / totalBatches ) * processStack);
                        Debug.WriteLine($"스레드 {threadId}: 배치 {batchIdx + 1}/{totalBatches} 완료 add percent : {(int)(batchIdx + 1 / totalBatches )}, Progress: {progressValue}%");

                        if (threadId == 0)
                        {                            
                            //await progress(progressValue, $"데이터 처리 중... (스레드 {threadId}, 배치 {batchIdx + 1}/{totalBatches})");
                            //await progress(progressValue, $"데이터 처리 중... ( {(int)(40 + 10 * (batchIdx + 1)/totalBatches)}% )");
                            await progress(progressValue, $"데이터 업로드 중... ({progressValue}%)");
                        }
                        
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"스레드 {threadId} 처리 오류: {ex.Message}");
                throw;
            }
        }

        // DataConverter.cs에 추가
        private void OptimizeSQLiteForBulkInsert()
        {
            DBManager dbManager = DBManager.Instance;

            // 대량 삽입을 위한 SQLite 최적화
            dbManager.ExecuteNonQuery("PRAGMA journal_mode = WAL");            // Write-Ahead Logging 모드
            dbManager.ExecuteNonQuery("PRAGMA synchronous = NORMAL");          // 동기화 수준 낮추기
            dbManager.ExecuteNonQuery("PRAGMA cache_size = 10000");            // 캐시 크기 증가
            dbManager.ExecuteNonQuery("PRAGMA temp_store = MEMORY");           // 임시 저장소를 메모리로
            dbManager.ExecuteNonQuery("PRAGMA page_size = 8192");              // 페이지 크기 증가
            dbManager.ExecuteNonQuery("PRAGMA mmap_size = 30000000000");       // 메모리 맵 크기 설정
            dbManager.ExecuteNonQuery("PRAGMA locking_mode = EXCLUSIVE");      // 독점 잠금

            // 삽입 전 인덱스 제거
            if (dbManager.TableExists(RAW_TABLE))
            {
                dbManager.ExecuteNonQuery($"DROP INDEX IF EXISTS idx_{RAW_TABLE}_import_date");
            }

            Debug.WriteLine("SQLite 성능 최적화 설정 완료");
        }

        // Excel 파일 로드
        private DataTable LoadExcelFile(string filePath)
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheets.First();
                var range = worksheet.RangeUsed();

                // 열 이름 정규화
                DataTable result = range.AsTable().AsNativeDataTable();
                NormalizeColumnNames(result);

                return result;
            }
        }

        // 열 이름 정규화 (특수문자 제거, 공백 대체 등)
        private void NormalizeColumnNames(DataTable dataTable)
        {
            List<string> originalColumns = new List<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                originalColumns.Add(column.ColumnName);
            }

            foreach (string originalName in originalColumns)
            {
                // 공백, 특수문자 처리
                string normalizedName = originalName
                    .Replace(" ", "_")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace(".", "_")
                    .Replace("/", "_")
                    .Replace("\\", "_")
                    .Replace("?", "")
                    .Replace("!", "")
                    .Replace("@", "")
                    .Replace("#", "")
                    .Replace("$", "")
                    .Replace("%", "")
                    .Replace("^", "")
                    .Replace("&", "")
                    .Replace("*", "")
                    .Replace("-", "_")
                    .Replace("+", "_plus")
                    .Replace("=", "_equals");

                // 컬럼명이 숫자로 시작하면 접두사 추가
                if (normalizedName.Length > 0 && char.IsDigit(normalizedName[0]))
                {
                    normalizedName = "col_" + normalizedName;
                }

                // 이름이 변경된 경우에만 처리
                if (normalizedName != originalName)
                {
                    // 이미 같은 이름의 컬럼이 있는지 확인하고 중복 방지
                    if (dataTable.Columns.Contains(normalizedName))
                    {
                        int suffix = 1;
                        string tempName = normalizedName;
                        while (dataTable.Columns.Contains(tempName))
                        {
                            tempName = $"{normalizedName}_{suffix}";
                            suffix++;
                        }
                        normalizedName = tempName;
                    }

                    dataTable.Columns[originalName].ColumnName = normalizedName;
                }
            }
        }

        // 메타데이터 컬럼 추가
        private void AddMetadataColumns(DataTable dataTable)
        {
            // 가져온 날짜/시간 컬럼 추가
            if (!dataTable.Columns.Contains(IMPORT_DATE_COLUMN))
            {
                dataTable.Columns.Add(IMPORT_DATE_COLUMN, typeof(DateTime)).DefaultValue = DateTime.Now;
            }

            // 값이 없는 셀은 DBNull로 설정
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    if (row[col] is string strValue && string.IsNullOrWhiteSpace(strValue))
                    {
                        row[col] = DBNull.Value;
                    }
                }

                // 임포트 날짜 설정
                if (row[IMPORT_DATE_COLUMN] == null || row[IMPORT_DATE_COLUMN] == DBNull.Value)
                {
                    row[IMPORT_DATE_COLUMN] = DateTime.Now;
                }
            }
        }

        // 컬럼 매핑 정보 저장
        private void SaveColumnMappingInfo(DataTable dataTable)
        {
            try
            {
                DBManager dbManager = DBManager.Instance;

                // 기존 매핑 정보 초기화
                dbManager.ExecuteNonQuery("DELETE FROM column_mapping");

                // 새 매핑 정보 저장
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    DataColumn column = dataTable.Columns[i];
                    string columnType = MapDotNetTypeToString(column.DataType);

                    Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                        { "original_name", column.ColumnName },
                        { "display_name", column.ColumnName }, // 초기값은 원본 이름과 동일
                        { "data_type", columnType },
                        { "is_visible", 1 },
                        { "sequence", i }
                    };

                    dbManager.ExecuteNonQuery(@"
                        INSERT INTO column_mapping 
                        (original_name, display_name, data_type, is_visible, sequence)
                        VALUES 
                        (@original_name, @display_name, @data_type, @is_visible, @sequence)",
                        parameters);
                }

                Debug.WriteLine($"{dataTable.Columns.Count}개 컬럼 매핑 정보 저장 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"컬럼 매핑 정보 저장 중 오류: {ex.Message}");
                throw;
            }
        }

        // .NET 데이터 타입을 문자열로 변환
        private string MapDotNetTypeToString(Type dotNetType)
        {
            if (dotNetType == typeof(int) || dotNetType == typeof(long) ||
                dotNetType == typeof(short) || dotNetType == typeof(byte))
                return "integer";
            else if (dotNetType == typeof(bool))
                return "boolean";
            else if (dotNetType == typeof(float) || dotNetType == typeof(double) ||
                     dotNetType == typeof(decimal))
                return "decimal";
            else if (dotNetType == typeof(DateTime))
                return "datetime";
            else if (dotNetType == typeof(byte[]))
                return "blob";
            else
                return "text";
        }


        // raw_data 테이블 생성
        private void CreateRawDataTable(DataTable dataTable)
        {
            DBManager dbManager = DBManager.Instance;

            // 기존 테이블 삭제
            dbManager.DropTableIfExists(RAW_TABLE);

            // 새 테이블 생성 쿼리 구성
            StringBuilder createTableQuery = new StringBuilder();
            createTableQuery.AppendLine($"CREATE TABLE {RAW_TABLE} (");
            createTableQuery.AppendLine("  id INTEGER PRIMARY KEY AUTOINCREMENT,");

            List<string> columnDefs = new List<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                string sqliteType = MapColumnTypeToSQLite(column);
                columnDefs.Add($"  {column.ColumnName} {sqliteType}");
            }

            createTableQuery.AppendLine(string.Join(",\n", columnDefs));
            createTableQuery.AppendLine(")");

            // 테이블 생성
            dbManager.ExecuteNonQuery(createTableQuery.ToString());
            Debug.WriteLine($"{RAW_TABLE} 테이블 생성 완료");
        }

        // DataColumn 타입을 SQLite 타입으로 변환
        private string MapColumnTypeToSQLite(DataColumn column)
        {
            Type columnType = column.DataType;

            if (columnType == typeof(int) || columnType == typeof(long) ||
                columnType == typeof(short) || columnType == typeof(byte) ||
                columnType == typeof(bool))
                return "INTEGER";
            else if (columnType == typeof(float) || columnType == typeof(double) ||
                     columnType == typeof(decimal))
                return "REAL";
            else if (columnType == typeof(DateTime))
                return "DATETIME";
            else if (columnType == typeof(byte[]))
                return "BLOB";
            else
                return "TEXT";
        }

        // 배치 삽입 처리
        private async Task InsertDataBatchAsync(string tableName, List<Dictionary<string, object>> allRows, IProgress<int> progress = null)
        {
            if (allRows.Count == 0) return;

            int totalRows = allRows.Count;
            int batchSize = 1000;
            int processedRows = 0;

            using (var dbManager = DBManager.Instance)
            using (var transaction = dbManager.BeginTransaction())
            {
                try
                {
                    for (int batchStart = 0; batchStart < totalRows; batchStart += batchSize)
                    {
                        int currentBatchSize = Math.Min(batchSize, totalRows - batchStart);
                        var batch = allRows.GetRange(batchStart, currentBatchSize);

                        await Task.Run(() => {
                            InsertBatch(tableName, batch, dbManager);
                        });

                        processedRows += currentBatchSize;

                        // 진행률 업데이트 (70%~90%)
                        if (progress != null)
                        {
                            int progressValue = 70 + (int)((processedRows / (double)totalRows) * 20);
                            progress.Report(progressValue);
                        }
                    }

                    transaction.Commit();
                    Debug.WriteLine($"{processedRows}개 행이 {tableName} 테이블에 삽입됨");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Debug.WriteLine($"배치 삽입 오류: {ex.Message}");
                    throw;
                }
            }
        }

        // 단일 배치 삽입
        private void InsertBatch(string tableName, List<Dictionary<string, object>> rows, DBManager dbManager)
        {
            if (rows.Count == 0) return;

            // 첫 번째 행에서 컬럼 목록 가져오기
            List<string> columns = rows[0].Keys.ToList();

            // INSERT 쿼리 생성
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append($"INSERT INTO {tableName} (");
            queryBuilder.Append(string.Join(", ", columns));
            queryBuilder.Append(") VALUES ");

            List<string> valuesSets = new List<string>();
            Dictionary<string, object> allParameters = new Dictionary<string, object>();

            // 각 행에 대한 파라미터 생성
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                List<string> parameterNames = new List<string>();

                foreach (var column in columns)
                {
                    string paramName = $"{column}_{rowIndex}";
                    parameterNames.Add($"@{paramName}");
                    allParameters[paramName] = row.ContainsKey(column) ? row[column] : DBNull.Value;
                }

                valuesSets.Add($"({string.Join(", ", parameterNames)})");
            }

            queryBuilder.Append(string.Join(", ", valuesSets));

            // 쿼리 실행
            dbManager.ExecuteNonQuery(queryBuilder.ToString(), allParameters);
        }

        // raw_data 테이블에 인덱스 생성
        private void CreateIndicesForRawData()
        {
            DBManager dbManager = DBManager.Instance;

            // 자주 검색하는 컬럼에 인덱스 생성
            dbManager.ExecuteNonQuery($"CREATE INDEX IF NOT EXISTS idx_{RAW_TABLE}_import_date ON {RAW_TABLE} ({IMPORT_DATE_COLUMN})");

            // 추가 인덱스는 필요에 따라 생성
            Debug.WriteLine("raw_data 테이블 인덱스 생성 완료");
        }

        // SQLite에서 원본 데이터 조회
        public DataTable GetRawDataFromSQLite(int limit = 1000)
        {
            DBManager dbManager = DBManager.Instance;
            return dbManager.ExecuteQuery($"SELECT * FROM {RAW_TABLE} LIMIT {limit}");
        }

        // 페이징된 원본 데이터 조회
        public DataTable GetPagedRawData(int pageNumber, int pageSize , bool hiddenTableYN=false)
        {
            DBManager dbManager = DBManager.Instance;
            string baseQuery = $"SELECT * FROM {RAW_TABLE}";
            //hidden_row 존재할 경우
            if (hiddenTableYN)
            {
                baseQuery = $"SELECT r.*, CASE WHEN h.row_id IS NOT NULL THEN 0 ELSE 1  END AS hiddenYN"
                            + $" FROM { RAW_TABLE} r LEFT JOIN  hidden_rows h"
                            + $" ON  r.id = h.row_id AND h.original_table = '{RAW_TABLE}'";
            }
            //default
            else
            {
                baseQuery = $"SELECT *,1 as hiddenYN FROM {RAW_TABLE}";
            }
            
            string countQuery = $"SELECT COUNT(*) FROM {RAW_TABLE}";

            return dbManager.ExecutePagedQuery(
                baseQuery,
                countQuery,
                pageNumber,
                pageSize);
        }

        public DataTable GetPagedProcessData(int pageNumber, int pageSize,List<string> columnList, bool hiddenTableYN = false)
        {
            try
            {
                // 클러스터 매핑 임시 테이블 생성
                CreateTempClusterMappingTable();

                DBManager dbManager = DBManager.Instance;

                // 컬럼 목록 문자열 생성 (r.id는 항상 포함)
                string columnSelection = "r.id";
                foreach (string column in columnList)
                {
                    columnSelection += $", r.{column}";
                }
                Debug.WriteLine($" columnSelection: {columnSelection}");

                // 클러스터명 컬럼 추가
                columnSelection += ", IFNULL(cm.cluster_name, '') AS 클러스터명";

                string baseQuery;
                // hidden_row 존재할 경우
                if (hiddenTableYN)
                {
                    baseQuery = $"SELECT {columnSelection}, CASE WHEN h.row_id IS NOT NULL THEN 0 ELSE 1 END AS hiddenYN"
                              + $" FROM {RAW_TABLE} r"
                              + $" LEFT JOIN temp_cluster_mapping cm ON r.id = cm.raw_data_id"
                              + $" LEFT JOIN hidden_rows h ON r.id = h.row_id AND h.original_table = '{RAW_TABLE}'";
                }
                // default
                else
                {
                    baseQuery = $"SELECT {columnSelection}, 1 as hiddenYN"
                              + $" FROM {RAW_TABLE} r"
                              + $" LEFT JOIN temp_cluster_mapping cm ON r.id = cm.raw_data_id";
                }

                string countQuery = $"SELECT COUNT(*) FROM {RAW_TABLE}";

                // 페이징된 쿼리 실행
                DataTable result = dbManager.ExecutePagedQuery(
                    baseQuery,
                    countQuery,
                    pageNumber,
                    pageSize);

                return result;
            }
            finally
            {
                // 임시 테이블 삭제
                DropTempClusterMappingTable();
            }
        }

        public async Task<DataTable> GetAllRawDataWithClustersAsync(
        List<string> columnList,
        bool hiddenTableYN = false,
        int maxThreads = 4,
        int totalRecords = 0,
        int initialProgress = 5,
        int maxProgress = 80,
        ProcessProgressForm.UpdateProgressDelegate progressHandler = null)
        {
            try
            {
                // 프로그레스 핸들러가 제공되지 않은 경우 새 폼 생성
                ProcessProgressForm progressForm = null;
                ProcessProgressForm.UpdateProgressDelegate updateProgress;

                if (progressHandler == null)
                {
                    progressForm = new ProcessProgressForm();
                    progressForm.Show();
                    await Task.Delay(10);
                    updateProgress = progressForm.UpdateProgressHandler;
                }
                else
                {
                    // 외부에서 제공된 핸들러 사용
                    updateProgress = progressHandler;
                }

                await updateProgress(initialProgress, "데이터 조회 준비 중...");

                // 클러스터 매핑 테이블 먼저 생성
                await updateProgress(initialProgress + 3, "클러스터 매핑 정보 준비 중...");

                // 임시 클러스터 매핑 테이블 생성 및 데이터 추가
                CreateTempClusterMappingTable();

                // 컬럼 목록 문자열 생성
                string columnSelection = "r.id";
                foreach (string column in columnList)
                {
                    columnSelection += $", r.{column}";
                }

                // 클러스터명 컬럼 추가
                columnSelection += ", IFNULL(cm.cluster_name, '') AS 클러스터명";

                string baseQuery;
                // hidden_row 존재할 경우
                if (hiddenTableYN)
                {
                    baseQuery = $"SELECT {columnSelection} "
                              + $" FROM {RAW_TABLE} r "
                              + $" LEFT JOIN temp_cluster_mapping cm ON r.id = cm.raw_data_id "
                              + $" LEFT JOIN hidden_rows h ON r.id = h.row_id AND h.original_table = '{RAW_TABLE}'"
                              + $" WHERE h.row_id IS NULL"; // 숨김 처리된 행은 제외
                }
                else
                {
                    baseQuery = $"SELECT {columnSelection} "
                              + $" FROM {RAW_TABLE} r "
                              + $" LEFT JOIN temp_cluster_mapping cm ON r.id = cm.raw_data_id";
                }

                // 전체 데이터 수 확인
                DBManager dbManager = DBManager.Instance;

                // totalRecords가 제공되지 않았으면 계산
                if (totalRecords <= 0)
                {
                    await updateProgress(initialProgress + 5, "전체 데이터 수 확인 중...");
                    string countQuery = hiddenTableYN
                        ? $"SELECT COUNT(*) FROM {RAW_TABLE} r WHERE NOT EXISTS "
                        + $"(SELECT 1 FROM hidden_rows h WHERE r.id = h.row_id AND h.original_table = '{RAW_TABLE}')"
                        : $"SELECT COUNT(*) FROM {RAW_TABLE}";

                    totalRecords = Convert.ToInt32(dbManager.ExecuteScalar(countQuery));
                }

                int progressAfterCount = initialProgress + 10;
                await updateProgress(progressAfterCount, $"총 {totalRecords}개 데이터 처리 준비 중...");

                // 병렬 처리 설정
                int processorCount = Environment.ProcessorCount;
                int optimalThreads = Math.Max(2, Math.Min(processorCount - 1, maxThreads));
                int recordsPerThread = (int)Math.Ceiling(totalRecords / (double)optimalThreads);

                Debug.WriteLine($"병렬 데이터 조회 시작: {optimalThreads}개 스레드, 스레드당 약 {recordsPerThread}행");

                int progressAfterSetup = progressAfterCount + 5;
                await updateProgress(progressAfterSetup, $"병렬 처리 준비 중 ({optimalThreads}개 스레드)...");

                // 결과를 저장할 DataTable 생성
                DataTable resultTable = null;
                List<Task<DataTable>> tasks = new List<Task<DataTable>>();

                // 스레드별 작업 생성
                for (int t = 0; t < optimalThreads; t++)
                {
                    int threadId = t;
                    int offset = t * recordsPerThread;
                    int limit = recordsPerThread;

                    // 마지막 스레드는 남은 모든 레코드를 처리
                    if (t == optimalThreads - 1)
                    {
                        limit = totalRecords - offset;
                    }

                    string threadQuery = $"{baseQuery} LIMIT {limit} OFFSET {offset}";
                    tasks.Add(Task.Run(() => FetchDataSegment(threadQuery, threadId, optimalThreads)));
                }

                // 진행 상황 업데이트를 위한 변수
                int progressRange = maxProgress - progressAfterSetup - 10; // 10은 데이터 병합 단계용
                int progressStep = Math.Max(1, progressRange / 30); // 30단계로 나누어 업데이트

                // 진행 상황 업데이트를 위한 별도 태스크
                _ = Task.Run(async () =>
                {
                    for (int i = 0; i < 30; i++)
                    {
                        int currentProgress = progressAfterSetup + (i * progressStep);
                        currentProgress = Math.Min(currentProgress, maxProgress - 10);

                        await Task.Delay(200); // 0.2초마다 업데이트
                        await updateProgress(currentProgress, $"데이터 조회 중... ({currentProgress}%)");
                    }
                });

                // 모든 작업 완료 대기
                await Task.WhenAll(tasks);

                Debug.WriteLine($"[excel Export] 모든 작업 완료");

                int progressAfterFetch = maxProgress - 10;
                await updateProgress(progressAfterFetch, "데이터 병합 중...");

                // 결과 병합
                resultTable = MergeDataTables(tasks.Select(t => t.Result).ToList());

                Debug.WriteLine($"[excel Export] 모든 데이터 병합 완료");

                // 임시 테이블 삭제
                DropTempClusterMappingTable();

                await updateProgress(maxProgress, $"데이터 처리 완료. 총 {resultTable.Rows.Count}개 행");
                await Task.Delay(100); // 진행 상황 메시지를 잠시 표시

                return resultTable;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"전체 데이터 조회 중 오류 발생: {ex.Message}");
                MessageBox.Show($"데이터 조회 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 임시 테이블이 남아있을 수 있으므로 삭제 시도
                try { DropTempClusterMappingTable(); } catch { }
                throw;
            }
        }

        private void CreateTempClusterMappingTable()
        {
            DBManager dbManager = DBManager.Instance;

            try
            {
                // 기존 임시 테이블이 있다면 삭제
                dbManager.ExecuteNonQuery("DROP TABLE IF EXISTS temp_cluster_mapping");

                // 임시 테이블 생성
                dbManager.ExecuteNonQuery(
                    "CREATE TEMPORARY TABLE temp_cluster_mapping (" +
                    "raw_data_id INTEGER, " +
                    "cluster_name TEXT" +
                    ")");

                // 클러스터 이름 맵 생성
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

                // 대량 삽입을 위한 트랜잭션 시작
                using (var transaction = dbManager.BeginTransaction())
                {
                    try
                    {
                        List<Dictionary<string, object>> batchRows = new List<Dictionary<string, object>>();

                        // 데이터 매핑 추가
                        foreach (DataRow clusterRow in DataHandler.finalClusteringData.Rows)
                        {
                            if (clusterRow["ClusterID"] == DBNull.Value) continue;

                            int clusterId = Convert.ToInt32(clusterRow["ClusterID"]);
                            string dataIndices = clusterRow["dataIndex"]?.ToString();

                            if (clusterNameMap.ContainsKey(clusterId) && !string.IsNullOrEmpty(dataIndices))
                            {
                                string clusterName = clusterNameMap[clusterId];
                                string[] indexStrings = dataIndices.Split(',');

                                foreach (string indexStr in indexStrings)
                                {
                                    if (int.TryParse(indexStr.Trim(), out int rawDataId))
                                    {
                                        Dictionary<string, object> rowData = new Dictionary<string, object>
                                {
                                    { "raw_data_id", rawDataId },
                                    { "cluster_name", clusterName }
                                };

                                        batchRows.Add(rowData);

                                        // 배치 크기가 1000개가 되면 삽입
                                        if (batchRows.Count >= 1000)
                                        {
                                            InsertBatch("temp_cluster_mapping", batchRows);
                                            batchRows.Clear();
                                        }
                                    }
                                }
                            }
                        }

                        // 남은 배치 처리
                        if (batchRows.Count > 0)
                        {
                            InsertBatch("temp_cluster_mapping", batchRows);
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                // 인덱스 생성으로 조인 성능 향상
                dbManager.ExecuteNonQuery("CREATE INDEX idx_temp_cluster_raw_data_id ON temp_cluster_mapping(raw_data_id)");

                Debug.WriteLine("임시 클러스터 매핑 테이블 생성 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"임시 클러스터 매핑 테이블 생성 오류: {ex.Message}");
                throw;
            }
        }

        private void DropTempClusterMappingTable()
        {
            DBManager dbManager = DBManager.Instance;
            dbManager.ExecuteNonQuery("DROP TABLE IF EXISTS temp_cluster_mapping");
            Debug.WriteLine("임시 클러스터 매핑 테이블 삭제 완료");
        }

        private DataTable FetchDataSegment(string query, int threadId, int totalThreads)
        {
            try
            {
                DBManager dbManager = DBManager.Instance;
                DataTable segment = dbManager.ExecuteQuery(query);
                Debug.WriteLine($"스레드 {threadId}: {segment.Rows.Count}개 행 조회 완료");
                return segment;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"스레드 {threadId} 데이터 조회 오류: {ex.Message}");
                throw;
            }
        }

        private DataTable MergeDataTables(List<DataTable> tables)
        {
            if (tables == null || tables.Count == 0)
                return new DataTable();

            DataTable result = tables[0].Clone();

            foreach (DataTable table in tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    result.ImportRow(row);
                }
            }

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

        // 프로세스 테이블 준비
        public void PrepareProcessTable(IEnumerable<string> selectedColumns)
        {
            DBManager dbManager = DBManager.Instance;
            // 기존 테이블 삭제
            dbManager.DropTableIfExists(PROCESS_TABLE);

            // 새 테이블 생성
            StringBuilder createTableQuery = new StringBuilder();
            createTableQuery.AppendLine($"CREATE TABLE {PROCESS_TABLE} (");
            createTableQuery.AppendLine("  id INTEGER PRIMARY KEY,");
            createTableQuery.AppendLine("  raw_data_id INTEGER,");  // 원본 raw_data 테이블의 ID 참조 컬럼 추가

            List<string> columnDefs = new List<string>();
            foreach (string column in selectedColumns)
            {
                // 원본 컬럼의 타입 정보 가져오기
                object dataType = dbManager.ExecuteScalar(
                    "SELECT data_type FROM column_mapping WHERE original_name = @column",
                    new Dictionary<string, object> { { "column", column } }
                );
                string sqliteType = "TEXT"; // 기본값
                if (dataType != null && dataType != DBNull.Value)
                {
                    sqliteType = MapStringTypeToSQLite(dataType.ToString());
                }
                columnDefs.Add($"  {column} {sqliteType}");
            }

            // 메타데이터 컬럼 추가
            columnDefs.Add($"  {IMPORT_DATE_COLUMN} DATETIME");
            columnDefs.Add("  processed_date DATETIME");
            createTableQuery.AppendLine(string.Join(",\n", columnDefs));
            createTableQuery.AppendLine(")");

            // 테이블 생성
            dbManager.ExecuteNonQuery(createTableQuery.ToString());

            //hiddenrow table 생성
            CreateHiddenRowsTable();

            // 데이터 복사 - raw_data_id로 원본 ID도 저장
            StringBuilder insertQuery = new StringBuilder();
            insertQuery.AppendLine($"INSERT INTO {PROCESS_TABLE} (");
            insertQuery.AppendLine("id, raw_data_id, " + string.Join(", ", selectedColumns) + $", {IMPORT_DATE_COLUMN}, processed_date");
            insertQuery.AppendLine(") SELECT ");
            insertQuery.AppendLine("id, id AS raw_data_id, " + string.Join(", ", selectedColumns) + $", {IMPORT_DATE_COLUMN}, CURRENT_TIMESTAMP");
            insertQuery.AppendLine($"FROM {RAW_TABLE} r");
            insertQuery.AppendLine($"WHERE NOT EXISTS (");
            insertQuery.AppendLine($"  SELECT 1 FROM hidden_rows h");
            insertQuery.AppendLine($"  WHERE h.row_id = r.id AND h.original_table = '{RAW_TABLE}'");
            insertQuery.AppendLine($")");

            dbManager.ExecuteNonQuery(insertQuery.ToString());

            // 인덱스 생성
            CreateIndicesForProcessTable();

            // 원본 ID에 대한 인덱스도 추가
            string createRawIdIndexQuery = $"CREATE INDEX idx_{PROCESS_TABLE}_raw_id ON {PROCESS_TABLE}(raw_data_id)";
            dbManager.ExecuteNonQuery(createRawIdIndexQuery);

            Debug.WriteLine($"{PROCESS_TABLE} 테이블 생성 완료");
        }

        // 프로세스 테이블 인덱스 생성
        private void CreateIndicesForProcessTable()
        {
            DBManager dbManager = DBManager.Instance;

            // 임포트 날짜 인덱스
            dbManager.ExecuteNonQuery($"CREATE INDEX IF NOT EXISTS idx_{PROCESS_TABLE}_import_date ON {PROCESS_TABLE} ({IMPORT_DATE_COLUMN})");

            // 처리 날짜 인덱스
            dbManager.ExecuteNonQuery($"CREATE INDEX IF NOT EXISTS idx_{PROCESS_TABLE}_processed_date ON {PROCESS_TABLE} (processed_date)");

            Debug.WriteLine($"{PROCESS_TABLE} 테이블 인덱스 생성 완료");
        }

        // 문자열 타입을 SQLite 타입으로 변환
        private string MapStringTypeToSQLite(string dataType)
        {
            switch (dataType.ToLower())
            {
                case "integer":
                case "int":
                case "boolean":
                case "bool":
                    return "INTEGER";
                case "decimal":
                case "float":
                case "double":
                case "real":
                    return "REAL";
                case "datetime":
                case "date":
                    return "DATETIME";
                case "blob":
                case "binary":
                    return "BLOB";
                default:
                    return "TEXT";
            }
        }

        // process_data 테이블에서 데이터 가져오기
        public DataTable GetProcessData(int limit = 1000)
        {
            DBManager dbManager = DBManager.Instance;
            return dbManager.ExecuteQuery($"SELECT * FROM {PROCESS_TABLE} LIMIT {limit}");
        }

        // 페이징된 프로세스 데이터 조회
        public DataTable GetPagedProcessData(int pageNumber, int pageSize)
        {
            DBManager dbManager = DBManager.Instance;
            string baseQuery = $"SELECT * FROM {PROCESS_TABLE}";
            string countQuery = $"SELECT COUNT(*) FROM {PROCESS_TABLE}";

            return dbManager.ExecutePagedQuery(
                baseQuery,
                countQuery,
                pageNumber,
                pageSize);
        }

        // 특정 컬럼 가시성 업데이트
        public void UpdateColumnVisibility(string columnName, bool isVisible)
        {
            DBManager dbManager = DBManager.Instance;

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "column", columnName },
                { "visible", isVisible ? 1 : 0 }
            };

            dbManager.ExecuteNonQuery(
                "UPDATE column_mapping SET is_visible = @visible WHERE original_name = @column",
                parameters);
        }

        

        // 행 숨기기 플래그 테이블 생성
        public void CreateHiddenRowsTable()
        {
            DBManager dbManager = DBManager.Instance;

            dbManager.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS hidden_rows (
                    row_id INTEGER PRIMARY KEY,
                    original_table TEXT NOT NULL,
                    hidden_reason TEXT,
                    hidden_date DATETIME DEFAULT CURRENT_TIMESTAMP
                )");
        }

        // 행 숨기기
        public void HideRow(int rowId, string reason = null)
        {
            DBManager dbManager = DBManager.Instance;

            // hidden_rows 테이블이 없으면 생성
            CreateHiddenRowsTable();

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "row_id", rowId },
                { "table", RAW_TABLE },
                { "reason", reason }
            };

            dbManager.ExecuteNonQuery(@"
                INSERT OR REPLACE INTO hidden_rows (row_id, original_table, hidden_reason)
                VALUES (@row_id, @table, @reason)",
                parameters);
        }

        // 보이는 행만 가져오기
        public DataTable GetVisibleRows(int limit = 1000)
        {
            DBManager dbManager = DBManager.Instance;

            return dbManager.ExecuteQuery($@"
                SELECT r.* FROM {RAW_TABLE} r
                LEFT JOIN hidden_rows h ON r.id = h.row_id AND h.original_table = '{RAW_TABLE}'
                WHERE h.row_id IS NULL
                LIMIT {limit}");
        }

        // 숨겨진 행 보이기
        public void UnhideRow(int rowId)
        {
            DBManager dbManager = DBManager.Instance;

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "row_id", rowId },
                { "table", RAW_TABLE }
            };

            dbManager.ExecuteNonQuery(@"
                DELETE FROM hidden_rows 
                WHERE row_id = @row_id AND original_table = @table",
                parameters);
        }

        // 숨겨진 모든 행 보이기
        public void UnhideAllRows()
        {
            DBManager dbManager = DBManager.Instance;

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "table", RAW_TABLE }
            };

            dbManager.ExecuteNonQuery(@"
                DELETE FROM hidden_rows 
                WHERE original_table = @table",
                parameters);
        }

        // 특정 컬럼 값을 기준으로 행 숨기기
        public int HideRowsByColumnValue(string columnName, string value)
        {
            DBManager dbManager = DBManager.Instance;

            // 1. 숨길 row_id 목록 가져오기
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "value", value }
            };

            DataTable rowsToHide = dbManager.ExecuteQuery(
                $"SELECT id FROM {RAW_TABLE} WHERE {columnName} = @value",
                parameters);

            // 2. 각 행을 숨기기
            int hiddenCount = 0;
            foreach (DataRow row in rowsToHide.Rows)
            {
                int rowId = Convert.ToInt32(row["id"]);
                HideRow(rowId, $"Column '{columnName}' has value '{value}'");
                hiddenCount++;
            }

            return hiddenCount;
        }
    }
}