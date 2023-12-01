using KFOpenApi.NET;
using KOAStudio.Core.Helpers;
using KOAStudio.Core.Models;
using KOAStudio.Core.ViewModels;
using System.Diagnostics;
using System.Text;

namespace WKOAStudio.Business;

internal sealed partial class BusinessLogic
{
    private void AxKFOpenAPI_OnReceiveTrData(object sender, _DKFOpenAPIEvents_OnReceiveTrDataEvent e)
    {
        //
        DateTime Now = DateTime.Now;
        var memory_full_data = _axOpenAPI!.GetCommFullData(e.sTrCode, e.sRQName, 0);
        var received_data = _appEncoder.GetBytes(memory_full_data);
        //
        OutputLog((int)TAB_LIST_KIND.메시지목록, $"<OnReceiveTrData> sScrNo = {e.sScrNo},  sRQName = {e.sRQName}, sTrCode = {e.sTrCode}, sRecordName = {e.sRecordName}, sPrevNext = {e.sPreNext}, received size = {received_data.Length}");

        if (string.Equals(e.sScrNo, _scrNum_CHART_CONTENT))
        {
            CharDataReqViewModel? model = null;
            if (_chartDataReqViewModel_선물 != null && e.sRQName.StartsWith(_chartDataReqViewModel_선물.Title))
            {
                model = _chartDataReqViewModel_선물;
            }

            if (model != null)
            {
                StringBuilder sb = new();
                int nRepeatCount = _axOpenAPI!.GetRepeatCnt(e.sTrCode, e.sRQName);

                for (int i = 0; i < nRepeatCount; i++)
                {
                    bool b분틱 = model.SelectedChartRound == ChartRound.분 || model.SelectedChartRound == ChartRound.틱;
                    sb.Append(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, b분틱 ? "체결시간" : "일자"));
                    sb.Append(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가"));
                    sb.Append(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가"));
                    sb.Append(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가"));
                    sb.Append(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                    sb.AppendLine(_axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, b분틱 ? "거래량" : "누적거래량"));
                }

                model.ReceivedTime = Now;
                model.ReceivedDataCount = nRepeatCount;
                model.ResultText = sb.ToString();
                model.NextText = e.sPreNext;
                model.NextEnabled = e.sPreNext.Length > 0;
            }
        }



        //if (e.sScrNo == _scrNum_REQ_TR_BASE)
        {
            _tr_NextKey = e.sPreNext.TrimStart();
            SetPropertyQueryNextEnable(_tr_NextKey.Length > 0);
            var trData = _trDatas.Find(x => x.Code.Equals(e.sTrCode, StringComparison.CurrentCultureIgnoreCase));
            if (trData != null)
            {
                int nRepeatCount = _axOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);
                int nReqCount = (trData.OutputSingle != null ? trData.OutputSingle.Count : 0)
                    + nRepeatCount * (trData.OutputMuti != null ? trData.OutputMuti.Count : 0);

                int nMemorySize = (trData.SizeSingle != null) ? trData.SizeSingle.Sum() : 0
                     + nRepeatCount * (trData.SizeMuti != null ? trData.SizeMuti.Sum() : 0);

                string[] lines = new string[1 + nReqCount];
                string TimeLine = DateTime.Now.ToString("HH:mm:ss.fff : ");
                int nLineIndex = 0;

                // GetCommFullData 사이즈와 매칭될경우: 문자열 인덱스 파싱, 불필요한 호출 없앤다
                // 매칭안될경우 : api호출
                bool bMemoryMode = received_data.Length == nMemorySize;
                string szReadMode = bMemoryMode ? "GetCommFullData" : "GetCommData";
                lines[nLineIndex++] = $"{TimeLine}[{trData.Name}] [{e.sRQName}] Count={nRepeatCount} PrevNext={e.sPreNext}, received size = {received_data.Length}, mode={szReadMode}";

                Stopwatch timer = Stopwatch.StartNew();
                if (bMemoryMode)
                {
                    int nCharIndex = 0;
                    if (trData.OutputSingle != null)
                        for (int i = 0; i < trData.OutputSingle.Count; i++)
                        {
                            int filed_size = trData.SizeSingle![i];
                            string value = _appEncoder.GetString(received_data, nCharIndex, filed_size);
                            nCharIndex += filed_size;
                            lines[nLineIndex++] = $"[{e.sTrCode}] {trData.OutputSingle[i]}={value}";
                        }
                    if (trData.OutputMuti != null)
                        for (int i = 0; i < nRepeatCount; i++)
                        {
                            for (int j = 0; j < trData.OutputMuti.Count; j++)
                            {
                                int filed_size = trData.SizeMuti![j];
                                string value = _appEncoder.GetString(received_data, nCharIndex, filed_size);
                                nCharIndex += filed_size;
                                lines[nLineIndex++] = $"[{e.sTrCode}][{i}] {trData.OutputMuti[j]}={value}";
                            }
                        }
                }
                else
                {
                    if (trData.OutputSingle != null)
                        for (int i = 0; i < trData.OutputSingle.Count; i++)
                        {
                            string value = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, trData.OutputSingle[i])/*.Trim()*/;
                            lines[nLineIndex++] = $"[{e.sTrCode}] {trData.OutputSingle[i]}={value}";
                        }
                    if (trData.OutputMuti != null)
                        for (int i = 0; i < nRepeatCount; i++)
                        {
                            for (int j = 0; j < trData.OutputMuti.Count; j++)
                            {
                                string value = _axOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, trData.OutputMuti[j])/*.Trim()*/;
                                lines[nLineIndex++] = $"[{e.sTrCode}][{i}] {trData.OutputMuti[j]}={value}";
                            }
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

    private void AxKFOpenAPI_OnReceiveRealData(object sender, _DKFOpenAPIEvents_OnReceiveRealDataEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.실시간데이터, $"sJongmokCode = {e.sJongmokCode}, sRealType = {e.sRealType}, sRealData = {e.sRealData}", 100, focus: false);
    }

    private void AxKFOpenAPI_OnReceiveMsg(object sender, _DKFOpenAPIEvents_OnReceiveMsgEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.메시지목록, $"<OnReceiveMsg> sScrNo = {e.sScrNo}, sRQName = {e.sRQName}, sTrCode = {e.sTrCode}, sMsg = {e.sMsg}");
        SetStatusText(e.sMsg);
    }

    private void AxKFOpenAPI_OnReceiveChejanData(object sender, _DKFOpenAPIEvents_OnReceiveChejanDataEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.실시간주문체결, $"sGubun = {e.sGubun},  nItemCnt = {e.nItemCnt}, sFIdList = {e.sFIdList}", 300);
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

    private void AxKFOpenAPI_OnEventConnect(object sender, _DKFOpenAPIEvents_OnEventConnectEvent e)
    {
        OutputLog((int)TAB_LIST_KIND.메시지목록, $"<OnReceiveEventConnect> nErrCode = {e.nErrCode}");
        if (e.nErrCode == 0 && LoginState != OpenApiLoginState.LoginFailed)
        {
            _isRealServer = !string.Equals(_axOpenAPI!.GetCommonFunc("GetServerGubunW", string.Empty), "1");
            LoginState = OpenApiLoginState.LoginSucceed;

            _apiFolder = _axOpenAPI.GetAPIModulePath();

            Load_TR목록Async();
            Load_화면목록Async();
            Load_개발가이드Async();

            Load_종목정보();
            Load_사용자기능();
        }
        else
        {
            LoginState = OpenApiLoginState.LoginFailed;
        }
    }
}
