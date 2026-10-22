Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices

''' <summary>
''' Forgiving, case-insensitive accessors for JSON (Newtonsoft JToken) so supplier parsers
''' don't blow up when a field is missing or null. All of them accept Nothing.
''' </summary>
Public Module JsonHelpers

    ''' <summary>
    ''' Parses JSON text. Dates are left as plain text and numbers read as Decimal,
    ''' so prices like 0.083 come through exactly.
    ''' </summary>
    Public Function ParseJson(text As String) As JToken
        Using reader As New JsonTextReader(New StringReader(text)) With {
            .DateParseHandling = DateParseHandling.None,
            .FloatParseHandling = FloatParseHandling.Decimal
        }
            Return JToken.ReadFrom(reader)
        End Using
    End Function

    <Extension>
    Public Function GetPath(e As JToken, ParamArray path() As String) As JToken
        Dim cur = e
        For Each name In path
            Dim obj = TryCast(cur, JObject)
            If obj Is Nothing Then Return Nothing
            Dim prop = obj.Property(name, StringComparison.OrdinalIgnoreCase)
            If prop Is Nothing Then Return Nothing
            cur = prop.Value
        Next
        Return cur
    End Function

    <Extension>
    Public Function GetStr(e As JToken, ParamArray path() As String) As String
        Dim v = e.GetPath(path)
        If v Is Nothing Then Return ""
        Select Case v.Type
            Case JTokenType.String
                Return If(CStr(DirectCast(v, JValue).Value), "").Trim()
            Case JTokenType.Integer, JTokenType.Float, JTokenType.Boolean
                Return v.ToString(Formatting.None) ' as written in the JSON, e.g. 123, 0.5, true
            Case Else
                Return ""
        End Select
    End Function

    <Extension>
    Public Function GetNum(e As JToken, ParamArray path() As String) As Decimal?
        Dim v = e.GetPath(path)
        If v Is Nothing Then Return Nothing
        Select Case v.Type
            Case JTokenType.Integer, JTokenType.Float
                Try
                    Return Convert.ToDecimal(DirectCast(v, JValue).Value, CultureInfo.InvariantCulture)
                Catch ex As OverflowException
                    Return Nothing
                End Try
            Case JTokenType.String
                Return ParseHelpers.ParseDecimalLoose(CStr(DirectCast(v, JValue).Value))
            Case Else
                Return Nothing
        End Select
    End Function

    <Extension>
    Public Function GetArr(e As JToken, ParamArray path() As String) As List(Of JToken)
        Dim arr = TryCast(e.GetPath(path), JArray)
        If arr Is Nothing Then Return New List(Of JToken)
        Return arr.ToList()
    End Function

End Module
