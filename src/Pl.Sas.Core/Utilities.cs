using System.ComponentModel;
using System.Data;
using System.Net;
using System.Text;

namespace Pl.Sas.Core
{
    public static class Utilities
    {
        private static readonly object lockerWriteLog = new();
        private static readonly Random random = new();

        /// <summary>
        /// Hàm cắt ngắn một chuỗi
        /// Nếu nẻ một chữ thì bỏ chữ đó cho đến dấu khoảng cách cuối cùng
        /// </summary>
        /// <param name="sentence">Chuỗi cần cắt</param>
        /// <param name="len">Độ dài cần lấy</param>
        /// <param name="expanded"></param>
        /// <returns>Chuỗi cộng thêm sau khi cắt ngắn</returns>
        public static string TruncateString(string sentence, int len, string expanded = "...")
        {
            if (string.IsNullOrEmpty(sentence))
            {
                return string.Empty;
            }

            len -= expanded.Length;
            if (sentence.Length > len)
            {
                sentence = sentence[..len];
                int pos = sentence.LastIndexOf(' ');
                if (pos > 0)
                {
                    sentence = sentence[..pos];
                }

                return sentence + expanded;
            }
            return sentence;
        }

        /// <summary>
        /// Cắt một bỏ một đoạn ở cuối một chuỗi
        /// </summary>
        /// <param name="input">Chuỗi ban đầu</param>
        /// <param name="suffixToRemove">Chuỗi cần cắt bỏ</param>
        /// <param name="comparisonType">Loại so sánh chuỗi cắt bỏ</param>
        /// <returns>string</returns>
        public static string TrimEnd(this string input, string suffixToRemove, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(suffixToRemove)) return input;

            string result = input;
            while (result.EndsWith(suffixToRemove, comparisonType))
            {
                result = result[..(input.Length - suffixToRemove.Length)];
            }

            return result;
        }

        /// <summary>
        /// Cắt một bỏ một đoạn ở đầu một chuỗi
        /// </summary>
        /// <param name="input">Chuỗi ban đầu</param>
        /// <param name="perfixToRemove">Chuỗi cần cắt bỏ</param>
        /// <param name="comparisonType">Loại so sánh chuỗi cắt bỏ</param>
        /// <returns>string</returns>
        public static string TrimStart(this string input, string perfixToRemove, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(perfixToRemove)) return input;

            string result = input;
            while (result.StartsWith(perfixToRemove, comparisonType))
            {
                result = result[perfixToRemove.Length..];
            }

