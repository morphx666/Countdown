Module ModuleMain
    Sub Main(ByVal ParamArray args() As String)
        Const margin As String = "    "
        Dim input As String = ""
        Dim target As Double
        Dim source() As Double = {}
        Dim showProgress As Boolean = False
        Dim findAll As Boolean = False
        Dim pause As Boolean = False
        Dim ignoreErrors As Boolean = False
        Dim iter As Integer = 0
        Dim solutionsFound As Integer = 0
        Dim navigator As System.Xml.XPath.XPathNavigator = New System.Xml.XPath.XPathDocument(New IO.StringReader("<r/>")).CreateNavigator()
        Dim rex As System.Text.RegularExpressions.Regex = New System.Text.RegularExpressions.Regex("([\+\-\*])")
        Dim Evaluator As Func(Of String, Double) = Function(exp) CDbl(navigator.Evaluate("number(" + rex.Replace(exp, " ${1} ").Replace("/", " div ").Replace("%", " mod ") + ")"))
        Dim result As Double
        Dim numbers() As Double
        Dim swPermutations As Stopwatch = New Stopwatch()
        Dim swProcess As Stopwatch = New Stopwatch()
        Dim precision As Double = 0
        Dim NumberToString = Function(n As Double)
                                 If n >= 0 Then
                                     Return n.ToString()
                                 Else
                                     Return "(" + n.ToString() + ")"
                                 End If
                             End Function
        Dim infoOffset As Integer = -1
#If DEBUG Then
        ReDim args(0)
        args(0) = "TARGET:10;SOURCE:1,1,5,8;"
		'args(0) = "TARGET:20;SOURCE:3,5,7,9;"
        'args(0) = "TARGET:952;SOURCE:25,50,75,100,3,6;"
        ' One solution: ((100+6)*3*75-50)/25 
        ' http://www.vidaddict.com/incredible-numbers-countdown/

        'args(0) = "TARGET:120;SOURCE:2,5;"
        'args(0) = "TARGET:104;SOURCE:100,3,4,7,1,0,5,8;"
        'args(0) = "TARGET:5;SOURCE:1,2,3,5;"
        'args(0) = "TARGET:-33.5;SOURCE:39,6,2;"
        'args(0) = "TARGET:0;SOURCE:6,0,1,5,5.5;"
        'args(0) = "TARGET:50.5;SOURCE:1,2,3,4,5,6.2,7;"
        'args(0) = "TARGET:5;SOURCE:1,1,1,1,1,10,1,1,1,1,1;"
