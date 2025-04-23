using System;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;

namespace FinanceTool
{
    internal class TrialManager
    {

        // 만료 일자를 리터럴로 지정 (예: 2025년 3월 31일)
        private static readonly DateTime ExpirationDate = new DateTime(2025, 9, 30);

        // 시간 API URL
        // 한국 지역 기준 시간 API URL
        private const string TimeApiUrl = "http://worldtimeapi.org/api/timezone/Asia/Seoul";
        //private const string TimeApiUrl = "http://worldclockapi.com/api/json/utc/now";

        // 프로그램 시작 시 호출하는 메서드
        public async Task CheckTrial()
        {
            try
            {
                // 인터넷에서 현재 시간 가져오기
                DateTime? currentTime = await GetOnlineTime();

                if (currentTime  == null)
                {
                    // 만료된 경우
                    MessageBox.Show("프로그램 사용을 위해 인터넷 연결이 필요합니다. 인터넷 연결을 확인해주세요.",
                                   "인터넷 연결 확인", MessageBoxButtons.OK, MessageBoxIcon.Stop);

                    // 프로그램 종료
                    Environment.Exit(0);
                }
                Debug.WriteLine($"프로그램 일자 : {currentTime} , 만료일자 : {ExpirationDate}");
                // 인터넷 연결이 안 되면 로컬 시간 사용 (보안상 더 좋은 접근법은 인터넷 연결 필수로 하는 것)
                DateTime timeToCheck = currentTime ?? DateTime.Now;

                // 만료 일자와 비교
                if (timeToCheck > ExpirationDate)
                {
                    // 만료된 경우
                    MessageBox.Show("이 프로그램의 평가판 기간이 만료되었습니다. 구매를 위해 제작자에게 연락해주세요.",
                                   "평가판 만료", MessageBoxButtons.OK, MessageBoxIcon.Stop);

                    // 프로그램 종료
                    Environment.Exit(0);
                }
                else
                {
                    // 만료되지 않은 경우, 남은 일수 계산
                    int daysLeft = (ExpirationDate - timeToCheck).Days;

                    // 사용자에게 남은 일수 알림
                    MessageBox.Show($"평가판 기간이 {daysLeft}일 남았습니다.",
                                   "평가판 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // 오류 발생 시 처리
                MessageBox.Show($"평가판 확인 중 오류가 발생했습니다: {ex.Message}",
                               "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task<DateTime?> GetGoogleDateTime()

        {
            //리턴 할 날짜 선언
            DateTime dateTime = DateTime.MinValue;
            try
            {
                //WebRequest 객체로 구글사이트 접속 해당 날짜와 시간을 로컬 형태의 포맷으로 리턴 일자에 담는다.
                using (var response = WebRequest.Create("http://www.google.com").GetResponse())
                    dateTime = DateTime.ParseExact(response.Headers["date"],
                        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                        CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.AssumeUniversal);
            }
            catch (Exception ex)
            {
                //오류 발생시 로컬 날짜그대로 리턴
                //dateTime = DateTime.Now;
                Debug.WriteLine("GetGoogleDateTime Exception");
                Debug.WriteLine(ex.Message);
                return null;
            }

            return dateTime;
        }

        // 온라인 시간 가져오기
        private async Task<DateTime?> GetOnlineTime()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 타임아웃 설정 (5초)
                    client.Timeout = TimeSpan.FromSeconds(5);

                    // API 응답 가져오기
                    HttpResponseMessage response = await client.GetAsync(TimeApiUrl);
                    
                    Debug.WriteLine("await client.GetAsync(TimeApiUrl) complete");


                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"API 응답: {jsonResponse}"); // 디버깅용

                        // 첫 번째 API 형식 시도 (worldtimeapi.org)
                        try
                        {
                            using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                            {
                                if (doc.RootElement.TryGetProperty("datetime", out JsonElement datetimeElement))
                                {
                                    string datetimeStr = datetimeElement.GetString();
                                    return DateTime.Parse(datetimeStr);
                                }
                            }
                        }
                        catch
                        {
                            // 두 번째 API 형식 시도 (worldclockapi.com)
                            try
                            {
                                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                                {
                                    if (doc.RootElement.TryGetProperty("currentDateTime", out JsonElement datetimeElement))
                                    {
                                        string datetimeStr = datetimeElement.GetString();
                                        return DateTime.Parse(datetimeStr);
                                    }
                                }
                            }
                            catch
                            {
                                Debug.WriteLine("두 번째 API 형식 파싱 실패");
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"response error : {response.StatusCode}");
                        Debug.WriteLine($"response error : {response.IsSuccessStatusCode}");
                        Debug.WriteLine($"response error : {response.Content}");

                        return await GetGoogleDateTime();
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"온라인 시간 조회 오류: {ex.Message}");
                //return null;
                return await GetGoogleDateTime();
            }
        }
    }
}
