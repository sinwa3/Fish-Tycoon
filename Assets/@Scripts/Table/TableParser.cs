using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary> KDH : CSV 헤더와 데이터클래스 필드를 매핑하기 위한 커스텀 애트리뷰트 (CSV 헤더 순서와 필드 순서가 일치하지 않아도 매핑할 수 있도록 한다.) </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class CsvFieldAttribute : Attribute
{
    public string HeaderName { get; }

    public CsvFieldAttribute(string headerName)
    {
        HeaderName = headerName;
    }
}

/// <summary> KDH : CSV 헤더와 데이터클래스 필드를 매핑하여 CSV 데이터 객체로 파싱한다. </summary>
public class TableParser<TData> where TData : class, new()
{
    // 쉼표를 찾되, CSV의 "..." 안에 있는 쉼표는 무시한다.
    private const string SplitRegex = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

    private readonly Dictionary<string, FieldInfo> _fieldMap = new();

    #region ===== 초기화 =====

    public TableParser()
    {
        CacheFieldMap();
    }

    private void CacheFieldMap()
    {
        FieldInfo[] fields = typeof(TData).GetFields();

        foreach (FieldInfo field in fields)
        {
            CsvFieldAttribute attribute = field.GetCustomAttribute<CsvFieldAttribute>();

            if (attribute == null)
            {
                continue;
            }

            _fieldMap[attribute.HeaderName] = field;
        }
    }

    #endregion ===== 초기화 =====

    /// <summary> KDH : 매개변수로 전달된 한 줄의 데이터를 필드맵을 기준으로 파싱한다. </summary>
    public TData ParseLine(string headerLine, string dataLine)
    {
        string[] headers = SplitCsvLine(headerLine);
        string[] values = SplitCsvLine(dataLine);

        if (headers.Length != values.Length)
        {
            CPrint.Error($"[TableParser] : 헤더({headers.Length}) <{headerLine}>와 데이터({values.Length})의 개수가 다르다.");

            return null;
        }

        TData data = new TData();

        for (int i = 0; i < headers.Length; i++)
        {
            string header = NormalizeValue(headers[i]);
            
            if (string.IsNullOrEmpty(header))
            {
                continue;
            }

            if (!_fieldMap.TryGetValue(header, out FieldInfo field))
            {
                CPrint.Error($"[TableParser] : CSV 헤더 <{header}>에 대응하는 <{typeof(TData).Name}>의 필드를 찾을 수 없다.");
                continue;
            }

            string value = i < values.Length ? NormalizeValue(values[i]) : string.Empty;
            
            try
            {
                object convertedValue = ConvertValue(field.FieldType, value);

                field.SetValue(data, convertedValue);
            }
            catch (Exception exception)
            {
                CPrint.Error($"[TableParser] : <{header}> 값을 <{field.FieldType.Name}> " +
                             $"타입으로 변환하지 못했다. Value: <{value}> / {exception.Message}");
            }
        }

        return data;
    }

    /// <summary> KDH : 한 줄의 데이터를 쉼표 기준으로 나눈다. 단, 큰따옴표 내부의 쉼표는 구분자로 처리하지 않는다. </summary>
    private string[] SplitCsvLine(string line)
    {
        return System.Text.RegularExpressions.Regex.Split(line, SplitRegex);
    }

    /// <summary> KDH : 쉼표 기준으로 나눠진 문자열을 정규화한다. </summary>
    private string NormalizeValue(string value)
    {
        // br : breakLine
        // c : ,
        return value.Trim('"').Replace("<br>", "\n").Replace("<c>", ",");
    }

    /// <summary> KDH : 문자열 값을 해당하는 타입으로 반환한다. </summary>
    private object ConvertValue(Type fieldType, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            CPrint.Error($"[TableParse] 테이블에 비어있는 값이 존재합니다.");
            return null;
        }
        
        if (fieldType == typeof(string))
        {
            return value;
        }

        if (fieldType == typeof(int))
        {
            return int.Parse(value);
        }

        if (fieldType == typeof(float))
        {
            return float.Parse(value);
        }

        if (fieldType == typeof(bool))
        {
            return bool.Parse(value);
        }
        
        throw new NotSupportedException($"지원하지 않는 필드 타입 : {fieldType}");
    }
}