#End If
        'showProgress = True
        'findAll = True

        Console.Clear()
        Console.Write("Countdown {0}.{1}",
                    My.Application.Info.Version.Major,
                    My.Application.Info.Version.Minor)

        If args.Count = 0 Then
            ShowHelp()
            Exit Sub
        Else
            Try
                For Each arg In args
                    If arg = "/sp" Then showProgress = True : Continue For
                    If arg = "/all" Then findAll = True : Continue For
                    If arg = "/w" Then pause = True : Continue For
                    If arg = "/e" Then ignoreErrors = True : Continue For
                    If arg.StartsWith("/p:") Then
                        Dim p As Integer = Integer.Parse(arg.Split(":"c)(1))
                        If p = 0 Then
                            precision = 0
                        Else
                            precision = 1 / 10 ^ p
                        End If
                    End If

                    If arg.StartsWith("TARGET:") Then
                        target = Double.Parse(arg.Split(":"c)(1).Split(";")(0))
                        If arg.Contains("SOURCE:") Then
                            arg = arg.Substring(arg.IndexOf("S"))
                            source = Array.ConvertAll(Of String, Double)(arg.Split(":"c)(1).Split(";")(0).Split(","c), Function(n) Double.Parse(n))
                            Continue For
                        End If
                    End If

                    Throw New Exception($"Unknown command line argument '{arg}'")
                Next
            Catch ex As Exception
                ShowHelp(ex.Message)
                Exit Sub
            End Try

            If source.Length < 2 Then Throw New ArgumentException("SOURCE must have at least two numbers")
        End If

        Console.WriteLine()
        Console.WriteLine(String.Format("Calculating {0:n0} Permutations", source.Count.Fact()))

        swPermutations.Start()
        Dim permutations()() As Double = source.Permutate3().Unique()
        swPermutations.Stop()

        Console.WriteLine($"Finding Solution{If(findAll, "s", "")} for {permutations.Length} unique permutations...")
        Console.WriteLine()

        swProcess.Start()
        For permutation As Integer = 0 To permutations.Length - 1
            numbers = permutations(permutation)

            Dim opening As Integer = 0
            Dim closing As Integer = 0
            Dim srcLength As Integer = source.Count \ 2
            Dim expression As String = ""

            For i As Integer = 0 To source.Count - 1
                If i > srcLength Then
                    expression += String.Format("+{0})", NumberToString(numbers(i)).Replace("-", "−"))
                    closing += 1
                Else
                    expression += String.Format("({0}+", NumberToString(numbers(i)).Replace("-", "−"))
                    opening += 1
                End If
            Next
            expression = expression.Replace("++", "+")
            expression = expression.PadRight(expression.Length + opening - closing, ")")
            expression = expression.Replace("+)", ")")

            Dim originalExpression As String = expression

            If showProgress Then
                If permutation > 0 Then Console.WriteLine()
                Console.WriteLine("Analyzing Permutation {0:N0} ({1:N2}%): {2}",
                                  permutation + 1,
                                  (permutation + 1) / permutations.Length * 100,
                                  numbers.ToStringList())
            End If

            Do
                iter += 1

                Try
                    result = Evaluator(expression.Replace("−", "-"))
                Catch
                    ' Probably caused by a division by 0 or an overflow
                    result = Double.PositiveInfinity
                End Try

                If Not (ignoreErrors AndAlso (result = Double.PositiveInfinity OrElse result = Double.NegativeInfinity)) Then
                    If Math.Abs(result - target) <= precision Then
                        Dim solution As String
                        If result <> target Then
                            solution = expression + " ~= " + target.ToString()
                        Else
                            solution = expression + " == " + target.ToString()
                        End If

                        Console.WriteLine(margin + "┌" + StrDup(solution.Length + 2, "─") + "┐")
                        Console.WriteLine(margin + "│" + StrDup(solution.Length + 2, " ") + "│")
                        Console.WriteLine(margin + "│ " + solution + " │")
                        Console.WriteLine(margin + "│" + StrDup(solution.Length + 2, " ") + "│")
                        Console.WriteLine(margin + "└" + StrDup(solution.Length + 2, "─") + "┘")

                        If infoOffset = -1 Then infoOffset = 2 * margin.Length + solution.Length

                        Console.CursorTop -= 4
                        Console.CursorLeft = infoOffset : Console.WriteLine(margin + "Permutation:     {0:N0} of {1:N0}",
                                                            permutation + 1,
                                                            permutations.Length)
                        Console.CursorLeft = infoOffset : Console.WriteLine(margin + "Iteration:       {0:N0}", iter)
                        Console.CursorLeft = infoOffset : Console.WriteLine(margin + "Processing Time: {0}", swProcess.Elapsed)

                        Console.WriteLine()
                        Console.WriteLine()

                        solutionsFound += 1

                        If pause Then Console.ReadKey()

                        If Not findAll Then Exit For
                    ElseIf showProgress Then
                        Console.WriteLine(margin + expression + " == " + result.ToString())
                    End If
                End If

                Dim opt As String = ""
                For i As Integer = 2 To expression.Length - 2
                    Select Case expression.Substring(i, 1)
                        Case "+" : opt = "-"
                        Case "-" : opt = "*"
                        Case "*" : opt = "/"
                        Case "/" : opt = "+"
                        Case Else : Continue For
                    End Select

                    If opt <> "" Then
                        If i > 0 Then expression = expression.Substring(0, i) + opt + expression.Substring(i + 1)
                        If opt <> "+" Then Exit For
                        opt = ""
                        i += 1
                    End If
                Next
            Loop While expression <> originalExpression
        Next
        swProcess.Stop()

        Console.WriteLine(StrDup(Console.WindowWidth - 1, "─"))

        Console.WriteLine()
        If solutionsFound = 0 Then
            Console.WriteLine("No solutions found...")
        ElseIf findAll Then
            Console.WriteLine("Solutions Found: {0}", solutionsFound)
        End If
        Console.WriteLine()

        Console.WriteLine("Time Elapsed:")
        Console.WriteLine(margin + "Calculating Permutations: {0}", swPermutations.Elapsed)
        Console.WriteLine(margin + "Finding Solutions:        {0}", swProcess.Elapsed)

#If DEBUG Then
        Console.ReadKey()
#End If
    End Sub

    Private Sub ShowInfo()
        Console.WriteLine(" by Xavier Flix (Jun 12, 2008)")
        Console.WriteLine("http://software.xfx.net")
        Console.WriteLine()
    End Sub

    Private Sub ShowChangeLog()
        Console.WriteLine()
        Console.WriteLine(My.Resources.StrChangeLog)
    End Sub

    Private Sub ShowHelp(Optional message As String = "")
        ShowInfo()

        If message <> "" Then
            Console.WriteLine(" === Error ===")
            Console.WriteLine("The INPUT string does not appear to be in the correct format:")
            Console.WriteLine(message)
            Console.WriteLine()
        End If

        Console.WriteLine(My.Resources.StrDocumentation)

        If message = "" Then ShowChangeLog()
    End Sub

#If DEBUG Then
    Private Sub PrintPermutation(k()() As Double)
        For i As Integer = 0 To k.Length - 1
            For j As Integer = 0 To k(i).Length - 1
                Debug.WriteLine($"[{i}]: {k(i)(j)}")
            Next
            Debug.WriteLine("")
        Next
    End Sub
#End If
End Module
