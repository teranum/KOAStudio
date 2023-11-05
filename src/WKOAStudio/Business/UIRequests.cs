﻿using KOAStudio.Core.Helpers;
using KOAStudio.Core.Models;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace WKOAStudio.Business;

internal sealed partial class BusinessLogic
{
    public int ReqApiLogin(bool bLogin)
    {
        if (bLogin)
        {
            LoginState = OpenApiLoginState.LoginProcess;
            return axOpenAPI?.CommConnect(0) ?? -1;
        }
        else
        {
            LoginState = OpenApiLoginState.LoginOuted;
            axOpenAPI?.CommTerminate();
        }
        return 0;
    }

    public void ReqStopRealTime()
    {
        if (LoginState == OpenApiLoginState.LoginSucceed)
        {
            axOpenAPI?.DisconnectRealData(SCR_REQ_TR_BASE);
        }
    }

    public void ItemSelectedChanged(int tabIndex, IconTextItem selectedItem)
    {
        TREETAB_KIND tabKind = (TREETAB_KIND)tabIndex;

        string SelectedText = selectedItem.Text!;

        switch (tabKind)
        {
            case TREETAB_KIND.실시간목록:
                {
                    if (selectedItem.IconId != 1 || _Data_실시간목록 == null) return;
                    var org_item = _Data_실시간목록.Items.FirstOrDefault((t) =>
                    {
                        var image_text = t as IconText;
                        return image_text?.Text.Equals(selectedItem.Text) ?? false;
                    }) as IconTextItem;
                    if (org_item != null)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine("/********************************************************************/");
                        stringBuilder.AppendLine("/// ########## 실시간타입 FID 리스트 입니다.");
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine($"\t{org_item.Text} ({org_item.Items.Count})");
                        stringBuilder.AppendLine();
                        foreach (var item in org_item.Items)
                        {
                            IconTextItem? iconTextItem = item as IconTextItem;
                            if (iconTextItem != null)
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
            case TREETAB_KIND.TR목록:
                {
                    if (selectedItem.IconId != 4 && selectedItem.IconId != 14) return;
                    string selected_code = SelectedText.Substring(0, 8);
                    TR_SPECIAL? trData = TrDatas.FirstOrDefault(t => t.Code.Equals(selected_code, StringComparison.CurrentCultureIgnoreCase));
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
            case TREETAB_KIND.개발가이드:
                {
                    if (selectedItem.Items.Count > 0) return;
                    string FullKey = SelectedText;
                    IconTextItem? _parent = selectedItem.Parent;
                    while (_parent != null)
                    {
                        FullKey = _parent.Text + "/" + FullKey;
                        _parent = _parent.Parent;
                    }
                    if (MapDevContentToDescs.ContainsKey(FullKey))
                    {
                        SetResultText(MapDevContentToDescs[FullKey]);
                    }

                    // 함수 선택 확정
                    if (selectedItem.Parent != null && string.Equals(selectedItem.Parent.Text, "함수들") && selectedItem.IconId == 9 && axOpenAPI != null)
                    {
                        string szFuncName = SelectedText;
                        MethodInfo? theMethod = axOpenAPI.GetType().GetMethod(szFuncName);
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
                break;
            case TREETAB_KIND.화면목록:
                {
                    if (selectedItem.IconId != 9) return;
                    int nFindPos = SelectedText.IndexOf(':');
                    if (nFindPos > 0)
                    {
                        string trCode = SelectedText.Substring(0, nFindPos).Trim();
                        var trData = TrDatas.FirstOrDefault(tr => string.Equals(tr.Code, trCode, StringComparison.OrdinalIgnoreCase));
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
            case TREETAB_KIND.사용자기능:
                {
                    if (axOpenAPI!.GetConnectState() == 0) return;
                    if (selectedItem.IconId == 13)
                    {
                        if (string.Equals(SelectedText, "사용자정보"))
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine("\t[사용자정보]");
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine($"\t사용자 ID : {axOpenAPI.GetLoginInfo("USER_ID")}");
                            stringBuilder.AppendLine($"\t사용자 이름 : {axOpenAPI.GetLoginInfo("USER_NAME")}");
                            stringBuilder.AppendLine($"\t보유계좌수 : {axOpenAPI.GetLoginInfo("ACCOUNT_CNT")}");
                            var 계좌s = axOpenAPI.GetLoginInfo("ACCNO").Split(';', StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < 계좌s.Length; i++)
                            {
                                stringBuilder.AppendLine($"\t\t계좌{i + 1} : {계좌s[i]}");
                            }
                            stringBuilder.AppendLine($"\t계좌비밀번호 설정여부 : {axOpenAPI.GetCommonFunc("GetAcnoPswdState", "")}");
                            string server = _IsRealServer ? "실서버" : "모의투자";
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine($"\t접속서버구분 : {server}");
                            SetResultText(stringBuilder.ToString());
                        }
                    }
                }
                break;
            default:
                break;
        }

        // sub function
        string GetTrDescript(TR_SPECIAL trData)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\r\n/********************************************************************/\r\n/// ########## Open API 함수를 이용한 전문처리 샘플코드 예제입니다. \r\n\r\n");
            stringBuilder.Append(" [ ");
            stringBuilder.Append($"{trData.Code} : {trData.Name}");
            stringBuilder.Append(" ]\r\n\r\n");
            if (trData.Caution.Length > 0)
            {
                stringBuilder.Append(" [ 주의 ] \r\n");
                stringBuilder.Append($" {trData.Caution}\r\n\r\n");
            }
            stringBuilder.Append(" 1. Open API 조회 함수 입력값을 설정합니다.\r\n");
            for (int i = 0; i < trData.Inputs!.Count; i++)
            {
                string inputdesc = trData.InputDescs![i];
                if (inputdesc.Length > 0)
                    stringBuilder.Append($"\t{trData.Inputs[i]} = {trData.InputDescs![i]}\r\n");
                stringBuilder.Append($"\tSetInputValue(\"{trData.Inputs[i]}\"\t, \"입력값{i + 1}\");\r\n\r\n");
            }
            stringBuilder.Append($"\r\n 2. Open API 조회 함수를 호출해서 전문을 서버로 전송합니다.\r\n\tCommRqData( \"RQName\"\t,  \"{trData.Code}\"\t,  \"\"\t,  \"화면번호\"); \r\n");
            stringBuilder.Append("\r\n/********************************************************************/\r\n");

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
                        stringBuilder.Append("\t");
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
                stringBuilder.Append($"[OUTPUT:싱글데이터] ({trData.OutputSingle.Count}), size : {trData.SizeSingle!.Sum()}");
                for (int i = 0; i < trData.OutputSingle.Count; i++)
                {
                    if (i % 8 == 0)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.Append("\t");
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
                if (trData.OutputMuti_add != null)
                    stringBuilder.Append($"[OUTPUT:멀티데이터1] ({trData.OutputMuti.Count}), size : {trData.SizeMuti!.Sum()}");
                else
                    stringBuilder.Append($"[OUTPUT:멀티데이터] ({trData.OutputMuti.Count}), size : {trData.SizeMuti!.Sum()}");
                for (int i = 0; i < trData.OutputMuti.Count; i++)
                {
                    if (i % 8 == 0)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.Append("\t");
                    }
                    if (i != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append($"\"{trData.OutputMuti[i]}\"");
                }
                stringBuilder.AppendLine();

                if (trData.OutputMuti_add != null && trData.OutputMuti_add.Count != 0)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append($"[OUTPUT:멀티데이터2] ({trData.OutputMuti_add.Count}), size : {trData.SizeMuti_add!.Sum()}");
                    for (int i = 0; i < trData.OutputMuti_add.Count; i++)
                    {
                        if (i % 8 == 0)
                        {
                            stringBuilder.AppendLine();
                            stringBuilder.Append("\t");
                        }
                        if (i != 0)
                            stringBuilder.Append(", ");
                        stringBuilder.Append($"\"{trData.OutputMuti_add[i]}\"");
                    }
                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString();
        }
    }

    public void ReqTRHistory(int tabIndex, string text)
    {
        string codeName = string.Empty;
        if ((LIST_TAB_KIND)tabIndex == LIST_TAB_KIND.조회한TR목록)
        {
            if (text != null && text != string.Empty)
            {
                int nSubPos = text.IndexOf(" : ");
                if (nSubPos != -1)
                {
                    codeName = text.Substring(nSubPos + " : ".Length);
                }
            }
        }

        int nPos = codeName.IndexOf(" : ");
        if (nPos != -1)
        {
            string code = codeName.Substring(0, nPos);
            for (int i = 0; i < TrDatas.Count; i++)
            {
                var trData = TrDatas[i];
                if (string.Equals(trData.Code, code))
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

    private string TR_NextKey = string.Empty;
    public void QueryApiAction(string reqText, object parameters, bool bNext)
    {
        string szActionMsg = string.Empty;
        string SelectedText = reqText;
        if (SelectedText.Length < 7) return;
        var datagrid_PropertiesItems = parameters as IList<PropertyItem>;
        if (datagrid_PropertiesItems == null || axOpenAPI == null || axOpenAPI.Created == false)
            return;
        if (string.Equals(SelectedText.Substring(0, 2).ToUpper(), "OP")) // TR요청
        {
            string OptCode = SelectedText.Substring(0, SelectedText.IndexOf(" : "));
            for (int i = 0; i < datagrid_PropertiesItems.Count; i++)
            {
                var nvd = datagrid_PropertiesItems[i];
                _appRegistry.SetValue(OptCode, nvd.Name, nvd.Value);
                axOpenAPI.SetInputValue(nvd.Name, nvd.Value);
            }
            if (axOpenAPI.GetConnectState() != 0)
            {
                long lRet = axOpenAPI.CommRqData(OptCode, OptCode, bNext ? TR_NextKey : "", SCR_REQ_TR_BASE);
                if (lRet == 0)
                {
                    szActionMsg = $"<TR ({OptCode}) 요청: 성공> lRet = {lRet}";
                }
                else
                    szActionMsg = $"<TR ({OptCode}) 요청: 실패> lRet = {lRet}";
            }
        }
        else if (string.Equals(SelectedText.Substring(0, 7), "함수호출 : ")) // 함수호출
        {
            string szFuncName = SelectedText.Substring("함수호출 : ".Length);
            VariantWrapper[] Params = new VariantWrapper[datagrid_PropertiesItems.Count];

            string parameter_text = string.Empty;
            for (int i = 0; i < datagrid_PropertiesItems.Count; i++)
            {
                var nvd = datagrid_PropertiesItems[i];

                if (i > 0) parameter_text += ", ";
                if (nvd.type == PropertyItem.VALUE_TYPE.VALUE_STRING)
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

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            object? obj;
            try
            {
                obj = CallInstanceFunc(axOpenAPI, szFuncName, Params);
            }
            catch (Exception ex)
            {
                obj = ex;
            }
            stopwatch.Stop();

            string szAddText = $"\r\n---- 호출결과  {szFuncName}(";
            szAddText += parameter_text;
            szAddText += ")\t\t" + string.Format("({0:n0} uS)\r\n", (int)(stopwatch.Elapsed.TotalSeconds * 1000000));
            if (obj != null)
            {
                szAddText += obj.ToString();
                szAddText += "\r\n";
            }
            SetResultText(szAddText, true);
        }

        if (szActionMsg.Length > 0)
        {
            OutputLog((int)LIST_TAB_KIND.메시지목록, szActionMsg);
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
                    var sInfo = new System.Diagnostics.ProcessStartInfo("https://www.kiwoom.com/h/customer/download/VFofuopOpenApiInfoView?dummyVal=0")
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
                    StringBuilder stringBuilder = new StringBuilder();
                    // sort
                    var array = Map_FidToName.ToArray();
                    Comparison<KeyValuePair<string, string>> value = (v1, v2) => Convert.ToInt32(v1.Key) - Convert.ToInt32(v2.Key);
                    Array.Sort(array, value);

                    foreach (var item in array)
                    {
                        stringBuilder.AppendLine($"\t[{item.Key}] : {item.Value}");
                    }
                    SetResultText(stringBuilder.ToString());
                }
                break;
            default:
                break;
        }
    }
}
