namespace Sylvester.Tensors

open System
open MathNet.Numerics.LinearAlgebra

open Sylvester.Arithmetic
open Sylvester.Arithmetic.N10
open Sylvester.Arithmetic.Collections

type Matrix<'t, 'd10,'d9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1, 'e10,'e9, 'e8, 'e7, 'e6, 'e5, 'e4, 'e3, 'e2, 'e1 
    when 'd10 :> Base10Digit and 'd9 :> Base10Digit and 'd8 :> Base10Digit and 'd7 :> Base10Digit and 'd6 :> Base10Digit
    and 'd5 :> Base10Digit and 'd4 :> Base10Digit and 'd3 :> Base10Digit and 'd2 :> Base10Digit 
    and 'd1 :> Base10Digit and 'e10 :> Base10Digit and 'e9 :> Base10Digit and 'e8 :> Base10Digit and 'e7 :> Base10Digit and 'e6 :> Base10Digit
    and 'e5 :> Base10Digit and 'e4 :> Base10Digit and 'e3 :> Base10Digit and 'e2 :> Base10Digit 
    and 'e1 :> Base10Digit and 't : struct and 't: (new: unit -> 't) and 't:> ValueType and 't :> IEquatable<'t> and 't :> IFormattable>(dim0: N10<'d10, 'd9, 'd8, 'd7, 'd6, 'd5, 'd4, 'd3, 'd2, 'd1>, dim1:N10<'e10, 'e9, 'e8, 'e7, 'e6, 'e5, 'e4, 'e3, 'e2, 'e1>, items:'t[,]) = 
    
    member val Array = va2darray dim0 dim1 items

    member val _Array = items

    member x.Dims = x.Array ^+^ VNil |> varrays

    member x.Dim0 = x.Array.Length0

    member x.Dim1 = x.Array.Length1

    member inline x._Matrix = DenseMatrix.ofArray2(x._Array)

    member inline x.SetVal(i:'i, j:'j, item: 't) = x.Array.SetVal(i, j, item)
    
    member inline x.Item(i:'i, j:'j) = x.Array.[i, j]

    member inline x.GetSlice(start0: 'a option, finish0 : 'b option, start1: 'c option, finish1 : 'd option) : 
        Matrix<'t, 'f10, 'f9, 'f8, 'f7, 'f6, 'f5, 'f4, 'f3, 'f2, 'f1, 'g10, 'g9, 'g8, 'g7, 'g6, 'g5, 'g4, 'g3, 'g2, 'g1> =
        let inline create(z0:'z0, z1:'z1, items: 't[,] when 'z0 :> N10<'f10, 'f9, 'f8, 'f7, 'f6, 'f5, 'f4, 'f3, 'f2, 'f1> 
                                                    and 'z1 :> N10<'g10, 'g9, 'g8, 'g7, 'g6, 'g5, 'g4, 'g3, 'g2, 'g1>) = 
            Matrix<'t, 'f10, 'f9, 'f8, 'f7, 'f6, 'f5, 'f4, 'f3, 'f2, 'f1, 'g10, 'g9, 'g8, 'g7, 'g6, 'g5, 'g4, 'g3, 'g2, 'g1>(z0, z1, items)
            
        checkidx(start0.Value, x.Dim0)
        checkidx(finish0.Value, x.Dim0)
        checkidx(start1.Value, x.Dim1)
        checkidx(finish1.Value, x.Dim1)
        checklt(start0.Value, finish0.Value)
        checklt(start1.Value, finish1.Value)
        let _start0, _finish0 = start0.Value, finish0.Value
        let _start1, _finish1 = start1.Value, finish1.Value
        let intstart0, intfinish0 = _start0 |> int, _finish0 |> int
        let intstart1, intfinish1 = _start1 |> int, _finish1 |> int
        let length0 = (_finish0 - _start0) + one
        let length1 = (_finish1 - _start1) + one

        create(length0, length1, x._Array.[intstart0..intfinish0, intstart1..intfinish1])


    static member inline (+) (l:Matrix<'t, 'ld10,'ld9, 'ld8, 'ld7, 'ld6, 'ld5, 'ld4, 'ld3, 'ld2, 'ld1, 'le10,'le9, 'le8, 'le7, 'le6, 'le5, 'le4, 'le3, 'le2, 'le1>, 
                              r:Matrix<'t, 'ld10,'ld9, 'ld8, 'ld7, 'ld6, 'ld5, 'ld4, 'ld3, 'ld2, 'ld1, 'le10,'le9, 'le8, 'le7, 'le6, 'le5, 'le4, 'le3, 'le2, 'le1>) :
                              Matrix<'t, 'ld10,'ld9, 'ld8, 'ld7, 'ld6, 'ld5, 'ld4, 'ld3, 'ld2, 'ld1, 'le10,'le9, 'le8, 'le7, 'le6, 'le5, 'le4, 'le3, 'le2, 'le1> =
        
        let inline create(z0:'z0, z1:'z1, items: 't[,] when 'z0 :> N10<'f10, 'f9, 'f8, 'f7, 'f6, 'f5, 'f4, 'f3, 'f2, 'f1> and 'z1 :> N10<'g10, 'g9, 'g8, 'g7, 'g6, 'g5, 'g4, 'g3, 'g2, 'g1>) = 
            Matrix<'t, 'f10, 'f9, 'f8, 'f7, 'f6, 'f5, 'f4, 'f3, 'f2, 'f1, 'g10, 'g9, 'g8, 'g7, 'g6, 'g5, 'g4, 'g3, 'g2, 'g1>(z0, z1, items)
                      
        let res = l._Matrix + r._Matrix
        create(l.Array.Length0, l.Array.Length1, res.AsArray())
    
    static member inline (*) (l:Matrix<'t, 'ld10,'ld9, 'ld8, 'ld7, 'ld6, 'ld5, 'ld4, 'ld3, 'ld2, 'ld1, 'le10,'le9, 'le8, 'le7, 'le6, 'le5, 'le4, 'le3, 'le2, 'le1>, 
                              r:Matrix<'t, 'rd10,'rd9, 'rd8, 'rd7, 'rd6, 'rd5, 'rd4, 'rd3, 'rd2, 'rd1, 're10,'re9, 're8, 're7, 're6, 're5, 're4, 're3, 're2, 're1>) :
                              Matrix<'t, 'ld10,'ld9, 'ld8, 'ld7, 'ld6, 'ld5, 'ld4, 'ld3, 'ld2, 'ld1, 're10,'re9, 're8, 're7, 're6, 're5, 're4, 're3, 're2, 're1> =
        
        let inline create(z0:'z0, z1:'z1, items: 't[,] when 'z0 :> N10<'f10, 'f9, 'f8, 'f7, 'f6, 'f5, 'f4, 'f3, 'f2, 'f1> and 'z1 :> N10<'g10, 'g9, 'g8, 'g7, 'g6, 'g5, 'g4, 'g3, 'g2, 'g1>) = 
            Matrix<'t, 'f10, 'f9, 'f8, 'f7, 'f6, 'f5, 'f4, 'f3, 'f2, 'f1, 'g10, 'g9, 'g8, 'g7, 'g6, 'g5, 'g4, 'g3, 'g2, 'g1>(z0, z1, items)
                      
        let res = l._Matrix * r._Matrix
        create(l.Array.Length0, r.Array.Length1, res.AsArray())
        

    
        

        
        
        