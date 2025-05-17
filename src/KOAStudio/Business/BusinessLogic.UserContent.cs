using KOAStudio.Core.Models;
using KOAStudio.Core.ViewModels;
using KOAStudio.Core.Views;
using System.Diagnostics;
using System.Text;

namespace KOAStudio.Business;
internal sealed partial class BusinessLogic
{
    private CharDataReqViewModel? _chartDataReqViewModel_업종;
    private CharDataReqViewModel? _chartDataReqViewModel_주식;
    private CharDataReqViewModel? _chartDataReqViewModel_선물;
    private CharDataReqViewModel? _chartDataReqViewModel_옵션;
    private OrderViewModel? _orderViewModel_주식;
    private OrderViewModel? _orderViewModel_선물옵션;

    void ShowUserContent(string require)
    {
        if (require.Equals("업종차트요청"))
        {
            var model = _chartDataReqViewModel_업종;
            if (model == null)
            {
                _chartDataReqViewModel_업종 = model = new CharDataReqViewModel(CharDataReqViewModel.KIND.업종, require)
                {
                    ExtProcedure = ChartContentExtProcedure,
                    Selected종목 = _appRegistry.GetValue(require, "종목코드", "001"),
                    SelectedChartInterval_분 = _appRegistry.GetValue(require, "분주기", "10"),
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "100"),
                };
                ChartRound chartRound = ChartRound.분;
                _ = Enum.TryParse(_appRegistry.GetValue(require, "시간타입", string.Empty), out chartRound);
                model.SelectedChartRound = chartRound;
            }
            model.NextEnabled = false;
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new CharDataReqView(model));
        }
        else if (require.Equals("주식차트요청"))
        {
            var model = _chartDataReqViewModel_주식;
            if (model == null)
            {
                _chartDataReqViewModel_주식 = model = new CharDataReqViewModel(CharDataReqViewModel.KIND.주식, require)
                {
                    ExtProcedure = ChartContentExtProcedure,
                    Selected종목 = _appRegistry.GetValue(require, "종목코드", "005930"),
                    SelectedChartInterval_분 = _appRegistry.GetValue(require, "분주기", "1"),
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "100"),
                    Is수정주가 = _appRegistry.GetValue(require, "수정주가", defValue: true),
                };
                ChartRound chartRound = ChartRound.일;
                _ = Enum.TryParse(_appRegistry.GetValue(require, "시간타입", string.Empty), out chartRound);
                model.SelectedChartRound = chartRound;
            }
            model.NextEnabled = false;
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new CharDataReqView(model));
        }
        else if (require.Equals("선물차트요청"))
        {
            var model = _chartDataReqViewModel_선물;
            if (model == null)
            {
                _chartDataReqViewModel_선물 = model = new CharDataReqViewModel(CharDataReqViewModel.KIND.선물, require)
                {
                    ExtProcedure = ChartContentExtProcedure,
                    Selected종목 = _appRegistry.GetValue(require, "종목코드", "10100000"),
                    SelectedChartInterval_분 = _appRegistry.GetValue(require, "분주기", "1"),
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "300"),
                };
                ChartRound chartRound = ChartRound.틱;
                _ = Enum.TryParse(_appRegistry.GetValue(require, "시간타입", string.Empty), out chartRound);
                model.SelectedChartRound = chartRound;
            }
            model.NextEnabled = false;
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new CharDataReqView(model));
        }
        else if (require.Equals("옵션차트요청"))
        {
            var model = _chartDataReqViewModel_옵션;
            if (model == null)
            {
                _chartDataReqViewModel_옵션 = model = new CharDataReqViewModel(CharDataReqViewModel.KIND.옵션, require)
                {
                    ExtProcedure = ChartContentExtProcedure,
                    Selected종목 = _appRegistry.GetValue(require, "종목코드", "201TC340"),
                    SelectedChartInterval_분 = _appRegistry.GetValue(require, "분주기", "1"),
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "100"),
                };
                ChartRound chartRound = ChartRound.분;
                _ = Enum.TryParse(_appRegistry.GetValue(require, "시간타입", string.Empty), out chartRound);
                model.SelectedChartRound = chartRound;
            }
            model.NextEnabled = false;
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new CharDataReqView(model));
        }
        else if (require.Equals("주식주문요청"))
        {
            var model = _orderViewModel_주식;
            if (model == null)
            {
                _orderViewModel_주식 = model = new OrderViewModel(require, OrderExtCallProc)
                {
                    종목코드 = _appRegistry.GetValue(require, "종목코드", "005930"),
                };
            }
            if ((model.계좌리스트 == null || !model.계좌리스트.Any()) && _axOpenAPI != null && _axOpenAPI.GetConnectState() == 1)
            {
                var accs = _axOpenAPI.GetLoginInfo("ACCNO").Split(';', StringSplitOptions.RemoveEmptyEntries);
                model.계좌리스트 = accs.Where(x => x.EndsWith("11") || x.EndsWith("10"));
                if (model.계좌리스트.Any())
                {
                    model.Selected계좌 = model.계좌리스트.First();
                }
            }
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new OrderView(model));
        }
        else if (require.Equals("선물옵션주문요청"))
        {
            var model = _orderViewModel_선물옵션;
            if (model == null)
            {
                _orderViewModel_선물옵션 = model = new OrderViewModel(require, OrderExtCallProc)
                {
                    종목코드 = _appRegistry.GetValue(require, "종목코드", "10100000"),
                };
            }
            if ((model.계좌리스트 == null || !model.계좌리스트.Any()) && _axOpenAPI != null && _axOpenAPI.GetConnectState() == 1)
            {
                var accs = _axOpenAPI.GetLoginInfo("ACCNO").Split(';', StringSplitOptions.RemoveEmptyEntries);
                model.계좌리스트 = accs.Where(x => x.EndsWith("31"));
                if (model.계좌리스트.Any())
                {
                    model.Selected계좌 = model.계좌리스트.First();
                }
            }
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new OrderView(model));
        }
    }

    private async Task OrderExtCallProc(OrderViewModel model, string require)
    {

        bool b매수주문 = require.Equals("매수주문");
        bool b매도주문 = require.Equals("매도주문");
        bool b정정주문 = require.Equals("정정주문");
        bool b취소주문 = require.Equals("취소주문");
        bool b조회 = require.Equals("조 회");

        if (b조회)
        {
            if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0)
            {
                OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title} {require}] : 로그인 후 요청해 주세요");
                return;
            }
            if (model.Title.Equals("주식주문요청"))
            {
                if (model.SelectedTabIndex == 0) // 미체결
                {
                    model.MicheItems.Clear();
                    var inputs = new Dictionary<string, string>
                        {
                            { "종목코드", string.Empty },
                            { "조회구분", "0" },
                            { "매도수구분", "0" },
                            { "계좌번호", model.Selected계좌 },
                            { "비밀번호", string.Empty },
                            { "주문번호", string.Empty },
                            { "체결구분", "1" },
                        };
                    var response = await _axOpenAPI.RequestTrAsync("opt10076", inputs, [], ["종목코드", "종목명", "주문번호", "원주문번호", "매매구분", "주문수량", "미체결수량", "주문가격", "주문시간"]);
                    if (response.nErrCode != 0)
                    {
                        OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 미체결 요청오류, [{response.nErrCode}] {response.rsp_msg}");
                        return;
                    }

                    foreach (var data in response.OutputMultiDatas)
                    {
                        string 종목코드 = data[0];
                        string 종목명 = data[1];
                        string 주문번호 = data[2];
                        string 원주문번호 = data[3];
                        bool 매도수구분 = data[4].Equals("1");
                        _ = int.TryParse(data[5], out int 주문수량);
                        _ = int.TryParse(data[6], out int 미체결수량);
                        _ = double.TryParse(data[7], out double 주문가격);
                        string 통화코드 = "KRW";
                        string 주문시간 = data[8];
                        model.MicheItems.Add(new MicheItem(
                            종목코드, 종목명, 주문번호, 원주문번호, 매도수구분, 주문수량, 미체결수량, 주문가격, 통화코드, 주문시간
                            ));
                    }
                }
                else if (model.SelectedTabIndex == 1) //잔고"
                {
                    model.JangoItems.Clear();
                    var response = await _axOpenAPI.RequestTrAsync("opt10085", [("계좌번호", model.Selected계좌)], [], ["종목코드", "종목명", "보유수량", "매입가", "현재가"]);
                    if (response.nErrCode != 0)
                    {
                        OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 잔고 요청오류, [{response.nErrCode}] {response.rsp_msg}");
                        return;
                    }
                    foreach (var data in response.OutputMultiDatas)
                    {
                        string 종목코드 = data[0];
                        string 종목명 = data[1];
                        _ = int.TryParse(data[2], out int 보유수량);
                        _ = double.TryParse(data[3], out double 매입가);
                        _ = double.TryParse(data[4], out double 현재가);
                        bool 매도수구분 = false;
                        double 평가손익 = 0;
                        string 통화코드 = "KRW";
                        model.JangoItems.Add(new JangoItem(
                            종목코드, 종목명, 매도수구분, 보유수량, 매입가, 현재가, 평가손익, 통화코드
                            ));

                    }
                }
            }
            else if (model.Title.Equals("선물옵션주문요청"))
            {
                if (model.SelectedTabIndex == 0) // 미체결
                {
                    model.MicheItems.Clear();
                    var inputs = new Dictionary<string, string>
                        {
                            { "종목코드", string.Empty },
                            { "조회구분", "0" },
                            { "매매구분", "0" },
                            { "체결구분", "1" },
                            { "계좌번호", model.Selected계좌 },
                            { "주문번호", string.Empty },
                        };
                    var response = await _axOpenAPI.RequestTrAsync("OPT50026", inputs, [], ["종목코드", "종목명", "주문번호", "원주문번호", "매매구분", "주문수량", "미체결수량", "주문가격", "주문체결시간"]);
                    if (response.nErrCode != 0)
                    {
                        OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 미체결 요청오류, [{response.nErrCode}] {response.rsp_msg}");
                        return;
                    }
                    foreach (var data in response.OutputMultiDatas)
                    {
                        string 종목코드 = data[0];
                        string 종목명 = data[1];
                        string 주문번호 = data[2];
                        string 원주문번호 = data[3];
                        bool 매도수구분 = data[4].Equals("1");
                        _ = int.TryParse(data[5], out int 주문수량);
                        _ = int.TryParse(data[6], out int 미체결수량);
                        _ = double.TryParse(data[7], out double 주문가격);
                        string 통화코드 = "KRW";
                        string 주문시간 = data[8];
                        model.MicheItems.Add(new MicheItem(
                            종목코드, 종목명, 주문번호, 원주문번호, 매도수구분, 주문수량, 미체결수량, 주문가격, 통화코드, 주문시간
                            ));
                    }
                }
                else if (model.SelectedTabIndex == 1) //잔고"
                {
                    model.JangoItems.Clear();
                    var response = await _axOpenAPI.RequestTrAsync("OPT50027", [("계좌번호", model.Selected계좌)], [], ["종목코드", "종목명", "매매구분", "보유수량", "매입단가", "현재가"]);
                    if (response.nErrCode != 0)
                    {
                        OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 잔고 요청오류, [{response.nErrCode}] {response.rsp_msg}");
                        return;
                    }
                    foreach (var data in response.OutputMultiDatas)
                    {
                        string 종목코드 = data[0];
                        string 종목명 = data[1];
                        bool 매도수구분 = data[2].Equals("1");
                        _ = int.TryParse(data[3], out int 보유수량);
                        _ = double.TryParse(data[4], out double 매입가);
                        _ = double.TryParse(data[5], out double 현재가);
                        double 평가손익 = 0;
                        string 통화코드 = "KRW";
                        model.JangoItems.Add(new JangoItem(
                            종목코드, 종목명, 매도수구분, 보유수량, 매입가, 현재가, 평가손익, 통화코드
                            ));
                    }
                }
            }

            return;
        }
        string sRQName = model.Title + $" {model.매매구분}";
        string sScreenNo = _scrNum_ORDER_CONTENT;
        string sAccNo = model.Selected계좌;
        string sCode = model.종목코드;
        int nQty = model.주문수량;
        string sOrgOrderNo = model.매매구분 switch // 원주문번호
        {
            OrderType.매수 => string.Empty,
            OrderType.매도 => string.Empty,
            OrderType.정정취소 => model.주문번호,
            _ => string.Empty,
        };

        if (model.Title.Equals("주식주문요청"))
        {
            int nOrderType = model.매매구분 switch //  (1:신규매도, 2:신규매수 3:매도취소, 4:매수취소, 5:매도정정, 6:매수정정)
            {
                OrderType.매수 => 1,
                OrderType.매도 => 2,
                OrderType.정정취소 => model.원주문매도수구분 ? 6 : 5,
                _ => 0,
            };
            // 취소주문 -= 2;

            _ = int.TryParse(model.주문가격, out int nPrice);
            if ((model.매매구분 == OrderType.매수 || model.매매구분 == OrderType.매도) && model.주문종류 == OrderKind.시장가) nPrice = 0;
            string sHogaGb = model.주문종류 switch // 거래구분 (1:시장가, 2:지정가)
            {
                OrderKind.지정가 => "00",
                OrderKind.시장가 => "03",
                _ => "00",
            };

            StringBuilder stringBuilder = new();

            if (model.매매구분 == OrderType.정정취소)
            {
                sRQName = $"{model.Title} 정정";
                stringBuilder.AppendLine($"// {sRQName}");
                stringBuilder.AppendLine($"(int nRet, string sMsg) = await _axOpenAPI.SendOrderAsync(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, {nPrice}, \"{sHogaGb}\", \"{sOrgOrderNo}\");");

                nOrderType -= 2;
                sRQName = $"{model.Title} 취소";
                stringBuilder.AppendLine($"// {sRQName}");
                stringBuilder.AppendLine($"(int nRet, string sMsg) = await _axOpenAPI.SendOrderAsync(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, {nPrice}, \"{sHogaGb}\", \"{sOrgOrderNo}\");");
            }
            else
            {
                stringBuilder.AppendLine($"// {sRQName}");
                stringBuilder.AppendLine($"(int nRet, string sMsg) = await _axOpenAPI.SendOrderAsync(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, {nPrice}, \"{sHogaGb}\", \"{sOrgOrderNo}\");");
            }
            stringBuilder.AppendLine($"Output($\"nRet={{nRet}}, sMsg={{sMsg}}\");");

            model.CodeText = stringBuilder.ToString();
            if (b매수주문 || b매도주문 || b정정주문 || b취소주문 || b조회)
            {
                if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0)
                {
                    OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title} {require}] : 로그인 후 요청해 주세요");
                    return;
                }
                if (!model.주문확인생략)
                {
                    var msgWindow = new MessageOkCancel($"{model.종목코드} {model.주문수량}계약\r\n {require} 하시겠습니까?", $"{require}");
                    if (msgWindow.ShowDialog() != true)
                    {
                        return;
                    }
                }

                sRQName = model.Title + " " + ((model.매매구분 != OrderType.정정취소) ? $"{model.매매구분}" : (b정정주문 ? "정정" : "취소"));

                // 취소경우 -= 2;
                if (b취소주문) nOrderType -= 2;

                if ((model.매매구분 == OrderType.매수 || model.매매구분 == OrderType.매도) && model.주문종류 == OrderKind.시장가) nPrice = 0;

                DateTime CallTime = DateTime.Now;
                Stopwatch stopwatch = Stopwatch.StartNew();
                (int nRet, string sMsg) = await _axOpenAPI.SendOrderAsync(sRQName, sScreenNo, sAccNo, nOrderType, sCode, nQty, nPrice, sHogaGb, sOrgOrderNo);
                stopwatch.Stop();
                model.CodeText += $"[{CallTime:HH:mm:ss.fff}] : ({stopwatch.Elapsed.TotalMilliseconds})mS, nRet={nRet}, sMsg={sMsg}\r\n";
            }
        }
        else if (model.Title.Equals("선물옵션주문요청"))
        {
            int lOrdKind = model.매매구분 switch //  (1:신규매도, 2:신규매수 3:매도취소, 4:매수취소, 5:매도정정, 6:매수정정)
            {
                OrderType.매수 => 1,
                OrderType.매도 => 1,
                OrderType.정정취소 => 2,
                _ => 0,
            };
            // 취소주문 lOrdKind += 1;
            string sSlbyTp = model.매매구분 switch //  매매구분	1: 매도, 2:매수
            {
                OrderType.매수 => "2",
                OrderType.매도 => "1",
                OrderType.정정취소 => model.원주문매도수구분 ? "1" : "2",
                _ => "0",
            };

            string sOrdTp = model.주문종류 switch // 거래구분 (1:지정가, 3:시장가)
            {
                OrderKind.지정가 => "1",
                OrderKind.시장가 => "3",
                _ => "0",
            };
            string sPrice = model.매매구분 switch
            {
                OrderType.매수 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
                OrderType.매도 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
                OrderType.정정취소 => model.주문가격,
                _ => model.주문가격,
            };

            StringBuilder stringBuilder = new();

            if (model.매매구분 == OrderType.정정취소)
            {
                sRQName = $"{model.Title} 정정";
                stringBuilder.AppendLine($"// {sRQName}");
                stringBuilder.AppendLine($"(int nRet, string sMsg) = await _axOpenAPI.SendOrderFOAsync(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", \"{sCode}\", {lOrdKind}, \"{sSlbyTp}\", \"{sOrdTp}\", {nQty}, \"{sPrice}\", \"{sOrgOrderNo}\");");

                lOrdKind += 1;
                sRQName = $"{model.Title} 취소";
                stringBuilder.AppendLine($"// {sRQName}");
                stringBuilder.AppendLine($"(int nRet, string sMsg) = await _axOpenAPI.SendOrderFOAsync(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", \"{sCode}\", {lOrdKind}, \"{sSlbyTp}\", \"{sOrdTp}\", {nQty}, \"{sPrice}\", \"{sOrgOrderNo}\");");
            }
            else
            {
                stringBuilder.AppendLine($"// {sRQName}");
                stringBuilder.AppendLine($"(int nRet, string sMsg) = await _axOpenAPI.SendOrderFOAsync(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", \"{sCode}\", {lOrdKind}, \"{sSlbyTp}\", \"{sOrdTp}\", {nQty}, \"{sPrice}\", \"{sOrgOrderNo}\");");
            }
            stringBuilder.AppendLine($"Output($\"nRet={{nRet}}, sMsg={{sMsg}}\");");

            model.CodeText = stringBuilder.ToString();

            if (b매수주문 || b매도주문 || b정정주문 || b취소주문 || b조회)
            {
                if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0)
                {
                    OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title} {require}] : 로그인 후 요청해 주세요");
                    return;
                }
                if (!model.주문확인생략)
                {
                    var msgWindow = new MessageOkCancel($"{model.종목코드} {model.주문수량}계약\r\n {require} 하시겠습니까?", $"{require}");
                    if (msgWindow.ShowDialog() != true)
                    {
                        return;
                    }
                }

                sRQName = model.Title + " " + ((model.매매구분 != OrderType.정정취소) ? $"{model.매매구분}" : (b정정주문 ? "정정" : "취소"));

                if (b취소주문) lOrdKind += 1;

                DateTime CallTime = DateTime.Now;
                Stopwatch stopwatch = Stopwatch.StartNew();
                (int nRet, string sMsg) = await _axOpenAPI.SendOrderFOAsync(sRQName, sScreenNo, sAccNo, sCode, lOrdKind, sSlbyTp, sOrdTp, nQty, sPrice, sOrgOrderNo);
                stopwatch.Stop();
                model.CodeText += $"[{CallTime:HH:mm:ss.fff}] : ({stopwatch.Elapsed.TotalMilliseconds})mS, nRet={nRet}, sMsg={sMsg}\r\n";
            }
        }

        return;
    }

    private async Task ChartContentExtProcedure(CharDataReqViewModel model, string require)
    {
        string trCode;
        Dictionary<string, string> inputs = [];
        List<string> multies = ["체결시간", "시가", "고가", "저가", "현재가", "거래량"];
        if (model.Kind == CharDataReqViewModel.KIND.업종)
        {
            trCode = model.SelectedChartRound switch
            {
                ChartRound.틱 => "opt20004",
                ChartRound.분 => "opt20005",
                ChartRound.일 => "opt20006",
                ChartRound.주 => "opt20007",
                ChartRound.월 => "opt20008",
                _ => throw new NotSupportedException(),
            };

            inputs.Add("업종코드", model.Selected종목);
            switch (model.SelectedChartRound)
            {
                case ChartRound.틱:
                case ChartRound.분:
                    inputs.Add("틱범위", model.SelectedChartInterval);
                    break;
                case ChartRound.일:
                case ChartRound.주:
                case ChartRound.월:
                    inputs.Add("기준일자", model.조회일자.ToString("yyyyMMdd"));
                    multies[0] = "일자";
                    break;
            }
        }
        else if (model.Kind == CharDataReqViewModel.KIND.주식)
        {
            trCode = model.SelectedChartRound switch
            {
                ChartRound.틱 => "opt10079",
                ChartRound.분 => "opt10080",
                ChartRound.일 => "opt10081",
                ChartRound.주 => "opt10082",
                ChartRound.월 => "opt10083",
                _ => throw new NotSupportedException(),
            };

            inputs.Add("종목코드", model.Selected종목);
            switch (model.SelectedChartRound)
            {
                case ChartRound.틱:
                case ChartRound.분:
                    inputs.Add("틱범위", model.SelectedChartInterval);
                    break;
                case ChartRound.일:
                    inputs.Add("기준일자", model.조회일자.ToString("yyyyMMdd"));
                    multies[0] = "일자";
                    break;
                case ChartRound.주:
                case ChartRound.월:
                    inputs.Add("기준일자", model.조회일자.ToString("yyyyMMdd"));
                    inputs.Add("끝일자", "");
                    multies[0] = "일자";
                    break;
            }
            inputs.Add("수정주가구분", (model.Is수정주가 == true) ? "1" : "0");
        }
        else if (model.Kind == CharDataReqViewModel.KIND.선물)
        {
            trCode = model.SelectedChartRound switch
            {
                ChartRound.틱 => "opt50028",
                ChartRound.분 => "opt50029",
                ChartRound.일 => "opt50030",
                ChartRound.주 => "opt50071",
                ChartRound.월 => "opt50072",
                _ => throw new NotSupportedException(),
            };

            inputs.Add("종목코드", model.Selected종목);
            switch (model.SelectedChartRound)
            {
                case ChartRound.틱:
                case ChartRound.분:
                    inputs.Add("시간단위", model.SelectedChartInterval);
                    break;
                case ChartRound.일:
                case ChartRound.주:
                case ChartRound.월:
                    inputs.Add("기준일자", model.조회일자.ToString("yyyyMMdd"));
                    multies[0] = "일자";
                    multies[5] = "누적거래량"; // 선물옵션은 누적거래량
                    break;
            }
        }
        else if (model.Kind == CharDataReqViewModel.KIND.옵션)
        {
            trCode = model.SelectedChartRound switch
            {
                ChartRound.틱 => "opt50066",
                ChartRound.분 => "opt50067",
                ChartRound.일 => "opt50068",
                _ => "opt50068",
            };

            inputs.Add("종목코드", model.Selected종목);
            switch (model.SelectedChartRound)
            {
                case ChartRound.틱:
                case ChartRound.분:
                    inputs.Add("시간단위", model.SelectedChartInterval);
                    break;
                case ChartRound.일:
                    inputs.Add("기준일자", model.조회일자.ToString("yyyyMMdd"));
                    multies[0] = "일자";
                    multies[5] = "누적거래량"; // 선물옵션은 누적거래량
                    break;
            }
        }
        else
            throw new NotSupportedException();

        bool b조회 = require.Equals("조 회");
        bool b다음 = require.Equals("다 음");

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"// {model.Title}");
        stringBuilder.AppendLine("var inputs = new Dictionary<string, string>()");
        stringBuilder.AppendLine("    {");
        foreach (var input in inputs)
        {
            stringBuilder.AppendLine($"        {{\"{input.Key}\", \"{input.Value}\"}},");
        }
        stringBuilder.AppendLine("    };");
        stringBuilder.Append($"var response = await _axOpenAPI.RequestTrAsync(\"{trCode}\", inputs, [], [");
        for (int i = 0; i < multies.Count; i++)
        {
            stringBuilder.Append($"\"{multies[i]}\"");
            if (i < multies.Count - 1)
                stringBuilder.Append(", ");
        }
        stringBuilder.Append(']');
        if (b다음)
            stringBuilder.Append($", \"{model.NextText}\"");
        stringBuilder.AppendLine(");");
        stringBuilder.AppendLine($"Output(response.OutputMultiDatas);");

        model.CodeText = stringBuilder.ToString();
        if (b조회 || b다음)
        {
            if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0)
            {
                OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 로그인 후 요청해 주세요");
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            var response = await _axOpenAPI.RequestTrAsync(trCode, inputs, [], multies, b다음 ? model.NextText : string.Empty);
            stopwatch.Stop();
            model.ReceivedTime = DateTime.Now;
            var datas = response.OutputMultiDatas;
            model.ReceivedDataCount = datas.Count;
            StringBuilder sb = new StringBuilder();
            if (b조회)
            {
                for (int i = 0; i < response.InMultiFields.Length; i++)
                {
                    sb.Append(response.InMultiFields[i]);
                    if (i < response.InMultiFields.Length - 1)
                        sb.Append(", ");
                }
                sb.AppendLine();
            }
            foreach (var data in datas)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i]);
                    if (i < data.Length - 1)
                        sb.Append(", ");
                }
                sb.AppendLine();
            }
            model.ResultText = sb.ToString();
            model.NextText = response.cont_key;
            model.NextEnabled = !string.IsNullOrEmpty(response.cont_key);
        }

        return;
    }

    private void SaveUserContentInfo()
    {
        if (_chartDataReqViewModel_업종 != null)
        {
            var model = _chartDataReqViewModel_업종;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.Selected종목);
            _appRegistry.SetValue(section, "시간타입", model.SelectedChartRound);
            _appRegistry.SetValue(section, "분주기", model.SelectedChartInterval_분);
            _appRegistry.SetValue(section, "틱주기", model.SelectedChartInterval_틱);
        }
        if (_chartDataReqViewModel_주식 != null)
        {
            var model = _chartDataReqViewModel_주식;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.Selected종목);
            _appRegistry.SetValue(section, "시간타입", model.SelectedChartRound);
            _appRegistry.SetValue(section, "분주기", model.SelectedChartInterval_분);
            _appRegistry.SetValue(section, "틱주기", model.SelectedChartInterval_틱);
            _appRegistry.SetValue(section, "수정주가", model.Is수정주가);
        }
        if (_chartDataReqViewModel_선물 != null)
        {
            var model = _chartDataReqViewModel_선물;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.Selected종목);
            _appRegistry.SetValue(section, "시간타입", model.SelectedChartRound);
            _appRegistry.SetValue(section, "분주기", model.SelectedChartInterval_분);
            _appRegistry.SetValue(section, "틱주기", model.SelectedChartInterval_틱);
        }
        if (_chartDataReqViewModel_옵션 != null)
        {
            var model = _chartDataReqViewModel_옵션;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.Selected종목);
            _appRegistry.SetValue(section, "시간타입", model.SelectedChartRound);
            _appRegistry.SetValue(section, "분주기", model.SelectedChartInterval_분);
            _appRegistry.SetValue(section, "틱주기", model.SelectedChartInterval_틱);
        }
        if (_orderViewModel_주식 != null)
        {
            var model = _orderViewModel_주식;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.종목코드);
        }
        if (_orderViewModel_선물옵션 != null)
        {
            var model = _orderViewModel_선물옵션;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.종목코드);
        }
    }
}
