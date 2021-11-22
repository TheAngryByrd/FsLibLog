namespace FsOtel
open System
open System.Diagnostics
open System.Runtime.CompilerServices

[<Extension>]
type ActivityExtensions =
    [<Extension>]
    static member inline AddBaggageSafe (span : Activity, key : string,value : string) =
        if span <> null then
            span.AddBaggage(key,value)
        else
            span

    [<Extension>]
    static member inline AddEventSafe (span : Activity, e : ActivityEvent) =
        if span <> null then
            span.AddEvent(e)
        else
            span

    [<Extension>]
    static member inline AddEventSafe (span : Activity, e : string) =
        span.AddEventSafe(ActivityEvent e)

    [<Extension>]
    static member inline SetTagSafe (span : Activity, key, value : obj) =
        if span <> null then
            span.SetTag(key,value)
        else
            span

    [<Extension>]
    static member SetStatusErrorSafe (span : Activity, description : string)  =
        span
            .SetTagSafe("otel.status_code","ERROR")
            .SetTagSafe("otel.status_description", description)

[<Extension>]
type ActivitySourceExtensions =
    [<Extension>]
    static member inline StartActivityForName(
            tracer : ActivitySource,
            name: string,
            [<CallerMemberName>] ?memberName: string,
            [<CallerFilePath>] ?path: string,
            [<CallerLineNumberAttribute>] ?line: int) =
        let mi = Reflection.MethodBase.GetCurrentMethod()
        let ``namespace`` = mi.DeclaringType.FullName.Split("+") |> Seq.tryHead |> Option.defaultValue ""
        let span =
            name
            |> tracer.StartActivity
        span
            .SetTagSafe("code.filepath", path.Value)
            .SetTagSafe("code.lineno", line.Value)
            .SetTagSafe("code.namespace", ``namespace``)
            .SetTagSafe("code.function", memberName.Value)

    [<Extension>]
    static member inline StartActivityForFunc(
            tracer : ActivitySource,
            [<CallerMemberName>] ?memberName: string,
            [<CallerFilePath>] ?path: string,
            [<CallerLineNumberAttribute>] ?line: int) =
        let mi = Reflection.MethodBase.GetCurrentMethod()
        let ``namespace`` = mi.DeclaringType.FullName.Split("+") |> Seq.tryHead |> Option.defaultValue ""
        let span =
            sprintf "%s.%s" ``namespace`` memberName.Value
            |> tracer.StartActivity
        span
            .SetTagSafe("code.filepath", path.Value)
            .SetTagSafe("code.lineno", line.Value)
            .SetTagSafe("code.namespace", ``namespace``)
            .SetTagSafe("code.function", memberName.Value)
