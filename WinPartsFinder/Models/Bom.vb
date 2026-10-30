Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>One line of a BOM: a supplier's part and how many to buy. Prices are the supplier's own, as added.</summary>
Public Class BomLine
    ''' <summary>Ticked: counted in the totals. Unticked lines are saved as DNP.</summary>
    Public Property Include As Boolean = True
    ''' <summary>Component references, e.g. "R1,R2" - only filled in by hand or by a loaded file.</summary>
    Public Property Refs As String = ""
    ''' <summary>Package; saved in the Footprint column.</summary>
    Public Property Footprint As String = ""
    Public Property Qty As Integer = 1
    Public Property Supplier As String = ""
    Public Property SupplierPartNumber As String = ""
    Public Property ManufacturerPartNumber As String = ""
    Public Property Manufacturer As String = ""
    Public Property Description As String = ""
    ''' <summary>Unit price at the lowest price break, in <see cref="Currency"/>.</summary>
    Public Property UnitPrice As Decimal?
    Public Property Currency As String = ""
    Public Property PriceBreaks As New List(Of PriceBreak)
    Public Property ProductUrl As String = ""
    ''' <summary>Stock when the part was added.</summary>
    Public Property Stock As Long?
    Public Property DateAdded As DateTime?

    ''' <summary>Unit price when buying <see cref="Qty"/>: the highest price break at or below it.</summary>
    Public Function PriceEach() As Decimal?
        If PriceBreaks.Count = 0 Then Return UnitPrice
        Dim ordered = PriceBreaks.OrderBy(Function(b) b.Quantity).ToList()
        Dim best = ordered(0)
        For Each b In ordered
            If b.Quantity <= Qty Then best = b
        Next
        Return best.Price
    End Function

    ''' <summary>True when this is the same supplier part as <paramref name="other"/> (so adding it again adds to Qty).</summary>
    Public Function SamePart(other As BomLine) As Boolean
        Return SupplierPartNumber.Length > 0 AndAlso
               String.Equals(Supplier, other.Supplier, StringComparison.OrdinalIgnoreCase) AndAlso
               String.Equals(SupplierPartNumber, other.SupplierPartNumber, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>"1:0.2;10:0.139" - the price breaks as numbers.</summary>
    Public Function BreaksRaw() As String
        Return String.Join(";", PriceBreaks.OrderBy(Function(b) b.Quantity).
                           Select(Function(b) b.Quantity.ToString(CultureInfo.InvariantCulture) & ":" & b.Price.ToString(CultureInfo.InvariantCulture)))
    End Function

    ''' <summary>Reads "1:0.2;10:0.139"; anything that isn't a number pair is skipped.</summary>
    Public Shared Function ParseBreaks(raw As String) As List(Of PriceBreak)
        Dim list As New List(Of PriceBreak)
        For Each pair In If(raw, "").Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
            Dim bits = pair.Split(":"c)
            Dim q As Integer, p As Decimal
            If bits.Length = 2 AndAlso Integer.TryParse(bits(0).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, q) AndAlso
               Decimal.TryParse(bits(1).Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, p) Then
                list.Add(New PriceBreak(q, p))
            End If
        Next
        Return list
    End Function
End Class

