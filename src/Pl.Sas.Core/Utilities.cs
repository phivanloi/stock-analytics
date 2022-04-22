﻿using Pl.Sas.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
        /// Xây dựng cây danh sách
        /// </summary>
        /// <typeparam name="T">Loại object</typeparam>
        /// <typeparam name="K">Thuộc tính object</typeparam>
        /// <param name="collection">Danh sách cần tạo tree</param>
        /// <param name="idSelector">Thuộc tính id của object</param>
        /// <param name="parentIdSelector">Thuộc tính cha id</param>
        /// <param name="orderSelector">Thuộc tính xắp xếp thứ tự</param>
        /// <param name="rootId">Giá trị gốc</param>
        /// <param name="selector">Điều kiện lựa chọn nếu có</param>
        public static IEnumerable<TreeItem<T>> GenerateTree<T, K, O>(this IEnumerable<T> collection, Func<T, K> idSelector, Func<T, K> parentIdSelector, Func<T, O> orderSelector, K rootId = default, Func<T, bool> selector = null)
        {
            foreach (T c in collection.Where(c => parentIdSelector(c).Equals(rootId)).OrderBy(q => orderSelector(q)))
            {
                if (selector is null || selector(c))
                {
                    yield return new TreeItem<T>
                    {
                        Item = c,
                        Children = collection.GenerateTree(idSelector, parentIdSelector, orderSelector, idSelector(c))
                    };
                }
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
        /// hàm kiểm tra sự tăng giảm và tính % tăng trưởng một tập hợp phần tử, hàm chạy từ phần tử đẩu đến phần tử cuối cùng
        /// </summary>
        /// <typeparam name="T">Loại phần tử</typeparam>
        /// <param name="collection">Tập hợp phần tử</param>
        /// <param name="valueCheck">trường dữ liệu cần check</param>
        /// <returns>
        /// <para>EqualCount số điểm bằng nhau</para>
        /// <para>IncreaseCount số điểm tăng thêm</para>
        /// <para>ReductionCount sổ điểm giảm đi</para>
        /// <para>FistEqualCount số điểm bằng nhau</para>
        /// <para>FistGrowCount số điểm tăng trưởng liên tục</para>
        /// <para>FistDropCount số điểm giảm liên tục</para>
        /// <para>Percents Phàn trăm tăng giảm</para>
        /// </returns>
        public static (int EqualCount, int IncreaseCount, int ReductionCount, int FistEqualCount, int FistGrowCount, int FistDropCount, List<decimal> Percents) GetFluctuationsTopDown<T>
            (this List<T> collection, Func<T, decimal> valueCheck)
        {
            var equalCount = 0;
            var increaseCount = 0;
            var reductionCount = 0;
            var fistEqualCount = 0;
            var fistGrowCount = 0;
            var fistDropCount = 0;
            var percents = new List<decimal>();
            var checkCount = collection.Count;
            for (int i = 0; i < checkCount; i++)
            {
                if ((i + 1) >= checkCount)
                {
                    break;
                }

                if (valueCheck(collection[i + 1]) != 0)
                {
                    percents.Add(valueCheck(collection[i]).GetPercent(valueCheck(collection[i + 1])));
                }
                else
                {
                    if (valueCheck(collection[i]) > 0)
                    {
                        percents.Add(100);
                    }
                    else if (valueCheck(collection[i]) == 0)
                    {
                        percents.Add(0);
                    }
                    else
                    {
                        percents.Add(-100);
                    }
                }

                if (valueCheck(collection[i]) > valueCheck(collection[i + 1]))
                {
                    increaseCount++;
                    if (reductionCount == 0 && equalCount == 0)
                    {
                        fistGrowCount++;
                    }
                }
                else if (valueCheck(collection[i]) < valueCheck(collection[i + 1]))
                {
                    reductionCount++;
                    if (increaseCount == 0 && equalCount == 0)
                    {
                        fistDropCount++;
                    }
                }
                else
                {
                    equalCount++;
                    if (fistDropCount == 0 && increaseCount == 0)
                    {
                        fistEqualCount++;
                    }
                }
            }
            return (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents);
        }

        /// <summary>
        /// hàm kiểm tra sự tăng giảm và tính % tăng trưởng một tập hợp phần tử, hàm chạy từ phần tử cuối cho đến phần tử đầu
        /// </summary>
        /// <typeparam name="T">Loại phần tử</typeparam>
        /// <param name="collection">Tập hợp phần tử</param>
        /// <param name="valueCheck">trường dữ liệu cần check</param>
        /// <returns>
        /// <para>EqualCount số điểm bằng nhau</para>
        /// <para>IncreaseCount số điểm tăng thêm</para>
        /// <para>ReductionCount sổ điểm giảm đi</para>
        /// <para>FistEqualCount số điểm bằng nhau</para>
        /// <para>FistGrowCount số điểm tăng trưởng liên tục</para>
        /// <para>FistDropCount số điểm giảm liên tục</para>
        /// <para>Percents Phàn trăm tăng giảm</para>
        /// </returns>
        public static (int EqualCount, int IncreaseCount, int ReductionCount, int FistEqualCount, int FistGrowCount, int FistDropCount, List<decimal> Percents) GetFluctuationsDownTop<T>
            (this List<T> collection, Func<T, decimal> valueCheck)
        {
            var equalCount = 0;
            var increaseCount = 0;
            var reductionCount = 0;
            var fistEqualCount = 0;
            var fistGrowCount = 0;
            var fistDropCount = 0;
            var percents = new List<decimal>();
            for (int i = (collection.Count - 1); i >= 0; i--)
            {
                if (i == 0)
                {
                    break;
                }

                if (valueCheck(collection[i - 1]) != 0)
                {
                    percents.Add(valueCheck(collection[i]).GetPercent(valueCheck(collection[i - 1])));
                }
                else
                {
                    if (valueCheck(collection[i]) > 0)
                    {
                        percents.Add(100);
                    }
                    else if (valueCheck(collection[i]) == 0)
                    {
                        percents.Add(0);
                    }
                    else
                    {
                        percents.Add(-100);
                    }
                }

                if (valueCheck(collection[i]) > valueCheck(collection[i - 1]))
                {
                    increaseCount++;
                    if (reductionCount == 0 && equalCount == 0)
                    {
                        fistGrowCount++;
                    }
                }
                else if (valueCheck(collection[i]) < valueCheck(collection[i - 1]))
                {
                    reductionCount++;
                    if (increaseCount == 0 && equalCount == 0)
                    {
                        fistDropCount++;
                    }
                }
                else
                {
                    equalCount++;
                    if (fistDropCount == 0 && increaseCount == 0)
                    {
                        fistEqualCount++;
                    }
                }
            }
            return (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents);
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
        /// Lấy ngày giao dịch hiện tại
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
        /// Lấy ngày giao dịch cuói cùng không phải ngày giao dịch hiện tại
        /// </summary>
        /// <returns>DateTime</returns>
        public static DateTime GetLastTradingDate()
        {
            if (DayOfWeek.Sunday == DateTime.Now.DayOfWeek)
            {
                return DateTime.Now.Date.AddDays(-3);
            }
            if (DayOfWeek.Monday == DateTime.Now.DayOfWeek)
            {
                return DateTime.Now.Date.AddDays(-4);
            }
            return DateTime.Now.Date.AddDays(-2);
        }

        /// <summary>
        /// Lấy chuỗi đường dẫn cho ngày giao dịch hiện tại
        /// </summary>
        /// <param name="value">Giá trị cần lấy nếu null thì lấy của hàm GetTradingDate</param>
        /// <returns></returns>
        public static string GetTradingDatePath(DateTime? value = null) => (value ?? GetTradingDate()).ToString("ddMMyyyy");

        /// <summary>
        /// Lấy chuỗi đường dẫn cho ngày giao dịch cuối cùng không phải ngày hiện tại
        /// </summary>
        /// <param name="value">Giá trị cần lấy nếu null thì lấy của hàm GetLastTradingDate</param>
        /// <returns>string</returns>
        public static string GetLastTradingDatePath(DateTime? value = null) => (value ?? GetLastTradingDate()).ToString("ddMMyyyy");

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
        /// Chuyển dữ liệu lịch sử giá bình thường thành dữ liệu lịch sử giá điều chỉnh
        /// </summary>
        /// <param name="stockPrice">Dữ liệu lịch sử giá bình thường</param>
        /// <returns>StockPriceAdj</returns>
        public static StockPriceAdj ToStockPriceAdj(this StockPrice stockPrice)
        {
            var changePercent = (stockPrice.ClosePrice - stockPrice.ClosePriceAdjusted) / stockPrice.ClosePrice;
            return new()
            {
                Symbol = stockPrice.Symbol,
                TradingDate = stockPrice.TradingDate,
                DatePath = stockPrice.DatePath,
                OpenPrice = stockPrice.OpenPrice - (stockPrice.OpenPrice * changePercent),
                HighestPrice = stockPrice.HighestPrice - (stockPrice.HighestPrice * changePercent),
                LowestPrice = stockPrice.LowestPrice - (stockPrice.LowestPrice * changePercent),
                ClosePrice = stockPrice.ClosePriceAdjusted,
                TotalMatchVol = stockPrice.TotalMatchVol
            };
        }

        /// <summary>
        /// Chuyển lịch sử khớp lệnh từ dữ liệu ban đầu sang dữ liệu sau điều chỉnh phục vụ cho quá trình trading
        /// </summary>
        /// <param name="stockTransactionDetails">Dữ liệu nén của phiên giao dịch</param>
        /// <param name="stockPrice">Dữ liệu lịch sử giá của phiện giao dịch tương ứng</param>
        /// <returns>StockPriceAdj</returns>
        public static List<StockTransactionDetails> RebuildStockTransactionDetails(this List<StockTransactionDetails> stockTransactionDetails, StockPrice stockPrice)
        {
            var changePercent = (stockPrice.ClosePrice - stockPrice.ClosePriceAdjusted) / stockPrice.ClosePrice;
            foreach (var stockTransactionDetail in stockTransactionDetails)
            {
                stockTransactionDetail.Price -= (stockTransactionDetail.Price * changePercent);
                stockTransactionDetail.RefPrice -= (stockTransactionDetail.RefPrice * changePercent);
            }
            return stockTransactionDetails;
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