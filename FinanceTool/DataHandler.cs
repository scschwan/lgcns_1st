using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using Newtonsoft.Json;


namespace FinanceTool
{
    public class DataHandler
    {
        //2025.02.17
        //fileload page 에서 정제된 table
        public static DataTable processTable = new DataTable();

        public static DataTable excelData = new DataTable();
        public static DataTable preprocessedData = new DataTable();
        public static DataTable lowLevelData = new DataTable();
        public static DataTable moneyDataTable = new DataTable();

        public static DataTable recomandKeywordTable = new DataTable();

        //2025.02.13
        //clustering 저장 테이블
        public static DataTable firstClusteringData = new DataTable();
        public static DataTable secondClusteringData = new DataTable();
        public static DataTable finalClusteringData = new DataTable();


        public static int moneyIndex = 0;
        public static List<int> levelList = new List<int>();
        public static List<string> levelName = new List<string>();
        /*
        public static HashSet<string> separator = new HashSet<string> { " ", ",", ".", "/", "(", ")", "_", "#", "~", "*", "[", "]", "!", ":", "%", "-", "'", "&" };

        public static HashSet<string> remover = new HashSet<string> { 
                                               // 월
                                               "12월", "11월", "10월", "9월", "8월", "7월", "6월", "5월", "4월", "3월", "2월", "1월",
                                               // 명수
                                               "9명", "8명", "7명", "6명", "5명", "4명", "3명", "2명", "1명", "0명",
                                               // 연도
                                               "1년", "2년", "3년", "4년", "5년", "6년", "7년", "8년", "9년",
                                               // 숫자
                                               "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
                                            };
        */

        // 다른 CS 파일에서
        //public static SeparatorManager spManager = new SeparatorManager();
        public static SeparatorManager spManager;

        public static string dept_col_name;
        public static string prod_col_name;
        public static string sub_acc_col_name;

        public static bool dept_col_yn = true;
        public static bool prod_col_yn = true;

        public static bool hiddenData = false;

        private static string tempFilePath = Path.Combine(Path.GetTempPath(), "finance_data_temp.json");

