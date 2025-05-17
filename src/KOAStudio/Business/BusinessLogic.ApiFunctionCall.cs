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
}
