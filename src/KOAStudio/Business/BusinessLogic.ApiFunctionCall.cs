using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KOAStudio.Business;

internal sealed partial class BusinessLogic
{
    /// <summary>
    /// OpenApi 함수 실행
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="funcName"></param>
    /// <param name="Params"></param>
    /// <returns></returns>
    private static object? CallInstanceFunc(object instance, string funcName, VariantWrapper[] Params)
    {
        if (instance == null) return null;
        object? result = null;

        MethodInfo? theMethod = instance.GetType().GetMethod(funcName);
        if (theMethod != null)
        {
            var inner_parameters = theMethod.GetParameters();
            Debug.Assert(inner_parameters.Length == Params.Length);
            object?[] call_parmas = new object?[inner_parameters.Length];
            if (inner_parameters.Length > 0)
            {
                for (int i = 0; i < inner_parameters.Length; i++)
                {
                    call_parmas[i] = Convert.ChangeType(Params[i].WrappedObject, inner_parameters[i].ParameterType);
                }
            }
            result = theMethod.Invoke(instance, call_parmas);
        }
        return result;
    }


    //    /// <summary>
    //    /// OpenApi 함수 실행
    //    /// </summary>
    //    /// <param name="funcName"></param>
    //    /// <param name="Params"></param>
    //    /// <returns></returns>
    //#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable ValueType.
    //#pragma warning disable CS8605 // Unboxing a possibly null value.
    //    private object? CallOcxFunc(string funcName, VariantWrapper[] Params)
    //    {
    //        if (axOpenAPI == null) return null;
    //        switch (funcName)
    //        {
    //            case "CommConnect":
    //                if ((LoginState == OpenApiLoginState.None) || (LoginState == OpenApiLoginState.LoginOuted))
    //                {
    //                    return ReqApiLogin(true);
    //                    //axOpenAPI.CommConnect();
    //                }
    //                return "로그인 요청불가.";
    //            case "CommTerminate":
    //                if (LoginState == OpenApiLoginState.LoginSucceed)
    //                {
    //                    ReqApiLogin(false);
    //                }
    //                //axOpenAPI.CommTerminate();
    //                break;
    //            case "CommRqData":
    //                return axOpenAPI.CommRqData((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (int)Params[2].WrappedObject, (string)Params[3].WrappedObject);
    //            case "GetLoginInfo":
    //                return axOpenAPI.GetLoginInfo((string)Params[0].WrappedObject);
    //            case "SendOrder":
    //                return axOpenAPI.SendOrder((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (string)Params[2].WrappedObject, (int)Params[3].WrappedObject, (string)Params[4].WrappedObject, (int)Params[5].WrappedObject, (int)Params[6].WrappedObject, (string)Params[7].WrappedObject, (string)Params[8].WrappedObject);
    //            case "SendOrderFO":
    //                return axOpenAPI.SendOrderFO((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (string)Params[2].WrappedObject, (string)Params[3].WrappedObject, (int)Params[4].WrappedObject, (string)Params[5].WrappedObject, (string)Params[6].WrappedObject, (int)Params[7].WrappedObject, (string)Params[8].WrappedObject, (string)Params[9].WrappedObject);
    //            case "SetInputValue":
    //                axOpenAPI.SetInputValue((string)Params[0].WrappedObject, (string)Params[1].WrappedObject);
    //                break;
    //            case "SetOutputFID":
    //                return axOpenAPI.SetOutputFID((string)Params[0].WrappedObject);
    //            case "CommGetData":
    //                return axOpenAPI.CommGetData((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (string)Params[2].WrappedObject, (int)Params[3].WrappedObject, (string)Params[4].WrappedObject);
    //            case "DisconnectRealData":
    //                axOpenAPI.DisconnectRealData((string)Params[0].WrappedObject);
    //                break;
    //            case "GetRepeatCnt":
    //                return axOpenAPI.GetRepeatCnt((string)Params[0].WrappedObject, (string)Params[1].WrappedObject);
    //            case "CommKwRqData":
    //                return axOpenAPI.CommKwRqData((string)Params[0].WrappedObject, (int)Params[1].WrappedObject, (int)Params[2].WrappedObject, (int)Params[3].WrappedObject, (string)Params[4].WrappedObject, (string)Params[5].WrappedObject);
    //            case "GetAPIModulePath":
    //                return axOpenAPI.GetAPIModulePath();
    //            case "GetCodeListByMarket":
    //                return axOpenAPI.GetCodeListByMarket((string)Params[0].WrappedObject);
    //            case "GetConnectState":
    //                return axOpenAPI.GetConnectState();
    //            case "GetMasterCodeName":
    //                return axOpenAPI.GetMasterCodeName((string)Params[0].WrappedObject);
    //            case "GetMasterListedStockCnt":
    //                return axOpenAPI.GetMasterListedStockCnt((string)Params[0].WrappedObject);
    //            case "GetMasterConstruction":
    //                return axOpenAPI.GetMasterConstruction((string)Params[0].WrappedObject);
    //            case "GetMasterListedStockDate":
    //                return axOpenAPI.GetMasterListedStockDate((string)Params[0].WrappedObject);
    //            case "GetMasterLastPrice":
    //                return axOpenAPI.GetMasterLastPrice((string)Params[0].WrappedObject);
    //            case "GetMasterStockState":
    //                return axOpenAPI.GetMasterStockState((string)Params[0].WrappedObject);
    //            case "GetDataCount":
    //                return axOpenAPI.GetDataCount((string)Params[0].WrappedObject);
    //            case "GetOutputValue":
    //                return axOpenAPI.GetOutputValue((string)Params[0].WrappedObject, (int)Params[1].WrappedObject, (int)Params[2].WrappedObject);
    //            case "GetCommData":
    //                return axOpenAPI.GetCommData((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (int)Params[2].WrappedObject, (string)Params[3].WrappedObject);
    //            case "GetCommRealData":
    //                return axOpenAPI.GetCommRealData((string)Params[0].WrappedObject, (int)Params[1].WrappedObject);
    //            case "GetChejanData":
    //                return axOpenAPI.GetChejanData((int)Params[0].WrappedObject);
    //            case "GetThemeGroupList":
    //                return axOpenAPI.GetThemeGroupList((int)Params[0].WrappedObject);
    //            case "GetThemeGroupCode":
    //                return axOpenAPI.GetThemeGroupCode((string)Params[0].WrappedObject);
    //            case "GetFutureList":
    //                return axOpenAPI.GetFutureList();
    //            case "GetFutureCodeByIndex":
    //                return axOpenAPI.GetFutureCodeByIndex((int)Params[0].WrappedObject);
    //            case "GetActPriceList":
    //                return axOpenAPI.GetActPriceList();
    //            case "GetMonthList":
    //                return axOpenAPI.GetMonthList();
    //            case "GetOptionCode":
    //                return axOpenAPI.GetOptionCode((string)Params[0].WrappedObject, (int)Params[1].WrappedObject, (string)Params[2].WrappedObject);
    //            case "GetOptionCodeByMonth":
    //                return axOpenAPI.GetOptionCodeByMonth((string)Params[0].WrappedObject, (int)Params[1].WrappedObject, (string)Params[2].WrappedObject);
    //            case "GetOptionCodeByActPrice":
    //                return axOpenAPI.GetOptionCodeByActPrice((string)Params[0].WrappedObject, (int)Params[1].WrappedObject, (int)Params[2].WrappedObject);
    //            case "GetSFutureList":
    //                return axOpenAPI.GetSFutureList((string)Params[0].WrappedObject);
    //            case "GetSFutureCodeByIndex":
    //                return axOpenAPI.GetSFutureCodeByIndex((string)Params[0].WrappedObject, (int)Params[1].WrappedObject);
    //            case "GetSActPriceList":
    //                return axOpenAPI.GetSActPriceList((string)Params[0].WrappedObject);
    //            case "GetSMonthList":
    //                return axOpenAPI.GetSMonthList((string)Params[0].WrappedObject);
    //            case "GetSOptionCode":
    //                return axOpenAPI.GetSOptionCode((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (int)Params[2].WrappedObject, (string)Params[3].WrappedObject);
    //            case "GetSOptionCodeByMonth":
    //                return axOpenAPI.GetSOptionCodeByMonth((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (int)Params[2].WrappedObject, (string)Params[3].WrappedObject);
    //            case "GetSOptionCodeByActPrice":
    //                return axOpenAPI.GetSOptionCodeByActPrice((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (int)Params[2].WrappedObject, (int)Params[3].WrappedObject);
    //            case "GetSFOBasisAssetList":
    //                return axOpenAPI.GetSFOBasisAssetList();
    //            case "GetOptionATM":
    //                return axOpenAPI.GetOptionATM();
    //            case "GetSOptionATM":
    //                return axOpenAPI.GetSOptionATM((string)Params[0].WrappedObject);
    //            case "GetBranchCodeName":
    //                return axOpenAPI.GetBranchCodeName();
    //            case "CommInvestRqData":
    //                return axOpenAPI.CommInvestRqData((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (string)Params[2].WrappedObject);
    //            case "SendOrderCredit":
    //                return axOpenAPI.SendOrderCredit((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (string)Params[2].WrappedObject, (int)Params[3].WrappedObject, (string)Params[4].WrappedObject, (int)Params[5].WrappedObject, (int)Params[6].WrappedObject, (string)Params[7].WrappedObject, (string)Params[8].WrappedObject, (string)Params[9].WrappedObject, (string)Params[10].WrappedObject);
    //            case "KOA_Functions":
    //                return axOpenAPI.KOA_Functions((string)Params[0].WrappedObject, (string)Params[1].WrappedObject);
    //            case "SetInfoData":
    //                return axOpenAPI.SetInfoData((string)Params[0].WrappedObject);
    //            case "SetRealReg":
    //                return axOpenAPI.SetRealReg((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (string)Params[2].WrappedObject, (string)Params[3].WrappedObject);
    //            case "GetConditionLoad":
    //                return axOpenAPI.GetConditionLoad();
    //            case "GetConditionNameList":
    //                return axOpenAPI.GetConditionNameList();
    //            case "SendCondition":
    //                return axOpenAPI.SendCondition((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (int)Params[2].WrappedObject, (int)Params[3].WrappedObject);
    //            case "SendConditionStop":
    //                axOpenAPI.SendConditionStop((string)Params[0].WrappedObject, (string)Params[1].WrappedObject, (int)Params[2].WrappedObject);
    //                break;
    //            case "GetCommDataEx":
    //                return axOpenAPI.GetCommDataEx((string)Params[0].WrappedObject, (string)Params[1].WrappedObject);
    //            case "SetRealRemove":
    //                axOpenAPI.SetRealRemove((string)Params[0].WrappedObject, (string)Params[1].WrappedObject);
    //                break;
    //            case "GetMarketType":
    //                return axOpenAPI.GetMarketType((string)Params[0].WrappedObject);
    //            default:
    //                break;
    //        }
    //        return null;
    //    }
    //#pragma warning restore CS8605 // Unboxing a possibly null value.
    //#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable ValueType.

}
