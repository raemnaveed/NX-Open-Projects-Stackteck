'---------------Developed By Soren (2016-2017)--------------'
'---------------Updated by Raem (2017)----------------------'
'------------Latest Change addes makes txtboxvisname as default material in titleblock-------------------'

Option Strict Off
Imports System
Imports System.Windows.Forms
Imports System.Data.OleDb
Imports System.IO
Imports System.Drawing
Imports System.ComponentModel
Imports System.Collections
Imports NXOpenUI
Imports NXOpen
Imports NXOpen.UF
Imports NXOpen.Utilities
Imports NXOpen.Assemblies

'Do you even generate left?

Module Module1
    Public s As Session = s.GetSession()
    Public theUI As UI = UI.GetUI()
    Public ufs As UFSession = ufs.GetUFSession()
    Public lw As ListingWindow = s.ListingWindow()
    Dim density As Double = Nothing ' Pounds per cubic inch

    Sub Main()
        CreateUsageLog("AssignPartAttrInfo")
        Dim form As AssignDatabaseAttributes
        form = New AssignDatabaseAttributes()
        form.ShowDialog()
    End Sub

    Public Sub CreateUsageLog(ByVal ProgramName As String)
        Dim username As String = System.Environment.UserName
        Dim UseDate As String = Now().Day & "-" & Now().Month & "-" & Now().Year
        Dim UsageLogFolderDir As String = "u:\logs\UG_Prog"

        If System.IO.Directory.Exists(UsageLogFolderDir) = False Then
            System.IO.Directory.CreateDirectory(UsageLogFolderDir)
        End If

        Dim UsageLogFileName As String = UsageLogFolderDir & "\" & ProgramName & ".log"

        If System.IO.File.Exists(UsageLogFileName) = True Then
            Dim objReader As New System.IO.StreamReader(UsageLogFileName)
            Dim TempContent As String = objReader.ReadToEnd
            objReader.Close()
            objReader.Dispose()

            Dim objWriter As New System.IO.StreamWriter(UsageLogFileName)
            objWriter.WriteLine(TempContent)
            objWriter.WriteLine(username & ";" & UseDate)
            objWriter.Close()
            objWriter.Dispose()
        Else
            Dim objWriter As New System.IO.StreamWriter(UsageLogFileName)
            objWriter.WriteLine("username;DD-MM-YY")
            objWriter.WriteLine(username & ";" & UseDate)
            objWriter.Close()
            objWriter.Dispose()
        End If
    End Sub
    Function CheckMDMPart() As Boolean
        Dim myValue As String = Nothing

        ReadAttribute("DB_PART_NO", myValue)
        If myValue.StartsWith("DM_") = True Then
            Return True
            Exit Function
        End If

        Return False
    End Function
    Public Function GetUnloadOption(ByVal dummy As String) As Integer

        'Unloads the image immediately after execution within NX
        GetUnloadOption = NXOpen.Session.LibraryUnloadOption.Immediately
    End Function
    Public Function CountCharacter(ByVal value As String, ByVal ch As Char) As Integer
        Dim cnt As Integer = 0
        For Each c As Char In value
            If c = ch Then cnt += 1
        Next
        Return cnt
    End Function

    Public Sub GetUserInfo(ByRef UserName As String, ByRef UserGroup As String, ByRef Status As Boolean)

        Dim UserID As String
        UserID = Environment.UserName()

        'Define the connectors
        Dim cn As OleDbConnection
        Dim cmd As OleDbCommand
        Dim dr As OleDbDataReader
        Dim oConnect, oQuery As String
        Dim FoundStatus As Boolean = False

        'Define connection string
        Dim FileName As String = "Y:\eng\ENG_ACCESS_DATABASES\UGMisDatabase.mdb"
        If File.Exists(FileName) = False Then
            MessageBox.Show("File " & FileName & " is not found.")
            Status = False
            Exit Sub
        End If

        'oConnect = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & FileName
        oConnect = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & FileName

        'Query String
        oQuery = "SELECT * FROM Title_Blk_User_Info where UserID='" & UserID & "'"

        'Instantiate the connectors
        cn = New OleDbConnection(oConnect)

        Try
            cn.Open()
        Catch ex As Exception
        Finally
            cn = New OleDbConnection(oConnect)
            cn.Open()
        End Try

        cmd = New OleDbCommand(oQuery, cn)
        dr = cmd.ExecuteReader

        While dr.Read()
            UserName = dr("Title Blk Name")
            UserName = UserName.Trim()
            'MessageBox.Show("UserName is: " & UserName)

            UserGroup = dr("Design Group")
            UserGroup = UserGroup.Trim()
            'MessageBox.Show("UserGroup is: " & UserGroup)

            FoundStatus = True
            Status = True
        End While

        dr.Close()
        cn.Close()

        If FoundStatus = False Then
            MessageBox.Show("UserID " & UserID & " is not found from database")
        End If
    End Sub
    Private Sub GetDate(ByRef myDate As String)

        Dim objDate As Date = Date.Now()
        Dim Year As String
        Dim Month As String
        Dim Day As String

        Year = objDate.Year
        Month = objDate.Month
        Day = objDate.Day

        Select Case Month
            Case 1
                Month = "JAN"
            Case 2
                Month = "FEB"
            Case 3
                Month = "MAR"
            Case 4
                Month = "APR"
            Case 5
                Month = "MAY"
            Case 6
                Month = "JUN"
            Case 7
                Month = "JUL"
            Case 8
                Month = "AUG"
            Case 9
                Month = "SEP"
            Case 10
                Month = "OCT"
            Case 11
                Month = "NOV"
            Case 12
                Month = "DEC"
            Case Else
                MsgBox("Error")
        End Select

        myDate = Month & " " & Day & ", " & Year
    End Sub
    Public Sub ReadAttribute(ByVal title As String, ByRef value As String)
        Dim dispPart As Part = s.Parts.Display()
        find_part_attr_by_name(dispPart, title, value)
    End Sub
    Public Sub find_part_attr_by_name(ByVal thePart As Part,
                                      ByVal attrName As String,
                                      ByRef attrVal As String)

        Dim theAttr As Attribute = Nothing
        Dim attr_info() As NXObject.AttributeInformation
        attr_info =
          thePart.GetAttributeTitlesByType(NXObject.AttributeType.String)

        Dim title As String = ""
        Dim value As String = ""
        Dim inx As Integer = 0
        Dim count As Integer = attr_info.Length()

        If attr_info.GetLowerBound(0) < 0 Then
            Return
        End If

        Do Until inx = count
            Dim result As Integer = 0

            title = attr_info(inx).Title.ToString
            result = String.Compare(attrName, title)

            If result = 0 Then
                attrVal = thePart.GetStringAttribute(title)
                Return
            End If
            inx += 1
        Loop

        Return
    End Sub
    Public Function CheckSpecification() As Boolean
        Dim dispPart As Part = s.Parts.Display()

        Dim myJobNum As String
        myJobNum = s.Parts.Work.FullPath()

        If myJobNum.Contains("specification") = True Then
            MessageBox.Show("Please run this program in UG Master Part.", "Not Run In Specification", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return True
        Else
            Return False
        End If
    End Function
    Public Sub SetAttribute(ByVal title As String, ByRef value As String)
        Dim dispPart As Part = s.Parts.Display()
        Try
            dispPart.SetAttribute(title, value)
        Catch ex As Exception
            'MsgBox(ex.ToString)
        End Try
    End Sub

    Public Class AssignDatabaseAttributes
        ' Leave these as global!
        Dim indentCount As Integer = 0
        Dim isIndentedBom As Boolean = False
        Dim fromPartChanged As Boolean = False
        Dim lastObjectChanged As Object = Nothing

        Private Sub chkBoxRawMaterial_CheckedChanged(sender As Object, e As EventArgs) Handles chkBoxRawMaterial.CheckedChanged
            If (chkBoxRawMaterial.Checked) Then
                Me.comboxMaterial.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None
                chkBoxPurchasedPart.Checked = False
                chkBoxSkipMaterialSelection.Checked = False
                MaterialButton.Enabled = True
                comboxMaterial.Sorted = False
                comboxMaterial.Items.Add("- - OTHER - -")
                comboxMaterial.Items.Add("4140")
                comboxMaterial.Items.Add("ALU-PLATE")
                comboxMaterial.Items.Add("ALU-PLATE (AIR)")
                comboxMaterial.Items.Add("B-25")
                comboxMaterial.Items.Add("CRS")
                comboxMaterial.Items.Add("H13")
                comboxMaterial.Items.Add("H13-PRE SUPERIOR")
                comboxMaterial.Items.Add("MIRRAX")
                comboxMaterial.Items.Add("P-20")
                comboxMaterial.Items.Add("RAMAX (ROUND)")
                comboxMaterial.Items.Add("SS420")
                comboxMaterial.Items.Add("S420 FM")
                comboxMaterial.Items.Add("S-1")
                comboxMaterial.Items.Add("S-7")
                comboxMaterial.Items.Add("- - - - - - - - - - - - - - - - - - - - - - -")
                comboxMaterial.Items.Add("A-2")
                comboxMaterial.Items.Add("A-10")
                comboxMaterial.Items.Add("ALU-BAR")
                comboxMaterial.Items.Add("ALU-CHANNEL")
                comboxMaterial.Items.Add("ALU-PLATE")
                comboxMaterial.Items.Add("ALU-PLATE (AIR)")
                comboxMaterial.Items.Add("AMPCO 18")
                comboxMaterial.Items.Add("AMPCO 21")
                comboxMaterial.Items.Add("AMPCO 22")
                comboxMaterial.Items.Add("AMPCO 25")
                comboxMaterial.Items.Add("B-25")
                comboxMaterial.Items.Add("BRASS")
                comboxMaterial.Items.Add("BRASS (HEX BAR)")
                comboxMaterial.Items.Add("BRONZE")
                comboxMaterial.Items.Add("CALDIE")
                comboxMaterial.Items.Add("COPPER")
                comboxMaterial.Items.Add("CRS")
                comboxMaterial.Items.Add("D2")
                comboxMaterial.Items.Add("DIEVAR")
                comboxMaterial.Items.Add("FP 100")
                comboxMaterial.Items.Add("H13")
                comboxMaterial.Items.Add("H13-SUPREME")
                comboxMaterial.Items.Add("H13-PRE SUPERIOR")
                comboxMaterial.Items.Add("HOVADUR")
                comboxMaterial.Items.Add("KEY STOCK")
                comboxMaterial.Items.Add("M-2 TOOL STEEL")
                comboxMaterial.Items.Add("M314 SS")
                comboxMaterial.Items.Add("MIRRAX")
                comboxMaterial.Items.Add("MIRRAX 40")
                comboxMaterial.Items.Add("MOLDMAXV")
                comboxMaterial.Items.Add("MOLD TAG GENERAL")
                comboxMaterial.Items.Add("MOLD TAG QPC")
                comboxMaterial.Items.Add("NAK 55")
                comboxMaterial.Items.Add("NIMAX")
                comboxMaterial.Items.Add("NITRALLOY")
                comboxMaterial.Items.Add("NYLON BAR")
                comboxMaterial.Items.Add("NYLATRON")
                comboxMaterial.Items.Add("O-1")
                comboxMaterial.Items.Add("O-6")
                comboxMaterial.Items.Add("ORVAR")
                comboxMaterial.Items.Add("ORVAR SUPERIOR")
                comboxMaterial.Items.Add("P-6")
                comboxMaterial.Items.Add("P-20")
                comboxMaterial.Items.Add("P-20 IMPAX")
                comboxMaterial.Items.Add("P-20 HI-HARD")
                comboxMaterial.Items.Add("POLY-URETHANE")
                comboxMaterial.Items.Add("PROTHERM")
                comboxMaterial.Items.Add("QRO-90")
                comboxMaterial.Items.Add("RAMAX (ROUND)")
                comboxMaterial.Items.Add("S-1")
                comboxMaterial.Items.Add("S-7")
                comboxMaterial.Items.Add("SS303")
                comboxMaterial.Items.Add("SS304")
                comboxMaterial.Items.Add("SS420")
                comboxMaterial.Items.Add("SS420 ELMAX")
                comboxMaterial.Items.Add("S420 FM")
                comboxMaterial.Items.Add("SS420 SUPREME")
                comboxMaterial.Items.Add("TELCO")
                comboxMaterial.Items.Add("TEFLON")
                comboxMaterial.Items.Add("THRD ROD C IMP")
                comboxMaterial.Items.Add("THRD ROD F IMP")
                comboxMaterial.Items.Add("THRD ROD C MET")
                comboxMaterial.Items.Add("THRD ROD F MET")
                comboxMaterial.Items.Add("TITANIUM 5")
                comboxMaterial.Items.Add("TIVAR")
                comboxMaterial.Items.Add("TOUGHMET")
                comboxMaterial.Items.Add("TUBING (ALUMINUM)")
                comboxMaterial.Items.Add("TUBING (BRONZE)")
                comboxMaterial.Items.Add("TUBING (BRASS)")
                comboxMaterial.Items.Add("TUBING HONED")
                comboxMaterial.Items.Add("TUBING (SS)")
                comboxMaterial.Items.Add("VANCRON40")
                comboxMaterial.Items.Add("VANCRON50")
                comboxMaterial.Items.Add("VISCOUNT")
                Me.comboxMaterial.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
                Try
                    ReadAttribute("STACKTECK_MATERIAL", comboxMaterial.Text)
                Catch ex As Exception
                End Try

            ElseIf (chkBoxRawMaterial.Checked = False) Then

                MaterialButton.Enabled = False

                comboxMaterial.Text = ""
                txtBoxVisMaterial.Text = ""
                txtBoxVisMaterialDescription.Text = ""
                txtBoxEachQuantity.Text = ""
                txtBoxTotalQuantity.Text = ""
                txtBoxUnitOfMeasure.Text = ""

                comboxMaterial.Items.Clear()
                comboxMaterial.Sorted = True

            End If
        End Sub

        Private Sub chkBoxPurchasedPart_CheckedChanged(sender As Object, e As EventArgs) Handles chkBoxPurchasedPart.CheckedChanged

            If (chkBoxPurchasedPart.Checked) Then
                chkBoxRawMaterial.Checked = False
                chkBoxSkipMaterialSelection.Checked = False
                MaterialButton.Enabled = True
                comboxMaterial.Sorted = True

                comboxMaterialHdn.Text = "AS SUPPLIED"
                comboxMaterialHdn.Enabled = False


                Dim cn As OleDbConnection
                Dim cmd As OleDbCommand
                Dim dr As OleDbDataReader
                Dim oConnect, oQuery As String
                oConnect = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Y:\eng\ENG_ACCESS_DATABASES\VisibPartAttributes.mdb"
                oQuery = "SELECT * FROM VISIB_PARTMASTER_LOCAL WHERE PRODUCT_LINE LIKE '%PUR%' OR PRODUCT_LINE LIKE '%NOSTD%' AND PARTDESCR NOT LIKE '%OBSOLETE%'"

                Try
                    cn.Open()
                Catch ex As Exception
                Finally
                    cn = New OleDbConnection(oConnect)
                    cn.Open()
                End Try

                cmd = New OleDbCommand(oQuery, cn)
                dr = cmd.ExecuteReader

                Me.comboxMaterial.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None


                comboxMaterial.Items.Add("- - OTHER - -")

                While dr.Read()
                    comboxMaterial.Items.Add(dr(0))
                End While
                Me.comboxMaterial.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
                Try
                    ReadAttribute("STACKTECK_MATERIAL", comboxMaterial.Text)
                Catch ex As Exception
                End Try
                dr.Close()
                cn.Close()

                Try
                    Dim s As Session = Session.GetSession()
                    Dim dispPart As Part = s.Parts.Display()
                    Dim c As NXOpen.Assemblies.Component = dispPart.ComponentAssembly.RootComponent

                    Dim children As NXOpen.Assemblies.Component() = c.GetChildren()

                    Dim childMaterial As String = Nothing

                    For Each child As NXOpen.Assemblies.Component In children
                        childMaterial = child.GetStringAttribute("STACKTECK_PARTN")
                        If (childMaterial.Length > 5 Or child.Name.StartsWith("PUR")) Then
                            comboxMaterial.Text = childMaterial
                        End If
                    Next

                Catch ex As Exception

                End Try

            ElseIf (chkBoxPurchasedPart.Checked = False) Then

                comboxMaterialHdn.Text = ""
                comboxMaterialHdn.Enabled = True


                txtBoxDiameter.Enabled = True

                txtBoxRoundLength.Enabled = True

                txtBoxInnerDiameter.Enabled = True

                txtBoxLength.Enabled = True

                txtBoxWidth.Enabled = True

                txtBoxThickness.Enabled = True

                MaterialButton.Enabled = False
                txtBoxVisMaterial.Text = ""
                txtBoxVisMaterialDescription.Text = ""
                txtBoxEachQuantity.Text = ""
                txtBoxTotalQuantity.Text = ""
                txtBoxUnitOfMeasure.Text = ""
                comboxMaterial.Sorted = False

                comboxMaterial.Items.Clear()
                comboxMaterial.Text = ""
            End If
        End Sub
        Private Sub chkBoxSkipMaterialSelection_CheckedChanged(sender As Object, e As EventArgs) Handles chkBoxSkipMaterialSelection.CheckedChanged
            If (chkBoxSkipMaterialSelection.Checked) Then
                chkBoxRawMaterial.Checked = False
                chkBoxPurchasedPart.Checked = False
                MaterialButton.Enabled = False
                comboxMaterial.Sorted = True

                comboxMaterial.Text = ""
                comboxMaterial.Enabled = False

                comboxMaterialHdn.Text = ""
                comboxMaterialHdn.Enabled = False

                txtBoxDiameter.Text = ""
                txtBoxDiameter.Enabled = False

                txtBoxRoundLength.Text = ""
                txtBoxRoundLength.Enabled = False

                txtBoxInnerDiameter.Text = ""
                txtBoxInnerDiameter.Enabled = False

                txtBoxLength.Text = ""
                txtBoxLength.Enabled = False

                txtBoxWidth.Text = ""
                txtBoxWidth.Enabled = False

                txtBoxThickness.Text = ""
                txtBoxThickness.Enabled = False

            ElseIf (chkBoxSkipMaterialSelection.Checked = False) Then
                comboxMaterial.Enabled = True
                comboxMaterialHdn.Enabled = True

                comboxMaterialHdn.Text = ""
                comboxMaterialHdn.Enabled = True

                txtBoxDiameter.Text = ""
                txtBoxDiameter.Enabled = True

                txtBoxRoundLength.Text = ""
                txtBoxRoundLength.Enabled = True

                txtBoxInnerDiameter.Text = ""
                txtBoxInnerDiameter.Enabled = True

                txtBoxLength.Text = ""
                txtBoxLength.Enabled = True

                txtBoxWidth.Text = ""
                txtBoxWidth.Enabled = True

                txtBoxThickness.Text = ""
                txtBoxThickness.Enabled = True

                MaterialButton.Enabled = True
                txtBoxVisMaterial.Text = ""
                txtBoxVisMaterialDescription.Text = ""
                txtBoxEachQuantity.Text = ""
                txtBoxTotalQuantity.Text = ""
                txtBoxUnitOfMeasure.Text = ""
                comboxMaterial.Sorted = False

                comboxMaterial.Items.Clear()
                comboxMaterial.Text = ""
            End If
        End Sub

        Private Sub txtBoxQty_TextChanged(sender As Object, e As EventArgs) Handles txtBoxQty.TextChanged
            If IsNumeric(txtBoxQty.Text) = False And txtBoxQty.Text <> "" And txtBoxQty.Text <> "." Then
                FailureTimerTick(txtBoxQty)
                Exit Sub
            End If
        End Sub

        Private Sub txtBoxDiameter_TextChanged(sender As Object, e As EventArgs) Handles txtBoxDiameter.TextChanged
            If txtBoxDiameter.Text <> "" Or txtBoxRoundLength.Text <> "" Or txtBoxInnerDiameter.Text <> "" Then
                txtBoxWidth.Text = ""
                txtBoxWidth.Enabled = False
                txtBoxThickness.Text = ""
                txtBoxThickness.Enabled = False
                txtBoxLength.Text = ""
                txtBoxLength.Enabled = False
            ElseIf txtBoxDiameter.Text = "" And txtBoxRoundLength.Text = "" And txtBoxInnerDiameter.Text = "" Then
                txtBoxWidth.Enabled = True
                txtBoxThickness.Enabled = True
                txtBoxLength.Enabled = True
            End If

            If IsNumeric(txtBoxDiameter.Text) = False And txtBoxDiameter.Text <> "" And txtBoxDiameter.Text <> "." Then
                FailureTimerTick(txtBoxDiameter)
                txtBoxFullDimensions.Text = ""
                Exit Sub
            End If

            If txtBoxDiameter.Text <> "" And txtBoxRoundLength.Text <> "" Then
                txtBoxFullDimensions.Text = txtBoxDiameter.Text + "DIAx" + txtBoxRoundLength.Text
            ElseIf txtBoxDiameter.Text = "" And txtBoxRoundLength.Text <> "" Then
                txtBoxFullDimensions.Text = txtBoxDiameter.Text + "DIAx" + txtBoxRoundLength.Text
            ElseIf txtBoxDiameter.Text <> "" And txtBoxRoundLength.Text = "" Then
                txtBoxFullDimensions.Text = txtBoxDiameter.Text + "DIAx" + txtBoxRoundLength.Text
            ElseIf txtBoxDiameter.Text = "" And txtBoxRoundLength.Text = "" Then
                txtBoxFullDimensions.Text = ""
            End If
        End Sub
        Private Sub txtBoxRoundLength_TextChanged(sender As Object, e As EventArgs) Handles txtBoxRoundLength.TextChanged
            If txtBoxRoundLength.Text <> "" Or txtBoxDiameter.Text <> "" Or txtBoxInnerDiameter.Text <> "" Then
                txtBoxWidth.Text = ""
                txtBoxWidth.Enabled = False
                txtBoxThickness.Text = ""
                txtBoxThickness.Enabled = False
                txtBoxLength.Text = ""
                txtBoxLength.Enabled = False
            ElseIf txtBoxRoundLength.Text = "" And txtBoxDiameter.Text = "" And txtBoxInnerDiameter.Text = "" Then
                txtBoxWidth.Enabled = True
                txtBoxThickness.Enabled = True
                txtBoxLength.Enabled = True
            End If

            If IsNumeric(txtBoxRoundLength.Text) = False And txtBoxRoundLength.Text <> "" And txtBoxRoundLength.Text <> "." Then
                FailureTimerTick(txtBoxRoundLength)
                txtBoxFullDimensions.Text = ""
                Exit Sub
            End If

            If txtBoxDiameter.Text <> "" And txtBoxRoundLength.Text <> "" Then
                txtBoxFullDimensions.Text = txtBoxDiameter.Text + "DIAx" + txtBoxRoundLength.Text
            ElseIf txtBoxDiameter.Text = "" And txtBoxRoundLength.Text <> "" Then
                txtBoxFullDimensions.Text = txtBoxDiameter.Text + "DIAx" + txtBoxRoundLength.Text
            ElseIf txtBoxDiameter.Text <> "" And txtBoxRoundLength.Text = "" Then
                txtBoxFullDimensions.Text = txtBoxDiameter.Text + "DIAx"
            ElseIf txtBoxDiameter.Text = "" And txtBoxRoundLength.Text = "" Then
                txtBoxFullDimensions.Text = ""
            End If
        End Sub

        Private Sub txtBoxInnerDiameter_TextChanged(sender As Object, e As EventArgs) Handles txtBoxInnerDiameter.TextChanged
            If txtBoxRoundLength.Text <> "" Or txtBoxDiameter.Text <> "" Or txtBoxInnerDiameter.Text <> "" Then
                txtBoxWidth.Text = ""
                txtBoxWidth.Enabled = False
                txtBoxThickness.Text = ""
                txtBoxThickness.Enabled = False
                txtBoxLength.Text = ""
                txtBoxLength.Enabled = False
            ElseIf txtBoxRoundLength.Text = "" And txtBoxDiameter.Text = "" And txtBoxInnerDiameter.Text = "" Then
                txtBoxWidth.Enabled = True
                txtBoxThickness.Enabled = True
                txtBoxLength.Enabled = True
            End If

            If IsNumeric(txtBoxInnerDiameter.Text) = False And txtBoxInnerDiameter.Text <> "" And txtBoxInnerDiameter.Text <> "." Then
                FailureTimerTick(txtBoxInnerDiameter)
                Exit Sub
            End If
        End Sub
        Private Sub txtBoxThickness_TextChanged(sender As Object, e As EventArgs) Handles txtBoxThickness.TextChanged
            If txtBoxLength.Text <> "" Or txtBoxWidth.Text <> "" Or txtBoxThickness.Text <> "" Then
                txtBoxDiameter.Text = ""
                txtBoxDiameter.Enabled = False
                txtBoxInnerDiameter.Text = ""
                txtBoxRoundLength.Text = ""
                txtBoxRoundLength.Enabled = False
            ElseIf txtBoxLength.Text = "" And txtBoxWidth.Text = "" And txtBoxThickness.Text = "" And chkBoxRawMaterial.Checked Then
                txtBoxDiameter.Enabled = True
                txtBoxRoundLength.Enabled = True
                txtBoxRoundLength.Enabled = True
            ElseIf txtBoxLength.Text = "" And txtBoxWidth.Text = "" And txtBoxThickness.Text = "" Then 'this code is new
                txtBoxDiameter.Enabled = True
                txtBoxRoundLength.Enabled = True
                txtBoxRoundLength.Enabled = True
            End If
            'spoilt some of them 
            If IsNumeric(txtBoxThickness.Text) = False And txtBoxThickness.Text <> "" And txtBoxThickness.Text <> "." Then
                FailureTimerTick(txtBoxThickness)
                txtBoxFullDimensions.Text = ""
                Exit Sub
            End If

            If txtBoxWidth.Text <> "" Or txtBoxThickness.Text <> "" Then
                txtBoxFullDimensions.Text = txtBoxThickness.Text + "x" + txtBoxWidth.Text + "x" + txtBoxLength.Text
            ElseIf txtBoxWidth.Text = "" And txtBoxThickness.Text = "" And txtBoxLength.Text = "" Then
                txtBoxFullDimensions.Text = ""
            End If
        End Sub
        Private Sub txtBoxWidth_TextChanged(sender As Object, e As EventArgs) Handles txtBoxWidth.TextChanged
            If txtBoxLength.Text <> "" Or txtBoxWidth.Text <> "" Or txtBoxThickness.Text <> "" Then
                txtBoxDiameter.Text = ""
                txtBoxDiameter.Enabled = False
                txtBoxInnerDiameter.Text = ""
                txtBoxRoundLength.Text = ""
                txtBoxRoundLength.Enabled = False
            ElseIf txtBoxLength.Text = "" And txtBoxWidth.Text = "" And txtBoxThickness.Text = "" And chkBoxRawMaterial.Checked Then
                txtBoxDiameter.Enabled = True
                txtBoxRoundLength.Enabled = True
            ElseIf txtBoxLength.Text = "" And txtBoxWidth.Text = "" And txtBoxThickness.Text = "" Then 'this code is new
                txtBoxDiameter.Enabled = True
                txtBoxRoundLength.Enabled = True
                txtBoxRoundLength.Enabled = True
            End If

            If IsNumeric(txtBoxWidth.Text) = False And txtBoxWidth.Text <> "" And txtBoxWidth.Text <> "." Then
                FailureTimerTick(txtBoxWidth)
                txtBoxFullDimensions.Text = ""
                Exit Sub
            End If

            If txtBoxWidth.Text <> "" Or txtBoxThickness.Text <> "" Then
                txtBoxFullDimensions.Text = txtBoxThickness.Text + "x" + txtBoxWidth.Text + "x" + txtBoxLength.Text
            ElseIf txtBoxWidth.Text = "" And txtBoxThickness.Text = "" And txtBoxLength.Text = "" Then
                txtBoxFullDimensions.Text = ""
            End If
        End Sub
        Private Sub txtBoxLength_TextChanged(sender As Object, e As EventArgs) Handles txtBoxLength.TextChanged
            If txtBoxLength.Text <> "" Or txtBoxWidth.Text <> "" Or txtBoxThickness.Text <> "" Or chkBoxRawMaterial.Checked = False Then
                txtBoxDiameter.Text = ""
                txtBoxDiameter.Enabled = False
                txtBoxInnerDiameter.Text = ""
                txtBoxRoundLength.Text = ""
                txtBoxRoundLength.Enabled = False
            ElseIf txtBoxLength.Text = "" And txtBoxWidth.Text = "" And txtBoxThickness.Text = "" And chkBoxRawMaterial.Checked Then
                txtBoxDiameter.Enabled = True
                txtBoxRoundLength.Enabled = True
            ElseIf txtBoxLength.Text = "" And txtBoxWidth.Text = "" And txtBoxThickness.Text = "" Then 'this code is new
                txtBoxDiameter.Enabled = True
                txtBoxRoundLength.Enabled = True
                txtBoxRoundLength.Enabled = True
            End If

            If IsNumeric(txtBoxLength.Text) = False And txtBoxLength.Text <> "" And txtBoxLength.Text <> "." Then
                FailureTimerTick(txtBoxLength)
                txtBoxFullDimensions.Text = ""
                Exit Sub
            End If

            If txtBoxWidth.Text <> "" Or txtBoxThickness.Text <> "" Then
                txtBoxFullDimensions.Text = txtBoxThickness.Text + "x" + txtBoxWidth.Text + "x" + txtBoxLength.Text
            End If
        End Sub
        Private Sub comboxMaterial_SelectedIndexChanged(sender As Object, e As EventArgs) Handles comboxMaterial.SelectedIndexChanged
            If (comboxMaterial.Text = "THRD ROD C MET" Or comboxMaterial.Text = "THRD ROD F MET" Or comboxMaterial.Text = "HOVADUR") Then
                MsgBox("For this part, enter the outer diameter In mm, but enter the length In inches")

            ElseIf (comboxMaterial.Text = "PIN,ANGLE,DM M" Or comboxMaterial.Text = "PIN,ANGLE,HA M" Or comboxMaterial.Text = "PIN,CORE,HRD M" Or comboxMaterial.Text = "PIN,LEADER,SH,M" Or comboxMaterial.Text = "PIN,LEADER,STR,M" Or comboxMaterial.Text = "PIN,THMPSHFT,CL'G6'" Or comboxMaterial.Text = "PIN,THMPSHFT,CL'M'") Then
                MsgBox("For this part, enter both the outer diameter and length in mm")
            End If

            If (comboxMaterial.Text = "BRONZE" Or comboxMaterial.Text = "AMPCO 18" Or comboxMaterial.Text = "TUBING (ALUMINUM)" Or comboxMaterial.Text = "TUBING (BRONZE)" Or comboxMaterial.Text = "TUBING (BRASS)" Or comboxMaterial.Text = "TUBING HONED" Or comboxMaterial.Text = "TUBING (SS)" Or comboxMaterial.Text = "PIN,EJECT,N,SH") Then
                txtBoxInnerDiameter.Enabled = True
            Else
                txtBoxInnerDiameter.Enabled = False
                txtBoxInnerDiameter.Text = ""
            End If

            'If (comboxMaterial.Text <> "BRONZE" And comboxMaterial.Text <> "AMPCO 18" And comboxMaterial.Text <> "TUBING (ALUMINUM)" And comboxMaterial.Text <> "TUBING (BRONZE)" And comboxMaterial.Text <> "TUBING (BRASS)" And comboxMaterial.Text <> "TUBING HONED" And comboxMaterial.Text <> "TUBING (SS)" And comboxMaterial.Text <> "PIN,EJECT,N,SH") Then
            '    txtBoxInnerDiameter.Text = ""
            '    txtBoxInnerDiameter.Enabled = False
            'Else
            '    txtBoxInnerDiameter.Enabled = True
            'End If
        End Sub
        Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

            If (System.Environment.UserName.ToUpper.Trim = "SUSAN") Then ' SUSAN EDITION
                btnOK.Location = New System.Drawing.Point(630, 630)
                btnCancel.Location = New System.Drawing.Point(750, 630)
            End If

            If CheckSpecification() = True Then
                Exit Sub
            End If

            Dim IsMDM As Boolean = False
            If CheckMDMPart() = True Then
                IsMDM = True
            End If


            comboxMaterialHdn.Items.Add("PRE-HDN")
            comboxMaterialHdn.Items.Add("36-38 Rc")
            comboxMaterialHdn.Items.Add("44-46 Rc")
            comboxMaterialHdn.Items.Add("48-50 Rc")
            comboxMaterialHdn.Items.Add("54-56 Rc")
            comboxMaterialHdn.Items.Add("AS SUPPLIED")

            ' Read General Info Attributes
            ReadAttribute("TITBLK_DESIGNER", txtBoxDesigner.Text)
            ReadAttribute("TITBLK_TEAM", txtBoxTeam.Text)
            ReadAttribute("TITBLK_DATE", txtBoxDate.Text)
            ReadAttribute("TITBLK_CHECKER", txtBoxChecker.Text)
            ReadAttribute("TITBLK_DWGSCALE", txtBoxScale.Text)
            ReadAttribute("STACKTECK_PROJNUM", txtBoxJobNo.Text)

            ' Read Part Info Attributes
            ReadAttribute("STACKTECK_PARTN", txtBoxPartNo.Text)
            ReadAttribute("STACKTECK_QTY", txtBoxQty.Text)
            ReadAttribute("STACKTECK_SPAREQTY", txtBoxSpareQty.Text)
            ReadAttribute("STACKTECK_DESC", txtBoxDesc.Text)

            ' Read Vis Info Attribute
            ReadAttribute("STACKTECK_VISDESC", txtBoxVisDesc.Text)
            ReadAttribute("STACKTECK_VISCOMCODE", txtBoxVisComCode.Text)
            ReadAttribute("STACKTECK_VISDWGNUM", txtBoxVisDwgNo.Text)
            ReadAttribute("STACKTECK_VISNUM", "")

            Dim isRaw As String = Nothing
            Dim isPurchased As String = Nothing

            ' Read Material Info Attributes

            Try
                ReadAttribute("STACKTECK_IS_RAW_MATERIAL", isRaw)
            Catch ex As Exception
                isRaw = "False"
            End Try


            Try
                ReadAttribute("STACKTECK_IS_PURCHASED_COMPONENT", isPurchased)
            Catch ex As Exception
                isPurchased = "False"
            End Try

            chkBoxRawMaterial.Checked = Convert.ToBoolean(isRaw)
            chkBoxPurchasedPart.Checked = Convert.ToBoolean(isPurchased)

            If (chkBoxRawMaterial.Checked = False And chkBoxPurchasedPart.Checked = False) Then
                chkBoxRawMaterial.Checked = True
                txtBoxInnerDiameter.Enabled = False
            End If

            ReadAttribute("STACKTECK_MATERIAL", comboxMaterial.Text)
            ReadAttribute("STACKTECK_HARDNESS", comboxMaterialHdn.Text)

            ReadAttribute("STACKTECK_PART_DIAMETER", txtBoxDiameter.Text)
            ReadAttribute("STACKTECK_PART_INNDER_DIAMETER", txtBoxInnerDiameter.Text)
            ReadAttribute("STACKTECK_PART_ROUND_LENGTH", txtBoxRoundLength.Text)
            ReadAttribute("STACKTECK_PART_LENGTH", txtBoxLength.Text)
            ReadAttribute("STACKTECK_PART_WIDTH", txtBoxWidth.Text)
            ReadAttribute("STACKTECK_PART_THICKNESS", txtBoxThickness.Text)
            ReadAttribute("STACKTECK_BOM_NOTES", txtBoxNotes.Text)

            ReadAttribute("STACKTECK_COMPONENT_PART", txtBoxVisMaterial.Text)
            ReadAttribute("STACKTECK_COMPONENT_DESCRIPTION", txtBoxVisMaterialDescription.Text)
            ReadAttribute("STACKTECK_EACH_QUANTITY", txtBoxEachQuantity.Text)
            ReadAttribute("STACKTECK_TOTAL_QUANTITY", txtBoxTotalQuantity.Text)
            ReadAttribute("STACKTECK_UNIT_OF_MEASUREMENT", txtBoxUnitOfMeasure.Text)
            ReadAttribute("STACKTECK_PARTSIZE", txtBoxFullDimensions.Text)
            Dim count As Integer = 0
            Try
                If txtBoxPrefix.text.trim = "" Then
                    While IsNumeric(txtBoxFullDimensions.text(count)) = False And txtBoxFullDimensions.text(count) <> "."
                        txtBoxPrefix.text = txtBoxPrefix.text + txtBoxFullDimensions.text(count)
                        count += 1
                    End While
                End If
                If txtBoxPrefix.text.trim = "" And (comboxMaterial.text.contains("PCH") Or comboxMaterial.text.contains("PCS") Or comboxMaterial.text.contains("PEN") Or comboxMaterial.text.contains("PLH")) Then
                    txtBoxPrefix.text = "NORM"
                End If

            Catch ex As Exception
            End Try
            If txtBoxVisMaterial.text.trim = "" Then
                Try
                    AutoCompleteVisData()
                Catch ex As Exception
                    'MsgBox(ex.ToString)
                End Try
            End If
            Try
                correctRawMaterials()
            Catch ex As Exception
            End Try


            'txtBoxNotes.Text = txtBoxNotes.text + Environment.NewLine + "If the program give repeated errors, please select other as raw material and submit"

            'If (comboxMaterial.Text <> "OTHER") Then
            If (chkBoxRawMaterial.Checked And comboxMaterial.FindStringExact(comboxMaterial.Text) <= -1 And isRaw = "True") Then
                'MsgBox("Please re-select material!")
                'comboxMaterial.Text = ""
            End If
            'End If
            GetPreviousDimensions()
        End Sub
        Public Sub correctRawMaterials()
            If (comboxMaterial.text.toUpper.contains("H-13") Or comboxMaterial.text.toUpper.contains("H13")) And (comboxMaterial.text.toUpper.endswith("S") Or comboxMaterial.text.toUpper.contains("SUPERIOR")) Then
                comboxMaterial.text = "H13-PRE SUPERIOR"
            ElseIf comboxMaterial.text.toUpper.contains("H-13") Or comboxMaterial.text.toUpper.contains("H13") Then
                comboxMaterial.text = "H13"
            ElseIf (comboxMaterial.text.toUpper.contains("S420") Or comboxMaterial.text.toUpper.contains("SS-420")) And comboxMaterial.text.toUpper.contains("FM") Then
                comboxMaterial.text = "S420 FM"
            ElseIf (comboxMaterial.text.toUpper.contains("S420") Or comboxMaterial.text.toUpper.contains("SS-420")) And (comboxMaterial.text.trim.toUpper.endswith("S") Or comboxMaterial.text.toUpper.contains("SUPREME")) Then
                comboxMaterial.text = "SS420 SUPREME"
            ElseIf comboxMaterial.text.toUpper.contains("S420") Or comboxMaterial.text.toUpper.contains("SS-420") Then
                comboxMaterial.text = "SS420"
            ElseIf comboxMaterial.text.toUpper.startswith("S01") Then
                comboxMaterial.text = "S-1"
            ElseIf comboxMaterial.text.toUpper.startswith("S07") Then
                comboxMaterial.text = "S-7"
            ElseIf comboxMaterial.text.toUpper.contains("S303") Then
                comboxMaterial.text = "SS303"
            ElseIf comboxMaterial.text.toUpper.startswith("S07") Then
                comboxMaterial.text = "S-7"
            ElseIf comboxMaterial.text.toUpper.startswith("TSS") Then
                comboxMaterial.text = "TUBING (SS)"
            ElseIf comboxMaterial.text.toUpper.startswith("TSH") Then
                comboxMaterial.text = "TUBING HONED"
            ElseIf comboxMaterial.text.toUpper.startswith("RAM") Then
                comboxMaterial.text = "RAMAX (ROUND)"
            ElseIf comboxMaterial.text.toUpper.startswith("CRS") Then
                comboxMaterial.text = "CRS"
            End If
        End Sub

        Public Function ReadVisData(ByVal FileName As String, ByVal PartNum As String, ByRef VisPartDesc As String, ByRef VisComCode As String, ByRef VisDwgNum As String) As Boolean

            'Define the connectors
            Dim cn As OleDbConnection
            Dim cmd As OleDbCommand
            Dim dr As OleDbDataReader
            Dim oConnect, oQuery As String
            Dim FoundStatus As Boolean = False

            'Define connection string
            'oConnect = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & FileName
            oConnect = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & FileName

            'Query String
            oQuery = "SELECT * FROM VISIB_PARTMASTER_LOCAL where PARTNO='" & PartNum & "'"

            'Instantiate the connectors
            cn = New OleDbConnection(oConnect)

            Try
                cn.Open()
            Catch ex As Exception
            Finally
                cn = New OleDbConnection(oConnect)
                cn.Open()
            End Try

            cmd = New OleDbCommand(oQuery, cn)
            dr = cmd.ExecuteReader

            While dr.Read()
                VisPartDesc = dr("PARTDESCR")
                VisPartDesc = VisPartDesc.Trim()

                VisComCode = dr("COMMODITY_CODE")
                VisComCode = VisComCode.Trim()

                VisDwgNum = dr("DRAWINGNO")
                VisDwgNum = VisDwgNum.Trim()
                FoundStatus = True
            End While

            dr.Close()
            cn.Close()

            If FoundStatus = False Then
                FailureTimerTick(txtBoxPartNo)
                MsgBox("Unable to find part")
            End If
            Return FoundStatus
        End Function

        Public Function ReadVisDataForMaterial(ByVal materialPartNumber As String, ByVal isRound As Boolean) As Boolean
            Dim accessMaterialName(0) As String
            Dim accessMaterialDesc(0) As String
            Dim accessMaterialDia(0) As String
            Dim accessMaterialLength(0) As String
            Dim accessMaterialThickness(0) As String
            Dim accessMaterialWidth(0) As String
            Dim accessMaterialShoulderThickness_InnerDia(0) As String
            Dim accessWallThickness(0) As String
            Dim accessUnitOfMeasure(0) As String

            ' We only need density for plates/blocks, that's why some materials arent' included here
            If (comboxMaterial.Text.Contains("ALU")) Then
                density = 0.0979
            ElseIf (comboxMaterial.Text.Contains("AMPCO")) Then
                density = 0.292
            ElseIf (comboxMaterial.Text = "B-25") Then
                density = 0.302
            ElseIf (comboxMaterial.Text.Contains("BRASS")) Then
                density = 0.307
            ElseIf (comboxMaterial.Text = "BRONZE") Then
                density = 0.309
            ElseIf (comboxMaterial.Text = "COPPER") Then
                density = 0.323
            ElseIf (comboxMaterial.Text = "FP 100") Then
                density = 0.309
            ElseIf (comboxMaterial.Text = "HOVADUR") Then
                density = 0.299857
            ElseIf (comboxMaterial.Text = "M-2 Tool Steel") Then
                density = 0.295
            ElseIf (comboxMaterial.Text = "M314 SS") Then
                density = 0.276
            ElseIf (comboxMaterial.Text = "MIRRAX") Then
                density = 0.28
            ElseIf (comboxMaterial.Text = "MIRRAX 40") Then
                density = 0.278
            ElseIf (comboxMaterial.Text = "MOLDMAXV") Then
                density = 0.302
            ElseIf (comboxMaterial.Text = "NYLATRON") Then
                density = 0.04154639
            ElseIf (comboxMaterial.Text = "TOUGHMET") Then
                density = 0.325
            Else
                density = 0.283
            End If

            'Define the connectors
            Dim cn As OleDbConnection
            Dim cmd As OleDbCommand
            Dim dr As OleDbDataReader
            Dim oConnect, oQuery As String
            Dim FoundStatus As Boolean = False

            'Define connection string
            'oConnect = "Provider= Microsoft.Jet.OLEDB.4.0;Data Source=" & FileName
            oConnect = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Y:\eng\ENG_ACCESS_DATABASES\VisibPartAttributes.mdb"

            If (isRound And chkBoxRawMaterial.Checked) Then
                If (comboxMaterial.Text = "AMPCO 18" And txtBoxInnerDiameter.Text <> "") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME Like'" & materialPartNumber & "H%' AND DIA = " & txtBoxDiameter.Text & " AND LENGTH > " & txtBoxRoundLength.Text & " AND WALL_THICKNESS = " & ((Convert.ToDouble(txtBoxDiameter.Text) - Convert.ToDouble(txtBoxInnerDiameter.Text)) / 2).ToString
                ElseIf (comboxMaterial.Text = "AMPCO 18" And txtBoxInnerDiameter.Text = "") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DESCRIPTION NOT LIKE '%HOLLOW%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text
                ElseIf (comboxMaterial.Text = "BRONZE" And txtBoxInnerDiameter.Text <> "") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'BZH%' AND DIA = " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text & " AND WALL_THICKNESS = " & ((Convert.ToDouble(txtBoxDiameter.Text) - Convert.ToDouble(txtBoxInnerDiameter.Text)) / 2).ToString
                ElseIf (comboxMaterial.Text = "CALDIE") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'%CALDIE%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text
                ElseIf (comboxMaterial.Text = "H13") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND MATERIAL_NAME NOT LIKE '%S%' AND MATERIAL_NAME NOT LIKE '%D%' AND MATERIAL_NAME NOT LIKE '%PRE%' AND MATERIAL_NAME NOT LIKE '%CALDIE%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text
                ElseIf (comboxMaterial.Text = "DIEVAR") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND MATERIAL_NAME LIKE '%D%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text
                ElseIf (comboxMaterial.Text = "H13-SUPREME") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND MATERIAL_NAME LIKE '%S%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text & " AND DESCRIPTION NOT LIKE '%SUPERIOR%'"
                ElseIf (comboxMaterial.Text = "P-20 HI-HARD") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text & " AND DESCRIPTION LIKE '%HI-HARD%'"
                ElseIf (comboxMaterial.Text = "P-20") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text & " AND DESCRIPTION NOT LIKE '%HI-HARD%' AND DESCRIPTION NOT LIKE '%IMPAX%'"
                ElseIf (comboxMaterial.Text = "SS303") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text & " AND DESCRIPTION NOT LIKE '%HEX%'"
                ElseIf (comboxMaterial.Text = "SS420 ELMAX") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND MATERIAL_NAME LIKE '%E%' AND MATERIAL_NAME NOT LIKE '%FM%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text & " AND DESCRIPTION LIKE '%ELMAX%'"
                ElseIf (comboxMaterial.Text = "SS420 SUPREME") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND MATERIAL_NAME LIKE '%S%' AND MATERIAL_NAME NOT LIKE '%FM%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text & " AND DESCRIPTION LIKE '%SUPREME%'"
                ElseIf (comboxMaterial.Text = "SS420") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text & " AND DESCRIPTION NOT LIKE '%SUPREME%' AND MATERIAL_NAME NOT LIKE '%FM%' AND DESCRIPTION NOT LIKE '%ELMAX%'"
                ElseIf (comboxMaterial.Text.Contains("TUBING") And comboxMaterial.Text <> "TUBING HONED") Then 'Change it closest lower value
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DIA <= " & txtBoxDiameter.Text & " AND LENGTH > " & txtBoxRoundLength.Text '& " AND WALL_THICKNESS >= " & (Math.Round(((Convert.ToDouble(txtBoxDiameter.Text) - Convert.ToDouble(txtBoxInnerDiameter.Text)) / 2), 4)).ToString
                ElseIf (comboxMaterial.Text.Contains("TUBING") And comboxMaterial.Text = "TUBING HONED") Then  'Designers use wall thickenss to calculate inner diameter not vice versa
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DIA <= " & txtBoxDiameter.Text & " AND LENGTH > " & txtBoxRoundLength.Text '& " AND WALL_THICKNESS = " & (Math.Round(((Convert.ToDouble(txtBoxDiameter.Text) - Convert.ToDouble(txtBoxInnerDiameter.Text)) / 2), 4)).ToString
                ElseIf (comboxMaterial.Text = "THRD ROD C IMP") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH > " & txtBoxRoundLength.Text & " AND DESCRIPTION LIKE '%COARSE%' AND DESCRIPTION NOT LIKE '%M%'"
                ElseIf (comboxMaterial.Text = "THRD ROD C MET") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH > " & txtBoxRoundLength.Text & " AND DESCRIPTION LIKE '%COARSE%' AND DESCRIPTION LIKE '%M%'"
                ElseIf (comboxMaterial.Text = "THRD ROD F IMP") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH > " & txtBoxRoundLength.Text & " AND DESCRIPTION LIKE '%FINE%' AND DESCRIPTION NOT LIKE '%M%'"
                ElseIf (comboxMaterial.Text = "THRD ROD F MET") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH > " & txtBoxRoundLength.Text & " AND DESCRIPTION LIKE '%FINE%' AND DESCRIPTION LIKE '%M%'"
                Else
                    oQuery = "Select * FROM RAW_MATERIALS WHERE MATERIAL_NAME Like'" & materialPartNumber & "%' AND DIA >= " & txtBoxDiameter.Text & " AND LENGTH >= " & txtBoxRoundLength.Text
                End If
            ElseIf (isRound = False And chkBoxRawMaterial.Checked) Then
                If (comboxMaterial.Text = "ALU-PLATE") Then ' Special case
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND MATERIAL_NAME <> 'ALUPLATEACG' And THICKNESS >= " & (Convert.ToDouble(txtBoxThickness.Text)).ToString & " And LENGTH > " & txtBoxLength.Text & " And Width >= " & (Convert.ToDouble(txtBoxWidth.Text)).ToString
                ElseIf (comboxMaterial.Text = "H13") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE '" & materialPartNumber & "%' AND MATERIAL_NAME LIKE '%PLATE%' AND MATERIAL_NAME NOT LIKE '%PLATES%' AND DESCRIPTION NOT LIKE '%GRADE%'"
                ElseIf (comboxMaterial.Text = "H13-SUPREME") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE '" & materialPartNumber & "%' AND MATERIAL_NAME LIKE '%PLATES%' AND DESCRIPTION LIKE '%GRADE%'"
                ElseIf (comboxMaterial.Text = "DIEVAR") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE'" & materialPartNumber & "%' AND MATERIAL_NAME LIKE '%D%' And THICKNESS >= " & (Convert.ToDouble(txtBoxThickness.Text)).ToString & " And LENGTH > " & txtBoxLength.Text & " And Width >= " & (Convert.ToDouble(txtBoxWidth.Text)).ToString
                ElseIf (comboxMaterial.Text = "P-20") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME='P20PLATE'"
                ElseIf (comboxMaterial.Text = "P-20 HI-HARD") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME='P20PLATEHH'"
                ElseIf (comboxMaterial.Text = "SS303") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME Like'" & materialPartNumber & "%' AND DESCRIPTION NOT LIKE '%HEX%' AND THICKNESS >= " & (Convert.ToDouble(txtBoxThickness.Text)).ToString & " AND LENGTH >= " & txtBoxLength.Text & " AND WIDTH >= " & (Convert.ToDouble(txtBoxWidth.Text)).ToString
                ElseIf (comboxMaterial.Text = "SS420 ELMAX") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME Like'" & materialPartNumber & "%' AND MATERIAL_NAME NOT LIKE '%FM%' AND DESCRIPTION LIKE '%ELMAX%' AND THICKNESS >= " & (Convert.ToDouble(txtBoxThickness.Text)).ToString & " AND LENGTH >= " & txtBoxLength.Text & " AND WIDTH >= " & (Convert.ToDouble(txtBoxWidth.Text)).ToString
                ElseIf (comboxMaterial.Text = "SS420 SUPREME") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME Like'" & materialPartNumber & "%' AND MATERIAL_NAME NOT LIKE '%FM%' AND DESCRIPTION LIKE '%SUPREME%' AND THICKNESS >= " & (Convert.ToDouble(txtBoxThickness.Text)).ToString & " AND LENGTH >= " & txtBoxLength.Text & " AND WIDTH >= " & (Convert.ToDouble(txtBoxWidth.Text)).ToString
                ElseIf (comboxMaterial.Text = "SS420") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME Like'" & materialPartNumber & "%' AND MATERIAL_NAME NOT LIKE '%FM%' AND DESCRIPTION NOT LIKE '%SUPREME%' AND DESCRIPTION NOT LIKE '%ELMAX%' AND DESCRIPTION NOT LIKE '%STAVAX%' AND DESCRIPTION NOT LIKE '%ROYALLOY%'  AND THICKNESS >= " & (Convert.ToDouble(txtBoxThickness.Text)).ToString & " AND LENGTH >= " & txtBoxLength.Text & " AND WIDTH >= " & (Convert.ToDouble(txtBoxWidth.Text)).ToString
                ElseIf (comboxMaterial.Text = "MOLD TAG GENERAL" Or comboxMaterial.Text = "MOLD TAG QPC") Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME Like'" & materialPartNumber & "%'"
                ElseIf (comboxMaterial.Text = "BRONZE" And txtBoxVisDesc.Text.ToUpper.Contains("PLATE")) Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE 'BRZPLATE'"
                ElseIf (comboxMaterial.Text = "O-1" And txtBoxVisDesc.Text.ToUpper.Contains("PLATE")) Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE '%O10PLATE%'"
                ElseIf (comboxMaterial.Text = "SS420" And txtBoxVisDesc.Text.ToUpper.Contains("PLATE")) Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE '%S420PLATE%'"
                ElseIf (comboxMaterial.Text = "S420 FM" And txtBoxVisDesc.Text.ToUpper.Contains("PLATE")) Then
                    oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME LIKE '%S420FMPLATE%'"
                Else ' Special cases end 
                    oQuery = "Select * FROM RAW_MATERIALS WHERE MATERIAL_NAME Like'" & materialPartNumber & "%' AND THICKNESS >= " & (Convert.ToDouble(txtBoxThickness.Text)).ToString & " AND LENGTH >= " & (Convert.ToDouble(txtBoxLength.Text)).ToString & " AND WIDTH >= " & (Convert.ToDouble(txtBoxWidth.Text)).ToString
                End If
            ElseIf (chkBoxPurchasedPart.Checked) Then
                If (comboxMaterial.Text <> "AS SUPPLIED") Then
                    oQuery = "SELECT * FROM VISIB_PARTMASTER_LOCAL where PARTNO='" & comboxMaterial.Text & "'"
                ElseIf (comboxMaterial.Text = "AS SUPPLIED") Then
                    txtBoxVisMaterial.Text = "AS SUPPLIED"
                    Exit Function
                End If
            End If

            'Instantiate the connectors
            cn = New OleDbConnection(oConnect)
            cn.Open()

            cmd = New OleDbCommand(oQuery, cn)
            dr = cmd.ExecuteReader

            Dim count As Integer = 0

            Try
                While dr.Read()
                    If (chkBoxRawMaterial.Checked) Then
                        accessMaterialName(count) = dr("MATERIAL_NAME")
                        ReDim Preserve accessMaterialName(count + 1)

                        accessMaterialDesc(count) = dr("DESCRIPTION")
                        ReDim Preserve accessMaterialDesc(count + 1)

                        accessMaterialDia(count) = dr("DIA")
                        ReDim Preserve accessMaterialDia(count + 1)

                        accessMaterialLength(count) = dr("LENGTH")
                        ReDim Preserve accessMaterialLength(count + 1)

                        accessMaterialThickness(count) = dr("THICKNESS")
                        ReDim Preserve accessMaterialThickness(count + 1)

                        accessMaterialWidth(count) = dr("WIDTH")
                        ReDim Preserve accessMaterialWidth(count + 1)

                        accessWallThickness(count) = dr("WALL_THICKNESS")
                        ReDim Preserve accessWallThickness(count + 1)

                        accessUnitOfMeasure(count) = dr("UNIT_OF_MEASURE")
                        ReDim Preserve accessUnitOfMeasure(count + 1)
                    ElseIf (chkBoxPurchasedPart.Checked) Then   ' Reading from a different table
                        accessMaterialName(count) = dr("PARTNO")
                        ReDim Preserve accessMaterialName(count + 1)

                        accessMaterialDesc(count) = dr("PARTDESCR")
                        ReDim Preserve accessMaterialDesc(count + 1)

                        accessUnitOfMeasure(count) = dr("UM")
                        ReDim Preserve accessUnitOfMeasure(count + 1)
                    End If

                    count += 1
                    FoundStatus = True
                End While
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try

            dr.Close()
            cn.Close()

            Dim largestDiameterIndex As Integer = 0
            Dim smallestDiameterIndex As Integer = 0
            Dim smallestWidthThicknessIndex As Integer = 0
            Dim minVolume As Double = 0


            If FoundStatus = False Then
                FailureTimerTick(txtBoxVisMaterial)
                txtBoxVisMaterial.Text = ""
                txtBoxVisMaterialDescription.Text = ""
                txtBoxEachQuantity.Text = ""
                txtBoxTotalQuantity.Text = ""
                txtBoxUnitOfMeasure.Text = ""
                Return False
            Else

                SuccessTimerTick(txtBoxVisMaterial)
            End If

            If (chkBoxPurchasedPart.Checked) Then

                txtBoxVisMaterial.Text = accessMaterialName(0)
                txtBoxVisMaterialDescription.Text = accessMaterialDesc(0)
                txtBoxUnitOfMeasure.Text = accessUnitOfMeasure(0)

                If (txtBoxUnitOfMeasure.Text.Trim = "IN" And isRound) Then
                    txtBoxEachQuantity.Text = Math.Round((Convert.ToDouble(txtBoxRoundLength.Text) + 0.25), 2).ToString("N2")
                ElseIf (txtBoxUnitOfMeasure.Text.Trim = "EA") Then
                    txtBoxEachQuantity.Text = "1.00"
                End If
                '
            ElseIf isRound And chkBoxRawMaterial.Checked Then
                Dim wallThickness As Double = Nothing
                If (IsNothing(txtBoxInnerDiameter) = False And IsNothing(txtBoxDiameter) = False And comboxMaterial.Text.Contains("TUB") = True) Then
                    wallThickness = (Math.Round(((Convert.ToDouble(txtBoxDiameter.Text) - Convert.ToDouble(txtBoxInnerDiameter.Text)) / 2), 4))
                End If

                For i As Integer = 0 To count - 1
                    If (comboxMaterial.Text.Contains("TUBING") = False) Then
                        If Convert.ToDouble(accessMaterialDia(i)) <= Convert.ToDouble(accessMaterialDia(smallestDiameterIndex)) And Convert.ToDouble(accessMaterialDia(i)) > 0 Then
                            smallestDiameterIndex = i
                        End If
                    Else
                        If Convert.ToDouble(accessMaterialDia(i)) >= Convert.ToDouble(accessMaterialDia(largestDiameterIndex)) And Convert.ToDouble(accessMaterialDia(i)) > 0 Then
                            'If Math.Abs(wallthickness - Convert.ToDouble(accessWallThickness(largestDiameterIndex))) > Math.Abs(wallthickness - Convert.ToDouble(accessWallThickness(i)) Then
                            'If Convert.ToDouble(accessWallThickness(i)) <= Convert.ToDouble(accessWallThickness(largestDiameterIndex)) And Convert.ToDouble(accessWallThickness(i)) > 0 Then
                            largestDiameterIndex = i
                            'End If
                        End If
                    End If
                Next

                Dim tempDia = accessMaterialDia(largestDiameterIndex)

                If (comboxMaterial.Text.Contains("TUBING")) Then

                    For i As Integer = 0 To count - 1
                        If Convert.ToDouble(accessMaterialDia(i)) = tempDia Then

                            Dim n As String = Environment.NewLine
                            'MsgBox("best thickness " + Convert.ToDouble(accessWallThickness(largestDiameterIndex)).ToString + n + "Current thickness " + Convert.ToDouble(accessWallThickness(i)).ToString + n + "Best difference in wall thickness " + (Math.Abs(wallThickness - Convert.ToDouble(accessWallThickness(largestDiameterIndex)))).ToString + n + "Current difference in wall thickness: " + Math.Abs(wallThickness - Convert.ToDouble(accessWallThickness(i))).ToString)
                            If Math.Abs(wallThickness - Convert.ToDouble(accessWallThickness(largestDiameterIndex))) > Math.Abs(wallThickness - Convert.ToDouble(accessWallThickness(i))) Then
                                largestDiameterIndex = i
                            End If
                        End If
                    Next
                End If

                If (comboxMaterial.Text.Contains("TUBING") = False) Then
                    txtBoxEachQuantity.Text = Math.Round((Convert.ToDouble(txtBoxRoundLength.Text) + 0.25), 2).ToString("N2")
                    txtBoxVisMaterial.Text = accessMaterialName(smallestDiameterIndex)
                    txtBoxVisMaterialDescription.Text = accessMaterialDesc(smallestDiameterIndex)
                    txtBoxUnitOfMeasure.Text = accessUnitOfMeasure(smallestDiameterIndex)
                Else
                    txtBoxEachQuantity.Text = Math.Round((Convert.ToDouble(txtBoxRoundLength.Text) + 0.25), 2).ToString("N2")
                    txtBoxVisMaterial.Text = accessMaterialName(largestDiameterIndex)
                    txtBoxVisMaterialDescription.Text = accessMaterialDesc(largestDiameterIndex)
                    txtBoxUnitOfMeasure.Text = accessUnitOfMeasure(largestDiameterIndex)
                End If

            ElseIf isRound = False And chkBoxRawMaterial.Checked Then
                For i As Integer = 0 To count - 1
                    If (((Convert.ToDouble(accessMaterialWidth(i)) <= Convert.ToDouble(accessMaterialWidth(smallestWidthThicknessIndex)) And Convert.ToDouble(accessMaterialWidth(i)) > 0) Or (Convert.ToDouble(accessMaterialThickness(i)) <= Convert.ToDouble(accessMaterialThickness(smallestWidthThicknessIndex)) And Convert.ToDouble(accessMaterialThickness(i) > 0))) And (Convert.ToDouble(accessMaterialWidth(i)) * Convert.ToDouble(accessMaterialThickness(i)) < Convert.ToDouble(accessMaterialWidth(smallestWidthThicknessIndex)) * Convert.ToDouble(accessMaterialThickness(smallestWidthThicknessIndex)))) Then
                        smallestWidthThicknessIndex = i
                    End If
                Next

                If (accessUnitOfMeasure(smallestWidthThicknessIndex) = "LB") Then
                    txtBoxEachQuantity.Text = (Math.Round((Convert.ToDouble(txtBoxLength.Text) * Convert.ToDouble(txtBoxWidth.Text) * Convert.ToDouble(txtBoxThickness.Text) * density), 2)).ToString("N2")
                ElseIf (accessUnitOfMeasure(smallestWidthThicknessIndex) = "IN") Then
                    txtBoxEachQuantity.Text = Math.Round((Convert.ToDouble(txtBoxLength.Text) + 0.25), 2).ToString("N2")
                ElseIf (accessUnitOfMeasure(smallestWidthThicknessIndex) = "EA") Then
                    txtBoxEachQuantity.Text = "1.00"
                End If

                txtBoxVisMaterial.Text = accessMaterialName(smallestWidthThicknessIndex)
                txtBoxVisMaterialDescription.Text = accessMaterialDesc(smallestWidthThicknessIndex)
                txtBoxUnitOfMeasure.Text = accessUnitOfMeasure(smallestWidthThicknessIndex)
            End If
        End Function

        Public Function AutoCompleteVisData() As Boolean
            Dim accessMaterialName(0) As String
            Dim accessMaterialDesc(0) As String
            Dim accessMaterialDia(0) As String
            Dim accessMaterialLength(0) As String
            Dim accessMaterialThickness(0) As String
            Dim accessMaterialWidth(0) As String
            Dim accessMaterialShoulderThickness_InnerDia(0) As String
            Dim accessWallThickness(0) As String
            Dim accessUnitOfMeasure(0) As String
            Dim accessProdLine(0) As String

            ' We only need density for plates/blocks, that's why some materials arent' included here


            'Define the connectors
            Dim cn As OleDbConnection
            Dim cmd As OleDbCommand
            Dim dr As OleDbDataReader
            Dim oConnect, oQuery As String
            Dim FoundStatus As Boolean = False
            Dim isRound As Boolean = False

            'Define connection string
            'oConnect = "Provider= Microsoft.Jet.OLEDB.4.0;Data Source=" & FileName
            oConnect = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Y:\eng\ENG_ACCESS_DATABASES\VisibPartAttributes.mdb"
            oQuery = "SELECT * FROM VISIB_PARTMASTER_LOCAL where PARTNO='" & comboxMaterial.Text & "'"

            'If (isRound And chkBoxRawMaterial.Checked) Then
            '    If (comboxMaterial.Text = "AMPCO 18" And txtBoxInnerDiameter.Text <> "") Then
            '        oQuery = "SELECT * FROM RAW_MATERIALS WHERE MATERIAL_NAME Like'" & materialPartNumber & "H%' AND DIA = " & txtBoxDiameter.Text & " AND LENGTH > " & txtBoxRoundLength.Text & " AND WALL_THICKNESS = " & ((Convert.ToDouble(txtBoxDiameter.Text) - Convert.ToDouble(txtBoxInnerDiameter.Text)) / 2).ToString
            '    End If
            'ElseIf (chkBoxPurchasedPart.Checked) Then
            '    If (comboxMaterial.Text <> "AS SUPPLIED") Then
            '        oQuery = "SELECT * FROM VISIB_PARTMASTER_LOCAL where PARTNO='" & comboxMaterial.Text & "'"
            '    ElseIf (comboxMaterial.Text = "AS SUPPLIED") Then
            '        txtBoxVisMaterial.Text = "AS SUPPLIED"
            '        Exit Function
            '    End If
            'End If

            'Instantiate the connectors
            cn = New OleDbConnection(oConnect)
            cn.Open()

            cmd = New OleDbCommand(oQuery, cn)
            dr = cmd.ExecuteReader
            Dim count As Integer = 0

            Try
                While dr.Read()

                    accessMaterialName(count) = dr("PARTNO")
                    ReDim Preserve accessMaterialName(count + 1)

                    accessMaterialDesc(count) = dr("PARTDESCR")
                    ReDim Preserve accessMaterialDesc(count + 1)

                    accessProdLine(count) = dr("PRODUCT_LINE")
                    ReDim Preserve accessUnitOfMeasure(count + 1)

                    accessUnitOfMeasure(count) = dr("UM")
                    ReDim Preserve accessUnitOfMeasure(count + 1)


                    count += 1
                    FoundStatus = True
                End While
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try

            dr.Close()
            cn.Close()
            If FoundStatus = False Then
                FailureTimerTick(txtBoxVisMaterial)
                txtBoxVisMaterial.Text = ""
                txtBoxVisMaterialDescription.Text = ""
                txtBoxEachQuantity.Text = ""
                txtBoxTotalQuantity.Text = ""
                txtBoxUnitOfMeasure.Text = ""
                Return False
            Else

                SuccessTimerTick(txtBoxVisMaterial)
            End If

            If (accessMaterialDesc(0).Contains("DIA") Or accessMaterialDesc(0).Contains("ID") Or accessMaterialDesc(0).Contains("OD")) Then
                isRound = True

            End If
            If accessProdLine(0).Contains("RAW") Then
                chkBoxRawMaterial.checked = True

            Else
                chkBoxPurchasedPart.checked = True

            End If
            'MsgBox(accessMaterialDesc(0))
            'MsgBox(accessMaterialName(0))
            'MsgBox(accessUnitOfMeasure(0))
            'MsgBox(accessProdLine(0))


            If (chkBoxPurchasedPart.Checked) Then
                txtBoxVisMaterial.Text = accessMaterialName(0)
                txtBoxVisMaterialDescription.Text = accessMaterialDesc(0)
                txtBoxUnitOfMeasure.Text = accessUnitOfMeasure(0)

                If (txtBoxUnitOfMeasure.Text.Trim = "IN" And isRound = True) Then
                    txtBoxEachQuantity.Text = Math.Round((Convert.ToDouble(txtBoxRoundLength.Text) + 0.25), 2).ToString("N2")
                ElseIf (txtBoxUnitOfMeasure.Text.Trim = "EA") Then
                    txtBoxEachQuantity.Text = "1.00"
                End If

            ElseIf isRound And chkBoxRawMaterial.Checked Then
                Dim wallThickness As Double = Nothing
                'If (IsNothing(txtBoxInnerDiameter) = False And IsNothing(txtBoxDiameter) = False And (comboxMaterial.Text.Contains("TUB") = True Or comboxMaterial.Text.Contains("TSH") = True Or comboxMaterial.Text.Contains("TSS") = True)) Then
                '    wallThickness = (Math.Round(((Convert.ToDouble(txtBoxDiameter.Text) - Convert.ToDouble(txtBoxInnerDiameter.Text)) / 2), 4))
                'End If

                'Dim tempDia = accessMaterialDia(largestDiameterIndex)


                Try
                    txtBoxEachQuantity.Text = Math.Round((Convert.ToDouble(txtBoxRoundLength.Text) + 0.25), 2).ToString("N2")
                Catch ex As Exception
                    'MsgBox(ex.ToString)
                End Try
                txtBoxVisMaterial.Text = accessMaterialName(0)
                txtBoxVisMaterialDescription.Text = accessMaterialDesc(0)
                txtBoxUnitOfMeasure.Text = accessUnitOfMeasure(0)


            ElseIf isRound = False And chkBoxRawMaterial.Checked Then

                If (accessUnitOfMeasure(0) = "LB") Then
                    txtBoxEachQuantity.Text = (Math.Round((Convert.ToDouble(txtBoxLength.Text) * Convert.ToDouble(txtBoxWidth.Text) * Convert.ToDouble(txtBoxThickness.Text) * density), 2)).ToString("N2")
                ElseIf (accessUnitOfMeasure(0) = "IN") Then
                    txtBoxEachQuantity.Text = Math.Round((Convert.ToDouble(txtBoxLength.Text) + 0.25), 2).ToString("N2")
                ElseIf (accessUnitOfMeasure(0) = "EA") Then
                    txtBoxEachQuantity.Text = "1.00"
                End If

                txtBoxVisMaterial.Text = accessMaterialName(0)
                txtBoxVisMaterialDescription.Text = accessMaterialDesc(0)
                txtBoxUnitOfMeasure.Text = accessUnitOfMeasure(0)
            End If
        End Function
        Private Function ReadSpecialPartNum(ByVal FileName As String, ByVal PartNum As String, ByRef VisPartDesc As String, ByRef VisComCode As String, ByRef VisDwgNum As String) As Boolean
            'Define the connectors
            Dim cn As OleDbConnection
            Dim cmd As OleDbCommand
            Dim dr As OleDbDataReader
            Dim oConnect, oQuery As String
            Dim FoundStatus As Boolean = False

            'Define connection string
            'oConnect = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & FileName
            oConnect = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & FileName

            'Query String
            oQuery = "SELECT * FROM SPECIAL_PARTMASTER where PARTNO='" & PartNum & "'"

            'Instantiate the connectors
            cn = New OleDbConnection(oConnect)

            Try
                cn.Open()
            Catch ex As Exception
                oConnect = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & FileName
            Finally
                cn = New OleDbConnection(oConnect)
                cn.Open()
            End Try

            cmd = New OleDbCommand(oQuery, cn)
            dr = cmd.ExecuteReader

            While dr.Read()
                VisPartDesc = dr("PARTDESCR")
                VisPartDesc = VisPartDesc.Trim()

                VisComCode = dr("COMMODITY_CODE")
                VisComCode = VisComCode.Trim()

                VisDwgNum = dr("DRAWING_NO")
                VisDwgNum = VisDwgNum.Trim()

                FoundStatus = True
            End While

            dr.Close()
            cn.Close()

            Return FoundStatus
        End Function
        Private Function CheckVisData(ByVal myPartNum As String, ByRef VisPartDesc As String, ByRef VisComCode As String, ByRef VisDwgNum As String) As Boolean
            If File.Exists("Y:\eng\ENG_ACCESS_DATABASES\VisibPartAttributes.mdb") = False Then
                MsgBox("Database File does not exist.  Please contact administrator.")
                Exit Function
            End If
            Return ReadVisData("Y:\eng\ENG_ACCESS_DATABASES\VisibPartAttributes.mdb", myPartNum, VisPartDesc, VisComCode, VisDwgNum)
        End Function
        Public Sub GetPreviousDimensions()
            Try
                If (txtBoxDiameter.Text = "" And txtBoxRoundLength.Text = "" And txtBoxWidth.Text = "" And txtBoxLength.Text = "" And txtBoxThickness.Text = "") Then
                    If txtBoxFullDimensions.Text <> "" Then
                        If txtBoxFullDimensions.Text.Contains("DIA") Then
                            ' Filter out dimension 
                            Dim tempDia As String = Nothing
                            Dim tempRoundLength As String = Nothing

                            Dim splitIndex = txtBoxFullDimensions.Text.ToUpper.IndexOf("X")
                            If (splitIndex > -1) Then
                                tempDia = txtBoxFullDimensions.Text.Substring(0, splitIndex)
                                tempDia = tempDia.ToUpper.Replace("DIA", "")
                                tempDia = tempDia.Trim()
                                tempRoundLength = txtBoxFullDimensions.Text.Substring(splitIndex + 1, txtBoxFullDimensions.Text.Length - splitIndex - 1)
                                tempRoundLength = tempRoundLength.ToUpper.Replace("X", "")
                                tempRoundLength = tempRoundLength.Trim()
                                txtBoxDiameter.Text = tempDia
                                txtBoxRoundLength.Text = tempRoundLength
                            End If
                        Else
                            Dim tempLength As String = Nothing
                            Dim tempWidth As String = Nothing
                            Dim tempThickness As String = Nothing
                            Dim myTempMaterialSize As String = Nothing

                            Dim splitIndex = txtBoxFullDimensions.Text.ToUpper.IndexOf("X") ' Splitting thickness and width
                            If (splitIndex > -1) Then
                                tempThickness = txtBoxFullDimensions.Text.Substring(0, splitIndex)
                                tempThickness = tempThickness.Trim()
                                myTempMaterialSize = txtBoxFullDimensions.Text.Substring(splitIndex + 1)
                                splitIndex = myTempMaterialSize.ToUpper.IndexOf("X")
                                If (splitIndex > -1) Then
                                    tempWidth = myTempMaterialSize.Substring(0, splitIndex - 1)
                                    tempWidth = tempWidth.Trim()
                                    tempLength = myTempMaterialSize.Substring(splitIndex + 1, myTempMaterialSize.Length - splitIndex - 1)
                                    tempLength = tempLength.Trim()
                                End If

                                txtBoxLength.Text = tempLength
                                txtBoxWidth.Text = tempWidth
                                txtBoxThickness.Text = tempThickness
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
            End Try
        End Sub
        Private Function checkQuantity()
            If (txtBoxQty.Text = "XX" Or txtBoxQty.Text = "" Or IsNumeric(txtBoxQty.Text) = False) Then
                FailureTimerTick(txtBoxQty)
                Return False
            End If
            Return True
        End Function
        Private Function checkRawMaterialSearchInputs()

            If (checkQuantity() = False) Then
                Return False
            End If

            If (comboxMaterial.Items.Contains(comboxMaterial.Text.Trim) = False) Then
                FailureTimerTick(comboxMaterial)
                Return False
            End If

            If (comboxMaterial.Text = "XX" Or comboxMaterial.Text = "") Then
                FailureTimerTick(comboxMaterial)
                Return False
            End If

            If txtBoxLength.Text = "" And txtBoxWidth.Text = "" And txtBoxThickness.Text = "" And txtBoxDiameter.Text = "" And txtBoxRoundLength.Text = "" Then
                MsgBox("Please Enter Dimensions")
                Return False
            End If

            If (txtBoxLength.Text = "" And txtBoxDiameter.Text = "" And txtBoxRoundLength.Text = "") Then
                FailureTimerTick(txtBoxLength)
                Return False
            End If

            If (txtBoxWidth.Text = "" And txtBoxDiameter.Text = "" And txtBoxRoundLength.Text = "") Then
                FailureTimerTick(txtBoxWidth)
                Return False
            End If

            If (txtBoxThickness.Text = "" And txtBoxDiameter.Text = "" And txtBoxRoundLength.Text = "") Then
                FailureTimerTick(txtBoxThickness)
                Return False
            End If

            If (txtBoxDiameter.Text = "" And txtBoxLength.Text = "" And txtBoxWidth.Text = "" And txtBoxThickness.Text = "") Then
                FailureTimerTick(txtBoxDiameter)
                Return False
            End If

            If (txtBoxRoundLength.Text = "" And txtBoxLength.Text = "" And txtBoxWidth.Text = "" And txtBoxThickness.Text = "") Then
                FailureTimerTick(txtBoxRoundLength)
                Return False
            End If

            If (comboxMaterial.Text.Contains("MOLD TAG")) Then
                Return True
            End If

            If ((comboxMaterial.Text.Contains("TUBING") Or comboxMaterial.Text = "AMPCO 18" Or comboxMaterial.Text = "TUBING (ALUMINUM)" Or comboxMaterial.Text = "TUBING (BRONZE)" Or comboxMaterial.Text = "TUBING (BRASS)" Or comboxMaterial.Text = "TUBING HONED" Or comboxMaterial.Text = "TUBING (SS)" Or comboxMaterial.Text = "PIN,EJECT,N,SH") And txtBoxInnerDiameter.Text = "") Then
                FailureTimerTick(txtBoxInnerDiameter)
                Return False
            End If

            Return True
        End Function
        Private Function checkPurchasedPartSearchInputs()
            If (checkQuantity() = False) Then
                Return False
            End If

            If (comboxMaterial.Text = "XX" Or comboxMaterial.Text = "") Then
                FailureTimerTick(comboxMaterial)
                Return False
            End If

            Return True
        End Function
        Private Sub btnAutoAssign_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAutoAssign.Click
            GetUserInfo(txtBoxDesigner.Text, txtBoxTeam.Text, Nothing)
            GetDate(txtBoxDate.Text)
            txtBoxJobNo.Text = s.Parts.Work.FullPath().Substring(0, 5)
        End Sub
        Private Sub btnChangePartNum_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnChangePartNum.Click

            Dim myNewPartNum As String = Nothing
            myNewPartNum = NXInputBox.GetInputString("My New Part Number", "Change Part Number")

            If (myNewPartNum = txtBoxPartNo.Text.Trim Or myNewPartNum = "") Then
                Exit Sub
            End If

            Dim myVisDesc As String = Nothing
            Dim myVisDwgNum As String = Nothing
            Dim myVisComCode As String = Nothing

            If CheckMDMPart() Then
                txtBoxPartNo.Text = myNewPartNum
                txtBoxVisDesc.Text = "N/A"
                txtBoxVisDwgNo.Text = "N/A"
                txtBoxVisComCode.Text = "N/A"
            Else
                If (myNewPartNum.Trim.StartsWith("00")) Then
                    txtBoxPartNo.Text = myNewPartNum
                    txtBoxVisDesc.Text = ""
                    txtBoxVisDwgNo.Text = ""
                    txtBoxVisComCode.Text = ""
                    SuccessTimerTick(txtBoxPartNo)
                Else
                    If CheckVisData(myNewPartNum, myVisDesc, myVisComCode, myVisDwgNum) Then
                        SuccessTimerTick(txtBoxPartNo)
                        txtBoxVisDesc.Text = myVisDesc
                        txtBoxDesc.Text = myVisDesc
                        If myVisDwgNum = "" Or myVisDwgNum = Nothing Then
                            myVisDwgNum = "  "
                            MsgBox("myVisDwgNum set to 1 whitespace ")
                        End If
                        txtBoxVisDwgNo.Text = myVisDwgNum
                        txtBoxVisComCode.Text = myVisComCode
                        txtBoxPartNo.Text = myNewPartNum
                    End If
                End If
            End If
        End Sub

        Private Sub MaterialButton_Click(sender As Object, e As EventArgs) Handles MaterialButton.Click
            If (comboxMaterial.Text.Contains("- - - - -")) Then
                FailureTimerTick(comboxMaterial)
                Exit Sub
            End If

            Dim isRound As Boolean = True
            If (txtBoxDiameter.Text = "" And txtBoxRoundLength.Text = "") Then
                isRound = False
            End If

            If chkBoxRawMaterial.Checked Then
                If (comboxMaterial.Text <> "- - OTHER - -") Then
                    If (checkRawMaterialSearchInputs() = False) Then
                        Exit Sub
                    End If
                ElseIf (comboxMaterial.Text = "- - OTHER - -") Then
                    MsgBox("Please enter your material into the notes for now, you do not need to press MaterialButton")
                    Exit Sub
                End If

                Dim materialPartNumber As String = Nothing

                If (comboxMaterial.Text = "4140") Then
                    materialPartNumber = "4140"
                ElseIf (comboxMaterial.Text = "A-2") Then
                    materialPartNumber = "A02"
                ElseIf (comboxMaterial.Text = "A-10" And isRound = True) Then
                    materialPartNumber = "A10"
                ElseIf (comboxMaterial.Text = "A-10" And isRound = False) Then
                    materialPartNumber = "A10BLK"
                ElseIf (comboxMaterial.Text = "ALU-BAR") Then
                    materialPartNumber = "ALN"
                ElseIf (comboxMaterial.Text = "ALU-CHANNEL") Then
                    materialPartNumber = "ALC"
                ElseIf (comboxMaterial.Text = "ALU-PLATE") Then
                    materialPartNumber = "ALU"
                ElseIf (comboxMaterial.Text = "ALU-PLATE (AIR)") Then
                    materialPartNumber = "ALUPLATEACG"
                ElseIf (comboxMaterial.Text = "AMPCO 18") Then
                    materialPartNumber = "AM"
                ElseIf (comboxMaterial.Text = "AMPCO 21" And isRound = True) Then
                    materialPartNumber = "AMP21"
                ElseIf (comboxMaterial.Text = "AMPCO 21" And isRound = False) Then
                    materialPartNumber = "AMPPLATE21"
                ElseIf (comboxMaterial.Text = "AMPCO 22") Then
                    materialPartNumber = "AMP22"
                ElseIf (comboxMaterial.Text = "AMPCO 25") Then
                    materialPartNumber = "AMP25"
                ElseIf (comboxMaterial.Text = "B-25") Then
                    materialPartNumber = "B25"
                ElseIf (comboxMaterial.Text = "BRASS") Then
                    materialPartNumber = "BRA"
                ElseIf (comboxMaterial.Text = "BRASS (HEX BAR)") Then
                    materialPartNumber = "BRH"
                ElseIf (comboxMaterial.Text = "BRONZE" And isRound) Then
                    materialPartNumber = "BRS"
                ElseIf (comboxMaterial.Text = "BRONZE" And isRound = False) Then
                    materialPartNumber = "BRZ"
                ElseIf (comboxMaterial.Text = "CALDIE" And isRound) Then
                    materialPartNumber = "CALDIE"
                ElseIf (comboxMaterial.Text = "COPPER") Then
                    materialPartNumber = "CPP"
                ElseIf (comboxMaterial.Text = "CRS") Then
                    materialPartNumber = "CRS"
                ElseIf (comboxMaterial.Text = "D2") Then
                    materialPartNumber = "D02"
                ElseIf (comboxMaterial.Text = "FP 100") Then
                    materialPartNumber = "FP1P"
                ElseIf (comboxMaterial.Text = "H13" Or comboxMaterial.Text = "H13-SUPREME" Or comboxMaterial.Text = "DIEVAR") Then
                    materialPartNumber = "H13"
                ElseIf (comboxMaterial.Text = "ORVAR") Then
                    materialPartNumber = "H13ORVPLT"
                ElseIf (comboxMaterial.Text = "ORVAR SUPERIOR") Then
                    materialPartNumber = "H13ORVSUPERPL"
                ElseIf (comboxMaterial.Text = "H13-PRE SUPERIOR") Then
                    materialPartNumber = "H13PRESUP3638"
                ElseIf (comboxMaterial.Text = "HOVADUR") Then
                    materialPartNumber = "HOVADUR"
                ElseIf (comboxMaterial.Text = "KEY STOCK") Then
                    materialPartNumber = "KS0"
                ElseIf (comboxMaterial.Text = "M2") Then
                    materialPartNumber = "M2"
                ElseIf (comboxMaterial.Text = "M314") Then
                    materialPartNumber = "M314PLATE"
                ElseIf (comboxMaterial.Text = "MIRRAX") Then
                    materialPartNumber = "MIRBLK"
                ElseIf (comboxMaterial.Text = "MIRRAX 40") Then
                    materialPartNumber = "MIR40BLK"
                ElseIf (comboxMaterial.Text = "MOLDMAXV") Then
                    materialPartNumber = "MOLDMAXV"
                ElseIf (comboxMaterial.Text = "MOLD TAG GENERAL") Then
                    materialPartNumber = "16749"
                ElseIf (comboxMaterial.Text = "MOLD TAG QPC") Then
                    materialPartNumber = "16729"
                ElseIf (comboxMaterial.Text = "NAK 55") Then
                    materialPartNumber = "N55"
                ElseIf (comboxMaterial.Text = "NIMAX") Then
                    materialPartNumber = "NIMAX"
                ElseIf (comboxMaterial.Text = "NITRALLOY") Then
                    materialPartNumber = "NITR"
                ElseIf (comboxMaterial.Text = "NYLON BAR") Then
                    materialPartNumber = "NYL0"
                ElseIf (comboxMaterial.Text = "NYLATRON") Then
                    materialPartNumber = "NYLP"
                ElseIf (comboxMaterial.Text = "O-1") Then
                    materialPartNumber = "O10"
                ElseIf (comboxMaterial.Text = "O-6") Then
                    materialPartNumber = "O6"
                ElseIf (comboxMaterial.Text = "P-6") Then
                    materialPartNumber = "P06"
                ElseIf (comboxMaterial.Text = "P-20 IMPAX") Then
                    materialPartNumber = "P20PL-IMPAX"
                ElseIf (comboxMaterial.Text <> "P-20 IMPAX" And comboxMaterial.Text.Contains("P-20")) Then
                    materialPartNumber = "P20" ' special case
                ElseIf (comboxMaterial.Text = "POLY-URETHANE") Then
                    materialPartNumber = "PLY"
                ElseIf (comboxMaterial.Text = "PROTHERM") Then
                    materialPartNumber = "PRO"
                ElseIf (comboxMaterial.Text = "QRO-90") Then
                    materialPartNumber = "QRO90"
                ElseIf (comboxMaterial.Text = "RAMAX (ROUND)") Then
                    materialPartNumber = "RAM"
                ElseIf (comboxMaterial.Text = "S-1") Then
                    materialPartNumber = "S01"
                ElseIf (comboxMaterial.Text = "S-7") Then
                    materialPartNumber = "S07"
                ElseIf (comboxMaterial.Text = "SS303") Then
                    materialPartNumber = "S303"
                ElseIf (comboxMaterial.Text = "SS304") Then
                    materialPartNumber = "S304"
                ElseIf (comboxMaterial.Text = "S420 FM" And txtBoxVisDesc.Text.ToUpper.Contains("PLATE") = False) Then
                    materialPartNumber = "S420FMBLK"
                ElseIf (comboxMaterial.Text = "S420 FM") Then
                    materialPartNumber = "S420FM"
                ElseIf (comboxMaterial.Text = "SS420" Or comboxMaterial.Text = "SS420 SUPREME") Then
                    materialPartNumber = "S42"
                ElseIf (comboxMaterial.Text = "TELCO") Then
                    materialPartNumber = "TELCO"
                ElseIf (comboxMaterial.Text = "TEFLON") Then
                    materialPartNumber = "TFL"
                ElseIf (comboxMaterial.Text.Contains("THRD")) Then
                    materialPartNumber = "THR"
                ElseIf (comboxMaterial.Text = "TITANIUM 5") Then
                    materialPartNumber = "TIT"
                ElseIf (comboxMaterial.Text = "TIVAR") Then
                    materialPartNumber = "TIV0"
                ElseIf (comboxMaterial.Text = "TOUGHMET") Then
                    materialPartNumber = "TOUGHMET"
                ElseIf (comboxMaterial.Text = "TUBING (ALUMINUM)") Then
                    materialPartNumber = "TAL0150187"
                ElseIf (comboxMaterial.Text = "TUBING (BRONZE)") Then
                    materialPartNumber = "TBR0"
                ElseIf (comboxMaterial.Text = "TUBING (BRASS)") Then
                    materialPartNumber = "TBRA"
                ElseIf (comboxMaterial.Text = "TUBING HONED") Then
                    materialPartNumber = "TSH0"
                ElseIf (comboxMaterial.Text = "TUBING (SS)") Then
                    materialPartNumber = "TSS"
                ElseIf (comboxMaterial.Text = "VANCRON40") Then
                    materialPartNumber = "VANCRON40"
                ElseIf (comboxMaterial.Text = "VANCRON50") Then
                    materialPartNumber = "VANCRON50"
                ElseIf (comboxMaterial.Text = "VISCOUNT") Then
                    materialPartNumber = "VIS"
                End If

                ReadVisDataForMaterial(materialPartNumber, isRound)

            ElseIf (chkBoxPurchasedPart.Checked) Then
                If (checkPurchasedPartSearchInputs() = False) Then
                    Exit Sub
                End If

                If (comboxMaterial.Text = "- - OTHER - -") Then
                    MsgBox("Please enter your material into the notes for now, you do not need to press MaterialButton")
                    Exit Sub
                End If

                ReadVisDataForMaterial(Nothing, isRound)
            End If
        End Sub
        Public Function checkFormBeforeSubmit() As Boolean

            If (comboxMaterial.Text.Contains("- - OTHER - -")) Then
                Return True
            End If

            If (comboxMaterial.Text.Contains("- - - - -")) Then
                FailureTimerTick(comboxMaterial)
                Return False
            End If

            If (chkBoxRawMaterial.Checked And comboxMaterial.Text = "") Then
                FailureTimerTick(comboxMaterial)
                Return False
            End If

            If ((chkBoxRawMaterial.Checked Or chkBoxPurchasedPart.Checked) And txtBoxVisMaterial.Text = "") Then
                FailureTimerTick(MaterialButton)
                Return False
            End If
            Return True
        End Function
        Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click

            Try
                If (txtBoxSpareQty.Text.Trim = "") Then
                    txtBoxSpareQty.Text = "0"
                End If
            Catch ex As Exception
                txtBoxSpareQty.Text = "0"
            End Try

            If (chkBoxSkipMaterialSelection.Checked = False) Then
                If (checkFormBeforeSubmit() = False) Then
                    Exit Sub
                End If
            End If

            Try
                UpdateLogs()
            Catch ex As Exception
                lw.WriteLine("Unable to log information to database, please contact system admin. ")
            End Try


            Try
                SetAttribute("TITBLK_DESIGNER", txtBoxDesigner.Text)
                SetAttribute("TITBLK_TEAM", txtBoxTeam.Text)
                SetAttribute("TITBLK_DATE", txtBoxDate.Text)
                SetAttribute("TITBLK_CHECKER", txtBoxChecker.Text)
                SetAttribute("TITBLK_DWGSCALE", txtBoxScale.Text)
                SetAttribute("STACKTECK_PROJNUM", txtBoxJobNo.Text)


                'Set Part Info Attribute
                SetAttribute("STACKTECK_PARTN", txtBoxPartNo.Text.ToUpper)
                SetAttribute("STACKTECK_DESC", txtBoxDesc.Text)
                SetAttribute("STACKTECK_QTY", txtBoxQty.Text)
                SetAttribute("STACKTECK_SPAREQTY", txtBoxSpareQty.Text)

                'Set Vis Info Attribute
                SetAttribute("STACKTECK_VISDESC", txtBoxVisDesc.Text)
                SetAttribute("STACKTECK_VISCOMCODE", txtBoxVisComCode.Text)
                SetAttribute("STACKTECK_VISDWGNUM", txtBoxVisDwgNo.Text) ' Sending in blanks results in an error, finding a cheap way  out for now
                SetAttribute("STACKTECK_VISNUM", "")

                SetAttribute("STACKTECK_IS_RAW_MATERIAL", chkBoxRawMaterial.Checked.ToString)
                SetAttribute("STACKTECK_IS_PURCHASED_COMPONENT", chkBoxPurchasedPart.Checked.ToString)

                If txtBoxVisDwgNo.text = Nothing Then
                    txtBoxVisDwgNo.text = " "
                End If
                If (chkBoxSkipMaterialSelection.Checked = False) Then

                    If (comboxMaterial.Text <> "- - OTHER - -") Then
                        If chkBoxRawMaterial.Checked Then
                            Try
                                txtBoxTotalQuantity.Text = (Convert.ToDouble(txtBoxQty.Text) * Convert.ToDouble(txtBoxEachQuantity.Text)).ToString("N2")
                            Catch ex As Exception
                            End Try
                        ElseIf (chkBoxPurchasedPart.Checked) Then
                            Try
                                txtBoxTotalQuantity.Text = Math.Round(Convert.ToDouble(txtBoxQty.Text), 2).ToString("N2")
                            Catch ex As Exception
                            End Try
                        End If
                    End If

                    If (comboxMaterial.Text <> "- - OTHER - -" And chkBoxRawMaterial.checked = True) Then
                        SetAttribute("STACKTECK_MATERIAL", txtBoxVisMaterial.Text)

                    ElseIf (comboxMaterial.text <> "- - OTHER - -") Then
                        SetAttribute("STACKTECK_MATERIAL", comboxMaterial.Text)
                    Else
                        SetAttribute("STACKTECK_MATERIAL", txtBoxNotes.Text)
                    End If


                    SetAttribute("STACKTECK_HARDNESS", comboxMaterialHdn.Text)
                    SetAttribute("STACKTECK_PART_DIAMETER", txtBoxDiameter.Text)
                    SetAttribute("STACKTECK_PART_INNER_DIAMETER", txtBoxInnerDiameter.Text)
                    SetAttribute("STACKTECK_PART_LENGTH", txtBoxLength.Text)
                    SetAttribute("STACKTECK_PART_ROUND_LENGTH", txtBoxRoundLength.Text)
                    SetAttribute("STACKTECK_PART_WIDTH", txtBoxWidth.Text)
                    SetAttribute("STACKTECK_PART_THICKNESS", txtBoxThickness.Text)

                    'BOM Attributes
                    SetAttribute("STACKTECK_COMPONENT_PART", txtBoxVisMaterial.Text)
                    SetAttribute("STACKTECK_EACH_QUANTITY", txtBoxEachQuantity.Text)
                    SetAttribute("STACKTECK_TOTAL_QUANTITY", txtBoxTotalQuantity.Text)
                    SetAttribute("STACKTECK_UNIT_OF_MEASUREMENT", txtBoxUnitOfMeasure.Text)
                    SetAttribute("STACKTECK_COMPONENT_DESCRIPTION", txtBoxVisMaterialDescription.Text)
                    txtBoxPrefix.text = txtBoxPrefix.text.toUpper
                    Try
                        If txtBoxPrefix.text <> "" And txtBoxPrefix.text <> "." And txtBoxFullDimensions.text.contains(txtBoxPrefix.text) = False Then
                            txtBoxFullDimensions.text = txtBoxPrefix.text + " " + txtBoxFullDimensions.text
                        End If
                        Dim count As Integer = 0
                        Dim tempStr As String = Nothing
                        If txtBoxPrefix.text.trim = "" Then
                            Do
                                If IsNumeric(txtBoxFullDimensions.text(0)) Or txtBoxFullDimensions.text(0) = "." Then
                                    Exit Do
                                End If
                                'MsgBox(txtBoxFullDimensions.text(count))
                                tempStr = txtBoxFullDimensions.text
                                tempStr = tempStr.Remove(0, 1)
                                txtBoxFullDimensions.text = tempStr
                                count = +1
                            Loop Until count > 10
                        End If
                    Catch ex As Exception
                        'MsgBox(ex.ToString)
                    End Try
                    'MsgBox(txtBoxFullDimensions.text)
                    If (chkBoxPurchasedPart.Checked And txtBoxDiameter.Text.Trim = "" And txtBoxRoundLength.Text.Trim = "" And txtBoxInnerDiameter.Text.Trim = "" And txtBoxThickness.Text.Trim = "" And txtBoxWidth.Text.Trim = "" And txtBoxLength.Text.Trim = "") Then
                        SetAttribute("STACKTECK_PARTSIZE", "AS SUPPLIED") ' Size
                    Else
                        SetAttribute("STACKTECK_PARTSIZE", txtBoxFullDimensions.Text) ' Size
                    End If

                    SetAttribute("STACKTECK_BOM_NOTES", txtBoxNotes.Text)
                End If
            Catch ex As Exception
                MsgBox(ex.ToString + Environment.NewLine + "This file is probably locked in teamcenter")
            End Try

            If (comboxMaterial.Text = "- - OTHER - -") Then
                sendEmail(Nothing)
            End If

            Me.Close()
            Me.Dispose()
        End Sub

        Public Sub sendEmail(ByVal body As String)
            Dim adminEmails As String = Nothing

            GetAdminEmails(adminEmails)

            Dim issueEmail As New System.Net.Mail.MailMessage
            Dim smtp_server As New System.Net.Mail.SmtpClient

            Dim emailBody As String = Nothing

            issueEmail.IsBodyHtml = True
            smtp_server.EnableSsl = False
            smtp_server.Host = "192.168.0.47"

            Dim tempFrom As String = Nothing
            tempFrom = System.Environment.UserName & "@stackteck.com" 'from field, is editable
            'ToBox.Text = System.Environment.UserName & "@stackteck.com"  'to field 

            'exception for usernames
            '-------------------------------------------------------------------

            If System.Environment.UserName = "htang" Or System.Environment.UserName = "henryt" Then
                tempFrom = "htang@stackteck.com"
            End If

            If System.Environment.UserName = "JOHN" Or System.Environment.UserName = "john" Then
                tempFrom = "jbrtka@stackteck.com"
            End If

            If System.Environment.UserName = "susan" Or System.Environment.UserName = "SUSAN" Then
                tempFrom = "shiltz@stackteck.com"
            End If

            If System.Environment.UserName = "wei" Or System.Environment.UserName = "WEI" Then
                tempFrom = "wyuan@stackteck.com"
            End If
            If System.Environment.UserName.ToLower = "mandeep" Or System.Environment.UserName.ToLower = "mthandi" Then
                tempFrom = "mthandi@stackteck.com"
            End If

            issueEmail.From = New System.Net.Mail.MailAddress(tempFrom)

            If (adminEmails.Trim <> "") Then
                issueEmail.To.Add(adminEmails + ",mimran@stackteck.com") ' Remember to update Y:\eng\ENG_ACCESS_DATABASES\UGMisAttributes.mdb admin_emails table with your e-mail!
            Else
                issueEmail.To.Add("rnaveed@stackteck.com")
            End If

            If (IsNothing(body)) Then

                issueEmail.Subject = System.Environment.UserName + " requested new part - (Other) - AssignDatabaseAttributes" ' Modify this line to be specific to the program

                ' For UG Specific programs, uncomment this

                Dim filePath As String = Nothing

                Try
                    Dim s As Session = Session.GetSession
                    filePath = s.Parts.Work.FullPath() ' e.g. aim_stackcup24oz_s37452/001
                Catch ex As Exception
                End Try

                emailBody = emailBody + "User Name: " + System.Environment.UserName + "<br><br>"
                emailBody = emailBody + "Material Requested: " + txtBoxNotes.text + "<br><br>"
                emailBody = emailBody + "PartNumber: " + txtBoxPartNo.text + "<br><br>"
                emailBody = emailBody + "Filename: " + filePath + "<br><br>"

                If chkBoxPurchasedPart.checked = True Then
                    emailBody = emailBody + "Purchase Part"
                Else
                    emailBody = emailBody + "Raw material"
                End If

                issueEmail.Body = emailBody

                smtp_server.Send(issueEmail)

            End If
        End Sub

        Public Sub GetAdminEmails(ByRef adminEmails As String)

            'Define the connectors
            Dim cn As OleDbConnection
            Dim cmd As OleDbCommand
            Dim dr As OleDbDataReader
            Dim oConnect, oQuery As String
            Dim FoundStatus As Boolean = False

            'Define connection string
            Dim FileName As String = "Y:\eng\ENG_ACCESS_DATABASES\UGMisDatabase.mdb"
            If File.Exists(FileName) = False Then
                Exit Sub
            End If

            oConnect = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & FileName

            'Query String
            oQuery = "SELECT * FROM ADMIN_EMAILS"

            'Instantiate the connectors
            cn = New OleDbConnection(oConnect)
            cn.Open()

            cmd = New OleDbCommand(oQuery, cn)
            dr = cmd.ExecuteReader

            While dr.Read()
                adminEmails += dr(1).Trim
                adminEmails += ", "
                FoundStatus = True
            End While

            adminEmails = adminEmails.Substring(0, adminEmails.LastIndexOf(","))

            dr.Close()
            cn.Close()
        End Sub



        Public Sub UpdateLogs()
            Dim Conn As New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Y:\eng\ENG_ACCESS_DATABASES\MaterialSelectionLogs.mdb; User Id=admin")
            Dim Com As OleDbCommand

            Dim sql = "INSERT INTO MATERIAL_SELECTION_LOGS (FILENAME, USERID, DADATE, DATIME, STK_PARTN, STK_DESC, STK_QTY, STK_SPQTY, STK_VISDESC,STK_MATERIAL_GENERIC,STK_PARTDIA,STK_RNDLEN,STK_INNDIA,STK_LEN,STK_WID,STK_THK,STK_NOTES, STK_MATERIAL_ACTUAL) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)"

            Com = New OleDbCommand(sql, Conn)
            Conn.Open()

            'If (txtBoxComments.Text.Length > 255) Then
            '    txtBoxComments.Text = txtBoxComments.Text.SubString(0, 255)
            'End If

            ' Step 0. Each top level assembly
            Try

                Com = New OleDbCommand(sql, Conn)
                Com.Parameters.AddWithValue("@p1", s.Parts.Work.FullPath())
                Com.Parameters.AddWithValue("@p2", System.Environment.UserName)
                Com.Parameters.AddWithValue("@p3", DateTime.Now.ToString("MMM-dd-yyyy"))
                Com.Parameters.AddWithValue("@p4", DateTime.Now.ToString("HH:mm"))
                Com.Parameters.AddWithValue("@p5", txtBoxPartNo.Text)
                Com.Parameters.AddWithValue("@p6", txtBoxDesc.Text)
                Com.Parameters.AddWithValue("@p7", txtBoxQty.Text)
                Com.Parameters.AddWithValue("@p8", txtBoxSpareQty.Text)
                Com.Parameters.AddWithValue("@p9", txtBoxVisDesc.Text)
                Com.Parameters.AddWithValue("@p10", comboxMaterial.Text)
                Com.Parameters.AddWithValue("@p11", txtBoxDiameter.Text)
                Com.Parameters.AddWithValue("@p12", txtBoxRoundLength.Text)
                Com.Parameters.AddWithValue("@p13", txtBoxInnerDiameter.Text)
                Com.Parameters.AddWithValue("@p14", txtBoxLength.Text)
                Com.Parameters.AddWithValue("@p15", txtBoxWidth.Text)
                Com.Parameters.AddWithValue("@p16", txtBoxThickness.Text)
                Com.Parameters.AddWithValue("@p17", txtBoxNotes.Text)
                Com.Parameters.AddWithValue("@p18", txtBoxVisMaterial.Text)
                Com.ExecuteNonQuery()
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try
            Conn.Close()
        End Sub

        Private Sub helpButton_Click(sender As Object, e As EventArgs) Handles helpButton.Click
            Try
                System.Diagnostics.Process.Start("\\cntfiler\_JobLib\EngProcedures\UG\ENGA70T - Assign Database Attribute Program in UG.docx")
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try
        End Sub
        Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
            Timer1.Stop()
            lastObjectChanged.BackColor = Color.Gold
            Timer2.Interval = 5000
            Timer2.Start()
        End Sub
        Private Sub Timer2_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer2.Tick
            Timer2.Stop()
            lastObjectChanged.BackColor = Color.Empty
        End Sub
        Private Sub SuccessTimerTick(ByVal changedObject As Object)
            changedObject.BackColor = Color.LightGreen
            lastObjectChanged = changedObject
            Timer1.Interval = 400
            Timer1.Start()
        End Sub
        Private Sub FailureTimerTick(ByVal changedObject As Object)
            changedObject.BackColor = Color.Red
            lastObjectChanged = changedObject
            Timer1.Interval = 400
            Timer1.Start()
        End Sub
    End Class


    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class AssignDatabaseAttributes
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me.btnHelp = New System.Windows.Forms.Button()
            Me.chkBoxPurchasedPart = New System.Windows.Forms.CheckBox()
            Me.chkBoxRawMaterial = New System.Windows.Forms.CheckBox()
            Me.txtBoxNotes = New System.Windows.Forms.TextBox()
            Me.Label35 = New System.Windows.Forms.Label()
            Me.txtBoxTotalQuantity = New System.Windows.Forms.TextBox()
            Me.txtBoxUnitOfMeasure = New System.Windows.Forms.TextBox()
            Me.Label34 = New System.Windows.Forms.Label()
            Me.Label27 = New System.Windows.Forms.Label()
            Me.txtBoxInnerDiameter = New System.Windows.Forms.TextBox()
            Me.MaterialButton = New System.Windows.Forms.Button()
            Me.txtBoxFullDimensions = New System.Windows.Forms.TextBox()
            Me.txtBoxEachQuantity = New System.Windows.Forms.TextBox()
            Me.Label31 = New System.Windows.Forms.Label()
            Me.Label32 = New System.Windows.Forms.Label()
            Me.txtBoxVisMaterialDescription = New System.Windows.Forms.TextBox()
            Me.txtBoxVisMaterial = New System.Windows.Forms.TextBox()
            Me.Label29 = New System.Windows.Forms.Label()
            Me.Label30 = New System.Windows.Forms.Label()
            Me.Label28 = New System.Windows.Forms.Label()
            Me.Label8 = New System.Windows.Forms.Label()
            Me.txtBoxLength = New System.Windows.Forms.TextBox()
            Me.txtBoxWidth = New System.Windows.Forms.TextBox()
            Me.txtBoxRoundLength = New System.Windows.Forms.TextBox()
            Me.Label26 = New System.Windows.Forms.Label()
            Me.txtBoxThickness = New System.Windows.Forms.TextBox()
            Me.Label4 = New System.Windows.Forms.Label()
            Me.txtBoxDiameter = New System.Windows.Forms.TextBox()
            Me.Label23 = New System.Windows.Forms.Label()
            Me.txtBoxSpareQty = New System.Windows.Forms.TextBox()
            Me.Label22 = New System.Windows.Forms.Label()
            Me.btnChangePartNum = New System.Windows.Forms.Button()
            Me.comboxMaterialHdn = New System.Windows.Forms.ComboBox()
            Me.comboxMaterial = New System.Windows.Forms.ComboBox()
            Me.txtBoxVisDesc = New System.Windows.Forms.TextBox()
            Me.Label21 = New System.Windows.Forms.Label()
            Me.Label20 = New System.Windows.Forms.Label()
            Me.Label19 = New System.Windows.Forms.Label()
            Me.Label18 = New System.Windows.Forms.Label()
            Me.Label17 = New System.Windows.Forms.Label()
            Me.btnCancel = New System.Windows.Forms.Button()
            Me.btnOK = New System.Windows.Forms.Button()
            Me.btnAutoAssign = New System.Windows.Forms.Button()
            Me.txtBoxVisComCode = New System.Windows.Forms.TextBox()
            Me.Label13 = New System.Windows.Forms.Label()
            Me.txtBoxVisDwgNo = New System.Windows.Forms.TextBox()
            Me.Label14 = New System.Windows.Forms.Label()
            Me.txtBoxDesc = New System.Windows.Forms.TextBox()
            Me.Label15 = New System.Windows.Forms.Label()
            Me.txtBoxQty = New System.Windows.Forms.TextBox()
            Me.Label12 = New System.Windows.Forms.Label()
            Me.txtBoxJobNo = New System.Windows.Forms.TextBox()
            Me.Label9 = New System.Windows.Forms.Label()
            Me.txtBoxScale = New System.Windows.Forms.TextBox()
            Me.Label10 = New System.Windows.Forms.Label()
            Me.txtBoxChecker = New System.Windows.Forms.TextBox()
            Me.Label11 = New System.Windows.Forms.Label()
            Me.txtBoxPartNo = New System.Windows.Forms.TextBox()
            Me.Label7 = New System.Windows.Forms.Label()
            Me.Label6 = New System.Windows.Forms.Label()
            Me.Label5 = New System.Windows.Forms.Label()
            Me.txtBoxDate = New System.Windows.Forms.TextBox()
            Me.Label3 = New System.Windows.Forms.Label()
            Me.txtBoxDesigner = New System.Windows.Forms.TextBox()
            Me.Label2 = New System.Windows.Forms.Label()
            Me.txtBoxTeam = New System.Windows.Forms.TextBox()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.Label36 = New System.Windows.Forms.Label()
            Me.Label24 = New System.Windows.Forms.Label()
            Me.Label25 = New System.Windows.Forms.Label()
            Me.helpButton = New System.Windows.Forms.Button()
            Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
            Me.chkBoxSkipMaterialSelection = New System.Windows.Forms.CheckBox()
            Me.Timer2 = New System.Windows.Forms.Timer(Me.components)
            Me.txtBoxPrefix = New System.Windows.Forms.TextBox()
            Me.Label16 = New System.Windows.Forms.Label()
            Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
            Me.SuspendLayout()
            '
            'btnHelp
            '
            Me.btnHelp.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.btnHelp.Location = New System.Drawing.Point(1254, 7)
            Me.btnHelp.Name = "btnHelp"
            Me.btnHelp.Size = New System.Drawing.Size(35, 26)
            Me.btnHelp.TabIndex = 162
            Me.btnHelp.Text = "?"
            Me.btnHelp.UseVisualStyleBackColor = True
            '
            'chkBoxPurchasedPart
            '
            Me.chkBoxPurchasedPart.AutoSize = True
            Me.chkBoxPurchasedPart.Location = New System.Drawing.Point(549, 190)
            Me.chkBoxPurchasedPart.Name = "chkBoxPurchasedPart"
            Me.chkBoxPurchasedPart.Size = New System.Drawing.Size(77, 17)
            Me.chkBoxPurchasedPart.TabIndex = 160
            Me.chkBoxPurchasedPart.Text = "Purchased"
            Me.chkBoxPurchasedPart.UseVisualStyleBackColor = True
            '
            'chkBoxRawMaterial
            '
            Me.chkBoxRawMaterial.AutoSize = True
            Me.chkBoxRawMaterial.Location = New System.Drawing.Point(457, 190)
            Me.chkBoxRawMaterial.Name = "chkBoxRawMaterial"
            Me.chkBoxRawMaterial.Size = New System.Drawing.Size(88, 17)
            Me.chkBoxRawMaterial.TabIndex = 159
            Me.chkBoxRawMaterial.Text = "Raw Material"
            Me.chkBoxRawMaterial.UseVisualStyleBackColor = True
            '
            'txtBoxNotes
            '
            Me.txtBoxNotes.Location = New System.Drawing.Point(457, 323)
            Me.txtBoxNotes.Multiline = True
            Me.txtBoxNotes.Name = "txtBoxNotes"
            Me.txtBoxNotes.Size = New System.Drawing.Size(217, 45)
            Me.txtBoxNotes.TabIndex = 127
            '
            'Label35
            '
            Me.Label35.AutoSize = True
            Me.Label35.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label35.Location = New System.Drawing.Point(403, 326)
            Me.Label35.Name = "Label35"
            Me.Label35.Size = New System.Drawing.Size(40, 13)
            Me.Label35.TabIndex = 157
            Me.Label35.Text = "Notes"
            '
            'txtBoxTotalQuantity
            '
            Me.txtBoxTotalQuantity.Location = New System.Drawing.Point(215, 448)
            Me.txtBoxTotalQuantity.Name = "txtBoxTotalQuantity"
            Me.txtBoxTotalQuantity.Size = New System.Drawing.Size(37, 20)
            Me.txtBoxTotalQuantity.TabIndex = 156
            Me.txtBoxTotalQuantity.Visible = False
            '
            'txtBoxUnitOfMeasure
            '
            Me.txtBoxUnitOfMeasure.Location = New System.Drawing.Point(172, 448)
            Me.txtBoxUnitOfMeasure.Name = "txtBoxUnitOfMeasure"
            Me.txtBoxUnitOfMeasure.Size = New System.Drawing.Size(37, 20)
            Me.txtBoxUnitOfMeasure.TabIndex = 155
            Me.txtBoxUnitOfMeasure.Visible = False
            '
            'Label34
            '
            Me.Label34.AutoSize = True
            Me.Label34.ForeColor = System.Drawing.Color.CornflowerBlue
            Me.Label34.Location = New System.Drawing.Point(12, 448)
            Me.Label34.Name = "Label34"
            Me.Label34.Size = New System.Drawing.Size(154, 13)
            Me.Label34.TabIndex = 154
            Me.Label34.Text = "Don't Delete These Textboxes!"
            Me.Label34.Visible = False
            '
            'Label27
            '
            Me.Label27.AutoSize = True
            Me.Label27.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label27.Location = New System.Drawing.Point(601, 274)
            Me.Label27.Name = "Label27"
            Me.Label27.Size = New System.Drawing.Size(24, 13)
            Me.Label27.TabIndex = 152
            Me.Label27.Text = "ID:"
            Me.ToolTip1.SetToolTip(Me.Label27, "Inner Diameter")
            '
            'txtBoxInnerDiameter
            '
            Me.txtBoxInnerDiameter.Location = New System.Drawing.Point(628, 271)
            Me.txtBoxInnerDiameter.Name = "txtBoxInnerDiameter"
            Me.txtBoxInnerDiameter.Size = New System.Drawing.Size(45, 20)
            Me.txtBoxInnerDiameter.TabIndex = 121
            '
            'MaterialButton
            '
            Me.MaterialButton.BackColor = System.Drawing.Color.Gold
            Me.MaterialButton.Enabled = False
            Me.MaterialButton.Location = New System.Drawing.Point(515, 437)
            Me.MaterialButton.Name = "MaterialButton"
            Me.MaterialButton.Size = New System.Drawing.Size(101, 35)
            Me.MaterialButton.TabIndex = 129
            Me.MaterialButton.Text = "Get Material Data"
            Me.MaterialButton.UseVisualStyleBackColor = False
            '
            'txtBoxFullDimensions
            '
            Me.txtBoxFullDimensions.Location = New System.Drawing.Point(15, 475)
            Me.txtBoxFullDimensions.Name = "txtBoxFullDimensions"
            Me.txtBoxFullDimensions.Size = New System.Drawing.Size(217, 20)
            Me.txtBoxFullDimensions.TabIndex = 151
            Me.txtBoxFullDimensions.Visible = False
            '
            'txtBoxEachQuantity
            '
            Me.txtBoxEachQuantity.Enabled = False
            Me.txtBoxEachQuantity.Location = New System.Drawing.Point(628, 382)
            Me.txtBoxEachQuantity.Name = "txtBoxEachQuantity"
            Me.txtBoxEachQuantity.Size = New System.Drawing.Size(46, 20)
            Me.txtBoxEachQuantity.TabIndex = 150
            '
            'Label31
            '
            Me.Label31.AutoSize = True
            Me.Label31.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label31.Location = New System.Drawing.Point(596, 391)
            Me.Label31.Name = "Label31"
            Me.Label31.Size = New System.Drawing.Size(26, 13)
            Me.Label31.TabIndex = 149
            Me.Label31.Text = "Qty"
            '
            'Label32
            '
            Me.Label32.AutoSize = True
            Me.Label32.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label32.Location = New System.Drawing.Point(590, 378)
            Me.Label32.Name = "Label32"
            Me.Label32.Size = New System.Drawing.Size(36, 13)
            Me.Label32.TabIndex = 148
            Me.Label32.Text = "Each"
            '
            'txtBoxVisMaterialDescription
            '
            Me.txtBoxVisMaterialDescription.Enabled = False
            Me.txtBoxVisMaterialDescription.Location = New System.Drawing.Point(457, 411)
            Me.txtBoxVisMaterialDescription.Name = "txtBoxVisMaterialDescription"
            Me.txtBoxVisMaterialDescription.Size = New System.Drawing.Size(217, 20)
            Me.txtBoxVisMaterialDescription.TabIndex = 147
            '
            'txtBoxVisMaterial
            '
            Me.txtBoxVisMaterial.Enabled = False
            Me.txtBoxVisMaterial.Location = New System.Drawing.Point(457, 382)
            Me.txtBoxVisMaterial.Name = "txtBoxVisMaterial"
            Me.txtBoxVisMaterial.Size = New System.Drawing.Size(126, 20)
            Me.txtBoxVisMaterial.TabIndex = 146
            '
            'Label29
            '
            Me.Label29.AutoSize = True
            Me.Label29.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label29.Location = New System.Drawing.Point(389, 426)
            Me.Label29.Name = "Label29"
            Me.Label29.Size = New System.Drawing.Size(67, 13)
            Me.Label29.TabIndex = 145
            Me.Label29.Text = "Mat'l Desc"
            '
            'Label30
            '
            Me.Label30.AutoSize = True
            Me.Label30.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label30.Location = New System.Drawing.Point(398, 413)
            Me.Label30.Name = "Label30"
            Me.Label30.Size = New System.Drawing.Size(53, 13)
            Me.Label30.TabIndex = 144
            Me.Label30.Text = "Visibility"
            '
            'Label28
            '
            Me.Label28.AutoSize = True
            Me.Label28.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label28.Location = New System.Drawing.Point(408, 391)
            Me.Label28.Name = "Label28"
            Me.Label28.Size = New System.Drawing.Size(34, 13)
            Me.Label28.TabIndex = 143
            Me.Label28.Text = "Mat'l"
            '
            'Label8
            '
            Me.Label8.AutoSize = True
            Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label8.Location = New System.Drawing.Point(398, 378)
            Me.Label8.Name = "Label8"
            Me.Label8.Size = New System.Drawing.Size(53, 13)
            Me.Label8.TabIndex = 142
            Me.Label8.Text = "Visibility"
            '
            'txtBoxLength
            '
            Me.txtBoxLength.Location = New System.Drawing.Point(628, 296)
            Me.txtBoxLength.Name = "txtBoxLength"
            Me.txtBoxLength.Size = New System.Drawing.Size(45, 20)
            Me.txtBoxLength.TabIndex = 126
            '
            'txtBoxWidth
            '
            Me.txtBoxWidth.Location = New System.Drawing.Point(545, 297)
            Me.txtBoxWidth.Name = "txtBoxWidth"
            Me.txtBoxWidth.Size = New System.Drawing.Size(45, 20)
            Me.txtBoxWidth.TabIndex = 124
            '
            'txtBoxRoundLength
            '
            Me.txtBoxRoundLength.Location = New System.Drawing.Point(545, 270)
            Me.txtBoxRoundLength.Name = "txtBoxRoundLength"
            Me.txtBoxRoundLength.Size = New System.Drawing.Size(45, 20)
            Me.txtBoxRoundLength.TabIndex = 120
            '
            'Label26
            '
            Me.Label26.AutoSize = True
            Me.Label26.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label26.Location = New System.Drawing.Point(422, 274)
            Me.Label26.Name = "Label26"
            Me.Label26.Size = New System.Drawing.Size(29, 13)
            Me.Label26.TabIndex = 141
            Me.Label26.Text = "OD:"
            Me.ToolTip1.SetToolTip(Me.Label26, "Outer Diameter")
            '
            'txtBoxThickness
            '
            Me.txtBoxThickness.Location = New System.Drawing.Point(457, 297)
            Me.txtBoxThickness.Name = "txtBoxThickness"
            Me.txtBoxThickness.Size = New System.Drawing.Size(45, 20)
            Me.txtBoxThickness.TabIndex = 123
            '
            'Label4
            '
            Me.Label4.AutoSize = True
            Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label4.Location = New System.Drawing.Point(519, 273)
            Me.Label4.Name = "Label4"
            Me.Label4.Size = New System.Drawing.Size(18, 13)
            Me.Label4.TabIndex = 138
            Me.Label4.Text = "L:"
            Me.ToolTip1.SetToolTip(Me.Label4, "Length (Round Part Only)")
            '
            'txtBoxDiameter
            '
            Me.txtBoxDiameter.Location = New System.Drawing.Point(457, 270)
            Me.txtBoxDiameter.Name = "txtBoxDiameter"
            Me.txtBoxDiameter.Size = New System.Drawing.Size(45, 20)
            Me.txtBoxDiameter.TabIndex = 118
            '
            'Label23
            '
            Me.Label23.AutoSize = True
            Me.Label23.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label23.Location = New System.Drawing.Point(12, 9)
            Me.Label23.Name = "Label23"
            Me.Label23.Size = New System.Drawing.Size(120, 13)
            Me.Label23.TabIndex = 137
            Me.Label23.Text = "Last Modified: Jun 2017"
            '
            'txtBoxSpareQty
            '
            Me.txtBoxSpareQty.Location = New System.Drawing.Point(148, 268)
            Me.txtBoxSpareQty.Name = "txtBoxSpareQty"
            Me.txtBoxSpareQty.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxSpareQty.TabIndex = 106
            '
            'Label22
            '
            Me.Label22.AutoSize = True
            Me.Label22.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label22.Location = New System.Drawing.Point(64, 271)
            Me.Label22.Name = "Label22"
            Me.Label22.Size = New System.Drawing.Size(77, 13)
            Me.Label22.TabIndex = 136
            Me.Label22.Text = "SPARE QTY"
            '
            'btnChangePartNum
            '
            Me.btnChangePartNum.Location = New System.Drawing.Point(270, 188)
            Me.btnChangePartNum.Name = "btnChangePartNum"
            Me.btnChangePartNum.Size = New System.Drawing.Size(75, 23)
            Me.btnChangePartNum.TabIndex = 100
            Me.btnChangePartNum.Text = "Change"
            Me.btnChangePartNum.UseVisualStyleBackColor = True
            '
            'comboxMaterialHdn
            '
            Me.comboxMaterialHdn.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.comboxMaterialHdn.FormattingEnabled = True
            Me.comboxMaterialHdn.Location = New System.Drawing.Point(457, 242)
            Me.comboxMaterialHdn.Name = "comboxMaterialHdn"
            Me.comboxMaterialHdn.Size = New System.Drawing.Size(126, 21)
            Me.comboxMaterialHdn.TabIndex = 116
            '
            'comboxMaterial
            '
            Me.comboxMaterial.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
            Me.comboxMaterial.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.comboxMaterial.FormattingEnabled = True
            Me.comboxMaterial.Location = New System.Drawing.Point(457, 215)
            Me.comboxMaterial.Name = "comboxMaterial"
            Me.comboxMaterial.Size = New System.Drawing.Size(217, 21)
            Me.comboxMaterial.TabIndex = 115
            '
            'txtBoxVisDesc
            '
            Me.txtBoxVisDesc.Enabled = False
            Me.txtBoxVisDesc.Location = New System.Drawing.Point(148, 400)
            Me.txtBoxVisDesc.Name = "txtBoxVisDesc"
            Me.txtBoxVisDesc.Size = New System.Drawing.Size(178, 20)
            Me.txtBoxVisDesc.TabIndex = 114
            '
            'Label21
            '
            Me.Label21.AutoSize = True
            Me.Label21.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label21.Location = New System.Drawing.Point(75, 403)
            Me.Label21.Name = "Label21"
            Me.Label21.Size = New System.Drawing.Size(64, 13)
            Me.Label21.TabIndex = 133
            Me.Label21.Text = "VIS DESC"
            '
            'Label20
            '
            Me.Label20.AutoSize = True
            Me.Label20.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Underline), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label20.Location = New System.Drawing.Point(514, 163)
            Me.Label20.Name = "Label20"
            Me.Label20.Size = New System.Drawing.Size(98, 17)
            Me.Label20.TabIndex = 132
            Me.Label20.Text = "Material Info"
            '
            'Label19
            '
            Me.Label19.AutoSize = True
            Me.Label19.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Underline), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label19.Location = New System.Drawing.Point(145, 316)
            Me.Label19.Name = "Label19"
            Me.Label19.Size = New System.Drawing.Size(100, 17)
            Me.Label19.TabIndex = 131
            Me.Label19.Text = "Visibility Info"
            '
            'Label18
            '
            Me.Label18.AutoSize = True
            Me.Label18.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Underline), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label18.Location = New System.Drawing.Point(143, 163)
            Me.Label18.Name = "Label18"
            Me.Label18.Size = New System.Drawing.Size(121, 17)
            Me.Label18.TabIndex = 130
            Me.Label18.Text = "Component Info"
            '
            'Label17
            '
            Me.Label17.AutoSize = True
            Me.Label17.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Underline), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label17.Location = New System.Drawing.Point(297, 11)
            Me.Label17.Name = "Label17"
            Me.Label17.Size = New System.Drawing.Size(152, 17)
            Me.Label17.TabIndex = 128
            Me.Label17.Text = "General Information"
            '
            'btnCancel
            '
            Me.btnCancel.BackColor = System.Drawing.Color.Firebrick
            Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.btnCancel.ForeColor = System.Drawing.Color.Gold
            Me.btnCancel.Location = New System.Drawing.Point(391, 493)
            Me.btnCancel.Name = "btnCancel"
            Me.btnCancel.Size = New System.Drawing.Size(93, 38)
            Me.btnCancel.TabIndex = 135
            Me.btnCancel.Text = "Cancel"
            Me.btnCancel.UseVisualStyleBackColor = False
            '
            'btnOK
            '
            Me.btnOK.BackColor = System.Drawing.Color.ForestGreen
            Me.btnOK.ForeColor = System.Drawing.Color.Gold
            Me.btnOK.Location = New System.Drawing.Point(263, 493)
            Me.btnOK.Name = "btnOK"
            Me.btnOK.Size = New System.Drawing.Size(92, 38)
            Me.btnOK.TabIndex = 134
            Me.btnOK.Text = "Save"
            Me.btnOK.UseVisualStyleBackColor = False
            '
            'btnAutoAssign
            '
            Me.btnAutoAssign.Location = New System.Drawing.Point(628, 99)
            Me.btnAutoAssign.Name = "btnAutoAssign"
            Me.btnAutoAssign.Size = New System.Drawing.Size(75, 23)
            Me.btnAutoAssign.TabIndex = 108
            Me.btnAutoAssign.Text = "Auto Assign"
            Me.btnAutoAssign.UseVisualStyleBackColor = True
            '
            'txtBoxVisComCode
            '
            Me.txtBoxVisComCode.Enabled = False
            Me.txtBoxVisComCode.Location = New System.Drawing.Point(148, 374)
            Me.txtBoxVisComCode.Name = "txtBoxVisComCode"
            Me.txtBoxVisComCode.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxVisComCode.TabIndex = 112
            '
            'Label13
            '
            Me.Label13.AutoSize = True
            Me.Label13.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label13.Location = New System.Drawing.Point(43, 377)
            Me.Label13.Name = "Label13"
            Me.Label13.Size = New System.Drawing.Size(96, 13)
            Me.Label13.TabIndex = 125
            Me.Label13.Text = "VIS COM CODE"
            '
            'txtBoxVisDwgNo
            '
            Me.txtBoxVisDwgNo.Enabled = False
            Me.txtBoxVisDwgNo.Location = New System.Drawing.Point(148, 348)
            Me.txtBoxVisDwgNo.Name = "txtBoxVisDwgNo"
            Me.txtBoxVisDwgNo.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxVisDwgNo.TabIndex = 110
            '
            'Label14
            '
            Me.Label14.AutoSize = True
            Me.Label14.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label14.Location = New System.Drawing.Point(66, 351)
            Me.Label14.Name = "Label14"
            Me.Label14.Size = New System.Drawing.Size(73, 13)
            Me.Label14.TabIndex = 122
            Me.Label14.Text = "VIS DWG #"
            '
            'txtBoxDesc
            '
            Me.txtBoxDesc.Location = New System.Drawing.Point(148, 216)
            Me.txtBoxDesc.Name = "txtBoxDesc"
            Me.txtBoxDesc.Size = New System.Drawing.Size(184, 20)
            Me.txtBoxDesc.TabIndex = 104
            '
            'Label15
            '
            Me.Label15.AutoSize = True
            Me.Label15.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label15.Location = New System.Drawing.Point(78, 220)
            Me.Label15.Name = "Label15"
            Me.Label15.Size = New System.Drawing.Size(66, 13)
            Me.Label15.TabIndex = 119
            Me.Label15.Text = "Part Name"
            '
            'txtBoxQty
            '
            Me.txtBoxQty.Location = New System.Drawing.Point(148, 242)
            Me.txtBoxQty.Name = "txtBoxQty"
            Me.txtBoxQty.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxQty.TabIndex = 105
            '
            'Label12
            '
            Me.Label12.AutoSize = True
            Me.Label12.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label12.Location = New System.Drawing.Point(109, 245)
            Me.Label12.Name = "Label12"
            Me.Label12.Size = New System.Drawing.Size(32, 13)
            Me.Label12.TabIndex = 116
            Me.Label12.Text = "QTY"
            '
            'txtBoxJobNo
            '
            Me.txtBoxJobNo.Location = New System.Drawing.Point(489, 101)
            Me.txtBoxJobNo.Name = "txtBoxJobNo"
            Me.txtBoxJobNo.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxJobNo.TabIndex = 97
            '
            'Label9
            '
            Me.Label9.AutoSize = True
            Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label9.Location = New System.Drawing.Point(399, 106)
            Me.Label9.Name = "Label9"
            Me.Label9.Size = New System.Drawing.Size(85, 13)
            Me.Label9.TabIndex = 113
            Me.Label9.Text = "Job/Project #"
            '
            'txtBoxScale
            '
            Me.txtBoxScale.Location = New System.Drawing.Point(489, 75)
            Me.txtBoxScale.Name = "txtBoxScale"
            Me.txtBoxScale.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxScale.TabIndex = 96
            '
            'Label10
            '
            Me.Label10.AutoSize = True
            Me.Label10.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label10.Location = New System.Drawing.Point(399, 78)
            Me.Label10.Name = "Label10"
            Me.Label10.Size = New System.Drawing.Size(68, 13)
            Me.Label10.TabIndex = 111
            Me.Label10.Text = "Dwg Scale"
            '
            'txtBoxChecker
            '
            Me.txtBoxChecker.Location = New System.Drawing.Point(489, 49)
            Me.txtBoxChecker.Name = "txtBoxChecker"
            Me.txtBoxChecker.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxChecker.TabIndex = 95
            '
            'Label11
            '
            Me.Label11.AutoSize = True
            Me.Label11.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label11.Location = New System.Drawing.Point(413, 49)
            Me.Label11.Name = "Label11"
            Me.Label11.Size = New System.Drawing.Size(54, 13)
            Me.Label11.TabIndex = 107
            Me.Label11.Text = "Checker"
            '
            'txtBoxPartNo
            '
            Me.txtBoxPartNo.Enabled = False
            Me.txtBoxPartNo.Location = New System.Drawing.Point(148, 190)
            Me.txtBoxPartNo.Name = "txtBoxPartNo"
            Me.txtBoxPartNo.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxPartNo.TabIndex = 103
            '
            'Label7
            '
            Me.Label7.AutoSize = True
            Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label7.Location = New System.Drawing.Point(99, 197)
            Me.Label7.Name = "Label7"
            Me.Label7.Size = New System.Drawing.Size(42, 13)
            Me.Label7.TabIndex = 102
            Me.Label7.Text = "Part #"
            '
            'Label6
            '
            Me.Label6.AutoSize = True
            Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label6.Location = New System.Drawing.Point(399, 218)
            Me.Label6.Name = "Label6"
            Me.Label6.Size = New System.Drawing.Size(52, 13)
            Me.Label6.TabIndex = 101
            Me.Label6.Text = "Material"
            '
            'Label5
            '
            Me.Label5.AutoSize = True
            Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label5.Location = New System.Drawing.Point(393, 245)
            Me.Label5.Name = "Label5"
            Me.Label5.Size = New System.Drawing.Size(61, 13)
            Me.Label5.TabIndex = 99
            Me.Label5.Text = "Mat'l Hdn"
            '
            'txtBoxDate
            '
            Me.txtBoxDate.Location = New System.Drawing.Point(239, 101)
            Me.txtBoxDate.Name = "txtBoxDate"
            Me.txtBoxDate.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxDate.TabIndex = 93
            '
            'Label3
            '
            Me.Label3.AutoSize = True
            Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label3.Location = New System.Drawing.Point(196, 101)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New System.Drawing.Size(34, 13)
            Me.Label3.TabIndex = 94
            Me.Label3.Text = "Date"
            '
            'txtBoxDesigner
            '
            Me.txtBoxDesigner.Location = New System.Drawing.Point(239, 75)
            Me.txtBoxDesigner.Name = "txtBoxDesigner"
            Me.txtBoxDesigner.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxDesigner.TabIndex = 92
            '
            'Label2
            '
            Me.Label2.AutoSize = True
            Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label2.Location = New System.Drawing.Point(173, 75)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(57, 13)
            Me.Label2.TabIndex = 91
            Me.Label2.Text = "Designer"
            '
            'txtBoxTeam
            '
            Me.txtBoxTeam.Location = New System.Drawing.Point(239, 49)
            Me.txtBoxTeam.Name = "txtBoxTeam"
            Me.txtBoxTeam.Size = New System.Drawing.Size(116, 20)
            Me.txtBoxTeam.TabIndex = 90
            '
            'Label1
            '
            Me.Label1.AutoSize = True
            Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label1.Location = New System.Drawing.Point(149, 49)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(81, 13)
            Me.Label1.TabIndex = 89
            Me.Label1.Text = "Design Team"
            '
            'Label36
            '
            Me.Label36.AutoSize = True
            Me.Label36.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label36.Location = New System.Drawing.Point(424, 300)
            Me.Label36.Name = "Label36"
            Me.Label36.Size = New System.Drawing.Size(28, 13)
            Me.Label36.TabIndex = 163
            Me.Label36.Text = "TH:"
            Me.Label36.TextAlign = System.Drawing.ContentAlignment.TopCenter
            Me.ToolTip1.SetToolTip(Me.Label36, "Thickness")
            '
            'Label24
            '
            Me.Label24.AutoSize = True
            Me.Label24.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label24.Location = New System.Drawing.Point(517, 299)
            Me.Label24.Name = "Label24"
            Me.Label24.Size = New System.Drawing.Size(23, 13)
            Me.Label24.TabIndex = 164
            Me.Label24.Text = "W:"
            Me.ToolTip1.SetToolTip(Me.Label24, "Width")
            '
            'Label25
            '
            Me.Label25.AutoSize = True
            Me.Label25.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label25.Location = New System.Drawing.Point(604, 300)
            Me.Label25.Name = "Label25"
            Me.Label25.Size = New System.Drawing.Size(18, 13)
            Me.Label25.TabIndex = 165
            Me.Label25.Text = "L:"
            Me.ToolTip1.SetToolTip(Me.Label25, "Length (Of Block)")
            '
            'helpButton
            '
            Me.helpButton.Location = New System.Drawing.Point(15, 493)
            Me.helpButton.Name = "helpButton"
            Me.helpButton.Size = New System.Drawing.Size(94, 38)
            Me.helpButton.TabIndex = 166
            Me.helpButton.Text = "Help"
            Me.helpButton.UseVisualStyleBackColor = True
            '
            'chkBoxSkipMaterialSelection
            '
            Me.chkBoxSkipMaterialSelection.AutoSize = True
            Me.chkBoxSkipMaterialSelection.Location = New System.Drawing.Point(632, 190)
            Me.chkBoxSkipMaterialSelection.Name = "chkBoxSkipMaterialSelection"
            Me.chkBoxSkipMaterialSelection.Size = New System.Drawing.Size(47, 17)
            Me.chkBoxSkipMaterialSelection.TabIndex = 168
            Me.chkBoxSkipMaterialSelection.Text = "Skip"
            Me.chkBoxSkipMaterialSelection.UseVisualStyleBackColor = True
            '
            'txtBoxPrefix
            '
            Me.txtBoxPrefix.Location = New System.Drawing.Point(627, 242)
            Me.txtBoxPrefix.Name = "txtBoxPrefix"
            Me.txtBoxPrefix.Size = New System.Drawing.Size(46, 20)
            Me.txtBoxPrefix.TabIndex = 117
            '
            'Label16
            '
            Me.Label16.AutoSize = True
            Me.Label16.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label16.Location = New System.Drawing.Point(595, 245)
            Me.Label16.Name = "Label16"
            Me.Label16.Size = New System.Drawing.Size(26, 13)
            Me.Label16.TabIndex = 170
            Me.Label16.Text = "Pre"
            Me.ToolTip1.SetToolTip(Me.Label16, "Prefix For Dimensions (Optional)")
            '
            'AssignDatabaseAttributes
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(718, 552)
            Me.Controls.Add(Me.Label16)
            Me.Controls.Add(Me.txtBoxPrefix)
            Me.Controls.Add(Me.chkBoxSkipMaterialSelection)
            Me.Controls.Add(Me.helpButton)
            Me.Controls.Add(Me.Label25)
            Me.Controls.Add(Me.Label24)
            Me.Controls.Add(Me.Label36)
            Me.Controls.Add(Me.btnHelp)
            Me.Controls.Add(Me.chkBoxPurchasedPart)
            Me.Controls.Add(Me.chkBoxRawMaterial)
            Me.Controls.Add(Me.txtBoxNotes)
            Me.Controls.Add(Me.Label35)
            Me.Controls.Add(Me.txtBoxTotalQuantity)
            Me.Controls.Add(Me.txtBoxUnitOfMeasure)
            Me.Controls.Add(Me.Label34)
            Me.Controls.Add(Me.Label27)
            Me.Controls.Add(Me.txtBoxInnerDiameter)
            Me.Controls.Add(Me.MaterialButton)
            Me.Controls.Add(Me.txtBoxFullDimensions)
            Me.Controls.Add(Me.txtBoxEachQuantity)
            Me.Controls.Add(Me.Label31)
            Me.Controls.Add(Me.Label32)
            Me.Controls.Add(Me.txtBoxVisMaterialDescription)
            Me.Controls.Add(Me.txtBoxVisMaterial)
            Me.Controls.Add(Me.Label29)
            Me.Controls.Add(Me.Label30)
            Me.Controls.Add(Me.Label28)
            Me.Controls.Add(Me.Label8)
            Me.Controls.Add(Me.txtBoxLength)
            Me.Controls.Add(Me.txtBoxWidth)
            Me.Controls.Add(Me.txtBoxRoundLength)
            Me.Controls.Add(Me.Label26)
            Me.Controls.Add(Me.txtBoxThickness)
            Me.Controls.Add(Me.Label4)
            Me.Controls.Add(Me.txtBoxDiameter)
            Me.Controls.Add(Me.Label23)
            Me.Controls.Add(Me.txtBoxSpareQty)
            Me.Controls.Add(Me.Label22)
            Me.Controls.Add(Me.btnChangePartNum)
            Me.Controls.Add(Me.comboxMaterialHdn)
            Me.Controls.Add(Me.comboxMaterial)
            Me.Controls.Add(Me.txtBoxVisDesc)
            Me.Controls.Add(Me.Label21)
            Me.Controls.Add(Me.Label20)
            Me.Controls.Add(Me.Label19)
            Me.Controls.Add(Me.Label18)
            Me.Controls.Add(Me.Label17)
            Me.Controls.Add(Me.btnCancel)
            Me.Controls.Add(Me.btnOK)
            Me.Controls.Add(Me.btnAutoAssign)
            Me.Controls.Add(Me.txtBoxVisComCode)
            Me.Controls.Add(Me.Label13)
            Me.Controls.Add(Me.txtBoxVisDwgNo)
            Me.Controls.Add(Me.Label14)
            Me.Controls.Add(Me.txtBoxDesc)
            Me.Controls.Add(Me.Label15)
            Me.Controls.Add(Me.txtBoxQty)
            Me.Controls.Add(Me.Label12)
            Me.Controls.Add(Me.txtBoxJobNo)
            Me.Controls.Add(Me.Label9)
            Me.Controls.Add(Me.txtBoxScale)
            Me.Controls.Add(Me.Label10)
            Me.Controls.Add(Me.txtBoxChecker)
            Me.Controls.Add(Me.Label11)
            Me.Controls.Add(Me.txtBoxPartNo)
            Me.Controls.Add(Me.Label7)
            Me.Controls.Add(Me.Label6)
            Me.Controls.Add(Me.Label5)
            Me.Controls.Add(Me.txtBoxDate)
            Me.Controls.Add(Me.Label3)
            Me.Controls.Add(Me.txtBoxDesigner)
            Me.Controls.Add(Me.Label2)
            Me.Controls.Add(Me.txtBoxTeam)
            Me.Controls.Add(Me.Label1)
            Me.Name = "AssignDatabaseAttributes"
            Me.Text = "Assign Database Attributes V2"
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Friend WithEvents btnHelp As Button
        Friend WithEvents chkBoxPurchasedPart As CheckBox
        Friend WithEvents chkBoxRawMaterial As CheckBox
        Friend WithEvents txtBoxNotes As TextBox
        Friend WithEvents Label35 As Label
        Friend WithEvents txtBoxTotalQuantity As TextBox
        Friend WithEvents txtBoxUnitOfMeasure As TextBox
        Friend WithEvents Label34 As Label
        Friend WithEvents Label27 As Label
        Friend WithEvents txtBoxInnerDiameter As TextBox
        Friend WithEvents MaterialButton As Button
        Friend WithEvents txtBoxFullDimensions As TextBox
        Friend WithEvents txtBoxEachQuantity As TextBox
        Friend WithEvents Label31 As Label
        Friend WithEvents Label32 As Label
        Friend WithEvents txtBoxVisMaterialDescription As TextBox
        Friend WithEvents txtBoxVisMaterial As TextBox
        Friend WithEvents Label29 As Label
        Friend WithEvents Label30 As Label
        Friend WithEvents Label28 As Label
        Friend WithEvents Label8 As Label
        Friend WithEvents txtBoxLength As TextBox
        Friend WithEvents txtBoxWidth As TextBox
        Friend WithEvents txtBoxRoundLength As TextBox
        Friend WithEvents Label26 As Label
        Friend WithEvents txtBoxThickness As TextBox
        Friend WithEvents Label4 As Label
        Friend WithEvents txtBoxDiameter As TextBox
        Friend WithEvents Label23 As Label
        Friend WithEvents txtBoxSpareQty As TextBox
        Friend WithEvents Label22 As Label
        Friend WithEvents btnChangePartNum As Button
        Friend WithEvents comboxMaterialHdn As ComboBox
        Friend WithEvents comboxMaterial As ComboBox
        Friend WithEvents txtBoxVisDesc As TextBox
        Friend WithEvents Label21 As Label
        Friend WithEvents Label20 As Label
        Friend WithEvents Label19 As Label
        Friend WithEvents Label18 As Label
        Friend WithEvents Label17 As Label
        Friend WithEvents btnCancel As Button
        Friend WithEvents btnOK As Button
        Friend WithEvents btnAutoAssign As Button
        Friend WithEvents txtBoxVisComCode As TextBox
        Friend WithEvents Label13 As Label
        Friend WithEvents txtBoxVisDwgNo As TextBox
        Friend WithEvents Label14 As Label
        Friend WithEvents txtBoxDesc As TextBox
        Friend WithEvents Label15 As Label
        Friend WithEvents txtBoxQty As TextBox
        Friend WithEvents Label12 As Label
        Friend WithEvents txtBoxJobNo As TextBox
        Friend WithEvents Label9 As Label
        Friend WithEvents txtBoxScale As TextBox
        Friend WithEvents Label10 As Label
        Friend WithEvents txtBoxChecker As TextBox
        Friend WithEvents Label11 As Label
        Friend WithEvents txtBoxPartNo As TextBox
        Friend WithEvents Label7 As Label
        Friend WithEvents Label6 As Label
        Friend WithEvents Label5 As Label
        Friend WithEvents txtBoxDate As TextBox
        Friend WithEvents Label3 As Label
        Friend WithEvents txtBoxDesigner As TextBox
        Friend WithEvents Label2 As Label
        Friend WithEvents txtBoxTeam As TextBox
        Friend WithEvents Label1 As Label
        Friend WithEvents Label36 As Label
        Friend WithEvents Label24 As Label
        Friend WithEvents Label25 As Label
        Friend WithEvents helpButton As Button
        Friend WithEvents Timer1 As Timer
        Friend WithEvents chkBoxSkipMaterialSelection As CheckBox
        Friend WithEvents Timer2 As Timer
        Friend WithEvents txtBoxPrefix As TextBox
        Friend WithEvents Label16 As Label
        Friend WithEvents ToolTip1 As ToolTip
    End Class


End Module