using KHOpenApi.NET;
using KOAStudio.Core.Models;
using KOAStudio.Core.ViewModels;
using System.Diagnostics;
using System.Text;

namespace KOAStudio.Business;

internal sealed partial class BusinessLogic
{
    private void AxKHOpenApi_OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
    {
        DateTime Now = DateTime.Now;
        int nRepeatCount = _axOpenAPI!.GetRepeatCnt(e.sTrCode, e.sRQName);
        OutputLog((int)TAB_LIST_KIND.메시지목록, $"<OnReceiveTrData> sScrNo = {e.sScrNo},  sRQName = {e.sRQName}, sTrCode = {e.sTrCode}, sRecordName = {e.sRecordName}, sPrevNext = {e.sPrevNext}");

        if (string.Equals(e.sScrNo, _scrNum_CHART_CONTENT))
        {
            CharDataReqViewModel? model = null;
            if (_chartDataReqViewModel_업종 != null && e.sRQName.StartsWith(_chartDataReqViewModel_업종.Title))
            {
                model = _chartDataReqViewModel_업종;
            }
            else if (_chartDataReqViewModel_주식 != null && e.sRQName.StartsWith(_chartDataReqViewModel_주식.Title))
            {
                model = _chartDataReqViewModel_주식;
            }
            else if (_chartDataReqViewModel_선물 != null && e.sRQName.StartsWith(_chartDataReqViewModel_선물.Title))
            {
                model = _chartDataReqViewModel_선물;
            }
            else if (_chartDataReqViewModel_옵션 != null && e.sRQName.StartsWith(_chartDataReqViewModel_옵션.Title))
            {
                model = _chartDataReqViewModel_옵션;
            }

            if (model != null)
            {
                bool bFuture = model.Kind == CharDataReqViewModel.KIND.선물 || model.Kind == CharDataReqViewModel.KIND.옵션;
                StringBuilder sb = new();

                for (int i = 0; i < nRepeatCount; i++)
                {
                    bool b분틱 = model.SelectedChartRound == ChartRound.분 || model.SelectedChartRound == ChartRound.틱;
                    sb.Append(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, b분틱 ? "체결시간" : "일자"));
                    sb.Append(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가"));
                    sb.Append(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가"));
                    sb.Append(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가"));
                    sb.Append(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                    sb.AppendLine(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, (bFuture && !b분틱) ? "누적거래량" : "거래량"));
                }

                model.ReceivedTime = Now;
                model.ReceivedDataCount = nRepeatCount;
                model.ResultText = sb.ToString();
                model.NextText = e.sPrevNext;
                model.NextEnabled = e.sPrevNext.Equals("2");
            }
        }
        else if (e.sScrNo.Equals(_scrNum_ORDER_CONTENT))
        {
            if (_orderViewModel_주식 != null && e.sTrCode.Equals("opt10076")) // 미체결
            {
                var model = _orderViewModel_주식;
                model.MicheItems.Clear();

                for (int i = 0; i < nRepeatCount; i++)
                {
                    string 종목코드 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string 종목명 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    string 주문번호 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문번호").Trim();
                    string 원주문번호 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "원주문번호").Trim();
                    bool 매도수구분 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매매구분").Trim().Equals("1");
                    int.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문수량").Trim(), out int 주문수량);
                    int.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "미체결수량").Trim(), out int 미체결수량);
                    double.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문가격").Trim(), out double 주문가격);
                    string 통화코드 = "KRW";
                    string 주문시각 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문시간").Trim();
                    model.MicheItems.Add(new MicheItem(
                        종목코드, 종목명, 주문번호, 원주문번호, 매도수구분, 주문수량, 미체결수량, 주문가격, 통화코드, 주문시각
                        ));
                }
            }
            else if (_orderViewModel_주식 != null && e.sTrCode.Equals("opt10085")) // 잔고
            {
                var model = _orderViewModel_주식;
                model.JangoItems.Clear();

                for (int i = 0; i < nRepeatCount; i++)
                {
                    string 종목코드 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string 종목명 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    bool 매도수구분 = false;
                    int.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim(), out int 수량);
                    double.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim(), out double 평균단가);
                    double.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim(), out double 현재가격);
                    double 평가손익 = 0;
                    string 통화코드 = "KRW";
                    model.JangoItems.Add(new JangoItem(
                        종목코드, 종목명, 매도수구분, 수량, 평균단가, 현재가격, 평가손익, 통화코드
                        ));
                }
            }
            else if (_orderViewModel_선물옵션 != null && e.sTrCode.Equals("OPT50026")) // 미체결
            {
                var model = _orderViewModel_선물옵션;
                model.MicheItems.Clear();

                for (int i = 0; i < nRepeatCount; i++)
                {
                    string 종목코드 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string 종목명 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    string 주문번호 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문번호").Trim();
                    string 원주문번호 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "원주문번호").Trim();
                    bool 매도수구분 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매매구분").Trim().Equals("1");
                    int.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문수량").Trim(), out int 주문수량);
                    int.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "미체결수량").Trim(), out int 미체결수량);
                    double.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문가격").Trim(), out double 주문가격);
                    string 통화코드 = "KRW";
                    string 주문시각 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문체결시간").Trim();
                    model.MicheItems.Add(new MicheItem(
                        종목코드, 종목명, 주문번호, 원주문번호, 매도수구분, 주문수량, 미체결수량, 주문가격, 통화코드, 주문시각
                        ));
                }
            }
            else if (_orderViewModel_선물옵션 != null && e.sTrCode.Equals("OPT50027")) // 잔고
            {
                var model = _orderViewModel_선물옵션;
                model.JangoItems.Clear();

                for (int i = 0; i < nRepeatCount; i++)
                {
                    string 종목코드 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string 종목명 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    bool 매도수구분 = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매매구분").Trim().Equals("1");
                    int.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim(), out int 수량);
                    double.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입단가").Trim(), out double 평균단가);
                    double.TryParse(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim(), out double 현재가격);
                    double 평가손익 = 0;
                    string 통화코드 = "KRW";
                    model.JangoItems.Add(new JangoItem(
                        종목코드, 종목명, 매도수구분, 수량, 평균단가, 현재가격, 평가손익, 통화코드
                        ));
                }
            }
        }


        {
            SetPropertyQueryNextEnable(e.sPrevNext.Equals("2"));
            // TR코드 필드 찾기
            var trData = _trDatas.Find(x => x.Code.Equals(e.sTrCode));
            if (trData is not null)
            {
                Stopwatch timer = Stopwatch.StartNew();
                string[] lines = new string[1
                    + (trData.OutputSingle != null ? trData.OutputSingle.Count : 0)
                    + nRepeatCount * (trData.OutputMuti != null ? trData.OutputMuti.Count : 0)];
                string TimeLine = DateTime.Now.ToString("HH:mm:ss.fff : ");
                int nLineIndex = 0;
                lines[nLineIndex++] = $"{TimeLine}[{trData.Name}] [{e.sRQName}] Count={nRepeatCount} PrevNext={e.sPrevNext}";
                if (trData.OutputSingle != null)
                    foreach (var field in trData.OutputSingle)
                    {
                        string value = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, field).Trim();
                        lines[nLineIndex++] = $"[{e.sTrCode}] {field}={value}";
                    }
                if (trData.OutputMuti != null)
                    for (int i = 0; i < nRepeatCount; i++)
                    {
                        foreach (var field in trData.OutputMuti)
                        {
                            string value = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, field).Trim();
                            lines[nLineIndex++] = $"[{e.sTrCode}][{i}] {field}={value}";
                        }
                    }
                timer.Stop();
                int nTotalDataCount = 0;
                if (trData.OutputSingle != null) nTotalDataCount += trData.OutputSingle.Count;
                if (trData.OutputMuti != null) nTotalDataCount += (trData.OutputMuti.Count * nRepeatCount);
                lines[0] = string.Format("{0}, 데이터수집 ({1:n0}개, {2:n0}uS)"
                    , lines[0]
                    , nTotalDataCount
                    , timer.Elapsed.TotalMilliseconds * 1000);
                OutputLog((int)TAB_LIST_KIND.조회데이터);
                OutputLog((int)TAB_LIST_KIND.조회데이터, lines, -1, focus: true);

                // 최근 조회목록창에 추가
                OutputLog((int)TAB_LIST_KIND.조회한TR목록, $"{trData.Code} : {trData.Name}");
            }
        }
    }

    private void AxKHOpenApi_OnReceiveTrCondition(object sender, _DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.메시지목록, $"<OnReceiveTrCondition> sScrNo = {e.sScrNo}, strConditionName = {e.strConditionName}, nIndex = {e.nIndex}, nNext = {e.nNext}");

        int nScrNo = Convert.ToInt32(e.sScrNo);
        if (nScrNo >= Convert.ToInt32(_scrNum_REQ_COND_BASE) && nScrNo <= Convert.ToInt32(_scrNum_REQ_COND_LAST))
        {
            // 조건검색 요청 결과
            string[] szCodeAndPrices = e.strCodeList.Split(';', StringSplitOptions.RemoveEmptyEntries);
            StringBuilder szViewText = new();
            szViewText.AppendLine();
            szViewText.AppendLine("----------------------조건검색 요청 결과----------------------");
            szViewText.AppendLine();
            szViewText.AppendLine($"화면번호 : {e.sScrNo}");
            szViewText.AppendLine($"조건식 이름 : {e.strConditionName}");
            szViewText.AppendLine($"조건식 고유번호 : {e.nIndex}");
            szViewText.AppendLine($"연속조회 여부 : {e.nNext}");
            szViewText.AppendLine($"종목개수 : {szCodeAndPrices.Length}");
            szViewText.AppendLine();
            szViewText.AppendLine($"**********");

            foreach (var s in szCodeAndPrices)
            {
                string[] strings = s.Split('^', StringSplitOptions.RemoveEmptyEntries);
                if (strings.Length > 0)
                {
                    string code = strings[0];
                    string name = _axOpenAPI!.GetMasterCodeName(code);
                    if (strings.Length > 1)
                    {
                        string sPrice = strings[1];
                        double dPrice = Convert.ToDouble(sPrice);
                        double dLastPrice = Convert.ToDouble(_axOpenAPI.GetMasterLastPrice(code));
                        szViewText.AppendLine(string.Format("[{0}] : {1} ({2}, {3:N2}%)"
                            , code, name, (int)dPrice, (dPrice - dLastPrice) / dLastPrice * 100.0
                            ));
                    }
                    else
                        szViewText.AppendLine($"[{code}] : {name}");
                }
            }
            szViewText.AppendLine("---------------------------------------------------------");
            SetResultText(szViewText.ToString());
        }
    }

    private void AxKHOpenApi_OnReceiveRealData(object sender, _DKHOpenAPIEvents_OnReceiveRealDataEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.실시간데이터, $"sRealKey = {e.sRealKey}, sRealType = {e.sRealType}, sRealData = {e.sRealData}", 100, focus: false);
    }

    private void AxKHOpenApi_OnReceiveRealCondition(object sender, _DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.조건검색실시간, $"sTrCode = {e.sTrCode}, strType = {e.strType}, strConditionName = {e.strConditionName}, strConditionIndex = {e.strConditionIndex}");
    }

    private void AxKHOpenApi_OnReceiveMsg(object sender, _DKHOpenAPIEvents_OnReceiveMsgEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.메시지목록, $"<OnReceiveMsg> sScrNo = {e.sScrNo}, sRQName = {e.sRQName}, sTrCode = {e.sTrCode}, sMsg = {e.sMsg}");
        SetStatusText(e.sMsg);
    }

    private void AxKHOpenApi_OnReceiveInvestRealData(object sender, _DKHOpenAPIEvents_OnReceiveInvestRealDataEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.메시지목록, $"<OnReceiveInvestRealData> sRealKey = {e.sRealKey}");
    }

    private void AxKHOpenApi_OnReceiveConditionVer(object sender, _DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.메시지목록, $"<OnReceiveConditionVer> lRet = {e.lRet}, sMsg = {e.sMsg}");

        if (e.lRet == 1) // 정상
        {
            // 조건검색기 항목 로딩
            if (_mapCondNameToIndex.Count > 0) return;
            string[] lists = _axOpenAPI!.GetConditionNameList().Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in lists)
            {
                string[] datas = item.Split('^');
                _mapCondNameToIndex.Add(datas[1], datas[0]);
            }
            OutputLog((int)TAB_LIST_KIND.메시지목록, $"<OnReceiveConditionVer> 조건검색식({_mapCondNameToIndex.Count}개) 로딩완료");

            Load_사용자기능();
        }
    }

    private void AxKHOpenApi_OnReceiveChejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.실시간주문체결, $"sGubun = {e.sGubun}, nItemCnt = {e.nItemCnt}, sFIdList = {e.sFIdList}", 300);
        string[] szFids = e.sFIdList.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var sFid in szFids)
        {
            string sVal = _axOpenAPI!.GetChejanData(Convert.ToInt32(sFid));
            if (_map_FidToName.TryGetValue(sFid, out var name))
            {
                OutputLog((int)TAB_LIST_KIND.실시간주문체결, $"\t[{sFid}][{name}] = {sVal}", 300);
            }
            else
            {
                OutputLog((int)TAB_LIST_KIND.실시간주문체결, $"\t[{sFid}][NOFIDNAME] = {sVal}", 300);
            }
        }
    }

    private void AxKHOpenApi_OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.메시지목록, $"<OnReceiveEventConnect> nErrCode = {e.nErrCode}");
        if (e.nErrCode == 0)
        {
            _isRealServer = !string.Equals(_axOpenAPI!.GetLoginInfo("GetServerGubun"), "1");
            LoginState = OpenApiLoginState.LoginSucceed;

            _apiFolder = _axOpenAPI.GetAPIModulePath();

            Load_실시간목록Async();
            Load_TR목록Async();
            Load_화면목록Async();
            Load_개발가이드Async();

            Load_종목정보();

            // 조건검색 로딩, 결과 이벤트 OnReceiveConditionVer
            _axOpenAPI.GetConditionLoad();
        }
        else
        {
            LoginState = OpenApiLoginState.LoginOuted;
        }
    }
}
