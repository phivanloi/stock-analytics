using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Services
{
    public static class CompanyGrowthAnalyticsService
    {
        /// <summary>
        /// kiểm tra tăng trưởng danh thu theo năm, mỗi năm tăng trưởng dương liền mạch được cộng một điểm
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty càn kiểm tra</param>
        /// <param name="financialIndicatorsSameIndustries">Danh sách 5 năm chỉ số tài chính của các công ty cùng ngành</param>
        /// <returns>int</returns>
        public static int YearlyRevenueGrowthCheck(List<AnalyticsMessage> notes, Company company, List<FinancialIndicator> financialIndicatorsSameIndustries)
        {
            var companyFinancialIndicators = financialIndicatorsSameIndustries.Where(q => q.Symbol == company.Symbol).ToList();
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";
            if (companyFinancialIndicators?.Count <= 0)
            {
                notes.Add("Không có thông tin doanh thu để đánh giá tăng trưởng.", -10, -2, guideLink);
                return -10;
            }

            var yearRange = 5;
            var yearRevenue = companyFinancialIndicators.Where(q => q.YearReport >= (DateTime.Now.Year - yearRange) && q.LengthReport == 5).OrderByDescending(q => q.YearReport).ToList();
            if (yearRevenue?.Count <= 1)
            {
                notes.Add($"{yearRange} năm gần đây không có thông tin doanh thu năm.", -5, -1, guideLink);
                return -5;
            }

            var fistYear = yearRevenue.FirstOrDefault()?.YearReport ?? 0;
            if (fistYear < (DateTime.Now.Year - 1))
            {
                notes.Add("Năm gần đây không có thông tin doanh thu năm.", -3, -1, guideLink);
                return -3;
            }

            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = yearRevenue.GetFluctuationsTopDown(q => q.Revenue);

            if (percents?.Count <= 0)
            {
                notes.Add("không có thông tin tăng trưởng doanh thu năm.", -1, -1, guideLink);
                return -1;
            }

            var avgPercent = percents.Average();
            var type = 0;
            var note = $"Tăng trưởng doanh thu trung bình {yearRevenue.Count} năm {avgPercent:0.0}%, năm gần nhất là {percents[0]:0.0}%";

            if (percents[0] > 25)
            {
                score++;
            }
            else if (percents[0] < 0)
            {
                score -= 2;
            }

            if (avgPercent > 0)
            {
                score++;
                type++;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<decimal, int>() {
                    {15, 1 },
                    {25, 1 },
                    {35, 2 }
                }, (point) => avgPercent > point);
                score += pointLadderScore;
            }
            else if (avgPercent < 0)
            {
                score--;
                type--;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<decimal, int>() {
                    {-10, 2 },
                    {-20, 2 },
                    {-30, 2 }
                }, (point) => avgPercent < point);
                score -= pointLadderScore;
            }

            if (fistEqualCount > 0)
            {
                note += $", duy trì trong {fistEqualCount} năm đến hiện tại.";
            }
            if (fistGrowCount > 0)
            {
                score += fistGrowCount;
                note += $", tăng trong {fistGrowCount} năm đến hiện tại.";
                type++;
            }
            if (fistDropCount > 0)
            {
                score -= fistDropCount;
                note += $", giảm trong {fistDropCount} năm đến hiện tại.";
                type--;
            }

            note += $" Tổng biến động:";
            if (equalCount > 0)
            {
                note += $" duy trì {equalCount} đợt, ";
            }
            if (increaseCount > 0)
            {
                note += $" tăng {increaseCount} đợt, ";
            }
            if (reductionCount > 0)
            {
                note += $" giảm {reductionCount} đợt, ";
            }

            notes.Add(note, score, type, guideLink);
            return score;
        }

        /// <summary>
        /// kiểm tra tăng trưởng danh thu theo 4 quy gần nhất, với mỗi kiểm tra cao hơn quý trước được cộng 1 điểm,
        /// trung bình các quý tăng trưởng trên 15,25,35% thì được cộng thêm 3 điểm tương ứng với các thay đổi, và ngược lại với tăng trưởng âm
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty càn kiểm tra</param>
        /// <param name="financialIndicatorsSameIndustries">Danh sách chỉ số tài chính của các công ty cùng ngành</param>
        /// <returns>int</returns>
        public static int QuarterlyRevenueGrowthCheck(List<AnalyticsMessage> notes, Company company, List<FinancialIndicator> financialIndicatorsSameIndustries)
        {
            var score = 0;
            var type = 0;
            var note = "";
            var companyFinancialIndicators = financialIndicatorsSameIndustries.Where(q => q.Symbol == company.Symbol).ToList();
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";
            if (companyFinancialIndicators?.Count <= 0)
            {
                notes.Add("Không có thông tin doanh thu để đánh giá tăng trưởng quý.", -10, -2, guideLink);
                return -10;
            }

            var topFourItem = companyFinancialIndicators.Where(q => q.LengthReport != 5).OrderByDescending(q => q.YearReport).ThenByDescending(q => q.LengthReport).Take(4).ToList();
            if (topFourItem.Count <= 0)
            {
                notes.Add("Không có thông tin doanh thu quý để đánh giá tăng trưởng quý.", -5, -2, guideLink);
                return -5;
            }

            var percents = new List<decimal>();
            foreach (var item in topFourItem)
            {
                var checkItem = companyFinancialIndicators.FirstOrDefault(q => q.YearReport == item.YearReport - 1 && q.LengthReport == item.LengthReport);
                if (checkItem is not null)
                {
                    var changePercent = item.Revenue.GetPercent(checkItem.Revenue);
                    if (changePercent > 0)
                    {
                        note += $"Doanh thu quý {item.LengthReport} năm {item.YearReport} cao hơn cùng kỳ {changePercent:0,0}%. ";
                        score++;
                    }
                    else
                    {
                        note += $"Doanh thu quý {item.LengthReport} năm {item.YearReport} thấp hơn cùng kỳ {changePercent:0,0}%. ";
                        score--;
                    }
                    percents.Add(changePercent);
                }
            }
            if (percents?.Count > 0)
            {
                var avgPercent = percents.Average();
                if (avgPercent <= 0)
                {
                    score--;
                    type = -1;
                    if (avgPercent > -15)
                    {
                        type = 2;
                        score--;
                    }
                    if (avgPercent > -25)
                    {
                        score--;
                    }
                    if (avgPercent > -35)
                    {
                        score--;
                    }
                }
                else
                {
                    score++;
                    type = 1;
                    if (avgPercent > 15)
                    {
                        score++;
                    }
                    if (avgPercent > 25)
                    {
                        score++;
                    }
                    if (avgPercent > 35)
                    {
                        score++;
                        type = 2;
                    }
                }
            }

            notes.Add(note, score, type, guideLink);
            return score;
        }

        /// <summary>
        /// kiểm tra tăng trưởng lợi nhuận theo năm, mỗi năm tăng trưởng dương liền mạch được cộng một điểm
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty càn kiểm tra</param>
        /// <param name="financialIndicatorsSameIndustries">Danh sách chỉ số tài chính của các công ty cùng ngành</param>
        /// <returns>int</returns>
        public static int YearlyProfitGrowthCheck(List<AnalyticsMessage> notes, Company company, List<FinancialIndicator> financialIndicatorsSameIndustries)
        {
            var companyFinancialIndicators = financialIndicatorsSameIndustries.Where(q => q.Symbol == company.Symbol).ToList();
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";
            if (companyFinancialIndicators?.Count <= 0)
            {
                notes.Add("Không có thông tin lợi nhuận để đánh giá tăng trưởng.", -10, -2, guideLink);
                return -10;
            }

            var yearRange = 5;
            var yearProfit = companyFinancialIndicators.Where(q => q.YearReport >= (DateTime.Now.Year - yearRange) && q.LengthReport == 5).OrderByDescending(q => q.YearReport).ToList();
            if (yearProfit?.Count <= 1)
            {
                notes.Add($"{yearRange} năm gần đây không có thông tin lợi nhuận năm.", -5, -1, guideLink);
                return -5;
            }

            var fistYear = yearProfit.FirstOrDefault()?.YearReport ?? 0;
            if (fistYear < (DateTime.Now.Year - 1))
            {
                notes.Add("Năm gần đây không có thông tin lợi nhuận năm.", -3, -1, guideLink);
                return -3;
            }

            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = yearProfit.GetFluctuationsTopDown(q => q.Profit);

            if (percents?.Count <= 0)
            {
                notes.Add("không có thông tin tăng trưởng lợi nhuận năm.", -1, -1, guideLink);
                return -1;
            }

            var avgPercent = percents.Average();
            var type = 0;
            var note = $"Tăng trưởng lợi nhuận trung bình {yearProfit.Count} năm {avgPercent:0.0}%, năm gần nhất là {percents[0]:0.0}%";

            if (percents[0] > 25)
            {
                score++;
            }
            else if (percents[0] < 0)
            {
                score -= 2;
            }

            if (avgPercent > 0)
            {
                score++;
                type++;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<decimal, int>() {
                    {15, 1 },
                    {25, 1 },
                    {35, 2 }
                }, (point) => avgPercent > point);
                score += pointLadderScore;
            }
            else if (avgPercent < 0)
            {
                score--;
                type--;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<decimal, int>() {
                    {-10, 2 },
                    {-20, 2 },
                    {-30, 2 }
                }, (point) => avgPercent < point);
                score -= pointLadderScore;
            }

            if (fistEqualCount > 0)
            {
                note += $", duy trì trong {fistEqualCount} năm đến hiện tại.";
            }
            if (fistGrowCount > 0)
            {
                score += fistGrowCount;
                note += $", tăng trong {fistGrowCount} năm đến hiện tại.";
                type++;
            }
            if (fistDropCount > 0)
            {
                score -= fistDropCount;
                note += $", giảm trong {fistDropCount} năm đến hiện tại.";
                type--;
            }

            note += $" Tổng biến động:";
            if (equalCount > 0)
            {
                note += $" duy trì {equalCount} đợt, ";
            }
            if (increaseCount > 0)
            {
                note += $" tăng {increaseCount} đợt, ";
            }
            if (reductionCount > 0)
            {
                note += $" giảm {reductionCount} đợt, ";
            }

            notes.Add(note, score, type, guideLink);
            return score;
        }

        /// <summary>
        /// kiểm tra tăng trưởng lợi nhuận theo 4 quy gần nhất, với mỗi kiểm tra cao hơn quý trước được cộng 1 điểm,
        /// trung bình các quý tăng trưởng trên 15,25,35% thì được cộng thêm 3 điểm tương ứng với các thay đổi, và ngược lại với tăng trưởng âm
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty càn kiểm tra</param>
        /// <param name="financialIndicatorsSameIndustries">Danh sách chỉ số tài chính của các công ty cùng ngành</param>
        /// <returns>int</returns>
        public static int QuarterlyProfitGrowthCheck(List<AnalyticsMessage> notes, Company company, List<FinancialIndicator> financialIndicatorsSameIndustries)
        {
            var score = 0;
            var type = 0;
            var note = "";
            var companyFinancialIndicators = financialIndicatorsSameIndustries.Where(q => q.Symbol == company.Symbol).ToList();
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";
            if (companyFinancialIndicators?.Count <= 0)
            {
                notes.Add("Không có thông tin lợi nhuận để đánh giá tăng trưởng quý.", -10, -2, guideLink);
                return -10;
            }

            var topFourItem = companyFinancialIndicators.Where(q => q.LengthReport != 5).OrderByDescending(q => q.YearReport).ThenByDescending(q => q.LengthReport).Take(4).ToList();
            if (topFourItem.Count <= 0)
            {
                notes.Add("Không có thông tin lợi nhuận quý để đánh giá tăng trưởng quý.", -5, -2, guideLink);
                return -5;
            }

            var percents = new List<decimal>();
            foreach (var item in topFourItem)
            {
                var checkItem = companyFinancialIndicators.FirstOrDefault(q => q.YearReport == item.YearReport - 1 && q.LengthReport == item.LengthReport);
                if (checkItem is not null)
                {
                    var changePercent = item.Profit.GetPercent(checkItem.Profit);
                    if (changePercent > 0)
                    {
                        note += $"Lợi nhuận quý {item.LengthReport} năm {item.YearReport} cao hơn cùng kỳ {changePercent:0,0}%. ";
                        score++;
                    }
                    else
                    {
                        note += $"Lợi nhuận quý {item.LengthReport} năm {item.YearReport} thấp hơn cùng kỳ {changePercent:0,0}%. ";
                        score--;
                    }
                    percents.Add(changePercent);
                }
            }
            if (percents?.Count > 0)
            {
                var avgPercent = percents.Average();
                if (avgPercent <= 0)
                {
                    score--;
                    type = -1;
                    if (avgPercent > -15)
                    {
                        type = 2;
                        score--;
                    }
                    if (avgPercent > -25)
                    {
                        score--;
                    }
                    if (avgPercent > -35)
                    {
                        score--;
                    }
                }
                else
                {
                    score++;
                    type = 1;
                    if (avgPercent > 15)
                    {
                        score++;
                    }
                    if (avgPercent > 25)
                    {
                        score++;
                    }
                    if (avgPercent > 35)
                    {
                        score++;
                        type = 2;
                    }
                }
            }
            notes.Add(note, score, type, guideLink);
            return score;
        }

        /// <summary>
        /// Đánh giá tài sản
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialGrowths">Danh sách chỉ số tài chính theo năm</param>
        /// <returns>
        /// Tối đa là 7 và tối thiểu là 0
        /// </returns>
        public static int AssetCheck(List<AnalyticsMessage> notes, List<FinancialGrowth> financialGrowths)
        {
            var guideLink = "https://govalue.vn/von-chu-so-huu-equity-la-gi";
            if (financialGrowths?.Count <= 0)
            {
                notes.Add("Không có thông tin vốn tài sản.", -10, -2, guideLink);
                return -10;
            }

            var yearRange = 5;
            var financialGrowthsOwnerCapital = financialGrowths.Where(q => q.Year >= (DateTime.Now.Year - yearRange)).OrderByDescending(q => q.Year).ToList();
            if (financialGrowthsOwnerCapital?.Count <= 0)
            {
                notes.Add($"{yearRange} năm gần đây không có thông tin tài sản.", -5, -1, guideLink);
                return -5;
            }

            var fistYear = financialGrowthsOwnerCapital.FirstOrDefault()?.Year ?? 0;
            if (fistYear < (DateTime.Now.Year - 2))
            {
                notes.Add("Năm gần đây không có thông tin tài sản.", -3, -1, guideLink);
                return -3;
            }

            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = financialGrowthsOwnerCapital.GetFluctuationsTopDown(q => q.Asset);

            if (percents?.Count <= 0)
            {
                notes.Add("không có thông tin tăng trưởng tài sản.", 0, -1, guideLink);
                return 0;
            }

            var avgPercent = percents.Average();
            var type = 0;
            var note = $"Tăng trưởng tài sản trung bình {financialGrowthsOwnerCapital.Count} năm {avgPercent:0.0}%";
            if (fistEqualCount > 0)
            {
                note += $", duy trì trong {fistEqualCount} năm đến hiện tại.";
            }
            if (fistGrowCount > 0)
            {
                score += fistGrowCount;
                note += $", tăng trong {fistGrowCount} năm đến hiện tại.";
                type++;
            }
            if (fistDropCount > 0)
            {
                score -= fistDropCount;
                note += $", giảm trong {fistDropCount} năm đến hiện tại.";
                type--;
            }

            note += $" Tổng biến động:";
            if (equalCount > 0)
            {
                note += $" duy trì {equalCount} đợt, ";
            }
            if (increaseCount > 0)
            {
                note += $" tăng {increaseCount} đợt, ";
            }
            if (reductionCount > 0)
            {
                note += $" giảm {reductionCount} đợt, ";
            }

            notes.Add(note, score, type, guideLink);
            return score;
        }

        /// <summary>
        /// Đánh giá vốn chủ sở hữu
        /// https://govalue.vn/von-chu-so-huu-equity-la-gi
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialGrowths">Danh sách chỉ số tài chính theo năm</param>
        /// <returns>
        /// Tối đa là 7 và tối thiểu là 0
        /// </returns>
        public static int OwnerCapitalCheck(List<AnalyticsMessage> notes, List<FinancialGrowth> financialGrowths)
        {
            var guideLink = "https://govalue.vn/von-chu-so-huu-equity-la-gi";
            if (financialGrowths?.Count <= 0)
            {
                notes.Add("Không có thông tin vốn chủ sở hữu.", -10, -2, guideLink);
                return -10;
            }

            var yearRange = 5;
            var financialGrowthsOwnerCapital = financialGrowths.Where(q => q.Year >= (DateTime.Now.Year - yearRange)).OrderByDescending(q => q.Year).ToList();
            if (financialGrowthsOwnerCapital?.Count <= 0)
            {
                notes.Add($"{yearRange} năm gần đây không có thông tin vốn chủ sở hữu.", -5, -1, guideLink);
                return -5;
            }

            var fistYear = financialGrowthsOwnerCapital.FirstOrDefault()?.Year ?? 0;
            if (fistYear < (DateTime.Now.Year - 2))
            {
                notes.Add("Năm gần đây không có thông tin vốn chủ sở hữu.", -1, -1, guideLink);
                return -1;
            }

            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = financialGrowthsOwnerCapital.GetFluctuationsTopDown(q => q.OwnerCapital);

            if (percents?.Count <= 0)
            {
                notes.Add("không có thông tin tăng trưởng vốn chủ sở hữu.", 0, -1, guideLink);
                return 0;
            }

            var avgPercent = percents.Average();
            var type = 0;
            var note = $"Tăng trưởng vốn chủ sở hữu trung bình {financialGrowthsOwnerCapital.Count} năm {avgPercent:0.0}%";
            if (fistEqualCount > 0)
            {
                note += $", duy trì trong {fistEqualCount} năm đến hiện tại.";
            }
            if (fistGrowCount > 0)
            {
                score += fistGrowCount;
                note += $", tăng trong {fistGrowCount} năm đến hiện tại.";
                type++;
            }
            if (fistDropCount > 0)
            {
                score -= fistDropCount;
                note += $", giảm trong {fistDropCount} năm đến hiện tại.";
                type--;
            }

            note += $" Tổng biến động:";
            if (equalCount > 0)
            {
                note += $" duy trì {equalCount} đợt, ";
            }
            if (increaseCount > 0)
            {
                note += $" tăng {increaseCount} đợt, ";
            }
            if (reductionCount > 0)
            {
                note += $" giảm {reductionCount} đợt, ";
            }

            notes.Add(note, score, type, guideLink);
            return score;
        }

        /// <summary>
        /// Đánh giá chi trả cổ tức bằng tiền mặt
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialGrowths">Danh sách chỉ số tài chính theo năm</param>
        /// <returns>
        /// Tối đa là 7 và tối thiểu là 0
        /// </returns>
        public static int DividendCheck(List<AnalyticsMessage> notes, List<FinancialGrowth> financialGrowths)
        {
            var guideLink = "https://govalue.vn/von-chu-so-huu-equity-la-gi";
            if (financialGrowths?.Count <= 0)
            {
                notes.Add("Không có thông tin chi trả cổ tức bằng tiền.", -10, -2, guideLink);
                return -10;
            }

            var yearRange = 5;
            var financialGrowthsOwnerCapital = financialGrowths.Where(q => q.Year >= (DateTime.Now.Year - yearRange)).OrderByDescending(q => q.Year).ToList();
            if (financialGrowthsOwnerCapital?.Count <= 0)
            {
                notes.Add($"{yearRange} năm gần đây không có thông tin chi trả cổ tức bằng tiền.", -5, -1, guideLink);
                return -5;
            }

            var fistYear = financialGrowthsOwnerCapital.FirstOrDefault()?.Year ?? 0;
            if (fistYear < (DateTime.Now.Year - 2))
            {
                notes.Add("Năm gần đây không có thông tin chi trả cổ tức bằng tiền.", -1, -1, guideLink);
                return -1;
            }

            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = financialGrowthsOwnerCapital.GetFluctuationsTopDown(q => q.ValuePershare);

            if (percents?.Count <= 0)
            {
                notes.Add("không có thông tin tăng trưởng trả cổ tức bằng tiền.", 0, -1, guideLink);
                return 0;
            }

            var avgPercent = percents.Average();
            var type = 0;
            var note = $"Tăng trưởng trả cổ tức bằng tiền trung bình {financialGrowthsOwnerCapital.Count} năm {avgPercent:0.0}%";
            if (fistEqualCount > 0)
            {
                note += $", duy trì trong {fistEqualCount} năm đến hiện tại.";
            }
            if (fistGrowCount > 0)
            {
                score += fistGrowCount;
                note += $", tăng trong {fistGrowCount} năm đến hiện tại.";
                type++;
            }
            if (fistDropCount > 0)
            {
                score -= fistDropCount;
                note += $", giảm trong {fistDropCount} năm đến hiện tại.";
                type--;
            }

            note += $" Tổng biến động:";
            if (equalCount > 0)
            {
                note += $" duy trì {equalCount} đợt, ";
            }
            if (increaseCount > 0)
            {
                note += $" tăng {increaseCount} đợt, ";
            }
            if (reductionCount > 0)
            {
                note += $" giảm {reductionCount} đợt, ";
            }

            notes.Add(note, score, type, guideLink);
            return score;
        }

        /// <summary>
        /// Kiểm tra tăng trưởng theo năm của chỉ số Eps, 
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty càn kiểm tra</param>
        /// <param name="financialIndicatorsSameIndustries">Danh sách 5 năm chỉ số tài chính của các công ty cùng ngành</param>
        /// <returns>int</returns>
        public static int YearlyEpsGrowthCheck(List<AnalyticsMessage> notes, Company company, List<FinancialIndicator> financialIndicatorsSameIndustries)
        {
            var companyFinancialIndicators = financialIndicatorsSameIndustries.Where(q => q.Symbol == company.Symbol).ToList();
            var guideLink = "https://vcbs.com.vn/vn/Utilities/Index/53";
            if (companyFinancialIndicators?.Count <= 0)
            {
                notes.Add("Không có thông tin eps để đánh giá tăng trưởng.", -10, -2, guideLink);
                return -10;
            }

            var yearRange = 5;
            var yearlyEps = companyFinancialIndicators.Where(q => q.YearReport >= (DateTime.Now.Year - yearRange) && q.LengthReport == 5).OrderByDescending(q => q.YearReport).ToList();
            if (yearlyEps?.Count <= 1)
            {
                notes.Add($"{yearRange} năm gần đây không có thông tin eps năm.", -5, -1, guideLink);
                return -5;
            }

            var fistYear = yearlyEps.FirstOrDefault()?.YearReport ?? 0;
            if (fistYear < (DateTime.Now.Year - 1))
            {
                notes.Add("Năm gần đây không có thông tin eps năm.", -3, -1, guideLink);
                return -3;
            }

            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = yearlyEps.GetFluctuationsTopDown(q => q.Eps);

            if (percents?.Count <= 0)
            {
                notes.Add("không có thông tin tăng trưởng eps năm.", -1, -1, guideLink);
                return -1;
            }

            var avgPercent = percents.Average();
            var type = 0;
            var note = $"Tăng trưởng Eps trung bình {yearlyEps.Count} năm {avgPercent:0.0}%, năm gần nhất là {percents[0]:0.0}%";

            if (percents[0] > 25)
            {
                score++;
            }
            else if (percents[0] < 0)
            {
                score -= 2;
            }

            if (fistEqualCount > 0)
            {
                note += $", duy trì trong {fistEqualCount} năm đến hiện tại.";
            }
            if (fistGrowCount > 0)
            {
                score += fistGrowCount;
                note += $", tăng trong {fistGrowCount} năm đến hiện tại.";
                type++;
            }
            if (fistDropCount > 0)
            {
                score -= fistDropCount;
                note += $", giảm trong {fistDropCount} năm đến hiện tại.";
                type--;
            }

            note += $" Tổng biến động:";
            if (equalCount > 0)
            {
                note += $" duy trì {equalCount} đợt, ";
            }
            if (increaseCount > 0)
            {
                note += $" tăng {increaseCount} đợt, ";
            }
            if (reductionCount > 0)
            {
                note += $" giảm {reductionCount} đợt, ";
            }

            notes.Add(note, score, type, guideLink);
            return score;
        }

        /// <summary>
        /// kiểm tra tăng trưởng eps theo 4 quy gần nhất, với mỗi kiểm tra cao hơn quý trước được cộng 1 điểm,
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Công ty càn kiểm tra</param>
        /// <param name="financialIndicatorsSameIndustries">Danh sách chỉ số tài chính của các công ty cùng ngành</param>
        /// <returns>int</returns>
        public static int QuarterlyEpsGrowthCheck(List<AnalyticsMessage> notes, Company company, List<FinancialIndicator> financialIndicatorsSameIndustries)
        {
            var score = 0;
            var type = 0;
            var note = "";
            var companyFinancialIndicators = financialIndicatorsSameIndustries.Where(q => q.Symbol == company.Symbol).ToList();
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";
            if (companyFinancialIndicators?.Count <= 0)
            {
                notes.Add("Không có thông tin eps để đánh giá tăng trưởng quý.", -10, -2, guideLink);
                return -10;
            }

            var topFourItem = companyFinancialIndicators.Where(q => q.LengthReport != 5).OrderByDescending(q => q.YearReport).ThenByDescending(q => q.LengthReport).Take(4).ToList();
            if (topFourItem.Count <= 0)
            {
                notes.Add("Không có thông tin eps quý để đánh giá tăng trưởng quý.", -5, -2, guideLink);
                return -5;
            }

            var percents = new List<decimal>();
            foreach (var item in topFourItem)
            {
                var checkItem = companyFinancialIndicators.FirstOrDefault(q => q.YearReport == item.YearReport - 1 && q.LengthReport == item.LengthReport);
                if (checkItem is not null)
                {
                    var changePercent = item.Revenue.GetPercent(checkItem.Revenue);
                    if (changePercent > 0)
                    {
                        note += $"Eps quý {item.LengthReport} năm {item.YearReport} cao hơn cùng kỳ {changePercent:0,0}%. ";
                        score++;
                    }
                    else
                    {
                        note += $"Eps quý {item.LengthReport} năm {item.YearReport} thấp hơn cùng kỳ {changePercent:0,0}%. ";
                        score--;
                    }
                    percents.Add(changePercent);
                }
            }

            if (percents?.Count > 0)
            {
                var avgPercent = percents.Average();
                if (avgPercent <= 0)
                {
                    score--;
                    type = -1;
                    if (avgPercent > -15)
                    {
                        type = 2;
                        score--;
                    }
                    if (avgPercent > -25)
                    {
                        score--;
                    }
                    if (avgPercent > -35)
                    {
                        score--;
                    }
                }
                else
                {
                    score++;
                    type = 1;
                    if (avgPercent > 15)
                    {
                        score++;
                    }
                    if (avgPercent > 25)
                    {
                        score++;
                    }
                    if (avgPercent > 35)
                    {
                        score++;
                        type = 2;
                    }
                }
            }
            notes.Add(note, score, type, guideLink);
            return score;
        }

        /// <summary>
        /// Đánh giá số lượng nhận viên của doanh nghiệp cùng ngành
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Doanh nghiệp cần kiểm tra</param>
        /// <param name="companiesSameSubsectorCode">Danh sách công ty cùng ngành</param>
        /// <returns>
        /// Tối đa 1 điểm, thấp nhất 0 điểm
        /// </returns>
        public static int NumberOfEmployeeCheck(List<AnalyticsMessage> notes, Company company, IEnumerable<Company> companiesSameSubsectorCode)
        {
            var score = 0;
            var type = 0;
            var note = $"Số nhân viên {company.NumberOfEmployee:0,0}, ";
            var avgMarketCap = companiesSameSubsectorCode.Average(q => q.MarketCap);
            if (avgMarketCap == 0)
            {
                return 0;
            }
            var avgPercent = (company.MarketCap - avgMarketCap) / Math.Abs(avgMarketCap) * 100;
            var averageNumberOfEmployee = companiesSameSubsectorCode.Average(q => q.NumberOfEmployee);
            var numberOfEmployeeForecast = avgPercent * (decimal)averageNumberOfEmployee / 100;
            var numberOfEmployeeForecastRound = Math.Round(numberOfEmployeeForecast);
            if (numberOfEmployeeForecastRound == 0)
            {
                return 0;
            }
            var forecastPercent = Math.Abs(company.NumberOfEmployee - numberOfEmployeeForecastRound) / numberOfEmployeeForecastRound * 100;
            if (company.NumberOfEmployee < numberOfEmployeeForecastRound)
            {
                score++;
                type = 1;
                note += $"nhỏ hơn số nhân viên tăng trưởng theo vốn hóa dự tính {numberOfEmployeeForecast:0,0} ({forecastPercent:0,0}%)";
            }
            else
            {
                type = -1;
                note += $"lớn hơn số nhân viên tăng trưởng theo vốn hóa dự tính {numberOfEmployeeForecast:0,0} ({forecastPercent:0,0}%)";
            }
            notes.Add(note, score, type, null);
            return score;
        }

        /// <summary>
        /// Đánh giá số lượng chi nhánh
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="company">Doanh nghiệp cần kiểm tra</param>
        /// <param name="companiesSameSubsectorCode">Danh sách công ty cùng ngành</param>
        /// <returns>
        /// Tối đa 1 điểm, thấp nhất 0 điểm
        /// </returns>
        public static int BankNumberOfBranchCheck(List<AnalyticsMessage> notes, Company company, IEnumerable<Company> companiesSameSubsectorCode)
        {
            var score = 0;
            var type = 0;
            var note = $"Số chi nhánh {company.BankNumberOfBranch:0,0}, ";
            var averageBranch = companiesSameSubsectorCode.Average(q => q.BankNumberOfBranch);
            var avgPercent = (company.BankNumberOfBranch - averageBranch) / Math.Abs(averageBranch) * 100;
            if (company.BankNumberOfBranch > averageBranch)
            {
                note += $"lớn hơn số chi nhánh bình quân toàn ngành {averageBranch:0,0} ({avgPercent:0.0}%)";
                score = 1;
                type = 1;
            }
            else
            {
                note += $"lớn hơn số chi nhánh bình quân toàn ngành {averageBranch:0,0} ({avgPercent:0.0}%)";
            }
            notes.Add(note, score, type, null);
            return score;
        }

        /// <summary>
        /// Phân tích tăng trưởng của cổ phiếu bằng đánh giá của fiintrading
        /// </summary>
        /// <param name="notes">Ghi chú đánh giá</param>
        /// <param name="fiinEvaluate">Kết quả đánh giá của fiintrading</param>
        /// <returns></returns>
        public static int FiinGrowthCheck(List<AnalyticsMessage> notes, FiinEvaluate fiinEvaluate)
        {
            var type = 0;
            var score = 0;
            var note = string.Empty;

            if (fiinEvaluate is null)
            {
                note += "Không có thông tin đánh giá của fiintrading. ";
                score -= 1;
                type--;

                notes.Add(note, score, type, null);
                return score;
            }

            if (fiinEvaluate.ControlStatusCode >= 0)
            {
                note += $"Trạng thái giao dịch cổ phiếu {fiinEvaluate.ControlStatusName}. ";
                score -= 5;
                type -= 2;
            }

            note += $"Fiin đánh giá tăng trưởng của cổ phiếu điểm {fiinEvaluate.Growth}, ";
            switch (fiinEvaluate.Growth)
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

            notes.Add(note, score, type, null);
            return score;
        }

        /// <summary>
        /// Phân tích tăng trưởng của cổ phiếu bằng đánh giá của vnd
        /// </summary>
        /// <param name="notes">Ghi chú đánh giá</param>
        /// <param name="vndStockScores">Kết quả đánh giá của vnd</param>
        /// <returns></returns>
        public static int VndGrowthCheck(List<AnalyticsMessage> notes, List<VndStockScore> vndStockScores)
        {
            var type = 0;
            var score = 0;
            var note = string.Empty;

            if (vndStockScores?.Count <= 0)
            {
                note += "Không có thông tin đánh giá của vnd. ";
                score -= 1;
                type--;

                notes.Add(note, score, type, null);
                return score;
            }

            var avgScore = vndStockScores.Where(q => "102000,103000".Contains(q.CriteriaCode)).Average(q => q.Point);
            note += $"Vnd trung bình đánh giá các tiêu chí tốc độ tăng trưởng, năng lực sinh lời là {avgScore:0.0}";

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

            notes.Add(note, score, type, "https://dstock.vndirect.com.vn/tong-quan/FPT");
            return score;
        }
    }
}