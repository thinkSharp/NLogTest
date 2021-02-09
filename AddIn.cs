using System;
using ExcelDna.Integration;
using ExcelDna.Registration;
using ExcelDna.IntelliSense;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;

namespace NLogTest
{
    public class AddIn : IExcelAddIn
    {
        public void AutoOpen()
        {
            RegisterFunctions();

            IntelliSenseServer.Install();
        }


        public void AutoClose()
        {

            IntelliSenseServer.Uninstall();
        }

        void RegisterFunctions()
        {
            var postAsyncReturnConfig = GetPostAsyncReturnConversionConfig();
            var conversionConfig = GetParameterConversionConfig();

            ExcelRegistration.GetExcelFunctions()
                             .ProcessMapArrayFunctions(conversionConfig)
                             .ProcessParameterConversions(conversionConfig)
                             .ProcessAsyncRegistrations(nativeAsyncIfAvailable: false)
                             .ProcessParameterConversions(postAsyncReturnConfig)
                             .ProcessParamsRegistrations()
                             .RegisterFunctions();
        }

        static ParameterConversionConfiguration GetPostAsyncReturnConversionConfig()
        {
            // This conversion replaces the default #N/A return value of async functions with the #GETTING_DATA value.
            // This is not supported on old Excel versions, bu looks nicer these days.
            // Note that this ReturnConversion does not actually check whether the functions is an async function, 
            // so all registered functions are affected by this processing.
            return new ParameterConversionConfiguration()
                .AddReturnConversion((type, customAttributes) => type != typeof(object) ? null : ((Expression<Func<object, object>>)
                                                ((object returnValue) => returnValue.Equals(ExcelError.ExcelErrorNA) ? ExcelError.ExcelErrorGettingData : returnValue)));
        }

        static ParameterConversionConfiguration GetParameterConversionConfig()
        {
            // NOTE: The parameter conversion list is processed once per parameter.
            //       Parameter conversions will apply from most inside, to most outside.
            //       So to apply a conversion chain like
            //           string -> Type1 -> Type2
            //       we need to register in the (reverse) order
            //           Type1 -> Type2
            //           string -> Type1
            //
            //       (If the registration were in the order
            //           string -> Type1
            //           Type1 -> Type2
            //       the parameter (starting as Type2) would not match the first conversion,
            //       then the second conversion (Type1 -> Type2) would be applied, and no more,
            //       leaving the parameter having Type1 (and probably not eligible for Excel registration.)
            //      
            //
            //       Return conversions are also applied from most inside to most outside.
            //       So to apply a return conversion chain like
            //           Type1 -> Type2 -> string
            //       we need to register the ReturnConversions as
            //           Type1 -> Type2 
            //           Type2 -> string
            //       

            var paramConversionConfig = new ParameterConversionConfiguration()

                // Register the Standard Parameter Conversions (with the optional switch on how to treat references to empty cells)
                .AddParameterConversion(ParameterConversions.GetOptionalConversion(treatEmptyAsMissing: true))

                .AddReturnConversion((Complex value) => new double[2] { value.Real, value.Imaginary })

                //  .AddParameterConversion((string value) => convert2(convert1(value)));

                // This parameter conversion adds support for string[] parameters (by accepting object[] instead).
                // It uses the TypeConversion utility class defined in ExcelDna.Registration to get an object->string
                // conversion that is consist with Excel (in this case, Excel is called to do the conversion).
                .AddParameterConversion((object[] inputs) => inputs.Select(TypeConversion.ConvertToString).ToArray())

                // This is a pair of very generic conversions for Enum types
                .AddReturnConversion((Enum value) => value.ToString(), handleSubTypes: true)
                .AddParameterConversion(ParameterConversions.GetEnumStringConversion())

                .AddParameterConversion((object[] input) => new Complex(TypeConversion.ConvertToDouble(input[0]), TypeConversion.ConvertToDouble(input[1])))
                .AddNullableConversion(treatEmptyAsMissing: true, treatNAErrorAsMissing: true);

            return paramConversionConfig;
        }
    }
}
