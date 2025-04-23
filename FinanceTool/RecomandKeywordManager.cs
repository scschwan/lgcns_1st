using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Unicode;


namespace FinanceTool
{
    // Lv1 항목 클래스 - 각 Lv1 항목과 그에 종속된 키워드 목록을 가짐
    internal class Lv1Item
    {
        public string Name { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();

        // 매개변수가 없는 기본 생성자 추가
        public Lv1Item()
        {
            Name = string.Empty;
            Keywords = new List<string>();
        }

        public Lv1Item(string name)
        {
            Name = name;
        }

        public Lv1Item(string name, List<string> keywords)
        {
            Name = name;
            Keywords = keywords ?? new List<string>();
        }
    }

    internal class RecomandKeywordManager
    {
        private const string ConfigFileName = "recomand_config.json";
        private ConfigData _config;

        // Lv1 항목 목록 속성
        public List<Lv1Item> Lv1Items => _config.Lv1Items;

        // 설정 데이터 클래스
        private class ConfigData
        {
            public List<Lv1Item> Lv1Items { get; set; } = new List<Lv1Item>();
        }

        public RecomandKeywordManager()
        {
            LoadConfig();
        }

        // 설정 파일 로드
        private void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFileName))
                {
                    string jsonContent = File.ReadAllText(ConfigFileName);

                    // JSON 역직렬화 옵션 설정
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true,
                        PropertyNameCaseInsensitive = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    };

                    try
                    {
                        _config = JsonSerializer.Deserialize<ConfigData>(jsonContent, options);

                        // 역직렬화 결과 확인
                        if (_config == null || _config.Lv1Items == null)
                        {
                            Debug.WriteLine("설정 파일 로드 후 데이터가 null입니다. 기본값으로 초기화합니다.");
                            _config = new ConfigData();
                            InitializeDefaultValues();
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        Debug.WriteLine($"JSON 형식 오류: {jsonEx.Message}");
                        // JSON 오류 시 백업 파일 생성
                        CreateBackupFile(jsonContent);
                        // 기본값으로 초기화
                        _config = new ConfigData();
                        InitializeDefaultValues();
                    }
                }
                else
                {
                    Debug.WriteLine("설정 파일이 존재하지 않습니다. 기본값으로 초기화합니다.");
                    _config = new ConfigData();
                    InitializeDefaultValues();
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"설정 파일 로드 중 오류: {ex.Message}");
                Debug.WriteLine($"스택 트레이스: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"내부 예외: {ex.InnerException.Message}");
                }

