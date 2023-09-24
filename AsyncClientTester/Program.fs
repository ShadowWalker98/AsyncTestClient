// For more information see https://aka.ms/fsharp-console-apps
open System.Net
open System.Net.Sockets
open System.Threading.Tasks

let receiveMessageTask (socket : Socket) =
    let bytesResponse = [|for i in 0..256 -> byte(i)|]
    let messageReceivedTask = socket.ReceiveAsync(bytesResponse)
    
    messageReceivedTask, bytesResponse

[<EntryPoint>]

let main args =
    let mutable keepOpen = true
    let socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    let endpoint = IPEndPoint(IPAddress.Loopback, 1223)
    let remoteEP = IPEndPoint(IPAddress.Loopback, 3314)
    socket.Bind(endpoint)
    let connectedTask = socket.ConnectAsync(remoteEP)
    connectedTask.Wait()
    
    let mutable keepOpen = true
    while keepOpen do
        let (messageReceiveTask, responseArray) = receiveMessageTask socket
        messageReceiveTask.Wait()
        
        if messageReceiveTask.IsCompletedSuccessfully then
            printfn $"message from server is: %s{System.Text.Encoding.ASCII.GetString(responseArray[0..messageReceiveTask.Result - 1])}"
            printfn $"%d{messageReceiveTask.Result}"
            keepOpen <- false
    
    socket.Shutdown(SocketShutdown.Both)
    socket.DisconnectAsync |> ignore

    
    0