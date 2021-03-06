'---------------Created by Raem Naveed (2017)---------------------'


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
Imports NXOpen.Drawings


'Do you even bhangra bro?

Module Module1

    Dim filename As String = Nothing
    Dim filenameShrunk As String = Nothing
    Sub Main()
        Dim form As Form1
        form = New Form1()
        form.ShowDialog()
    End Sub
    Public Class Form1

        Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            comboxFileType.Items.Add("STEP")
            comboxFileType.Items.Add("IGES")
            comboxFileType.Items.Add("ParaSolid")
            comboxFileType.Text = "STEP"
            comboxCustomer.Items.Add("SCJohnson")
            comboxCustomer.Items.Add("Husky")
            comboxCustomer.Items.Add("Plastipak")
            comboxCustomer.Items.Add("P&G")
            comboxCustomer.Items.Add("Unilever")
            comboxCustomer.Items.Add("Ferrero")
            comboxCustomer.Items.Add("Sonoco Plastics")
            'comboxCustomer.Items.Add("")
            comboxSales.Items.Add("Peter S.")
            comboxSales.Items.Add("Rob R.")
            comboxSales.Items.Add("Vince T.")
            'comboxSales.Items.Add("")
            txtboxQuoteNum.Text = "XXXXXX"
        End Sub
        Private Sub txtBoxFilePath_TextChanged(sender As Object, e As EventArgs) Handles txtBoxFilePath.TextChanged
            If txtBoxFilePath.Text.EndsWith("igs") Then
                comboxFileType.Text = "IGES"
            ElseIf txtBoxFilePath.Text.EndsWith("stp") Then
                comboxFileType.Text = "STEP"
            ElseIf txtBoxFilePath.Text.EndsWith("step") Then
                comboxFileType.Text = "STEP"
            ElseIf txtBoxFilePath.Text.EndsWith("x_t") Then
                comboxFileType.Text = "ParaSolid"
            End If
        End Sub
        Private Sub btnFind_Click(sender As Object, e As EventArgs) Handles btnFind.Click
            If (OpenFileDialog1.ShowDialog = DialogResult.OK) Then
                txtBoxFilePath.Text = OpenFileDialog1.FileName
                filename = ExtractFilename(txtBoxFilePath.Text)
                filename = filename.Replace(" ", "")
                'MessageBox.Show(filename)
                If txtBoxFilePath.Text.EndsWith("step") Then
                    filenameShrunk = filename.Trim().Substring(0, filename.Length - 5)
                Else
                    filenameShrunk = filename.Trim().Substring(0, filename.Length - 4)
                End If

                'MessageBox.Show(filename)
            End If
        End Sub
        'c note drawing 
        Private Function ExtractFilename(filepath As String) As String
            ' If path ends with a "\", it's a path only so return String.Empty.
            If filepath.Trim().EndsWith("\") Then Return String.Empty

            ' Determine where last backslash is.
            Dim position As Integer = filepath.LastIndexOf("\"c)
            ' If there is no backslash, assume that this is a filename.
            If position = -1 Then
                ' Determine whether file exists in the current directory.
                If File.Exists(Environment.CurrentDirectory + Path.DirectorySeparatorChar + filepath) Then
                    Return filepath
                Else
                    Return String.Empty
                End If
            Else
                ' Determine whether file exists using filepath.
                If File.Exists(filepath) Then
                    ' Return filename without file path.
                    Return filepath.Substring(position + 1)
                Else
                    Return String.Empty
                End If
            End If
        End Function

        Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
            'Dim theSession As Session = Session.GetSession()
            CreateDrawing()
            savePart()
        End Sub
        Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
            savePart()
            exportDrawings()
        End Sub
        Public Sub savePart()
            Dim theSession As Session = Session.GetSession()
            Dim workPart As Part = theSession.Parts.Work
            Dim partSaveStatus1 As PartSaveStatus
            partSaveStatus1 = workPart.Save(BasePart.SaveComponents.True, BasePart.CloseAfterSave.False)
            partSaveStatus1.Dispose()
        End Sub
        Public Sub readAttribute(ByVal attrName As String, ByRef attrVal As String)
            Dim theSession As Session = Session.GetSession()
            Dim thePart As Part = theSession.Parts.Display
            Dim theAttr As Attribute = Nothing
            Dim attr_info() As NXObject.AttributeInformation
            attr_info = thePart.GetAttributeTitlesByType(NXObject.AttributeType.String)

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

        Public Sub exportDrawings()
            Dim theSession As Session = Session.GetSession()
            Dim workPart As Part = theSession.Parts.Work
            Dim displayPart As Part = theSession.Parts.Display
            Dim dateToday As String = System.DateTime.Now.ToString("MMddyyyy")
            dateToday = dateToday.Replace("/", "")
            Dim attrName As String = "FileName"
            Dim attrVal As String = Nothing
            readAttribute(attrName, attrVal)
            filenameShrunk = attrVal

            attrName = "CustomerName"
            attrVal = Nothing
            readAttribute(attrName, attrVal)
            Dim customerName As String = attrVal
            'MsgBox("C:\Users\" + System.Environment.UserName + "\Desktop\" + txtboxQuoteNum.Text + "_" + customerName + "_" + dateToday + "_" + filenameShrunk + ".pdf")
            Dim printPDFBuilder1 As PrintPDFBuilder
            printPDFBuilder1 = workPart.PlotManager.CreatePrintPdfbuilder()
            printPDFBuilder1.Scale = 1.0
            printPDFBuilder1.Colors = PrintPDFBuilder.Color.BlackOnWhite
            printPDFBuilder1.Widths = PrintPDFBuilder.Width.CustomThreeWidths
            printPDFBuilder1.Units = PrintPDFBuilder.UnitsOption.English
            printPDFBuilder1.XDimension = 8.5
            printPDFBuilder1.YDimension = 11.0
            printPDFBuilder1.OutputText = PrintPDFBuilder.OutputTextOption.Polylines
            printPDFBuilder1.RasterImages = True
            printPDFBuilder1.Watermark = ""

            Dim sheets1(0) As NXObject
            Dim dwgName As String = Nothing
            Dim allDwgs() As DrawingSheet = displayPart.DrawingSheets.ToArray()

            For Each thisDwg As DrawingSheet In allDwgs
                If thisDwg.Name.Contains("1") Then
                    dwgName = thisDwg.Name()
                End If
            Next
            'MsgBox(dwgName) and number and title here until mohammed leaves with his money 
            Dim drawingSheet1 As Drawings.DrawingSheet = CType(workPart.DrawingSheets.FindObject(dwgName), Drawings.DrawingSheet)
            sheets1(0) = drawingSheet1
            printPDFBuilder1.SourceBuilder.SetSheets(sheets1)
            printPDFBuilder1.Filename = "C:\Users\" + System.Environment.UserName + "\Desktop\" + txtboxQuoteNum.Text + "_" + customerName.Replace(" ", "_") + "_" + dateToday + "_" + filenameShrunk + ".pdf"
            Dim nXObject1 As NXObject
            nXObject1 = printPDFBuilder1.Commit()
            printPDFBuilder1.Destroy()
            ' ----------------------------------------------
            '   Menu: File->Export->AutoCAD DXF/DWG...
            ' ----------------------------------------------
            Dim dxfdwgCreator1 As DxfdwgCreator
            dxfdwgCreator1 = theSession.DexManager.CreateDxfdwgCreator()
            dxfdwgCreator1.ExportData = DxfdwgCreator.ExportDataOption.Drawing
            dxfdwgCreator1.AutoCADRevision = DxfdwgCreator.AutoCADRevisionOptions.R2004
            dxfdwgCreator1.ViewEditMode = True
            dxfdwgCreator1.FlattenAssembly = True
            'Try
            '    'dxfdwgCreator1.ExportScaleValue = "1:1"
            'Catch ex As Exception
            'End Try
            dxfdwgCreator1.SettingsFile = "U:\Default_Settings\Translator_Default\dxfdwg_English.def"
            dxfdwgCreator1.ExportFrom = DxfdwgCreator.ExportFromOption.ExistingPart
            dxfdwgCreator1.OutputFile = "C:\Users\" + System.Environment.UserName + "\Desktop\" + txtboxQuoteNum.Text + "_" + customerName + "_" + dateToday + "_" + filenameShrunk + ".dxf"
            dxfdwgCreator1.ObjectTypes.Curves = True
            dxfdwgCreator1.ObjectTypes.Annotations = True
            dxfdwgCreator1.ObjectTypes.Structures = True
            dxfdwgCreator1.AutoCADRevision = DxfdwgCreator.AutoCADRevisionOptions.R2000
            dxfdwgCreator1.FlattenAssembly = False
            dxfdwgCreator1.InputFile = "C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\" + filenameShrunk + ".prt"
            dxfdwgCreator1.InputFile = "C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\" + filenameShrunk + ".prt"
            dxfdwgCreator1.OutputFile = "C:\Users\" + System.Environment.UserName + "\Desktop\" + txtboxQuoteNum.Text + "_" + customerName.Replace(" ", "_") + "_" + dateToday + "_" + filenameShrunk + ".dxf"
            dxfdwgCreator1.WidthFactorMode = DxfdwgCreator.WidthfactorMethodOptions.AutomaticCalculation
            dxfdwgCreator1.LayerMask = "1-256"
            dxfdwgCreator1.DrawingList = dwgName

            Dim nXObject2 As NXObject
            nXObject2 = dxfdwgCreator1.Commit()

            dxfdwgCreator1.Destroy()
        End Sub
        Public Sub CreateDrawing()
            Dim theSession As Session = Session.GetSession()
            ' ----------------------------------------------
            '   Menu: File->New...
            ' ----------------------------------------------
            Try
                If (System.IO.File.Exists("C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\" + filenameShrunk + ".prt")) Then
                    System.IO.File.Delete("C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\" + filenameShrunk + ".prt")
                    'MsgBox("here")
                End If
            Catch ex As Exception
            End Try
            Dim fileNew1 As FileNew
            fileNew1 = theSession.Parts.FileNew()
            fileNew1.TemplateFileName = "Blank"
            fileNew1.ApplicationName = "GatewayTemplate"
            fileNew1.Units = Part.Units.Inches
            fileNew1.RelationType = ""
            fileNew1.UsesMasterModel = "No"
            fileNew1.TemplateType = FileNewTemplateType.Item
            Dim path As String = "C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\"
            If (Not System.IO.Directory.Exists(path)) Then
                System.IO.Directory.CreateDirectory(path)
            End If
            fileNew1.NewFileName = "C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\" + filenameShrunk + ".prt"
            'MsgBox(fileNew1.newfilename)
            fileNew1.MasterFileName = ""
            fileNew1.UseBlankTemplate = True
            fileNew1.MakeDisplayedPart = True

            Dim nXObject1 As NXObject
            nXObject1 = fileNew1.Commit()
            Dim workPart As Part = theSession.Parts.Work
            Dim displayPart As Part = theSession.Parts.Display
            fileNew1.Destroy()

            ' ----------------------------------------------
            '   Menu: File->Import->STEP203...
            ' ----------------------------------------------
            If comboxFileType.Text = "STEP" Then
                ImportStep()
            ElseIf comboxFileType.Text = "ParaSolid" Then
                ImportParaSolid()
            ElseIf comboxFileType.Text = "IGES" Then
                importIges()
            Else
                MsgBox("File Type Not Supported, Exiting")
                Exit Sub
            End If

            ' ----------------------------------------------
            '   Menu: Application->Drafting...
            ' ----------------------------------------------
            workPart.ModelingViews.WorkView.UpdateCustomSymbols()
            workPart.Drafting.SetTemplateInstantiationIsComplete(True)
            ' ----------------------------------------------
            '   Menu: Insert->Sheet...
            ' ----------------------------------------------

            Dim nullDrawings_DrawingSheet As Drawings.DrawingSheet = Nothing

            Dim drawingSheetBuilder1 As Drawings.DrawingSheetBuilder
            drawingSheetBuilder1 = workPart.DrawingSheets.DrawingSheetBuilder(nullDrawings_DrawingSheet)
            drawingSheetBuilder1.Option = Drawings.DrawingSheetBuilder.SheetOption.StandardSize
            drawingSheetBuilder1.StandardMetricScale = Drawings.DrawingSheetBuilder.SheetStandardMetricScale.S11
            drawingSheetBuilder1.StandardEnglishScale = Drawings.DrawingSheetBuilder.SheetStandardEnglishScale.S11
            drawingSheetBuilder1.MetricSheetTemplateLocation = ""
            drawingSheetBuilder1.EnglishSheetTemplateLocation = ""
            drawingSheetBuilder1.Height = 12.0
            drawingSheetBuilder1.Length = 18.0
            drawingSheetBuilder1.StandardMetricScale = Drawings.DrawingSheetBuilder.SheetStandardMetricScale.S11
            drawingSheetBuilder1.StandardEnglishScale = Drawings.DrawingSheetBuilder.SheetStandardEnglishScale.S11
            drawingSheetBuilder1.ScaleNumerator = 1.0
            drawingSheetBuilder1.ScaleDenominator = 1.0
            drawingSheetBuilder1.Units = Drawings.DrawingSheetBuilder.SheetUnits.English
            drawingSheetBuilder1.ProjectionAngle = Drawings.DrawingSheetBuilder.SheetProjectionAngle.Third
            drawingSheetBuilder1.Number = "1"
            drawingSheetBuilder1.SecondaryNumber = ""
            drawingSheetBuilder1.Revision = "A"
            drawingSheetBuilder1.Height = 24.0
            drawingSheetBuilder1.Length = 36.0
            drawingSheetBuilder1.Height = 18.0
            drawingSheetBuilder1.Length = 24.0

            Dim nXObject3 As NXObject
            nXObject3 = drawingSheetBuilder1.Commit()
            drawingSheetBuilder1.Destroy()
            workPart.Drafting.SetTemplateInstantiationIsComplete(True)
            ' ----------------------------------------------
            '   Menu: Insert->View->Base...
            ' ----------------------------------------------
            ' 
            Dim nullDrawings_BaseView As Drawings.BaseView = Nothing
            Dim baseViewBuilder1 As Drawings.BaseViewBuilder
            baseViewBuilder1 = workPart.DraftingViews.CreateBaseViewBuilder(nullDrawings_BaseView)
            baseViewBuilder1.Placement.Associative = True
            Dim modelingView1 As ModelingView = CType(workPart.ModelingViews.FindObject("Top"), ModelingView)
            baseViewBuilder1.SelectModelView.SelectedView = modelingView1
            baseViewBuilder1.Style.ViewStyleBase.Part = workPart
            baseViewBuilder1.Style.ViewStyleBase.PartName = "C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\" + filenameShrunk + ".prt"
            Dim partLoadStatus1 As PartLoadStatus
            partLoadStatus1 = workPart.LoadFully()
            partLoadStatus1.Dispose()
            baseViewBuilder1.SelectModelView.SelectedView = modelingView1
            Dim nullAssemblies_Arrangement As Assemblies.Arrangement = Nothing
            baseViewBuilder1.Style.ViewStyleBase.Arrangement.SelectedArrangement = nullAssemblies_Arrangement
            baseViewBuilder1.SelectModelView.SelectedView = modelingView1
            baseViewBuilder1.Style.ViewStyleBase.Arrangement.SelectedArrangement = nullAssemblies_Arrangement
            Dim point1 As Point3d = New Point3d(7.63371886120996, 13.3006227758007, 0.0)
            baseViewBuilder1.Placement.Placement.SetValue(Nothing, workPart.Views.WorkView, point1)
            Dim nXObject4 As NXObject
            nXObject4 = baseViewBuilder1.Commit()
            baseViewBuilder1.Destroy()

            ' ----------------------------------------------
            '   Menu: Insert->View->Projected...
            ' ----------------------------------------------
            Dim nullDrawings_ProjectedView As Drawings.ProjectedView = Nothing
            Dim projectedViewBuilder1 As Drawings.ProjectedViewBuilder
            projectedViewBuilder1 = workPart.DraftingViews.CreateProjectedViewBuilder(nullDrawings_ProjectedView)
            projectedViewBuilder1.Placement.Associative = True
            projectedViewBuilder1.Placement.AlignmentMethod = Drawings.ViewPlacementBuilder.Method.PerpendicularToHingeLine
            Dim nullDirection As Direction = Nothing
            projectedViewBuilder1.Placement.AlignmentVector = nullDirection
            projectedViewBuilder1.Placement.AlignmentOption = Drawings.ViewPlacementBuilder.Option.ModelPoint

            Dim unit1 As Unit = CType(workPart.UnitCollection.FindObject("Inch"), Unit)
            Dim expression1 As Expression
            expression1 = workPart.Expressions.CreateSystemExpressionWithUnits("0", unit1)
            Dim baseView1 As Drawings.BaseView = CType(nXObject4, Drawings.BaseView)
            projectedViewBuilder1.Parent.View.Value = baseView1
            projectedViewBuilder1.Style.ViewStyleBase.PartName = "C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\" + filenameShrunk + ".prt"
            projectedViewBuilder1.Style.ViewStyleGeneral.ToleranceValue = 0.00210106761565836
            Dim vieworigin1 As Point3d = New Point3d(2.66895676502431, -0.0, -0.00863886038481576)
            projectedViewBuilder1.Style.ViewStylePerspective.ViewOrigin = vieworigin1
            Dim point2 As NXopen.Point = Nothing 'CType(workPart.Points.FindObject("ENTITY 2 1"), Point)
            Dim point3 As Point3d = New Point3d(-2.58783103793338, -0.26, 0.337140671457704)
            projectedViewBuilder1.Placement.AlignmentPoint.SetValue(point2, baseView1, point3)
            projectedViewBuilder1.Placement.AlignmentView.Value = baseView1
            Dim point4 As Point3d = New Point3d(7.63371886120996, 4.72126334519573, 0.0)
            projectedViewBuilder1.Placement.Placement.SetValue(Nothing, workPart.Views.WorkView, point4)
            Dim nXObject5 As NXObject
            nXObject5 = projectedViewBuilder1.Commit()
            projectedViewBuilder1.Destroy()
            workPart.Expressions.Delete(expression1)

            ' ----------------------------------------------
            '   Menu: Insert->View->View Creation Wizard...
            ' ----------------------------------------------
            Dim viewCreationWizardBuilder1 As Drawings.ViewCreationWizardBuilder
            viewCreationWizardBuilder1 = workPart.DraftingViews.CreateViewCreationWizardBuilder()
            viewCreationWizardBuilder1.MultipleViewPlacement.ViewPlacementCenter.Associative = True
            viewCreationWizardBuilder1.MultipleViewPlacement.ViewPlacementFirstCorner.Associative = True
            viewCreationWizardBuilder1.MultipleViewPlacement.ViewPlacementSecondCorner.Associative = True
            viewCreationWizardBuilder1.AssociativeAlignment = True
            viewCreationWizardBuilder1.BaseView = "FRONT"
            viewCreationWizardBuilder1.SpecialBaseView = False
            viewCreationWizardBuilder1.ViewRepresentation = Drawings.ViewCreationWizardBuilder.ViewRepresentations.SmartLightweight
            viewCreationWizardBuilder1.Resolution = Drawings.ViewCreationWizardBuilder.ResolutionOption.Medium
            Dim partLoadStatus2 As PartLoadStatus
            partLoadStatus2 = workPart.LoadFully()
            partLoadStatus2.Dispose()
            viewCreationWizardBuilder1.Part = workPart
            viewCreationWizardBuilder1.ViewStyle.ViewStyleBase.Arrangement.SelectedArrangement = nullAssemblies_Arrangement
            viewCreationWizardBuilder1.BaseView = "FRONT"
            viewCreationWizardBuilder1.ViewStyle.ViewStyleBase.Part = workPart
            viewCreationWizardBuilder1.ViewStyle.ViewStyleBase.PartName = "C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\" + filenameShrunk + ".prt"
            viewCreationWizardBuilder1.MarginToBorder = 0.75
            viewCreationWizardBuilder1.MarginBetweenViews = 0.25
            ' ----------------------------------------------
            '   Dialog Begin View Creation Wizard
            ' ----------------------------------------------
            ' ----------------------------------------------
            '   Dialog Begin View Creation Wizard
            ' ----------------------------------------------
            viewCreationWizardBuilder1.BaseView = "Isometric"
            viewCreationWizardBuilder1.SpecialBaseView = False
            viewCreationWizardBuilder1.MarginToBorder = 0.75
            viewCreationWizardBuilder1.MarginBetweenViews = 0.25
            viewCreationWizardBuilder1.ViewScale.Numerator = 2.0
            viewCreationWizardBuilder1.PlacementOption = Drawings.ViewCreationWizardBuilder.Option.Automatic
            viewCreationWizardBuilder1.MultipleViewPlacement.OptionType = Drawings.MultipleViewPlacementBuilder.Option.Center
            ' ----------------------------------------------
            '   Dialog Begin View Creation Wizard
            ' ----------------------------------------------
            viewCreationWizardBuilder1.TrimetricView = True
            viewCreationWizardBuilder1.MarginToBorder = 0.75
            viewCreationWizardBuilder1.MarginBetweenViews = 0.25
            viewCreationWizardBuilder1.ViewScale.Numerator = 1.0
            viewCreationWizardBuilder1.TrimetricView = False
            viewCreationWizardBuilder1.MarginToBorder = 0.75
            viewCreationWizardBuilder1.MarginBetweenViews = 0.25
            viewCreationWizardBuilder1.ViewScale.Numerator = 2.0
            ' ----------------------------------------------
            '   Dialog Begin View Creation Wizard
            ' ----------------------------------------------
            ' ----------------------------------------------
            '   Dialog Begin View Creation Wizard
            ' ----------------------------------------------

            Dim nXObject6 As NXObject
            nXObject6 = viewCreationWizardBuilder1.Commit()
            Dim objects1() As NXObject
            objects1 = viewCreationWizardBuilder1.GetCommittedObjects()
            viewCreationWizardBuilder1.Destroy()

            ' ----------------------------------------------
            '   Menu: Edit->View->Settings...
            ' ----------------------------------------------

            Dim views1(0) As NXopen.View
            Dim baseView2 As Drawings.BaseView = CType(objects1(0), Drawings.BaseView)
            views1(0) = baseView2
            Dim editViewSettingsBuilder1 As Drawings.EditViewSettingsBuilder
            editViewSettingsBuilder1 = workPart.SettingsManager.CreateDrawingEditViewSettingsBuilder(views1)

            Dim editsettingsbuilders1(0) As Drafting.BaseEditSettingsBuilder
            editsettingsbuilders1(0) = editViewSettingsBuilder1
            workPart.SettingsManager.ProcessForMutipleObjectsSettings(editsettingsbuilders1)
            ' ----------------------------------------------
            '   Dialog Begin Settings
            ' ----------------------------------------------
            editViewSettingsBuilder1.ViewStyle.ViewStyleGeneral.Scale.Numerator = 1.5

            Dim nXObject7 As NXObject
            nXObject7 = editViewSettingsBuilder1.Commit()
            editViewSettingsBuilder1.Destroy()
            Dim views2(0) As NXopen.View
            Dim baseView3 As Drawings.BaseView = CType(nXObject7, Drawings.BaseView)
            views2(0) = baseView3
            Dim editViewSettingsBuilder2 As Drawings.EditViewSettingsBuilder
            editViewSettingsBuilder2 = workPart.SettingsManager.CreateDrawingEditViewSettingsBuilder(views2)

            Dim editsettingsbuilders2(0) As Drafting.BaseEditSettingsBuilder
            editsettingsbuilders2(0) = editViewSettingsBuilder2
            workPart.SettingsManager.ProcessForMutipleObjectsSettings(editsettingsbuilders2)

            ' ----------------------------------------------
            '   Dialog Begin Settings
            ' ----------------------------------------------
            Dim nXObject8 As NXObject
            nXObject8 = editViewSettingsBuilder2.Commit()


            editViewSettingsBuilder2.Destroy()
            Dim baseView4 As Drawings.BaseView = CType(nXObject8, Drawings.BaseView)
            Dim drawingReferencePoint11 As Point3d = New Point3d(18.3721128643143, 5.15298695576163, 0.0)
            baseView4.MoveView(drawingReferencePoint11)
            Dim dispPart As Part = theSession.Parts.Display()
            Dim title As String = "CustomerName"
            Dim value As String = comboxCustomer.Text
            Try
                dispPart.SetAttribute(title, value)
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try
            title = "FileName"
            value = filenameShrunk
            Try
                dispPart.SetAttribute(title, value)
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try

            ' ----------------------------------------------
            '   Menu: Insert->Annotation->Note...
            ' ----------------------------------------------

            Dim nullAnnotations_SimpleDraftingAid As Annotations.SimpleDraftingAid = Nothing
            Dim draftingNoteBuilder1 As Annotations.DraftingNoteBuilder
            draftingNoteBuilder1 = workPart.Annotations.CreateDraftingNoteBuilder(nullAnnotations_SimpleDraftingAid)
            draftingNoteBuilder1.Origin.SetInferRelativeToGeometry(True)
            draftingNoteBuilder1.Origin.SetInferRelativeToGeometry(True)
            draftingNoteBuilder1.Origin.Anchor = Annotations.OriginBuilder.AlignmentPosition.BottomLeft
            Dim text1(3) As String
            text1(0) = "CUSTOMER:    " + comboxCustomer.Text
            text1(1) = "SALES:       " + comboxSales.Text
            text1(2) = "FILE:        " + filename
            text1(3) = "DATE:        " + System.DateTime.Now.ToString("MM/dd/yyyy")
            draftingNoteBuilder1.Text.TextBlock.SetText(text1)
            draftingNoteBuilder1.Text.TextBlock.SymbolPreferences = Annotations.TextWithSymbolsBuilder.SymbolPreferencesType.UseDefinition
            draftingNoteBuilder1.Origin.Plane.PlaneMethod = Annotations.PlaneBuilder.PlaneMethodType.XyPlane
            draftingNoteBuilder1.Origin.SetInferRelativeToGeometry(True)
            Dim leaderData1 As Annotations.LeaderData
            leaderData1 = workPart.Annotations.CreateLeaderData()
            leaderData1.StubSize = 0.125
            leaderData1.Arrowhead = Annotations.LeaderData.ArrowheadType.FilledArrow
            draftingNoteBuilder1.Leader.Leaders.Append(leaderData1)
            leaderData1.StubSide = Annotations.LeaderSide.Inferred
            Dim symbolscale1 As Double
            symbolscale1 = draftingNoteBuilder1.Text.TextBlock.SymbolScale
            Dim symbolaspectratio1 As Double
            symbolaspectratio1 = draftingNoteBuilder1.Text.TextBlock.SymbolAspectRatio
            draftingNoteBuilder1.Origin.SetInferRelativeToGeometry(True)
            draftingNoteBuilder1.Origin.SetInferRelativeToGeometry(True)
            Dim assocOrigin1 As Annotations.Annotation.AssociativeOriginData
            assocOrigin1.OriginType = Annotations.AssociativeOriginType.Drag
            Dim nullView As NXopen.View = Nothing
            assocOrigin1.View = nullView
            assocOrigin1.ViewOfGeometry = nullView
            Dim nullPoint As NXopen.Point = Nothing

            assocOrigin1.PointOnGeometry = nullPoint
            Dim nullAnnotations_Annotation As Annotations.Annotation = Nothing

            assocOrigin1.VertAnnotation = nullAnnotations_Annotation
            assocOrigin1.VertAlignmentPosition = Annotations.AlignmentPosition.TopLeft
            assocOrigin1.HorizAnnotation = nullAnnotations_Annotation
            assocOrigin1.HorizAlignmentPosition = Annotations.AlignmentPosition.TopLeft
            assocOrigin1.AlignedAnnotation = nullAnnotations_Annotation
            assocOrigin1.DimensionLine = 0
            assocOrigin1.AssociatedView = nullView
            assocOrigin1.AssociatedPoint = nullPoint
            assocOrigin1.OffsetAnnotation = nullAnnotations_Annotation
            assocOrigin1.OffsetAlignmentPosition = Annotations.AlignmentPosition.TopLeft
            assocOrigin1.XOffsetFactor = 0.0
            assocOrigin1.YOffsetFactor = 0.0
            assocOrigin1.StackAlignmentPosition = Annotations.StackAlignmentPosition.Above
            draftingNoteBuilder1.Origin.SetAssociativeOrigin(assocOrigin1)

            Dim point5 As Point3d = New Point3d(10.8947508896797, 8.87962633451957, 0.0)
            draftingNoteBuilder1.Origin.Origin.SetValue(Nothing, nullView, point5)

            draftingNoteBuilder1.Origin.SetInferRelativeToGeometry(True)


            Dim nXObject9 As NXObject
            nXObject9 = draftingNoteBuilder1.Commit()


            draftingNoteBuilder1.Destroy()

        End Sub
        Public Sub ImportStep()
            Dim theSession As Session = Session.GetSession()
            Dim step203Importer1 As Step203Importer
            step203Importer1 = theSession.DexManager.CreateStep203Importer()
            step203Importer1.SimplifyGeometry = True
            step203Importer1.LayerDefault = 1
            step203Importer1.SettingsFile = "C:\UGNX9\step203ug\step203ug.def"
            step203Importer1.ObjectTypes.Solids = True
            step203Importer1.InputFile = ""
            step203Importer1.OutputFile = ""
            step203Importer1.InputFile = txtBoxFilePath.Text
            step203Importer1.OutputFile = "C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\" + filenameShrunk + ".prt"
            step203Importer1.FileOpenFlag = False
            Dim nXObject2 As NXObject
            nXObject2 = step203Importer1.Commit()
            step203Importer1.Destroy()

            'Dim scaleAboutPoint1 As Point3d = New Point3d(-0.671875000000001, -0.369791666666667, 0.0)
            'Dim viewCenter1 As Point3d = New Point3d(0.671875000000001, 0.369791666666667, 0.0)
            'workPart.ModelingViews.WorkView.ZoomAboutPoint(0.8, scaleAboutPoint1, viewCenter1)

            'Dim scaleAboutPoint2 As Point3d = New Point3d(-0.839843750000001, -0.462239583333334, 0.0)
            'Dim viewCenter2 As Point3d = New Point3d(0.839843750000001, 0.462239583333334, 0.0)
            'workPart.ModelingViews.WorkView.ZoomAboutPoint(0.8, scaleAboutPoint2, viewCenter2)

            'Dim scaleAboutPoint3 As Point3d = New Point3d(-5.86751302083333, -0.480143229166667, 0.0)
            'Dim viewCenter3 As Point3d = New Point3d(5.86751302083333, 0.480143229166667, 0.0)
            'workPart.ModelingViews.WorkView.ZoomAboutPoint(1.25, scaleAboutPoint3, viewCenter3)

            'Dim scaleAboutPoint4 As Point3d = New Point3d(-4.69401041666667, -0.384114583333334, 0.0)
            'Dim viewCenter4 As Point3d = New Point3d(4.69401041666666, 0.384114583333334, 0.0)
            'workPart.ModelingViews.WorkView.ZoomAboutPoint(1.25, scaleAboutPoint4, viewCenter4)
        End Sub
        Public Sub ImportParaSolid()
            Dim theSession As Session = Session.GetSession()
            Dim workPart As Part = theSession.Parts.Work
            Dim importer1 As Importer
            importer1 = workPart.ImportManager.CreateParasolidImporter()
            importer1.FileName = txtBoxFilePath.Text
            Dim nXObject1 As NXObject
            nXObject1 = importer1.Commit()
            importer1.Destroy()
        End Sub
        Public Sub importIges()
            Dim theSession As Session = Session.GetSession()
            Dim igesImporter1 As IgesImporter
            igesImporter1 = theSession.DexManager.CreateIgesImporter()
            igesImporter1.CopiousData = IgesImporter.CopiousDataEnum.LinearNURBSpline
            igesImporter1.SmoothBSurf = True
            igesImporter1.LayerDefault = 1
            igesImporter1.SurfTrimTol = 0.001
            igesImporter1.GeomFixupTol = 0.0005
            igesImporter1.ObjectTypes.Curves = True
            igesImporter1.ObjectTypes.Surfaces = True
            igesImporter1.ObjectTypes.Annotations = True
            igesImporter1.ObjectTypes.Structures = True
            igesImporter1.ObjectTypes.Solids = True
            igesImporter1.SimplifyGeometry = True
            igesImporter1.SettingsFile = "C:\UGNX9\iges\igesimport.def"
            igesImporter1.InputFile = txtBoxFilePath.Text
            igesImporter1.OutputFile = "C:\Users\" + System.Environment.UserName + "\Documents\Quotation Drawings\" + filenameShrunk + ".prt"
            igesImporter1.FileOpenFlag = False
            igesImporter1.LayerMask = "0-99999"
            Dim nXObject1 As NXObject
            nXObject1 = igesImporter1.Commit()
            igesImporter1.Destroy()
        End Sub
    End Class
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class Form1
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
            Me.txtBoxFilePath = New System.Windows.Forms.TextBox()
            Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
            Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
            Me.btnFind = New System.Windows.Forms.Button()
            Me.comboxFileType = New System.Windows.Forms.ComboBox()
            Me.btnOk = New System.Windows.Forms.Button()
            Me.btnCancel = New System.Windows.Forms.Button()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.Label2 = New System.Windows.Forms.Label()
            Me.Label3 = New System.Windows.Forms.Label()
            Me.Label4 = New System.Windows.Forms.Label()
            Me.comboxCustomer = New System.Windows.Forms.ComboBox()
            Me.comboxSales = New System.Windows.Forms.ComboBox()
            Me.Label5 = New System.Windows.Forms.Label()
            Me.txtboxQuoteNum = New System.Windows.Forms.TextBox()
            Me.btnExport = New System.Windows.Forms.Button()
            Me.Label6 = New System.Windows.Forms.Label()
            Me.Label7 = New System.Windows.Forms.Label()
            Me.Label8 = New System.Windows.Forms.Label()
            Me.Label9 = New System.Windows.Forms.Label()
            Me.Label10 = New System.Windows.Forms.Label()
            Me.SuspendLayout()
            '
            'txtBoxFilePath
            '
            Me.txtBoxFilePath.Location = New System.Drawing.Point(12, 25)
            Me.txtBoxFilePath.Name = "txtBoxFilePath"
            Me.txtBoxFilePath.Size = New System.Drawing.Size(166, 20)
            Me.txtBoxFilePath.TabIndex = 1
            '
            'OpenFileDialog1
            '
            Me.OpenFileDialog1.FileName = "OpenFileDialog1"
            '
            'btnFind
            '
            Me.btnFind.BackColor = System.Drawing.Color.Gold
            Me.btnFind.Location = New System.Drawing.Point(184, 25)
            Me.btnFind.Name = "btnFind"
            Me.btnFind.Size = New System.Drawing.Size(75, 23)
            Me.btnFind.TabIndex = 5
            Me.btnFind.Text = "Find"
            Me.btnFind.UseVisualStyleBackColor = False
            '
            'comboxFileType
            '
            Me.comboxFileType.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
            Me.comboxFileType.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
            Me.comboxFileType.FormattingEnabled = True
            Me.comboxFileType.Location = New System.Drawing.Point(12, 75)
            Me.comboxFileType.Name = "comboxFileType"
            Me.comboxFileType.Size = New System.Drawing.Size(246, 21)
            Me.comboxFileType.TabIndex = 10
            '
            'btnOk
            '
            Me.btnOk.BackColor = System.Drawing.Color.ForestGreen
            Me.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.btnOk.ForeColor = System.Drawing.Color.Gold
            Me.btnOk.Location = New System.Drawing.Point(42, 255)
            Me.btnOk.Name = "btnOk"
            Me.btnOk.Size = New System.Drawing.Size(84, 32)
            Me.btnOk.TabIndex = 25
            Me.btnOk.Text = "Import"
            Me.btnOk.UseVisualStyleBackColor = False
            '
            'btnCancel
            '
            Me.btnCancel.BackColor = System.Drawing.Color.DarkRed
            Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.btnCancel.ForeColor = System.Drawing.Color.Gold
            Me.btnCancel.Location = New System.Drawing.Point(90, 293)
            Me.btnCancel.Name = "btnCancel"
            Me.btnCancel.Size = New System.Drawing.Size(92, 37)
            Me.btnCancel.TabIndex = 30
            Me.btnCancel.Text = "Cancel"
            Me.btnCancel.UseVisualStyleBackColor = False
            '
            'Label1
            '
            Me.Label1.AutoSize = True
            Me.Label1.Location = New System.Drawing.Point(14, 59)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(50, 13)
            Me.Label1.TabIndex = 101
            Me.Label1.Text = "File Type"
            '
            'Label2
            '
            Me.Label2.AutoSize = True
            Me.Label2.Location = New System.Drawing.Point(14, 9)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(67, 13)
            Me.Label2.TabIndex = 100
            Me.Label2.Text = "File Location"
            '
            'Label3
            '
            Me.Label3.AutoSize = True
            Me.Label3.Location = New System.Drawing.Point(12, 110)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New System.Drawing.Size(82, 13)
            Me.Label3.TabIndex = 102
            Me.Label3.Text = "Customer Name"
            '
            'Label4
            '
            Me.Label4.AutoSize = True
            Me.Label4.Location = New System.Drawing.Point(14, 163)
            Me.Label4.Name = "Label4"
            Me.Label4.Size = New System.Drawing.Size(64, 13)
            Me.Label4.TabIndex = 103
            Me.Label4.Text = "Sales Name"
            '
            'comboxCustomer
            '
            Me.comboxCustomer.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
            Me.comboxCustomer.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
            Me.comboxCustomer.FormattingEnabled = True
            Me.comboxCustomer.Location = New System.Drawing.Point(15, 126)
            Me.comboxCustomer.Name = "comboxCustomer"
            Me.comboxCustomer.Size = New System.Drawing.Size(246, 21)
            Me.comboxCustomer.TabIndex = 15
            '
            'comboxSales
            '
            Me.comboxSales.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
            Me.comboxSales.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
            Me.comboxSales.FormattingEnabled = True
            Me.comboxSales.Location = New System.Drawing.Point(12, 179)
            Me.comboxSales.Name = "comboxSales"
            Me.comboxSales.Size = New System.Drawing.Size(246, 21)
            Me.comboxSales.TabIndex = 20
            '
            'Label5
            '
            Me.Label5.AutoSize = True
            Me.Label5.Location = New System.Drawing.Point(14, 213)
            Me.Label5.Name = "Label5"
            Me.Label5.Size = New System.Drawing.Size(76, 13)
            Me.Label5.TabIndex = 104
            Me.Label5.Text = "Quote Number"
            '
            'txtboxQuoteNum
            '
            Me.txtboxQuoteNum.Location = New System.Drawing.Point(12, 229)
            Me.txtboxQuoteNum.Name = "txtboxQuoteNum"
            Me.txtboxQuoteNum.Size = New System.Drawing.Size(246, 20)
            Me.txtboxQuoteNum.TabIndex = 21
            '
            'btnExport
            '
            Me.btnExport.BackColor = System.Drawing.Color.Gold
            Me.btnExport.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.btnExport.ForeColor = System.Drawing.SystemColors.WindowText
            Me.btnExport.Location = New System.Drawing.Point(158, 257)
            Me.btnExport.Name = "btnExport"
            Me.btnExport.Size = New System.Drawing.Size(81, 28)
            Me.btnExport.TabIndex = 29
            Me.btnExport.Text = "Export"
            Me.btnExport.UseVisualStyleBackColor = False
            '
            'Label6
            '
            Me.Label6.AutoSize = True
            Me.Label6.ForeColor = System.Drawing.Color.Green
            Me.Label6.Location = New System.Drawing.Point(87, 9)
            Me.Label6.Name = "Label6"
            Me.Label6.Size = New System.Drawing.Size(72, 13)
            Me.Label6.TabIndex = 107
            Me.Label6.Text = "- (Import Only)"
            '
            'Label7
            '
            Me.Label7.AutoSize = True
            Me.Label7.ForeColor = System.Drawing.Color.Green
            Me.Label7.Location = New System.Drawing.Point(70, 59)
            Me.Label7.Name = "Label7"
            Me.Label7.Size = New System.Drawing.Size(72, 13)
            Me.Label7.TabIndex = 108
            Me.Label7.Text = "- (Import Only)"
            '
            'Label8
            '
            Me.Label8.AutoSize = True
            Me.Label8.ForeColor = System.Drawing.Color.Green
            Me.Label8.Location = New System.Drawing.Point(84, 163)
            Me.Label8.Name = "Label8"
            Me.Label8.Size = New System.Drawing.Size(72, 13)
            Me.Label8.TabIndex = 109
            Me.Label8.Text = "- (Import Only)"
            '
            'Label9
            '
            Me.Label9.AutoSize = True
            Me.Label9.ForeColor = System.Drawing.Color.Green
            Me.Label9.Location = New System.Drawing.Point(100, 110)
            Me.Label9.Name = "Label9"
            Me.Label9.Size = New System.Drawing.Size(72, 13)
            Me.Label9.TabIndex = 110
            Me.Label9.Text = "- (Import Only)"
            '
            'Label10
            '
            Me.Label10.AutoSize = True
            Me.Label10.ForeColor = System.Drawing.Color.Gold
            Me.Label10.Location = New System.Drawing.Point(91, 213)
            Me.Label10.Name = "Label10"
            Me.Label10.Size = New System.Drawing.Size(73, 13)
            Me.Label10.TabIndex = 111
            Me.Label10.Text = "- (Export Only)"
            '
            'Form1
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.BackColor = System.Drawing.SystemColors.AppWorkspace
            Me.ClientSize = New System.Drawing.Size(271, 340)
            Me.Controls.Add(Me.Label10)
            Me.Controls.Add(Me.Label9)
            Me.Controls.Add(Me.Label8)
            Me.Controls.Add(Me.Label7)
            Me.Controls.Add(Me.Label6)
            Me.Controls.Add(Me.btnExport)
            Me.Controls.Add(Me.txtboxQuoteNum)
            Me.Controls.Add(Me.Label5)
            Me.Controls.Add(Me.comboxSales)
            Me.Controls.Add(Me.comboxCustomer)
            Me.Controls.Add(Me.Label4)
            Me.Controls.Add(Me.Label3)
            Me.Controls.Add(Me.Label2)
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.btnCancel)
            Me.Controls.Add(Me.btnOk)
            Me.Controls.Add(Me.comboxFileType)
            Me.Controls.Add(Me.btnFind)
            Me.Controls.Add(Me.txtBoxFilePath)
            Me.Name = "Form1"
            Me.Text = "Quotation Drawing"
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Friend WithEvents txtBoxFilePath As TextBox
        Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
        Friend WithEvents OpenFileDialog1 As OpenFileDialog
        Friend WithEvents btnFind As Button
        Friend WithEvents comboxFileType As ComboBox
        Friend WithEvents btnOk As Button
        Friend WithEvents btnCancel As Button
        Friend WithEvents Label1 As Label
        Friend WithEvents Label2 As Label
        Friend WithEvents Label3 As Label
        Friend WithEvents Label4 As Label
        Friend WithEvents comboxCustomer As ComboBox
        Friend WithEvents comboxSales As ComboBox
        Friend WithEvents Label5 As Label
        Friend WithEvents txtboxQuoteNum As TextBox
        Friend WithEvents btnExport As Button
        Friend WithEvents Label6 As Label
        Friend WithEvents Label7 As Label
        Friend WithEvents Label8 As Label
        Friend WithEvents Label9 As Label
        Friend WithEvents Label10 As Label
    End Class

End Module