        public static DataTable LoadExcelFile(string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheets.First();
                    var range = worksheet.RangeUsed();

                    // 엑셀 데이터를 DataTable로 변환
                    return range.AsTable().AsNativeDataTable();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"엑셀 파일 읽기 실패: {ex.Message}");
            }
        }
        
        public static void DisplayColumnData(DataTable table, int columnIndex, ListBox listBox)
        {
            if (table == null)
            {
                MessageBox.Show("DataTable이 null입니다. 데이터를 로드하세요.");
                return;
            }

            if (columnIndex < 0 || columnIndex >= table.Columns.Count)
            {
                MessageBox.Show("유효하지 않은 열 인덱스입니다.");
                return;
            }

            listBox.Items.Clear();
            foreach (DataRow row in table.Rows)
            {
                if (row[columnIndex] != DBNull.Value)
                {
                    listBox.Items.Add(row[columnIndex].ToString());
                }
                else
                {
                    listBox.Items.Add("(빈 값)");
                }
            }  
        }

        public static DataTable CreateDataTableFromColumns(DataTable sourceTable, List<int> columnIndices)
        {
            // 새로운 DataTable을 생성
            DataTable resultTable = new DataTable();

            Debug.WriteLine($"columns List : { String.Join(", ", columnIndices)}");
            Debug.WriteLine($"sourceTable.Columns.Count : {sourceTable.Columns.Count}");

            // columnIndices에 있는 인덱스를 기반으로 열을 추가
            foreach (int index in columnIndices)
            {
                if (index >= 0 && index < sourceTable.Columns.Count)
                {
                    // 선택된 열을 resultTable에 추가
                    Debug.WriteLine($"sourceTable.Columns[index].ColumnNamet : {sourceTable.Columns[index].ColumnName}");
                    resultTable.Columns.Add(sourceTable.Columns[index].ColumnName, sourceTable.Columns[index].DataType);
                }
                else
                {
                    throw new ArgumentException($"Invalid column index: {index}");
                }
            }
            //raw_data_id 추가
            resultTable.Columns.Add("raw_data_id", typeof(int));

            // sourceTable에서 데이터를 가져와서 resultTable에 추가
            foreach (DataRow row in sourceTable.Rows)
            {
                DataRow newRow = resultTable.NewRow();
                for (int i = 0; i < columnIndices.Count; i++)
                {
                    newRow[i] = row[columnIndices[i]];
                }
                //raw_data_id 추가
                newRow[columnIndices.Count] = row["raw_data_id"];
                resultTable.Rows.Add(newRow);
            }

            return resultTable;
        }

        //2025.02.17
        //preprocessing 에서 사용되는 함수
        //keyword 추출 dataTable 생성
        public static DataTable CombineDataTables(DataTable inputTable)
        {
            DataTable newTable = new DataTable();
            try
            {
                // 1. 필수 컬럼 존재 여부 확인
                if (!processTable.Columns.Contains(dept_col_name) ||
                    !processTable.Columns.Contains(prod_col_name))
                {
                    throw new Exception($"필수 컬럼이 없습니다. (부서: {dept_col_name}, 제품: {prod_col_name})");
                }

                // 2. 필수 컬럼 확인 및 추가
                if (!newTable.Columns.Contains(dept_col_name))
                    newTable.Columns.Add(dept_col_name, processTable.Columns[dept_col_name].DataType);

                if (!newTable.Columns.Contains(prod_col_name))
                    newTable.Columns.Add(prod_col_name, processTable.Columns[prod_col_name].DataType);

                // 3. inputTable의 모든 컬럼 추가
                foreach (DataColumn col in inputTable.Columns)
                {
                    if (col.ColumnName.Equals(dept_col_name) || col.ColumnName.Equals(prod_col_name))
                    {
                        Debug.WriteLine($"중복 컬럼 확인 col.ColumnName : {col.ColumnName}");
                        continue;
                    }

                    //Debug.WriteLine($"컬럼명 확인 col.ColumnName : {col.ColumnName}");

                    newTable.Columns.Add(col.ColumnName, col.DataType);
                    //Debug.WriteLine($"inputTable Columnsname : {col.ColumnName}");
                }

                // 4. 데이터 복사
                for (int i = 0; i < inputTable.Rows.Count; i++)
                {
                    DataRow newRow = newTable.NewRow();

                    // 같은 인덱스의 행에서 데이터 가져오기
                    if (i < processTable.Rows.Count)
                    {
                        newRow[dept_col_name] = processTable.Rows[i][dept_col_name];
                        newRow[prod_col_name] = processTable.Rows[i][prod_col_name];
                    }
                    else if (processTable.Rows.Count > 0)
                    {
                        // 인덱스가 범위를 벗어나면 첫 번째 행 데이터 사용
                        newRow[dept_col_name] = processTable.Rows[0][dept_col_name];
                        newRow[prod_col_name] = processTable.Rows[0][prod_col_name];
                    }
                    else
                    {
                        // processTable이 비어있을 경우
                        newRow[dept_col_name] = DBNull.Value;
                        newRow[prod_col_name] = DBNull.Value;
                    }

                    // inputTable의 현재 행 데이터 복사
                    foreach (DataColumn col in inputTable.Columns)
                    {
                        newRow[col.ColumnName] = inputTable.Rows[i][col.ColumnName];
                    }

                    newTable.Rows.Add(newRow);
                }

                return newTable;
            }
            catch (Exception ex)
            {
                throw new Exception($"테이블 생성 중 오류 발생: {ex.Message}");
            }
        }



        //2025.01.23
        //progress dialog 창
        public class ProgressDialog : Form
        {
            public ProgressBar progressBar;
            private Label statusLabel;

            public ProgressDialog()
            {
                InitializeComponents();
            }

            private void InitializeComponents()
            {
                this.Width = 400;
                this.Height = 120;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.ControlBox = false;
                this.Text = "처리 중...";

               
                progressBar = new ProgressBar
                {
                    Style = ProgressBarStyle.Blocks,
                    Location = new Point(20, 20),
                    Width = 360,
                    Size = new System.Drawing.Size(340, 30),
                    Minimum = 0,
                    Maximum = 100,
                    Value = 0
                };

                statusLabel = new Label
                {
                    Location = new Point(20, 60),
                    Size = new System.Drawing.Size(340, 20),                    
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "데이터 처리 중... (0%)"
                };

                this.Controls.Add(progressBar);
                this.Controls.Add(statusLabel);
            }

            public async Task UpdateProgress(int percentage, string status = null)
            {
                if (InvokeRequired)
                {
                    await Invoke(async () => await UpdateProgress(percentage, status));
                    return;
                }

                progressBar.Value = percentage;
                statusLabel.Text = status ?? $"데이터 처리 중입니다... ({percentage}%)";

                if (percentage >= 100)
                {
                    statusLabel.Text = "처리가 완료되었습니다. (100%)";
                    await Task.Delay(500); // 0.5초 대기
                }
            }
        }

        public static async Task<DataTable> SplitColumnByModel(DataTable inputTable, int limit)
        {
            if (inputTable == null || inputTable.Columns.Count < 1)
            {
                throw new ArgumentException("입력 DataTable이 유효하지 않습니다.");
            }

            
            using var progress = new ProgressDialog();
            DataTable resultTable = null;

            var processTask = Task.Run(async () =>
            {
                var extractor = new KeywordExtractor(1); // exe 파일 사용
                //var extractor = new KeywordExtractor(0); // python_code 파일 사용
                resultTable = await extractor.ExtractKeywordsFromDataTable(inputTable, 0, limit, 1000,
                                new Progress<int>(async value => await progress.UpdateProgress(value)));
            });

            progress.Show();
            await processTask;
            await Task.Delay(500); // 완료 후 0.5초 대기
            progress.Close();

            Debug.WriteLine($"처리 결과: {resultTable.Rows.Count}행, {resultTable.Columns.Count}열");

            // 결과 검증
            foreach (DataRow row in resultTable.Rows)
            {
                var rowValues = row.ItemArray.Select(x => x?.ToString() ?? "").ToList();
                //Debug.WriteLine($"행 데이터: {string.Join(", ", rowValues)}");
            }

            return resultTable;
        }

        public static DataTable ReplaceSeparatorInColumn(DataTable inputTable, int columnIndex, string target, string mode)
        {
            // 입력 DataTable이 비어 있거나 열 인덱스가 유효하지 않은 경우 처리
            if (inputTable == null || inputTable.Columns.Count <= columnIndex)
            {
                Debug.WriteLine($"inputTable.Columns.Count : {inputTable.Columns.Count} , columnIndex : {columnIndex}");
                throw new ArgumentException("유효하지 않은 DataTable 또는 열 인덱스입니다.");
            }

            // 새로운 DataTable 생성 및 대상 열 추가
            DataTable outputTable = new DataTable();
            outputTable.Columns.Add(inputTable.Columns[columnIndex].ColumnName, typeof(string)); // 대상 열 이름 유지

            // 각 행의 데이터를 처리하여 새로운 DataTable에 추가
            foreach (DataRow row in inputTable.Rows)
            {
                if (row[columnIndex] != DBNull.Value) // 값이 null이 아닌 경우 처리
                {
                    string originalValue = row[columnIndex].ToString();
                    string modifiedValue = ReplaceSeparators(originalValue, target, mode);
                    Console.WriteLine("origin : " + originalValue);
                    Console.WriteLine("modifiedValue : " + modifiedValue);

                    // 수정된 값을 새로운 DataTable에 추가
                    outputTable.Rows.Add(modifiedValue);
                }
                else
                {
                    // null 값은 그대로 추가
                    outputTable.Rows.Add(DBNull.Value);
                    Console.WriteLine("Null");
                }
            }
            return outputTable;
        }

        
        private static string ReplaceSeparators(string input, string target, string mode)
        {
            if (mode == "separate")
            {
                //Debug.WriteLine($"manager.Separators : {string.Join(",", spManager.Separators)}");
                //Debug.WriteLine($"manager.getSeparator; : {string.Join(",", manager.getSeparators())}");
                // List<string> separator를 사용
                //foreach (string sep in manager.Separators)
                foreach (string sep in spManager.Separators)
                {
                    input = input.Replace(sep, target);
                }
            }
            else if (mode == "remove")
            {
                // List<string> remover를 사용
                foreach (string rem in spManager.Removers)
                {
                    input = input.Replace(rem, target);
                }
            }
            return input;
        }

        public static DataTable SplitColumnBySeparator(DataTable inputTable, string separator)
        {
            // 입력 DataTable이 유효한지 확인
            if (inputTable == null || inputTable.Columns.Count != 1)
            {
                throw new ArgumentException("입력 DataTable은 반드시 하나의 열만 포함해야 합니다.");
            }

            if (string.IsNullOrEmpty(separator))
            {
                throw new ArgumentException("separator는 null이거나 빈 문자열일 수 없습니다.");
            }

            // 새로운 DataTable 생성
            DataTable outputTable = new DataTable();

            // 각 행의 데이터를 읽고 separator 기준으로 분리
            foreach (DataRow row in inputTable.Rows)
            {
                if (row[0] != DBNull.Value) // 값이 null이 아닌 경우
                {
                    string[] splitValues = row[0].ToString().Split(new string[] { separator }, StringSplitOptions.None);

                    // 분리된 값들의 개수에 따라 열 추가
                    while (outputTable.Columns.Count < splitValues.Length)
                    {
                        //outputTable.Columns.Add($"Column{outputTable.Columns.Count + 1}", typeof(string));
                        outputTable.Columns.Add($"Column{outputTable.Columns.Count}", typeof(string));
                    }

                    // 새 행 추가
                    DataRow newRow = outputTable.NewRow();
                    for (int i = 0; i < splitValues.Length; i++)
                    {
                        newRow[i] = splitValues[i];
                    }
                    outputTable.Rows.Add(newRow);
                }
                else
                {
                    // null 값 처리: 빈 행 추가
                    DataRow emptyRow = outputTable.NewRow();
                    outputTable.Rows.Add(emptyRow);
                }
            }

            return outputTable;
        }

        public static DataTable ProcessUnderscoresInColumn(DataTable inputTable)
        {
            // 입력 검증: DataTable이 null이거나 열이 1개가 아니면 예외 발생
            if (inputTable == null || inputTable.Columns.Count != 1)
            {
                throw new ArgumentException("입력 DataTable은 반드시 하나의 열만 포함해야 합니다.");
            }

            // 새로운 DataTable 생성
            DataTable outputTable = new DataTable();
            outputTable.Columns.Add(inputTable.Columns[0].ColumnName, typeof(string)); // 열 이름 유지

            // 각 행의 데이터를 처리
            foreach (DataRow row in inputTable.Rows)
            {
                if (row[0] != DBNull.Value) // 값이 null이 아닌 경우 처리
                {
                    string originalValue = row[0].ToString();
                    string modifiedValue = ProcessString(originalValue);

                    // 처리된 값을 새로운 DataTable에 추가
                    outputTable.Rows.Add(modifiedValue);
                }
                else
                {
                    // null 값은 그대로 추가
                    outputTable.Rows.Add(DBNull.Value);
                }
            }

            return outputTable;
        }

        public static DataTable ProcessUnderscoresInAllColumn(DataTable inputTable)
        {
            // 입력 검증: DataTable이 null이거나 열이 0개일 경우 예외 발생
            if (inputTable == null || inputTable.Columns.Count == 0)
            {
                throw new ArgumentException("입력 DataTable은 반드시 열이 하나 이상 있어야 합니다.");
            }

            // 새로운 DataTable 생성
            DataTable outputTable = new DataTable();

            // 기존 열의 이름과 타입을 유지하면서 새로운 DataTable의 열을 추가
            foreach (DataColumn column in inputTable.Columns)
            {
                outputTable.Columns.Add(column.ColumnName, column.DataType);
            }

            // 각 행의 데이터를 처리
            foreach (DataRow row in inputTable.Rows)
            {
                // 새로운 행을 생성
                DataRow newRow = outputTable.NewRow();

                // 각 열의 값을 처리
                for (int colIndex = 0; colIndex < inputTable.Columns.Count; colIndex++)
                {
                    if (row[colIndex] != DBNull.Value) // 값이 null이 아닌 경우 처리
                    {
                        string originalValue = row[colIndex].ToString();
                        string modifiedValue = ProcessString(originalValue);

                        // 처리된 값을 새로운 행에 추가
                        newRow[colIndex] = modifiedValue;
                    }
                    else
                    {
                        // null 값은 그대로 추가
                        newRow[colIndex] = DBNull.Value;
                    }
                }

                // 처리된 행을 새로운 DataTable에 추가
                outputTable.Rows.Add(newRow);
            }

            return outputTable;
        }

        private static string ProcessString(string input)
        {
            // 앞뒤의 "_" 제거
            string trimmed = input.Trim('_');

            // 연속된 "_"를 하나로 축소
            string collapsed = Regex.Replace(trimmed, "_+", "_");

            return collapsed;
        }

        public static DataTable CountAndSortAllOccurrences(DataTable inputTable)
        {
            // 입력 검증: DataTable이 null인지 확인
            if (inputTable == null)
            {
                throw new ArgumentNullException(nameof(inputTable), "입력 DataTable이 null입니다.");
            }

            // 값과 반복 횟수를 저장할 Dictionary 생성
            Dictionary<string, int> occurrenceDict = new Dictionary<string, int>();

            // DataTable의 모든 행과 열을 순회하며 값의 반복 횟수를 계산
            foreach (DataRow row in inputTable.Rows)
            {
                foreach (var cell in row.ItemArray) // 각 열의 값을 순회
                {
                    if (cell != DBNull.Value) // Null 값은 무시
                    {
                        string value = cell.ToString();
                        if (occurrenceDict.ContainsKey(value))
                        {
                            occurrenceDict[value]++;
                        }
                        else
                        {
                            occurrenceDict[value] = 1;
                        }
                    }
                }
            }

            // 반복 횟수 기준으로 내림차순 정렬
            var sortedOccurrences = occurrenceDict
                .OrderByDescending(x => x.Value) // 반복 횟수 내림차순
                .ThenBy(x => x.Key)             // 값 오름차순 (반복 횟수가 같은 경우)
                .ToList();

            // 결과를 담을 새로운 DataTable 생성
            DataTable outputTable = new DataTable();
            outputTable.Columns.Add("Value", typeof(string));      // 값 열 추가
            outputTable.Columns.Add("Count", typeof(int));  // 반복 횟수 열 추가

            // 정렬된 값을 DataTable에 추가
            foreach (var entry in sortedOccurrences)
            {
                outputTable.Rows.Add(entry.Key, entry.Value);
            }

            return outputTable;
        }

        public static int CountRowsWithOccurrencesAboveThreshold(DataTable inputTable, int threshold, string targetColumn)
        {
            // 입력 검증
            if (inputTable == null)
            {
                MessageBox.Show("입력 DataTable이 null입니다.", "입력 검증 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0; // 메서드 종료
            }

            if (!inputTable.Columns.Contains(targetColumn))
            {
                MessageBox.Show($"DataTable에 '{targetColumn}' 열이 포함되어야 합니다.", "입력 검증 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0; // 메서드 종료
            }

            // 반복 횟수가 기준 이상인 행을 카운트 "Occurrences"
            int count = 0;
            foreach (DataRow row in inputTable.Rows)
            {
                if (row[targetColumn] != DBNull.Value && int.TryParse(row[targetColumn].ToString(), out int occurrences))
                {
                    if (occurrences >= threshold)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /* -- backup
        public static DataTable CreateListRowsWithOccurrencesAboveThreshold(DataTable inputTable, int threshold)
        {
            DataTable outputTable = new DataTable();
            try
            {
                // 입력 검증
                if (inputTable == null)
                {
                    throw new ArgumentNullException(nameof(inputTable), "입력 DataTable이 null입니다.");
                }
                if (!inputTable.Columns.Contains("Occurrences"))
                {
                    throw new ArgumentException("DataTable에 'Occurrences' 열이 포함되어야 합니다.");
                }

                foreach (DataRow row in inputTable.Rows)
                {
                    if (row["Occurrences"] != DBNull.Value && int.TryParse(row["Occurrences"].ToString(), out int occurrences))
                    {
                        if (occurrences >= threshold)
                        {
                            while (outputTable.Columns.Count < row.ItemArray.Length)
                            {
                                outputTable.Columns.Add($"Column{outputTable.Columns.Count + 1}");
                            }
                            DataRow newRow = outputTable.NewRow();
                            newRow.ItemArray = row.ItemArray;
                            outputTable.Rows.Add(newRow);
                        }
                    }
                }
            }
            catch
            {

            }

            return outputTable;
        }*/

        public static DataTable CreateListRowsWithOccurrencesAboveThreshold(DataTable inputTable, int threshold, string targetColumn)
        {
            DataTable outputTable = new DataTable();
            try
            {
                // 입력 검증
                if (inputTable == null)
                {
                    throw new ArgumentNullException(nameof(inputTable), "입력 DataTable이 null입니다.");
                }
                if (!inputTable.Columns.Contains(targetColumn))
                {
                    throw new ArgumentException($"DataTable에 '{targetColumn}' 열이 포함되어야 합니다.");
                }

                // 출력 테이블 열 정의 (데이터 타입 유지)
                foreach (DataColumn column in inputTable.Columns)
                {
                    outputTable.Columns.Add(column.ColumnName, column.DataType); // 원본 열의 데이터 타입 유지
                }

                // 행 추가
                foreach (DataRow row in inputTable.Rows)
                {
                    if (row[targetColumn] != DBNull.Value && int.TryParse(row[targetColumn].ToString(), out int occurrences))
                    {
                        if (occurrences >= threshold)
                        {
                            DataRow newRow = outputTable.NewRow();
                            newRow.ItemArray = row.ItemArray;
                            outputTable.Rows.Add(newRow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}");
            }

            return outputTable;
        }

        public static DataTable RemoveMatchingRows(DataTable A, DataTable B, int index)
        {
            // 결과로 반환할 DataTable 생성 (A의 스키마 복사)
            DataTable C = A.Clone();

            // B의 특정 열 데이터를 HashSet으로 저장 (중복 제거 및 빠른 조회를 위해)
            var bValues = B.AsEnumerable()
                           .Select(row => row.Field<object>(index))
                           .ToHashSet();

            // A의 특정 열 데이터를 순회하며 B와 겹치지 않는 행만 C에 추가
            foreach (DataRow row in A.Rows)
            {
                if (!bValues.Contains(row[index]))
                {
                    C.Rows.Add(row.ItemArray); // A의 행 데이터를 그대로 추가
                }
            }

            return C;
        }

        public static List<string> GetColumnValuesAsList(DataTable table, int columnIndex)
        {
            // 반환할 리스트 초기화
            List<string> result = new List<string>();

            // DataTable의 행을 순회하며 특정 열의 값을 리스트에 추가
            foreach (DataRow row in table.Rows)
            {
                // 열의 값을 문자열로 변환하여 추가 (null 값은 빈 문자열로 처리)
                result.Add(row[columnIndex]?.ToString() ?? string.Empty);
            }

            return result;
        }

        public static List<string> FindMatchingPairs(List<string> listA, List<string> listB)
        {
            // 결과를 저장할 리스트
            List<string> output = new List<string>();

            // B의 각 값을 A와 비교
            foreach (string valueB in listB)
            {
                foreach (string valueA in listA)
                {
                    // A와 B의 값이 서로 포함 관계인지 확인
                    if (valueA.Contains(valueB) || valueB.Contains(valueA))
                    {
                        // 포함 관계가 있으면 출력 리스트에 추가
                        output.Add($"{valueA}<{valueB}");
                    }
                }
            }

            return output;
        }

        //2025.02.13
        //키워드 기준 매칭 데이터 비교
        public static List<string> FindMachKeyword(List<string> listA, string search_keyword)
        {
            // 결과를 저장할 리스트
            List<string> output = new List<string>();

            // B의 각 값을 A와 비교
            foreach (string valueA in listA)
            {
                
                
                // A와 B의 값이 서로 포함 관계인지 확인
                if (CompareByTwoChars(search_keyword, valueA))
                {
                    // 포함 관계가 있으면 출력 리스트에 추가
                    //Debug.WriteLine($"포함 키워드 대상 감지 : search_keyword : {search_keyword} valueA : {valueA}");
                    output.Add(valueA);
                }
            }
            
            //Debug.WriteLine($"output List : {string.Join(",", output)}");

            return output;
        }

        public static List<string> FindMachEqualsKeyword(List<string> listA, string search_keyword)
        {
            // 결과를 저장할 리스트
            List<string> output = new List<string>();

            // search_keyword가 쉼표를 포함하는지 확인
            if (search_keyword.Contains(","))
            {
                // 쉼표로 분리하여 리스트로 변환
                List<string> searchKeywords = search_keyword.Split(',')
                                                           .Select(k => k.Trim())
                                                           .Where(k => !string.IsNullOrEmpty(k))
                                                           .ToList();

                // 각 검색 키워드에 대해
                foreach (string keyword in searchKeywords)
                {
                    foreach (string valueA in listA)
                    {
                        // 정확히 일치하는지 확인
                        if (keyword.Equals(valueA))
                        {
                            // 일치하면 출력 리스트에 추가
                            output.Add(valueA);
                            break; // 현재 keyword에 대한 검색 종료
                        }
                    }
                }
            }
            else
            {
                // 쉼표가 없는 경우 기존 로직 유지
                foreach (string valueA in listA)
                {
                    if (search_keyword.Equals(valueA))
                    {
                        output.Add(valueA);
                        break;
                    }
                }
            }

            return output;
        }



        public static void ReplaceCellValues(DataTable inputTable, string valueToFind, string replacementValue)
        {
            // DataTable의 모든 행과 열 순회
            foreach (DataRow row in inputTable.Rows)
            {
                for (int columnIndex = 0; columnIndex < inputTable.Columns.Count; columnIndex++)
                {
                    // 특정 값(A)이 발견되면 B로 변경
                    if (row[columnIndex]?.ToString() == valueToFind)
                    {
                        row[columnIndex] = replacementValue;
                    }
                }
            }
        }

        public static void SaveDataTableToExcel(DataTable firstTable, DataTable secondTable = null)
        {
            try
            {
                // SaveFileDialog 생성
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                    saveFileDialog.Title = "Save Excel File";
                    saveFileDialog.DefaultExt = "xlsx";
                    saveFileDialog.AddExtension = true;

                    // 대화 상자를 띄워 사용자로부터 경로를 입력받음
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;

                        // 엑셀 파일 생성 및 저장
                        using (var workbook = new XLWorkbook())
                        {
                            // 첫 번째 테이블은 항상 추가
                            var firstSheet = workbook.Worksheets.Add(firstTable, "Clustering 결과");

                            // 두 번째 테이블이 있으면 추가
                            if (secondTable != null && secondTable.Rows.Count > 0)
                            {
                                var secondSheet = workbook.Worksheets.Add(secondTable, "Clustering 원본");
                            }

                            workbook.SaveAs(filePath);

                            /*
                            string message = secondTable != null ?
                                "Excel file이 두 개의 시트로 생성되었습니다." :
                                "Excel file이 생성되었습니다.";
                            */
                            string message = "Excel file이 생성되었습니다.";
                            MessageBox.Show($"{message}\n{filePath}",
                                           "Success",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel file 생성이 실패하였습니다:\n{ex.Message}",
                               "Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
        }

        //tableB에 하위레벨과 빈도수가 적혀 있음
        public static DataTable ProcessDataTables(DataTable tableA, DataTable tableB)
        {
            // Output DataTable 생성
            DataTable outputTable = new DataTable();
            //outputTable.Columns.Add("ValuesFromA", typeof(string));
            outputTable.Columns.Add("MatchedValueFromB", typeof(string));

            // A의 각 행 순회
            foreach (DataRow rowA in tableA.Rows)
            {
                string matchedValue = "Undefined"; // 기본값 "Undefined"

                // A의 모든 열을 순차적으로 비교
                foreach (var valueFromA in rowA.ItemArray)
                {
                    int MaxCount = 0;
                    // B의 첫 번째 열과 비교
                    foreach (DataRow rowB in tableB.Rows)
                    {
                        string valueFromB = rowB[0]?.ToString();
                        if (valueFromA.ToString() == valueFromB)
                        {
                            //Debug.WriteLine(rowB[1]?.ToString());
                            int nowCount = Convert.ToInt32(rowB[1]?.ToString());
                            if(MaxCount < nowCount)
                            {
                                MaxCount = nowCount;
                                matchedValue = valueFromB; // 일치하는 값 발견 시 갱신
                            }
                        }
                    }
                    /*
                    if (matchedValue != "Undefined")
                    {
                        break; // 일치하는 값을 찾았으므로 더 이상 비교할 필요 없음
                    }*/
                }

                // Output DataTable에 결과 추가
                string valuesFromA = string.Join(",", rowA.ItemArray); // A의 행에 있는 모든 열 값
                outputTable.Rows.Add(matchedValue);
            }

            return outputTable;
        }

        public static DataTable AddColumnsFromBToA(DataTable target, DataTable source)
        {
            // B의 각 열을 A에 추가
            foreach (DataColumn columnB in source.Columns)
            {
                // tableA에 B의 열을 추가
                target.Columns.Add(columnB.ColumnName, columnB.DataType);
            }

            int rowIndex = 0;

            // B의 각 행을 A의 새로운 열에 추가
            foreach (DataRow rowB in source.Rows)
            {
                if (rowIndex < target.Rows.Count)
                {
                    // A의 각 행에 대해 B의 열 데이터를 추가
                    for (int i = 0; i < source.Columns.Count; i++)
                    {
                        target.Rows[rowIndex][source.Columns[i].ColumnName] = rowB[i];
                    }
                    rowIndex++;
                }
            }

            // B의 행 수가 A의 행 수보다 적으면 남은 A의 행에 기본값 추가
            while (rowIndex < target.Rows.Count)
            {
                for (int i = 0; i < source.Columns.Count; i++)
                {
                    target.Rows[rowIndex][source.Columns[i].ColumnName] = "NULL"; // 기본값 설정
                }
                rowIndex++;
            }

            return target;
        }

        public static DataTable SumMoneyDataByKewords(DataTable modifiedDataTable, DataTable moneyDataTable, DataTable originDataTable)
        {
            // 새로운 데이터테이블 생성 (키워드와 합산 금액 열 추가)
            DataTable resultDataTable = new DataTable();
            resultDataTable.Columns.Add("키워드", typeof(string));
            resultDataTable.Columns.Add("합산 금액", typeof(decimal));

            // modifiedDataTable의 0열 (키워드) 값을 기준으로 검색
            foreach (DataRow modifiedRow in modifiedDataTable.Rows)
            {
                string keyword = modifiedRow[0].ToString(); // 키워드 가져오기
                decimal totalAmount = 0;

                // originDataTable에서 해당 키워드를 가진 행 찾기
                foreach (DataRow originRow in originDataTable.Rows)
                {
                    if (originRow.ItemArray.Contains(keyword))
                    {
                        // moneyDataTable에서 동일한 행의 첫 번째 열 값 합산
                        int rowIndex = originDataTable.Rows.IndexOf(originRow);
                        if (rowIndex < moneyDataTable.Rows.Count)
                        {
                            if (decimal.TryParse(moneyDataTable.Rows[rowIndex][0].ToString(), out decimal moneyValue))
                            {
                                totalAmount += moneyValue;
                            }
                        }
                    }
                }

                // 결과를 resultDataTable에 추가
                DataRow resultRow = resultDataTable.NewRow();
                resultRow["키워드"] = keyword;
                resultRow["합산 금액"] = totalAmount;
                resultDataTable.Rows.Add(resultRow);
            }

            return resultDataTable;
        }

        public static DataTable ExtractColumnToNewTable(DataTable inputTable, int index)
        {
            // 유효성 검사
            if (inputTable == null)
                throw new ArgumentNullException(nameof(inputTable));
            if (index < 0 || index >= inputTable.Columns.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "유효하지 않은 열 인덱스입니다.");

            // 새로운 데이터테이블 생성
            DataTable resultTable = new DataTable();
            resultTable.Columns.Add(inputTable.Columns[index].ColumnName, inputTable.Columns[index].DataType);
            resultTable.Columns.Add("raw_data_id", typeof(decimal));

            // 데이터 복사
            foreach (DataRow row in inputTable.Rows)
            {
                DataRow newRow = resultTable.NewRow();
                newRow[0] = row[index];
                
                //2025.02.27
                //raw_data_id도 추가
                newRow[1] = row["raw_data_id"];
                resultTable.Rows.Add(newRow);
            }

            return resultTable;
        }

        public static DataTable ProcessShortStringsToNull(DataTable inputTable )
        {
            // 입력 검증: DataTable이 null이면 예외 발생
            if (inputTable == null)
            {
                throw new ArgumentException("입력 DataTable은 null일 수 없습니다.");
            }

            // 새로운 DataTable 생성 (기존 DataTable의 구조 유지)
            DataTable outputTable = inputTable.Copy();  // 구조와 데이터를 복사

            // 각 행의 데이터를 처리
            foreach (DataRow row in outputTable.Rows)
            {
                
                // 각 열을 순회
                for (int colIndex = 0; colIndex < outputTable.Columns.Count; colIndex++)
                {
                    // 셀 값이 null이 아니고 글자 길이가 1 이하면 null로 변경
                    if (row[colIndex] != DBNull.Value)
                    {
                        string cellValue = row[colIndex].ToString();
                        if (cellValue.Length <= 1)
                        {
                            row[colIndex] = DBNull.Value;  // 글자 길이가 1 이하인 경우 null로 변경
                        }
                    }
                }
               
                
            }

            return outputTable;
        }


        //2025.02.13
        //키워드 비교 함수
        //2글자씩 slice 하여 비교
        public static bool CompareByTwoChars(string baseWord, string targetWord)
        {

            if (targetWord.Length < 2)
            {
                return false;
            }

            // 2글자 미만인 경우 처리
            if (baseWord.Length < 2 )
            {
                //return false;
                return targetWord.Contains(baseWord);

            }

            // 기준 단어를 2글자씩 자르기
            List<string> baseParts = new List<string>();
            for (int i = 0; i < baseWord.Length - 1; i++)
            {
                baseParts.Add(baseWord.Substring(i, 2));
            }

            // 대상 단어를 2글자씩 자르기
            List<string> targetParts = new List<string>();
            for (int i = 0; i < targetWord.Length - 1; i++)
            {
                targetParts.Add(targetWord.Substring(i, 2));
            }

            // 두 리스트 간에 공통된 2글자 조합이 있는지 확인
            return baseParts.Any(b => targetParts.Contains(b));
        }

        //2025.02.13
        //dataTable Clustering 함수 구현
        public static DataTable CreateSetGroupDataTable(DataTable sourceTable, DataTable referenceTable, bool secondyn = false)
        {
            Debug.WriteLine("CreateSetGroupDataTable 수행");
            Debug.WriteLine($"sourceTable Table count: {sourceTable.Rows.Count}");

            // 결과 DataTable 생성
            DataTable resultTable = new DataTable();
            resultTable.Columns.Add("ID", typeof(int));
            resultTable.Columns.Add("ClusterID", typeof(int));
            resultTable.Columns.Add("클러스터명", typeof(string));
            resultTable.Columns.Add("키워드목록", typeof(string));
            resultTable.Columns.Add("Count", typeof(int));
            resultTable.Columns.Add("합산금액", typeof(decimal));
            resultTable.Columns.Add("dataIndex", typeof(string));

            // referenceTable의 0번 컬럼명 가져오기 (금액 컬럼)
            string moneyColumnName = referenceTable.Columns[0].ColumnName;

            // 금액 정보 조회를 위한 딕셔너리 생성
            Dictionary<int, decimal> moneyLookup = new Dictionary<int, decimal>();

            // SQL을 통해 금액 데이터 조회 (raw_data_id 기준)
            string moneyQuery = $"SELECT raw_data_id, {moneyColumnName} FROM temp_money_data";
            DataTable moneyTable = DBManager.Instance.ExecuteQuery(moneyQuery);

            foreach (DataRow row in moneyTable.Rows)
            {
                int rawDataId = Convert.ToInt32(row["raw_data_id"]);
                decimal moneyValue = Convert.ToDecimal(row[moneyColumnName]);
                moneyLookup[rawDataId] = moneyValue;
            }

            // secondyn=true인 경우 process_data에서 부서/공급업체 정보 조회
            Dictionary<int, string> deptLookup = new Dictionary<int, string>();
            Dictionary<int, string> prodLookup = new Dictionary<int, string>();

            if (secondyn && (dept_col_yn || prod_col_yn))
            {
                List<string> columnsToSelect = new List<string> { "raw_data_id" };
                if (dept_col_yn) columnsToSelect.Add(dept_col_name);
                if (prod_col_yn) columnsToSelect.Add(prod_col_name);

                string processQuery = $"SELECT {string.Join(", ", columnsToSelect)} FROM process_data";
                DataTable processInfo = DBManager.Instance.ExecuteQuery(processQuery);

                foreach (DataRow row in processInfo.Rows)
                {
                    int rawDataId = Convert.ToInt32(row["raw_data_id"]);

                    if (dept_col_yn)
                    {
                        string deptValue = row[dept_col_name]?.ToString() ?? "";
                        deptLookup[rawDataId] = deptValue;
                    }

                    if (prod_col_yn)
                    {
                        string prodValue = row[prod_col_name]?.ToString() ?? "";
                        prodLookup[rawDataId] = prodValue;
                    }
                }
            }

            // 집합 셋을 저장할 Dictionary
            Dictionary<string, (int ID, int Count, decimal SumValue, List<int> SourceIndices)> setGroups =
                new Dictionary<string, (int, int, decimal, List<int>)>();

            // 데이터 테이블 이름 결정
            string dataTableName = secondyn ? "temp_transform_data" : "process_view_data";

            // 키워드 데이터를 한 번에 가져오기
            string keywordQuery = $@"
        SELECT * FROM {dataTableName}
    ";

            DataTable keywordData = DBManager.Instance.ExecuteQuery(keywordQuery);

            // 각 행을 순회하면서 집합 셋 생성
            foreach (DataRow row in keywordData.Rows)
            {
                int rawDataId = Convert.ToInt32(row["raw_data_id"]);

                // 집합 셋 생성
                List<string> setElements = new List<string>();

                // 테이블의 모든 컬럼을 확인하면서 키워드 추출
                foreach (DataColumn col in keywordData.Columns)
                {
                    string colName = col.ColumnName;

                    // id, raw_data_id, import_date는 제외
                    if (colName == "id" || colName == "raw_data_id" || colName == "import_date")
                    {
                        continue;
                    }

                    string keywordValue = row[colName]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(keywordValue))
                    {
                        setElements.Add(keywordValue);
                    }
                }

                // secondyn=true인 경우 부서/공급업체 정보 추가
                if (secondyn)
                {
                    if (dept_col_yn && deptLookup.TryGetValue(rawDataId, out string deptValue) && !string.IsNullOrWhiteSpace(deptValue))
                    {
                        setElements.Add(deptValue);
                    }

                    if (prod_col_yn && prodLookup.TryGetValue(rawDataId, out string prodValue) && !string.IsNullOrWhiteSpace(prodValue))
                    {
                        setElements.Add(prodValue);
                    }
                }

                // 집합 셋을 정렬하여 일관성 유지
                setElements.Sort();

                // 집합 셋 문자열 생성
                string setKey = string.Join(",", setElements);

                // 금액 정보 조회
                decimal refValue = 0;
                moneyLookup.TryGetValue(rawDataId, out refValue);

                // Dictionary에 추가 또는 업데이트
                if (setGroups.ContainsKey(setKey))
                {
                    var existing = setGroups[setKey];
                    existing.SourceIndices.Add(rawDataId);
                    setGroups[setKey] = (existing.ID, existing.Count + 1, existing.SumValue + refValue, existing.SourceIndices);
                }
                else
                {
                    setGroups.Add(setKey, (setGroups.Count, 1, refValue, new List<int> { rawDataId }));
                }
            }

            // Dictionary의 데이터를 결과 테이블에 추가
            foreach (var group in setGroups)
            {
                string[] elements = group.Key.Split(',');
                DataRow newRow = resultTable.NewRow();
                newRow["ID"] = group.Value.ID;
                newRow["ClusterID"] = -1;
                newRow["클러스터명"] = string.Join("_", elements);
                newRow["키워드목록"] = group.Key;
                newRow["Count"] = group.Value.Count;
                newRow["합산금액"] = group.Value.SumValue;
                newRow["dataIndex"] = string.Join(",", group.Value.SourceIndices);
                resultTable.Rows.Add(newRow);
            }

            Debug.WriteLine("CreateSetGroupDataTable 수행완료");
            Debug.WriteLine($"result Table count: {resultTable.Rows.Count}");
            return resultTable;
        }

        //2025.02.18
        //clustering 결과 datagridview 생성 함수
        public static void SetupDataGridView(DataGridView dgv, DataTable dt)
        {
            // 조건에 맞는 데이터만 필터링
            var filteredData = dt.AsEnumerable()
                .Where(row =>
                    Convert.ToInt32(row["ClusterID"]) <= 0 ||
                    Convert.ToInt32(row["ClusterID"]) == Convert.ToInt32(row["ID"]))
                .CopyToDataTable();

            dgv.DataSource = filteredData;

            //Debug.WriteLine($"dt Count : {dt.Rows.Count}");
            //Debug.WriteLine($"filteredData Count : {filteredData.Rows.Count}");


            // ID 컬럼 숨기기
            if (dgv.Columns["ID"] != null)
            {
                dgv.Columns["ID"].Visible = false;
            }

            // ClusterID 컬럼 숨기기
            dgv.Columns["ClusterID"].Visible = false;

            // dataIndex 컬럼 숨기기
            dgv.Columns["dataIndex"].Visible = false;


            // Count 컬럼 형식 지정
            if (dgv.Columns["Count"] != null)
            {
                dgv.Columns["Count"].DefaultCellStyle.Format = "N0"; // 천 단위 구분자
                dgv.Columns["Count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // 합산금액 컬럼 형식 지정
            if (dgv.Columns["합산금액"] != null)
            {
                dgv.Columns["합산금액"].DefaultCellStyle.Format = "N0"; // 천 단위 구분자
                dgv.Columns["합산금액"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (dgv.Columns["클러스터명"] != null)
            {
                //Debug.WriteLine("second table logic 적용");
                dgv.Columns["클러스터명"].ReadOnly = true;
            }

            // 나머지 컬럼들은 읽기 전용
            if (dgv.Columns["키워드목록"] != null)
            {
                dgv.Columns["키워드목록"].ReadOnly = true;
            }

            // 기본 설정
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
        }


        private static bool isProcessingSelection = false;

        public static void SyncDataGridViewSelections(DataGridView dataGridView1, DataGridView dataGridView2)
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
                catch (Exception ex)
                {
                    //Debug.WriteLine(ex.Message);
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
                catch (Exception ex)
                {
                    //Debug.WriteLine(ex.Message);
                }
                finally
                {
                    isProcessingSelection = false;
                }
            };          
        }


        // DataGridView별로 선택된 셀들을 추적하기 위한 딕셔너리
        // 마우스 다운/업 이벤트를 사용하여 선택 영역 추적
        
        public static Dictionary<DataGridView, List<DataGridViewCell>> dragSelections = new Dictionary<DataGridView, List<DataGridViewCell>>();

        public static void RegisterDataGridView(DataGridView dgv)
        {
            // 초기화
            dragSelections[dgv] = new List<DataGridViewCell>();

            // 이벤트 핸들러 등록
            dgv.MouseUp += DataGridView_MouseUp;
            dgv.CellContentClick += DataGridView_CellContentClick;
        }

       

        public static void DataGridView_MouseUp(object sender, MouseEventArgs e)
        {
            
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;

            // 마우스 업 시 현재 선택된 셀 저장
            dragSelections[dgv].Clear();
            foreach (DataGridViewCell cell in dgv.SelectedCells)
            {
                dragSelections[dgv].Add(cell);
            }

            // 디버그용
            Debug.WriteLine($"선택된 셀 수: {dragSelections[dgv].Count}");
        }

        public static void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;

            //Debug.WriteLine($"DataGridView_CellContentClick start => dragSelections[dgv].Count : {dragSelections[dgv].Count}");
            // 체크박스 컬럼 클릭 시 (체크박스 컬럼이 0번 컬럼이라고 가정)
            //if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                // 클릭된 셀의 체크박스 상태 확인
                DataGridViewCheckBoxCell clickedCell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;

                
                if (clickedCell == null) return;

                bool newValue = !(Convert.ToBoolean(clickedCell.Value)); // 현재 값의 반대로 설정

                // 마우스 업에서 저장한 선택 영역 사용
                if (dragSelections.ContainsKey(dgv) && dragSelections[dgv].Count > 0)
                {
                    // 저장된 선택 영역의 모든 체크박스 상태 변경
                    foreach (DataGridViewCell cell in dragSelections[dgv])
                    {
                        if (cell.ColumnIndex == 0) // 체크박스 컬럼인 경우
                        {
                            Debug.WriteLine($"cell.RowIndex : {cell.RowIndex}");
                            
                            DataGridViewCheckBoxCell checkCell = dgv.Rows[cell.RowIndex].Cells[0] as DataGridViewCheckBoxCell;
                            if (checkCell != null)
                                checkCell.Value = newValue;
                        }
                    }
                }
                else
                {
                    // 저장된 선택 영역이 없으면 클릭된 셀만 변경
                    clickedCell.Value = newValue;
                }
                // 마우스 다운 시 현재 선택된 셀 저장
                dragSelections[dgv].Clear();
                // 데이터그리드뷰 새로고침
                dgv.Refresh();
            }
            //Debug.WriteLine("DataGridView_CellContentClick end");
        }

        // 커스텀 정렬 이벤트 핸들러
        public static void money_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            // 디버깅을 위한 로깅 추가
            //Debug.WriteLine($"SortCompare 호출됨: DataGridView={sender.GetType().Name}, Column={e.Column.Name}, HeaderText={e.Column.HeaderText}");
            try
            {
                // 정렬이 어떤 DataGridView에서 발생했는지 확인
                // 디버그 로그 추가
                //Debug.WriteLine($"정렬 시도: Column={e.Column.Name}, HeaderText={e.Column.HeaderText}, ValueType={e.Column.ValueType}");

                // 두 값이 모두 null이면 동등하게 처리
                if ((e.CellValue1 == null || e.CellValue1 == DBNull.Value) &&
                    (e.CellValue2 == null || e.CellValue2 == DBNull.Value))
                {
                    e.SortResult = 0;
                    e.Handled = true;
                    return;
                }

                // 값1이 null이면 값2보다 작게 처리
                if (e.CellValue1 == null || e.CellValue1 == DBNull.Value)
                {
                    e.SortResult = -1;
                    e.Handled = true;
                    return;
                }

                // 값2가 null이면 값1보다 크게 처리
                if (e.CellValue2 == null || e.CellValue2 == DBNull.Value)
                {
                    e.SortResult = 1;
                    e.Handled = true;
                    return;
                }

                // "금액" 컬럼에 대해서만 커스텀 정렬 적용
                if (e.Column.Name == "합산금액" || e.Column.HeaderText == "합산금액" || e.Column.Name == "total_money" || e.Column.HeaderText == "total_money")
                {
                    //Debug.WriteLine($"커스텀 정렬 적용: Column={e.Column.Name}, HeaderText={e.Column.HeaderText}");
                    // 셀 값에서 숫자만 추출
                    Decimal val1 = ExtractNumber(e.CellValue1?.ToString() ?? "");
                    Decimal val2 = ExtractNumber(e.CellValue2?.ToString() ?? "");

                    //Debug.WriteLine($"비교 값: {e.CellValue1} ({val1}) vs {e.CellValue2} ({val2})");

                    // 숫자 기준으로 비교
                    e.SortResult = val1.CompareTo(val2);
                    // 이벤트 처리 완료 표시
                    e.Handled = true;

                    //Debug.WriteLine("커스텀 정렬 완료");
                }
                else if (e.Column.ValueType == typeof(string))
                {
                    //Debug.WriteLine($"[string]기본 정렬 사용: Column={e.Column.Name}, HeaderText={e.Column.HeaderText} , ValueType={e.Column.ValueType}");
                    // 문자열 타입에 대한 안전한 처리 추가
                    string value1 = e.CellValue1?.ToString() ?? string.Empty;
                    string value2 = e.CellValue2?.ToString() ?? string.Empty;

                    e.SortResult = string.Compare(value1, value2);
                    e.Handled = true;
                }
                else
                {
                    //Debug.WriteLine($"[default]기본 정렬 사용: Column={e.Column.Name}, HeaderText={e.Column.HeaderText} , ValueType={e.Column.ValueType}");
                }
            }
            catch (Exception ex)
            {
                // 예외 발생 시 로그 기록
                Debug.WriteLine($"정렬 중 예외 발생: {ex.Message}");

                // 기본 정렬 사용
                Debug.WriteLine($"기본 정렬 사용: Column={e.Column.Name}, HeaderText={e.Column.HeaderText}");
                e.Handled = false;
            }
           
        }

        // 문자열에서 숫자만 추출하는 함수
        private static Decimal ExtractNumber(string text)
        {
            /*
            if (string.IsNullOrEmpty(text))
                return 0;

            // 마이너스 부호 여부 확인
            bool isNegative = text.Trim().StartsWith("-");

            // 숫자만 추출 (마이너스 부호 제외)
            string numericPart = new string(text.Where(c => char.IsDigit(c)).ToArray());

            // 숫자 부분이 비어 있으면 0 반환
            if (string.IsNullOrEmpty(numericPart))
                return 0;

            // 숫자로 변환
            if (Decimal.TryParse(numericPart, out Decimal result))
            {
                // 마이너스 부호가 있었다면 결과값을 음수로 변환
                return isNegative ? -result : result;
            }

            return 0;
            */
            if (string.IsNullOrEmpty(text))
            {
                Debug.WriteLine($"text is null?? {text}");
                return 0;
            }

            //Debug.WriteLine($"tExtractNumber text :  {text}");
            //, 값 변환

            // 콤마 제거
            string cleanText = text.Replace(",", "");

            // 정규식으로 숫자 패턴 추출 (부호, 숫자, 소수점 포함)
            Match match = Regex.Match(cleanText, @"(-?\d+(\.\d+)?)");

            if (match.Success && Decimal.TryParse(match.Value, out Decimal result))
            {
                return result;
            }
            else
            {
               
                Debug.WriteLine($"text is match.Success  : {match.Success}, match.Value :  {match.Value} " );
                if (Decimal.TryParse(match.Value, out Decimal result33))
                {
                    Debug.WriteLine($",Convert.ToDecimal(match.Value) :  {Convert.ToDecimal(match.Value)}");
                }
                
            }

            return 0;
        }


        public static void ConfigureDataGridView(DataTable dataTable, DataGridView dataGridView)
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
                        row.DefaultCellStyle.BackColor = Color.LightGray;
                        row.DefaultCellStyle.ForeColor = Color.DarkGray;
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
                //MessageBox.Show("'hiddenYN' 컬럼이 존재하지 않습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (dataGridView.Columns["raw_data_id"] != null)
            {
                dataGridView.Columns["raw_data_id"].Visible = false;
            }

            if (dataGridView.Columns["hiddenYN"] != null)
            {
                dataGridView.Columns["hiddenYN"].Visible = false;
            }

            if (dataGridView.Columns["import_date"] != null)
            {
                dataGridView.Columns["import_date"].Visible = false;
            }

            
        }
    }
}
