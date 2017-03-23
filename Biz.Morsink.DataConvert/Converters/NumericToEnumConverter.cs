using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
using static Biz.Morsink.DataConvert.DataConvertUtils;
namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This converter converts numeric values to enum values. 
    /// Performs a simple min/max bounds check for regular enums, or a bitmask check for enums attributed with the FlagsAttribute
    /// </summary>
    public class NumericToEnumConverter : IConverter, IDataConverterRef
    {
        private static DataConverter helper = new DataConverter(new[] { new SimpleNumericConverter() });

        public IDataConverter Ref { get; set; }

        public bool CanConvert(Type from, Type to)
            => (from == typeof(sbyte) || from == typeof(short) || from == typeof(int) || from == typeof(long)
                || from == typeof(byte) || from == typeof(ushort) || from == typeof(uint) || from == typeof(ulong))
            && to.GetTypeInfo().IsEnum;
        
        public Delegate Create(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var ut = Enum.GetUnderlyingType(to);
            if (from == ut)
            {
                if (to.GetTypeInfo().GetCustomAttributes<FlagsAttribute>().Any())
                {
                    var vals = Enum.GetValues(to).OfType<object>().Select(x => Convert.ToInt64(x));
                    var mask = vals.Aggregate(0L, (x, y) => x | y);
                    var block = Ex.Condition(
                        Ex.MakeBinary(Et.Equal,
                            Ex.MakeBinary(Et.And, input, Ex.Constant(helper.DoGeneralConversion(mask, ut).Result, ut)),
                            input),
                        Result(to, Ex.Convert(input, to)),
                        NoResult(to));
                    var lambda = Ex.Lambda(block, input);
                    return lambda.Compile();
                }
                else
                {
                    var vals = Enum.GetValues(to).OfType<object>().Select(x => Convert.ToInt64(x));
                    var min = vals.Min();
                    var max = vals.Max();
                    var block = Ex.Condition(
                        Ex.MakeBinary(Et.AndAlso,
                            Ex.MakeBinary(Et.GreaterThanOrEqual, input, Ex.Constant(helper.DoGeneralConversion(min, ut).Result, ut)),
                            Ex.MakeBinary(Et.LessThanOrEqual, input, Ex.Constant(helper.DoGeneralConversion(max, ut).Result, ut))),
                        Result(to, Ex.Convert(input, to)),
                        NoResult(to));
                    var lambda = Ex.Lambda(block, input);
                    return lambda.Compile();
                }
            }
            else
            {
                var res = Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(ut), "res");
                var block = Ex.Block(new[] { res },
                    Ex.Assign(res,
                        Ex.Call(typeof(DataConverterExt).GetTypeInfo().GetDeclaredMethod(nameof(DataConverterExt.DoConversion)).MakeGenericMethod(from, ut),
                            Ex.Constant(Ref),
                            input)),
                    Ex.Condition(Ex.Property(res, nameof(IConversionResult.IsSuccessful)),
                        Ex.Call(typeof(DataConverterExt).GetTypeInfo().GetDeclaredMethod(nameof(DataConverterExt.DoConversion)).MakeGenericMethod(ut, to),
                            Ex.Constant(Ref),
                            Ex.Property(res, nameof(IConversionResult.Result))),
                        NoResult(to)));
                var lambda = Ex.Lambda(block, input);
                return lambda.Compile();
            }
        }
    }
}
