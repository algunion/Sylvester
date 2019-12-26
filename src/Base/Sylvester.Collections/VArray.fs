namespace Sylvester.Collections
 
open System
open System.Collections.Generic

open Sylvester.Arithmetic
open Sylvester.Arithmetic.N10

type VArray<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1 when 'd10 :> Base10Digit and 'd9 :> Base10Digit 
and 'd8 :> Base10Digit and 'd7 :> Base10Digit and 'd6 :> Base10Digit
and 'd5 :> Base10Digit and 'd4 :> Base10Digit and 'd3 :> Base10Digit and 'd2 :> Base10Digit 
and 'd1 :> Base10Digit>() = 
    
    member x.Length = N10<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1>()
    
    member x.IntLength = x.Length.IntVal

[<StructuredFormatDisplay("{_Array}")>]
type VArray<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1, 't when 'd10 :> Base10Digit and 'd9 :> Base10Digit 
                and 'd8 :> Base10Digit and 'd7 :> Base10Digit and 'd6 :> Base10Digit
                and 'd5 :> Base10Digit and 'd4 :> Base10Digit and 'd3 :> Base10Digit and 'd2 :> Base10Digit 
                and 'd1 :> Base10Digit>(items:'t[]) = 
    inherit VArray<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1>()
    
    member x._Array = if items.Length = x.IntLength then items else raise (ArgumentOutOfRangeException("items", sprintf "The initializing array length %i does not match %i." items.Length x.IntLength))
         
    member inline x.SetVal(i:'i, item:'t) =
        checkidx(i, x.Length)
        x._Array.[i |> int] <- item

    member inline x.For(start:'start, finish:'finish, f: int -> 't -> unit) =
        checkidx(start, x.Length)
        checkidx(finish, x.Length)
        checklt(start, finish)
        for i in ((int) start)..((int)finish) do f i x._Array.[i]
      
    member inline x.ForAll(f: int -> 't -> unit) =
        for i in 0..x._Array.Length - 1 do f i x._Array.[i]

    member inline x.SetVals(items: IEnumerable<'t> ) = 
        do if Seq.length items <> x.IntLength then raise(ArgumentOutOfRangeException("items"))
        x.ForAll(fun i a -> x._Array.SetValue(a, i))

    member inline x.Item(i:'i) : 't = 
        checkidx(i, x.Length)
        x._Array.[i |> int]
           
    member inline x.GetSlice(start: 'a option, finish : 'b option) = 
        let inline create(c:'c, items: 't[] when 'c :> N10<'f10, 'f9, 'f8, 'f7, 'f6, 'f5, 'f4, 'f3, 'f2, 'f1>) = 
            VArray<'f10, 'f9, 'f8, 'f7, 'f6, 'f5, 'f4, 'f3, 'f2, 'f1, 't>(items)

        checkidx(start.Value, x.Length)
        checkidx(finish.Value, x.Length)
        checklt(start.Value, finish.Value)
        let _start, _finish = start.Value, finish.Value            
        let intstart, intfinish = _start |> int, _finish |> int
        let length = (_finish - _start) + one  

        create(length, x._Array.[intstart..intfinish])
        
    new(n:N10<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1>, x:'t) = 
        VArray<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1, 't>(Array.create (getN<N10<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1>>.IntVal) x)

    new(n:N10<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1>) = 
        VArray<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1, 't>(Array.create n.IntVal Unchecked.defaultof<'t>)

    static member inline VArray = _true

    static member inline (!+) (v:VArray<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1, 't>) = v.Length 
   
 type VArray<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1 when 'd10 :> Base10Digit and 'd9 :> Base10Digit 
 and 'd8 :> Base10Digit and 'd7 :> Base10Digit and 'd6 :> Base10Digit
 and 'd5 :> Base10Digit and 'd4 :> Base10Digit and 'd3 :> Base10Digit and 'd2 :> Base10Digit 
 and 'd1 :> Base10Digit> with
    static member create(arr: 't[]) = new VArray<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1, 't>(arr)
     
 type VArray<'d1, 't  when 'd1 :> Base10Digit> = VArray<``0``, ``0``, ``0``, ``0``, ``0``, ``0``, ``0``, ``0``, ``0``, 'd1, 't>

 type VArray<'d2, 'd1, 't when 'd1 :> Base10Digit and 'd2 :> Base10Digit> = VArray<``0``, ``0``, ``0``, ``0``, ``0``, ``0``, ``0``, ``0``, 'd2, 'd1, 't>

 type Varray<'d3, 'd2, 'd1, 't when 'd1 :> Base10Digit and 'd2 :> Base10Digit and 'd3 :> Base10Digit> = VArray<``0``, ``0``, ``0``, ``0``, ``0``, ``0``, ``0``, 'd3, 'd2, 'd1, 't>