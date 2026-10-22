Imports System.Globalization
Imports System.Text

Public Module ParseHelpers

    ''' <summary>Parses prices like "£1.23", "1,234.50", "0,52 €". Returns Nothing if no number found.</summary>
    Public Function ParseDecimalLoose(s As String) As Decimal?
        If String.IsNullOrWhiteSpace(s) Then Return Nothing
        Dim sb As New StringBuilder()
        For Each c In s
            If Char.IsDigit(c) OrElse c = "."c OrElse c = ","c Then sb.Append(c)
        Next
        Dim t = sb.ToString().Trim("."c, ","c)
        If t.Length = 0 Then Return Nothing

        If t.Contains("."c) AndAlso t.Contains(","c) Then
            ' Whichever separator comes last is the decimal point.
            If t.LastIndexOf(","c) > t.LastIndexOf("."c) Then
                t = t.Replace(".", "").Replace(",", ".")
            Else
                t = t.Replace(",", "")
            End If
        ElseIf t.Contains(","c) Then
            Dim idx = t.LastIndexOf(","c)
            Dim looksLikeThousands = (t.Length - idx - 1 = 3) AndAlso Not t.StartsWith("0,")
            t = If(looksLikeThousands, t.Replace(",", ""), t.Replace(",", "."))
        End If

        Dim d As Decimal
        If Decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, d) Then Return d
        Return Nothing
    End Function

    ''' <summary>Pulls the first integer out of text like "1,234 In Stock". Returns Nothing if none.</summary>
    Public Function ParseLongLoose(s As String) As Long?
        If String.IsNullOrWhiteSpace(s) Then Return Nothing
        Dim sb As New StringBuilder()
        For Each c In s
            If Char.IsDigit(c) Then
                sb.Append(c)
            ElseIf c = ","c AndAlso sb.Length > 0 Then
                Continue For
            ElseIf sb.Length > 0 Then
                Exit For
            End If
        Next
        Dim n As Long
        If sb.Length > 0 AndAlso Long.TryParse(sb.ToString(), n) Then Return n
        Return Nothing
    End Function

    Public Function Truncate(s As String, maxLen As Integer) As String
        If s Is Nothing Then Return ""
        Return If(s.Length <= maxLen, s, s.Substring(0, maxLen) & "…")
    End Function

    Public Function ToNullableInt(d As Decimal?) As Integer?
        If Not d.HasValue Then Return Nothing
        Return CInt(Math.Min(Integer.MaxValue, Math.Max(0D, d.Value)))
    End Function

    Public Function ToNullableLong(d As Decimal?) As Long?
        If Not d.HasValue Then Return Nothing
        Return CLng(Math.Max(0D, d.Value))
    End Function

End Module
