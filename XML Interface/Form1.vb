Imports System
Imports System.Xml
Imports System.IO
Public Class Form1
    Dim CurrentSession As Session
    Dim AllCars As List(Of Car) = New List(Of Car)
    Dim inputlocation As String
    Dim outputlocation As String
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        startstop()
    End Sub

    Private Sub startstop()
        If Button1.Text = "Start" Then
            Button1.Text = "Stop Running"
            Button1.BackColor = Color.Red
            Timer1.Enabled = True

        Else
            Button1.Text = "Start"
            Button1.BackColor = Color.LimeGreen
            Timer1.Enabled = False
        End If
    End Sub

    Private Sub ReadXML()
        Try
            AllCars.Clear()

            Dim Sr As StreamReader = New StreamReader(inputlocation & "\racedata.xml")



            Dim StreamData As String = Sr.ReadToEnd
            Dim xmlDoc As New XmlDocument()
            xmlDoc.LoadXml(StreamData)
            Dim carNodes As XmlNodeList = xmlDoc.SelectNodes("//CAR")
            Dim SessionNodes As XmlNodeList = xmlDoc.SelectNodes("//SESSION")

            For Each sessionnode As XmlNode In SessionNodes
                Dim s As New Session
                s.Info = sessionnode.SelectSingleNode("INFO")?.InnerText
                s.Mode = sessionnode.SelectSingleNode("MODE")?.InnerText
                s.Flag = sessionnode.SelectSingleNode("FLAG")?.InnerText
                s.LastTime = sessionnode.SelectSingleNode("LASTTIME")?.InnerText
                s.Lap = sessionnode.SelectSingleNode("LAP")?.InnerText
                s.LapsToGo = sessionnode.SelectSingleNode("LAPSTOGO")?.InnerText
                s.TimeRemaining = sessionnode.SelectSingleNode("TIMEREMAINING")?.InnerText
                s.StartTime = sessionnode.SelectSingleNode("STARTTIME")?.InnerText

                s.LastTime = FormatDecimal(s.LastTime)
                s.TimeRemaining = FormatTotalTime(s.TimeRemaining)


                CurrentSession = s
            Next

            For Each carNode As XmlNode In carNodes
                Dim x As New Car
                x.Position = carNode.SelectSingleNode("POSITION")?.InnerText
                x.Name = carNode.SelectSingleNode("NAME")?.InnerText
                x.CarNumber = carNode.SelectSingleNode("CARNO")?.InnerText
                x.Laps = carNode.SelectSingleNode("LAPS")?.InnerText
                x.Lag = carNode.SelectSingleNode("LAG")?.InnerText
                x.LastLap = carNode.SelectSingleNode("LASTLAP")?.InnerText
                Try
                    x.FastTime = carNode.SelectSingleNode("FASTTIME")?.InnerText
                Catch
                End Try
                x.Misc = carNode.SelectSingleNode("MISC")?.InnerText
                x.TXID = carNode.SelectSingleNode("TXID")?.InnerText
                x.TotalTime = carNode.SelectSingleNode("TOTALTIME")?.InnerText

                x.Lag = FormatDecimal(x.Lag)
                x.LastLap = FormatDecimal(x.LastLap)
                x.FastTime = FormatDecimal(x.FastTime)
                x.TotalTime = FormatTotalTime(x.TotalTime)


                AllCars.Add(x)
            Next
        Catch
            startstop()
            MsgBox("Error Reading input XML")
        End Try
    End Sub
    Function FormatDecimal(input As String) As String
        Dim number As Double
        If Double.TryParse(input, number) Then
            Return number.ToString("F3")
        Else
            Return input ' Return original if parsing fails
        End If
    End Function


    Function FormatTotalTime(input As String) As String
        Dim parts() As String = input.Split(":"c)
        If parts.Length = 2 AndAlso parts(1).Length > 2 Then
            Dim seconds As String = FormatDecimal(parts(1))
            Return $"{parts(0)}:{seconds}"
        Else
            Return input ' Return original if format is unexpected
        End If
    End Function

    Private Sub GenerateTopFive()
        Try
            Dim Place As Integer = 1
            Dim SW As StreamWriter = New StreamWriter(outputlocation & "\top5.txt", False)
            For Each x As Car In AllCars
                If x.Position = Place Then

                    If Place = 4 Then
                        SW.Close()
                        Return
                    End If

                    If Place = "3" Then
                        Dim nameParts As String() = x.Name.Split(" "c)
                        Dim lastName As String = If(nameParts.Length > 1, String.Join(" ", nameParts.Skip(1)), x.Name)
                        Place = Place + 1
                    End If

                    If Place = "2" Then
                        Dim nameParts As String() = x.Name.Split(" "c)
                        Dim lastName As String = If(nameParts.Length > 1, String.Join(" ", nameParts.Skip(1)), x.Name)
                        Place = Place + 1
                    End If

                    If Place = "1" Then
                        Dim nameParts As String() = x.Name.Split(" "c)
                        Dim lastName As String = If(nameParts.Length > 1, String.Join(" ", nameParts.Skip(1)), x.Name)
                        SW.WriteLine(Place & ") " & x.CarNumber & "-" & lastName)
                        Place = Place + 1
                    End If


                End If
            Next
        Catch
            startstop()
            MsgBox("Error Writing to Output Location")

        End Try
    End Sub

    Private Sub GenerateQualifying()
        Dim Place As Integer = 1

        Try
            Dim SW As StreamWriter = New StreamWriter(outputlocation & "\Qualifying.txt", False)

            AllCars = AllCars.OrderBy(Function(x) x.Position).ToList()
                For Each x As Car In AllCars
                    Dim nameParts As String() = x.Name.Split(" "c)
                    Dim lastName As String = If(nameParts.Length > 1, String.Join(" ", nameParts.Skip(1)), x.Name)
                    SW.WriteLine($"{x.Position}) {x.CarNumber}-{lastName} - {x.FastTime}")
                Next
            SW.Close()
        Catch ex As Exception
            startstop()
            MsgBox("Error Writing to Output Location")
        End Try

    End Sub

    Private Sub GenerateQuickTime()
        Try
            AllCars = AllCars.Where(Function(x) x.FastTime <> Nothing AndAlso x.FastTime > 0).OrderBy(Function(x) x.FastTime).ToList()
            Dim c As Car = AllCars(0)
            Dim nameParts As String() = AllCars(0).Name.Split(" "c)
            Dim lastName As String = If(nameParts.Length > 1, String.Join(" ", nameParts.Skip(1)), AllCars(0).Name)
            Dim QTString As String
            Dim sw As StreamWriter = New StreamWriter(outputlocation & "\Quick Time.txt", False)
            sw.WriteLine("QT " & c.CarNumber & "-" & lastName & " (" & c.FastTime & ")")
            sw.Close()
        Catch
            startstop()
            MsgBox("Error Writing to Output Location")
        End Try

    End Sub

    Private Sub GenerateScrolling()
        Try
            AllCars = AllCars.OrderBy(Function(x) x.Position).ToList
            Dim CurrentLap As String = AllCars(0).Laps
            Dim ScrollString As String
            ScrollString = ScrollString & "RO Lap " & CurrentLap & "] "
            For Each x As Car In AllCars
                Dim nameParts As String() = x.Name.Split(" "c)
                Dim lastName As String = If(nameParts.Length > 1, String.Join(" ", nameParts.Skip(1)), x.Name)
                ScrollString = ScrollString & x.Position & ") " & x.CarNumber & "-" & lastName & "  "

            Next
            Dim sw As StreamWriter = New StreamWriter(outputlocation & "\Scrolling.txt", False)
            sw.WriteLine(ScrollString)
            sw.Close()
        Catch
            startstop()
            MsgBox("Error Writing to Output Location")
        End Try
    End Sub

    Private Sub GenerateLapsIn()
        Try
            Dim CurrentLap As String
            Dim Lag As String

            For Each x As Car In AllCars
                If x.Position = "1" Then
                    CurrentLap = x.Laps
                End If

                If x.Position = "2" Then
                    Lag = x.Lag.replace("-", "+")

                End If
            Next

            Dim SW As StreamWriter = New StreamWriter(outputlocation & "\laps in.txt", False)
            SW.WriteLine("Lap " & CurrentLap & "/" & CurrentSession.Lap & "  " & Lag)
            SW.Close()
        Catch
            startstop()
            MsgBox("Error Writing to Output Location")

        End Try
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load

        If File.Exists("Settings.cfg") Then
            Dim sr As StreamReader = New StreamReader("Settings.cfg")
            inputlocation = sr.ReadLine
            outputlocation = sr.ReadLine
            sr.Close()
            InputTextbox.Text = inputlocation
            OutputTextbox.Text = outputlocation
        Else
            File.Create("Settings.cfg")
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim folderBrowserDialog As New FolderBrowserDialog()

        ' Set the description for the dialog
        folderBrowserDialog.Description = "Select a folder to save files"

        ' Optionally set the initial directory
        folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyDocuments

        ' Show the dialog and check if the user selected a folder
        If folderBrowserDialog.ShowDialog() = DialogResult.OK Then
            ' Get the selected folder path
            Dim selectedPath As String = folderBrowserDialog.SelectedPath

            InputTextbox.Text = selectedPath
            inputlocation = selectedPath

            SaveSettings()
        End If
    End Sub

    Private Sub SaveSettings()
        Dim sr As StreamWriter = New StreamWriter("Settings.cfg", False)
        sr.WriteLine(inputlocation)
        sr.WriteLine(outputlocation)
        sr.Close()
        InputTextbox.Text = inputlocation
        OutputTextbox.Text = outputlocation
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim folderBrowserDialog As New FolderBrowserDialog()

        ' Set the description for the dialog
        folderBrowserDialog.Description = "Select a folder to save files"

        ' Optionally set the initial directory
        folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyDocuments

        ' Show the dialog and check if the user selected a folder
        If folderBrowserDialog.ShowDialog() = DialogResult.OK Then
            ' Get the selected folder path
            Dim selectedPath As String = folderBrowserDialog.SelectedPath

            OutputTextbox.Text = selectedPath
            outputlocation = selectedPath

            SaveSettings()
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick


        If pbar.Value >= 100 Then
            ReadXML()
            GenerateTopFive()
            GenerateScrolling()
            GenerateQuickTime()
            GenerateLapsIn()
            GenerateQualifying()
            pbar.Value = 0
        End If
        pbar.PerformStep()
    End Sub
End Class
