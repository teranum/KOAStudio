using KHOpenApi.NET;
using KOAStudio.Core.Helpers;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace WKOAStudio.Business;

internal sealed partial class BusinessLogic(IAppRegistry appRegistry) : BaseAppLogic, IUIRequest
{
    [DllImport("kernel32", CharSet = CharSet.Unicode)] private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    private static string GetProfileString(string section, string key, string file, int buflength = 255)
    {
        StringBuilder temp = new(buflength);
        _ = GetPrivateProfileString(section, key, string.Empty, temp, buflength, file);
        return temp.ToString();
    }

    private AxKFOpenAPI? _axOpenAPI;
    private string _apiFolder = string.Empty;
    private readonly Encoding _appEncoder = Encoding.GetEncoding("EUC-KR");

    private static readonly string _scrNum_REQ_TR_BASE = "3000";
    private static readonly string _scrNum_CHART_CONTENT = "3101";
    private static readonly string _scrNum_ORDER_CONTENT = "3102";

    private readonly Dictionary<string, string> _map_FidToName = [];


    private sealed class TR_SPECIAL
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
    private readonly List<TR_SPECIAL> _trDatas = [];

    private readonly Dictionary<string, string> _mapDevContentToDescs = [];

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
    private readonly Dictionary<string, List<KEY_VALUE>> _map_IniFile_TrInfo = [];

    private readonly List<string> _menu_Customize =
    [
        "키움 Open API 서비스",
        "상시모의투자 신청",
        "FID 리스트",
        "WKOAStudio 오픈소스",
    ];

    private enum TAB_TREE_KIND
    {
        실시간목록,
        TR목록,
        종목정보,
        개발가이드,
        화면목록,
        사용자기능,
    }

    private enum TAB_LIST_KIND
    {
        메시지목록,
        조회데이터,
        실시간데이터,
        실시간주문체결,
        조회한TR목록,
    }

    private bool _isRealServer = false;
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

