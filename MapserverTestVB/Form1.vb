Public Class Form1

    Public rootEXEPath As String = System.Reflection.Assembly.GetExecutingAssembly.Location.ToUpper
    Public ImageLocation As String = "tmp\"
    Public DataLocation As String = "data\"
    Public MSMap As mapObj
    ' Public MaxExtent As rectObj
    Public ClickCount As Integer = 0
    Public FirstX As Integer = -1
    Public FirstY As Integer = -1

    Public NumLayers = -1

    Private Enum RESULT_CODE As Integer
        MS_SUCCESS = 0
        MS_FAILURE = 1
    End Enum

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        rootEXEPath = rootEXEPath.Substring(0, rootEXEPath.LastIndexOf("\BIN\") + 1)

        Dim mapfile As String = "I:\My DropBox\Visual Studio 2005\MapserverTestVB_Tamas\mapBasic.map"
        MSMap = New mapObj(mapfile)
        MSMap.shapepath = rootEXEPath & DataLocation
        'MaxExtent = New rectObj(MSMap.extent.minx, MSMap.extent.miny, MSMap.extent.maxx, MSMap.extent.maxy, 0)

        Dim img As imageObj
        img = MSMap.draw
        NumLayers = MSMap.numlayers

        RefreshImage()
    End Sub


    Public Sub RefreshImage()
        Dim imageLoc As String
        Dim img As imageObj
        img = MSMap.draw
        imageLoc = rootexepath & ImageLocation & Math.Abs(Now.ToBinary) & "." & img.format.extension
        img.save(imageLoc, Nothing)

        Dim BM As New Bitmap(imageLoc)

        With Me.PictureBox1
            .Width = BM.Width
            .Height = BM.Height
            .Image = BM
        End With
    End Sub

    Private Sub PictureBox1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseUp
        Dim result As Integer
        Dim MouseArgs As System.Windows.Forms.MouseEventArgs = DirectCast(e, System.Windows.Forms.MouseEventArgs)
        Dim ClickPt As New pointObj(MouseArgs.X, MouseArgs.Y, Nothing, Nothing)
        Select Case MapAction.SelectedText.ToUpper
            Case "ZOOMIN"
                MSMap.zoomPoint(2, ClickPt, MSMap.width, MSMap.height, MSMap.extent, Nothing)
            Case "ZOOMOUT"
                MSMap.zoomPoint(-2, ClickPt, MSMap.width, MSMap.height, MSMap.extent, Nothing)
            Case "PAN"
                MSMap.zoomPoint(1, ClickPt, MSMap.width, MSMap.height, MSMap.extent, Nothing)
            Case "BOX"
                If ClickCount < 1 Then
                    ClickCount += 1
                    FirstX = MouseArgs.X
                    FirstY = MouseArgs.Y
                Else
                    ClickCount = 0

                    Dim minX, minY, maxX, maxY As Integer
                    If FirstX < MouseArgs.X Then
                        minX = FirstX
                        maxX = MouseArgs.X
                    Else
                        minX = MouseArgs.X
                        maxX = FirstX
                    End If

                    If FirstY < MouseArgs.Y Then
                        minY = FirstY
                        maxY = MouseArgs.Y
                    Else
                        minY = MouseArgs.Y
                        maxY = FirstY
                    End If
                    Dim thisRect As New rectObj(0, 0, 1, 1, 0)
                    thisRect.minx = minX
                    thisRect.miny = maxY
                    thisRect.maxx = maxX
                    thisRect.maxy = minY

                    selectRect(thisRect)

                    'MSMap.zoomRectangle(thisRect, MSMap.width, MSMap.height, MSMap.extent, Nothing)
                End If
            Case "ID"
                ID(ClickPt)
            Case "QUERY"
                QUERY()
        End Select

        RefreshImage()
    End Sub

    Private Sub selectRect(ByVal r As rectObj)

        Dim d As Double
        If r.minx > r.maxx Then
            d = r.minx
            r.minx = r.maxx
            r.maxx = d
        End If
        If r.miny > r.maxy Then
            d = r.miny
            r.miny = r.maxy
            r.maxy = d
        End If
        Dim ptTemp1 As New pointObj(r.minx, r.miny, Nothing, Nothing)
        Dim ptTemp2 As New pointObj(r.maxx, r.maxy, Nothing, Nothing)
        ptTemp1 = Pixel2Geo(ptTemp1)
        ptTemp2 = Pixel2Geo(ptTemp2)
        If ptTemp1.x > ptTemp2.x Then
            d = ptTemp1.x
            ptTemp1.x = ptTemp2.x
            ptTemp2.x = d
        End If
        If ptTemp1.y > ptTemp2.y Then
            d = ptTemp1.y
            ptTemp1.y = ptTemp2.y
            ptTemp2.y = d
        End If

        Dim aRect As New rectObj(ptTemp1.x, ptTemp1.y, ptTemp2.x, ptTemp2.y, 0)
        
        Dim result As Integer
        Dim lyr As layerObj
        lyr = MSMap.getLayerByName("county")

        Dim s As New System.Text.StringBuilder

        result = lyr.queryByRect(MSMap, aRect)
        If result = RESULT_CODE.MS_SUCCESS Then
            Dim resCache As resultCacheObj = lyr.getResults

            If resCache.numresults > 0 Then
                Dim shp As shapeObj
                Dim fName As String
                lyr.open()
                For i As Integer = 0 To resCache.numresults - 1
                    shp = lyr.getFeature(resCache.getResult(i).shapeindex, resCache.getResult(i).tileindex)
                    For iItem As Integer = 0 To lyr.numitems - 1
                        fName = lyr.getItem(iItem)
                        With s
                            .Append(fName)
                            .Append("=")
                            .Append(shp.getValue(iItem))
                            .Append(", ")
                        End With
                    Next
                Next
                lyr.close()
            End If
        End If


    End Sub

    Private Sub Query()
        Dim result As Integer
        Dim lyr As layerObj
        lyr = MSMap.getLayerByName("county")
        Dim s As New System.Text.StringBuilder

        result = lyr.queryByAttributes(MSMap, "NAME", "ADAIR or ALFALFA", 1)

        If result = RESULT_CODE.MS_SUCCESS Then
            Dim resCache As resultCacheObj = lyr.getResults

            If resCache.numresults > 0 Then
                Dim shp As shapeObj
                Dim fName As String
                lyr.open()
                For i As Integer = 0 To resCache.numresults - 1
                    shp = lyr.getFeature(resCache.getResult(i).shapeindex, resCache.getResult(i).tileindex)
                    For iItem As Integer = 0 To lyr.numitems - 1
                        fName = lyr.getItem(iItem)
                        With s
                            .Append(fName)
                            .Append("=")
                            .Append(shp.getValue(iItem))
                            .Append(", ")
                        End With
                    Next
                Next
                lyr.close()
            End If
        End If
    End Sub
    Private Sub ID(ByVal ClickPt As pointObj)
        Dim result As Integer
        Dim lyr As layerObj
        lyr = MSMap.getLayerByName("county")

        Dim geoPT As pointObj = Pixel2Geo(ClickPt)
        AddCircleLayer(geoPT, 10000)
        Dim s As New System.Text.StringBuilder

        result = lyr.queryByPoint(MSMap, geoPT, 1, 1)
        If result = RESULT_CODE.MS_SUCCESS Then
            Dim resCache As resultCacheObj = lyr.getResults

            If resCache.numresults > 0 Then
                Dim shp As shapeObj
                Dim fName As String
                lyr.open()
                For i As Integer = 0 To resCache.numresults - 1
                    shp = lyr.getFeature(resCache.getResult(i).shapeindex, resCache.getResult(i).tileindex)
                    For iItem As Integer = 0 To lyr.numitems - 1
                        fName = lyr.getItem(iItem)
                        With s
                            .Append(fName)
                            .Append("=")
                            .Append(shp.getValue(iItem))
                            .Append(", ")
                        End With
                    Next
                Next
                lyr.close()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Resultant point is in GEO coords
    ''' </summary>
    ''' <param name="pt">Image Coords</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function Pixel2Geo(ByVal pt As pointObj) As pointObj
        Dim imgXMin, imgXMax, imgYMin, imgYMax As Integer
        Dim geoXMin, geoXMax, geoYMin, geoYMax As Double

        '//init corner coords
        imgXMin = 0
        imgYMin = 0
        imgXMax = MSMap.width
        imgYMax = MSMap.height


        geoXMin = MSMap.extent.minx
        geoXMax = MSMap.extent.maxx
        geoYMin = MSMap.extent.miny
        geoYMax = MSMap.extent.maxy

        Dim imgWidth, imgHeight As Integer
        Dim geoWidth, geoHeight As Double

        '//calc the width
        imgWidth = imgXMax - imgXMin
        imgHeight = imgYMax - imgYMin

        If geoXMin < geoXMax Then
            geoWidth = geoXMax - geoXMin
        Else
            geoWidth = geoXMin - geoXMax
        End If
        If geoYMin < geoYMax Then
            geoHeight = geoYMax - geoYMin
        Else
            geoHeight = geoYMin - geoYMax
        End If

        '//calc the percent along each axis
        Dim xPercent, yPercent As Double
        xPercent = pt.x / imgWidth
        yPercent = pt.y / imgHeight


        Dim newX, newY As Double
        newX = (xPercent * geoWidth) + geoXMin
        newY = geoYMax - (yPercent * geoHeight)

        Pixel2Geo = New pointObj(newX, newY, Nothing, Nothing)



    End Function


    Private Function AddCircleLayer(ByVal pt As pointObj, ByVal radius As Double) As layerObj

        While MSMap.numlayers > NumLayers
            MSMap.removeLayer(MSMap.numlayers - 1)
        End While


        Dim lyr As New layerObj(MSMap)
        lyr.type = MS_LAYER_TYPE.MS_LAYER_CIRCLE

        Dim minX, maxX, minY, maxY As Double
        minX = pt.x - radius
        maxX = pt.x + radius
        minY = pt.y - radius
        maxY = pt.y + radius
        Dim cirExt As New rectObj(minX, minY, maxX, maxY, 0)

        Dim aPt As pointObj
        Dim aLine As New lineObj()
        Dim extShp As New shapeObj(MS_SHAPE_TYPE.MS_SHAPE_POINT)

        aPt = New pointObj(cirExt.minx, cirExt.miny, Nothing, Nothing)
        aLine.add(aPt)
        aPt = New pointObj(cirExt.maxx, cirExt.maxy, Nothing, Nothing)
        aLine.add(aPt)

        extShp.add(aLine)
        
        lyr.addFeature(extShp)
        lyr.status = 1

        Dim c As New classObj(lyr)
        Dim sty As New styleObj(c)
        sty.color = New colorObj(255, 0, 0, 1)
        lyr.transparency = 60

    End Function

    Private Sub PictureBox1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox1.Click

    End Sub

    Private Sub MapAction_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MapAction.SelectedIndexChanged

    End Sub
End Class
