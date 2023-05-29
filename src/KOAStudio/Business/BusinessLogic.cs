using KHOpenApi.NET;
using KOAStudio.Core.Helpers;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Xml;

namespace KOAStudio.Business;

internal sealed partial class BusinessLogic : IUIRequest, ILogicNotify
{
    [DllImport("kernel32")] private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    private static string GetProfileString(string section, string key, string file)
    {
        StringBuilder temp = new StringBuilder(255);
        int ret = GetPrivateProfileString(section, key, "", temp, 255, file);
        return temp.ToString();
    }

    private AxKHOpenAPI? axOpenAPI;
    private string ApiFolder = string.Empty;
    private readonly Encoding AppEncoder = Encoding.GetEncoding(949);

    private static readonly string SCR_REQ_TR_BASE = "3000";
    private static readonly string SCR_REQ_COND_BASE = "4000";
    private static readonly string SCR_REQ_COND_LAST = "4999";

    private readonly Dictionary<string, string> MapCondNameToIndex = new Dictionary<string, string>();

    private readonly Dictionary<string, string> Map_FidToName = new Dictionary<string, string>();

    private class TR_SPECIAL
    {
        public string Code = string.Empty;
        public string Name = string.Empty;
        public List<string>? Inputs;
        public List<string>? OutputSingle;
        public List<string>? OutputMuti;
        public string Caution = string.Empty;
        public List<string>? InputDescs;
    }
    private readonly List<TR_SPECIAL> TrDatas = new List<TR_SPECIAL>();

    private readonly Dictionary<string, string> MapDevContentToDescs = new Dictionary<string, string>();

    private struct SCRN_SPECIAL
    {
        public string Catagory;
        public string ScreenNumber;
        public string ScreenName;
        public string[] TRs;
    }

    private struct KEY_VALUE
    {
        public string Key;
        public string Value;
    };

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
        조건검색실시간,
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
    private readonly IAppRegistry _appRegistry;
    public BusinessLogic(IAppRegistry appRegistry)
    {
        _appRegistry = appRegistry;
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
            new IconText(-1, "사용자기능"),
        };
        SetTabTrees(TreeTab_image_Names);

        // 로그 리스트뷰 만들기
        var ListTab_Names = new List<string>();
        foreach (LIST_TAB_KIND item in Enum.GetValues(typeof(LIST_TAB_KIND)))
            ListTab_Names.Add(item.ToString());
        SetTabLists(ListTab_Names);

        // Api Creating
        IntPtr Handle = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
        axOpenAPI = new AxKHOpenAPI(Handle);
        if (!axOpenAPI.Created)
        {
            LoginState = OpenApiLoginState.ApiCreateFailed;
            return;
        }

        ApiFolder = axOpenAPI.GetAPIModulePath();

        axOpenAPI.OnEventConnect += AxKHOpenApi_OnEventConnect;
        axOpenAPI.OnReceiveChejanData += AxKHOpenApi_OnReceiveChejanData;
        axOpenAPI.OnReceiveConditionVer += AxKHOpenApi_OnReceiveConditionVer;
        axOpenAPI.OnReceiveInvestRealData += AxKHOpenApi_OnReceiveInvestRealData;
        axOpenAPI.OnReceiveMsg += AxKHOpenApi_OnReceiveMsg;
        axOpenAPI.OnReceiveRealCondition += AxKHOpenApi_OnReceiveRealCondition;
        axOpenAPI.OnReceiveRealData += AxKHOpenApi_OnReceiveRealData;
        axOpenAPI.OnReceiveTrCondition += AxKHOpenApi_OnReceiveTrCondition;
        axOpenAPI.OnReceiveTrData += AxKHOpenApi_OnReceiveTrData;

        OutputLog((int)LIST_TAB_KIND.메시지목록, "여기에 수신된 메시지가 표시됩니다");
        OutputLog((int)LIST_TAB_KIND.조회데이터, "여기에 전문 조회 데이터가 표시됩니다 (OnReceiveTrData)");
        OutputLog((int)LIST_TAB_KIND.실시간데이터, "여기에 전문 실시간 데이터가 표시됩니다 (OnReceiveRealData)");
        OutputLog((int)LIST_TAB_KIND.조건검색실시간, "여기에 전문 조건검색 실시간 데이터가 표시됩니다 (OnReceiveRealCondition)");
        OutputLog((int)LIST_TAB_KIND.실시간주문체결, "여기에 전문 실시간 주문체결 데이터가 표시됩니다 (OnReceiveChejanData)");
        OutputLog((int)LIST_TAB_KIND.조회한TR목록, "여기에 조회한 전문목록(TR목록)이 최근순으로 표시됩니다. 각TR을 더블클릭하면 조회입력값이 자동으로 설정됩니다");

