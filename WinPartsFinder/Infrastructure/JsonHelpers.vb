Imports System.Runtime.CompilerServices

''' <summary>
''' Forgiving, case-insensitive accessors for JsonElement so supplier parsers
''' don't blow up when a field is missing or null.
''' </summary>
Public Module JsonHelpers

    <Extension>
    Public Function GetPath(e As JsonElement, ParamArray path() As String) As JsonElement
        Dim cur = e
        For Each name In path
            If cur.ValueKind <> JsonValueKind.Object Then Return Nothing
            Dim found As JsonElement = Nothing
            Dim ok = False
            For Each p In cur.EnumerateObject()
                If String.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) Then
                    found = p.Value
                    ok = True
                    Exit For
                End If
            Next
            If Not ok Then Return Nothing
            cur = found
        Next
        Return cur
    End Function

    <Extension>
    Public Function GetStr(e As JsonElement, ParamArray path() As String) As String
        Dim v = e.GetPath(path)
        Select Case v.ValueKind
            Case JsonValueKind.String
                Return If(v.GetString(), "").Trim()
            Case JsonValueKind.Number, JsonValueKind.True, JsonValueKind.False
                Return v.GetRawText()
            Case Else
                Return ""
        End Select
    End Function

    <Extension>
    Public Function GetNum(e As JsonElement, ParamArray path() As String) As Decimal?
        Dim v = e.GetPath(path)
        Select Case v.ValueKind
            Case JsonValueKind.Number
                Dim d As Decimal
                If v.TryGetDecimal(d) Then Return d
                Return CDec(v.GetDouble())
            Case JsonValueKind.String
                Return ParseHelpers.ParseDecimalLoose(v.GetString())
            Case Else
                Return Nothing
        End Select
    End Function

    <Extension>
    Public Function GetArr(e As JsonElement, ParamArray path() As String) As List(Of JsonElement)
        Dim v = e.GetPath(path)
        If v.ValueKind = JsonValueKind.Array Then Return v.EnumerateArray().ToList()
        Return New List(Of JsonElement)
    End Function

End Module