                SetStatusText(message, changedLoginState: true, _isRealServer);
            }
        }
    }
    private readonly IAppRegistry _appRegistry = appRegistry;

    private void GetIniFileData(string ini_path, IDictionary<string, List<KEY_VALUE>> IniFile_Info)
    {
        // 파일에서 TR정보 가져오기
        string[] lines = [];
        try
        {
            lines = System.IO.File.ReadAllLines(ini_path, _appEncoder);
        }
        catch (Exception)
        {
        }

        if (lines.Length > 0)
        {

            string section = string.Empty;
            List<KEY_VALUE> key_vals = [];
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                if (line[0] == '[')
                {
                    if (section.Length > 0)
                    {
                        IniFile_Info[section] = key_vals;
                    }
                    section = line[1..^1];
                    key_vals = [];
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
        var TreeTab_image_Names = new List<IdText>()
        {
            new(0, "실시간목록"),
            new(3, "TR목록"),
            new(10, "종목정보"),
            new(3, "개발가이드"),
            new(10, "화면목록"),
            new(-1, "사용자기능"),
        };
        SetTabTrees(TreeTab_image_Names);

        // 로그 리스트뷰 만들기
        var ListTab_Names = new List<string>();
        foreach (TAB_LIST_KIND item in Enum.GetValues(typeof(TAB_LIST_KIND)))
            ListTab_Names.Add(item.ToString());
        SetTabLists(ListTab_Names);

        // 초기화
        IntPtr Handle = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
        _axOpenAPI = new AxKFOpenAPI(Handle);
        if (!_axOpenAPI.Created)
        {
            LoginState = OpenApiLoginState.ApiCreateFailed;
            return;
        }

        _apiFolder = _axOpenAPI.GetAPIModulePath();

        _axOpenAPI.OnEventConnect += AxKFOpenAPI_OnEventConnect;
        _axOpenAPI.OnReceiveChejanData += AxKFOpenAPI_OnReceiveChejanData;
        _axOpenAPI.OnReceiveMsg += AxKFOpenAPI_OnReceiveMsg;
        _axOpenAPI.OnReceiveRealData += AxKFOpenAPI_OnReceiveRealData;
        _axOpenAPI.OnReceiveTrData += AxKFOpenAPI_OnReceiveTrData;

        OutputLog((int)TAB_LIST_KIND.메시지목록, "여기에 수신된 메시지가 표시됩니다");
        OutputLog((int)TAB_LIST_KIND.조회데이터, "여기에 전문 조회 데이터가 표시됩니다 (OnReceiveTrData)");
        OutputLog((int)TAB_LIST_KIND.실시간데이터, "여기에 전문 실시간 데이터가 표시됩니다 (OnReceiveRealData)");
        OutputLog((int)TAB_LIST_KIND.실시간주문체결, "여기에 전문 실시간 주문체결 데이터가 표시됩니다 (OnReceiveChejanData)");
        OutputLog((int)TAB_LIST_KIND.조회한TR목록, "여기에 조회한 전문목록(TR목록)이 최근순으로 표시됩니다. 각TR을 더블클릭하면 조회입력값이 자동으로 설정됩니다");
        OutputLogResetAllChangeState();

        // FID 네임 가져오기
        // /data/fidinfo.dat
        // #field_id	field_name	data_type	data_size	description
        // # 탭으로 구분자 두어 저장합니다.
        string FID_KORNAME = string.Empty;
        try
        {
            string fid_define_path = $"{_apiFolder}\\data\\fidinfo.dat";
            FID_KORNAME = System.IO.File.ReadAllText(fid_define_path, _appEncoder);
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

                        if (name[nPos] == ',')
                        {
                            name = name[..nPos];
                            break;
                        }
                        nPos++;
                    }
                    _map_FidToName[key] = name;
                }
            }
        }

        // 파일에서 TR정보 가져오기
        GetIniFileData($"{_apiFolder}\\data\\kfoptrinfo.dat", _map_IniFile_TrInfo);

        // 초기데이터 로딩
        Load_실시간목록Async();
        Load_TR목록Async();
        Load_화면목록Async();
        Load_개발가이드Async();
        Load_사용자기능();
    }

    private IdTextItem? _data_실시간목록;
    private IdTextItem? _data_TR목록;
    private IdTextItem? _data_개발가이드;
    private IdTextItem? _data_화면목록;
    private IdTextItem? _data_선물종목정보;
    private IdTextItem? _data_옵션종목정보;
    private IdTextItem? _data_사용자정보;

    private async void Load_실시간목록Async()
    {
        if (_data_실시간목록 != null) return;

        var task = Task.Run(() =>
        {
            var root = new IdTextItem(0, "실시간목록");
            if (_map_IniFile_TrInfo.TryGetValue("TRLIST", out List<KEY_VALUE>? trlists))
            {
                if (trlists != null)
                {
                    string[]? real_tr_names = null;
                    var real_tr = trlists.Find(tr => tr.Key.Equals("Real"));

                    real_tr_names = real_tr.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (real_tr_names != null && real_tr_names.Length > 0)
                    {
                        foreach (var key in real_tr_names)
                        {
                            if (_map_IniFile_TrInfo.TryGetValue(key, out List<KEY_VALUE>? key_values))
                            {
                                string name = string.Empty;
                                List<string> Fids = [];
                                IdTextItem? hitem = null;
                                foreach (var item in key_values)
                                {
                                    if (item.Key.Equals("Title"))
                                    {
                                        hitem = new IdTextItem(1, "Real Type : " + item.Value);
                                    }
                                    else if (hitem != null && !item.Key.Equals("Type") && !item.Value.Equals("-1"))
                                    {
                                        var valTemp = item.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim());
                                        if (valTemp != null && valTemp.Any())
                                        {
                                            string text = "[";
                                            foreach (var tmpFid in valTemp)
                                            {
                                                if (text.Length > 1)
                                                {
                                                    text += ",";
                                                }
                                                text += tmpFid;
                                                _map_FidToName[tmpFid] = item.Key;
                                            }
                                            text += $"] = {item.Key}";
                                            hitem.AddChild(new IdTextItem(2, text));
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
                            root.Text += $" ({root.Items.Count})";
                        }
                    }
                }
            }
            return root;
        });

        var root = await task.ConfigureAwait(true);
        if (root != null)
        {
            root.IsExpanded = true;
            _data_실시간목록 = root;
            SetTreeItems((int)TAB_TREE_KIND.실시간목록, new List<object>() { root });
        }
    }

    private async void Load_TR목록Async()
    {
        if (_data_TR목록 != null) return;

        var task = Task.Run(() =>
        {
            if (_map_IniFile_TrInfo.TryGetValue("TRLIST", out List<KEY_VALUE>? trlists))
            {
                if (trlists != null)
                {
                    string[] sections = ["Quote", "Order", "Chart"];
                    var all_trs = trlists.Where(tr => sections.Contains(tr.Key, StringComparer.Ordinal));
                    List<string> tr_names = [];

                    foreach (var tr_part in all_trs)
                    {
                        tr_names.AddRange([.. tr_part.Value.Split(',', StringSplitOptions.RemoveEmptyEntries)]);
                    }
                    if (tr_names.Count > 0)
                    {
                        foreach (var tr_name in tr_names)
                        {
                            TR_SPECIAL trData = new()
                            {
                                Code = tr_name,
                                Inputs = [],
                                InputDescs = [],
                                Caution = string.Empty,
                            };

                            if (_map_IniFile_TrInfo.TryGetValue(tr_name + "_INPUT", out List<KEY_VALUE>? key_values))
                            {
                                trData.Name = key_values.Find(tr => tr.Key.Equals("Title")).Value;
                                foreach (var key_value in key_values)
                                {
                                    if (key_value.Key.Equals("Title"))
                                        trData.Name = key_value.Value;
                                    else
                                    {
                                        trData.Inputs.Add(key_value.Key);
                                        trData.InputDescs.Add(string.Empty);
                                    }
                                }
                            }
                            if (_map_IniFile_TrInfo.TryGetValue(tr_name + "_INPUT_LEGEND", out key_values))
                            {
                                foreach (var key_value in key_values)
                                {
                                    for (int i = 0; i < trData.Inputs.Count; i++)
                                    {
                                        if (key_value.Key.Equals(trData.Inputs[i]))
                                        {
                                            trData.InputDescs[i] = key_value.Value;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (_map_IniFile_TrInfo.TryGetValue(tr_name + "_OUTPUT", out key_values))
                            {
                                trData.OutputSingle = [];
                                trData.SizeSingle = [];
                                foreach (var key_value in key_values)
                                {
                                    if (!key_value.Key.Equals("Title") && !key_value.Key.Equals("출력건수"))
                                    {
                                        trData.OutputSingle.Add(key_value.Key);
                                        // size
                                        int size = 0;
                                        int pos = key_value.Value.IndexOf(',');
                                        if (pos >= 0)
                                            size = Convert.ToInt32(key_value.Value[..pos]);
                                        trData.SizeSingle.Add(size);
                                    }
                                }
                            }
                            string GFID = string.Empty;
                            if (_map_IniFile_TrInfo.TryGetValue(tr_name + "_TRINFO", out key_values))
                            {
                                GFID = key_values.Find(tr => tr.Key.Equals("GFID")).Value;
                            }

                            if (!GFID.Contains(','))
                            {
                                if (_map_IniFile_TrInfo.TryGetValue(tr_name + "_OCCURS_" + GFID, out key_values))
                                {
                                    trData.OutputMuti = [];
                                    trData.SizeMuti = [];
                                    foreach (var key_value in key_values)
                                    {
                                        if (!key_value.Key.Equals("Title") && !key_value.Key.Equals("Count"))
                                        {
                                            trData.OutputMuti.Add(key_value.Key);
                                            // size
                                            int size = 0;
                                            int pos = key_value.Value.IndexOf(',');
                                            if (pos >= 0)
                                                size = Convert.ToInt32(key_value.Value[..pos]);
                                            trData.SizeMuti.Add(size);
                                        }
                                    }
                                }
                            }
                            else if (GFID.Equals("0,1"))
                            {
                                var gfids = GFID.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                if (gfids.Length == 2)
                                {
                                    if (_map_IniFile_TrInfo.TryGetValue(tr_name + "_OCCURS_" + gfids[0], out key_values))
                                    {
                                        trData.OutputMuti = [];
                                        trData.SizeMuti = [];
                                        foreach (var key_value in key_values)
                                        {
                                            if (!key_value.Key.Equals("Title") && !key_value.Key.Equals("Count"))
                                            {
                                                trData.OutputMuti.Add(key_value.Key);
                                                // size
                                                int size = 0;
                                                int pos = key_value.Value.IndexOf(',');
                                                if (pos >= 0)
                                                    size = Convert.ToInt32(key_value.Value[..pos]);
                                                trData.SizeMuti.Add(size);
                                            }
                                        }
                                    }
                                    if (_map_IniFile_TrInfo.TryGetValue(tr_name + "_OCCURS_" + gfids[1], out key_values))
                                    {
                                        trData.OutputMuti_add = [];
                                        trData.SizeMuti_add = [];
                                        foreach (var key_value in key_values)
                                        {
                                            if (!key_value.Key.Equals("Title") && !key_value.Key.Equals("Count"))
                                            {
                                                trData.OutputMuti_add.Add(key_value.Key);
                                                // size
                                                int size = 0;
                                                int pos = key_value.Value.IndexOf(',');
                                                if (pos >= 0)
                                                    size = Convert.ToInt32(key_value.Value[..pos]);
                                                trData.SizeMuti_add.Add(size);
                                            }
                                        }
                                    }
                                }
                            }

                            _trDatas.Add(trData);
                        }
                    }
                }
            }

            if (_trDatas.Count == 0) return null;

            var root = new IdTextItem(3, "TR목록");

            foreach (var trData in _trDatas)
            {
                var hitem = new IdTextItem(trData.Caution.Length > 0 ? 14 : 4, $"{trData.Code} : {trData.Name}");
                // Inputs
                if (trData.Inputs != null)
                {
                    var hInputs = new IdTextItem(5, "[INPUT]");
                    foreach (var item in trData.Inputs)
                    {
                        hInputs.AddChild(new IdTextItem(6, item));
                    }
                    hitem.AddChild(hInputs);
                }
                // Outnput
                var hOutputs = new IdTextItem(7, "[OUTPUT]");
                // Outnput Single
                if (trData.OutputSingle != null)
                {
                    var hOutputSingle = new IdTextItem(8, "싱글데이터");
                    for (int i = 0; i < trData.OutputSingle.Count; i++)
                    {
                        hOutputSingle.AddChild(new IdTextItem(6, $"{trData.OutputSingle[i]}({trData.SizeSingle![i]})"));
                    }
                    hOutputs.AddChild(hOutputSingle);
                }
                // Outnput Multi
                if (trData.OutputMuti != null)
                {
                    var hOutputMuti = new IdTextItem(8, "멀티데이터");
                    for (int i = 0; i < trData.OutputMuti.Count; i++)
                    {
                        hOutputMuti.AddChild(new IdTextItem(9, $"{trData.OutputMuti[i]}({trData.SizeMuti![i]})"));
                    }
                    if (trData.OutputMuti_add != null)
                    {
                        hOutputMuti.Text += "1";
                        var hOutputMuti_add = new IdTextItem(8, "멀티데이터2");
                        for (int i = 0; i < trData.OutputMuti_add.Count; i++)
                        {
                            hOutputMuti_add.AddChild(new IdTextItem(9, $"{trData.OutputMuti_add[i]}({trData.SizeMuti_add![i]})"));
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

            root.Text += $" ({root.Items.Count})";
            return root;
        });

        var root = await task.ConfigureAwait(true);
        if (root != null)
        {
            root.IsExpanded = true;
            _data_TR목록 = root;
            SetTreeItems((int)TAB_TREE_KIND.TR목록, new List<object>() { root });
        }
    }

    private async void Load_개발가이드Async()
    {
        if (_data_개발가이드 != null) return;

        var task = Task.Run(() =>
        {
            // 개발가이드 파일 kfopsamplelist.dat
            string file_path = $"{_apiFolder}\\data\\kfopsamplelist.dat";

            var SLists = GetProfileString("SAMPLELIST", "SList", file_path).Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (SLists.Length == 0) return null;

            var root = new IdTextItem(3, "샘플 목록");
            foreach (var section in SLists)
            {
                var hChild = new IdTextItem(4, section);
                var ss = GetProfileString(section, "SrcFile", file_path, 0x800);
                var SrcFiles = ss.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in SrcFiles)
                {
                    int nPos = item.IndexOf('(');
                    if (nPos > 0)
                    {
                        string Title = item[..nPos];
                        string path = item.Substring(nPos + 1, item.Length - nPos - 2);
                        hChild.AddChild(new IdTextItem(9, Title));
                        string Content = string.Empty;
                        try
                        {
                            Content = System.IO.File.ReadAllText($"{_apiFolder}\\data\\{path}.dat", _appEncoder);
                        }
                        catch (Exception)
                        {
                        }
                        _mapDevContentToDescs["샘플 목록/" + section + "/" + Title] = Content;
                    }
                }
                root.AddChild(hChild);
            }

            return root;
        });

        var root = await task.ConfigureAwait(true);
        if (root != null)
        {
            root.IsExpanded = true;
            _data_개발가이드 = root;
            SetTreeItems((int)TAB_TREE_KIND.개발가이드, new List<object>() { root });
        }
    }

    private async void Load_화면목록Async()
    {
        if (_data_화면목록 != null) return;

        var task = Task.Run(() =>
        {
            /// 화면목록 파일 kfopscrtrlist.dat

            string path = _apiFolder + "\\data\\kfopscrtrlist.dat";
            string[] scr_groups = ["Quote", "Order", "Chart", "Market"];
            List<string> scr_list = [];
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

            var root = new IdTextItem(3, "화면목록");

            foreach (var scrnData in ScrnSpecials)
            {
                var hitem = new IdTextItem(4, $"[{scrnData.ScreenNumber}] {scrnData.ScreenName}");
                if (scrnData.TRs != null)
                {
                    for (int i = 0; i < scrnData.TRs.Length; i++)
                    {
                        var hTRName = new IdTextItem(9, scrnData.TRs[i]);
                        hitem.AddChild(hTRName);
                    }
                }
                root.AddChild(hitem);
            }
            root.Text += $" ({root.Items.Count})";
            return root;
        });

        var root = await task.ConfigureAwait(true);
        if (root != null)
        {
            root.IsExpanded = true;
            _data_화면목록 = root;
            SetTreeItems((int)TAB_TREE_KIND.화면목록, new List<object>() { root });
        }
    }

    private void Load_종목정보()
    {
        if (_data_선물종목정보 != null && _data_옵션종목정보 != null) return;

        if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0) return;

        static List<string[]> ParseTextToInfo(string info, int[] nField_Width)
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
                    result.Add([.. item_info.Select(x => x.Trim())]);
                }
            }
            return result;
        }

        static IdTextItem CopyItem(IdTextItem orgitem)
        {
            var newItem = new IdTextItem(orgitem.Id, orgitem.Text);
            foreach (var childitem in orgitem.Items)
            {
                if (childitem is IdTextItem item)
                {
                    newItem.AddChild(CopyItem(item));
                }
            }
            return newItem;
        }

        string[] GroupNames = ["지수", "통화", "금리", "금속", "에너지", "농산물"];
        string[] GroupCodes = ["IDX", "CUR", "INT", "MTL", "ENG", "CMD"];

        int[] nFut_Field_Width =
        [
            2,
            10,
            6,
            40,
            3,
            3,
            15,
            15,
            15,
            15,
            1,
            15,
            10,
            8,
            10,
            1,
            1,
            16,
        ];
        string[] sFut_Field_Name =
        [
            "식별자_F0",
            "종목코드",
            "품목코드",
            "품목명",
            "품목구분",
            "통화코드",
            "TICK단위",
            "TICK가치",
            "거래단위",
            "거래승수",
            "진법코드",
            "가격표시조정계수",
            "해외거래소코드",
            "만기일자",
            "소숫점자리수",
            "최근월물구분",
            "액티브월물구분",
            "전일누적거래량",
        ];

        var root1 = new IdTextItem(10, "해외선물 종목정보");
        var child최근월물 = new IdTextItem(0, "최근월물");
        var child액티브월물 = new IdTextItem(0, "액티브월물");
        for (int i = 0; i < GroupNames.Length; i++)
        {
            string info = _axOpenAPI.GetGlobalFutOpCodeInfoByType(0, GroupCodes[i]);
            var item_infos = ParseTextToInfo(info, nFut_Field_Width);
            if (item_infos.Count > 0)
            {
                IdTextItem hGroup = new(11, GroupNames[i]);
                foreach (var item in item_infos)
                {
                    IdTextItem hChild = new(12, $"[{item[1]}] : {item[3]}"); // [종목코드] : 품목명
                    for (int k = 5; k < item.Length; k++)
                    {
                        hChild.AddChild(new IdTextItem(13, $"{sFut_Field_Name[k]} : {item[k]}"));
                    }
                    hGroup.AddChild(hChild);
                    if (item[15].Equals("1"))
                    {
                        child최근월물.AddChild(CopyItem(hChild));
                    }
                    if (item[16].Equals("1"))
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
        [
            2,
            10,
            6,
            40,
            3,
            3,
            15,
            15,
            15,
            15,
            1,
            15,
            10,
            8,
            12,
            1,
            13,
            1,
            10,
            15,
            15,
            4,
            5,
            1,
        ];
        string[] sOpt_Field_Name =
        [
            "식별자_F0",
            "종목코드",
            "품목코드",
            "품목명",
            "품목구분",
            "통화코드",
            "TICK단위",
            "TICK가치",
            "거래단위",
            "거래승수",
            "진법코드",
            "가격표시조정계수",
            "해외거래소코드",
            "만기일자",
            "기초자산코드",
            "ATM구분",
            "행사가",
            "콜풋구분",
            "frpc",
            "tick check price",
            "over tick price",
            "vtt code",
            "yymmd",
            "옵션type",
        ];

        var root2 = new IdTextItem(10, "해외옵션 종목정보");
        for (int i = 0; i < GroupNames.Length; i++)
        {
            string info = _axOpenAPI.GetGlobalFutOpCodeInfoByType(1, GroupCodes[i]);
            var item_infos = ParseTextToInfo(info, nOpt_Field_Width);
            if (item_infos.Count > 0)
            {
                IdTextItem hGroup = new(11, GroupNames[i]);
                foreach (var item in item_infos)
                {
                    IdTextItem hChild = new(12, $"[{item[1]}] : {item[3]}"); // [종목코드] : 품목명
                    for (int k = 5; k < item.Length; k++)
                    {
                        hChild.AddChild(new IdTextItem(13, $"{sOpt_Field_Name[k]} : {item[k]}"));
                    }
                    hGroup.AddChild(hChild);
                }
                hGroup.Text += $"({hGroup.Items.Count})";
                root2.AddChild(hGroup);
            }
        }

        root1.IsExpanded = true;
        root2.IsExpanded = true;

        _data_선물종목정보 = root1;
        _data_옵션종목정보 = root2;

        SetTreeItems((int)TAB_TREE_KIND.종목정보, new List<object>() { root1, root2 });
    }

    private void Load_사용자기능()
    {
        List<object> lists = [];
        // 기본정보 표시
        lists.Add(new IdTextItem(9, "Api정보"));

        // 로그인 정보
        if (_data_사용자정보 != null) goto end_proc;

        if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0) goto end_proc;

        // 사용자기능
        var rootAccount = new IdTextItem(10, "로그인정보");
        rootAccount.AddChild(new(13, "사용자정보"));
        rootAccount.IsExpanded = true;

        _data_사용자정보 = rootAccount;

        lists.Add(rootAccount);

    end_proc:

        // 기타 tools
        IdTextItem? rootTools;
        rootTools = new(0, "차트요청")
        {
            IsExpanded = true,
        };
        lists.Add(rootTools);
        rootTools.AddChild(new(9, "해외선물옵션차트요청"));

        rootTools = new(0, "주문요청")
        {
            IsExpanded = true,
        };
        lists.Add(rootTools);
        rootTools.AddChild(new(9, "해외선물옵션주문요청"));

        SetTreeItems((int)TAB_TREE_KIND.사용자기능, lists);
    }

    public void Close()
    {
        SaveUserContentInfo();
    }
}
