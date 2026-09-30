using System.Linq;
using System.Reflection;

namespace BOTWM.Server.HelperTypes;

public static class AutoMapperExtensions
{
	public static void Map<T>(this T Dst, object Src)
	{
		FieldInfo[] fields = typeof(T).GetFields();
		foreach (FieldInfo field in fields)
		{
			if (Src.GetType().GetFields().Any((FieldInfo fld) => fld.Name == field.Name && fld.FieldType == field.FieldType))
			{
				FieldInfo fieldInfo = (from fld in Src.GetType().GetFields()
					where fld.Name == field.Name && fld.FieldType == field.FieldType
					select fld).First();
				field.SetValue(Dst, fieldInfo.GetValue(Src));
			}
		}
	}
}