''' <summary>
''' A bill of materials built up across searches, saved as CSV. The file starts with the usual BOM columns
''' (Refs, Value, Footprint, Qty, DNP) followed by the supplier columns, so other BOM CSVs with those columns load too.
''' The BOM's name and notes go after the parts, below a blank line, as "#BOM Name" and "#Notes" rows.
''' </summary>
Public Class Bom
    Public Property Name As String = ""
    Public Property Notes As String = ""
    Public Property Lines As New List(Of BomLine)

    Public Const UntitledName As String = "Untitled BOM"

    ''' <summary>The name, or "Untitled BOM" when it hasn't been named.</summary>
    Public ReadOnly Property DisplayName As String
        Get
            Return If(Name.Trim().Length > 0, Name.Trim(), UntitledName)
        End Get
    End Property

    Private Const NameTag As String = "#BOM Name"
    Private Const NotesTag As String = "#Notes"

    Private Shared ReadOnly Headers As String() = {
        "Refs", "Value", "Footprint", "Qty", "DNP", "Supplier", "Supplier Part No", "MPN", "Manufacturer", "Description",
        "Unit Price", "Currency", "Price Breaks", "Product URL", "Stock", "Date Added"}

    ''' <summary>
    ''' Adds a line, keeping each supplier's lines together. If the same supplier part is already there,
    ''' its quantity goes up instead. Returns the line that holds the part.
    ''' </summary>
    Public Function Add(line As BomLine) As BomLine
        Dim existing = Lines.FirstOrDefault(Function(l) l.SamePart(line))
        If existing IsNot Nothing Then
            existing.Qty += line.Qty
            Return existing
        End If
        Dim lastOfSupplier = Lines.FindLastIndex(Function(l) String.Equals(l.Supplier, line.Supplier, StringComparison.OrdinalIgnoreCase))
        Lines.Insert(If(lastOfSupplier >= 0, lastOfSupplier + 1, Lines.Count), line)
        Return line
    End Function

    Public Sub Save(path As String)
        Dim inv = CultureInfo.InvariantCulture
        Dim sb As New StringBuilder()
        sb.AppendLine(String.Join(",", Headers.Select(AddressOf Quote)))
        For Each l In Lines
            sb.AppendLine(String.Join(",", {
                l.Refs, l.ManufacturerPartNumber, l.Footprint, l.Qty.ToString(inv), If(l.Include, "", "DNP"),
                l.Supplier, l.SupplierPartNumber, l.ManufacturerPartNumber, l.Manufacturer, l.Description,
                If(l.UnitPrice.HasValue, l.UnitPrice.Value.ToString(inv), ""), l.Currency, l.BreaksRaw(), l.ProductUrl,
                If(l.Stock.HasValue, l.Stock.Value.ToString(inv), ""),
                If(l.DateAdded.HasValue, l.DateAdded.Value.ToString("yyyy-MM-dd", inv), "")}.Select(AddressOf Quote)))
        Next
        sb.AppendLine()
        sb.AppendLine(Quote(NameTag) & "," & Quote(Name))
        sb.AppendLine(Quote(NotesTag) & "," & Quote(Notes))
        Directory.CreateDirectory(IO.Path.GetDirectoryName(path))
        File.WriteAllText(path, sb.ToString(), New UTF8Encoding(True)) ' BOM so Excel shows £ properly
    End Sub

    ''' <summary>Reads a BOM saved by WinPartsFinder, or another BOM CSV (any columns it doesn't have stay blank).</summary>
    Public Shared Function Load(path As String) As Bom
        Dim inv = CultureInfo.InvariantCulture
        Dim records = ReadCsv(File.ReadAllText(path))
        Dim bom As New Bom()
        Dim header = records.FirstOrDefault(Function(r) r.Any(Function(f) f.Trim().Length > 0))
        If header Is Nothing Then Return bom

        Dim cols As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i = 0 To header.Count - 1
            Dim h = header(i).Trim()
            If h.Length > 0 AndAlso Not cols.ContainsKey(h) Then cols(h) = i
        Next


        For Each r In records.Skip(records.IndexOf(header) + 1)
            If r.All(Function(f) f.Trim().Length = 0) Then Continue For
            Dim first = r(0).Trim()
            If first.Equals(NameTag, StringComparison.OrdinalIgnoreCase) Then
                bom.Name = If(r.Count > 1, r(1).Trim(), "")
                Continue For
            End If
            If first.Equals(NotesTag, StringComparison.OrdinalIgnoreCase) Then
                bom.Notes = If(r.Count > 1, r(1), "")
                Continue For
            End If

            Dim l As New BomLine With {
                .Refs = Cell(r, cols, "Refs", "Reference", "References", "Designator"),
                .Footprint = Cell(r, cols, "Footprint", "Package"),
                .Include = Cell(r, cols, "DNP", "Do not populate").Length = 0,
                .Supplier = Cell(r, cols, "Supplier"),
                .SupplierPartNumber = Cell(r, cols, "Supplier Part No"),
                .ManufacturerPartNumber = Cell(r, cols, "MPN", "Manufacturer Part No", "Value"),
                .Manufacturer = Cell(r, cols, "Manufacturer"),
                .Description = Cell(r, cols, "Description"),
                .Currency = Cell(r, cols, "Currency"),
                .PriceBreaks = BomLine.ParseBreaks(Cell(r, cols, "Price Breaks")),
                .ProductUrl = Cell(r, cols, "Product URL")}
            Dim q As Integer
            If Integer.TryParse(Cell(r, cols, "Qty", "Quantity", "Quantity Per PCB"), NumberStyles.Integer, inv, q) AndAlso q > 0 Then l.Qty = q
            Dim price As Decimal
            If Decimal.TryParse(Cell(r, cols, "Unit Price"), NumberStyles.Number, inv, price) Then l.UnitPrice = price
            Dim stock As Long
            If Long.TryParse(Cell(r, cols, "Stock"), NumberStyles.Integer, inv, stock) Then l.Stock = stock
            Dim added As DateTime
            If DateTime.TryParseExact(Cell(r, cols, "Date Added"), "yyyy-MM-dd", inv, DateTimeStyles.None, added) Then l.DateAdded = added
            bom.Lines.Add(l)
        Next
        Return bom
    End Function

    ''' <summary>The first of <paramref name="names"/> that is a column of the file (other common column names are accepted too); "" if none.</summary>
    Private Shared Function Cell(r As List(Of String), cols As Dictionary(Of String, Integer), ParamArray names As String()) As String
        For Each n In names
            Dim i As Integer
            If cols.TryGetValue(n, i) AndAlso i < r.Count Then Return r(i).Trim()
        Next
        Return ""
    End Function

    Private Shared Function Quote(s As String) As String
        Return """" & If(s, "").Replace("""", """""") & """"
    End Function

    ''' <summary>Splits CSV text into records of fields. Quoted fields may hold commas, quotes ("") and line breaks.</summary>
    Private Shared Function ReadCsv(text As String) As List(Of List(Of String))
        Dim records As New List(Of List(Of String))
        Dim record As New List(Of String)
        Dim field As New StringBuilder()
        Dim inQuotes = False
        Dim i = 0
        If text.Length > 0 AndAlso text(0) = ChrW(&HFEFF) Then i = 1
        While i < text.Length
            Dim c = text(i)
            If inQuotes Then
                If c = """"c Then
                    If i + 1 < text.Length AndAlso text(i + 1) = """"c Then
                        field.Append(""""c)
                        i += 1
                    Else
                        inQuotes = False
                    End If
                Else
                    field.Append(c)
                End If
            ElseIf c = """"c Then
                inQuotes = True
            ElseIf c = ","c Then
                record.Add(field.ToString())
                field.Clear()
            ElseIf c = ControlChars.Cr OrElse c = ControlChars.Lf Then
                If c = ControlChars.Cr AndAlso i + 1 < text.Length AndAlso text(i + 1) = ControlChars.Lf Then i += 1
                record.Add(field.ToString())
                field.Clear()
                records.Add(record)
                record = New List(Of String)
            Else
                field.Append(c)
            End If
            i += 1
        End While
        If field.Length > 0 OrElse record.Count > 0 Then
            record.Add(field.ToString())
            records.Add(record)
        End If
        Return records
    End Function
End Class
