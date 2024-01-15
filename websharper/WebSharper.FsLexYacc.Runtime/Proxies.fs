namespace FsLexYaccProxies

open WebSharper
open WebSharper.JavaScript

[<Proxy("Microsoft.FSharp.Control.CommonExtensions, FSharp.Core")>]
module internal ControlProxies =
    type System.IO.Stream with
        [<CompiledName("AsyncRead")>]
        member stream.AsyncRead(buffer: byte[], ?offset: int, ?count: int) : Async<int> =
            Unchecked.defaultof<_> // TODO
        
module internal Proxies =
    [<Proxy(typeof<System.Globalization.UnicodeCategory>)>]
    type UnicodeCategoryProxy =
    | ClosePunctuation = 21
    | ConnectorPunctuation = 18
    | Control = 14
    | CurrencySymbol = 26
    | DashPunctuation = 19
    | DecimalDigitNumber = 8
    | EnclosingMark = 7
    | FinalQuotePunctuation = 23
    | Format = 15
    | InitialQuotePunctuation = 22
    | LetterNumber = 9
    | LineSeparator = 12
    | LowercaseLetter = 1
    | MathSymbol = 25
    | ModifierLetter = 3
    | ModifierSymbol = 27
    | NonSpacingMark = 5
    | OpenPunctuation = 20
    | OtherLetter = 4
    | OtherNotAssigned = 29
    | OtherNumber = 10
    | OtherPunctuation = 24
    | OtherSymbol = 28
    | ParagraphSeparator = 13
    | PrivateUse = 17
    | SpaceSeparator = 11
    | SpacingCombiningMark = 6
    | Surrogate = 16
    | TitlecaseLetter = 2
    | UppercaseLetter = 0
    [<Proxy(typeof<System.Globalization.CharUnicodeInfo>)>]
    type CharUnicodeInfoProxy() = 
        static member GetUnicodeCategory(ch:char) : System.Globalization.UnicodeCategory = 0 |> As // TODO
    [<Proxy(typeof<System.IO.Stream>);AbstractClass>]
    type StreamProxy () =
        inherit ReadableStream()
        abstract Read: buffer:byte array * offset:int * count:int -> int
        // abstract AsyncRead: buffer:byte array * ?offset:int * ?count:int -> Async<int>
        
        // default this.AsyncRead(buffer: byte array, offset: int option, count: int option): Async<int> =
        //     Unchecked.defaultof<_> // TODO

    
    [<Proxy(typeof<System.IO.TextReader>);AbstractClass>]
    type TextReaderProxy() =
        inherit ReadableStream()
        abstract Read : buffer:char[] * index:int * count:int -> int
        abstract ReadAsync : buffer:char[] * index:int * count:int -> System.Threading.Tasks.Task<int>
        

    [<Proxy(typeof<System.IO.StreamReader>)>]
    type StreamReaderProxy() =
        inherit TextReaderProxy()
        // TODO: check
        new(stream:System.IO.Stream, detectEncodingFromByteOrderMarks: bool) = 
            StreamReaderProxy()

        new(stream:System.IO.Stream,encoding:System.Text.Encoding) =
            StreamReaderProxy()

        member this.Read():int =
            let reader = this.GetReader() |> As<ReadableStreamDefaultReader>
            let v = reader.Read().Value
            if v = null then -1 else As<int> v

        override this.Read(buffer:char[], index:int, count:int) : int =
            let mutable i = index
            let mutable ch = this.Read()
            let mutable ch_count = if ch > -1 then 1 else 0
            while i < index+count && ch > -1 do
                buffer[i] <- char ch
                ch_count <- ch_count + 1
            ch_count
        
        override this.ReadAsync(buffer:char[], index:int, count:int) : System.Threading.Tasks.Task<int> =
            task {
                return this.Read(buffer,index,count)
            } // TODO

    [<Proxy(typeof<System.IO.BinaryReader>)>]
    type BinaryReaderProxy() =
        inherit StreamProxy()

        override this.Read(buffer: byte array, offset: int, count: int): int =
            failwith "Not Implemented"
        // TODO
    [<Proxy(typeof<System.Text.Encoding>)>]
    type EncodingProxy() =
        static member GetEncoding(codepage:int): System.Text.Encoding = System.Text.Encoding.Default
        [<Inline("new TextEncoder().encoding")>]
        static member Default : System.Text.Encoding = Unchecked.defaultof<_>

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