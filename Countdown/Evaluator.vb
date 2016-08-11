'Imports System.CodeDom.Compiler
'Imports System.ComponentModel

'<Browsable(False), EditorBrowsable(EditorBrowsableState.Never)> _
'Public Class Evaluator
'    Implements IDisposable

'    Private codeProvider As VBCodeProvider
'    Private compParams As CompilerParameters

'    Public Sub New()
'        codeProvider = New VBCodeProvider()

'        compParams = New CompilerParameters()
'        'compParams.ReferencedAssemblies.Add("System.dll")
'        'compParams.ReferencedAssemblies.Add("mscorlib.dll")
'        compParams.CompilerOptions = "-t:library"
'        compParams.IncludeDebugInformation = True
'        compParams.GenerateInMemory = True
'        compParams.GenerateExecutable = False
'    End Sub

'    Private Function Compile(ByVal sCode As String) As CompilerResults
'        Dim sb As String = ""

'        sb += "Imports System" + vbCrLf
'        sb += "Imports System.Math" + vbCrLf
'        sb += "Imports Microsoft.VisualBasic" + vbCrLf
'        sb += "Namespace CCode" + vbCrLf
'        sb += "Class Code" + vbCrLf
'        sb += "Public Function EvaluateExpression() As Double" + vbCrLf
'        sb += "Return " + sCode + vbCrLf
'        sb += "End Function " + vbCrLf
'        sb += "End Class" + vbCrLf
'        sb += "End Namespace" + vbCrLf

'        Return codeProvider.CompileAssemblyFromSource(compParams, sb)
'    End Function

'    Private Function CreateObject(ByVal code As String) As Object
'        Dim compRes As CompilerResults = Compile(code)

'        If compRes.Errors.HasErrors Then
'            Throw New ApplicationException()
'        Else
'            Dim CompAsm As System.Reflection.Assembly = compRes.CompiledAssembly
'            Return CompAsm.CreateInstance("CCode.Code")
'        End If
'    End Function

'    Public Function Evaluate(ByVal expression As String) As Double
'        Try
'            Dim obj As Object = CreateObject(expression)
'            Dim mInfo As System.Reflection.MethodInfo = obj.GetType().GetMethod("EvaluateExpression")
'            Return CDbl(mInfo.Invoke(obj, Nothing))
'        Catch ex As Exception
'            Console.WriteLine(ex.Message)
'            Console.WriteLine(ex.StackTrace)
'            Throw New ApplicationException(ex.Message)
'        End Try
'    End Function

'#Region " IDisposable Support "
'    Private disposedValue As Boolean = False        ' To detect redundant calls

'    ' IDisposable
'    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
'        If Not Me.disposedValue Then
'            If disposing Then
'                ' TODO: free unmanaged resources when explicitly called
'            End If

'            ' TODO: free shared unmanaged resources
'            codeProvider.Dispose()
'            codeProvider = Nothing
'            compParams = Nothing
'        End If
'        Me.disposedValue = True
'    End Sub

'    ' This code added by Visual Basic to correctly implement the disposable pattern.
'    Public Sub Dispose() Implements IDisposable.Dispose
'        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
'        Dispose(True)
'        GC.SuppressFinalize(Me)
'    End Sub
'#End Region
'End Class
