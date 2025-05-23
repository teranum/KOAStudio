﻿using KOAStudio.Core.Helpers;
using KOAStudio.Core.Models;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace KOAStudio.Business;

internal sealed partial class BusinessLogic
{
    public int ReqApiLogin(bool bLogin)
    {
        if (bLogin)
        {
            LoginState = OpenApiLoginState.LoginProcess;
            return _axOpenAPI?.CommConnect() ?? -1;
        }

        LoginState = OpenApiLoginState.LoginOuted;
        _axOpenAPI?.CommTerminate();
        return 0;
    }

    public void ReqStopRealTime()
    {
        if (LoginState == OpenApiLoginState.LoginSucceed)
        {
            _axOpenAPI?.SetRealRemove("ALL", "ALL");
        }
    }

    public void ItemSelectedChanged(int tabIndex, IdTextItem selectedItem)
    {
        TAB_TREE_KIND tabKind = (TAB_TREE_KIND)tabIndex;

        string SelectedText = selectedItem.Text!;

        switch (tabKind)
        {
            case TAB_TREE_KIND.실시간목록:
                {
                    if (selectedItem.Id != 1 || _data_실시간목록 == null) return;
                    if (_data_실시간목록.Items.FirstOrDefault((t) =>
                    {
                        var image_text = t as IdText;
                        return image_text?.Text.Equals(selectedItem.Text) ?? false;
                    }) is IdTextItem org_item)
                    {
                        StringBuilder stringBuilder = new();
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine("/********************************************************************/");
                        stringBuilder.AppendLine("/// ########## 실시간타입 FID 리스트 입니다.");
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine($"\t{org_item.Text} ({org_item.Items.Count})");
                        stringBuilder.AppendLine();
                        foreach (var item in org_item.Items)
                        {
                            if (item is IdTextItem iconTextItem)
                            {
                                stringBuilder.AppendLine($"\t{iconTextItem.Text}");
                            }
                        }
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine("/********************************************************************/");
                        SetResultText(stringBuilder.ToString());
                    }
                }
                break;
            case TAB_TREE_KIND.TR목록:
                {
                    if (selectedItem.Id != 4 && selectedItem.Id != 14) return;
                    string selected_code = SelectedText[..8];
                    TR_SPECIAL? trData = _trDatas.Find(t => t.Code.Equals(selected_code, StringComparison.CurrentCultureIgnoreCase));
                    if (trData != null)
                    {
                        SetResultText(GetTrDescript(trData));

                        // property
                        var prop_items = new List<PropertyItem>();
                        for (int i = 0; i < trData.Inputs!.Count; i++)
                        {
                            string defRegData = _appRegistry.GetValue(trData.Code, trData.Inputs[i], string.Empty);
                            prop_items.Add(new PropertyItem
                            (
                                trData.Inputs[i],
                                defRegData,
                                trData.InputDescs![i]
                            ));
                        }
                        SetProperties($"{trData.Code} : {trData.Name} 설정", prop_items);
                    }
                }
                break;
            case TAB_TREE_KIND.개발가이드:
                {
                    if (selectedItem.Items.Count > 0) return;
                    string FullKey = SelectedText;
                    IdTextItem? _parent = selectedItem.Parent;
                    while (_parent != null)
                    {
                        FullKey = _parent.Text + "/" + FullKey;
                        _parent = _parent.Parent;
                    }
                    if (_mapDevContentToDescs.TryGetValue(FullKey, out string? value))
                    {
                        SetResultText(value);
                    }

                    // 함수 선택 확정
                    if (selectedItem.Id == 6 && _axOpenAPI != null)
                    {
                        int nFuncNameSpaceIndex = SelectedText.IndexOf(' ');
                        int nFuncNameLastIndex = SelectedText.IndexOf('(');
                        if (nFuncNameSpaceIndex != -1 && nFuncNameLastIndex != -1)
                        {
                            string szFuncName = SelectedText.Substring(nFuncNameSpaceIndex + 1, nFuncNameLastIndex - nFuncNameSpaceIndex - 1);

                            MethodInfo? theMethod = _axOpenAPI.GetType().GetMethod(szFuncName);
                            if (theMethod != null)
                            {
                                var inner_parameters = theMethod.GetParameters();
                                var prop_items = new List<PropertyItem>();
                                if (inner_parameters.Length > 0)
                                {
                                    for (int i = 0; i < inner_parameters.Length; i++)
                                    {
                                        var param = inner_parameters[i];
                                        string szParamName = param.Name!;
                                        bool bString = param.ParameterType == typeof(string);
                                        if (bString)
                                        {
                                            string szParamValue = _appRegistry.GetValue(szFuncName, szParamName, string.Empty);
                                            prop_items.Add(new PropertyItem
                                            (
                                                szParamName,
                                                szParamValue,
                                                "문자열"
                                            ));
                                        }
                                        else
                                        {
                                            int lParamValue = _appRegistry.GetValue(szFuncName, szParamName, 0);
                                            prop_items.Add(new PropertyItem
                                            (
                                                szParamName,
                                                lParamValue.ToString(),
                                                "숫자",
                                                PropertyItem.VALUE_TYPE.VALUE_LONG
                                            ));
                                        }
                                    }
                                }
                                SetProperties($"함수호출 : {szFuncName}", prop_items);
                            }
                        }
                    }
                }
                break;
            case TAB_TREE_KIND.화면목록:
                {
                    if (selectedItem.Id != 9) return;
                    int nFindPos1 = SelectedText.IndexOf('=');
                    int nFindPos2 = SelectedText.IndexOf(':');
                    if (nFindPos1 > 0 && nFindPos2 > nFindPos1)
                    {
                        string trCode = SelectedText.Substring(nFindPos1 + 1, nFindPos2 - nFindPos1 - 1).Trim();
                        var trData = _trDatas.Find(tr => tr.Code.Equals(trCode, StringComparison.OrdinalIgnoreCase));
                        if (trData != null)
                        {
                            SetResultText(GetTrDescript(trData));

                            // property
                            var prop_items = new List<PropertyItem>();
                            for (int i = 0; i < trData.Inputs!.Count; i++)
                            {
                                string defRegData = _appRegistry.GetValue(trData.Code, trData.Inputs[i], string.Empty);
                                prop_items.Add(new PropertyItem
                                (
                                    trData.Inputs[i],
                                    defRegData,
                                    trData.InputDescs![i]
                                ));
                            }
                            SetProperties($"{trData.Code} : {trData.Name} 설정", prop_items);
                        }
                    }
                }
                break;
            case TAB_TREE_KIND.사용자기능:
                {
                    if (selectedItem.Id == 9)
                    {
                        if (SelectedText.Equals("Api정보"))
                        {
                            string ProgID = "KHOPENAPI.KHOPenAPICtrl.1";
                            string CLSID = OcxPathHelper.GetClassIDFromProgID(ProgID);
                            string path = OcxPathHelper.GetOcxPathFromCLSID(CLSID);

                            StringBuilder stringBuilder = new();
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine("\t[Api정보]");
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine($"\tProgID : {ProgID}");
                            stringBuilder.AppendLine($"\tCLSID  : {CLSID}");
                            if (Environment.Is64BitProcess)
                            {
                                stringBuilder.AppendLine($"\t파일 경로(32bit) : {OcxPathHelper.GetOcxPathFromWOW6432NodeCLSID(CLSID)}");
                                stringBuilder.AppendLine($"\t파일 경로(64bit) : {path}");
                            }
                            else
                                stringBuilder.AppendLine($"\t파일 경로 : {path}");

                            try
                            {
                                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
                                stringBuilder.AppendLine($"\t파일 설명 : {fileVersionInfo.FileDescription}");
                                stringBuilder.AppendLine($"\t파일 버전 : {fileVersionInfo.FileVersion}");
                            }
                            catch
                            {
                            }

                            SetResultText(stringBuilder.ToString());
                        }
                        else ShowUserContent(SelectedText);
                        return;
                    }

                    if (_axOpenAPI!.GetConnectState() == 0) return;
                    if (selectedItem.Id == 13)
                    {
                        if (SelectedText.Equals("사용자정보"))
                        {
                            StringBuilder stringBuilder = new();
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine("\t[사용자정보]");
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine($"\t사용자 ID : {_axOpenAPI.GetLoginInfo("USER_ID")}");
                            stringBuilder.AppendLine($"\t사용자 이름 : {_axOpenAPI.GetLoginInfo("USER_NAME")}");
                            stringBuilder.AppendLine($"\t보유계좌수 : {_axOpenAPI.GetLoginInfo("ACCOUNT_CNT")}");
                            var 계좌s = _axOpenAPI.GetLoginInfo("ACCTLIST_DETAIL").Split(';', StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < 계좌s.Length; i++)
                            {
                                var detail = 계좌s[i].Split(',');
                                if (detail.Length == 3)
                                    stringBuilder.AppendLine($"\t\t계좌{i + 1} : {detail[0]}, {detail[1].Trim()}, {detail[2].Trim()}");
                                else
                                    stringBuilder.AppendLine($"\t\t계좌{i + 1} : 타입오류");
                            }
                            string server = _isRealServer ? "실서버" : "모의투자";
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine($"\t접속서버구분 : {server}");
                            SetResultText(stringBuilder.ToString());
                        }
                    }
                    else if (selectedItem.Id == 12) // 조건검색
                    {
                        if (_mapCondNameToIndex.TryGetValue(SelectedText, out var condIndex))
                        {
                            // property
                            string scrNum = (Convert.ToInt32(_scrNum_REQ_COND_BASE) + Convert.ToInt32(condIndex)).ToString();
                            var prop_items = new List<PropertyItem>
                            {
                                new(
                                    "화면번호",
                                    scrNum,
                                    $"문자열, 읽기 전용입니다.\r\n(앱 내부에서 {_scrNum_REQ_COND_BASE} - {_scrNum_REQ_COND_LAST} 사이 설정)",
                                    IsValueReadOnly:true
                                ),
                                new(
                                    "조건식 이름",
                                    SelectedText,
                                    "문자열, 읽기 전용입니다.\r\n(조건식 고유 이름)",
                                    IsValueReadOnly: true
                                ),
                                new(
                                    "조건식 고유번호",
                                    condIndex,
                                    "숫자, 읽기 전용입니다.\r\n(조건식 고유 번호)",
                                    IsValueReadOnly: true
                                ),
                                new(
                                    "실시간옵션",
                                    "0",
                                    "숫자, 0:조건검색만, 1:조건검색+실시간 조건검색"
                                ),
                            };
                            SetProperties("조건검색 : " + SelectedText + " 설정", prop_items);
                            //var sb = new StringBuilder();
                            //sb.AppendLine($"(int nRet, string sItems) = await _axOpenAPI.SendConditionAsync(\"{scrNum}\", \"{SelectedText}\", {condIndex}, \"0\");");
                            //SetResultText(sb.ToString());
                            SetResultText(string.Empty);
                        }
                    }
                }
                break;
            default:
                break;
        }

        // sub function
        static string GetTrDescript(TR_SPECIAL trData)
        {
            StringBuilder stringBuilder = new();
            stringBuilder
                .AppendLine()
                .AppendLine("/********************************************************************/")
                .AppendLine("/// ########## Open API 함수를 이용한 전문처리 샘플코드 예제입니다.")
                .AppendLine()
                .AppendLine($" [ {trData.Code} : {trData.Name} ]")
                .AppendLine();
            if (trData.Caution.Length > 0)
            {
                stringBuilder.AppendLine(" [ 주의 ]");
                stringBuilder.AppendLine($" {trData.Caution}");
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine(" 1. Open API 조회 함수 입력값을 설정합니다.");
            for (int i = 0; i < trData.Inputs!.Count; i++)
            {
                stringBuilder.AppendLine($"\t{trData.Inputs[i]} = {trData.InputDescs![i]}");
                stringBuilder.AppendLine($"\tSetInputValue(\"{trData.Inputs[i]}\"\t, \"입력값{i + 1}\");");
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($" 2. Open API 조회 함수를 호출해서 전문을 서버로 전송합니다.");
            stringBuilder.AppendLine($"\tCommRqData( \"RQName\"\t,  \"{trData.Code}\"\t,  0\t,  \"화면번호\");");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("/********************************************************************/");

            // input
            if (trData.Inputs != null && trData.Inputs.Count != 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append($"[INPUT] ({trData.Inputs.Count})");
                for (int i = 0; i < trData.Inputs.Count; i++)
                {
                    if (i % 8 == 0)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.Append('\t');
                    }
                    if (i != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append($"\"{trData.Inputs[i]}\"");
                }
                stringBuilder.AppendLine();
            }

            // output:싱글데이터
            if (trData.OutputSingle != null && trData.OutputSingle.Count != 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append($"[OUTPUT:싱글데이터] ({trData.OutputSingle.Count})");
                for (int i = 0; i < trData.OutputSingle.Count; i++)
                {
                    if (i % 8 == 0)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.Append('\t');
                    }
                    if (i != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append($"\"{trData.OutputSingle[i]}\"");
                }
                stringBuilder.AppendLine();
            }

            // output:멀티데이터
            if (trData.OutputMuti != null && trData.OutputMuti.Count != 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append($"[OUTPUT:멀티데이터] ({trData.OutputMuti.Count})");
                for (int i = 0; i < trData.OutputMuti.Count; i++)
                {
                    if (i % 8 == 0)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.Append('\t');
                    }
                    if (i != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append($"\"{trData.OutputMuti[i]}\"");
                }
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }
    }

    public void ReqTRHistory(int tabIndex, string text)
    {
        string codeName = string.Empty;
        if ((TAB_LIST_KIND)tabIndex == TAB_LIST_KIND.조회한TR목록)
        {
            if (text != null && text != string.Empty)
            {
                int nSubPos = text.IndexOf(" : ");
                if (nSubPos != -1)
                {
                    codeName = text[(nSubPos + " : ".Length)..];
                }
            }
        }

        int nPos = codeName.IndexOf(" : ");
        if (nPos != -1)
        {
            string code = codeName[..nPos];
            for (int i = 0; i < _trDatas.Count; i++)
            {
                var trData = _trDatas[i];
                if (trData.Code.Equals(code))
                {
                    // property
                    var prop_items = new List<PropertyItem>();
                    for (int j = 0; j < trData.Inputs!.Count; j++)
                    {
                        string defRegData = _appRegistry.GetValue(trData.Code, trData.Inputs[j], string.Empty);
                        prop_items.Add(new PropertyItem
                        (
                            trData.Inputs[j],
                            defRegData,
                            trData.InputDescs![j]
                        ));
                    }

                    SetProperties(codeName + " 설정", prop_items);
                    break;
                }
            }
        }
    }

    public void QueryApiAction(string reqText, object parameters, bool bNext)
    {
        string szActionMsg = string.Empty;
        string SelectedText = reqText;
        if (SelectedText.Length < 7) return;
        if (parameters is not IList<PropertyItem> datagrid_PropertiesItems || _axOpenAPI == null || !_axOpenAPI.Created)
            return;
        if (string.Equals(SelectedText[..2].ToUpper(), "OP")) // TR요청
        {
            string OptCode = SelectedText[..SelectedText.IndexOf(" : ")];
            for (int i = 0; i < datagrid_PropertiesItems.Count; i++)
            {
                var nvd = datagrid_PropertiesItems[i];
                _appRegistry.SetValue(OptCode, nvd.Name, nvd.Value);
                _axOpenAPI.SetInputValue(nvd.Name, nvd.Value);
            }
            if (_axOpenAPI.GetConnectState() != 0)
            {
                long lRet = _axOpenAPI.CommRqData(OptCode, OptCode, bNext ? 2 : 0, _scrNum_REQ_TR_BASE);
                if (lRet == 0)
                {
                    szActionMsg = $"<TR ({OptCode}) 요청: 성공> lRet = {lRet}";
                }
                else
                    szActionMsg = $"<TR ({OptCode}) 요청: 실패> lRet = {lRet}";
            }
        }
        else if (string.Equals(SelectedText[..7], "조건검색 : ")) // 조건검색
        {
            string? szScrNum = datagrid_PropertiesItems[0].Value;
            string? szCondName = datagrid_PropertiesItems[1].Value;
            string? szIndex = datagrid_PropertiesItems[2].Value;
            string? szSearch = datagrid_PropertiesItems[3].Value;
            if (szScrNum == null || szCondName == null)
                szActionMsg = "<조건검색 : 요청실패> 변수타입 오류";
            else
            {
                long lRet = _axOpenAPI.SendCondition(szScrNum,
                    szCondName,
                    Convert.ToInt32(szIndex),
                    Convert.ToInt32(szSearch));
                if (lRet == 1)
                    szActionMsg = $"<조건검색 ({szCondName}) 요청: 성공> lRet = {lRet}";
                else
                    szActionMsg = $"<조건검색 ({szCondName}) 요청: 실패> lRet = {lRet}";
            }
        }
        else if (string.Equals(SelectedText[..7], "함수호출 : ")) // 함수호출
        {
            string szFuncName = SelectedText["함수호출 : ".Length..];
            VariantWrapper[] Params = new VariantWrapper[datagrid_PropertiesItems.Count];

            string parameter_text = string.Empty;
            for (int i = 0; i < datagrid_PropertiesItems.Count; i++)
            {
                var nvd = datagrid_PropertiesItems[i];

                if (i > 0) parameter_text += ", ";
                if (nvd.ValueType == PropertyItem.VALUE_TYPE.VALUE_STRING)
                {
                    parameter_text += "\"";
                    parameter_text += nvd.Value;
                    parameter_text += "\"";
                    _appRegistry.SetValue(szFuncName, nvd.Name, nvd.Value);
                    Params[i] = new VariantWrapper(nvd.Value);
                }
                else
                {
                    parameter_text += nvd.Value;
                    int nVal;
                    try
                    {
                        nVal = Convert.ToInt32(nvd.Value);
                    }
                    catch
                    {
                        nVal = 0;
                    }
                    _appRegistry.SetValue(szFuncName, nvd.Name, nVal);
                    Params[i] = new VariantWrapper(nVal);
                }
            }

            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();
            object? obj;
            try
            {
                obj = CallInstanceFunc(_axOpenAPI, szFuncName, Params);
            }
            catch (Exception ex)
            {
                obj = ex;
            }
            stopwatch.Stop();

            string szAddText = $"\r\n---- 호출결과  {szFuncName}({parameter_text})\t\t";
            szAddText += string.Format("({0:n0} uS)\r\n", (int)(stopwatch.Elapsed.TotalSeconds * 1000000));
            if (obj != null)
            {
                szAddText += obj.ToString();
                szAddText += "\r\n";
            }
            SetResultText(szAddText, bAdd: true);
        }

        if (szActionMsg.Length > 0)
        {
            OutputLog((int)TAB_LIST_KIND.메시지목록, szActionMsg);
        }
    }

    public void MenuCustomizeAction(string text)
    {
        //"키움 Open API 서비스",
        //"상시모의투자 신청",
        //"FID 리스트"
        switch (text)
        {
            case "키움 Open API 서비스":
                {
                    var sInfo = new System.Diagnostics.ProcessStartInfo("https://www.kiwoom.com/h/customer/download/VOpenApiInfoView?dummyVal=0")
                    {
                        UseShellExecute = true,
                    };
                    System.Diagnostics.Process.Start(sInfo);
                }
                break;
            case "상시모의투자 신청":
                {
                    var sInfo = new System.Diagnostics.ProcessStartInfo("https://www.kiwoom.com/h/mock/ordinary/VMockTotalSMAIN1View?dummyVal=0")
                    {
                        UseShellExecute = true,
                    };
                    System.Diagnostics.Process.Start(sInfo);
                }
                break;
            case "FID 리스트":
                {
                    SetResultText(Properties.Resources.FID_KORNAME);
                }
                break;
            case "KOAStudio 오픈소스":
                {
                    var sInfo = new System.Diagnostics.ProcessStartInfo("https://github.com/teranum/KOAStudio")
                    {
                        UseShellExecute = true,
                    };
                    System.Diagnostics.Process.Start(sInfo);
                }
                break;
            default:
                break;
        }
    }
}
