using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Services
{
    public static class CompanyGrowthAnalyticsService
    {
        /// <summary>
        /// Kiểm tra tăng trưởng danh thu theo năm
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialIndicators">Dữ liệu tài chính của công ty</param>
        /// <returns>int</returns>
        public static int YearlyRevenueGrowthCheck(List<AnalyticsNote> notes, List<FinancialIndicator> financialIndicators)
        {
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";
            var yearRange = 5;

            //Lấy báo cáo 5 năm doanh thu, năm mới nhất lên đầu
            var checkItems = financialIndicators.Where(q => q.YearReport >= (DateTime.Now.Year - yearRange) && q.LengthReport == 5).OrderByDescending(q => q.YearReport).Take(yearRange).ToList();

            //Không có thông tin doanh thu thì từ điểm nặng và bỏ qua
            if (checkItems.Count <= 0)
            {
                return notes.Add("Không có thông tin doanh thu để đánh giá.", -5, -2, guideLink);
            }

            //Chỉ có một bảng ghi thông tin tài chính
            if (checkItems.Count == 1)
            {
                //Kiểm tra năm gần đây nhất có thông tin không
                if (checkItems[0].YearReport < (DateTime.Now.Year - 1))
                {
                    notes.Add("Năm gần đây không có thông tin doanh thu năm.", -3, -1, guideLink);
                    return -3;
                }

                //Doanh thu năm gần đây nhất có tốt không
                if (checkItems[0].Revenue > 0)
                {
                    return notes.Add($"Chỉ có dữ liệu 1 năm doanh thu {checkItems[0].Revenue:#,###}", 0, 0, guideLink);
                }
                else
                {
                    return notes.Add($"Chỉ có dữ liệu 1 năm doanh thu {checkItems[0].Revenue:#,###}", -3, -2, guideLink);
                }
            }

            var score = 0;
            var type = 0;
            var (equalCount, increaseCount, reductionCount, consecutiveEqualCount, consecutiveGrowCount, consecutiveDropCount, percents) = checkItems.GetFluctuationsTopDown(q => q.Revenue);
            var avgPercent = percents.Average();
            var note = $"Tăng trưởng doanh thu trung bình {checkItems.Count} năm {avgPercent:0.0}%, năm gần nhất là {percents[0]:0.0}%";

            //tín toán % tăng trưởng năm gần nhất
            if (percents[0] > 0)
            {
                score++;
                type++;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {15, 1 },
                    {35, 1 },
                    {65, 1 },
                    {95, 1 },
                    {125, 1 },
                    {155, 1 },
                    {205, 1 }
                }, (point) => percents[0] > point);
                score += pointLadderScore;
            }
            else if (percents[0] < 0)
            {
                score--;
                type--;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {-0, 1 },
                    {-15, 1 },
                    {-35, 1 },
                    {-65, 1 },
                    {-95, 1 },
                    {-125, 1 },
                    {-155, 1 },
                    {-205, 1 }
                }, (point) => percents[0] < point);
                score -= pointLadderScore;
            }

            //Tính trung bình các năm
            if (avgPercent > 0)
            {
                score++;
                type++;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {15, 1 },
                    {35, 1 },
                    {65, 1 },
                    {95, 1 },
                    {125, 1 },
                    {155, 1 },
                    {205, 1 }
                }, (point) => avgPercent > point);
                score += pointLadderScore;
            }
            else if (avgPercent < 0)
            {
                score--;
                type--;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {-0, 1 },
                    {-15, 1 },
                    {-35, 1 },
                    {-65, 1 },
                    {-95, 1 },
                    {-125, 1 },
                    {-155, 1 },
                    {-205, 1 }
                }, (point) => avgPercent < point);
                score -= pointLadderScore;
            }

            //Phân tích duy trì tăng trưởng
            if (consecutiveEqualCount > 0)
            {
                note += $", duy trì trong {consecutiveEqualCount} năm đến hiện tại.";
            }
            if (consecutiveGrowCount > 0)
            {
                score += consecutiveGrowCount;
                note += $", tăng trong {consecutiveGrowCount} năm đến hiện tại.";
                type++;
            }
            if (consecutiveDropCount > 0)
            {
                score -= consecutiveDropCount;
                note += $", giảm trong {consecutiveDropCount} năm đến hiện tại.";
                type--;
            }

            //Thống kê tổng biến động
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

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// kiểm tra tăng trưởng danh thu theo 4 quy gần nhất
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialIndicators">Danh sách chỉ số tài chính của công ty</param>
        /// <returns>int</returns>
        public static int QuarterlyRevenueGrowthCheck(List<AnalyticsNote> notes, List<FinancialIndicator> financialIndicators)
        {
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";
            var score = 0;
            var type = 0;
            var note = "";
            var checkItems = financialIndicators.Where(q => q.YearReport >= (DateTime.Now.Year - 2) && q.LengthReport != 5).OrderByDescending(q => q.YearReport).ThenByDescending(q => q.LengthReport).Take(4).ToList();//lấy 4 quý cuối cùng để check

            if (checkItems.Count <= 0)
            {
                return notes.Add("Không có thông tin doanh thu để đánh giá tăng trưởng quý. ", -5, -2, guideLink);
            }

            foreach (var item in checkItems)
            {
                var posterior = financialIndicators.FirstOrDefault(q => q.YearReport == item.YearReport - 1 && q.LengthReport == item.LengthReport);
                if (posterior is not null)
                {
                    var changePercent = item.Revenue.GetPercent(posterior.Revenue);
                    if (changePercent > 0)
                    {
                        note += $"Doanh thu quý {item.LengthReport} năm {item.YearReport} cao hơn cùng kỳ {changePercent:0,0}%. ";
                        score++;
                        type++;
                        var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                            {15, 1 },
                            {35, 1 },
                            {65, 1 },
                            {95, 1 },
                            {125, 1 },
                            {155, 1 },
                            {205, 1 }
                        }, (point) => changePercent > point);
                        score += pointLadderScore;
                    }
                    else if (changePercent < 0)
                    {
                        note += $"Doanh thu quý {item.LengthReport} năm {item.YearReport} thấp hơn cùng kỳ {changePercent:0,0}%. ";
                        score--;
                        type--;
                        var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                            {-0, 1 },
                            {-15, 1 },
                            {-35, 1 },
                            {-65, 1 },
                            {-95, 1 },
                            {-125, 1 },
                            {-155, 1 },
                            {-205, 1 }
                        }, (point) => changePercent < point);
                        score -= pointLadderScore;
                    }
                    else
                    {
                        note += $"Doanh thu quý {item.LengthReport} năm {item.YearReport} không thay đổi. ";
                    }
                }
                else
                {
                    note += $"Quý {item.LengthReport} năm {item.YearReport} không có thông tin doanh thu đối chiếu. ";
                    score--;
                }
            }
            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// kiểm tra tăng trưởng lợi nhuận theo năm
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialIndicators">Danh sách chỉ số tài chính của các công ty</param>
        /// <returns>int</returns>
        public static int YearlyProfitGrowthCheck(List<AnalyticsNote> notes, List<FinancialIndicator> financialIndicators)
        {
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";
            var yearRange = 5;

            //Lấy báo cáo 5 năm lợi nhuận, năm mới nhất lên đầu
            var checkItems = financialIndicators.Where(q => q.YearReport >= (DateTime.Now.Year - yearRange) && q.LengthReport == 5).OrderByDescending(q => q.YearReport).Take(yearRange).ToList();

            //Không có thông tin lợi nhuận thì từ điểm nặng và bỏ qua
            if (checkItems.Count <= 0)
            {
                return notes.Add("Không có thông tin lợi nhuận để đánh giá.", -5, -2, guideLink);
            }

            //Chỉ có một bảng ghi thông tin tài chính
            if (checkItems.Count == 1)
            {
                //Kiểm tra năm gần đây nhất có thông tin không
                if (checkItems[0].YearReport < (DateTime.Now.Year - 1))
                {
                    notes.Add("Năm gần đây không có thông tin lợi nhuận năm.", -3, -1, guideLink);
                    return -3;
                }

                //Lợi nhuận năm gần đây nhất có tốt không
                if (checkItems[0].Revenue > 0)
                {
                    return notes.Add($"Chỉ có dữ liệu 1 năm lợi nhuận {checkItems[0].Revenue:#,###}", 0, 0, guideLink);
                }
                else
                {
                    return notes.Add($"Chỉ có dữ liệu 1 năm lợi nhuận {checkItems[0].Revenue:#,###}", -3, -2, guideLink);
                }
            }

            var score = 0;
            var type = 0;
            var (equalCount, increaseCount, reductionCount, consecutiveEqualCount, consecutiveGrowCount, consecutiveDropCount, percents) = checkItems.GetFluctuationsTopDown(q => q.Profit);
            var avgPercent = percents.Average();
            var note = $"Tăng trưởng lợi nhuận trung bình {checkItems.Count} năm {avgPercent:0.0}%, năm gần nhất là {percents[0]:0.0}%";

            //tín toán % tăng trưởng năm gần nhất
            if (percents[0] > 0)
            {
                score++;
                type++;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {15, 1 },
                    {35, 1 },
                    {65, 1 },
                    {95, 1 },
                    {125, 1 },
                    {155, 1 },
                    {205, 1 }
                }, (point) => percents[0] > point);
                score += pointLadderScore;
            }
            else if (percents[0] < 0)
            {
                score--;
                type--;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {-0, 1 },
                    {-15, 1 },
                    {-35, 1 },
                    {-65, 1 },
                    {-95, 1 },
                    {-125, 1 },
                    {-155, 1 },
                    {-205, 1 }
                }, (point) => percents[0] < point);
                score -= pointLadderScore;
            }

            //Tính trung bình các năm
            if (avgPercent > 0)
            {
                score++;
                type++;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {15, 1 },
                    {35, 1 },
                    {65, 1 },
                    {95, 1 },
                    {125, 1 },
                    {155, 1 },
                    {205, 1 }
                }, (point) => avgPercent > point);
                score += pointLadderScore;
            }
            else if (avgPercent < 0)
            {
                score--;
                type--;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {-0, 1 },
                    {-15, 1 },
                    {-35, 1 },
                    {-65, 1 },
                    {-95, 1 },
                    {-125, 1 },
                    {-155, 1 },
                    {-205, 1 }
                }, (point) => avgPercent < point);
                score -= pointLadderScore;
            }

            //Phân tích duy trì tăng trưởng
            if (consecutiveEqualCount > 0)
            {
                note += $", duy trì trong {consecutiveEqualCount} năm đến hiện tại.";
            }
            if (consecutiveGrowCount > 0)
            {
                score += consecutiveGrowCount;
                note += $", tăng trong {consecutiveGrowCount} năm đến hiện tại.";
                type++;
            }
            if (consecutiveDropCount > 0)
            {
                score -= consecutiveDropCount;
                note += $", giảm trong {consecutiveDropCount} năm đến hiện tại.";
                type--;
            }

            //Thống kê tổng biến động
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

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// kiểm tra tăng trưởng lợi nhuận theo 4 quy gần nhất
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialIndicators">Danh sách chỉ số tài chính của các công ty cùng ngành</param>
        /// <returns>int</returns>
        public static int QuarterlyProfitGrowthCheck(List<AnalyticsNote> notes, List<FinancialIndicator> financialIndicators)
        {
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";
            var score = 0;
            var type = 0;
            var note = "";
            var checkItems = financialIndicators.Where(q => q.YearReport >= (DateTime.Now.Year - 2) && q.LengthReport != 5).OrderByDescending(q => q.YearReport).ThenByDescending(q => q.LengthReport).Take(4).ToList();//lấy 4 quý cuối cùng để check

            if (checkItems.Count <= 0)
            {
                return notes.Add("Không có thông tin lợi nhuận để đánh giá tăng trưởng quý. ", -5, -2, guideLink);
            }

            foreach (var item in checkItems)
            {
                var posterior = financialIndicators.FirstOrDefault(q => q.YearReport == item.YearReport - 1 && q.LengthReport == item.LengthReport);
                if (posterior is not null)
                {
                    var changePercent = item.Revenue.GetPercent(posterior.Revenue);
                    if (changePercent > 0)
                    {
                        note += $"Lợi nhuận quý {item.LengthReport} năm {item.YearReport} cao hơn cùng kỳ {changePercent:0,0}%. ";
                        score++;
                        type++;
                        var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                            {15, 1 },
                            {35, 1 },
                            {65, 1 },
                            {95, 1 },
                            {125, 1 },
                            {155, 1 },
                            {205, 1 }
                        }, (point) => changePercent > point);
                        score += pointLadderScore;
                    }
                    else if (changePercent < 0)
                    {
                        note += $"Lợi nhuận quý {item.LengthReport} năm {item.YearReport} thấp hơn cùng kỳ {changePercent:0,0}%. ";
                        score--;
                        type--;
                        var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                            {-0, 1 },
                            {-15, 1 },
                            {-35, 1 },
                            {-65, 1 },
                            {-95, 1 },
                            {-125, 1 },
                            {-155, 1 },
                            {-205, 1 }
                        }, (point) => changePercent < point);
                        score -= pointLadderScore;
                    }
                    else
                    {
                        note += $"Lợi nhuận quý {item.LengthReport} năm {item.YearReport} không thay đổi. ";
                    }
                }
                else
                {
                    note += $"Quý {item.LengthReport} năm {item.YearReport} không có thông tin lợi nhuận đối chiếu. ";
                    score--;
                }
            }
            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Đánh giá tài sản
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialGrowths">Danh sách chỉ số tài chính theo năm</param>
        /// <returns>
        /// Tối đa là 7 và tối thiểu là 0
        /// </returns>
        public static int AssetCheck(List<AnalyticsNote> notes, List<FinancialGrowth> financialGrowths)
        {
            var guideLink = "https://govalue.vn/von-chu-so-huu-equity-la-gi";
            var yearRange = 5;
            var financialGrowthsOwnerCapital = financialGrowths.Where(q => q.Year >= (DateTime.Now.Year - yearRange)).OrderByDescending(q => q.Year).ToList();
            if (financialGrowthsOwnerCapital.Count <= 0)
            {
                return notes.Add($"{yearRange} năm gần đây không có thông tin tài sản.", -5, -1, guideLink);
            }

            var fistYear = financialGrowthsOwnerCapital.FirstOrDefault()?.Year ?? 0;
            if (fistYear < (DateTime.Now.Year - 2))
            {
                return notes.Add("Năm gần đây không có thông tin tài sản.", -3, -1, guideLink);
            }

            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = financialGrowthsOwnerCapital.GetFluctuationsTopDown(q => q.Asset);

            if (percents.Count <= 0)
            {
                return notes.Add("không có thông tin tăng trưởng tài sản.", 0, -1, guideLink);
            }

            var avgPercent = percents.Average();
            var type = 0;
            var note = $"Tăng trưởng tài sản trung bình {financialGrowthsOwnerCapital.Count} năm {avgPercent:0.0}%";

            //tín toán % tăng trưởng năm gần nhất
            if (percents[0] > 0)
            {
                score++;
                type++;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {15, 1 },
                    {35, 1 },
                    {65, 1 },
                    {95, 1 },
                    {125, 1 },
                    {155, 1 },
                    {205, 1 }
                }, (point) => percents[0] > point);
                score += pointLadderScore;
            }
            else if (percents[0] < 0)
            {
                score--;
                type--;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {-0, 1 },
                    {-15, 1 },
                    {-35, 1 },
                    {-65, 1 },
                    {-95, 1 },
                    {-125, 1 },
                    {-155, 1 },
                    {-205, 1 }
                }, (point) => percents[0] < point);
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

            return notes.Add(note, score, type, guideLink);
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
        public static int OwnerCapitalCheck(List<AnalyticsNote> notes, List<FinancialGrowth> financialGrowths)
        {
            var guideLink = "https://govalue.vn/von-chu-so-huu-equity-la-gi";
            var yearRange = 5;
            var financialGrowthsOwnerCapital = financialGrowths.Where(q => q.Year >= (DateTime.Now.Year - yearRange)).OrderByDescending(q => q.Year).ToList();
            if (financialGrowthsOwnerCapital.Count <= 0)
            {
                return notes.Add($"{yearRange} năm gần đây không có thông tin vốn chủ sở hữu.", -5, -1, guideLink);
            }

            var fistYear = financialGrowthsOwnerCapital.FirstOrDefault()?.Year ?? 0;
            if (fistYear < (DateTime.Now.Year - 2))
            {
                return notes.Add("Năm gần đây không có thông tin vốn chủ sở hữu.", -1, -1, guideLink);
            }

            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = financialGrowthsOwnerCapital.GetFluctuationsTopDown(q => q.OwnerCapital);

            if (percents.Count <= 0)
            {
                return notes.Add("không có thông tin tăng trưởng vốn chủ sở hữu.", 0, -1, guideLink);
            }

            var avgPercent = percents.Average();
            var type = 0;
            var note = $"Tăng trưởng vốn chủ sở hữu trung bình {financialGrowthsOwnerCapital.Count} năm {avgPercent:0.0}%";

            //tín toán % tăng trưởng năm gần nhất
            if (percents[0] > 0)
            {
                score++;
                type++;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {15, 1 },
                    {35, 1 },
                    {65, 1 },
                    {95, 1 },
                    {125, 1 },
                    {155, 1 },
                    {205, 1 }
                }, (point) => percents[0] > point);
                score += pointLadderScore;
            }
            else if (percents[0] < 0)
            {
                score--;
                type--;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                    {-0, 1 },
                    {-15, 1 },
                    {-35, 1 },
                    {-65, 1 },
                    {-95, 1 },
                    {-125, 1 },
                    {-155, 1 },
                    {-205, 1 }
                }, (point) => percents[0] < point);
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

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Đánh giá chi trả cổ tức bằng tiền mặt
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialGrowths">Danh sách chỉ số tài chính theo năm</param>
        /// <returns>
        /// Tối đa là 7 và tối thiểu là 0
        /// </returns>
        public static int DividendCheck(List<AnalyticsNote> notes, List<FinancialGrowth> financialGrowths)
        {
            var guideLink = "https://govalue.vn/von-chu-so-huu-equity-la-gi";
            var yearRange = 5;
            var financialGrowthsOwnerCapital = financialGrowths.Where(q => q.Year >= (DateTime.Now.Year - yearRange)).OrderByDescending(q => q.Year).ToList();
            if (financialGrowthsOwnerCapital.Count <= 0)
            {
                return notes.Add($"{yearRange} năm gần đây không có thông tin chi trả cổ tức bằng tiền.", -5, -1, guideLink);
            }

            var fistYear = financialGrowthsOwnerCapital.FirstOrDefault()?.Year ?? 0;
            if (fistYear < (DateTime.Now.Year - 2))
            {
                return notes.Add("Năm gần đây không có thông tin chi trả cổ tức bằng tiền.", -1, -1, guideLink);
            }

            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = financialGrowthsOwnerCapital.GetFluctuationsTopDown(q => q.ValuePershare);
            if (percents.Count <= 0)
            {
                return notes.Add("không có thông tin tăng trưởng trả cổ tức bằng tiền.", 0, -1, guideLink);
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

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Kiểm tra tăng trưởng theo năm của chỉ số Eps, 
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialIndicators">Danh sách 5 năm chỉ số tài chính của các công ty</param>
        /// <returns>int</returns>
        public static int YearlyEpsGrowthCheck(List<AnalyticsNote> notes, List<FinancialIndicator> financialIndicators)
        {
            var guideLink = "https://vcbs.com.vn/vn/Utilities/Index/53";
            var yearRange = 5;
            var yearlyEps = financialIndicators.Where(q => q.YearReport >= (DateTime.Now.Year - yearRange) && q.LengthReport == 5).OrderByDescending(q => q.YearReport).ToList();
            if (yearlyEps.Count <= 0)
            {
                return notes.Add($"{yearRange} năm gần đây không có thông tin eps năm.", -5, -1, guideLink);
            }

            var fistYear = yearlyEps.FirstOrDefault()?.YearReport ?? 0;
            if (fistYear < (DateTime.Now.Year - 1))
            {
                return notes.Add("Năm gần đây không có thông tin eps năm.", -3, -1, guideLink);
            }

            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = yearlyEps.GetFluctuationsTopDown(q => q.Eps);

            if (percents.Count <= 0)
            {
                return notes.Add("không có thông tin tăng trưởng eps năm.", -1, -1, guideLink);
            }

            var avgPercent = percents.Average();
            var type = 0;
            var note = $"Tăng trưởng Eps trung bình {yearlyEps.Count} năm {avgPercent:0.0}%, năm gần nhất là {percents[0]:0.0}%";

            if (percents[0] > 0)
            {
                score++;
                type++;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                            {15, 1 },
                            {35, 1 },
                            {65, 1 },
                            {95, 1 },
                            {125, 1 },
                            {155, 1 },
                            {205, 1 }
                        }, (point) => percents[0] > point);
                score += pointLadderScore;
            }
            else if (percents[0] < 0)
            {
                score--;
                type--;
                var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                            {-0, 1 },
                            {-15, 1 },
                            {-35, 1 },
                            {-65, 1 },
                            {-95, 1 },
                            {-125, 1 },
                            {-155, 1 },
                            {-205, 1 }
                        }, (point) => percents[0] < point);
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

            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// kiểm tra tăng trưởng eps theo 4 quy gần nhất, với mỗi kiểm tra cao hơn quý trước được cộng 1 điểm,
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="financialIndicators">Danh sách chỉ số tài chính của các công ty cùng ngành</param>
        /// <returns>int</returns>
        public static int QuarterlyEpsGrowthCheck(List<AnalyticsNote> notes, List<FinancialIndicator> financialIndicators)
        {
            var score = 0;
            var type = 0;
            var note = "";
            var guideLink = "https://vi.wikipedia.org/wiki/Doanh_thu";

            var topFourItem = financialIndicators.Where(q => q.LengthReport != 5).OrderByDescending(q => q.YearReport).ThenByDescending(q => q.LengthReport).Take(4).ToList();
            if (topFourItem.Count <= 0)
            {
                notes.Add("Không có thông tin eps quý để đánh giá tăng trưởng quý.", -5, -2, guideLink);
                return -5;
            }

            foreach (var item in topFourItem)
            {
                var checkItem = financialIndicators.FirstOrDefault(q => q.YearReport == item.YearReport - 1 && q.LengthReport == item.LengthReport);
                if (checkItem is not null)
                {
                    var changePercent = item.Revenue.GetPercent(checkItem.Revenue);
                    if (changePercent > 0)
                    {
                        note += $"Eps quý {item.LengthReport} năm {item.YearReport} cao hơn cùng kỳ {changePercent:0,0}%. ";
                        score++;
                        type++;
                        var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                            {15, 1 },
                            {35, 1 },
                            {65, 1 },
                            {95, 1 },
                            {125, 1 },
                            {155, 1 },
                            {205, 1 }
                        }, (point) => changePercent > point);
                        score += pointLadderScore;
                    }
                    else if (changePercent < 0)
                    {
                        note += $"Eps quý {item.LengthReport} năm {item.YearReport} thấp hơn cùng kỳ {changePercent:0,0}%. ";
                        score--;
                        type--;
                        var (pointLadderScore, pointLadderMaxValue) = Utilities.GetPointLadder(new Dictionary<float, int>() {
                            {-0, 1 },
                            {-15, 1 },
                            {-35, 1 },
                            {-65, 1 },
                            {-95, 1 },
                            {-125, 1 },
                            {-155, 1 },
                            {-205, 1 }
                        }, (point) => changePercent < point);
                        score -= pointLadderScore;
                    }
                    else
                    {
                        note += $"Eps quý {item.LengthReport} năm {item.YearReport} không thay đổi. ";
                    }
                }
            }
            return notes.Add(note, score, type, guideLink);
        }

        /// <summary>
        /// Phân tích tăng trưởng của cổ phiếu bằng đánh giá của fiintrading
        /// </summary>
        /// <param name="notes">Ghi chú đánh giá</param>
        /// <param name="fiinEvaluate">Kết quả đánh giá của fiintrading</param>
        /// <returns></returns>
        public static int FiinGrowthCheck(List<AnalyticsNote> notes, FiinEvaluated? fiinEvaluate)
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
        public static int VndGrowthCheck(List<AnalyticsNote> notes, List<VndStockScore> vndStockScores)
        {
            var type = 0;
            var score = 0;
            var note = string.Empty;

            if (vndStockScores.Count <= 0)
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