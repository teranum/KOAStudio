using KHOpenApi.NET;
using KOAStudio.Core.Helpers;
using KOAStudio.Core.Models;
using System.Diagnostics;
using System.Text;

namespace KOAStudio.Business;

internal sealed partial class BusinessLogic
{
    private void AxKHOpenApi_OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
    {
        OutputLog((int)LIST_TAB_KIND.메시지목록, $"<OnReceiveTrData> sScrNo = {e.sScrNo},  sRQName = {e.sRQName}, sTrCode = {e.sTrCode}, sRecordName = {e.sRecordName}, sPrevNext = {e.sPrevNext}");
        //if (e.sScrNo == SCR_REQ_TR_BASE)
        {
            SetPropertyQueryNextEnable(e.sPrevNext.Equals("2"));
            // TR코드 필드 찾기
            var trData = _trDatas.Find(x => x.Code.Equals(e.sTrCode));
            if (trData is not null)
            {
                Stopwatch timer = Stopwatch.StartNew();
                int nRepeatCount = _axOpenAPI!.GetRepeatCnt(e.sTrCode, e.sRQName);
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
                OutputLog((int)LIST_TAB_KIND.조회데이터);
                OutputLog((int)LIST_TAB_KIND.조회데이터, lines, -1, focus: true);

                // 최근 조회목록창에 추가
                OutputLog((int)LIST_TAB_KIND.조회한TR목록, $"{trData.Code} : {trData.Name}");
            }
        }
    }

    private void AxKHOpenApi_OnReceiveTrCondition(object sender, _DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
    {
        OutputLog((int)LIST_TAB_KIND.메시지목록, $"<OnReceiveTrCondition> sScrNo = {e.sScrNo}, strConditionName = {e.strConditionName}, nIndex = {e.nIndex}, nNext = {e.nNext}");

        int nScrNo = Convert.ToInt32(e.sScrNo);
        if (nScrNo >= Convert.ToInt32(SCR_REQ_COND_BASE) && nScrNo <= Convert.ToInt32(SCR_REQ_COND_LAST))
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
        OutputLog((int)LIST_TAB_KIND.실시간데이터, $"sRealKey = {e.sRealKey}, sRealType = {e.sRealType}, sRealData = {e.sRealData}", 100, focus: false);
    }

    private void AxKHOpenApi_OnReceiveRealCondition(object sender, _DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
    {
        OutputLog((int)LIST_TAB_KIND.조건검색실시간, $"sTrCode = {e.sTrCode}, strType = {e.strType}, strConditionName = {e.strConditionName}, strConditionIndex = {e.strConditionIndex}");
    }

    private void AxKHOpenApi_OnReceiveMsg(object sender, _DKHOpenAPIEvents_OnReceiveMsgEvent e)
    {
        OutputLog((int)LIST_TAB_KIND.메시지목록, $"<OnReceiveMsg> sScrNo = {e.sScrNo}, sRQName = {e.sRQName}, sTrCode = {e.sTrCode}, sMsg = {e.sMsg}");
        SetStatusText(e.sMsg);
    }

    private void AxKHOpenApi_OnReceiveInvestRealData(object sender, _DKHOpenAPIEvents_OnReceiveInvestRealDataEvent e)
    {
        OutputLog((int)LIST_TAB_KIND.메시지목록, $"<OnReceiveInvestRealData> sRealKey = {e.sRealKey}");
    }

    private void AxKHOpenApi_OnReceiveConditionVer(object sender, _DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
    {
        OutputLog((int)LIST_TAB_KIND.메시지목록, $"<OnReceiveConditionVer> lRet = {e.lRet}, sMsg = {e.sMsg}");

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
            OutputLog((int)LIST_TAB_KIND.메시지목록, $"<OnReceiveConditionVer> 조건검색식({_mapCondNameToIndex.Count}개) 로딩완료");

            Load_사용자기능();
        }
    }

    private void AxKHOpenApi_OnReceiveChejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
    {
        OutputLog((int)LIST_TAB_KIND.실시간주문체결, $"sGubun = {e.sGubun}, nItemCnt = {e.nItemCnt}, sFIdList = {e.sFIdList}", 300);
        string[] szFids = e.sFIdList.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var sFid in szFids)
        {
            string sVal = _axOpenAPI!.GetChejanData(Convert.ToInt32(sFid));
            if (_map_FidToName.TryGetValue(sFid, out var name))
            {
                OutputLog((int)LIST_TAB_KIND.실시간주문체결, $"\t[{sFid}][{name}] = {sVal}", 300);
            }
            else
            {
                OutputLog((int)LIST_TAB_KIND.실시간주문체결, $"\t[{sFid}][NOFIDNAME] = {sVal}", 300);
            }
        }
    }

    private void AxKHOpenApi_OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
    {
        OutputLog((int)LIST_TAB_KIND.메시지목록, $"<OnReceiveEventConnect> nErrCode = {e.nErrCode}");
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