                _config = new ConfigData();
                InitializeDefaultValues();
            }

            // 로드된 데이터 디버그 출력
            Debug.WriteLine($"로드된 Lv1 항목 수: {_config.Lv1Items.Count}");
            foreach (var item in _config.Lv1Items)
            {
                Debug.WriteLine($"  Lv1 항목: {item.Name}, 키워드 수: {item.Keywords.Count}");
            }
        }


        private void CreateBackupFile(string content)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"{Path.GetFileNameWithoutExtension(ConfigFileName)}_{timestamp}.bak";
                File.WriteAllText(backupFileName, content);
                Debug.WriteLine($"손상된 설정 파일 백업 생성: {backupFileName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"백업 파일 생성 중 오류: {ex.Message}");
            }
        }


        // 저장
        public void SaveConfig()
        {
            try
            {
                // JSON 직렬화 옵션 설정
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };

                string jsonContent = JsonSerializer.Serialize(_config, options);
                File.WriteAllText(ConfigFileName, jsonContent);
                Debug.WriteLine("설정 파일 저장 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"설정 파일 저장 중 오류: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"내부 예외: {ex.InnerException.Message}");
                }
            }
        }

        // 기본값 설정
        private void InitializeDefaultValues()
        {
            // 기본 Lv1 항목 및 각각의 고유 키워드 설정
            var defaultItems = new List<Lv1Item>
            {
                new Lv1Item("대행", new List<string>
                {
                    "대행용역", "대행비", "서비스대행", "마케팅대행", "업무대행"
                }),

                new Lv1Item("용역", new List<string>
                {
                    "기술용역", "개발용역", "컨설팅용역", "전문용역", "IT용역"
                }),

                new Lv1Item("집행", new List<string>
                {
                    "예산집행", "광고집행", "비용집행", "자금집행", "예산사용"
                })
            };

            _config.Lv1Items = defaultItems;
        }

        // Lv1 항목 추가
        public void AddLv1Item(string name)
        {
            // 중복 확인
            if (!_config.Lv1Items.Any(item => item.Name == name))
            {
                _config.Lv1Items.Add(new Lv1Item(name));
                SaveConfig();
            }
        }

        // Lv1 항목 삭제 (인덱스 기반)
        public bool RemoveLv1ItemByIndex(int index)
        {
            Debug.WriteLine($"Removing Lv1 item by index: {index}");

            if (index >= 0 && index < _config.Lv1Items.Count)
            {
                string removedItemName = _config.Lv1Items[index].Name; // 로그 용도
                _config.Lv1Items.RemoveAt(index);
                Debug.WriteLine($"Successfully removed Lv1 item at index {index}: '{removedItemName}'");
                SaveConfig();
                return true;
            }

            Debug.WriteLine($"Failed to remove Lv1 item. Invalid index: {index}");
            return false;
        }

        // Lv1 항목 삭제 (이름 기반)
        public bool RemoveLv1Item(string name)
        {
            Debug.WriteLine($"Removing Lv1 item by name: '{name}'");

            int index = _config.Lv1Items.FindIndex(item => item.Name == name);
            if (index != -1)
            {
                return RemoveLv1ItemByIndex(index);
            }

            Debug.WriteLine($"Failed to remove Lv1 item. Item not found: '{name}'");
            return false;
        }

       

        // 특정 Lv1 항목에 키워드 추가 (이름으로)
        public bool AddKeyword(string lv1Name, string keyword)
        {
            var lv1Item = _config.Lv1Items.FirstOrDefault(item => item.Name == lv1Name);
            if (lv1Item != null)
            {
                if (!lv1Item.Keywords.Contains(keyword))
                {
                    lv1Item.Keywords.Add(keyword);
                    SaveConfig();
                }
                return true;
            }
            return false;
        }

        // 특정 Lv1 항목에서 키워드 삭제 (인덱스 기반)
        public bool RemoveKeywordByIndex(int lv1Index, int keywordIndex)
        {
            if (lv1Index >= 0 && lv1Index < _config.Lv1Items.Count)
            {
                var lv1Item = _config.Lv1Items[lv1Index];
                if (keywordIndex >= 0 && keywordIndex < lv1Item.Keywords.Count)
                {
                    lv1Item.Keywords.RemoveAt(keywordIndex);
                    SaveConfig();
                    return true;
                }
            }
            return false;
        }

        // 특정 Lv1 항목에서 키워드 삭제 (값 기반)
        public bool RemoveKeyword(int lv1Index, string keyword)
        {
            if (lv1Index >= 0 && lv1Index < _config.Lv1Items.Count)
            {
                var lv1Item = _config.Lv1Items[lv1Index];
                int keywordIndex = lv1Item.Keywords.IndexOf(keyword);
                if (keywordIndex != -1)
                {
                    return RemoveKeywordByIndex(lv1Index, keywordIndex);
                }
            }
            return false;
        }

        // 특정 Lv1 항목에서 키워드 삭제 (이름과 값 기반)
        public bool RemoveKeyword(string lv1Name, string keyword)
        {
            int lv1Index = _config.Lv1Items.FindIndex(item => item.Name == lv1Name);
            if (lv1Index != -1)
            {
                return RemoveKeyword(lv1Index, keyword);
            }
            return false;
        }

        // Lv1 항목 가져오기 (인덱스 기반)
      

        // Lv1 항목 가져오기 (이름 기반)
        public Lv1Item GetLv1Item(string name)
        {
            return _config.Lv1Items.FirstOrDefault(item => item.Name == name);
        }

        // 이전 API와의 호환성을 위한 메서드들

        // 레거시 지원: 모든 Lv1 이름 목록 가져오기
        public List<string> Lv1List => _config.Lv1Items.Select(item => item.Name).ToList();

       

     

     
    }
}