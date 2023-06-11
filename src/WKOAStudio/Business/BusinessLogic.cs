using KFOpenApi.NET;
using KOAStudio.Core.Helpers;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace WKOAStudio.Business;

internal sealed partial class BusinessLogic : IUIRequest, ILogicNotify
{
    [DllImport("kernel32")] private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    private static string GetProfileString(string section, string key, string file, int buflength = 255)
    {
        StringBuilder temp = new StringBuilder(buflength);
        int ret = GetPrivateProfileString(section, key, "", temp, buflength, file);
        return temp.ToString();
    }

    private AxKFOpenAPI? axOpenAPI;
    private string ApiFolder = string.Empty;
    private Encoding AppEncoder = Encoding.GetEncoding(949);

    private static readonly string SCR_REQ_TR_BASE = "3000";

    private Dictionary<string, string> Map_FidToName = new Dictionary<string, string>();


    private class TR_SPECIAL
    {
        public string Code = string.Empty;
        public string Name = string.Empty;
        public List<string>? Inputs;
        public List<string>? InputDescs;
        public List<string>? OutputSingle;
        public List<string>? OutputMuti;
        public List<string>? OutputMuti_add;
        public string Caution = string.Empty;

        // 출력데이터 사이즈
        public List<int>? SizeSingle;
        public List<int>? SizeMuti;
        public List<int>? SizeMuti_add;

    }
    private List<TR_SPECIAL> TrDatas = new List<TR_SPECIAL>();

    private Dictionary<string, string> MapDevContentToDescs = new Dictionary<string, string>();

    private struct SCRN_SPECIAL
    {
        public string ScreenNumber;
        public string ScreenName;
        public string[] TRs;
    }

    private struct KEY_VALUE
    {
        public string Key;
        public string Value;
    };
    private Dictionary<string, List<KEY_VALUE>> Map_IniFile_TrInfo = new Dictionary<string, List<KEY_VALUE>>();

    private readonly List<string> _menu_Customize = new List<string>
    {
        "키움 Open API 서비스",
        "상시모의투자 신청",
        "FID 리스트"
    };

    private enum TREETAB_KIND
    {
        실시간목록,
        TR목록,
        종목정보,
        개발가이드,
        화면목록,
        사용자기능,
    }

    private enum LIST_TAB_KIND
    {
        메시지목록,
        조회데이터,
        실시간데이터,
        실시간주문체결,
        조회한TR목록,
    }

    private bool _IsRealServer = false;
    private OpenApiLoginState _login_state = default;
    public OpenApiLoginState LoginState
    {
        get => _login_state;
        set
        {
            if (_login_state != value)
            {
                _login_state = value;
                string message = _login_state switch
                {
                    OpenApiLoginState.None => "준비됨",
                    OpenApiLoginState.ApiCreateFailed => "OpenApi 연결 오류",
                    OpenApiLoginState.LoginProcess => "로그인 중...",
                    OpenApiLoginState.LoginFailed => "로그인 실패",
                    OpenApiLoginState.LoginSucceed => "로그인 성공",
                    OpenApiLoginState.LoginOuted => "로그아웃",
                    _ => "준비됨",
                };

                SetStatusText(message, true, _IsRealServer);
            }
        }
    }
    private IAppRegistry _appRegistry;
    public BusinessLogic(IAppRegistry appRegistry)
    {
        _appRegistry = appRegistry;
    }