        //
        string[] FIDLines = Regex.Split(Properties.Resources.FID_KORNAME, "\r\n|\r|\n");
        int nFIDLines = FIDLines.Length;
        foreach (string line in FIDLines)
        {
            int nPos = line.IndexOf('=');
            if (nPos > 0)
            {
                string key = line.Substring(0, nPos);
                string name = line.Substring(nPos + 1);
                Map_FidToName.Add(key, name);
            }
        }
        //

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
    private IconTextItem? _Data_종목정보;
    private IconTextItem? _Data_사용자정보;
    private IconTextItem? _Data_조건검색;

    private async void Load_실시간목록Async()
    {
        if (_Data_실시간목록 != null) return;

        var task = Task.Run(() =>
        {
            string filepath = $"{ApiFolder}\\system\\koarealtime.dat";
            List<byte[]> lines = new List<byte[]>();
            try
            {
                var fileDatas = System.IO.File.ReadAllBytes(filepath);
                int filelength = fileDatas.Length;
                int nBytePos = 0;
                int nLineStartPos = 0;
                while (nBytePos < filelength)
                {
                    if (fileDatas[nBytePos] == '\n')
                    {
                        lines.Add(fileDatas.Skip(nLineStartPos).Take(nBytePos - nLineStartPos - 1).ToArray());
                        nLineStartPos = nBytePos + 1;
                    }
                    nBytePos++;
                }
                if (filelength > nLineStartPos)
                {
                    lines.Add(fileDatas.Skip(nLineStartPos).Take(filelength - nLineStartPos).ToArray());
                }
            }
            catch (Exception)
            {
                //throw;
            }

            if (lines.Count == 0) return null;

            var root = new IconTextItem(0, "실시간목록");

            foreach (var line in lines)
            {
                // 형식 = GIDC(1) + DESC(19) + NFID(3) + FID1(5) + ... + FIDn(5)'\r\n'
                if (line.Length < 23) continue;
                byte[] GIDC = line.Skip(0).Take(1).ToArray();
                byte[] DESC = line.Skip(1).Take(19).ToArray();
                byte[] NFID = line.Skip(20).Take(3).ToArray();
                if (GIDC[0] == ';') continue;
                int FidCount = Convert.ToInt32(AppEncoder.GetString(NFID));
                string name = AppEncoder.GetString(DESC).Trim();
                if (FidCount == 0 || line.Length < FidCount * 5 + 23)
                    continue;

                var hitem = new IconTextItem(1, "Real Type : " + name);
                for (int i = 0; i < FidCount; i++)
                {
                    string fid = AppEncoder.GetString(line.Skip(23 + 5 * i).Take(5).ToArray()).Trim();
                    string fiddesc;
                    if (Map_FidToName.TryGetValue(fid, out var fid_name))
                        fiddesc = $"[{fid}] = {fid_name}";
                    else
                        fiddesc = $"[{fid}] = 'Extra Item'";
                    hitem.AddChild(new IconTextItem(2, fiddesc));
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
            _Data_실시간목록 = root;
            SetTreeItems((int)TREETAB_KIND.실시간목록, new List<object>() { root });
        }
    }

    private async void Load_TR목록Async()
    {
        if (_Data_TR목록 != null) return;

        var task = Task.Run(() =>
        {
            /// ENC파일 읽기
            /// 폴더에서 ENC파일 검색후
            /// 압축해제
            /// 파일네임과 동일한 압축파일에서 읽기
            string path = ApiFolder + "\\data";
            if (!System.IO.Directory.Exists(path)) return null;

            string[] files = System.IO.Directory.GetFiles(path, "*.enc");
            if (files.Length == 0) return null;

            string szIniFilePath = ApiFolder + "\\koatrinputlegend.ini";
            var buffer = new byte[16384];
            foreach (var encfilepath in files)
            {
                string FileTitle = System.IO.Path.GetFileNameWithoutExtension(encfilepath);
                try
                {
                    using (var file = System.IO.File.OpenRead(encfilepath))
                    using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            string entruTitle = entry.Name.Substring(0, entry.Name.Length - 4);
                            if (entruTitle == FileTitle.ToUpper())
                            {
                                using (var stream = entry.Open())
                                {
                                    TR_SPECIAL trData = new TR_SPECIAL();
                                    //
                                    trData.Code = FileTitle;
                                    stream.Read(buffer, 0, buffer.Length);
                                    string text = AppEncoder.GetString(buffer);
                                    int nLen = text.Length;
                                    // [INPUT]
                                    int nPos = 0;
                                    int nPosEnd = 0;
                                    nPos = text.IndexOf("[INPUT]", nPos);
                                    nPos = text.IndexOf("@START_", nPos);
                                    nPos += "@START_".Length;
                                    nPosEnd = text.IndexOf("\r\n", nPos);
                                    string TRName = text.Substring(nPos, nPosEnd - nPos);
                                    trData.Name = TRName;
                                    nPos = nPosEnd + "\r\n".Length;
                                    nPosEnd = text.IndexOf("@END_", nPos);
                                    string InputBody = text.Substring(nPos, nPosEnd - nPos);
                                    trData.Inputs = GetKeyNames(InputBody);
                                    // [OUTPUT]
                                    nPos = nPosEnd;
                                    nPos = text.IndexOf("[OUTPUT]", nPos);
                                    nPos = text.IndexOf("@START_", nPos);
                                    nPosEnd = text.IndexOf("=", nPos);
                                    string OutName, OutIdent;
                                    OutName = text.Substring(nPos + 7, nPosEnd - nPos - 7);
                                    nPos = nPosEnd + 1;
                                    nPosEnd = text.IndexOf("\r\n", nPos);
                                    OutIdent = text.Substring(nPos, nPosEnd - nPos);
                                    nPos = nPosEnd + "\r\n".Length;
                                    nPosEnd = text.IndexOf("@END_", nPos);
                                    string namebody = text.Substring(nPos, nPosEnd - nPos);
                                    if (OutIdent == "*,*,*")
                                    {
                                        trData.OutputSingle = GetKeyNames(namebody);
                                        nPos = nPosEnd + "\r\n".Length;
                                        nPos = text.IndexOf("@START_", nPos);
                                        if (nPos != -1)
                                        {
                                            nPosEnd = text.IndexOf("\r\n", nPos);
                                            nPos = nPosEnd + "\r\n".Length;
                                            nPosEnd = text.IndexOf("@END_", nPos);
                                            string ortherbody = text.Substring(nPos, nPosEnd - nPos);
                                            trData.OutputMuti = GetKeyNames(ortherbody);
                                        }
                                    }
                                    else
                                    {
                                        trData.OutputMuti = GetKeyNames(namebody);
                                    }
                                    // Caution and InputDescs
                                    string section = trData.Code.ToUpper() + " : " + trData.Name;
                                    trData.Caution = GetProfileString(section, "주의", szIniFilePath);
                                    trData.InputDescs = trData.Inputs.Select(x => GetProfileString(section, x, szIniFilePath)).ToList();
                                    TrDatas.Add(trData);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
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
                    var hOutputSingle = new IconTextItem(8, $"싱글데이터 [{trData.Name.Substring(0, trData.Name.Length - 2)}]");
                    foreach (var item in trData.OutputSingle)
                    {
                        hOutputSingle.AddChild(new IconTextItem(6, item));
                    }
                    hOutputs.AddChild(hOutputSingle);
                }
                // Outnput Multi
                if (trData.OutputMuti != null)
                {
                    var hOutputMuti = new IconTextItem(8, $"멀티데이터 [{trData.Name.Substring(0, trData.Name.Length - 2)}]");
                    foreach (var item in trData.OutputMuti)
                    {
                        hOutputMuti.AddChild(new IconTextItem(9, item));
                    }
                    hOutputs.AddChild(hOutputMuti);
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

        // sub function
        List<string> GetKeyNames(string s)
        {
            List<string> sections = new List<string>();
            string[] lines = s.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                int pos = line.IndexOf('=');
                if (pos != -1)
                {
                    sections.Add(line.Substring(0, pos).Trim());
                }
            }
            return sections;
        };
    }

    private async void Load_개발가이드Async()
    {
        if (_Data_개발가이드 != null) return;

        var task = Task.Run(() =>
        {
            /// 개발가이드 파일 koa_devguide.xml
            /// use XML Parser

            string path = ApiFolder + "\\koa_devguide.xml";
            if (!System.IO.File.Exists(path)) return null;

            string text = System.IO.File.ReadAllText(path, AppEncoder);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(text);

            var nodes = xmlDocument.SelectNodes("/KOA_DevGuideList/*");
            if (nodes == null) return null;

            var root = new IconTextItem(3, "개발 가이드");

            string szMapName = "개발 가이드";
            foreach (XmlNode node in nodes)
            {
                XMLReadNameAndText(root, node, szMapName, 1);
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

        // sub function
        bool XMLReadNameAndText(IconTextItem hParent, XmlNode node, string szMapName, int stack)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                var DOMNamedNodeMapPtr = node.Attributes;
                if (DOMNamedNodeMapPtr != null)
                {
                    int length = DOMNamedNodeMapPtr.Count;
                    string szNameValue = string.Empty;
                    string szTypeValue = string.Empty; ;
                    for (int j = 0; j < length; j++)
                    {
                        //get attribute node:							
                        var pIAttrNode = DOMNamedNodeMapPtr.Item(j);
                        if (pIAttrNode != null)
                        {
                            string bName = pIAttrNode.Name;
                            if (bName == "type")
                            {
                                szTypeValue = pIAttrNode.InnerText;
                            }
                            if (bName == "name")
                            {
                                szNameValue = pIAttrNode.InnerText;
                                break;
                            }
                        }
                    }
                    string szNewMapName = szMapName;
                    szNewMapName += "/";
                    szNewMapName += szNameValue;

                    int nImg = stack;
                    if (nImg == 1)
                        nImg = 4;
                    else if (nImg == 2)
                        nImg = 7;
                    else if (nImg == 3)
                    {
                        if (szTypeValue == "event")
                            nImg = 9;
                        else
                            nImg = 6;
                    }
                    if (szNameValue.Length > 0)
                    {
                        var hLayer = new IconTextItem(nImg, szNameValue);
                        hParent.AddChild(hLayer);
                        long nChildCount = node.ChildNodes.Count;
                        if (nChildCount > 1)
                        {
                            for (int i = 0; i < nChildCount; i++)
                            {
                                var childnode = node.ChildNodes.Item(i);
                                if (childnode != null)
                                    XMLReadNameAndText(hLayer, childnode, szNewMapName, stack + 1);
                            }
                        }
                        else if (nChildCount == 1)
                        {
                            var childnode = node.ChildNodes.Item(0);
                            if (childnode != null)
                            {
                                if (childnode.NodeType == XmlNodeType.Element)
                                {
                                    XMLReadNameAndText(hLayer, childnode, szNewMapName, stack + 1);
                                }
                                else
                                {
                                    // Loading Text
                                    string szTextWithReturn = node.InnerText;
                                    int nLen = szTextWithReturn.Length;
                                    while (nLen > 0)
                                    {
                                        nLen--;
                                        if (szTextWithReturn[nLen] == '\n')
                                        {
                                            szTextWithReturn.Insert(nLen, "\r");
                                        }
                                    }
                                    MapDevContentToDescs.Add(szNewMapName, szTextWithReturn);
                                }
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }

    private async void Load_화면목록Async()
    {
        if (_Data_화면목록 != null) return;

        var task = Task.Run(() =>
        {
            /// 화면목록 파일 koascreentrmap.ini
            /// [0]
            /// Catagory=주식주문
            /// ScreenNumber=4989
            /// ScreenName=키움주문
            /// TRCount=1
            /// TR_0=

            string path = ApiFolder + "\\koascreentrmap.ini";
            if (!System.IO.File.Exists(path)) return null;

            string[] lines = System.IO.File.ReadAllLines(path, AppEncoder);

            string Section;
            KEY_VALUE KeyAndValue;
            string line = lines[0];


            SCRN_SPECIAL[] ScrnSpecials = Array.Empty<SCRN_SPECIAL>();
            if (line[0] == '[')
            {
                Section = GetSection(line);
                if (Section == "Info")
                {
                    line = lines[1];
                    KeyAndValue = GetKeyAndValue(line);
                    if (KeyAndValue.Key == "TotalScreenCount")
                    {
                        int TotalScreenCount = Convert.ToInt32(KeyAndValue.Value);
                        ScrnSpecials = new SCRN_SPECIAL[TotalScreenCount];
                        int nSectionNumber = 0;
                        for (int i = 2; i < lines.Length; i++)
                        {
                            line = lines[i];
                            if (line.Length < 3) continue;
                            if (line[0] == '[')
                            {
                                nSectionNumber = Convert.ToInt32(GetSection(line));
                            }
                            else
                            {
                                KeyAndValue = GetKeyAndValue(line);
                                ref SCRN_SPECIAL ScrnSpecial = ref ScrnSpecials[nSectionNumber];
                                switch (KeyAndValue.Key)
                                {
                                    case "Catagory":
                                        ScrnSpecial.Catagory = KeyAndValue.Value;
                                        break;
                                    case "ScreenNumber":
                                        ScrnSpecial.ScreenNumber = KeyAndValue.Value;
                                        break;
                                    case "ScreenName":
                                        ScrnSpecial.ScreenName = KeyAndValue.Value;
                                        break;
                                    case "TRCount":
                                        ScrnSpecial.TRs = new string[Convert.ToInt32(KeyAndValue.Value)];
                                        break;
                                    default:
                                        {
                                            int nPos = KeyAndValue.Key.IndexOf("TR_");
                                            if (nPos != -1)
                                            {
                                                int nIndex = Convert.ToInt32(KeyAndValue.Key.Substring(3, KeyAndValue.Key.Length - 3));
                                                if (ScrnSpecial.TRs != null && nIndex >= 0 && nIndex < ScrnSpecial.TRs.Length)
                                                    ScrnSpecial.TRs[nIndex] = KeyAndValue.Value;
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            if (ScrnSpecials.Length == 0) return null;

            var root = new IconTextItem(3, "화면목록");

            foreach (var scrnData in ScrnSpecials)
            {
                var hitem = new IconTextItem(4, $"[{scrnData.ScreenNumber}] {scrnData.ScreenName}");
                var hCategory = new IconTextItem(6, $"화면분류= {scrnData.Catagory}");
                hitem.AddChild(hCategory);
                if (scrnData.TRs != null)
                {
                    for (int i = 0; i < scrnData.TRs.Length; i++)
                    {
                        var hTRName = new IconTextItem(9, $"TR이름={scrnData.TRs[i]}");
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

        // sub function
        KEY_VALUE GetKeyAndValue(string s)
        {
            KEY_VALUE result;
            int nEndPos = s.IndexOf('=');
            result.Key = s.Substring(0, nEndPos);
            result.Value = s.Substring(nEndPos + 1, s.Length - nEndPos - 1);
            return result;
        };

        string GetSection(string s)
        {
            int nEndPos = s.IndexOf(']');
            return s.Substring(1, nEndPos - 1);
        };
    }

    private void Load_종목정보()
    {
        if (_Data_종목정보 != null) return;

        if (axOpenAPI == null || axOpenAPI.GetConnectState() == 0) return;

        // 주식종목
        string[] market_codes = { "0", "10", "3", "8", "50", "4", "5", "6", "9", "30" };
        string[] market_names = { "코스피", "코스닥", "ELW", "ETF", "KONEX", "뮤추얼펀드", "신주인수권", "리츠", "하이얼펀드", "K-OTC" };

        var root = new IconTextItem(10, "상장 종목정보");
        for (int i = 0; i < market_codes.Length; i++)
        {
            string[] codes = axOpenAPI.GetCodeListByMarket(market_codes[i]).Split(';', StringSplitOptions.RemoveEmptyEntries);
            var hGroup = new IconTextItem(11, $"{market_names[i]} ({codes.Length})");
            foreach (string code in codes)
            {
                var hItem = new IconTextItem(12, $"[{code}] : {axOpenAPI.GetMasterCodeName(code)}");
                hItem.AddChild(new IconTextItem(13, string.Format("전일가 : {0:n0} 원", Convert.ToInt32(axOpenAPI.GetMasterLastPrice(code)))));
                hItem.AddChild(new IconTextItem(13, $"상장일 : {axOpenAPI.GetMasterListedStockDate(code)}"));
                hItem.AddChild(new IconTextItem(13, string.Format("상장주식수 : {0:n0}", Convert.ToInt32(axOpenAPI.GetMasterListedStockCnt(code)))));
                hItem.AddChild(new IconTextItem(13, $"감리구분 : {axOpenAPI.GetMasterStockState(code)}"));
                hGroup.AddChild(hItem);
            }
            root.AddChild(hGroup);
        }

        root.IsExpanded = true;
        _Data_종목정보 = root;

        SetTreeItems((int)TREETAB_KIND.종목정보, new List<object>() { root });
    }

    private void Load_사용자기능()
    {
        if (_Data_사용자정보 != null && _Data_조건검색 != null) return;

        if (axOpenAPI == null || axOpenAPI.GetConnectState() == 0) return;

        // 사용자기능
        var rootInfo = new IconTextItem(10, "로그인정보");
        rootInfo.AddChild(new IconTextItem(13, "사용자정보"));
        rootInfo.IsExpanded = true;

        var rootCond = new IconTextItem(11, "조건검색");
        foreach (var item in MapCondNameToIndex)
        {
            rootCond.AddChild(new IconTextItem(12, item.Key));
        }

        rootCond.Text = rootCond.Text + $" ({rootCond.Items.Count})";
        rootCond.IsExpanded = true;
        _Data_사용자정보 = rootInfo;
        _Data_조건검색 = rootCond;

        SetTreeItems((int)TREETAB_KIND.사용자기능, new List<object>() { rootInfo, rootCond });
    }
}
