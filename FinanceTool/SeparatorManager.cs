using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace FinanceTool
{
    public class SeparatorManager
    {
        private const string ConfigFileName = "separators_config.json";
        private ConfigData _config;

       

        // 구분자, 불용어 속성 (List 형태로 변경)
        public List<string> Separators => _config.Separators;
        public List<string> Removers => _config.Removers;

        // 설정 데이터 클래스 (HashSet에서 List로 변경)
        private class ConfigData
        {
            public List<string> Separators { get; set; } = new List<string>();
            public List<string> Removers { get; set; } = new List<string>();
        }

        public SeparatorManager()
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
                    _config = JsonSerializer.Deserialize<ConfigData>(jsonContent);
                }
                else
                {
                    _config = new ConfigData();
                    // 기본값 설정
                    InitializeDefaultValues();
                    // 최초 설정 파일 저장
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"설정 파일 로드 중 오류: {ex.Message}");
                _config = new ConfigData();
                InitializeDefaultValues();
            }
        }

        // 저장
        public void SaveConfig()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonContent = JsonSerializer.Serialize(_config, options);
                File.WriteAllText(ConfigFileName, jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"설정 파일 저장 중 오류: {ex.Message}");
            }
        }

        public List<string> getSeparators()
        {
            return _config.Separators;
        }

        public List<string> getRemover()
        {
            return _config.Removers;
        }

        // 기본값 설정
        private void InitializeDefaultValues()
        {
            // 구분자 기본값 (List 형태로 변경)
            _config.Separators = new List<string> { " ", ",", ".", "/", "(", ")", "*", "#", "~", "[", "]", "!", ":", "%", "-", "'", "&" };

            // 불용어 기본값 (List 형태로 변경)
            _config.Removers = new List<string> {
            // 월
            "12월", "11월", "10월", "9월", "8월", "7월", "6월", "5월", "4월", "3월", "2월", "1월",
            // 명수
            "9명", "8명", "7명", "6명", "5명", "4명", "3명", "2명", "1명", "0명",
            // 연도
            "1년", "2년", "3년", "4년", "5년", "6년", "7년", "8년", "9년",
            // 숫자
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };
        }

        // 구분자 추가
        public void AddSeparator(string separator)
        {
            // 중복 확인 후 추가 (HashSet과 달리 List는 중복을 허용하므로)
            if (!_config.Separators.Contains(separator))
            {
                _config.Separators.Add(separator);
                SaveConfig();
            }
        }

        // 구분자 삭제 (인덱스 기반으로 변경)
        public bool RemoveSeparatorByIndex(int index)
        {
            Debug.WriteLine($"Removing separator by index: {index}");

            if (index >= 0 && index < _config.Separators.Count)
            {
                string removedItem = _config.Separators[index]; // 로그 용도
                _config.Separators.RemoveAt(index);
                Debug.WriteLine($"Successfully removed separator at index {index}: '{removedItem}'");
                SaveConfig();
                return true;
            }

            Debug.WriteLine($"Failed to remove separator. Invalid index: {index}");
            return false;
        }

        // 구분자 삭제 (문자열 기반 - 하위 호환성 유지)
        public bool RemoveSeparator(string separator)
        {
            Debug.WriteLine($"Removing separator by value: '{separator}'");

            int index = _config.Separators.IndexOf(separator);
            if (index != -1)
            {
                return RemoveSeparatorByIndex(index);
            }

            Debug.WriteLine($"Failed to remove separator. Item not found: '{separator}'");
            return false;
        }

        // 불용어 추가
        public void AddRemover(string remover)
        {
            // 중복 확인 후 추가
            if (!_config.Removers.Contains(remover))
            {
                _config.Removers.Add(remover);
                SaveConfig();
            }
        }

        // 불용어 삭제 (인덱스 기반으로 변경)
        public bool RemoveRemoverByIndex(int index)
        {
            if (index >= 0 && index < _config.Removers.Count)
            {
                _config.Removers.RemoveAt(index);
                SaveConfig();
                return true;
            }
            return false;
        }

        // 불용어 삭제 (문자열 기반 - 하위 호환성 유지)
        public bool RemoveRemover(string remover)
        {
            int index = _config.Removers.IndexOf(remover);
            if (index != -1)
            {
                return RemoveRemoverByIndex(index);
            }
            return false;
        }

        // 구분자 가져오기 (인덱스 기반)
        public string GetSeparatorAt(int index)
        {
            if (index >= 0 && index < _config.Separators.Count)
            {
                return _config.Separators[index];
            }
            return null;
        }

        // 불용어 가져오기 (인덱스 기반)
        public string GetRemoverAt(int index)
        {
            if (index >= 0 && index < _config.Removers.Count)
            {
                return _config.Removers[index];
            }
            return null;
        }
    }
}