    private void GetIniFileData(string ini_path, ref Dictionary<string, List<KEY_VALUE>> IniFile_Info)
    {
        // 파일에서 TR정보 가져오기
        string[] lines = Array.Empty<string>();
        try
        {
            lines = System.IO.File.ReadAllLines(ini_path, AppEncoder);
        }
        catch (Exception)
        {
        }

        if (lines.Length > 0)
        {

            string section = string.Empty;
            List<KEY_VALUE> key_vals = new List<KEY_VALUE>();
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                if (line[0] == '[')
                {
                    if (section.Length > 0)
                    {
                        IniFile_Info[section] = key_vals;
                    }
                    section = line.Substring(1, line.Length - 2);
                    key_vals = new List<KEY_VALUE>();
                }
                else
                {
                    // key = value
                    var key_value = line.Split('=');
                    if (key_value.Length == 2)
                    {
                        key_vals.Add(new KEY_VALUE { Key = key_value[0].Trim(), Value = key_value[1].Trim() });
                    }
                }
            }
            if (section.Length > 0)
            {
                IniFile_Info[section] = key_vals;
            }
        }
    }

    /// <summary>
    /// 초기데이터 설정
    /// </summary>
    public void Initialize()
    {
        // 커스텀 메뉴 등록
        SetMenuCustomize("리소스", _menu_Customize);

        // 아이템 트리뷰 모델 등록
        var TreeTab_image_Names = new List<IconText>()
        {
            new IconText(0, "실시간목록"),
            new IconText(3, "TR목록"),
            new IconText(10, "종목정보"),
            new IconText(3, "개발가이드"),
            new IconText(10, "화면목록"),
            new IconText(-1, "사용자기능")
        };
        SetTabTrees(TreeTab_image_Names);

        // 로그 리스트뷰 만들기
        var ListTab_Names = new List<string>();
        foreach (LIST_TAB_KIND item in Enum.GetValues(typeof(LIST_TAB_KIND)))
            ListTab_Names.Add(item.ToString());
        SetTabLists(ListTab_Names);

        // 초기화
        IntPtr Handle = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
        axOpenAPI = new AxKFOpenAPI(Handle);
        if (!axOpenAPI.Created)
        {
            LoginState = OpenApiLoginState.ApiCreateFailed;
            return;
        }

        ApiFolder = axOpenAPI.GetAPIModulePath();

        axOpenAPI.OnEventConnect += AxKFOpenAPI_OnEventConnect;
        axOpenAPI.OnReceiveChejanData += AxKFOpenAPI_OnReceiveChejanData;
        axOpenAPI.OnReceiveMsg += AxKFOpenAPI_OnReceiveMsg;
        axOpenAPI.OnReceiveRealData += AxKFOpenAPI_OnReceiveRealData;
        axOpenAPI.OnReceiveTrData += AxKFOpenAPI_OnReceiveTrData;

        OutputLog((int)LIST_TAB_KIND.메시지목록, "여기에 수신된 메시지가 표시됩니다");
        OutputLog((int)LIST_TAB_KIND.조회데이터, "여기에 전문 조회 데이터가 표시됩니다 (OnReceiveTrData)");
        OutputLog((int)LIST_TAB_KIND.실시간데이터, "여기에 전문 실시간 데이터가 표시됩니다 (OnReceiveRealData)");
        OutputLog((int)LIST_TAB_KIND.실시간주문체결, "여기에 전문 실시간 주문체결 데이터가 표시됩니다 (OnReceiveChejanData)");
        OutputLog((int)LIST_TAB_KIND.조회한TR목록, "여기에 조회한 전문목록(TR목록)이 최근순으로 표시됩니다. 각TR을 더블클릭하면 조회입력값이 자동으로 설정됩니다");

        // FID 네임 가져오기
        // /data/fidinfo.dat
        // #field_id	field_name	data_type	data_size	description
        // # 탭으로 구분자 두어 저장합니다.
        string FID_KORNAME = string.Empty;
        try
        {
            string fid_define_path = $"{ApiFolder}\\data\\fidinfo.dat";
            FID_KORNAME = System.IO.File.ReadAllText(fid_define_path, AppEncoder);
        }
        catch (Exception)
        {

        }

        string[] FIDLines = FID_KORNAME.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        int nFIDLines = FIDLines.Length;
        foreach (string line in FIDLines)
        {
            if (line.Length > 3 && line[0] != '#')
            {
                var tabStrings = line.Split('\t');
                if (tabStrings.Length >= 2)
                {
                    string key = tabStrings[0].Trim();
                    string name = tabStrings[1].Trim();
                    int nPos = 0;
                    while (nPos < name.Length)
                    {
                        if (name[nPos] == ' ')
                        {
                            name = name.Remove(nPos, 1);
                            continue;
                        }
                        else if (name[nPos] == ',')
                        {
                            name = name.Remove(nPos);
                            break;
                        }
                        nPos++;
                    }
                    Map_FidToName[key] = name;
                }
            }
        }

        // 파일에서 TR정보 가져오기
        GetIniFileData($"{ApiFolder}\\data\\kfoptrinfo.dat", ref Map_IniFile_TrInfo);

        // 초기데이터 로딩
        Load_실시간목록Async();
        Load_TR목록Async();
        Load_화면목록Async();
        Load_개발가이드Async();
    }

    private IconTextItem? _Data_실시간목록;
    private IconTextItem? _Data_TR목록;
    private IconTextItem? _Data_개발가이드;
    private IconTextItem? _Data_화면목록;
    private IconTextItem? _Data_선물종목정보;
    private IconTextItem? _Data_옵션종목정보;
    private IconTextItem? _Data_사용자정보;

    private async void Load_실시간목록Async()
    {
        if (_Data_실시간목록 != null) return;

        var task = Task.Run(() =>
        {
            var root = new IconTextItem(0, "실시간목록");
            if (Map_IniFile_TrInfo.TryGetValue("TRLIST", out List<KEY_VALUE>? trlists))
            {
                if (trlists != null)
                {
                    string[]? real_tr_names = null;
                    var real_tr = trlists.FirstOrDefault(tr => tr.Key == "Real");

                    real_tr_names = real_tr.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (real_tr_names != null && real_tr_names.Length > 0)
                    {
                        foreach (var key in real_tr_names)
                        {
                            if (Map_IniFile_TrInfo.TryGetValue(key, out List<KEY_VALUE>? key_values))
                            {
                                string name = string.Empty;
                                List<string> Fids = new List<string>();
                                IconTextItem? hitem = null;
                                foreach (var item in key_values)
                                {
                                    if (item.Key == "Title")
                                    {
                                        hitem = new IconTextItem(1, "Real Type : " + item.Value);
                                    }
                                    else if (hitem != null && item.Key != "Type" && item.Value != "-1")
                                    {
                                        var valTemp = item.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim());
                                        if (valTemp != null && valTemp.Count() > 0)
                                        {
                                            string text = "[";
                                            foreach (var tmpFid in valTemp)
                                            {
                                                if (text.Length > 1)
                                                {
                                                    text += ",";
                                                }
                                                text += tmpFid;
                                                Map_FidToName[tmpFid] = item.Key;
                                            }
                                            text += $"] = {item.Key}";
                                            hitem.AddChild(new IconTextItem(2, text));
                                        }
                                    }
                                }
                                if (hitem != null)
                                {
                                    root.AddChild(hitem);
                                }
                            }
                        }
                        if (root.Items.Count > 0)
                        {
                            root.Text = root.Text + $" ({root.Items.Count})";
                        }
                    }
                }
            }
            return root;
        });

        var root = await task;
        if (root != null)
        {
            root.IsExpanded = true;
            _Data_실시간목록 = root;
            SetTreeItems((int)TREETAB_KIND.실시간목록, new List<object>() { root });
        }
    }

    private async void Load_TR목록Async()
    {
        if (_Data_TR목록 != null) return;

        var task = Task.Run(() =>
        {
            if (Map_IniFile_TrInfo.TryGetValue("TRLIST", out List<KEY_VALUE>? trlists))
            {
                if (trlists != null)
                {
                    string[] sections = { "Quote", "Order", "Chart" };
                    var all_trs = trlists.Where(tr => sections.Contains(tr.Key));
                    List<string> tr_names = new List<string>();

                    foreach (var tr_part in all_trs)
                    {
                        tr_names.AddRange(tr_part.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
                    }
                    if (tr_names.Count > 0)
                    {
                        foreach (var tr_name in tr_names)
                        {
                            TR_SPECIAL trData = new TR_SPECIAL();
                            trData.Code = tr_name;
                            trData.Inputs = new List<string>();
                            trData.InputDescs = new List<string>();
                            trData.Caution = string.Empty;

                            List<KEY_VALUE>? key_values;
                            if (Map_IniFile_TrInfo.TryGetValue(tr_name + "_INPUT", out key_values))
                            {
                                trData.Name = key_values.FirstOrDefault(tr => tr.Key == "Title").Value;
                                foreach (var key_value in key_values)
                                {
                                    if (key_value.Key == "Title")
                                        trData.Name = key_value.Value;
                                    else
                                    {
                                        trData.Inputs.Add(key_value.Key);
                                        trData.InputDescs.Add(string.Empty);
                                    }
                                }
                            }
                            if (Map_IniFile_TrInfo.TryGetValue(tr_name + "_INPUT_LEGEND", out key_values))
                            {
                                foreach (var key_value in key_values)
                                {
                                    for (int i = 0; i < trData.Inputs.Count; i++)
                                    {
                                        if (key_value.Key == trData.Inputs[i])
                                        {
                                            trData.InputDescs[i] = key_value.Value;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (Map_IniFile_TrInfo.TryGetValue(tr_name + "_OUTPUT", out key_values))
                            {
                                trData.OutputSingle = new List<string>();
                                trData.SizeSingle = new List<int>();
                                foreach (var key_value in key_values)
                                {
                                    if (key_value.Key != "Title" && key_value.Key != "출력건수")
                                    {
                                        trData.OutputSingle.Add(key_value.Key);
                                        // size
                                        int size = 0;
                                        int pos = key_value.Value.IndexOf(",");
                                        if (pos >= 0)
                                            size = Convert.ToInt32(key_value.Value.Substring(0, pos));
                                        trData.SizeSingle.Add(size);
                                    }
                                }
                            }
                            string GFID = string.Empty;
                            if (Map_IniFile_TrInfo.TryGetValue(tr_name + "_TRINFO", out key_values))
                            {
                                GFID = key_values.FirstOrDefault(tr => tr.Key == "GFID").Value;
                            }

                            if (GFID.IndexOf(",") == -1)
                            {
                                if (Map_IniFile_TrInfo.TryGetValue(tr_name + "_OCCURS_" + GFID, out key_values))
                                {
                                    trData.OutputMuti = new List<string>();
                                    trData.SizeMuti = new List<int>();
                                    foreach (var key_value in key_values)
                                    {
                                        if (key_value.Key != "Title" && key_value.Key != "Count")
                                        {
                                            trData.OutputMuti.Add(key_value.Key);
                                            // size
                                            int size = 0;
                                            int pos = key_value.Value.IndexOf(",");
                                            if (pos >= 0)
                                                size = Convert.ToInt32(key_value.Value.Substring(0, pos));
                                            trData.SizeMuti.Add(size);
                                        }
                                    }
                                }
                            }
                            else if (GFID == "0,1")
                            {
                                var gfids = GFID.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                if (gfids.Length == 2)
                                {
                                    if (Map_IniFile_TrInfo.TryGetValue(tr_name + "_OCCURS_" + gfids[0], out key_values))
                                    {
                                        trData.OutputMuti = new List<string>();
                                        trData.SizeMuti = new List<int>();
                                        foreach (var key_value in key_values)
                                        {
                                            if (key_value.Key != "Title" && key_value.Key != "Count")
                                            {
                                                trData.OutputMuti.Add(key_value.Key);
                                                // size
                                                int size = 0;
                                                int pos = key_value.Value.IndexOf(",");
                                                if (pos >= 0)
                                                    size = Convert.ToInt32(key_value.Value.Substring(0, pos));
                                                trData.SizeMuti.Add(size);
                                            }
                                        }
                                    }
                                    if (Map_IniFile_TrInfo.TryGetValue(tr_name + "_OCCURS_" + gfids[1], out key_values))
                                    {
                                        trData.OutputMuti_add = new List<string>();
                                        trData.SizeMuti_add = new List<int>();
                                        foreach (var key_value in key_values)
                                        {
                                            if (key_value.Key != "Title" && key_value.Key != "Count")
                                            {
                                                trData.OutputMuti_add.Add(key_value.Key);
                                                // size
                                                int size = 0;
                                                int pos = key_value.Value.IndexOf(",");
                                                if (pos >= 0)
                                                    size = Convert.ToInt32(key_value.Value.Substring(0, pos));
                                                trData.SizeMuti_add.Add(size);
                                            }
                                        }
                                    }
                                }
                            }

                            TrDatas.Add(trData);
                        }
                    }
                }
            }

            if (TrDatas.Count == 0) return null;

            var root = new IconTextItem(3, "TR목록");

            foreach (var trData in TrDatas)
            {
                var hitem = new IconTextItem(trData.Caution.Length > 0 ? 14 : 4, $"{trData.Code} : {trData.Name}");
                // Inputs
                if (trData.Inputs != null)
                {
                    var hInputs = new IconTextItem(5, "[INPUT]");
                    foreach (var item in trData.Inputs)
                    {
                        hInputs.AddChild(new IconTextItem(6, item));
                    }
                    hitem.AddChild(hInputs);
                }
                // Outnput
                var hOutputs = new IconTextItem(7, "[OUTPUT]");
                // Outnput Single
                if (trData.OutputSingle != null)
                {
                    var hOutputSingle = new IconTextItem(8, "싱글데이터");
                    for (int i = 0; i < trData.OutputSingle.Count; i++)
                    {
                        hOutputSingle.AddChild(new IconTextItem(6, $"{trData.OutputSingle[i]}({trData.SizeSingle![i]})"));
                    }
                    hOutputs.AddChild(hOutputSingle);
                }
                // Outnput Multi
                if (trData.OutputMuti != null)
                {
                    var hOutputMuti = new IconTextItem(8, "멀티데이터");
                    for (int i = 0; i < trData.OutputMuti.Count; i++)
                    {
                        hOutputMuti.AddChild(new IconTextItem(9, $"{trData.OutputMuti[i]}({trData.SizeMuti![i]})"));
                    }
                    if (trData.OutputMuti_add != null)
                    {
                        hOutputMuti.Text += "1";
                        var hOutputMuti_add = new IconTextItem(8, "멀티데이터2");
                        for (int i = 0; i < trData.OutputMuti_add.Count; i++)
                        {
                            hOutputMuti_add.AddChild(new IconTextItem(9, $"{trData.OutputMuti_add[i]}({trData.SizeMuti_add![i]})"));
                        }
                        hOutputs.AddChild(hOutputMuti);
                        hOutputs.AddChild(hOutputMuti_add);
                    }
                    else
                    {
                        hOutputs.AddChild(hOutputMuti);
                    }
                }
                hitem.AddChild(hOutputs);
                root.AddChild(hitem);
            }

            root.Text = root.Text + $" ({root.Items.Count})";
            return root;
        });

        var root = await task;
        if (root != null)
        {
            root.IsExpanded = true;
            _Data_TR목록 = root;
            SetTreeItems((int)TREETAB_KIND.TR목록, new List<object>() { root });
        }
    }

    private async void Load_개발가이드Async()
    {
        if (_Data_개발가이드 != null) return;

        var task = Task.Run(() =>
        {
            // 개발가이드 파일 kfopsamplelist.dat
            string file_path = $"{ApiFolder}\\data\\kfopsamplelist.dat";

            var SLists = GetProfileString("SAMPLELIST", "SList", file_path).Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (SLists.Length == 0) return null;

            var root = new IconTextItem(3, "샘플 목록");
            foreach (var section in SLists)
            {
                var hChild = new IconTextItem(4, section);
                var ss = GetProfileString(section, "SrcFile", file_path, 0x800);
                var SrcFiles = ss.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in SrcFiles)
                {
                    int nPos = item.IndexOf('(');
                    if (nPos > 0)
                    {
                        string Title = item.Substring(0, nPos);
                        string path = item.Substring(nPos + 1, item.Length - nPos - 2);
                        hChild.AddChild(new IconTextItem(9, Title));
                        string Content = string.Empty;
                        try
                        {
                            Content = System.IO.File.ReadAllText($"{ApiFolder}\\data\\{path}.dat", AppEncoder);
                        }
                        catch (Exception)
                        {
                        }
                        MapDevContentToDescs["샘플 목록/" + section + "/" + Title] = Content;
                    }
                }
                root.AddChild(hChild);
            }

            return root;
        });

        var root = await task;
        if (root != null)
        {
            root.IsExpanded = true;
            _Data_개발가이드 = root;
            SetTreeItems((int)TREETAB_KIND.개발가이드, new List<object>() { root });
        }
    }

    private async void Load_화면목록Async()
    {
        if (_Data_화면목록 != null) return;

        var task = Task.Run(() =>
        {
            /// 화면목록 파일 kfopscrtrlist.dat

            string path = ApiFolder + "\\data\\kfopscrtrlist.dat";
            string[] scr_groups = { "Quote", "Order", "Chart", "Market" };
            List<string> scr_list = new List<string>();
            foreach (var scr_group in scr_groups)
            {
                var sub_list = GetProfileString("SCRLIST", scr_group, path).Split(',', StringSplitOptions.RemoveEmptyEntries);
                scr_list.AddRange(sub_list);
            }

            int TotalScreenCount = scr_list.Count;
            if (TotalScreenCount == 0) return null;

            SCRN_SPECIAL[] ScrnSpecials = new SCRN_SPECIAL[TotalScreenCount];
            for (int i = 0; i < TotalScreenCount; i++)
            {
                ref SCRN_SPECIAL ScrnSpecial = ref ScrnSpecials[i];
                ScrnSpecial.ScreenNumber = scr_list[i];
                ScrnSpecial.ScreenName = GetProfileString(ScrnSpecial.ScreenNumber, "Title", path);
                ScrnSpecial.TRs = GetProfileString(ScrnSpecial.ScreenNumber, "Trlist", path).Split(',', StringSplitOptions.RemoveEmptyEntries);
            }

            var root = new IconTextItem(3, "화면목록");

            foreach (var scrnData in ScrnSpecials)
            {
                var hitem = new IconTextItem(4, $"[{scrnData.ScreenNumber}] {scrnData.ScreenName}");
                if (scrnData.TRs != null)
                {
                    for (int i = 0; i < scrnData.TRs.Length; i++)
                    {
                        var hTRName = new IconTextItem(9, scrnData.TRs[i]);
                        hitem.AddChild(hTRName);
                    }
                }
                root.AddChild(hitem);
            }
            root.Text = root.Text + $" ({root.Items.Count})";
            return root;
        });

        var root = await task;
        if (root != null)
        {
            root.IsExpanded = true;
            _Data_화면목록 = root;
            SetTreeItems((int)TREETAB_KIND.화면목록, new List<object>() { root });
        }
    }

    private void Load_종목정보()
    {
        if (_Data_선물종목정보 != null && _Data_옵션종목정보 != null) return;

        if (axOpenAPI == null || axOpenAPI.GetConnectState() == 0) return;

        Func<string, int[], List<string[]>> ParseTextToInfo = (info, nField_Width) =>
        {
            var result = new List<string[]>();
            int nFrameWidth = nField_Width.Sum(x => x);
            int nItemCount = info.Length / nFrameWidth;
            if (nItemCount > 0 && info.Length == nItemCount * nFrameWidth)
            {
                for (int i = 0; i < nItemCount; i++)
                {
                    string[] item_info = new string[nField_Width.Length];
                    int nCharIndex = 0;
                    for (int j = 0; j < nField_Width.Length; j++)
                    {
                        item_info[j] = info.Substring(i * nFrameWidth + nCharIndex, nField_Width[j]);
                        nCharIndex += nField_Width[j];
                    }
                    result.Add(item_info.Select(x => x.Trim()).ToArray());
                }
            }
            return result;
        };

        IconTextItem CopyItem(IconTextItem orgitem)
        {
            var newItem = new IconTextItem(orgitem.IconId, orgitem.Text);
            foreach (var childitem in orgitem.Items)
            {
                if (childitem is IconTextItem item)
                {
                    newItem.AddChild(CopyItem(item));
                }
            }
            return newItem;
        }

        string[] GroupNames = { "지수", "통화", "금리", "금속", "에너지", "농산물" };
        string[] GroupCodes = { "IDX", "CUR", "INT", "MTL", "ENG", "CMD" };

        int[] nFut_Field_Width =
        {
            2, 10, 6, 40, 3, 3, 15, 15, 15, 15, 1, 15, 10, 8,
            10, 1, 1, 16
        };
        string[] sFut_Field_Name =
        {
            "식별자_F0", "종목코드", "품목코드", "품목명", "품목구분", "통화코드", "TICK단위", "TICK가치", "거래단위",
            "거래승수", "진법코드", "가격표시조정계수", "해외거래소코드", "만기일자",
            "소숫점자리수","최근월물구분", "액티브월물구분", "전일누적거래량"
        };

        var root1 = new IconTextItem(10, "해외선물 종목정보");
        var child최근월물 = new IconTextItem(0, "최근월물");
        var child액티브월물 = new IconTextItem(0, "액티브월물");
        for (int i = 0; i < GroupNames.Length; i++)
        {
            string info = axOpenAPI.GetGlobalFutOpCodeInfoByType(0, GroupCodes[i]);
            var item_infos = ParseTextToInfo(info, nFut_Field_Width);
            if (item_infos.Count > 0)
            {
                IconTextItem hGroup = new IconTextItem(11, GroupNames[i]);
                foreach (var item in item_infos)
                {
                    IconTextItem hChild = new IconTextItem(12, $"[{item[1]}] : {item[3]}"); // [종목코드] : 품목명
                    for (int k = 5; k < item.Length; k++)
                    {
                        hChild.AddChild(new IconTextItem(13, $"{sFut_Field_Name[k]} : {item[k]}"));
                    }
                    hGroup.AddChild(hChild);
                    if (item[15] == "1")
                    {
                        child최근월물.AddChild(CopyItem(hChild));
                    }
                    if (item[16] == "1")
                    {
                        child액티브월물.AddChild(CopyItem(hChild));
                    }
                }
                hGroup.Text += $"({hGroup.Items.Count})";
                root1.AddChild(hGroup);
            }
        }
        child최근월물.Text += $"({child최근월물.Items.Count})";
        child액티브월물.Text += $"({child액티브월물.Items.Count})";
        root1.AddChild(child최근월물);
        root1.AddChild(child액티브월물);

        int[] nOpt_Field_Width =
        {
            2, 10, 6, 40, 3, 3, 15, 15, 15, 15, 1, 15, 10, 8,
            12, 1, 13, 1, 10, 15, 15, 4, 5, 1
        };
        string[] sOpt_Field_Name =
        {
            "식별자_F0", "종목코드", "품목코드", "품목명", "품목구분", "통화코드", "TICK단위", "TICK가치", "거래단위",
            "거래승수", "진법코드", "가격표시조정계수", "해외거래소코드", "만기일자",
            "기초자산코드","ATM구분", "행사가", "콜풋구분", "frpc", "tick check price", "over tick price", "vtt code",
            "yymmd", "옵션type"
        };

        var root2 = new IconTextItem(10, "해외옵션 종목정보");
        for (int i = 0; i < GroupNames.Length; i++)
        {
            string info = axOpenAPI.GetGlobalFutOpCodeInfoByType(1, GroupCodes[i]);
            var item_infos = ParseTextToInfo(info, nOpt_Field_Width);
            if (item_infos.Count > 0)
            {
                IconTextItem hGroup = new IconTextItem(11, GroupNames[i]);
                foreach (var item in item_infos)
                {
                    IconTextItem hChild = new IconTextItem(12, $"[{item[1]}] : {item[3]}"); // [종목코드] : 품목명
                    for (int k = 5; k < item.Length; k++)
                    {
                        hChild.AddChild(new IconTextItem(13, $"{sOpt_Field_Name[k]} : {item[k]}"));
                    }
                    hGroup.AddChild(hChild);
                }
                hGroup.Text += $"({hGroup.Items.Count})";
                root2.AddChild(hGroup);
            }
        }

        root1.IsExpanded = true;
        root2.IsExpanded = true;

        _Data_선물종목정보 = root1;
        _Data_옵션종목정보 = root2;

        SetTreeItems((int)TREETAB_KIND.종목정보, new List<object>() { root1, root2 });
    }

    private void Load_사용자기능()
    {
        if (_Data_사용자정보 != null) return;

        if (axOpenAPI == null || axOpenAPI.GetConnectState() == 0) return;

        // 사용자기능
        var rootInfo = new IconTextItem(10, "로그인정보");
        rootInfo.AddChild(new IconTextItem(13, "사용자정보"));
        rootInfo.IsExpanded = true;

        _Data_사용자정보 = rootInfo;

        SetTreeItems((int)TREETAB_KIND.사용자기능, new List<object>() { rootInfo });
    }
}
