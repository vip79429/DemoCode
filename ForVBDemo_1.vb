Imports System.Net.Sockets
Imports System.Threading
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions

Public Class Server
    Dim serverTcp As TcpListener
    Dim serverThread As Thread

    Sub Main()
        '啟動Server
        serverThread = New Thread(AddressOf serverProc)
        serverThread.Start()
    End Sub

    Private Sub serverProc()
        '監聽port
        Dim localAddr As IPAddress = IPAddress.Parse("127.0.0.1")
        serverTcp = New TcpListener(localAddr, 8580)
        serverTcp.Start()

        Console.WriteLine("開始監聽 8580 ...")

        '開始接收連線
        While (True)
            Dim curSocket As TcpClient = serverTcp.AcceptTcpClient()
            Dim thread As New Thread(AddressOf clientProc)
            thread.Start(curSocket)
        End While
    End Sub

    Private Sub clientProc(ByVal sck As TcpClient)
        Dim netStream As NetworkStream = sck.GetStream()
        Dim netWriter As New IO.StreamWriter(netStream)
        Dim bytes(1023) As Byte
        Dim data As String = Nothing
        Dim line As Integer

        Console.WriteLine("接收到新的連線...")

        '開始接收訊息
        While (True)
            line = netStream.Read(bytes, 0, bytes.Length)
            data = Encoding.ASCII.GetString(bytes, 0, line)
            Console.WriteLine("如果是WebSocket連線，則會先收到交握訊息:" & vbCrLf & data)

            'GET / HTTP/1.1
            'Upgrade:    WebSocket  表示希望升級到Websocket協定
            'Connection: Upgrade  表示用戶端希望連線升級
            'Host:       127.0.0.1:8580
            'Origin:     http://example.com  Origin欄位是可選的
            'Sec-WebSocket-Key: sN9cRrP/n9NdMgdcy2VJFQ==  隨機的字串，可以儘量避免普通HTTP請求被誤認為Websocket協定
            'Sec-WebSocket-Version: 13   表示支援的Websocket版本。RFC6455要求使用的版本是13

            If line = 0 Then
                Exit While
            End If

            If (New Regex("^GET").IsMatch(data)) Then
                Console.WriteLine("是WebSocket連線方式")
                Dim serverResponse As Byte() = System.Text.Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" &
                                                                     vbCrLf & "Connection: Upgrade" &
                                                                     vbCrLf & "Upgrade: websocket" &
                                                                     vbCrLf & "Sec-WebSocket-Accept: " &
                                                                     Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(  '把Key + websocket key進行Sha1加密
                                                                     Encoding.UTF8.GetBytes(New Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups(1).Value.Trim() &
                                                                     "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))) &
                                                                     vbCrLf & vbCrLf)
                netStream.Write(serverResponse, 0, serverResponse.Length)
                Console.WriteLine("serverResponse: " & vbCrLf & Encoding.UTF8.GetString(serverResponse))
                'HTTP/1.1 101 Switching Protocols
                'Connection: Upgrade
                'Upgrade: WebSocket
                'Sec-WebSocket-Accept: zZmaZBUUyh7ZXkB9ERkJlkrNSW4=

                SendMessage(netStream, New String("A", 10896)) '發送成功測試

                Dim threadReceive As New Thread(AddressOf clientReceive)
                threadReceive.Start(sck)

                Exit Sub
            Else
                Console.WriteLine("不是WebSocket連線方式")
                sck.Close() '斷開連接
                Exit Sub
            End If
        End While

        sck.Close()
    End Sub

    Private Sub clientReceive(ByVal sck As TcpClient)
        Dim IA As System.IAsyncResult
        Dim list_dataBuffer As New List(Of Byte) 'data緩衝池
        Dim packageCount As Integer = 0  '計算分割封包的數量
        Dim content As String = Nothing   '實際資料
        Dim t1 As DateTime = Nothing

        'Dim read = sck.Client.EndReceive(IA)
        'If (read = 0) Then
        '    IA = sck.Client.BeginReceive(_dataBuffer, 0, _dataBuffer.Length, SocketFlags.None, read, Nothing)
        'End If

        While (True)

            If (sck.Client.Available) Then  '如有有資料，全部先收進來
                Dim netStream As NetworkStream = sck.GetStream()

                Dim Buffer(65535) As Byte
                Dim _dataBuffer() As Byte
                Dim _dataBufferLen As Int32

                '_dataBufferLen = netStream.Read(Buffer, 0, Buffer.Length)
                IA = sck.Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, Nothing, Nothing)
                _dataBufferLen = sck.Client.EndReceive(IA)

                ReDim _dataBuffer(_dataBufferLen - 1)
                Array.Copy(Buffer, _dataBuffer, _dataBufferLen)
                list_dataBuffer.AddRange(_dataBuffer)  '取得實際收到的資料長度後加入資料到緩衝池

                'Console.WriteLine("clientReceive: " & vbCrLf & BitConverter.ToString(_dataBuffer))
                '|1bit     3bit         4bit         |1bit       7bit               |                 |
                '|FIN      RSV1,2,3     op-code      |Mask       Payload Length     |Masks Keys       |Keys Payload Data
                '|1byte                              |1byte                         |4byte            |nbyte
                If t1 = Nothing Then
                    t1 = DateTime.Now
                End If

            ElseIf Not list_dataBuffer.Count = 0 Then  '當資料全部收完時，一個個拆封包

                Dim removeLength As Integer = 6 '刪除緩衝池裡的封包用。 最基本的封包裡，至少有6Byte保留 FIN, RSV op-code, Mask, Payload length, Masks Key

                If Not (list_dataBuffer(1) And 128) = 128 Then '在第二個Byte檢查Mask
                    Console.WriteLine("沒有Mask，連線關閉")
                    sck.Close() '沒有Mask應要關閉連線
                    Return
                End If

                Dim payloadLen As Integer = list_dataBuffer(1) And 127  '預先取得資料長度，之後可知道是<126 or >=126

                Dim masks(3) As Byte
                Dim payloadData As Byte() = filterPayloadData(list_dataBuffer.ToArray, payloadLen, masks)  '過濾並取得實際的資料和masks
                If payloadLen = 126 Then '如果超過126大小，第3和第4個byte是資料長度，須重新計算
                    payloadLen = list_dataBuffer(2) * 256 + list_dataBuffer(3)
                    removeLength += 2  'payloadLen為2個Byte
                End If

                For i As Integer = 0 To payloadLen - 1  '透過Masks的key進行解碼演算法
                    payloadData(i) = payloadData(i) Xor masks(i Mod 4)
                Next
                removeLength += payloadLen  '封包長度

                Dim _content As String = Encoding.UTF8.GetString(payloadData, 0, payloadData.Length)  '實際的資料內容

                packageCount += 1
                content += _content

                If (list_dataBuffer(0) And 128) = 128 Then '在第一個Byte檢查FIN，如果是最後一封包時為1，且合併Content
                    Dim t2 As DateTime = DateTime.Now
                    Console.WriteLine("---------------------clientReceive---------------------")
                    Console.WriteLine("完整封包總共花費:" & t2.Subtract(t1).TotalSeconds & "秒")
                    Console.WriteLine("package Count:" & packageCount)
                    Console.WriteLine("content Length:" & content.Length)
                    If (content.Length < 127) Then
                        Console.WriteLine("content:" & vbCrLf & content)
                    End If
                    Console.WriteLine("---------------------clientReceive---------------------")
                    SendMessage(sck.GetStream(), content) '發送成功測試
                    '初始化資料
                    content = ""
                    packageCount = 0
                    t1 = DateTime.Now

                End If
                list_dataBuffer.RemoveRange(0, removeLength)  '解析完一個分割封包的時候，從緩衝池裡移除(1024)
            Else
                'Console.WriteLine("目前沒有封包可以收，緩衝池裡也沒有資料")
            End If

        End While
    End Sub
    ''' <summary>
    ''' 過濾並取得實際的資料
    ''' </summary>
    ''' <param name="_dataBuffer"></param>
    ''' <param name="length">取得實際長度</param>
    ''' <param name="masks">取得masks</param>
    ''' <returns></returns>
    Function filterPayloadData(_dataBuffer As Byte(), length As Integer, masks As Byte())
        Dim payloadData As Byte()

        Select Case length

            Case 126 '包含16 bit Extend Payload Length
                Array.Copy(_dataBuffer, 4, masks, 0, 4)
                length = _dataBuffer(2) * 256 + _dataBuffer(3) '第3和第4個byte是資料長度
                ReDim payloadData(length - 1)
                Array.Copy(_dataBuffer, 8, payloadData, 0, length)

                '因為收封包最大設定1024個，所以不會有一次超過65535的封包，如果封包大小超過65535，才使用此方法
                'Case 127 '包含64 bit Extend Payload Length
                '    Console.WriteLine("資料長度=127")
                '    Array.Copy(_dataBuffer, 10, masks, 0, 4)
                '    Dim uInt64Bytes(8) As Byte
                '    For i As Integer = 0 To 8
                '        uInt64Bytes(i) = _dataBuffer(9 - i)
                '    Next

                '    Dim len As UShort = BitConverter.ToInt64(uInt64Bytes, 0)
                '    ReDim payloadData(len - 1)
                '    For i As Integer = 0 To len
                '        payloadData(i) = _dataBuffer(i + 14)
                '    Next

            Case Else '沒有 Extend Payload Length
                Array.Copy(_dataBuffer, 2, masks, 0, 4)
                ReDim payloadData(length - 1)
                Array.Copy(_dataBuffer, 6, payloadData, 0, length)

        End Select
        Return payloadData

    End Function
    ''' <summary>
    ''' 格式化傳送字串封包
    ''' </summary>
    ''' <param name="sck"></param>
    ''' <param name="message"></param>
    ''' <remarks></remarks>
    Sub SendMessage(sck As NetworkStream, message As String)
        Dim rawData = System.Text.Encoding.UTF8.GetBytes(message)
        Dim frame As Byte()

        '|1bit     3bit         4bit         |1bit       7bit               |126 <= dataLength <= 65535  /dataLength > 65535        |
        '|FIN      RSV1,2,3     op-code      |Mask       Payload Length     |Payload Length                                         |Keys Payload Data
        '|1byte                              |1byte                         |2byte                       /8byte                     |nbyte


        If (rawData.Length < 126) Then
            Console.WriteLine("---------------------SendMessage---------------------")
            Console.WriteLine("傳送的資料長度<126")
            Console.WriteLine("---------------------SendMessage---------------------")
            ReDim frame(2 + rawData.Length - 1)
            frame(0) = CByte(129) '10000001 FIN=1
            frame(1) = CByte(rawData.Length)

            '填入實際資料在表頭的最後面
            For i As Integer = 0 To rawData.Length - 1
                frame(i + 2) = rawData(i)
            Next

        ElseIf (rawData.Length >= 126) AndAlso rawData.Length <= 65535 Then
            Console.WriteLine("---------------------SendMessage---------------------")
            Console.WriteLine("傳送的資料長度>=126 <=65535")
            Console.WriteLine("---------------------SendMessage---------------------")
            ReDim frame(4 + rawData.Length - 1)
            frame(0) = CByte(129) '10000001 FIN=1
            frame(1) = CByte(126)
            Dim payloadLength As Byte() = BitConverter.GetBytes(rawData.Length)

            'BitConverter.ToString的資料由後往前讀
            frame(2) = payloadLength(1)
            frame(3) = payloadLength(0)

            '填入實際資料在表頭的最後面
            For i As Integer = 0 To rawData.Length - 1
                frame(i + 4) = rawData(i)
            Next
        Else
            Console.WriteLine("---------------------SendMessage---------------------")
            Console.WriteLine("傳送的資料長度>65535")
            Console.WriteLine("---------------------SendMessage---------------------")
            ReDim frame(10 + rawData.Length - 1)
            frame(0) = CByte(129) '10000001 FIN=1
            frame(1) = CByte(127) 'Payload Length=127 表示超過65535的資料
            Dim payloadLength As Byte() = BitConverter.GetBytes(rawData.Length)

            'BitConverter.ToString的資料由後往前讀
            '因為Xstar最大的封包傳送為3Byte，固處理4個Byte就夠用，其他4個Byte為0
            frame(6) = payloadLength(3)
            frame(7) = payloadLength(2)
            frame(8) = payloadLength(1)
            frame(9) = payloadLength(0)

            '填入實際資料在表頭的最後面
            For i As Integer = 0 To rawData.Length - 1
                frame(i + 10) = rawData(i)
            Next
        End If

        sck.Write(frame, 0, frame.Length)

    End Sub

End Class