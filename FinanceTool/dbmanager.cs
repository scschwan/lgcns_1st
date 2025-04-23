using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;  // StringBuilder를 사용하기 위한 네임스페이스 추가

namespace FinanceTool
{
    /// <summary>
    /// SQLite 데이터베이스 관리 클래스
    /// </summary>
    public class DBManager : IDisposable
    {
        private static readonly object _lockObj = new object();
        private static DBManager _instance;
        private SQLiteConnection _connection;
        private string _dbFilePath;
        private bool _disposed = false;
        private bool _isInitialized = false;

        // 싱글톤 인스턴스 접근자 - 필요시에만 인스턴스 생성
        public static DBManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new DBManager();
                        }
                    }
                }
                return _instance;
            }
        }

        // 생성자 - 초기화는 별도 수행
        private DBManager()
        {
            // 생성자에서는 초기화하지 않음
        }

        public bool ResetDatabase()
        {
            try
            {
                Debug.WriteLine("데이터베이스 초기화 시작...");

                // 기존 연결 닫기
                if (_connection != null)
                {
                    if (_connection.State == ConnectionState.Open)
                    {
                        _connection.Close();
                    }
                    _connection.Dispose();
                    _connection = null;
                }

                // 초기화 상태 리셋
                _isInitialized = false;

                // 데이터베이스 다시 초기화
                InitializeDatabase();
                _isInitialized = true;

                Debug.WriteLine("데이터베이스 초기화 완료");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"데이터베이스 초기화 오류: {ex.Message}");
                return false;
            }
        }

        // 초기화 상태 확인을 위한 속성 추가
        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        // 데이터베이스 파일 경로 확인을 위한 속성 추가
        public string DbPath
        {
            get { return _dbFilePath; }
        }


        // 데이터베이스 초기화 상태 확인 및 필요시 초기화
        public bool EnsureInitialized()
        {
            if (_disposed)
                return false;

            if (!_isInitialized)
            {
                lock (_lockObj)
                {
                    if (!_isInitialized)
                    {
                        try
                        {
                            InitializeDatabase();
                            _isInitialized = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"데이터베이스 초기화 오류: {ex.Message}");
                            return false;
                        }
                    }
                }
            }
            return _isInitialized;
        }

        // 데이터베이스 초기화
        private void InitializeDatabase()
        {
            try
            {
                //string tempPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                // 디렉토리 확인 및 생성
                //string dbDir = Path.Combine(tempPath, "FinanceTool");
                string dbDir = GetValidDatabaseDirectory();
                if (!Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }

               


                string dbFileName = $"FinanceTool_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                _dbFilePath = Path.Combine(dbDir, dbFileName);

                Debug.WriteLine($"SQLite 데이터베이스 파일 경로: {_dbFilePath}");

                // SQLite 데이터베이스 생성
                SQLiteConnection.CreateFile(_dbFilePath);

                // 이전 데이터베이스 파일 삭제
                CleanupOldDatabaseFiles(dbDir);

                // 연결 문자열 생성 및 연결
                string connectionString = $"Data Source={_dbFilePath};Version=3;";
                _connection = new SQLiteConnection(connectionString);
                _connection.Open();

                // SQLite 최적화 설정
                ExecuteNonQueryInternal("PRAGMA journal_mode = WAL");
                ExecuteNonQueryInternal("PRAGMA synchronous = NORMAL");
                ExecuteNonQueryInternal("PRAGMA cache_size = 10000");
                ExecuteNonQueryInternal("PRAGMA temp_store = MEMORY");

                // 기본 테이블 생성
                CreateBaseTables();

                Debug.WriteLine("SQLite 데이터베이스 초기화 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"데이터베이스 초기화 오류: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private string GetValidDatabaseDirectory()
        {
            // 기본 경로 시도
            string primaryPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dbDir = Path.Combine(primaryPath, "FinanceTool");

            // 기본 경로를 사용할 수 있는지 테스트
            try
            {
                if (!Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }

                // 테스트 파일 쓰기 시도
                string testFile = Path.Combine(dbDir, "test.tmp");
                File.WriteAllText(testFile, "Test");
                File.Delete(testFile);

                return dbDir;
            }
            catch
            {
                // 기본 경로 사용 불가 - 대체 경로 시도
                try
                {
                    // 애플리케이션 실행 경로에 대체 폴더 시도
                    string exePath = Path.GetDirectoryName(Application.ExecutablePath);
                    string fallbackDir = Path.Combine(exePath, "Data");

                    if (!Directory.Exists(fallbackDir))
                    {
                        Directory.CreateDirectory(fallbackDir);
                    }

                    // 테스트 파일 쓰기 시도
                    string testFile = Path.Combine(fallbackDir, "test.tmp");
                    File.WriteAllText(testFile, "Test");
                    File.Delete(testFile);

                    Debug.WriteLine("기본 LocalAppData 경로를 사용할 수 없어 대체 경로를 사용합니다: " + fallbackDir);
                    return fallbackDir;
                }
                catch (Exception ex)
                {
                    // 모든 대체 경로도 실패
                    Debug.WriteLine("모든 데이터베이스 경로 시도가 실패했습니다: " + ex.Message);
                    throw new IOException("데이터베이스를 저장할 유효한 경로를 찾을 수 없습니다.", ex);
                }
            }
        }

        private void CleanupOldDatabaseFiles(string dbDir)
        {
            try
            {
                // 현재 실행 중인 프로세스의 데이터베이스 파일 이름 가져오기
                string currentDbFileName = Path.GetFileName(_dbFilePath);

                // 디렉토리의 모든 .db 파일 가져오기
                string[] dbFiles = Directory.GetFiles(dbDir, "FinanceTool_*.db");

                Debug.WriteLine($"데이터베이스 디렉토리 정리 시작: {dbDir}");
                Debug.WriteLine($"총 {dbFiles.Length}개 파일 발견");

                foreach (string file in dbFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);

                        // 현재 사용 중인 파일은 건너뜀
                        if (fileName == currentDbFileName)
                        {
                            Debug.WriteLine($"현재 사용 중인 파일 건너뜀: {fileName}");
                            continue;
                        }

                        // 파일이 잠겨있는지 확인
                        FileInfo fileInfo = new FileInfo(file);
                        if (!IsFileLocked(fileInfo))
                        {
                            File.Delete(file);
                            Debug.WriteLine($"이전 데이터베이스 파일 삭제: {fileName}");
                        }
                        else
                        {
                            Debug.WriteLine($"파일이 사용 중이라 삭제할 수 없습니다: {fileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // 개별 파일 삭제 실패는 전체 프로세스를 중단시키지 않음
                        Debug.WriteLine($"파일 삭제 중 오류 발생: {ex.Message}, 파일: {file}");
                    }
                }

                Debug.WriteLine("데이터베이스 디렉토리 정리 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"데이터베이스 파일 정리 중 오류 발생: {ex.Message}");
                // 정리 실패가 전체 초기화 과정을 중단시키지는 않도록 예외를 다시 throw하지 않음
            }
        }

        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                // 파일이 이미 다른 프로세스에 의해 열려 있음
                return true;
            }

            return false;
        }

        // 연결 상태 확인 및 재연결
        private bool EnsureConnectionOpen()
        {
            if (!_isInitialized || _connection == null)
                return false;

            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"데이터베이스 연결 오류: {ex.Message}");
                return false;
            }
        }

        // 내부 사용 NonQuery 실행 (초기화 중에 사용)
        private int ExecuteNonQueryInternal(string query)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        // 기본 테이블 생성
        private void CreateBaseTables()
        {
            // 세션 상태 테이블
            /*
            ExecuteNonQueryInternal(@"
            CREATE TABLE IF NOT EXISTS session_state (
                key TEXT PRIMARY KEY,
                value TEXT,
                last_updated DATETIME DEFAULT CURRENT_TIMESTAMP
            )");
            */

            // 컬럼 매핑 테이블
            ExecuteNonQueryInternal(@"
            CREATE TABLE IF NOT EXISTS column_mapping (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                original_name TEXT NOT NULL,
                display_name TEXT,
                data_type TEXT,
                is_visible INTEGER DEFAULT 1,
                sequence INTEGER,
                UNIQUE(original_name)
            )");
        }

        // 테이블 존재 여부 확인
        public bool TableExists(string tableName)
        {
            if (!EnsureInitialized() || !EnsureConnectionOpen())
                return false;

            try
            {
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
                using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    object result = cmd.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"테이블 존재 확인 오류: {ex.Message}");
                return false;
            }
        }

        // 비쿼리 실행 (CREATE, INSERT, UPDATE, DELETE 등)
        public int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            if (!EnsureInitialized() || !EnsureConnectionOpen())
                throw new InvalidOperationException("데이터베이스가 초기화되지 않았거나 연결을 열 수 없습니다.");

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                        }
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"쿼리 실행 오류: {ex.Message}\n쿼리: {query}");
                throw;
            }
        }

        // 단일 값 반환 쿼리
        public object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            if (!EnsureInitialized() || !EnsureConnectionOpen())
                throw new InvalidOperationException("데이터베이스가 초기화되지 않았거나 연결을 열 수 없습니다.");

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                        }
                    }
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"쿼리 실행 오류: {ex.Message}\n쿼리: {query}");
                throw;
            }
        }

        // DataTable로 결과 반환
        public DataTable ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            if (!EnsureInitialized() || !EnsureConnectionOpen())
                throw new InvalidOperationException("데이터베이스가 초기화되지 않았거나 연결을 열 수 없습니다.");

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                        }
                    }

                    DataTable dataTable = new DataTable();
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"쿼리 실행 오류: {ex.Message}\n쿼리: {query}");
                throw;
            }
        }

        // 페이징 쿼리 실행
        public DataTable ExecutePagedQuery(string baseQuery, string countQuery, int pageNumber, int pageSize,
            Dictionary<string, object> parameters = null)
        {
            if (!EnsureInitialized() || !EnsureConnectionOpen())
                throw new InvalidOperationException("데이터베이스가 초기화되지 않았거나 연결을 열 수 없습니다.");

            try
            {
                int offset = (pageNumber - 1) * pageSize;
                string query = $"{baseQuery} LIMIT {pageSize} OFFSET {offset}";

                DataTable result = ExecuteQuery(query, parameters);

                // 전체 행 수 가져오기
                object totalRowsObj = ExecuteScalar(countQuery, parameters);
                int totalRows = Convert.ToInt32(totalRowsObj);

                // 총 페이지 수와 현재 페이지 등의 메타데이터 추가
                DataTable metaData = new DataTable();
                metaData.Columns.Add("TotalRows", typeof(int));
                metaData.Columns.Add("TotalPages", typeof(int));
                metaData.Columns.Add("CurrentPage", typeof(int));
                metaData.Columns.Add("PageSize", typeof(int));

                int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);
                metaData.Rows.Add(totalRows, totalPages, pageNumber, pageSize);

                // 메타데이터를 ExtendedProperties에 추가
                result.ExtendedProperties["Paging"] = metaData;

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"페이징 쿼리 실행 오류: {ex.Message}");
                throw;
            }
        }

        // 트랜잭션 시작
        public SQLiteTransaction BeginTransaction()
        {
            if (!EnsureInitialized() || !EnsureConnectionOpen())
                throw new InvalidOperationException("데이터베이스가 초기화되지 않았거나 연결을 열 수 없습니다.");

            return _connection.BeginTransaction();
        }

        // 테이블이 존재하면 삭제
        public void DropTableIfExists(string tableName)
        {
            if (!EnsureInitialized() || !EnsureConnectionOpen())
                throw new InvalidOperationException("데이터베이스가 초기화되지 않았거나 연결을 열 수 없습니다.");

            if (TableExists(tableName))
            {
                ExecuteNonQuery($"DROP TABLE {tableName}");
                Debug.WriteLine($"테이블 '{tableName}' 삭제됨");
            }
        }

        // Dispose 패턴 구현
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 관리되는 리소스 해제
                    if (_connection != null)
                    {
                        if (_connection.State == ConnectionState.Open)
                        {
                            _connection.Close();
                        }
                        _connection.Dispose();
                        _connection = null;
                    }

                    // 임시 데이터베이스 파일 삭제
                    try
                    {
                        if (File.Exists(_dbFilePath))
                        {
                            File.Delete(_dbFilePath);
                            Debug.WriteLine($"임시 데이터베이스 파일 삭제됨: {_dbFilePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"임시 파일 삭제 중 오류: {ex.Message}");
                    }
                }

                _disposed = true;
                _isInitialized = false;
            }
        }

        public void ManualDispose()
        {
            lock (_lockObj)
            {
                if (_isInitialized && !_disposed)
                {
                    Debug.WriteLine("DBManager 수동 정리 시작...");
                    Dispose(true);
                    Debug.WriteLine("DBManager 수동 정리 완료");
                }
                else
                {
                    Debug.WriteLine("DBManager는 이미 정리되었거나 초기화되지 않았습니다.");
                }
            }
        }

        ~DBManager()
        {
            Dispose(false);
        }
    }
}
