using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Services
{
    public static class CompanyValueAnalyticsService
    {
        /// <summary>
        /// Đánh giá ngày thành lập của công ty
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty cần kiểm tra</param>
        /// <returns>
        /// Với công ty thành lập lớn hơn 10 năm thì được 3 điểm
        /// Với công ty thành lập lớn hơn 5 năm thì được 2 điểm
        /// Với công ty thành lập dưới 5 năm thì được 1 điểm
        /// </returns>
        public static int FoundingDateCheck(List<AnalyticsNote> notes, Company company)
        {
            if (company.FoundingDate is null)
            {
                return notes.Add($"Công ty không có thông tin ngày thành lập.", -1, -1);
            }

            var score = 0;
            var type = 0;
            var foundingYear = company.FoundingDate.Value.Year;

            if (DateTime.Now.Year - foundingYear >= 10)
            {
                type += 2;
                score += 2;
            }
            else if (DateTime.Now.Year - foundingYear > 3)
            {
                type++;
                score++;
            }
            return notes.Add($"Ngày thành lập công ty {company.FoundingDate.Value:dd/MM/yyyy}", score, type);
        }

        /// <summary>
        /// Đánh giá 
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialIndicatorsSameIndustries">Danh sách chỉ số tài chính của các công ty cùng ngành</param>
        /// <returns>int</returns>
        public static int RevenueCheck(List<AnalyticsNote> notes, Company company, List<FinancialIndicator> financialIndicatorsSameIndustries)
        {
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";
            var companyFinancialIndicators = financialIndicatorsSameIndustries.Where(q => q.Symbol == company.Symbol).ToList();
            if (companyFinancialIndicators.Count <= 0)
            {
                return notes.Add("Không có thông tin doanh thu để đánh giá.", -1, -2, guideLink);
            }

            var lastCompanyReport = companyFinancialIndicators.OrderByDescending(q => q.YearReport).FirstOrDefault(q => q.LengthReport == 5);
            if (lastCompanyReport is null || lastCompanyReport.YearReport < (DateTime.Now.Year - 1))
            {
                return notes.Add("Năm gần đây không có thông tin doanh thu.", -1, -1, guideLink);
            }

            var score = 0;
            var type = 0;
            var note = "";

            var avgList = new List<FinancialIndicator>();
            var groupSelect = financialIndicatorsSameIndustries.GroupBy(q => q.Symbol);
            foreach (var item in groupSelect)
            {
                var lastReport = item.OrderByDescending(q => q.YearReport).FirstOrDefault(q => q.LengthReport == 5);
                if (lastReport is not null)
                {
                    avgList.Add(lastReport);
                }
            }
            var averageRevenueAll = avgList.Average(q => q.Revenue);
            var averageRevenueAllAndTenPercent = averageRevenueAll + (averageRevenueAll * 0.1f);
            if (lastCompanyReport.Revenue > averageRevenueAllAndTenPercent)
            {
                note += $"Doanh thu công ty báo cáo gần nhất {lastCompanyReport.Revenue:#,###} lớn hơn trung bình ngành {averageRevenueAllAndTenPercent:#,###}(+10%)";
                score += 1;
                type++;
            }
            else
            {
                note += $"Doanh thu công ty báo cáo gần nhất {lastCompanyReport.Revenue:#,###} nhỏ hơn trung bình ngành {averageRevenueAllAndTenPercent:#,###}(+10%)";
                score -= 1;
                type--;
            }

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Đánh giá các công ty có lợi nhuận cao hơn trunh bình ngành + 10%
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialIndicatorsSameIndustries">Danh sách chỉ số tài chính của các công ty cùng ngành</param>
        /// <returns>int</returns>
        public static int ProfitCheck(List<AnalyticsNote> notes, Company company, List<FinancialIndicator> financialIndicatorsSameIndustries)
        {
            var companyFinancialIndicators = financialIndicatorsSameIndustries.Where(q => q.Symbol == company.Symbol).ToList();
            var guideLink = "https://vcbs.com.vn/vn/Utilities/Index/5";
            if (companyFinancialIndicators.Count <= 0)
            {
                return notes.Add("Không có thông tin lợi nhuận để đánh giá.", -1, -2, guideLink);
            }

            var lastCompanyReport = companyFinancialIndicators.OrderByDescending(q => q.YearReport).FirstOrDefault(q => q.LengthReport == 5);
            if (lastCompanyReport is null || lastCompanyReport.YearReport < (DateTime.Now.Year - 1))
            {
                return notes.Add("Năm gần đây không có thông tin doanh thu.", -1, -1, guideLink);
            }

            var score = 0;
            var type = 0;
            var note = "";

            var avgList = new List<FinancialIndicator>();
            var groupSelect = financialIndicatorsSameIndustries.GroupBy(q => q.Symbol);
            foreach (var item in groupSelect)
            {
                var lastReport = item.OrderByDescending(q => q.YearReport).FirstOrDefault(q => q.LengthReport == 5);
                if (lastReport is not null)
                {
                    avgList.Add(lastReport);
                }
            }
            var averageProfitAll = avgList.Average(q => q.Revenue);
            var averageProfitAllAndTenPercent = averageProfitAll + (averageProfitAll * 0.1f);
            if (lastCompanyReport.Profit > averageProfitAllAndTenPercent)
            {
                note += $"Lơi nhuận công ty báo cáo gần nhất {lastCompanyReport.Profit:#,###} lớn hơn trung bình ngành {averageProfitAllAndTenPercent:#,###}(+10%)";
                score += 1;
                type++;
            }
            else
            {
                note += $"Lợi nhuận công ty báo cáo gần nhất {lastCompanyReport.Profit:#,###} nhỏ hơn trung bình ngành {averageProfitAllAndTenPercent:#,###}(+10%)";
                score -= 1;
                type--;
            }

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Kiểm tra chỉ số ep quý gần nhất
        /// </summary>
        /// <param name="notes">Ghi chú của việc kiểm tra</param>
        /// <param name="lastQuarterlyFinancialIndicator">Chỉ số tài chính của quý gần nhất</param>
        /// <param name="marketPrice">Thị giá của phiên giao dịch gần nhất của cổ phiếu</param>
        /// <param name="bankInterestRate6">Lãi suất ngân hàng kỳ hạn 6 tháng(có thể dùng lãi xuất 3 tháng)</param>
        /// <returns>Tối đa được 1 điểm, thấp nhất được -1</returns>
        public static int EpLastQuarterlyCheck(List<AnalyticsNote> notes, FinancialIndicator? lastQuarterlyFinancialIndicator, float marketPrice, float bankInterestRate6)
        {
            if (lastQuarterlyFinancialIndicator is null)
            {
                return notes.Add("Không có thông tin tài chính quý gần nhất để kiểm tra ep.", -5, -2);
            }
            if (marketPrice <= 0)
            {
                return notes.Add($"Ep gần nhất không kiểm tra được do thị giá không hợp lệ {marketPrice:0.00}VNĐ.", -1, -1, null);
            }
            var ep = lastQuarterlyFinancialIndicator.Eps / marketPrice * 100;
            if ((float)ep > (bankInterestRate6 + 1))//Cộng 1% là tính toán đến các chi phí giao dịch, thuế, thuế cổ tức, phí lưu ký vvv
            {
                return notes.Add($"Ep quý gần nhất {ep:0.00}% lớn hơn lãi suất ngân hàng(cao nhất thời điểm kiểm tra) + 1% chi phí đầu tư {bankInterestRate6 + 1:0.00}%", 1, 1, null);
            }
            else
            {
                if ((float)ep > bankInterestRate6)
                {
                    return notes.Add($"Ep quý gần nhất {ep:0.00}% lớn hơn lãi suất ngân hàng(cao nhất thời điểm kiểm tra) mà chưa tính chi phí đầu tư {bankInterestRate6:0.00}%", 0, 0, null);
                }
                else
                {
                    return notes.Add($"Ep quý gần nhất {ep:0.00}% nhỏ hơn hoặc bằng lãi suất ngân hàng(cao nhất thời điểm kiểm tra) mà chưa tính chi phí đầu tư {bankInterestRate6:0.00}%", -1, -1, null);
                }
            }
        }

        /// <summary>
        /// Đánh giá chỉ số eps
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty càn kiểm tra</param>
        /// <param name="companiesSameIndustries">Danh sách các công ty cùng ngành.</param>
        /// <returns></returns>
        public static int EpsCheck(List<AnalyticsNote> notes, Company company, List<Company> companiesSameIndustries)
        {
            var guideLink = "https://govalue.vn/chi-so-eps/";
            var score = 1;
            var type = 1;
            if (company is null || companiesSameIndustries.Count <= 0)
            {
                return notes.Add("Không có thông tin chỉ số Eps.", -5, -2, guideLink);
            }

            var note = $"Chỉ số Eps hiện tại {company.Eps:0,0.00}.";
            if (company.Eps < 2000)
            {
                return notes.Add($"{note}. Nhỏ hơn {2000:0,0}.", -2, -1, guideLink);
            }
            else
            {
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {5000, 1 },
                    {8000, 1 },
                    {10000, 1 }
                }, (point) => company.Eps > point);
                score += pointLadderScore;
                note += $" Lớn hơn {pointLadderMaxValue:0,0}.";
            }

            var avgSameIndustryEpsAvg = companiesSameIndustries.Average(q => q.Eps);
            if (company.Eps > avgSameIndustryEpsAvg)
            {
                note += $" Lớn hơn trung bình ngành {avgSameIndustryEpsAvg:0,0}.";
                score++;
                type++;
            }
            else
            {
                note += $" Nhỏ hơn trung bình ngành {avgSameIndustryEpsAvg:0,0}.";
                score--;
                type--;
            }

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Đánh giá chỉ số pe
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty càn kiểm tra</param>
        /// <param name="companiesSameIndustries">Danh sách các công ty cùng ngành.</param>
        /// <returns></returns>
        public static int PeCheck(List<AnalyticsNote> notes, Company company, List<Company> companiesSameIndustries)
        {
            var guideLink = "https://vcbs.com.vn/vn/Utilities/Index/53";
            if (company is null || companiesSameIndustries.Count <= 0)
            {
                return notes.Add("Không có thông tin chỉ số Pe.", -5, -2, guideLink);
            }

            var score = 0;
            var type = 0;
            var note = $"Chỉ số Pe hiện tại {company.Pe:0,0.00}.";
            var avgSameIndustryPeAvg = companiesSameIndustries.Average(q => q.Pe);
            if (company.Pe >= avgSameIndustryPeAvg)
            {
                note += $" Lớn hơn hoặc bằng trung bình ngành {avgSameIndustryPeAvg:0,0}";
                score--;
                type--;
            }
            else
            {
                note += $" Nhỏ hơn trung bình ngành {avgSameIndustryPeAvg:0,0}";
                score++;
                type++;
            }

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Đánh giá chỉ số roe
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty càn kiểm tra</param>
        /// <param name="companiesSameIndustries">Danh sách các công ty cùng ngành</param>
        /// <returns></returns>
        public static int RoeCheck(List<AnalyticsNote> notes, Company company, List<Company> companiesSameIndustries)
        {
            var guideLink = "https://govalue.vn/chi-so-roe/";
            if (company is null || companiesSameIndustries.Count <= 0)
            {
                return notes.Add("Không có thông tin chỉ số Roe.", -10, -2, guideLink);
            }

            var score = 0;
            var type = 0;
            var note = $"Chỉ số Roe hiện tại {company.Roe * 100:0,0.00}%";
            if (company.Roe < 0.18f)
            {
                score--;
                type--;
                note += " nhỏ hơn 18%.";
            }
            else
            {
                note += " lớn hơn 18%.";
                score++;
                type++;
            }

            if (company.Roe > 0.25f)
            {
                score++;
                type++;
            }

            var avgRoeSameIndustryValue = companiesSameIndustries.Average(q => q.Roe);
            if (company.Roe > avgRoeSameIndustryValue)
            {
                note += $" Cao hơn bình quân ngành {avgRoeSameIndustryValue * 100:0,0.00}%.";
                score++;
                type++;
            }
            else if (company.Roe <= avgRoeSameIndustryValue)
            {
                note += $" Thấp hơn bình quân ngành {avgRoeSameIndustryValue * 100:0,0.00}%.";
                score--;
                type--;
            }

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Đánh giá chỉ số roe
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty càn kiểm tra</param>
        /// <param name="companiesSameIndustries">Danh sách các công ty cùng ngành</param>
        /// <returns></returns>
        public static int RoaCheck(List<AnalyticsNote> notes, Company company, List<Company> companiesSameIndustries)
        {
            var guideLink = "https://govalue.vn/chi-so-roe/";
            if (company is null || companiesSameIndustries.Count <= 0)
            {
                return notes.Add("Không có thông tin chỉ số Roa.", -10, -2, guideLink);
            }

            var score = 0;
            var type = 0;
            var note = $"Chỉ số Roa hiện tại {company.Roa * 100:0,0.00}%";
            if (company.Roa < 0.1f)
            {
                score--;
                type--;
                note += " nhỏ hơn 10%.";
            }
            else
            {
                note += " lớn hơn 10%.";
                score++;
                type++;
            }

            if (company.Roa > 0.2f)
            {
                score++;
                type++;
            }

            var avgRoaSameIndustryValue = companiesSameIndustries.Average(q => q.Roe);
            if (company.Roa > avgRoaSameIndustryValue)
            {
                note += $" Cao hơn bình quân ngành {avgRoaSameIndustryValue * 100:0,0.00}%.";
                score++;
                type++;
            }
            else if (company.Roa < avgRoaSameIndustryValue)
            {
                note += $" Thấp hơn bình quân ngành {avgRoaSameIndustryValue * 100:0,0.00}%.";
                score--;
                type--;
            }

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Kiểm tra chỉ số ep năm gần nhất
        /// </summary>
        /// <param name="notes">Ghi chú của việc kiểm tra</param>
        /// <param name="lastQuarterlyFinancialIndicator">eps của năm gần nhất</param>
        /// <param name="marketPrice">Thị giá của phiên giao dịch gần nhất của cổ phiếu</param>
        /// <param name="bankInterestRate12">Lãi suất ngân hàng kỳ hạn 12 tháng</param>
        /// <returns>int</returns>
        public static int EpLastYearlyCheck(List<AnalyticsNote> notes, FinancialIndicator? lastQuarterlyFinancialIndicator, float marketPrice, float bankInterestRate12)
        {
            if (lastQuarterlyFinancialIndicator is null)
            {
                return notes.Add("Không có thông tin tài chính năm gần nhất để kiểm tra ep.", -5, -2);
            }
            if (marketPrice <= 0)
            {
                return notes.Add($"Ep gần nhất không kiểm tra được do thị giá không hợp lệ {marketPrice:0.00}VNĐ.", -1, -1, null);
            }
            var ep = lastQuarterlyFinancialIndicator.Eps / marketPrice * 100;
            if ((float)ep > (bankInterestRate12 + 1))//Cộng 1% là tính toán đến các chi phí giao dịch, thuế, thuế cổ tức, phí lưu ký vvv
            {
                return notes.Add($"Ep năm gần nhất {ep:0.00}% lớn hơn lãi suất ngân hàng(cao nhất thời điểm kiểm tra) + 1% chi phí đầu tư {bankInterestRate12 + 1:0.00}%", 1, 1, null);
            }
            else
            {
                if ((float)ep > bankInterestRate12)
                {
                    return notes.Add($"Ep năm gần nhất {ep:0.00}% lớn hơn lãi suất ngân hàng(cao nhất thời điểm kiểm tra) mà chưa tính chi phí đầu tư {bankInterestRate12:0.00}%", 0, 0, null);
                }
                else
                {
                    return notes.Add($"Ep năm gần nhất {ep:0.00}% nhỏ hơn hoặc bằng lãi suất ngân hàng(cao nhất thời điểm kiểm tra) mà chưa tính chi phí đầu tư {bankInterestRate12:0.00}%", -1, -1, null);
                }
            }
        }

        /// <summary>
        /// Kiểm tra vốn hóa của công ty, Nêu nhỏ hơn trunh bình toàn thị trường thì bỏ qua, ngược lại lớn hơn thì chưck tiếp đén trung bình ngành.
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty cần đánh giá</param>
        /// <param name="companiesSameIndustries">Danh sách các công ty cùng ngành.</param>
        /// <param name="avgTotalMarketCap">Trung bình vốn hóa toàn thị trường.</param>
        /// <returns>
        /// Tối đa được 3 điểm, nhỏ nhất -1 điểm
        /// </returns>
        public static int MarketCapCheck(List<AnalyticsNote> notes, Company company, List<Company> companiesSameIndustries, float avgTotalMarketCap)
        {
            if (companiesSameIndustries is null || !companiesSameIndustries.Any())
            {
                return notes.Add("Không có các công ty cùng ngành để phânt tích vốn hóa.", -1, -1);
            }

            var type = 0;
            var score = 0;
            var note = $"Giá trị vốn hóa {company.MarketCap:0,0} VNĐ, ";
            var avgMarketCap = companiesSameIndustries.Average(q => q.MarketCap);
            var sumMarketCap = companiesSameIndustries.Sum(q => q.MarketCap);
            var avgPercent = (company.MarketCap - avgMarketCap) / Math.Abs(avgMarketCap) * 100;
            var sumPercent = Math.Abs(company.MarketCap * 100) / Math.Abs(sumMarketCap);
            note += $"chiếm {sumPercent:0,0.00} % vốn hóa toàn ngành, ";
            if (company.MarketCap < avgTotalMarketCap)
            {
                note += $"nhỏ hơn trung bình vốn hóa toàn thị trường({avgTotalMarketCap:0,0} VNĐ). ";
                score--;
                type = -1;
            }
            else
            {
                if (company.MarketCap > avgMarketCap)
                {
                    note += $"lớn hơn bình quân toàn ngành {avgPercent:0,0.00} %. ";
                    type = 1;
                    score++;
                }
                else
                {
                    note += $"nhỏ hơn bình quân toàn ngành {avgPercent:0,0.00} %. ";
                    type = 0;
                }
                if (sumPercent > 80)
                {
                    note += $"Lớn hơn 80% vốn hóa toàn ngành. ";
                    score += 2;
                    type = 2;
                }
                else if (sumPercent > 50)
                {
                    note += $"Lớn hơn 50% vốn hóa toàn ngành. ";
                    score++;
                    type = 2;
                }
            }

            return notes.Add(note, score, type, null);
        }

        /// <summary>
        /// Kiểm tra tỉ lệ chi trả cổ tức bằng tiền mặt. Mặc dù việc trả cổ tức bằng tiền mặt là rất tốt(thể hiện dòng tiền mặt của công ty tốt) tuy nhiên vẫn có một số công ty tốt không trả cổ tức.
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="bankInterestRate12">Lái suất ngân hàng cao nhất kỳ hạn 12 tháng</param>
        /// <param name="dividendMoney">Tỉ lệ trả cổ tức bằng tiền mặt</param>
        /// <returns>
        /// Tối đa được một điểm, nhỏ nhất -1 điểm.
        /// </returns>
        public static int DividendDivCheck(List<AnalyticsNote> notes, float bankInterestRate12, float dividendMoney)
        {
            var guideLink = "https://vietnamfinance.vn/ty-le-co-tuc-la-gi-20180504224215063.htm";
            if (dividendMoney <= 0)
            {
                return notes.Add($"Công ty không trả cổ tức bằng tiền năm gần đây", -1, -1, guideLink);
            }

            var score = 0;
            var dp = dividendMoney * 100;
            if (dp > bankInterestRate12)
            {
                if (dp > (bankInterestRate12 * 2))
                {
                    score += 2;
                }
                score++;
                return notes.Add($"Tỉ suất cổ tức bằng tiền mặt {dp:0.00}% lớn hơn lãi suất ngân hàng(cao nhất thời điểm kiểm tra) {bankInterestRate12:0.00}%", score, 1, guideLink);
            }
            else
            {
                return notes.Add($"Tỉ suất cổ tức bằng tiền mặt {dp:0.00}% nhỏ hơn hoặc bằng lãi suất ngân hàng(cao nhất thời điểm kiểm tra) {bankInterestRate12:0.00}%", score, 0, guideLink);
            }
        }

        /// <summary>
        /// Việc kiểm tra chỉ số p/b(p/BVPS) chỉ số này thấp hơn trung bình ngành thì tốt nhưng nó cũng thể hiện răng đó có thể là công ty kếm chất lượng, tương tự khi chỉ số này cao hơn 
        /// trunh bình ngành thì có thể công ti được kỳ vọng tốt hơn trong tương lai. Việc kiểm tra chỉ số này cần đi kèm theo các chỉ số khác nữa tạm thời kiểm tra độc lập
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="company"></param>
        /// <param name="companiesSameIndustries"></param>
        /// <param name="companies"></param>
        /// <returns></returns>
        public static int PbCheck(List<AnalyticsNote> notes, Company company, List<Company> companiesSameIndustries, List<Company> companies)
        {
            var guideLink = "https://vnexpress.net/chi-so-p-b-co-y-nghia-nhu-the-nao-2695409.html";
            if (!companiesSameIndustries.Any() || !companies.Any())
            {
                return notes.Add("Chỉ số Giá/Tải sản hữu hình (pb) không có thông tin để đánh giá và so sánh.", -2, -2, guideLink);
            }

            var note = $"Chỉ số pb {company.Pb:0.00} ";
            var score = 0;
            var type = 0;

            var avgPb = companies.Average(q => q.Pb);
            if (company.Pb < avgPb)
            {
                note += $" thấp hơn trung bình toàn thị trường {avgPb:0.00}, ";
                type++;
                score++;
            }
            else
            {
                note += $" lớn hơn trung bình toàn thị trường {avgPb:0.00}, ";
                type--;
                score--;
            }
            avgPb = companiesSameIndustries.Average(q => q.Pb);
            if (company.Pb < avgPb)
            {
                note += $" thấp hơn trung bình ngành {avgPb:0.00}.";
                type++;
                score++;
            }
            else
            {
                note += $" thấp lớn hơn trung bình ngành {avgPb:0.00}.";
                type--;
                score--;
            }

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Tỉ lệ thanh toán hiện hành, lớn hơn 1.2, lớn hơn trung bình ngành là tốt nếu lớn hơn 1.5 là rất tốt
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="lastQuarterlyFinancialIndicator">Chỉ số tài chính quý gần đây nhất</param>
        /// <param name="lastQuarterlyFinancialIndicatorSameIndustries">Danh sách chỉ số tài chính quý gần đây nhất của các công ty cùng ngành</param>
        /// <returns>
        /// Tối đa 3 điểm, thấp nhất -1 điểm
        /// </returns>
        public static int CurrentRatioCheck(List<AnalyticsNote> notes, FinancialIndicator? lastQuarterlyFinancialIndicator, List<FinancialIndicator> lastQuarterlyFinancialIndicatorSameIndustries)
        {
            var guideLink = "https://vcbs.com.vn/vn/Utilities/Index/53";
            if (lastQuarterlyFinancialIndicator is null)
            {
                return notes.Add("Công ty không có thông tin tài chính quý gần nhất.", -5, -2, guideLink);
            }

            var score = 0;
            var type = 0;
            var note = $"Tỉ lệ thanh toán hiện hành(Current Ratio) {lastQuarterlyFinancialIndicator.CurrentRatio:0,0}";
            if (lastQuarterlyFinancialIndicator.CurrentRatio <= 1.2f)
            {
                score--;
                type--;
                note += " nhỏ hơn hoặc bằng 1.2.";
                return notes.Add(note, score, type, guideLink);
            }
            else
            {
                score++;
                type++;
                note += ", lớn hơn 1.2";
                if (lastQuarterlyFinancialIndicatorSameIndustries?.Count > 0)
                {
                    var avgSameIndustries = lastQuarterlyFinancialIndicatorSameIndustries.Average(q => q.CurrentRatio);
                    if (avgSameIndustries > 1 && lastQuarterlyFinancialIndicator.CurrentRatio > avgSameIndustries)
                    {
                        score++;
                        type++;
                        note += $", lớn hơn trung bình ngành {avgSameIndustries: 0,0}";
                    }
                }
                if (lastQuarterlyFinancialIndicator.CurrentRatio >= 1.5f)
                {
                    score++;
                    type++;
                    note += $", lớn hơn 1.5 rât tốt";
                }
            }
            return notes.Add(note, score, type, null);
        }

        /// <summary>
        /// Kiểm tra tỉ trọng lợi nhuạn gộp phải lớn hơn 25%
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="lastQuarterlyFinancialIndicator">Chỉ số tài chính quý gần đây nhất</param>
        /// <param name="bankInterestRate12">lái suất ngân hàng trong 12 tháng</param>
        /// <returns>Tối đa được 2 điểm, thấp nhất -2 điểm</returns>
        public static int GrossProfitMarginCheck(List<AnalyticsNote> notes, FinancialIndicator? lastQuarterlyFinancialIndicator, float bankInterestRate12)
        {
            var guideLink = "https://vietnambiz.vn/ti-suat-loi-nhuan-gop-gross-profit-margin-la-gi-cong-thuc-xac-dinh-va-y-nghia-20191008164428741.htm";
            if (lastQuarterlyFinancialIndicator is null)
            {
                return notes.Add("Không có thông tin tỉ trọng lợi nhuận gộp \"GrossProfitMarginCheck\".", -10, -2, guideLink);
            }

            var grossProfitMarginPercent = lastQuarterlyFinancialIndicator.GrossProfitMargin * 100;
            if (grossProfitMarginPercent > 25)
            {
                if (grossProfitMarginPercent > 35)
                {
                    return notes.Add($"Tỉ trọng lợi nhuận gộp {lastQuarterlyFinancialIndicator.GrossProfitMargin:0,0.00}% lớn hơn 35%", 2, 2, guideLink);
                }
                else
                {
                    return notes.Add($"Tỉ trọng lợi nhuận gộp {lastQuarterlyFinancialIndicator.GrossProfitMargin:0,0.00}% lớn hơn 25%", 1, 1, guideLink);
                }
            }
            else
            {
                if (grossProfitMarginPercent > (float)bankInterestRate12)
                {
                    return notes.Add($"Tỉ trọng lợi nhuận gộp {lastQuarterlyFinancialIndicator.GrossProfitMargin:0,0.00}% chỉ lớn hơn lái suất ngân hàng kỳ hạn 12 tháng {bankInterestRate12:0,0.00} %", -1, -1, guideLink);
                }
                else
                {
                    return notes.Add($"Tỉ trọng lợi nhuận gộp {lastQuarterlyFinancialIndicator.GrossProfitMargin:0,0.00}% thấp hơn lái suất ngân hàng kỳ hạn 12 tháng {bankInterestRate12:0,0.00} %", -2, -2, guideLink);
                }
            }
        }

        /// <summary>
        /// Kiểm tra tổng nợ/tài sản
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="lastYearlyFinancialIndicator">Chỉ số năm gần nhất</param>
        /// <param name="lastYearlyFinancialIndicatorSameIndustries">Danh sách các chỉ số tài chính công ty cùng ngành của năm gần nhất</param>
        /// <returns></returns>
        public static int DebtAssetCheck(List<AnalyticsNote> notes, FinancialIndicator? lastYearlyFinancialIndicator, List<FinancialIndicator> lastYearlyFinancialIndicatorSameIndustries)
        {
            var guideLink = "https://vietnambiz.vn/he-so-no-tren-von-chu-so-huu-dept-to-equity-ratio-he-so-d-e-la-gi-2019081919185477.htm";
            if (lastYearlyFinancialIndicator is null)
            {
                return notes.Add("Không có thông tin tổng nợ/tài sản \"DebtAssetCheck\".", -10, -2, guideLink);
            }

            var avgDA = lastYearlyFinancialIndicatorSameIndustries.Average(q => q.DebtAsset);
            if (lastYearlyFinancialIndicator.DebtAsset < 0.6f)
            {
                if (lastYearlyFinancialIndicator.DebtAsset < avgDA)
                {
                    return notes.Add($"Tỉ trọng tổng nợ/tài sản {lastYearlyFinancialIndicator.DebtAsset * 100:0,0.00}% nhỏ hơn 60% và nhỏ hơn trung bình ngành {avgDA * 100:0,0.00}", 2, 2, guideLink);
                }
                else
                {
                    return notes.Add($"Tỉ trọng tổng nợ/tài sản {lastYearlyFinancialIndicator.DebtAsset * 100:0,0.00}% nhỏ hơn 60%.", 1, 1, guideLink);
                }
            }
            else
            {
                if (lastYearlyFinancialIndicator.DebtAsset > avgDA)
                {
                    return notes.Add($"Tỉ trọng tổng nợ/tài sản {lastYearlyFinancialIndicator.DebtAsset * 100:0,0.00}% lớn hơn 60% và lớn hơn trung bình ngành {avgDA * 100:0,0.00}", -2, -2, guideLink);
                }
                else
                {
                    return notes.Add($"Tỉ trọng tổng nợ/tài sản {lastYearlyFinancialIndicator.DebtAsset * 100:0,0.00}% lớn hơn 60%", -1, -1, guideLink);
                }
            }
        }

        /// <summary>
        /// Kiểm tra tổng nợ/vốn chủ sở hữu
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="lastYearlyFinancialIndicator">Chỉ số năm gần nhất</param>
        /// <param name="lastYearlyFinancialIndicatorSameIndustries">Danh sách các chỉ số tài chính công ty cùng ngành của năm gần nhất</param>
        /// <returns></returns>
        public static int DebtEquityCheck(List<AnalyticsNote> notes, FinancialIndicator? lastYearlyFinancialIndicator, List<FinancialIndicator> lastYearlyFinancialIndicatorSameIndustries)
        {
            var guideLink = "https://vietnambiz.vn/he-so-no-tren-von-chu-so-huu-dept-to-equity-ratio-he-so-d-e-la-gi-2019081919185477.htm";
            if (lastYearlyFinancialIndicator is null)
            {
                return notes.Add("Không có thông tin tổng nợ/vốn chủ sở hữu \"DebtEquityCheck\".", -10, -2, guideLink);
            }

            var avgDA = lastYearlyFinancialIndicatorSameIndustries.Average(q => q.DebtEquity);
            if (lastYearlyFinancialIndicator.DebtEquity < 0.6f)
            {
                if (lastYearlyFinancialIndicator.DebtEquity < avgDA)
                {
                    return notes.Add($"Tỉ trọng tổng nợ/vốn chủ sở hữu {lastYearlyFinancialIndicator.DebtEquity * 100:0,0.00}% nhỏ hơn 60% và nhỏ hơn trung bình ngành {avgDA * 100:0,0.00}", 2, 2, guideLink);
                }
                else
                {
                    return notes.Add($"Tỉ trọng tổng nợ/vốn chủ sở hữu {lastYearlyFinancialIndicator.DebtEquity * 100:0,0.00}% nhỏ hơn 60%.", 1, 1, guideLink);
                }
            }
            else
            {
                if (lastYearlyFinancialIndicator.DebtAsset > avgDA)
                {
                    return notes.Add($"Tỉ trọng tổng nợ/vốn chủ sở hữu {lastYearlyFinancialIndicator.DebtEquity * 100:0,0.00}% lớn hơn 60% và lớn hơn trung bình ngành {avgDA * 100:0,0.00}", -2, -2, guideLink);
                }
                else
                {
                    return notes.Add($"Tỉ trọng tổng nợ/vốn chủ sở hữu {lastYearlyFinancialIndicator.DebtEquity * 100:0,0.00}% lớn hơn 60%", -1, -1, guideLink);
                }
            }
        }

        /// <summary>
        /// Đánh giá vốn đều lệ của doanh nghiệp
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Doanh nghiệp cần kiểm tra</param>
        /// <returns>
        /// Tối đa 1 điểm, thấp nhất 0 điểm
        /// </returns>
        public static int CharterCapitalCheck(List<AnalyticsNote> notes, Company company, List<Company> companiesSameSubsectorCode)
        {
            var score = 1;
            var type = 0;
            var note = $"Vốn điều lệ {company.CharterCapital:0,0} VNĐ, ";
            var avgCharterCapital = companiesSameSubsectorCode.Average(q => q.CharterCapital);
            var sumCharterCapital = companiesSameSubsectorCode.Sum(q => q.CharterCapital);
            if (avgCharterCapital <= 0 || sumCharterCapital <= 0)
            {
                return notes.Add("Không có thông tin vốn điều lệ.", -1, -1, null);
            }
            var avgPercent = (company.CharterCapital - avgCharterCapital) / Math.Abs(avgCharterCapital) * 100;
            var sumPercent = Math.Abs(company.CharterCapital * 100) / Math.Abs(sumCharterCapital);
            note += $"chiếm {sumPercent:0,0.00} % vốn điều lệ toàn ngành, ";
            if (company.CharterCapital > avgCharterCapital)
            {
                type = 1;
                note += $"lớn hơn bình quân toàn ngành {avgPercent:0,0.00} %.";
            }
            else
            {
                type = -1;
                note += $"nhỏ hơn bình quân toàn ngành {avgPercent:0,0.00} %.";
            }
            if (sumPercent > 50)
            {
                type = 2;
            }
            return notes.Add(note, score, type, null);
        }

        /// <summary>
        /// Phân tích giá trị của cổ phiếu bằng đánh giá của fiintrading
        /// </summary>
        /// <param name="notes">Ghi chú đánh giá</param>
        /// <param name="fiinEvaluate">Kết quả đánh giá của fiintrading</param>
        /// <returns></returns>
        public static int FiinValueCheck(List<AnalyticsNote> notes, FiinEvaluated? fiinEvaluate)
        {
            var type = 0;
            var score = 0;
            var note = string.Empty;

            if (fiinEvaluate is null)
            {
                note += "Không có thông tin đánh giá của fiintrading. ";
                score -= 1;
                type--;

                return notes.Add(note, score, type, null);
            }

            if (fiinEvaluate.ControlStatusCode >= 0)
            {
                note += $"Trạng thái giao dịch cổ phiếu {fiinEvaluate.ControlStatusName}. ";
                score -= 5;
                type -= 2;

                return notes.Add(note, score, type, null);
            }

            note += $"Fiin đánh giá giá trị của cổ phiếu điểm {fiinEvaluate.Value}, ";
            switch (fiinEvaluate.Value)
            {
                case "A":
                    score += 2;
                    type = 2;
                    break;
                case "B":
                    score++;
                    type = 1;
                    break;
                case "C":
                    break;
                case "D":
                    score--;
                    type = -1;
                    break;
                case "E":
                    score -= 2;
                    type = -1;
                    break;
                case "F":
                    score -= 2;
                    type = -2;
                    break;
            }

            if (fiinEvaluate.IcbRank > -1 && fiinEvaluate.IcbTotalRanked > -1)
            {
                if (fiinEvaluate.IcbRank == 1)
                {
                    score += 2;
                    type += 2;
                    note += $"thuộc top {fiinEvaluate.IcbRank} giá trị trong tổng {fiinEvaluate.IcbTotalRanked} cổ phiếu ngành, ";
                }
                else if (fiinEvaluate.IcbRank <= 3)
                {
                    score++;
                    type++;
                    note += $"thuộc top 3 giá trị trong tổng {fiinEvaluate.IcbTotalRanked} cổ phiếu ngành, ";
                }
            }

            if (fiinEvaluate.IndexRank > -1 && fiinEvaluate.IndexTotalRanked > -1)
            {
                if (fiinEvaluate.IndexRank == 1)
                {
                    score += 2;
                    type += 2;
                    note += $"thuộc top {fiinEvaluate.IndexRank} giá trị trong tổng {fiinEvaluate.IndexTotalRanked} cổ phiếu rổ {fiinEvaluate.ComGroupCode}. ";
                }
                else if (fiinEvaluate.IndexRank <= 3)
                {
                    score++;
                    type++;
                    note += $"thuộc top 3 giá trị trong tổng {fiinEvaluate.IndexTotalRanked} rổ {fiinEvaluate.ComGroupCode}. ";
                }
            }

            return notes.Add(note, score, type, $"https://fiin-fundamental.ssi.com.vn/Snapshot/GetCompanyScore?language=vi&OrganCode={fiinEvaluate.Symbol}");
        }

        /// <summary>
        /// Phân tích giá trị của cổ phiếu bằng đánh giá của vnd
        /// </summary>
        /// <param name="notes">Ghi chú đánh giá</param>
        /// <param name="vndStockScores">Kết quả đánh giá của vnd</param>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns></returns>
        public static int VndValueCheck(List<AnalyticsNote> notes, List<VndStockScore> vndStockScores, string symbol)
        {
            var type = 0;
            var score = 0;
            var note = string.Empty;

            if (vndStockScores.Count <= 0)
            {
                note += "Không có thông tin đánh giá của vnd. ";
                score -= 1;
                type--;

                return notes.Add(note, score, type, null);
            }
            var avgScore = vndStockScores.Where(q => "101000,104000,105000".Contains(q.CriteriaCode)).Average(q => q.Point);
            note += $"Vnd trung bình đánh giá các tiêu chí vị thế, tài chính, cam kết cổ đông là {avgScore:0.0}";

            if (avgScore >= 8)
            {
                score += 2;
                type = 2;
            }
            else if (avgScore >= 6)
            {
                score++;
                type++;
            }
            else if (avgScore <= 4)
            {
                score--;
                type--;
            }
            else if (avgScore <= 2)
            {
                score -= 2;
                type = -2;
            }

            return notes.Add(note, score, type, $"https://dstock.vndirect.com.vn/tong-quan/{symbol}");
        }
    }
}