            return result;
        }

        /// <summary>
        /// Lấy thông tin định danh máy chủ
        /// </summary>
        /// <returns>string</returns>
        public static string IdentityServer()
        {
            var appLicationName = Environment.GetEnvironmentVariable("ApplicationId");
            if (!string.IsNullOrEmpty(appLicationName))
            {
                return appLicationName;
            }
            string lastNumberIp = "";
            var hostName = Dns.GetHostName();
            if (!string.IsNullOrEmpty(hostName))
            {
                var ips = Dns.GetHostAddresses(hostName);
                var query = ips.Select(q => q.ToString()).Where(q => q.StartsWith("192") || q.StartsWith("172"));
                lastNumberIp = string.Join(", ", query);
            }
            return lastNumberIp;
        }

        /// <summary>
        /// Generate short guid
        /// </summary>
        /// <returns>string</returns>
        public static string GenerateShortGuid()
        {
            return Shorter(Convert.ToBase64String(Guid.NewGuid().ToByteArray())).ToUpper();
            static string Shorter(string base64String)
            {
                base64String = base64String.Split('=')[0];
                base64String = base64String.Replace('+', Convert.ToChar(random.Next(65, 91)));
                base64String = base64String.Replace('/', Convert.ToChar(random.Next(65, 91)));
                return base64String;
            }
        }

        /// <summary>
        /// Ghi log ra màn hình console và log vào file
        /// log file theo ngày
        /// </summary>
        /// <param name="message">Nội dung log</param>
        /// <param name="color">Màu sắc ở màn hình console</param>
        public static void WriteConsole(this string message, ConsoleColor color = ConsoleColor.White)
        {
            try
            {
                lock (lockerWriteLog)
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Chuyển một danh sách object thành một data table
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="data">list item</param>
        /// <param name="excludeProperties">danh sách thuộc tính bị loại bỏ</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> data, params string[] excludeProperties)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new();
            foreach (PropertyDescriptor prop in properties)
            {
                if (!prop.IsReadOnly && (excludeProperties is null || !excludeProperties.Contains(prop.Name)))
                {
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    if (!prop.IsReadOnly && (excludeProperties is null || !excludeProperties.Contains(prop.Name)))
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }
                }
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// Sinh chuỗi tự động với độ dài được cố định
        /// </summary>
        /// <param name="size">Độ dài</param>
        /// <returns>string</returns>
        public static string RandomString(int size)
        {
            StringBuilder builder = new();
            Random random = new();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor((26 * random.NextDouble()) + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Lấy số lần tăng của một list các item, duyệt từ item đầu tiền đến cuối
        /// </summary>
        /// <typeparam name="T">Loại item</typeparam>
        /// <param name="collection">Tập hợp các item</param>
        /// <param name="valueCheck">Cách check</param>
        /// <returns></returns>
        public static (int EqualCount, int GrowCount, int DropCount) GetTrendCountTopDown<T>(this List<T> collection, Func<T, decimal> valueCheck)
        {
            var equalCount = 0;
            var growCount = 0;
            var dropCount = 0;
            var checkCount = collection.Count;
            for (int i = 0; i < checkCount; i++)
            {
                if ((i + 1) >= checkCount)
                {
                    break;
                }
                if (valueCheck(collection[i]) > valueCheck(collection[i + 1]))
                {
                    if (equalCount > 0 || dropCount > 0)
                    {
                        break;
                    }
                    growCount++;
                }
                else if (valueCheck(collection[i]) < valueCheck(collection[i + 1]))
                {
                    if (equalCount > 0 || growCount > 0)
                    {
                        break;
                    }
                    dropCount++;
                }
                else
                {
                    if (dropCount > 0 || growCount > 0)
                    {
                        break;
                    }
                    equalCount++;
                }
            }
            return (equalCount, growCount, dropCount);
        }

        /// <summary>
        /// Lấy ngày giao dịch hiện tại, quy tắc xử lý 
        /// </summary>
        /// <returns>DateTime</returns>
        public static DateTime GetTradingDate()
        {
            if (DayOfWeek.Sunday == DateTime.Now.DayOfWeek)
            {
                return DateTime.Now.Date.AddDays(-2);
            }
            if (DayOfWeek.Monday == DateTime.Now.DayOfWeek)
            {
                return DateTime.Now.Date.AddDays(-3);
            }
            return DateTime.Now.Date.AddDays(-1);
        }

        /// <summary>
        /// Lấy tên phương pháp đầu tư
        /// </summary>
        /// <param name="principle">Id phương pháp</param>
        /// <returns>string</returns>
        public static string GetPrincipleName(int principle = 1)
        {
            return principle switch
            {
                0 => "Mua và nắm giữ",
                1 => "Dài hạn",
                2 => "Trung hạn",
                3 => "Ngắn hạn",
                4 => "Thử nghiệm",
                _ => "Dài hạn",
            };
        }

        /// <summary>
        /// Hàm kiểm tra một giá trị có lớn hơn các phần tử trong danh sách không, nếu lớn hơn thì cộng điểm và check tiếp đến phần tử tiếp theo
        /// </summary>
        /// <param name="pointLadder">Danh sách tham điểm cần check đi kèm điểm cộng</param>
        /// <param name="checker">Phương pháp kiểm tra</param>
        /// <returns>Score, MaxValue</returns>
        public static (int Score, decimal MaxValue) GetPointLadder(Dictionary<decimal, int> pointLadder, Func<decimal, bool> checker)
        {
            var score = 0;
            var maxValue = 0M;
            foreach (var item in pointLadder)
            {
                if (checker(item.Key))
                {
                    score += item.Value;
                    maxValue = item.Key;
                }
                else
                {
                    break;
                }
            }
            return (score, maxValue);
        }

        /// <summary>
        /// Hàm so sánh giá trị đầu và giá trị sau kèm và trả ra điểm số so sánh với mốc % được chuyền vào
        /// </summary>
        /// <param name="value">Giá tri đầu</param>
        /// <param name="compeValue">Giá trị cần so sánh</param>
        /// <param name="milestonesPercent">Phần trăm biến động lớn</param>
        /// <param name="standardScore">Điẻm bình thường</param>
        /// <param name="maxScore">Điểm tối đa được cộng</param>
        /// <returns>int</returns>
        public static int GetScoreCompetition(float value, float compeValue, float milestonesPercent, int standardScore = 1, int maxScore = 2)
        {
            var changePercent = value.GetPercent(compeValue);
            if (value > compeValue)
            {
                if (changePercent > milestonesPercent)
                {
                    return maxScore;
                }
                else
                {
                    return standardScore;
                }
            }
            else if (value == compeValue)
            {
                return 0;
            }
            else
            {
                if (changePercent < -milestonesPercent)
                {
                    return -maxScore;
                }
                else
                {
                    return -standardScore;
                }
            }
        }

        /// <summary>
        /// Lấy chỉ số quan trọng tương ứng với sàn
        /// </summary>
        /// <param name="exchange">Mã sàn</param>
        /// <returns>index</returns>
        public static string GetLeadIndexByExchange(string exchange)
        {
            if (exchange.Equals("HOSE", StringComparison.InvariantCultureIgnoreCase))
            {
                return "Vnindex";
            }
            if (exchange.Equals("HNX", StringComparison.InvariantCultureIgnoreCase))
            {
                return "HNX";
            }
            if (exchange.Equals("Upcom", StringComparison.InvariantCultureIgnoreCase))
            {
                return "Upcom";
            }
            return "Vnindex";
        }
    }
}