namespace FsLexYaccProxies

open WebSharper
open WebSharper.JavaScript

[<InternalProxy("Microsoft.FSharp.Control.CommonExtensions, FSharp.Core")>]
module ControlProxies =
    type System.IO.Stream with
        [<CompiledName("AsyncRead")>]
        member stream.AsyncRead(buffer: byte[], ?offset: int, ?count: int) : Async<int> =
            Unchecked.defaultof<_> // TODO
        
module internal Proxies =

    [<Proxy(typeof<System.Globalization.CharUnicodeInfo>)>]
    type CharUnicodeInfoProxy() = 
        member this.GetUnicodeCategory(ch:char) : System.Globalization.UnicodeCategory = Unchecked.defaultof<_> // TODO
    [<Proxy(typeof<System.IO.Stream>);AbstractClass>]
    type StreamProxy () =
        inherit ReadableStream({|``type`` = "bytes"|})
        abstract Read: buffer:byte array * offset:int * count:int -> int
        // abstract AsyncRead: buffer:byte array * ?offset:int * ?count:int -> Async<int>
        
        // default this.AsyncRead(buffer: byte array, offset: int option, count: int option): Async<int> =
        //     Unchecked.defaultof<_> // TODO
        default this.Read(buffer: byte array, offset: int, count: int): int =
            Unchecked.defaultof<_> // TODO

    [<Proxy(typeof<System.IO.StreamReader>)>]
    type StreamReaderProxy() =
        inherit StreamProxy()
        // TODO: check
        member private this.reader = ReadableStreamDefaultReader(this |> As<ReadableStream>)
        new(stream:System.IO.Stream, detectEncodingFromByteOrderMarks: bool) = 
            StreamReaderProxy()

        new(stream:System.IO.Stream,encoding:System.Text.Encoding) =
            StreamReaderProxy()

        override this.Read(buffer: byte array, offset: int, count: int): int =
            let buf = 
                Uint8Array(ArrayBuffer(buffer.Length),offset,count)
            let options = ReadableStreamGetReaderOptions(ReadableStreamReaderMode.Byob)
            let reader = this.GetReader(options) |> As<ReadableStreamBYOBReader>
            
            let maybeThisArr = reader.Read(buf).Value |> As<Uint8Array>
            maybeThisArr.ByteLength

        // override this.AsyncRead(buffer: byte array, offset: int option, count: int option): Async<int> =
        //     async {
        //         let offset = Option.defaultValue 0 offset
        //         let count = Option.defaultValue buffer.Length count
        //         return this.Read(buffer,offset,count)
        //     }

    [<Proxy(typeof<System.IO.BinaryReader>)>]
    type BinaryReaderProxy() =
        inherit StreamProxy()
        // TODO
    
    [<Proxy(typeof<System.IO.TextReader>)>]
    type TextReaderProxy() =
        inherit StreamProxy()

        override this.Read(buffer:byte[], index:int, count:int) : int =
            Unchecked.defaultof<_> // TODO
        member this.Read(buffer:char[], index:int, count:int) : int =
            this.Read(buffer,index,count)
        
        member this.ReadAsync(buffer:char[], index:int, count:int) : System.Threading.Tasks.Task<int> =
            task {
                return this.Read(buffer,index,count)
            } // TODO

    [<Proxy(typeof<System.Text.Encoding>)>]
    type EncodingProxy() =
        member this.GetEncoding(codepage:int): System.Text.Encoding = System.Text.Encoding.Default

    [<Proxy(typeof<System.IO.FileStream>)>]
    type FileStreamProxy() =
        inherit StreamProxy()

        // TODO proper proxy
        new(path  : string, mode  : System.IO.FileMode, access: System.IO.FileAccess, share : System.IO.FileShare) =
            FileStreamProxy()

        override this.Read(buffer: byte array, offset: int, count: int): int =
            let buf = 
                Uint8Array(ArrayBuffer(buffer.Length),offset,count)
            let options = ReadableStreamGetReaderOptions(ReadableStreamReaderMode.Byob)
            let reader = this.GetReader(options) |> As<ReadableStreamBYOBReader>
            
            let maybeThisArr = reader.Read(buf).Value |> As<Uint8Array>
            maybeThisArr.ByteLength
        // override this.AsyncRead(buffer: byte array, ?offset: int, ?count: int): Async<int> =
        //     async {
        //         let offset = Option.defaultValue 0 offset
        //         let count = Option.defaultValue buffer.Length count
        //         return this.Read(buffer,offset,count)
        //     }